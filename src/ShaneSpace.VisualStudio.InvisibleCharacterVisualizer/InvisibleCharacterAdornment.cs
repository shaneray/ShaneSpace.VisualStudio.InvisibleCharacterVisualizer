using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShaneSpace.VisualStudio.InvisibleCharacterVisualizer
{
    internal sealed class InvisibleCharacterAdornment : Button
    {
        internal InvisibleCharacterAdornment(InvisibleCharacterTag nonAsciiTag)
        {
            var text = nonAsciiTag.Text.ToHex();

            Content = new TextBlock
            {
                Text = text,
                Background = Brushes.Red,
                Foreground = Brushes.White,
                Padding = new Thickness(2)
            };

            // TODO MouseLeftButtonUp += (sender, args) => MessageBox.Show($"http://www.unicodemap.org/details/{text}/index.html");
        }
    }
}