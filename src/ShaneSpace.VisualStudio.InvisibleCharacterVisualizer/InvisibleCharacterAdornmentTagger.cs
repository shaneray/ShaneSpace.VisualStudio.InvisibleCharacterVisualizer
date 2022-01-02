using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace ShaneSpace.VisualStudio.InvisibleCharacterVisualizer
{
    /// <summary>
    /// The invisible character adornment tagger.
    /// </summary>
    internal sealed class InvisibleCharacterAdornmentTagger
        : IntraTextAdornmentTagTransformer<InvisibleCharacterTag, InvisibleCharacterAdornment>
    {
        private InvisibleCharacterAdornmentTagger(IWpfTextView view, ITagAggregator<InvisibleCharacterTag> colorTagger)
            : base(view, colorTagger)
        {
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            View.Properties.RemoveProperty(typeof(InvisibleCharacterTagger));
        }

        /// <summary>
        /// Gets the invisible character adornment tagger.
        /// </summary>
        /// <param name="view">The WPF text view.</param>
        /// <param name="invisibleCharacterTagger">The invisible character tagger.</param>
        /// <returns>The invisible character adornment tagger.</returns>
        internal static ITagger<IntraTextAdornmentTag> GetTagger(IWpfTextView view, Lazy<ITagAggregator<InvisibleCharacterTag>> invisibleCharacterTagger)
        {
            return view.Properties.GetOrCreateSingletonProperty(() => new InvisibleCharacterAdornmentTagger(view, invisibleCharacterTagger.Value));
        }

        /// <inheritdoc/>
        protected override InvisibleCharacterAdornment CreateAdornment(InvisibleCharacterTag data, SnapshotSpan span)
        {
            return new InvisibleCharacterAdornment(data);
        }

        /// <inheritdoc/>
        protected override bool UpdateAdornment(InvisibleCharacterAdornment adornment, InvisibleCharacterTag data)
        {
            return true;
        }
    }
}