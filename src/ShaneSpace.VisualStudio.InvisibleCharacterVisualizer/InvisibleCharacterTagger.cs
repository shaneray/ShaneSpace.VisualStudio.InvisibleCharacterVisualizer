using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;

namespace ShaneSpace.VisualStudio.InvisibleCharacterVisualizer
{
    internal sealed class InvisibleCharacterTagger : RegexTagger<InvisibleCharacterTag>
    {
        internal InvisibleCharacterTagger(ITextBuffer buffer)
            : base(buffer, new[] { new Regex($"[{string.Join(string.Empty, NonPrintableUnicodeCharacters.UnicodeRanges)}]", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase) })
        {
        }

        protected override InvisibleCharacterTag TryCreateTagForMatch(Match match)
        {
            return new InvisibleCharacterTag(match.Value);
        }
    }
}