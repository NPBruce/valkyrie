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
            Debug.Log(e);
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
                    output.Add(entryName, entryData);
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
                    entryData.Add(l.Substring(0, equalsLocation).Trim(), l.Substring(equalsLocation + 1).Trim().Trim('\"'));
                // This won't go anywhere if we don't have a section
                if (entryName.Equals(""))
                {
                    Debug.Log("Warning: data without section in " + path + " will be ignored.");
                }
            }
        }

        if (entryName != "")
        {
            output.Add(entryName, entryData);
        }

        return output;
    }
}

public class IniData
{
    public Dictionary<string, Dictionary<string, string>> data;

    public IniData()
    {
        data = new Dictionary<string, Dictionary<string, string>>();
    }

    public void Add(string name, Dictionary<string, string> dict)
    {
        data.Add(name, dict);
    }

    public Dictionary<string, string> Get(string section)
    {
        return data[section];
    }

    public string Get(string section, string item)
    {
        return data[section][item];
    }
}
