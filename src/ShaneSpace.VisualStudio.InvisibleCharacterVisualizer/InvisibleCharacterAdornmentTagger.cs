using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace ShaneSpace.VisualStudio.InvisibleCharacterVisualizer
{
    internal sealed class InvisibleCharacterAdornmentTagger
        : IntraTextAdornmentTagTransformer<InvisibleCharacterTag, InvisibleCharacterAdornment>
    {
        private InvisibleCharacterAdornmentTagger(IWpfTextView view, ITagAggregator<InvisibleCharacterTag> colorTagger)
            : base(view, colorTagger)
        {
        }

        public override void Dispose()
        {
            View.Properties.RemoveProperty(typeof(InvisibleCharacterTagger));
        }

        internal static ITagger<IntraTextAdornmentTag> GetTagger(IWpfTextView view, Lazy<ITagAggregator<InvisibleCharacterTag>> colorTagger)
        {
            return view.Properties.GetOrCreateSingletonProperty(() => new InvisibleCharacterAdornmentTagger(view, colorTagger.Value));
        }

        protected override InvisibleCharacterAdornment CreateAdornment(InvisibleCharacterTag data, SnapshotSpan span)
        {
            return new InvisibleCharacterAdornment(data);
        }

        protected override bool UpdateAdornment(InvisibleCharacterAdornment adornment, InvisibleCharacterTag data)
        {
            return true;
        }
    }
}