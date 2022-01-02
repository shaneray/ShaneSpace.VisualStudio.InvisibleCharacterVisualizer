using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using ShaneSpace.VisualStudio.InvisibleCharacterVisualizer.Core;

namespace ShaneSpace.VisualStudio.InvisibleCharacterVisualizer
{
    /// <summary>
    /// Tags invisible characters.
    /// </summary>
    internal sealed class InvisibleCharacterTagger : RegexTagger<InvisibleCharacterTag>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvisibleCharacterTagger"/> class.
        /// </summary>
        /// <param name="buffer">the text buffer.</param>
        internal InvisibleCharacterTagger(ITextBuffer buffer)
            : base(buffer, new[] { UnicodeHelper.InvisibleCharacterRegex, UnicodeHelper.HomoglyphRegex })
        {
        }

        /// <inheritdoc/>
        protected override InvisibleCharacterTag TryCreateTagForMatch(Match match)
        {
            return new InvisibleCharacterTag(match.Value);
        }
    }
}