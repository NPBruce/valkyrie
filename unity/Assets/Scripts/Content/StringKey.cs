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
        public static StringKey NULL = new StringKey("",false);

        /// <summary>
        /// Complete key.
        /// IE: {qst:MONSTER_NAME}
        /// </summary>
        public string fullKey { get; set; }

        private bool preventLookup = false;

        /// <summary>
        /// Basic constructor from a key
        /// </summary>
        /// <param name="newKey">key to translate</param>
        public StringKey(string newKey, bool doLookup = true)
        {
            fullKey = newKey;
            preventLookup = !doLookup;
        }

        /// <summary>
        /// Constructor from a dict, key and one parameter
        /// </summary>
        /// <param name="dict">dict to lookup</param>
        /// <param name="newKey">key to translate</param>
        /// <param name="numberZeroParam">first param for {0} replace</param>
        public StringKey(string dict, string newKey, StringKey numberZeroKeyParam)
        {
            fullKey = "{" + dict + ":" + newKey + ":{0}:" + numberZeroKeyParam.fullKey + "}";
        }

        /// <summary>
        /// Constructor from a dict, key and one parameter
        /// </summary>
        /// <param name="dict">dict to lookup</param>
        /// <param name="newKey">key to translate</param>
        /// <param name="numberZeroParam">first param for {0} replace</param>
        public StringKey(string dict, string newKey, string numberZeroParam)
        {
            fullKey = "{" + dict + ":" + newKey + ":{0}:" + numberZeroParam + "}";
        }

        /// <summary>
        /// Constructor from a dict, key and one parameter
        /// </summary>
        /// <param name="dict">dict to lookup</param>
        /// <param name="newKey">key to translate</param>
        /// <param name="numberZeroParam">first param for {0} replace</param>
        public StringKey(string dict, string newKey, int numberZeroNumParam)
        {
            fullKey = "{" + dict + ":" + newKey + ":{0}:" + numberZeroNumParam.ToString() + "}";
        }

        /// <summary>
        /// Constructor with dict and key
        /// </summary>
        /// <param name="dict">dict to lookup</param>
        /// <param name="newKey">key to lookup</param>
        public StringKey(string dict, string newKey)
        {
            fullKey = "{" + dict + ":" + newKey + "}";
        }

        /// <summary>
        /// Check if the StringKey object is a localization key
        /// </summary>
        /// <returns>true if string must be translated</returns>
        public bool isKey()
        {
            return fullKey.StartsWith("{") && !fullKey.StartsWith("{rnd:") && !fullKey.StartsWith("{var");
        }

        /// <summary>
        /// Search in current dictionary the text translated to the current language
        /// </summary>
        /// <returns>the text in the current language</returns>
        public string Translate()
        {
            if (this.isKey() && !preventLookup)
            {
                return LocalizationRead.DictLookup(this);
            } else
            {
                //non heys can have newline characters
                return fullKey.Replace("\\n", System.Environment.NewLine);
            }
                 
        }

        /// <summary>
        /// toString returns the key
        /// </summary>
        /// <returns>key</returns>
        public override string ToString()
        {
            return fullKey.Replace(System.Environment.NewLine, "\\n");
        }
    }
}
