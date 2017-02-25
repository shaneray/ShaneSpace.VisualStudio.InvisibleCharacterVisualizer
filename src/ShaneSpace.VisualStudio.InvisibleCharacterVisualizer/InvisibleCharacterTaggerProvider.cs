using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace ShaneSpace.VisualStudio.InvisibleCharacterVisualizer
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("text")]
    [TagType(typeof(InvisibleCharacterTag))]
    internal sealed class InvisibleCharacterTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer)
            where T : ITag
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return buffer.Properties.GetOrCreateSingletonProperty(() => new InvisibleCharacterTagger(buffer)) as ITagger<T>;
        }
    }
}