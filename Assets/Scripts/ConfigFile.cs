using System.IO;
using UnityEngine;
using System.Collections.Generic;

public class ConfigFile
{
    public IniData data;

    public ConfigFile()
    {
        data = new IniData();
        string optionsFile = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie/config.ini";
        if (File.Exists(optionsFile))
        {
            data = IniRead.ReadFromIni(optionsFile);
        }
    }

    public void Save()
    {
        string optionsFile = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie/config.ini";
        string content = data.ToString();
        try
        {
            if (!Directory.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie"))
            {
                Directory.CreateDirectory(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie");
            }
            File.WriteAllText(optionsFile, content);
        }
        catch (System.Exception)
        {
            Debug.Log("Warning: Unable to write to config file: " + optionsFile + System.Environment.NewLine);
        }
    }
}