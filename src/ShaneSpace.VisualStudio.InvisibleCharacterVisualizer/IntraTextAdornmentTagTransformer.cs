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
using System.Windows;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace ShaneSpace.VisualStudio.InvisibleCharacterVisualizer
{
    /// <summary>
    /// Helper class for producing intra-text adornments from data tags.
    /// </summary>
    /// <typeparam name="TDataTag">Type of data tag</typeparam>
    /// <typeparam name="TAdornment">Type of adornment</typeparam>
    /// <remarks>
    /// For cases where intra-text adornments do not correspond exactly to tags,
    /// use the <see cref="IntraTextAdornmentTagger"/> base class.
    /// </remarks>
    internal abstract class IntraTextAdornmentTagTransformer<TDataTag, TAdornment>
        : IntraTextAdornmentTagger<TDataTag, TAdornment>, IDisposable
        where TDataTag : ITag
        where TAdornment : UIElement
    {
        protected readonly ITagAggregator<TDataTag> DataTagger;
        protected readonly PositionAffinity? AdornmentAffinity;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntraTextAdornmentTagTransformer{TDataTag, TAdornment}"/> class.
        /// </summary>
        /// <param name="dataTagger">The data tagger</param>
        /// <param name="adornmentAffinity">Determines whether adornments based on data tags with zero-length spans
        /// will stick with preceding or succeeding text characters.</param>
        /// <param name="view">The view</param>
        protected IntraTextAdornmentTagTransformer(IWpfTextView view, ITagAggregator<TDataTag> dataTagger, PositionAffinity adornmentAffinity = PositionAffinity.Successor)
            : base(view)
        {
            AdornmentAffinity = adornmentAffinity;
            DataTagger = dataTagger;

            DataTagger.TagsChanged += HandleDataTagsChanged;
        }

        public virtual void Dispose()
        {
            DataTagger.Dispose();
        }

        protected override IEnumerable<Tuple<SnapshotSpan, PositionAffinity?, TDataTag>> GetAdornmentData(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                yield break;
            }

            ITextSnapshot snapshot = spans[0].Snapshot;

            foreach (IMappingTagSpan<TDataTag> dataTagSpan in DataTagger.GetTags(spans))
            {
                NormalizedSnapshotSpanCollection dataTagSpans = dataTagSpan.Span.GetSpans(snapshot);

                // Ignore data tags that are split by projection.
                // This is theoretically possible but unlikely in current scenarios.
                if (dataTagSpans.Count != 1)
                {
                    continue;
                }

                SnapshotSpan span = dataTagSpans[0];

                PositionAffinity? affinity = span.Length > 0 ? null : AdornmentAffinity;

                yield return Tuple.Create(span, affinity, dataTagSpan.Tag);
            }
        }

        private void HandleDataTagsChanged(object sender, TagsChangedEventArgs args)
        {
            NormalizedSnapshotSpanCollection changedSpans = args.Span.GetSpans(View.TextBuffer.CurrentSnapshot);
            InvalidateSpans(changedSpans);
        }
    }
}
