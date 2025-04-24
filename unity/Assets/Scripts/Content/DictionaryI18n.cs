using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
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
        public string defaultLanguage 
        {
            get => _defaultLanguage;
            set
            {
                AddRequiredLanguage(value);
                _defaultLanguage = value;
            }
        }


        public string currentLanguage
        {
            get => _currentLanguage;
            set
            {
                AddRequiredLanguage(value);
                _currentLanguage = value;
            }
        }

        private Dictionary<string, string> keyToGroup = new Dictionary<string, string>();
        private Dictionary<string, string> groupToLanguage = new Dictionary<string, string>();
        private HashSet<string> requiredLanguages = new HashSet<string> { "English" };

        // Each language has it's own dictionary
        private Dictionary<string, Dictionary<string, string>> data;

        // And each language has it's own raw data
        Dictionary<string, List<string>> rawData;

        // must be loaded to Dictionaries and not raw for edit
        protected bool loadedForEdit = false;
        private string _defaultLanguage = "English";
        private string _currentLanguage = "English";
        private static readonly string DOUBLE_QUOTE = "\"";
        private static readonly string TRIPLE_ENCLOSING = "|||";

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
            string text;

            // Read the whole file
            try
            {
                text = File.ReadAllText(file);
                if(text.Contains('\r'))
                {
                    lines = text.Split('\r');
                }
                else
                {
                    lines = text.Split('\n');
                }
                AddData(lines);
            }
            catch (IOException e)
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
            List<string> rawDataToAdd = new List<string>();

            List<string> currentEntry = new List<string>();
            bool endOfLine = false;
            bool tripleQuoteMode = false;
            // Remove extra new lines
            foreach (string rawLine in languageData)
            {
                string rawLineTrimmed = rawLine.Trim('\r', '\n');
                int sections = rawLineTrimmed.Split('\"').Length;
                bool isFirstLine = !currentEntry.Any();
                currentEntry.Add(rawLineTrimmed);

                // Contains triple quotes
                bool startsNewLineWithTripleQuotes = isFirstLine && rawLineTrimmed.IndexOf($",{TRIPLE_ENCLOSING}", StringComparison.InvariantCulture) != -1;
                if (startsNewLineWithTripleQuotes || tripleQuoteMode)
                {
                    tripleQuoteMode = !rawLineTrimmed.TrimEnd().EndsWith(TRIPLE_ENCLOSING, StringComparison.InvariantCulture);
                    endOfLine = !tripleQuoteMode;
                }
                // Even number of " characters, self contained line
                // Or middle of quote block
                else if ((sections % 2) == 1)
                {
                    // If first line - end the line (single line)
                    // Otherwise it's the middle of the block
                    endOfLine = isFirstLine && !tripleQuoteMode;
                }
                // Odd number of quotes *should* mean start or end of multi line block
                else if (!isFirstLine || isOldFormat(rawLineTrimmed))
                {
                    // Single line or does not have triple quotes
                    endOfLine = !tripleQuoteMode;
                }

                if (endOfLine)
                {
                    var combinedValue = string.Join("\n", currentEntry);
                    rawDataToAdd.Add(combinedValue);

                    currentEntry = new List<string>();
                    endOfLine = false;
                    tripleQuoteMode = false;
                }
            }

            if (currentEntry.Any())
            {
                var combinedValue = string.Join("\\n", currentEntry);
                Debug.Log("Failed to parse language data properly, remaining values: " + combinedValue);
                rawDataToAdd.Add(combinedValue);
            }

            string newLanguage = languageData[0].Split(COMMA)[1].Trim('"');

            if (!rawData.ContainsKey(newLanguage))
            {
                rawData.Add(newLanguage, new List<string>());
            }
            rawData[newLanguage].AddRange(rawDataToAdd);
        }

        private static bool isOldFormat(string rawLine)
        {
            string[] components = rawLine.Split(",".ToCharArray(), 2);
            // Text starts with ", it is a normal multi line block
            bool isNotOldFormat = components.Length > 1 && components[1].Length > 0 && components[1][0] == '\"';
            return !isNotOldFormat;
        }

        /// <summary>
        /// Loads all raw data into Dictionaries.  Must be called before any edits are made.
        /// </summary>
        protected void MakeEditable()
        {
            // Already loaded
            if (loadedForEdit) return;
            
            ForceLoadAllLanguages();

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
        public bool KeyExists(in string key)
        {
            // Check loaded Dictionary data
            foreach (string language in requiredLanguages)
            {
                if (data.TryGetValue(language, out Dictionary<string, string> languageData) 
                    && languageData.ContainsKey(key))
                {
                    return true;
                }
            }

            // If in edit mode don't check raw data, may be outdated
            if (loadedForEdit) return false;

            // Check raw data
            bool found = false;
            // Check all languages
            foreach (KeyValuePair<string, List<string>> kv in rawData)
            {
                // skip languages that are not used
                if (!requiredLanguages.Contains(kv.Key)) continue;
                
                found |= LookInOneLanguage(kv, key);
            }

            if (!found)
               ValkyrieDebug.Log("Key not found: " + key);

            return found;
        }

        /// <summary>
        /// Check if a key exists language 'kv.key' and load it
        /// This function exists as we need to leave foreach loop after finding the right value, without leaving the initial foreach
        /// </summary>
        /// <param name="key">key to check</param>
        private bool LookInOneLanguage(in KeyValuePair<string, List<string>> kv, in string key)
        {
            // Add this language to Dictionary data
            if (!data.ContainsKey(kv.Key))
            {
                data.Add(kv.Key, new Dictionary<string, string>(500));
            }

            if (data[kv.Key].ContainsKey(key))
            {
                ValkyrieDebug.Log("Duplicate Key in " + kv.Key + " Dictionary: " + key);
                return true;
            }

            string key_searched = key + ',';
            foreach (string raw in kv.Value)
            {
                if (raw.StartsWith(key_searched, false, null))
                {
                    string parsed_raw = ParseEntry(raw.Substring(key.Length + 1));

                    // If already present log warning
                    data[kv.Key].Add(key, parsed_raw);

                    // stop searching here, won't find duplicate
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the value for a key.  First check current language, then default, then any.  Returns key if not found.
        /// </summary>
        /// <param name="key">key to retreive</param>
        /// <returns>Value found or 'key' if not found</returns>
        public string GetValue(in string key)
        {
            // KeyExists ensures any matches are loaded to Dictionary data
            if (!KeyExists(key)) return key;

            string additionalLanguage = null;
            if (groupToLanguage.Count > 0)
            {
                if (keyToGroup.TryGetValue(key, out string group))
                {
                    groupToLanguage.TryGetValue(group, out additionalLanguage);
                }
            }

            string secondLanguageValue = null;
            if (additionalLanguage != null && data.ContainsKey(additionalLanguage) && data[additionalLanguage].TryGetValue(key, out secondLanguageValue))
            {
                if (secondLanguageValue.Length == 0)
                {
                    secondLanguageValue = null;
                }
            }

            // Check current language first
            if (data.ContainsKey(currentLanguage) && data[currentLanguage].TryGetValue(key, out string currentLanguageValue) && !string.IsNullOrWhiteSpace(currentLanguageValue))
            {
                return Combine(currentLanguageValue, secondLanguageValue);
            }

            // Then check default language
            if (data.ContainsKey(defaultLanguage) && data[defaultLanguage].TryGetValue(key, out string defaultLanguageValue) && !string.IsNullOrWhiteSpace(defaultLanguageValue))
            {
                return Combine(defaultLanguageValue, secondLanguageValue);
            }

            // Not in current or default, find any match
            foreach (Dictionary<string, string> langData in data.Values)
            {
                if (langData.TryGetValue(key, out string languageValue))
                {
                    return Combine(languageValue, secondLanguageValue);
                }
            }
            // Should never happen
            return "";
        }

        private string Combine(string mainLanguageValue, string secondLanguageValue)
        {
            if (secondLanguageValue == null || secondLanguageValue == mainLanguageValue)
            {
                return mainLanguageValue;
            }
            return $"{mainLanguageValue} [{secondLanguageValue}]";
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
                    string rawValue = entry.Value
                        .Replace("\r\n", "\n")
                        .Replace("\r", "\n")
                        .Replace("\n", "\\n");
                    if (rawValue.Contains(DOUBLE_QUOTE) && !rawValue.Contains(TRIPLE_ENCLOSING))
                    {
                        rawData[kv.Key].Add(entry.Key + ',' + TRIPLE_ENCLOSING + rawValue + TRIPLE_ENCLOSING);
                    }
                    else if (rawValue.Contains(DOUBLE_QUOTE) 
                             || rawValue.Contains(TRIPLE_ENCLOSING)
                             || rawValue.Contains("\\n"))
                    {
                        string quotedLine = DOUBLE_QUOTE + rawValue.Replace(DOUBLE_QUOTE, "\"\"") + DOUBLE_QUOTE;
                        rawData[kv.Key].Add(entry.Key + ',' + quotedLine);
                    }
                    else
                    {
                        rawData[kv.Key].Add(entry.Key + ',' + rawValue);
                    }
                }
            }

            return rawData;
        }

        /// <summary>
        /// Parse raw value entry
        /// </summary>
        /// <param name="entry">entry from raw text</param>
        /// <returns>Entry with newlines and quotes handled</returns>
        protected string ParseEntry(in string entry)
        {
            string parsedReturn = entry.Replace("\\n", "\n");
            // If entry is in triple quotes
            if (parsedReturn.Length >= TRIPLE_ENCLOSING.Length*2 
                && parsedReturn.StartsWith(TRIPLE_ENCLOSING) && parsedReturn.Trim().EndsWith(TRIPLE_ENCLOSING))
            {
                parsedReturn = parsedReturn.Substring(TRIPLE_ENCLOSING.Length, parsedReturn.Length - TRIPLE_ENCLOSING.Length*2);

            }
            // If entry is in quotes
            if (parsedReturn.Length > 1 && parsedReturn[0] == '\"' && parsedReturn.Last() == '\"')
            {
                // Trim leading and trailing quotes
                parsedReturn = parsedReturn.Substring(1, parsedReturn.Length - 2);
                // Handle escaped quotes
                parsedReturn = parsedReturn.Replace("\"\"", DOUBLE_QUOTE);
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
            ForceLoadAllLanguages();
            
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


        /// <summary>
        /// Get list of languages
        /// </summary>
        /// <returns>List of all available languages</returns>
        public List<string> GetLanguagesList()
        {
            return new List<string>(rawData.Keys);
        }

        public void SetKeyToGroup(string key, string groupId)
        {
            keyToGroup[key] = groupId;
        }

        public void SetGroupTranslationLanguage(string groupId, string language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                groupToLanguage.Remove(groupId);
                return;
            }

            groupToLanguage[groupId] = language;
            AddRequiredLanguage(language);
        }

        protected void ForceLoadAllLanguages()
        {
            GetLanguagesList().ForEach(AddRequiredLanguage);
        }
        
        public void AddRequiredLanguage(string value)
        {
            if (requiredLanguages.Add(value))
            {
                // reset loaded data - this won't happen when the data is editable as all languages are already loaded
                data = new Dictionary<string, Dictionary<string, string>>();
            }
        }
    }
}
