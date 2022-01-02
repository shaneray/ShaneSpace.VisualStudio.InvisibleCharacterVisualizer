using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace ShaneSpace.VisualStudio.InvisibleCharacterVisualizer
{
    /// <summary>
    /// The regex tagger base class.
    /// </summary>
    /// <typeparam name="T">ITag Param.</typeparam>
    internal abstract class RegexTagger<T> : ITagger<T>
        where T : ITag
    {
        private readonly IEnumerable<Regex> _matchExpressions;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexTagger{T}"/> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="matchExpressions">The match expressions.</param>
        protected RegexTagger(ITextBuffer buffer, IEnumerable<Regex> matchExpressions)
        {
            var expressions = matchExpressions as IList<Regex> ?? matchExpressions.ToList();
            if (expressions.Any(re => (re.Options & RegexOptions.Multiline) == RegexOptions.Multiline))
            {
                throw new ArgumentException("Multiline regular expressions are not supported.");
            }

            _matchExpressions = expressions;

            buffer.Changed += (sender, args) => HandleBufferChanged(args);
        }

        /// <inheritdoc/>
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        /// <inheritdoc/>
        public virtual IEnumerable<ITagSpan<T>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            // Here we grab whole lines so that matches that only partially fall inside the spans argument are detected.
            // Note that the spans argument can contain spans that are sub-spans of lines or intersect multiple lines.
            foreach (var line in GetIntersectingLines(spans))
            {
                var text = line.GetText();

                foreach (var regex in _matchExpressions)
                {
                    foreach (var match in regex.Matches(text).Cast<Match>())
                    {
                        var tag = TryCreateTagForMatch(match);
                        if (EqualityComparer<T>.Default.Equals(tag, default(T)))
                        {
                            continue;
                        }

                        var span = new SnapshotSpan(line.Start + match.Index, match.Length);
                        yield return new TagSpan<T>(span, tag);
                    }
                }
            }
        }

        /// <summary>
        /// Overridden in the derived implementation to provide a tag for each regular expression match.
        /// If the return value is <c>null</c>, this match will be skipped.
        /// </summary>
        /// <param name="match">The match to create a tag for.</param>
        /// <returns>The tag to return from <see cref="GetTags"/>, if non-<c>null</c>.</returns>
        protected abstract T TryCreateTagForMatch(Match match);

        /// <summary>
        /// Handle buffer changes. The default implementation expands changes to full lines and sends out
        /// a <see cref="TagsChanged"/> event for these lines.
        /// </summary>
        /// <param name="args">The buffer change arguments.</param>
        protected virtual void HandleBufferChanged(TextContentChangedEventArgs args)
        {
            if (args.Changes.Count == 0)
            {
                return;
            }

            var temp = TagsChanged;
            if (temp == null)
            {
                return;
            }

            // Combine all changes into a single span so that
            // the ITagger<>.TagsChanged event can be raised just once for a compound edit
            // with many parts.
            var snapshot = args.After;

            var start = args.Changes[0].NewPosition;
            var end = args.Changes[args.Changes.Count - 1].NewEnd;

            var totalAffectedSpan = new SnapshotSpan(
                snapshot.GetLineFromPosition(start).Start,
                snapshot.GetLineFromPosition(end).End);

            temp(this, new SnapshotSpanEventArgs(totalAffectedSpan));
        }

        private IEnumerable<ITextSnapshotLine> GetIntersectingLines(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                yield break;
            }

            var lastVisitedLineNumber = -1;
            var snapshot = spans[0].Snapshot;
            foreach (var span in spans)
            {
                var firstLine = snapshot.GetLineNumberFromPosition(span.Start);
                var lastLine = snapshot.GetLineNumberFromPosition(span.End);

                for (var i = Math.Max(lastVisitedLineNumber, firstLine); i <= lastLine; i++)
                {
                    yield return snapshot.GetLineFromLineNumber(i);
                }

                lastVisitedLineNumber = lastLine;
            }
        }
    }
}
