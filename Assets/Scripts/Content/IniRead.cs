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
        catch(System.Exception e)
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
                        entryData.Add(key, l.Substring(equalsLocation + 1).Trim().Trim('\"'));
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
