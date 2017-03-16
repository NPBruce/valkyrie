namespace Assets.Scripts.Content
{
    /// <summary>
    /// Represents a string corresponding to a Localization string key
    /// </summary>
    public class StringKey
    {
        public string key { get; set; }

        /// <summary>
        /// Basic constructor from a key
        /// </summary>
        /// <param name="newKey">key to translate</param>
        public StringKey(string newKey)
        {
            key = newKey;
        }

        /// <summary>
        /// Search in current dictionary the text translated to the current language
        /// </summary>
        /// <returns>the text in the current language</returns>
        public string Translate()
        {
            return LocalizationRead.FFGLookup(this);
        }

        /// <summary>
        /// toString returns the key
        /// </summary>
        /// <returns>key</returns>
        public override string ToString()
        {
            return key;
        }
    }
}
