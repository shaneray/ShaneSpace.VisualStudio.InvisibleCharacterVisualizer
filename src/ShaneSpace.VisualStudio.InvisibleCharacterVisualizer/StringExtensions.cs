using System.Text;

namespace ShaneSpace.VisualStudio.InvisibleCharacterVisualizer
{
    public static class StringExtensions
    {
        public static string ToHex(this string input)
        {
            var sb = new StringBuilder();
            foreach (var c in input)
            {
                sb.AppendFormat("0x{0:X2} ", (int)c);
            }

            return sb.ToString().Trim();
        }
    }
}