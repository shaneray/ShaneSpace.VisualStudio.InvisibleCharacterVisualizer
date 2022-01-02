namespace ShaneSpace.VisualStudio.InvisibleCharacterVisualizer.Core
{
    /// <summary>
    /// Information about a homoglyph.
    /// </summary>
    public class HomoglyphInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HomoglyphInfo"/> class.
        /// </summary>
        /// <param name="characters">The characters.</param>
        /// <param name="homoglyphCharacters">The homoglyphCharacters.</param>
        /// <param name="description">The description.</param>
        public HomoglyphInfo(string characters, string homoglyphCharacters, string description)
        {
            Characters = characters;
            Description = description;
            HomoglyphCharacters = homoglyphCharacters;
        }

        /// <summary>
        /// Gets the Characters
        /// </summary>
        public string Characters { get; }

        /// <summary>
        /// Gets the Homoglyph Characters
        /// </summary>
        public string HomoglyphCharacters { get; }

        /// <summary>
        /// Gets the description
        /// </summary>
        public string Description { get; }
    }
}