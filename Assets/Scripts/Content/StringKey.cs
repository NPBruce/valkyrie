namespace Assets.Scripts.Content
{
    /// <summary>
    /// Represents a string corresponding to a Localization string key
    /// </summary>
    public class StringKey
    {
        /// <summary>
        /// Empty string Key
        /// </summary>
        public static StringKey EmptyStringKey = new StringKey("");

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
        /// Check if the StringKey object is a localization key
        /// </summary>
        /// <returns>true if string must be translated</returns>
        private bool isKey()
        {
            return key.StartsWith("{");
        }

        /// <summary>
        /// Search in current dictionary the text translated to the current language
        /// </summary>
        /// <returns>the text in the current language</returns>
        public string Translate()
        {
            if (this.isKey())
            {
                return LocalizationRead.FFGLookup(this);
            } else
            {
                return key;
            }
                 
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
