using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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
        public static StringKey NULL = new StringKey(null,"",false);

        /// <summary>
        /// Complete key.
        /// IE: {qst:MONSTER_NAME}
        /// </summary>
        public string fullKey
        {
            get
            {
                if (dict == null)
                {
                    return key;
                }
                else
                {
                    StringBuilder result = new StringBuilder()
                        .Append('{')
                        .Append(dict)
                        .Append(':')
                        .Append(key);

                    if (parameters != null) { 
                        result.Append(':').Append(parameters);
                    }
                    return result.Append('}').ToString();
                }
            }
        }

        private string dict;

        public readonly string key;

        private string parameters = null;

        private bool preventLookup = false;

        private const string regexKey = "^{(ffg|val|qst):";

        public StringKey(string unknownKey)
        {
            if (Regex.Match(unknownKey, regexKey).Success)
            {
                string[] parts = unknownKey.Substring(1,unknownKey.Length -2).Split(":".ToCharArray(), 3, System.StringSplitOptions.RemoveEmptyEntries);

                dict = parts[0];
                key = parts[1];
                if (parts.Length == 3)
                {
                    parameters = parts[2];
                }
                preventLookup = false;
            } else
            {
                dict = null;
                key = unknownKey;
                preventLookup = true;
            }
        }


        /// <summary>
        /// Basic constructor from a key
        /// </summary>
        /// <param name="newKey">key to translate</param>
        public StringKey(string newDict, string newKey, bool doLookup = true)
        {
            dict = newDict;
            key = newKey;
            preventLookup = !doLookup;
        }

        /// <summary>
        /// Constructor from a dict, key and one parameter
        /// </summary>
        /// <param name="newDict">dict to lookup</param>
        /// <param name="newKey">key to translate</param>
        /// <param name="numberZeroParam">first param for {0} replace</param>
        public StringKey(string newDict, string newKey, StringKey numberZeroKeyParam)
            : this(newDict,newKey,numberZeroKeyParam.fullKey) { }

        /// <summary>
        /// Constructor from a dict, key and one parameter
        /// </summary>
        /// <param name="newDict">dict to lookup</param>
        /// <param name="newKey">key to translate</param>
        /// <param name="numberZeroParam">first param for {0} replace</param>
        public StringKey(string newDict, string newKey, string numberZeroParam)
        {
            dict = newDict;
            key = newKey;
            parameters = "{0}:"+ numberZeroParam;
        }

        /// <summary>
        /// Constructor from a dict, key and one parameter
        /// </summary>
        /// <param name="newKey">key to translate</param>
        /// <param name="param1">first param</param>
        /// <param name="param2">second param</param>
        public StringKey(StringKey templateStringKey, string param1, string param2)
        {
            dict = templateStringKey.dict;
            key = templateStringKey.key;
            parameters = param1 + ":" + param2;
        }

        /// <summary>
        /// Constructor from a dict, key and one parameter
        /// </summary>
        /// <param name="newDict">dict to lookup</param>
        /// <param name="newKey">key to translate</param>
        /// <param name="numberZeroParam">first param for {0} replace</param>
        public StringKey(string newDict, string newKey, int numberZeroNumParam)
            : this(newDict, newKey, numberZeroNumParam.ToString()) { }

        /// <summary>
        /// Check if the StringKey object is a localization key
        /// </summary>
        /// <returns>true if string must be translated</returns>
        public bool isKey()
        {
            return (dict != null);
        }

        /// <summary>
        /// Search in current dictionary the text translated to the current language
        /// </summary>
        /// <param name="emptyIfNotFound">If true return "" on missing key</param>
        /// <returns>the text in the current language</returns>
        public string Translate(bool emptyIfNotFound = false)
        {
            if (isKey() && !preventLookup)
            {
                if (emptyIfNotFound && !KeyExists()) return "";
                return LocalizationRead.DictLookup(this);
            } else
            {
                //non heys can have newline characters
                return fullKey.Replace("\\n", System.Environment.NewLine);
            }
        }

        public bool KeyExists()
        {
            if (isKey() && !preventLookup)
            {
                return LocalizationRead.CheckLookup(this);
            }
            return false;
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
