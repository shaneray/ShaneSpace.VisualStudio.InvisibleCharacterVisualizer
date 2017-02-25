using Microsoft.VisualStudio.Text.Tagging;

namespace ShaneSpace.VisualStudio.InvisibleCharacterVisualizer
{
    internal class InvisibleCharacterTag : ITag
    {
        internal readonly string Text;

        internal InvisibleCharacterTag(string text)
        {
            Text = text;
        }
    }
}
