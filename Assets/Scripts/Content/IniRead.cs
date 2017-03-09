using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

// Helper class to read an ini file into a nested dictionary
// This exists because .NET/Mono doesn't have one!!
public static class IniRead{
    // Function takes path to ini file and returns data object
    // Returns null on error
    public static IniData ReadFromIni(string path)
    {
        string[] lines;

        // Read the whole file
        try
        {
            lines = System.IO.File.ReadAllLines(path);
        }
        catch (System.Exception)
        {
            return null;
        }
        // Parse text data
        return ReadFromStringArray(lines, path);
    }


    // Function ini file contents as a string and returns data object
    // Returns null on error
    public static IniData ReadFromString(string content)
    {
        // split text into array of lines
        string[] lines = content.Split(new string[] { "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        return ReadFromStringArray(lines, "<INTERNAL>");
    }

    // Parse ini data into data structure
    public static IniData ReadFromStringArray(string[] lines, string path)
    {
        // Create a dictionary for the first section
        Dictionary<string, string> entryData = new Dictionary<string, string>();
        // Name for the current section
        string entryName = "";
        // Create object to hold output
        IniData output = new IniData();
        // Section headers have these chars removed
        char[] charsToTrim = { '[', ']' };

        // Parse all lines
        foreach (string l in lines)
        {
            // Start of new section
            if (l.Length > 0 && l.Trim()[0] == '[')
            {
                // If not first section, add the last section of data
                if (entryName != "")
                {
                    if (!output.Add(entryName, entryData))
                    {
                        Debug.Log("Warning: duplicate section \"" + entryName + "\" in " + path + " will be ignored.");
                    }
                }
                // create new data for new section
                entryData = new Dictionary<string, string>();
                // Get name of new section
                entryName = l.Trim().Trim(charsToTrim);
                // Blank section names not allowed, but not fatal
                if(entryName.Equals(""))
                {
                    Debug.Log("Warning: empty section in " + path + " will be ignored.");
                }
            }
            // If the line is not a comment (starts with ;)
            else if (l.Length > 0 && l.Trim()[0] != ';')
            {
                int equalsLocation = l.IndexOf('=');
                // Add data as entry with no value
                if (equalsLocation == -1)
                    entryData.Add(l.Trim(), "");
                // If there is an = add data as key and value
                else
                {
                    string key = l.Substring(0, equalsLocation).Trim();
                    if(entryData.ContainsKey(key))
                    {
                        Debug.Log("Warning: duplicate \"" + key + "\" data in section \"" + entryName + "\" in " + path + " will be ignored.");
                    }
                    else
                    {
                        string value = FFGLookup(l.Substring(equalsLocation + 1).Trim().Trim('\"'));
                        entryData.Add(key, value);
                    }
                }
                // This won't go anywhere if we don't have a section
                if (entryName.Equals(""))
                {
                    Debug.Log("Warning: data without section in " + path + " will be ignored.");
                }
            }
        }

        // Add the last section
        if (entryName != "")
        {
            if (!output.Add(entryName, entryData))
            {
                Debug.Log("Warning: duplicate section \"" + entryName + "\" in " + path + " will be ignored.");
            }
        }

        return output;
    }
    
    // Check for FFG text lookups and insert required text
    public static string FFGLookup(string input)
    {
        string output = input;
        // While there are more lookups
        while (output.IndexOf("{ffg:") != -1)
        {
            // Can be nested
            int bracketLevel = 1;
            // Start of lookup
            int lookupStart = output.IndexOf("{ffg:") + "{ffg:".Length;

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
            // Get key result
            string result = FFGQuery(lookup);
            // We (unity) don't support underlines
            // Unity uses <> not []
            result = result.Replace("[u]", "<b>").Replace("[/u]", "</b>");
            result = result.Replace("[i]", "<i>").Replace("[/i]", "</i>");
            result = result.Replace("[b]", "<b>").Replace("[/b]", "</b>");
            // Replace the lookup
            output = output.Replace("{ffg:" + lookup + "}", result);
        }
        return output;
    }

    // Look up a key in the FFG text Localization
    public static string FFGQuery(string input)
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
        string fetched = FFGKeyLookup(elements[0]);

        // Find and replace with other elements
        for (int i = 2; i < elements.Count; i += 2)
        {
            fetched = fetched.Replace(elements[i - 1], elements[i]);
        }
        return fetched;
    }

    // Key lookup in localization
    public static string FFGKeyLookup(string key)
    {
        // FIXME This appears redundant
        string[] elements = key.Split(":".ToCharArray());

        try
        {
            Game game = Game.Get();

            // We load the text into the game object so we only have to load it once
            if (game.ffgDict == null)
            {
                // FFG default language is allways English
                game.ffgDict = new DictionaryI18n(
                    System.IO.File.ReadAllLines(game.gameType.DataDirectory() + "ffg/text/Localization.txt"),
                    DictionaryI18n.DEFAULT_LANG);
                game.ffgDict.setCurrentLanguage(game.currentLang);
            }

            StringI18n valueOut;
             
            if (game.ffgDict.tryGetValue(elements[0],out valueOut))
            {
                return valueOut.getCurrentOrDefaultLanguageString();
            }
            else
            {
                return key;
            }            
        }
        catch(System.Exception e)
        {
            Debug.Log("Warning: Unable to process imported Localization file. Exception:" + e.Message + System.Environment.NewLine);
        }
        return key;
    }
}

// Class to store data read from ini
public class IniData
{
    // Dict of Dict to hold all data
    public Dictionary<string, Dictionary<string, string>> data;

    public IniData()
    {
        data = new Dictionary<string, Dictionary<string, string>>();
    }

    // Add new data returns 0 on collision
    public bool Add(string name, Dictionary<string, string> dict)
    {
        if (data.ContainsKey(name))
            return false;
        data.Add(name, dict);
        return true;
    }

    // Add new data, appends to data or replaces if exists
    public void Add(string section, string name, string value)
    {
        if (!data.ContainsKey(section))
        {
            data.Add(section, new Dictionary<string, string>());
        }

        if (data[section].ContainsKey(name))
        {
            data[section].Remove(name);
        }
        data[section].Add(name, value);
    }

    // Remove an item by section and name
    public void Remove(string section, string name)
    {
        if (!data.ContainsKey(section)) return;

        if (!data[section].ContainsKey(name)) return;

        data[section].Remove(name);
    }

    // Get section data, returns null if not found
    public Dictionary<string, string> Get(string section)
    {
        if (!data.ContainsKey(section))
            return null;
        return data[section];
    }

    // Get string by section and item, "" if not found
    public string Get(string section, string item)
    {
        if (!data.ContainsKey(section))
            return "";
        if (!data[section].ContainsKey(item))
            return "";
        return data[section][item];
    }

    // output the ini data back to a string
    override public string ToString()
    {
        string nl = System.Environment.NewLine;
        string r = "";
        foreach (KeyValuePair<string, Dictionary<string, string>> kv in data)
        {
            r += "[" + kv.Key + "]" + nl;
            foreach (KeyValuePair<string, string> kv2 in kv.Value)
            {
                r += kv2.Key;
                if (kv2.Value.Length > 0)
                {
                    r += "=" + kv2.Value;
                }
                r += nl;
            }
            r += nl;
        }
        return r;
    }
}
