using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Content
{
    /// <summary>
    /// String in international FFG format. Including all available languages supported by MoM App
    /// </summary>
    public class StringI18n
    {
        /// <summary>
        /// Fixed value included in FFGs Localization.txt file
        /// </summary>
        public const String FFG_LANGS = ".,English,Spanish,French,German,Italian,Portuguese,Polish,Japanese,Chinese";

        private static String[] languages;

        private static int currentLanguage;
        /// <summary>
        /// Static initialization of languages list
        /// </summary>
        static StringI18n()
        {
            setLanguages(FFG_LANGS);
        }

        /// <summary>
        /// Set the languages list from a comma separed string with them.
        /// </summary>
        /// <param name="stringLanguages">comma separed string</param>
        public static void setLanguages(String stringLanguages)
        {
            languages = stringLanguages.Split(COMMA);
            // Default language is english = 1. 
            // Language 0 is the key
            currentLanguage = 1;
        }

        /// <summary>
        /// Sets current language using the string of the language.
        /// If there is no language with this name, the default language
        /// remains the same.
        /// </summary>
        /// <param name="languageName">Name of the language</param>
        public static void setCurrentLanguage(String languageName)
        {
            for (int pos = 1; pos < languages.Length; pos++)
            {
                if (languages[pos] == languageName)
                {
                    currentLanguage = pos;
                }
            }
        }

        /// <summary>
        /// Sets curreng language using its position. 0 doesn't count.
        /// </summary>
        /// <param name="languagePosition"></param>
        public static void setCurrentLanguage(int languagePosition)
        {
            if (languagePosition != 0)
            {
                currentLanguage = languagePosition;
            }
        }

        /// <summary>
        /// Instance info of the current translations
        /// </summary>
        private String[] translations;

        /// <summary>
        /// Creates an empty instance of a Multilanguage String
        /// </summary>
        public StringI18n()
        {
            translations = new String[languages.Length];
        }

        private const char QUOTES = '\"';
        private const char COMMA = ',';

        /// <summary>
        /// Constructor with the complete localisation elements
        /// </summary>
        /// <param name="completeLocalisationString"></param>
        public StringI18n(String completeLocalisationString)
        {
            if (completeLocalisationString.Contains(QUOTES))
            {
                // with quotes, commas inside quotes isn't considered separator
                List<String> partialTranslation = new List<String>(completeLocalisationString.Split(COMMA));
                List<String> finalTranslation = new List<string>();
                String currentTranslation = "";
                bool opened = false;
                foreach (String suposedTranslation in partialTranslation)
                {
                    currentTranslation += suposedTranslation;

                    bool initialQuote = suposedTranslation.Length != 0 && suposedTranslation[0] == QUOTES;
                    bool finalQuote = suposedTranslation.Length > 1 &&
                        suposedTranslation[suposedTranslation.Length - 1] == QUOTES;

                    // If contains one quote we need to analyze
                    if (initialQuote ^ finalQuote)
                    {
                        if (opened)
                        {
                            // Closing quotes
                            finalTranslation.Add(
                                // remove initial and final quote
                                currentTranslation.Substring(1, currentTranslation.Length - 2)
                                // replace double quotes for single quotes
                                .Replace("\"\"", "\"")
                                );
                            currentTranslation = "";
                        }
                        else
                        {
                            currentTranslation += COMMA;
                        }
                        opened = !opened;
                    }
                    else
                    {
                        if (initialQuote)
                        {
                            currentTranslation = currentTranslation.Substring(1, currentTranslation.Length - 2);
                        }
                        // other options are no quotes
                        // both need same proceed.
                        finalTranslation.Add(currentTranslation.Replace("\"\"", "\""));
                        currentTranslation = "";
                    }
                }
                translations = finalTranslation.ToArray();
            }
            else
            {
                // Without quotes, all commas are separators
                translations = completeLocalisationString.Split(COMMA);
            }
        }

        // The key is que position 0 of the array
        public String key
        {
            get
            {
                return translations[0];
            }
        }

        public String getSpecificLanguageString(int nLanguage)
        {
            return translations[nLanguage];
        }

        /// <summary>
        /// In translation of texts. If we don't have current language text, a
        /// specific language text will be got. In order to know if there is a 
        /// current language text the method HasTextInCurrentLanguage can be used.
        /// </summary>
        /// <param name="nLanguage">number of the language to use</param>
        /// <returns></returns>
        public String getCurrentOrSpecificLanguageString(int nLanguage)
        {
            if (HasTextInCurrentLanguage)
            {
                return currentLanguageString;
            } else
            {
                return getSpecificLanguageString(nLanguage);
            }            
        }

        /// <summary>
        /// The string value of the key whith the current language
        /// </summary>
        public String currentLanguageString
        {
            get
            {
                return translations[currentLanguage];
            }
            set
            {
                translations[currentLanguage] = value;
            }
        }

        public bool HasTextInCurrentLanguage
        {
            get
            {
                return currentLanguageString.Length > 0;
            }
        }

        /// <summary>
        /// String representation of the multilanguage element
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            Boolean first = true;
            foreach (String oneTranslation in translations)
            {
                if (!first)
                {
                    result.Append(COMMA);
                }
                if (oneTranslation.Contains(COMMA))
                {
                    // The serializable text should repeat mid quotes and add initial and final quotes
                    result.Append(QUOTES).Append(oneTranslation.Replace(QUOTES.ToString(),"\"\"")).Append(QUOTES);
                }
                else
                {
                    result.Append(oneTranslation);
                }
                if (first)
                {
                    first = false;
                }
            }

            return result.ToString();
        }
    }
}
