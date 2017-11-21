
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using ValkyrieTools;

namespace Assets.Scripts.Content
{
    // Helper class to read a localization file into a nested dictionary
    // This exists because .NET/Mono doesn't have one!!
    public static class LocalizationRead
    {
        public static Dictionary<string, DictionaryI18n> dicts = new Dictionary<string, DictionaryI18n>();

        /// <summary>
        /// Change all dictionary languages
        /// </summary>
        /// <param name="newLang">string for new language</param>
        public static void changeCurrentLangTo(string newLang)
        {
            foreach (DictionaryI18n d in dicts.Values)
            {
                d.currentLanguage = newLang;
            }
        }

        private const int RECURSIVE_LIMIT = 10;

        // Check for FFG text lookups and insert required text
        /// <summary>
        /// Replaces a FFG key with his current text
        /// </summary>
        /// <param name="input">{ffg:XXXX} like input</param>
        /// <returns>Translation to current language</returns>
        public static string DictLookup(StringKey input)
        {
            string output = input.fullKey;
            // While there are more lookups

            // Count the number of replaces. One lookup should not replace more than RECURSIVE_LIMIT elements.
            int recursiveCount = 0;

            //while (output.IndexOf("{ffg:") != -1)
            while (Regex.Match(output, LookupRegexKey()).Success && recursiveCount < RECURSIVE_LIMIT)
            {
                int pos = Regex.Match(output, LookupRegexKey()).Index;
                // Can be nested
                int bracketLevel = 1;
                // Start of lookup
                // ffg val and qst has the same length
                int lookupStart = pos + "{ffg:".Length;

                // Loop to find end of lookup
                int lookupEnd = lookupStart;
                while (bracketLevel > 0)
                {
                    lookupEnd++;
                    if (output[lookupEnd].Equals('{'))
                    {
                        bracketLevel++;
                    }
                    if (output[lookupEnd].Equals('}'))
                    {
                        bracketLevel--;
                    }
                }

                // Extract lookup key
                string lookup = output.Substring(lookupStart, lookupEnd - lookupStart);

                // dict
                string dict = output.Substring(pos + 1, 3);

                // Get key result
                string result = DictQuery(dict,lookup);

                // We (unity) don't support underlines
                // Unity uses <> not []
                result = result.Replace("[u]", "<b>").Replace("[/u]", "</b>");
                result = result.Replace("[i]", "<i>").Replace("[/i]", "</i>");
                result = result.Replace("[b]", "<b>").Replace("[/b]", "</b>");

                // Some FFG text doesn't close b/i like it should
                while (Regex.Matches(result, "<b>").Count > Regex.Matches(result, "</b>").Count)
                {
                    result += "</b>";
                }
                while (Regex.Matches(result, "<i>").Count > Regex.Matches(result, "</i>").Count)
                {
                    result += "</i>";
                }

                // Replace the lookup
                output = output.Replace("{" + dict + ":" + lookup + "}", result);
                // Increase the recursive count
                recursiveCount++;
            }

            if (recursiveCount == RECURSIVE_LIMIT)
            {
                ValkyrieDebug.Log("ERROR Recursive loop limit reached translating " + input.fullKey + ". Dictionary entry must be fixed.");
            }

            return output;
        }

        public static bool CheckLookup(StringKey input)
        {
            string output = input.fullKey;
            // While there are more lookups

            if (!Regex.Match(output, LookupRegexKey()).Success) return false;

            int pos = Regex.Match(output, LookupRegexKey()).Index;
            // Can be nested
            int bracketLevel = 1;
            // Start of lookup
            // ffg val and qst has the same length
            int lookupStart = pos + "{ffg:".Length;

            // Loop to find end of lookup
            int lookupEnd = lookupStart;
            while (bracketLevel > 0)
            {
                lookupEnd++;
                if (output[lookupEnd].Equals('{'))
                {
                    bracketLevel++;
                }
                if (output[lookupEnd].Equals('}'))
                {
                    bracketLevel--;
                }
            }

            // Extract lookup key
            string lookup = output.Substring(lookupStart, lookupEnd - lookupStart);

            // dict
            string dict = output.Substring(pos + 1, 3);

            // Get key result
            return CheckDictQuery(dict,lookup);
        }

        /// <summary>
        /// Look up a key in the FFG text Localization. Can have parameters divided by ":"
        /// Example: A_GOES_B_MESSAGE:{A}:Peter:{B}:Dinning Room
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string DictQuery(string dict, string input)
        {
            int bracketLevel = 0;
            int lastSection = 0;
            List<string> elements = new List<string>();

            // Separate the input into sections
            for (int index = 0; index < input.Length; index++)
            {
                if (input[index].Equals('{'))
                {
                    bracketLevel++;
                }
                if (input[index].Equals('}'))
                {
                    bracketLevel--;
                }
                // Section divider
                if (input[index].Equals(':'))
                {
                    // Not in brackets
                    if (bracketLevel == 0)
                    {
                        // Add previous element
                        elements.Add(input.Substring(lastSection, index - lastSection));
                        lastSection = index + 1;
                    }
                }
            }
            // Add previous element
            elements.Add(input.Substring(lastSection, input.Length - lastSection));

            // Look up the first element (key)
            string fetched = DictKeyLookup(dict,elements[0]);

            // Find and replace with other elements
            for (int i = 2; i < elements.Count; i += 2)
            {
                fetched = fetched.Replace(elements[i - 1], elements[i]);
            }
            return fetched;
        }

        private static bool CheckDictQuery(string dict, string input)
        {
            int bracketLevel = 0;
            int lastSection = 0;
            List<string> elements = new List<string>();

            // Separate the input into sections
            for (int index = 0; index < input.Length; index++)
            {
                if (input[index].Equals('{'))
                {
                    bracketLevel++;
                }
                if (input[index].Equals('}'))
                {
                    bracketLevel--;
                }
                // Section divider
                if (input[index].Equals(':'))
                {
                    // Not in brackets
                    if (bracketLevel == 0)
                    {
                        // Add previous element
                        elements.Add(input.Substring(lastSection, index - lastSection));
                        lastSection = index + 1;
                    }
                }
            }
            // Add previous element
            elements.Add(input.Substring(lastSection, input.Length - lastSection));

            DictionaryI18n currentDict = selectDictionary(dict);
            if (currentDict == null) return false;
            return currentDict.KeyExists(elements[0]);
        }

        /// <summary>
        /// Sets a dictionary entry for key and text.Creates or replaces
        /// </summary>
        /// <param name="key">key of the string</param>
        /// <param name="text">text to insert in current language</param>
        public static void updateScenarioText(string key, string text)
        {
            dicts["qst"].AddEntry(key, text);
        }

        /// <summary>
        /// Transform a ffg key (without ffg prefig, into current language text
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string DictKeyLookup(string dict, string key)
        {
            DictionaryI18n currentDict = selectDictionary(dict);

            if (currentDict == null)
            {
                ValkyrieDebug.Log("Error: current dictionary not loaded");
                return key;
            }
            return currentDict.GetValue(key);
        }

        /// <summary>
        /// dictionary selection from string
        /// </summary>
        /// <param name="dict">dictionary name</param>
        /// <returns>dictionary selected</returns>
        public static DictionaryI18n selectDictionary(string dict)
        {
            if (!dicts.ContainsKey(dict)) return null;

            return dicts[dict];
        }

        /// <summary>
        /// Add a new dictionary, replaces if exists
        /// </summary>
        /// <param name="name">dictionary name</param>
        /// <param name="dict">DictionaryI18n data</param>
        /// <returns>void</returns>
        public static void AddDictionary(string name, DictionaryI18n dict)
        {
            if (!dicts.ContainsKey(name))
            {
                dicts.Add(name, dict);
            }
            else
            {
                dicts[name] = dict;
            }
        }

        /// <summary>
        /// Get a regex pattern to check if is it a valid lookup key
        /// </summary>
        /// <returns>regex string</returns>
        public static string LookupRegexKey()
        {
            string regexKey = "{(";
            foreach (string key in dicts.Keys)
            {
                regexKey += key + "|";
            }
            return regexKey.Substring(0, regexKey.Length - 1) + "):";
        }
    }
}
