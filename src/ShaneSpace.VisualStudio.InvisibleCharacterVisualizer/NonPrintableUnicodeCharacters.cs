namespace ShaneSpace.VisualStudio.InvisibleCharacterVisualizer
{
    public static class NonPrintableUnicodeCharacters
    {
        public static string[] UnicodeRanges { get; } =
        {
            "\\0-\\x08",
            "\\u000B",
            "\\u000C",
            "\\u000E-\\u001F",
            "\\x7F-\\u0084",
            "\\u0086-\\x9F",
            "\\u00A0",
            "\\xAD",
            "\\u200B-\\u200F",
            "\\u202A-\\u202E",
            "\\u2060-\\u206F",
            "\\u2072",
            "\\uFEFD-\\uFF00",
            "\\uFFEF-\\uFFFB"
        };
    }
}
