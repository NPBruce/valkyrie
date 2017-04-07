using System;
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
        private Dictionary<string, EntryI18n> dict;
        // raw dict
        private string[] rawDict;

        // default language. If current language doesn't have description, default will be used.
        public int defaultLanguage { get; set; }

        // current language. Current language to be used.
        public int currentLanguage {get; set;}

        /*
        /// <summary>
        /// Dictionary constructor from a languagesList
        /// </summary>
        /// <param name="languagesList"></param>
        public DictionaryI18n(string languagesList, int newDefaultLanguage)
        {
            languages = languagesList.Split(COMMA);
            defaultLanguage = newDefaultLanguage;
        }
        */

        /// <summary>
        /// Dictionary constructor from a localizacion file and default language
        /// </summary>
        /// <param name="languagesList"></param>
        public DictionaryI18n(string[] languagesAndTexts, string newDefaultLanguage,string newCurrentLanguage)
        {
            // Set languages list with first line of file
            languages = languagesAndTexts[0].Split(COMMA);
            // Get default language
            setDefaultLanguage(newDefaultLanguage);
            // set current language
            setCurrentLanguage(newCurrentLanguage);

            // Create dictionary with file lines capacity
            dict = new Dictionary<string, EntryI18n>(languagesAndTexts.Length);
            //Load raw dictionary
            rawDict = languagesAndTexts;
        }

        /// <summary>
        /// Create a dict entry with the StringI18n
        /// </summary>
        /// <param name="currentKeyValues">line of localization file</param>
        public void Add(EntryI18n currentKeyValues)
        {
            dict.Add(currentKeyValues.key, currentKeyValues);
        }

        /// <summary>
        /// Method for getting all languages of this dictionary 
        /// </summary>
        /// <returns></returns>
        public string[] getLanguages()
        {
            return languages;
        }

        public void setDefaultLanguage(string languageName)
        {
            int newLanguage = getPosFromName(languageName);
            if (newLanguage > 0)
            {
                defaultLanguage = newLanguage;
            }
        }

        /// <summary>
        /// Sets current language using the string of the language.
        /// If there is no language with this name, the default language
        /// remains the same.
        /// </summary>
        /// <param name="languageName">Name of the language</param>
        public void setCurrentLanguage(string languageName)
        {
            int newLanguage = getPosFromName(languageName);
            if (newLanguage > 0)
            {
                currentLanguage = newLanguage;
            }
        }

        /// <summary>
        /// Get language number from string
        /// </summary>
        /// <param name="languageName"></param>
        /// <returns></returns>
        private int getPosFromName(string languageName)
        {
            for (int pos = 1; pos < languages.Length; pos++)
            {
                if (languages[pos] == languageName)
                {
                    return pos;
                }
            }
            return -1;
        }

        /// <summary>
        /// Checks if a key exists in the dictionary
        /// gets its value
        /// </summary>
        /// <param name="v">key to find</param>
        /// <param name="valueOut">variable to store result if exists</param>
        /// <returns>true if the key exists in the dictionary</returns>
        public bool tryGetValue(string v, out EntryI18n valueOut)
        {
            bool found = dict.TryGetValue(v, out valueOut);

            if (found)
            {
                return true;
            }
            else
            {
                // Search element
                for (int pos = 1; pos < rawDict.Length; pos++)
                {
                    // if the line is not empty and not slash (/) insert
                    if (rawDict[pos].StartsWith(v + COMMA))
                    {
                        valueOut = new EntryI18n(this, GetEntry(pos));
                        Add(valueOut);
                        return true;
                    }
                }
                return false;
            }
        }

        public string GetEntry(int pos)
        {
            string r = rawDict[pos];
            int index = pos + 1;
            while(!EntryFinished(r) && index < rawDict.Length)
            {
                r += Environment.NewLine + rawDict[index++];
            }
            return r;
        }

        public bool EntryFinished(string entry)
        {
            bool quote = false;
            for (int i = 0; i < entry.Length; i++)
            {
                char next = '_';
                if (i < (entry.Length - 1))
                {
                    next = entry[i + 1];
                }
                if (!quote && entry[i] == '\\' && next == '\\')
                {
                    return true;
                }
                if (entry[i] == '\"')
                {
                    if (next != '\"')
                    {
                        quote = !quote;
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            return !quote;
        }
    }
}
