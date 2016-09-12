using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Helper class to read an ini file into a nested dictionary
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
        catch(System.Exception)
        {
            return null;
        }
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
    
    public static string FFGLookup(string input)
    {
        string output = input;
        while (output.IndexOf("{ffg:") != -1)
        {
            int bracketLevel = 1;
            int lookupStart = output.IndexOf("{ffg:") + "{ffg:".Length;

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

            string lookup = output.Substring(lookupStart, lookupEnd - lookupStart);
            string result = FFGQuery(lookup);
            // We don't support underlines
            result = result.Replace("[u]", "<b>").Replace("[/u]", "</b>");
            result = result.Replace("[i]", "<i>").Replace("[/i]", "</i>");
            result = result.Replace("[b]", "<b>").Replace("[/b]", "</b>");
            output = output.Replace("{ffg:" + lookup + "}", result);
        }
        return output;
    }

    public static string FFGQuery(string input)
    {
        string[] elements = input.Split(":".ToCharArray());

        string fetched = FFGKeyLookup(elements[0]);

        for (int i = 2; i < elements.Length; i += 2)
        {
            fetched = fetched.Replace(elements[i - 1], elements[i]);
        }
        return fetched;
    }

    public static string FFGKeyLookup(string key)
    {
        string[] elements = key.Split(":".ToCharArray());

        try
        {
            Game game = Game.Get();

            if (game.ffgText == null || game.ffgText.Length == 0)
            {
                game.ffgText = System.IO.File.ReadAllLines(game.gameType.DataDirectory() + "ffg/text/Localization.txt");
            }
            for (int i = 0; i < game.ffgText.Length; i++)
            {
                string[] values = game.ffgText[i].Split(",".ToCharArray(), 2);
                if (values.Length > 1 && values[0].Equals(elements[0]))
                {
                    string returnValue = values[1];
                    int nextQuote = 0;

                    while (true)
                    {
                        nextQuote = returnValue.IndexOf("\"", nextQuote + 1);
                        if (nextQuote == returnValue.Length - 1)
                        {
                            return returnValue.Replace("\"\"", "\"").Trim('\"');
                        }

                        if (returnValue[nextQuote + 1].Equals("\""))
                        {
                            nextQuote++;
                        }
                        else if (nextQuote == -1)
                        {
                            if (i >= game.ffgText.Length) return returnValue.Replace("\"\"", "\"").Trim('\"');
                            returnValue += System.Environment.NewLine + game.ffgText[++i];
                        }
                        else
                        {
                            return returnValue.Replace("\"\"", "\"").Trim('\"');
                        }
                        nextQuote = returnValue.IndexOf("\"", nextQuote + 1);
                    }

                }
            }
            return key;
        }
        catch(System.Exception)
        {
            Debug.Log("Warning: Unable to open imported Localization file." + System.Environment.NewLine);
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
}
