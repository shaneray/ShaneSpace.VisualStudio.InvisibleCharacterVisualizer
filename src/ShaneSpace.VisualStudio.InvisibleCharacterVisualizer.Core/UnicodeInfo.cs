namespace ShaneSpace.VisualStudio.InvisibleCharacterVisualizer.Core
{
    /// <summary>
    /// Information about a unicode point.
    /// </summary>
    public class UnicodeInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnicodeInfo"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="category">The category.</param>
        public UnicodeInfo(string name, string category)
        {
            Name = name;
            Category = category;
        }

        /// <summary>
        /// Gets the name of the unicode character.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the category of the unicode character.
        /// </summary>
        public string Category { get; }
    }
}