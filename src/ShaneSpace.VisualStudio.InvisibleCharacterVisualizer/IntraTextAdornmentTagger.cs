// ***************************************************************************
//
//    Copyright (c) Microsoft Corporation. All rights reserved.
//    This code is licensed under the Visual Studio SDK license terms.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace ShaneSpace.VisualStudio.InvisibleCharacterVisualizer
{
    /// <summary>
    /// Helper class for interspersing adornments into text.
    /// </summary>
    /// <typeparam name="TData">The data.</typeparam>
    /// <typeparam name="TAdornment">The adornment.</typeparam>
    /// <remarks>
    /// To avoid an issue around intra-text adornment support and its interaction with text buffer changes,
    /// this tagger reacts to text and color tag changes with a delay. It waits to send out its own TagsChanged
    /// event until the WPF Dispatcher is running again and it takes care to report adornments
    /// that are consistent with the latest sent TagsChanged event by storing that particular snapshot
    /// and using it to query for the data tags.
    /// </remarks>
    internal abstract class IntraTextAdornmentTagger<TData, TAdornment>
        : ITagger<IntraTextAdornmentTag>
        where TAdornment : UIElement
    {
        /// <summary>
        /// The WPF text view.
        /// </summary>
        protected readonly IWpfTextView View;
        private readonly List<SnapshotSpan> _invalidatedSpans = new List<SnapshotSpan>();
        private Dictionary<SnapshotSpan, TAdornment> _adornmentCache = new Dictionary<SnapshotSpan, TAdornment>();

        /// <summary>
        /// Initializes a new instance of the <see cref="IntraTextAdornmentTagger{TData, TAdornment}"/> class.
        /// </summary>
        /// <param name="view">The WPF text view.</param>
        protected IntraTextAdornmentTagger(IWpfTextView view)
        {
            View = view;
            Snapshot = view.TextBuffer.CurrentSnapshot;

            View.LayoutChanged += HandleLayoutChanged;
            View.TextBuffer.Changed += HandleBufferChanged;
        }

        /// <inheritdoc/>
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        /// <summary>
        /// Gets the text snapshot.
        /// </summary>
        protected ITextSnapshot Snapshot { get; private set; }

        /// <summary>
        /// Produces tags on the snapshot that the tag consumer asked for.
        /// </summary>
        /// <param name="spans">the spans.</param>
        /// <returns>Collection of tags.</returns>
        public virtual IEnumerable<ITagSpan<IntraTextAdornmentTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans == null || spans.Count == 0)
            {
                yield break;
            }

            // Translate the request to the snapshot that this tagger is current with.
            var requestedSnapshot = spans[0].Snapshot;

            var translatedSpans = new NormalizedSnapshotSpanCollection(spans.Select(span => span.TranslateTo(Snapshot, SpanTrackingMode.EdgeExclusive)));

            // Grab the adornments.
            foreach (var tagSpan in GetAdornmentTagsOnSnapshot(translatedSpans))
            {
                // Translate each adornment to the snapshot that the tagger was asked about.
                var span = tagSpan.Span.TranslateTo(requestedSnapshot, SpanTrackingMode.EdgeExclusive);

                var tag = new IntraTextAdornmentTag(tagSpan.Tag.Adornment, tagSpan.Tag.RemovalCallback, -1, null, null, -1, tagSpan.Tag.Affinity);
                yield return new TagSpan<IntraTextAdornmentTag>(span, tag);
            }
        }

        /// <summary>
        /// Create Adornment.
        /// </summary>
        /// <param name="data">The adornment data.</param>
        /// <param name="span">The span of text that this adornment will elide.</param>
        /// <returns>Adornment corresponding to given data. May be null.</returns>
        protected abstract TAdornment CreateAdornment(TData data, SnapshotSpan span);

        /// <summary>
        /// Update Adornment.
        /// </summary>
        /// <param name="adornment">The adornment.</param>
        /// <param name="data">The data.</param>
        /// <returns>True if the adornment was updated and should be kept. False to have the adornment removed from the view.</returns>
        protected abstract bool UpdateAdornment(TAdornment adornment, TData data);

        /// <summary>
        /// Get Adornment Data.
        /// </summary>
        /// <param name="spans">Spans to provide adornment data for. These spans do not necessarily correspond to text lines.</param>
        /// <remarks>
        /// If adornments need to be updated, call <see cref="RaiseTagsChanged"/> or <see cref="InavlidateSpans"/>.
        /// This will, indirectly, cause <see cref="GetAdornmentData"/> to be called.
        /// </remarks>
        /// <returns>
        /// A sequence of:
        ///  * Adornment data for each adornment to be displayed.
        ///  * The span of text that should be elided for that adornment (zero length spans are acceptable).
        ///  * Affinity of the adornment (this should be null if and only if the elided span has a length greater than zero).
        /// </returns>
        protected abstract IEnumerable<Tuple<SnapshotSpan, PositionAffinity?, TData>> GetAdornmentData(NormalizedSnapshotSpanCollection spans);

        /// <summary>
        /// Causes intra-text adornments to be updated asynchronously.
        /// </summary>
        /// <param name="spans">The spans.</param>
        protected void InvalidateSpans(IList<SnapshotSpan> spans)
        {
            lock (_invalidatedSpans)
            {
                var wasEmpty = _invalidatedSpans.Count == 0;
                _invalidatedSpans.AddRange(spans);

                if (wasEmpty && _invalidatedSpans.Count > 0)
                {
#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
                    _ = View.VisualElement.Dispatcher.BeginInvoke(new Action(AsyncUpdate));
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs
                }
            }
        }

        /// <summary>
        /// Causes intra-text adornments to be updated synchronously.
        /// </summary>
        /// <param name="span">The span.</param>
        protected void RaiseTagsChanged(SnapshotSpan span)
        {
            var handler = TagsChanged;
            handler?.Invoke(this, new SnapshotSpanEventArgs(span));
        }

        private void HandleBufferChanged(object sender, TextContentChangedEventArgs args)
        {
            var editedSpans = args.Changes.Select(change => new SnapshotSpan(args.After, change.NewSpan)).ToList();
            InvalidateSpans(editedSpans);
        }

        private void AsyncUpdate()
        {
            // Store the snapshot that we're now current with and send an event
            // for the text that has changed.
            if (Snapshot != View.TextBuffer.CurrentSnapshot)
            {
                Snapshot = View.TextBuffer.CurrentSnapshot;

                var translatedAdornmentCache = new Dictionary<SnapshotSpan, TAdornment>();

                foreach (var keyValuePair in _adornmentCache)
                {
                    var key = keyValuePair.Key.TranslateTo(Snapshot, SpanTrackingMode.EdgeExclusive);
                    var value = keyValuePair.Value;
                    if (translatedAdornmentCache.ContainsKey(key))
                    {
                        translatedAdornmentCache[key] = value;
                    }
                    else
                    {
                        translatedAdornmentCache.Add(key, value);
                    }
                }

                _adornmentCache = translatedAdornmentCache;
            }

            List<SnapshotSpan> translatedSpans;
            lock (_invalidatedSpans)
            {
                translatedSpans = _invalidatedSpans.Select(s => s.TranslateTo(Snapshot, SpanTrackingMode.EdgeInclusive)).ToList();
                _invalidatedSpans.Clear();
            }

            if (translatedSpans.Count == 0)
            {
                return;
            }

            var start = translatedSpans.Select(span => span.Start).Min();
            var end = translatedSpans.Select(span => span.End).Max();

            RaiseTagsChanged(new SnapshotSpan(start, end));
        }

        private void HandleLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            var visibleSpan = View.TextViewLines.FormattedSpan;

            // Filter out the adornments that are no longer visible.
            var toRemove = new List<SnapshotSpan>(
                from keyValuePair
                in _adornmentCache
                where !keyValuePair.Key.TranslateTo(visibleSpan.Snapshot, SpanTrackingMode.EdgeExclusive).IntersectsWith(visibleSpan)
                select keyValuePair.Key);

            foreach (var span in toRemove)
            {
                _adornmentCache.Remove(span);
            }
        }

        // Produces tags on the snapshot that this tagger is current with.
        private IEnumerable<TagSpan<IntraTextAdornmentTag>> GetAdornmentTagsOnSnapshot(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                yield break;
            }

            var snapshot = spans[0].Snapshot;

            System.Diagnostics.Debug.Assert(snapshot == Snapshot, "Snapshot does not equal snapshot");

            // Since WPF UI objects have state (like mouse hover or animation) and are relatively expensive to create and lay out,
            // this code tries to reuse controls as much as possible.
            // The controls are stored in this.adornmentCache between the calls.

            // Mark which adornments fall inside the requested spans with Keep=false
            // so that they can be removed from the cache if they no longer correspond to data tags.
            var toRemove = new HashSet<SnapshotSpan>();
            foreach (var ar in _adornmentCache)
            {
                if (spans.IntersectsWith(new NormalizedSnapshotSpanCollection(ar.Key)))
                {
                    toRemove.Add(ar.Key);
                }
            }

            foreach (var spanDataPair in GetAdornmentData(spans).Distinct(new Comparer()))
            {
                // Look up the corresponding adornment or create one if it's new.
                TAdornment adornment;
                var snapshotSpan = spanDataPair.Item1;
                var affinity = spanDataPair.Item2;
                var adornmentData = spanDataPair.Item3;
                if (_adornmentCache.TryGetValue(snapshotSpan, out adornment))
                {
                    if (UpdateAdornment(adornment, adornmentData))
                    {
                        toRemove.Remove(snapshotSpan);
                    }
                }
                else
                {
                    adornment = CreateAdornment(adornmentData, snapshotSpan);

                    if (adornment == null)
                    {
                        continue;
                    }

                    // Get the adornment to measure itself. Its DesiredSize property is used to determine
                    // how much space to leave between text for this adornment.
                    // Note: If the size of the adornment changes, the line will be reformatted to accommodate it.
                    // Note: Some adornments may change size when added to the view's visual tree due to inherited
                    // dependency properties that affect layout. Such options can include SnapsToDevicePixels,
                    // UseLayoutRounding, TextRenderingMode, TextHintingMode, and TextFormattingMode. Making sure
                    // that these properties on the adornment match the view's values before calling Measure here
                    // can help avoid the size change and the resulting unnecessary re-format.
                    adornment.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                    _adornmentCache.Add(snapshotSpan, adornment);
                }

                yield return new TagSpan<IntraTextAdornmentTag>(snapshotSpan, new IntraTextAdornmentTag(adornment, null, affinity));
            }

            foreach (var snapshotSpan in toRemove)
            {
                _adornmentCache.Remove(snapshotSpan);
            }
        }

        private class Comparer : IEqualityComparer<Tuple<SnapshotSpan, PositionAffinity?, TData>>
        {
            public bool Equals(Tuple<SnapshotSpan, PositionAffinity?, TData> x, Tuple<SnapshotSpan, PositionAffinity?, TData> y)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                return x.Item1.Equals(y.Item1);
            }

            public int GetHashCode(Tuple<SnapshotSpan, PositionAffinity?, TData> obj)
            {
                return obj.Item1.GetHashCode();
            }
        }
    }
}
