using System;
using System.Collections.Generic;
using System.Text;
using ValkyrieTools;


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
        /// Default initial language is English
        /// </summary>
        public string defaultLanguage = "English";

        public string currentLanguage = "English";

        // Each language has it's own dictionary
        private Dictionary<string, Dictionary<string, string>> data;

        // And each language has it's own raw data
        Dictionary<string, List<string>> rawData;

        // must be loaded to Dictionaries and not raw for edit
        protected bool loadedForEdit = false;

        /// <summary>
        /// Construct a new empty dictionary
        /// </summary>
        public DictionaryI18n()
        {
            data = new Dictionary<string, Dictionary<string, string>>();
            rawData = new Dictionary<string, List<string>>();
            currentLanguage = Game.Get().currentLang;
        }

        /// <summary>
        /// Construct a new empty dictionary with a default language
        /// </summary>
        /// <param name="newDefaultLanguage">Language to use as default</param>
        public DictionaryI18n(string newDefaultLanguage)
        {
            data = new Dictionary<string, Dictionary<string, string>>();
            rawData = new Dictionary<string, List<string>>();
            currentLanguage = Game.Get().currentLang;
        }

        /// <summary>
        /// Construct a dictionary and add language data
        /// </summary>
        public DictionaryI18n(string[] languageData)
        {
            data = new Dictionary<string, Dictionary<string, string>>();
            rawData = new Dictionary<string, List<string>>();
            currentLanguage = Game.Get().currentLang;
            AddData(languageData);
        }

        /// <summary>
        /// Construct a dictionary with a default language and add language data
        /// </summary>
        /// <param name="languageData">Language data</param>
        /// <param name="newDefaultLanguage">Language to use as default</param>
        public DictionaryI18n(string[] languageData, string newDefaultLanguage)
        {
            data = new Dictionary<string, Dictionary<string, string>>();
            rawData = new Dictionary<string, List<string>>();
            defaultLanguage = newDefaultLanguage;
            currentLanguage = Game.Get().currentLang;
            AddData(languageData);
        }

        /// <summary>
        /// Add language data to dictionary from file path.
        /// </summary>
        /// <param name="file">Language file to add</param>
        public void AddDataFromFile(string file)
        {
            string[] lines;

            // Read the whole file
            try
            {
                lines = System.IO.File.ReadAllLines(file);
                AddData(lines);
            }
            catch (System.IO.IOException e)
            {
                ValkyrieDebug.Log("Error loading localization file " + file + ":" + e.Message);
            }
        }

        /// <summary>
        /// Add language data to dictionary
        /// </summary>
        /// <param name="languageData">Language data</param>
        public void AddData(string[] languageData)
        {
            string newLanguage = languageData[0].Split(COMMA)[1].Trim('"');

            if (!rawData.ContainsKey(newLanguage))
            {
                rawData.Add(newLanguage, new List<string>());
            }
            rawData[newLanguage].AddRange(languageData);
        }

        /// <summary>
        /// Loads all raw data into Dictionaries.  Must be called before any edits are made.
        /// </summary>
        protected void MakeEditable()
        {
            // Already loaded
            if (loadedForEdit) return;

            // Remove all existing entries (helps maintain order)
            data = new Dictionary<string, Dictionary<string, string>>();

            // For all languages
            foreach (KeyValuePair<string, List<string>> kv in rawData)
            {
                // Create a Dictionary for each language
                data.Add(kv.Key, new Dictionary<string, string>());
                // Check each line
                for(int i = 1; i < kv.Value.Count; i++)
                {
                    // Ignore comments
                    if (kv.Value[i].Trim().IndexOf("//") == 0) continue;

                    // Split out key
                    string[] components = kv.Value[i].Split(",".ToCharArray(), 2);
                    if (components.Length != 2) continue;

                    // Only store the first occurance of a key
                    if (!data[kv.Key].ContainsKey(components[0]))
                    {
                        data[kv.Key].Add(components[0], ParseEntry(components[1]));
                    }
                }
            }
            loadedForEdit = true;
        }

        /// <summary>
        /// Add an entry with the current language, replace if exists
        /// </summary>
        /// <param name="key">Entry key</param>
        /// <param name="value">Data to add</param>
        public void AddEntry(string key, string value)
        {
            AddEntry(key, value, currentLanguage);
        }

        /// <summary>
        /// Add an entry to a language, replace if exists
        /// </summary>
        /// <param name="key">Entry key</param>
        /// <param name="value">Data to add</param>
        /// <param name="language">Language to use</param>
        public void AddEntry(string key, string value, string language)
        {
            MakeEditable();

            // Create language Dictionary if missing
            if (!data.ContainsKey(language))
            {
                data.Add(language, new Dictionary<string, string>());
            }

            if (data[language].ContainsKey(key))
            {
                data[language][key] = value;
            }
            else
            {
                data[language].Add(key, value);
            }
        }

        /// <summary>
        /// Remove an entry from all languages
        /// </summary>
        /// <param name="key">Entry key</param>
        public void Remove(string key)
        {
            MakeEditable();
            foreach (string lang in data.Keys)
            {
                if (data[lang].ContainsKey(key))
                {
                    data[lang].Remove(key);
                }
            }
        }

        /// <summary>
        /// Remove all entries starting with prefix from all languages
        /// </summary>
        /// <param name="prefix">prefix to match</param>
        public void RemoveKeyPrefix(string prefix)
        {
            MakeEditable();

            foreach (Dictionary<string, string> languageData in data.Values)
            {
                // Build list of keys to remove
                List<string> toRemove = new List<string>();
                foreach (string key in languageData.Keys)
                {
                    if (key.IndexOf(prefix) == 0)
                    {
                        toRemove.Add(key);
                    }
                }

                // Remove all keys
                foreach (string key in toRemove)
                {
                    languageData.Remove(key);
                }
            }
        }

        /// <summary>
        /// Rename key for all entries starting with prefix from all languages
        /// </summary>
        /// <param name="oldPrefix">prefix to match</param>
        /// <param name="newPrefix">text to replace prefix</param>
        public void RenamePrefix(string oldPrefix, string newPrefix)
        {
            MakeEditable();

            foreach (Dictionary<string, string> languageData in data.Values)
            {
                // Build list of keys to rename
                Dictionary<string, string> toRename = new Dictionary<string, string>();
                foreach (string key in languageData.Keys)
                {
                    if (key.IndexOf(oldPrefix) == 0)
                    {
                        toRename.Add(key, newPrefix + key.Substring(oldPrefix.Length));
                    }
                }

                // Replace with new key name
                foreach (KeyValuePair<string, string> kv in toRename)
                {
                    languageData.Add(kv.Value, languageData[kv.Key]);
                    languageData.Remove(kv.Key);
                }
            }
        }

        /// <summary>
        /// Check if a key exists in any language, also ensures all matching values are loaded into Dictionary objects
        /// </summary>
        /// <param name="key">key to check</param>
        public bool KeyExists(string key)
        {
            // Check loaded Dictionary data
            foreach (Dictionary<string, string> languageData in data.Values)
            {
                if (languageData.ContainsKey(key)) return true;
            }

            // If in edit mode don't check raw data, may be outdated
            if (loadedForEdit) return false;

            // Check raw data
            bool found = false;
            // Check all languages
            foreach (KeyValuePair<string, List<string>> kv in rawData)
            {
                // Check all lines
                foreach (string raw in kv.Value)
                {
                    if (raw.IndexOf(key + ',') == 0)
                    {
                        // Add this language to Dictionary data
                        if (!data.ContainsKey(kv.Key))
                        {
                            data.Add(kv.Key, new Dictionary<string, string>());
                        }

                        // If already present log warning
                        if (data[kv.Key].ContainsKey(key))
                        {
                            ValkyrieDebug.Log("Duplicate Key in " + kv.Key + " Dictionary: " + key);
                        }
                        else
                        {
                            data[kv.Key].Add(key, ParseEntry(raw.Substring(raw.IndexOf(',') + 1)));
                        }

                        // Continue after found to ensure all languages are loaded
                        found = true;
                    }
                }
            }

            return found;
        }

        /// <summary>
        /// Get the value for a key.  First check current language, then default, then any.  Returns key if not found.
        /// </summary>
        /// <param name="key">key to retreive</param>
        /// <returns>Value found or 'key' if not found</returns>
        public string GetValue(string key)
        {
            // KeyExists ensures any matches are loaded to Dictionary data
            if (!KeyExists(key)) return key;

            // Check current language first
            if (data.ContainsKey(currentLanguage) && data[currentLanguage].ContainsKey(key))
            {
                return data[currentLanguage][key];
            }

            // Then check default language
            if (data.ContainsKey(defaultLanguage) && data[defaultLanguage].ContainsKey(key))
            {
                return data[defaultLanguage][key];
            }

            // Not in current or default, find any match
            foreach (Dictionary<string, string> langData in data.Values)
            {
                if (langData.ContainsKey(key))
                {
                    return langData[key];
                }
            }
            // Should never happen
            return "";
        }

        /// <summary>
        /// Get the raw language data for this dictionary
        /// </summary>
        /// <returns>raw data by language</returns>
        public Dictionary<string,List<string>> SerializeMultiple()
        {
            // If we haven't edited can return what we were given
            if (!loadedForEdit) return rawData;

            // If we have edited we can replace the rawData
            rawData = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<string, Dictionary<string, string>> kv in data)
            {
                rawData.Add(kv.Key, new List<string>());
                rawData[kv.Key].Add(".," + kv.Key);
                foreach (KeyValuePair<string, string> entry in kv.Value)
                {
                    rawData[kv.Key].Add(entry.Key + ',' + entry.Value.Replace("\n", "\\n"));
                }
            }

            return rawData;
        }

        /// <summary>
        /// Parse raw value entry
        /// </summary>
        /// <param name="entry">entry from raw text</param>
        /// <returns>Entry with newlines and quotes handled</returns>
        protected string ParseEntry(string entry)
        {
            string parsedReturn = entry.Replace("\\n", "\n");
            // If entry is in quotes
            if (parsedReturn.Length > 2 && parsedReturn[0] == '\"')
            {
                // Trim leading and trailing quotes
                parsedReturn = parsedReturn.Substring(1, parsedReturn.Length - 2);
                // Handle escaped quotes
                parsedReturn = parsedReturn.Replace("\"\"", "\"");
            }
            return parsedReturn;
        }

        /// <summary>
        /// Get matches to a key for all languages
        /// </summary>
        /// <param name="key">key to check</param>
        /// <returns>Dictionary of results for each language</returns>
        public Dictionary<string, string> ExtractAllMatches(string key)
        {
            // Load into data
            KeyExists(key);

            Dictionary<string, string> returnData = new Dictionary<string, string>();
            foreach (KeyValuePair<string, Dictionary<string,  string>> kv in data)
            {
                if (kv.Value.ContainsKey(key))
                {
                    returnData.Add(kv.Key, kv.Value[key]);
                }
            }
            return returnData;
        }
    }
}
