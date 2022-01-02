using Microsoft.VisualStudio.Text.Tagging;

namespace ShaneSpace.VisualStudio.InvisibleCharacterVisualizer
{
    /// <summary>
    /// The invisible character tag.
    /// </summary>
    internal class InvisibleCharacterTag : ITag
    {
        /// <summary>
        /// The text.
        /// </summary>
        internal readonly string Text;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvisibleCharacterTag"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        internal InvisibleCharacterTag(string text)
        {
            Text = text;
        }
    }
}
