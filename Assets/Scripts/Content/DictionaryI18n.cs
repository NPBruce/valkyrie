using System.Collections.Generic;

namespace Assets.Scripts.Content
{
    /// <summary>
    /// Class to define a dictionary of KEYS, Strings in all available languages
    /// </summary>
    public class DictionaryI18n
    {
        private const char QUOTES = '\"';
        private const char COMMA = ',';

        /// <summary>
        /// Fixed value included in FFGs Localization.txt file
        /// </summary>
        public const string FFG_LANGS = ".,English,Spanish,French,German,Italian,Portuguese,Polish,Japanese,Chinese,Czech";
        /// <summary>
        /// Default initial language is English
        /// </summary>
        public const string DEFAULT_LANG = "English";

        // Languages
        private string[] languages;
        
        // Dictionary: Will be used to store all strings of a localization file
        private Dictionary<string, StringI18n> dict;

        // default language. If current language doesn't have description, default will be used.
        public int defaultLanguage { get; set; }

        // current language. Current language to be used.
        public int currentLanguage {get; set;}

        /// <summary>
        /// Dictionary constructor from a languagesList
        /// </summary>
        /// <param name="languagesList"></param>
        public DictionaryI18n(string languagesList, int newDefaultLanguage)
        {
            languages = languagesList.Split(COMMA);
            defaultLanguage = newDefaultLanguage;
        }

        /// <summary>
        /// Method for getting all languages of this dictionary 
        /// </summary>
        /// <returns></returns>
        public string[] getLanguages()
        {
            return languages;
        }

        /// <summary>
        /// Sets current language using the string of the language.
        /// If there is no language with this name, the default language
        /// remains the same.
        /// </summary>
        /// <param name="languageName">Name of the language</param>
        public void setCurrentLanguage(string languageName)
        {
            for (int pos = 1; pos < languages.Length; pos++)
            {
                if (languages[pos] == languageName)
                {
                    currentLanguage = pos;
                }
            }
        }
    }
}
