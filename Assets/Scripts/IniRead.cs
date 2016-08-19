using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class IniRead{
    public static IniData ReadFromIni(string path)
    {
        string[] lines = System.IO.File.ReadAllLines(path);
        Dictionary<string, string> entryData = new Dictionary<string, string>();
        string entryName = "";
        IniData output = new IniData();
        char[] charsToTrim = { '[', ']' };

        foreach (string l in lines)
        {
            if (l.Length > 0 && l.Trim()[0] == '[')
            {
                if (entryName != "")
                {
                    output.Add(entryName, entryData);
                }
                entryData = new Dictionary<string, string>();
                entryName = l.Trim().Trim(charsToTrim);
            }
            else if (l.Length > 0 && l.Trim()[0] != ';')
            {
                int equalsLocation = l.IndexOf('=');
                if (equalsLocation == -1)
                    entryData.Add(l.Trim(), "");
                else
                    entryData.Add(l.Substring(0, equalsLocation).Trim(), l.Substring(equalsLocation + 1).Trim().Trim('\"'));
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
