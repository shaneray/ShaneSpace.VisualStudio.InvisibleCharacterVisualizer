using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using ShaneSpace.VisualStudio.InvisibleCharacterVisualizer.Core;

namespace ShaneSpace.VisualStudio.InvisibleCharacterVisualizer
{
    /// <summary>
    /// The invisible character adornment.
    /// </summary>
    internal sealed class InvisibleCharacterAdornment : Label
    {
        private enum CharacterType
        {
            Invisible,
            Homoglyph
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvisibleCharacterAdornment"/> class.
        /// </summary>
        /// <param name="invisibleCharacterTag">the invisible character tag.</param>
        internal InvisibleCharacterAdornment(InvisibleCharacterTag invisibleCharacterTag)
        {
            var characterType = CharacterType.Invisible;
            if (UnicodeHelper.HomoglyphRegex.IsMatch(invisibleCharacterTag.Text))
            {
                characterType = CharacterType.Homoglyph;
            }

            Foreground = Brushes.White;
            Padding = new Thickness(0);

            VerticalContentAlignment = VerticalAlignment.Center;
            HorizontalContentAlignment = HorizontalAlignment.Center;
            FontSize -= 2;

            if (characterType == CharacterType.Homoglyph)
            {
                var description = "Unknown Homoglyph";
                var homoglyphDictionary = UnicodeHelper.HomoglyphDictionary.Value;
                if (homoglyphDictionary.ContainsKey(invisibleCharacterTag.Text))
                {
                    description = homoglyphDictionary[invisibleCharacterTag.Text].Description;
                }
                Content = invisibleCharacterTag.Text;
                ToolTip = new ToolTip { Content = description };
                Background = Brushes.Orange;
            }
            else
            {
                var unicodeHex = invisibleCharacterTag.Text.ToHex();
                var info = new UnicodeInfo("Unknown", "Unknown");
                if (UnicodeHelper.InvisibleCharacterDictionary.ContainsKey(unicodeHex))
                {
                    info = UnicodeHelper.InvisibleCharacterDictionary[unicodeHex];
                }

                Content = unicodeHex;
                MouseLeftButtonUp += (sender, args) => System.Diagnostics.Process.Start($"https://www.unicodemap.org/details/{unicodeHex}/index.html");
                ToolTip = new ToolTip { Content = $"{info.Name} / {info.Category}" };
                Background = Brushes.Red;
            }
        }
    }
}