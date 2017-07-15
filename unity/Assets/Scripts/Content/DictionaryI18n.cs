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

        protected bool loadedForEdit = false;

        public DictionaryI18n()
        {
            data = new Dictionary<string, Dictionary<string, string>>();
            rawData = new Dictionary<string, List<string>>();
            currentLanguage = Game.Get().currentLang;
        }

        public DictionaryI18n(string newDefaultLanguage)
        {
            data = new Dictionary<string, Dictionary<string, string>>();
            rawData = new Dictionary<string, List<string>>();
            currentLanguage = Game.Get().currentLang;
        }

        public DictionaryI18n(string[] languageData)
        {
            data = new Dictionary<string, Dictionary<string, string>>();
            rawData = new Dictionary<string, List<string>>();
            currentLanguage = Game.Get().currentLang;
            AddData(languageData);
        }

        public DictionaryI18n(string[] languageData, string newDefaultLanguage)
        {
            data = new Dictionary<string, Dictionary<string, string>>();
            rawData = new Dictionary<string, List<string>>();
            defaultLanguage = newDefaultLanguage;
            currentLanguage = Game.Get().currentLang;
            AddData(languageData);
        }

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

        public void AddData(string[] languageData)
        {
            string newLanguage = languageData[0].Split(COMMA)[1];

            if (!rawData.ContainsKey(newLanguage))
            {
                rawData.Add(newLanguage, new List<string>());
            }
            rawData[newLanguage].AddRange(languageData);
        }

        protected void MakeEditable()
        {
            if (loadedForEdit) return;

            data = new Dictionary<string, Dictionary<string, string>>();

            foreach (KeyValuePair<string, List<string>> kv in rawData)
            {
                data.Add(kv.Key, new Dictionary<string, string>());
                for(int i = 1; i < kv.Value.Count; i++)
                {
                    if (kv.Value[i].Trim().IndexOf("//") == 0) continue;

                    string[] components = kv.Value[i].Split(",".ToCharArray(), 2);
                    if (components.Length != 2) continue;

                    if (!data[kv.Key].ContainsKey(components[0]))
                    {
                        data[kv.Key].Add(components[0], ParseEntry(components[1]));
                    }
                }
            }
            loadedForEdit = true;
        }

        public void AddEntry(string key, string value)
        {
            AddEntry(key, value, currentLanguage);
        }

        public void AddEntry(string key, string value, string language)
        {
            MakeEditable();
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

        public void RemoveKeyPrefix(string prefix)
        {
            MakeEditable();

            foreach (Dictionary<string, string> languageData in data.Values)
            {
                List<string> toRemove = new List<string>();
                foreach (string key in languageData.Keys)
                {
                    if (key.IndexOf(prefix) == 0)
                    {
                        toRemove.Add(key);
                    }
                }
                foreach (string key in toRemove)
                {
                    languageData.Remove(key);
                }
            }
        }

        public void RenamePrefix(string oldPrefix, string newPrefix)
        {
            MakeEditable();

            foreach (Dictionary<string, string> languageData in data.Values)
            {
                Dictionary<string, string> toRename = new Dictionary<string, string>();
                foreach (string key in languageData.Keys)
                {
                    if (key.IndexOf(oldPrefix) == 0)
                    {
                        toRename.Add(key, newPrefix + key.Substring(oldPrefix.Length));
                    }
                }
                foreach (KeyValuePair<string, string> kv in toRename)
                {
                    languageData.Add(kv.Value, languageData[kv.Key]);
                    languageData.Remove(kv.Key);
                }
            }
        }

        public bool KeyExists(string key)
        {
            foreach (Dictionary<string, string> languageData in data.Values)
            {
                if (languageData.ContainsKey(key)) return true;
            }

            if (loadedForEdit) return false;

            bool found = false;
            foreach (KeyValuePair<string, List<string>> kv in rawData)
            {
                foreach (string raw in kv.Value)
                {
                    if (raw.IndexOf(key + ',') == 0)
                    {
                        if (!data.ContainsKey(kv.Key))
                        {
                            data.Add(kv.Key, new Dictionary<string, string>());
                        }
                        if (data[kv.Key].ContainsKey(key))
                        {
                            ValkyrieDebug.Log("Duplicate Key in " + kv.Key + " Dictionary: " + key);
                        }
                        else
                        {
                            data[kv.Key].Add(key, ParseEntry(raw.Substring(raw.IndexOf(',') + 1)));
                        }
                        found = true;
                    }
                }
            }

            return found;
        }

        public string GetValue(string key)
        {
            if (!KeyExists(key)) return key;

            // Key Exists forces the data to be in the dict, don't need to check raw
            if (data.ContainsKey(currentLanguage) && data[currentLanguage].ContainsKey(key))
            {
                return data[currentLanguage][key];
            }
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

        public Dictionary<string,List<string>> SerializeMultiple()
        {
            if (!loadedForEdit) return rawData;

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

        protected string ParseEntry(string entry)
        {
            string parsedReturn = entry.Replace("\\n", "\n");
            if (parsedReturn.Length > 2 && parsedReturn[0] == '\"')
            {
                parsedReturn = parsedReturn.Substring(1, parsedReturn.Length - 2);
                parsedReturn = parsedReturn.Replace("\"\"", "\"");
            }
            return parsedReturn;
        }

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
