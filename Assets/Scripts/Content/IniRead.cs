using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

// Helper class to read an ini file into a nested dictionary
// This exists because .NET/Mono doesn't have one!!
public static class IniRead{
    /// <summary>
    /// Function takes path to ini file and returns data object
    /// </summary>
    /// <param name="path">path to ini file</param>
    /// <returns>Returns null on error</returns>
    public static IniData ReadFromIni(string path)
    {
        string[] lines;

        // Read the whole file
        try
        {
            lines = System.IO.File.ReadAllLines(path);
        }
        catch (System.Exception ex)
        {
            ValkyrieDebug.Log(ex.Message);
            return null;
        }
        // Parse text data
        return ReadFromStringArray(lines, path);
    }

    // Function takes path to ini file and section name
    // Returns section as string, string dictionary
    // Returns empty dictionary if not found
    public static Dictionary<string, string> ReadFromIni(string path, string section)
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
        return ReadFromStringArray(lines, path, section);
    }

    /// <summary>
    /// Function ini file contents as a string and returns data object 
    /// </summary>
    /// <param name="content">string to read</param>
    /// <returns>Returns null on error</returns>
    public static IniData ReadFromString(string content)
    {
        // split text into array of lines
        string[] lines = content.Split(new string[] { "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        return ReadFromStringArray(lines, "<INTERNAL>");
    }

    /// <summary>
    /// Parse ini data into data structure 
    /// </summary>
    /// <param name="lines">array of text lines</param>
    /// <param name="path">path from where lines came</param>
    /// <returns></returns>
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
                        ValkyrieDebug.Log("Warning: duplicate section \"" + entryName + "\" in " + path + " will be ignored.");
                    }
                }
                // create new data for new section
                entryData = new Dictionary<string, string>();
                // Get name of new section
                entryName = l.Trim().Trim(charsToTrim);
                // Blank section names not allowed, but not fatal
                if(entryName.Equals(""))
                {
                    ValkyrieDebug.Log("Warning: empty section in " + path + " will be ignored.");
                }
            }
            // If the line is not a comment (starts with ;)
            else if (l.Length > 0 && l.Trim()[0] != ';')
            {
                int equalsLocation = l.IndexOf('=');
                // Add data as entry with no value
                if (equalsLocation == -1)
                {
                    if (entryData.ContainsKey(l.Trim()))
                    {
                        ValkyrieDebug.Log("Warning: duplicate \"" + l.Trim() + "\" data in section \"" + entryName + "\" in " + path + " will be ignored.");
                    }
                    else
                    {
                        entryData.Add(l.Trim(), "");
                    }
                }
                // If there is an = add data as key and value
                else
                {
                    string key = l.Substring(0, equalsLocation).Trim();
                    if (entryData.ContainsKey(key))
                    {
                        ValkyrieDebug.Log("Warning: duplicate \"" + key + "\" data in section \"" + entryName + "\" in " + path + " will be ignored.");
                    }
                    else
                    {
                        string value = l.Substring(equalsLocation + 1).Trim().Trim('\"');
                        //string translatedValue = LocalizationRead.FFGLookup(value);
                        entryData.Add(key, value);
                    }
                }
                // This won't go anywhere if we don't have a section
                if (entryName.Equals(""))
                {
                    ValkyrieDebug.Log("Warning: data without section in " + path + " will be ignored.");
                }
            }
        }

        // Add the last section
        if (entryName != "")
        {
            if (!output.Add(entryName, entryData))
            {
                ValkyrieDebug.Log("Warning: duplicate section \"" + entryName + "\" in " + path + " will be ignored.");
            }
        }

        return output;
    }
    // Parse ini data into data structure
    public static Dictionary<string, string> ReadFromStringArray(string[] lines, string path, string section)
    {
        // Create a dictionary for the section
        Dictionary<string, string> entryData = new Dictionary<string, string>();

        bool found = false;
        bool end = false;
        int i = 0;
        string find = "[" + section + "]";
        while (!end && i < lines.Length)
        {
            if (found)
            {
                if (lines[i].IndexOf('[') == 0)
                {
                    end = true;
                }
                else
                {
                    // If the line is not a comment (starts with ;)
                    if (lines[i].Length > 0 && lines[i].Trim()[0] != ';')
                    {
                        int equalsLocation = lines[i].IndexOf('=');
                        // Add data as entry with no value
                        if (equalsLocation == -1)
                            entryData.Add(lines[i].Trim(), "");
                        // If there is an = add data as key and value
                        else
                        {
                            string key = lines[i].Substring(0, equalsLocation).Trim();
                            if (entryData.ContainsKey(key))
                            {
                                Debug.Log("Warning: duplicate \"" + key + "\" data in section \"" + section + "\" in " + path + " will be ignored.");
                            }
                            else
                            {
                                string value = lines[i].Substring(equalsLocation + 1).Trim().Trim('\"');
                                //string translatedValue = LocalizationRead.FFGLookup(value);
                                entryData.Add(key, value);
                            }
                        }
                    }
                }
            }
            if (lines[i].IndexOf(find) == 0)
            {
                found = true;
            }
            i++;
        }
        return entryData;
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


    // Remove a section by name
    public void Remove(string section)
    {
        if (!data.ContainsKey(section)) return;

        data.Remove(section);
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
