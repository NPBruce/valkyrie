using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Assets.Scripts.Content
{
    // Helper class to read a localization file into a nested dictionary
    // This exists because .NET/Mono doesn't have one!!
    public static class LocalizationRead
    {
        public static DictionaryI18n ffgDict = null;
        public static DictionaryI18n valkyrieDict = null;
        public static DictionaryI18n scenarioDict = null;

        /// <summary>
        /// Change all dictionary languages
        /// </summary>
        /// <param name="newLang">string for new language</param>
        public static void changeCurrentLangTo(string newLang)
        {
            if (ffgDict != null)
            {
                ffgDict.setCurrentLanguage(newLang);
            }
            if (valkyrieDict != null)
            {
                valkyrieDict.setCurrentLanguage(newLang);
            }
            if (scenarioDict != null)
            {
                scenarioDict.setCurrentLanguage(newLang);
            }
        }

        // Function takes Unity TextAsset and returns localization dictionary
        public static DictionaryI18n ReadFromTextAsset(TextAsset asset, string newCurrentLang)
        {
            string[] lines;
            try
            {
                // split text into array of lines
                lines = asset.text.Split(new string[] { "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            }
            catch (System.Exception e)
            {
                ValkyrieDebug.Log("Error loading localization from asset " + asset.name + ":" + e.Message);
                return null;
            }

            // The assets has the english as default language
            return new DictionaryI18n(lines,DictionaryI18n.DEFAULT_LANG,newCurrentLang);
        }

        // Function takes path to localization file and returns data object
        // Returns null on error
        public static DictionaryI18n ReadFromFilePath(string path, string newDefaultLang, string newCurrentLang)
        {
            string[] lines;

            // Read the whole file
            try
            {
                lines = System.IO.File.ReadAllLines(path);
            }
            catch (System.Exception e)
            {
                ValkyrieDebug.Log("Error loading localization file " + path + ":" + e.Message);
                return null;
            }
            // Parse text data
            return new DictionaryI18n(lines, newDefaultLang,newCurrentLang);
        }


        // Function ini file contents as a string and returns data object
        // Returns null on error
        public static DictionaryI18n ReadFromString(string content, string newDefaultLang, string newCurrentLang)
        {
            // split text into array of lines
            string[] lines = content.Split(new string[] { "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            return new DictionaryI18n(lines, newDefaultLang, newCurrentLang);
        }


        // Check for FFG text lookups and insert required text
        /// <summary>
        /// Replaces a FFG key with his current text
        /// </summary>
        /// <param name="input">{ffg:XXXX} like input</param>
        /// <returns>Translation to current language</returns>
        public static string DictLookup(StringKey input)
        {
            string output = input.key;
            // While there are more lookups

            string regexKey = "{(ffg|val|qst):";

            //while (output.IndexOf("{ffg:") != -1)
            while (Regex.Match(output,regexKey).Success)
            {
                int pos = Regex.Match(output, regexKey).Index;
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
                // Replace the lookup

                output = output.Replace("{" + dict + ":" + lookup + "}", result);
            }
            return output;
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


        /// <summary>
        /// Transform a ffg key (without ffg prefig, into current language text
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string DictKeyLookup(string dict, string key)
        {
            DictionaryI18n currentDict = selectDictionary(dict);

            if (currentDict != null)
            {
                EntryI18n valueOut;

                if (currentDict.tryGetValue(key, out valueOut))
                {
                    return valueOut.getCurrentOrDefaultLanguageString();
                }
                else
                {
                    return key;
                }
            } else
            {
                ValkyrieDebug.Log("Error: current dictionary not loaded");
            }
            return key;
        }

        /// <summary>
        /// dictionary selection from string
        /// </summary>
        /// <param name="dict">dictionary name</param>
        /// <returns>dictionary selected</returns>
        private static DictionaryI18n selectDictionary(string dict)
        {
            switch (dict)
            {
                case "ffg":
                    return ffgDict;
                case "val":
                    return valkyrieDict;
                case "qst":
                    return scenarioDict;
                default:
                    return null;
            }
        }
    }
}
