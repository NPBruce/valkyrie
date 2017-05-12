using System;
using System.Collections.Generic;
using System.Text;


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
        /// Fixed value included in FFGs Localization files
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

            // Create dictionary with file lines capacity
            dict = new Dictionary<string, EntryI18n>(languagesAndTexts.Length);
            //Load raw dictionary
            rawDict = languagesAndTexts;

            // Get default language
            if (newDefaultLanguage != null)
            {
                setDefaultLanguage(newDefaultLanguage);
            }
            // set current language
            if (newDefaultLanguage != null)
            {
                setCurrentLanguage(newCurrentLanguage);
            }
        }

        /// <summary>
        /// Create a dict entry with the StringI18n
        /// </summary>
        /// <param name="currentKeyValues">line of localization file</param>
        public void Add(EntryI18n currentKeyValues)
        {
            dict[currentKeyValues.key] = currentKeyValues;
        }

        /// <summary>
        /// Add dict entries from another dict merging rawdata
        /// </summary>
        /// <param name="dictToCombine"></param>
        public void Add(DictionaryI18n dictToCombine)
        {
            if (dictToCombine != null)
            {
                foreach (string key in dictToCombine.dict.Keys)
                {
                    dict[key] = dictToCombine.dict[key];
                }

                int array1OriginalLength = rawDict.Length;
                System.Array.Resize<string>(ref rawDict, array1OriginalLength + dictToCombine.rawDict.Length);
                System.Array.Copy(dictToCombine.rawDict, 0, rawDict, array1OriginalLength, dictToCombine.rawDict.Length);
            }
        }

        /// <summary>
        /// Adds raw data to the dictionary. Current dict shouldn't have entries neither new dict. Only raw data
        /// </summary>
        /// <param name="dictToCombine"></param>
        public void AddRaw(DictionaryI18n dictToCombine)
        {
            if (dict.Count == 0 && dictToCombine.dict.Count == 0)
            {
                bool found = false;
                foreach (string lang in languages)
                {
                    if (lang != "." && lang == dictToCombine.languages[1])
                    {
                        found = true;
                    }
                }

                // If the language already exists don't add anything
                // If the new dict has more than one lang don't add anything
                if (!found && dictToCombine.languages.Length == 2)
                {
                    List<string> rawOut = new List<string>();
                    // Generate the dictionary list
                    string newLanguagesList = String.Join(COMMA.ToString(), languages) + COMMA + dictToCombine.languages[1];
                    rawOut.Add(newLanguagesList);
                    languages = newLanguagesList.Split(COMMA);

                    string currentKey;
                    string outString;
                    for(int entryPos = 1; entryPos < rawDict.Length; entryPos++)
                    {
                        currentKey = rawDict[entryPos].Split(COMMA)[0] + COMMA;

                        outString = rawDict[entryPos] + COMMA;
                        for (int newEntryPos = 1; newEntryPos < dictToCombine.rawDict.Length; newEntryPos++)
                        {
                            if (dictToCombine.rawDict[newEntryPos].StartsWith(currentKey))
                            {
                                outString += dictToCombine.rawDict[newEntryPos].Substring(currentKey.Length);
                                break;
                            }
                        }

                        rawOut.Add(outString);
                    }

                    rawDict = rawOut.ToArray();
                } else
                {
                    ValkyrieTools.ValkyrieDebug.Log("The AddRaw method only merges a dictionary with only one new lang");
                }
            } else
            {
                ValkyrieTools.ValkyrieDebug.Log("The AddRaw method only merges raw dictionaries");
            }
        }



        public void Remove(string key)
        {
            if (dict.ContainsKey(key))
            {
                dict.Remove(key);
            }
            RemoveRaw(key);
        }

        private void RemoveRaw(string key)
        {
            RemoveRawPrefix(key + ",");
        }

        private void RemoveRawPrefix(string key)
        {
            List<string> newRaw = new List<string>();
            foreach (string s in rawDict)
            {
                if (s.IndexOf(key) != 0)
                {
                    newRaw.Add(s);
                }
            }
            rawDict = newRaw.ToArray();
        }

        public void RemoveKeyPrefix(string prefix)
        {
            HashSet<string> toRemove = new HashSet<string>();
            foreach (string s in dict.Keys)
            {
                if (s.IndexOf(prefix) == 0)
                {
                    toRemove.Add(s);
                }
            }
            foreach (string s in toRemove)
            {
                Remove(s);
            }
            RemoveRawPrefix(prefix);
        }

        public void RenamePrefix(string oldPrefix, string newPrefix)
        {
            HashSet<string> toRemove = new HashSet<string>();
            List<EntryI18n> toAdd = new List<EntryI18n>();
            foreach (string s in dict.Keys)
            {
                if (s.IndexOf(oldPrefix) == 0)
                {
                    string newKey = newPrefix + s.Substring(oldPrefix.Length);
                    EntryI18n e = dict[s];
                    e.key = newKey;
                    toAdd.Add(e);
                    toRemove.Add(s);
                }
            }
            foreach (string s in toRemove)
            {
                Remove(s);
            }
            foreach (EntryI18n e in toAdd)
            {
                dict.Add(e.key, e);
            }
            for (int i = 0; i < rawDict.Length; i++)
            {
                if (rawDict[i].IndexOf(oldPrefix) == 0)
                {
                    rawDict[i] = newPrefix + rawDict[i].Substring(oldPrefix.Length);
                }
            }
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
            if (newLanguage == -1 && dict.Count == 0)
            {
                // If the language isn't on the list. 
                // There are 2 options. If we are importing the dictionaries
                // we can add the new language.
                // Only if all items are raw data. 
                // Else We set the default language

                AddRaw(new DictionaryI18n(new String[1] { ".," + languageName }, languageName, languageName));
                newLanguage = getPosFromName(languageName);
            }

            if (newLanguage > 0)
            {
                currentLanguage = newLanguage;
            } else
            {
                currentLanguage = defaultLanguage;
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

        /// <summary>
        /// Create entries for every raw data.
        /// Repeated data will not replace
        /// </summary>
        public void flushRaw()
        {
            foreach(string rawLine in rawDict)
            {
                string key = rawLine.Split(COMMA)[0];
                // Process non repeated list line
                if (key != "." && !dict.ContainsKey(key))
                {
                    Add(new EntryI18n(this, rawLine));
                }
            }
        }

        public string GetEntry(int pos)
        {
            string r = rawDict[pos];
            int index = pos + 1;
            while(!EntryFinished(r) && index < rawDict.Length)
            {
                r += System.Environment.NewLine + rawDict[index++];
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

        /// <summary>
        /// Serialization of dictionary.
        /// To save Localization.file
        /// </summary>
        /// <returns></returns>
        public List<string> Serialize()
        {
            List<string> result = new List<string>();

            // We first generate the languages line
            result.Add(string.Join(COMMA.ToString(), languages));

            // Force raw data to enter the dictionary
            flushRaw();

            // and then generate the multilanguage string for each entry
            foreach(EntryI18n entry in dict.Values)
            {
                // Replace real carry returns with the \n text.
                result.Add(entry.ToString());
            }

            return result;
        }

        /// <summary>
        /// Serialization of dictionary.
        /// To save one Localization files per language
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,List<string>> SerializeMultiple()
        {
            // Force raw data to enter the dictionary
            flushRaw();

            Dictionary<string, List<string>> totalResult = new Dictionary<string, List<string>>();

            List<string> oneLangResult;

            // For each language we create a new dictionary
            for (int oneLang = 1; oneLang < languages.Length; oneLang++)
            {
                bool empty = true;
                // We first generate the languages line

                oneLangResult = new List<string>();
                oneLangResult.Add(languages[0] + "," + languages[oneLang]);

                // and then generate the multilanguage string for each entry
                foreach (EntryI18n entry in dict.Values)
                {
                    StringBuilder text = entry.ToString(oneLang);
                    if (text.Length > 0)
                    {
                        empty = false;
                        // Replace real carry returns with the \n text.
                        oneLangResult.Add(new StringBuilder()
                                .Append(entry.key)
                                .Append(COMMA)
                                .Append(entry.ToString(oneLang))
                                .ToString()
                            );
                    }
                }

                if (!empty)
                {
                    totalResult.Add(languages[oneLang], oneLangResult);
                }
            }

            return totalResult;
        }
    }
}
