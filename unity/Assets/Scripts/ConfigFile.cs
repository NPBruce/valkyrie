using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Content;
using ValkyrieTools;

// Class to control user configuration of Valkyrie
// This is a generic system and has no knowledge of the actual configuration structure
public class ConfigFile
{
    // This is the configuration data structure, it is public for read/write
    // Save must be called if modifications are to be kept
    public IniData data;

    // Initialise data from the file on disk
    public ConfigFile()
    {
        data = new IniData();
        string optionsFile = Game.AppData() + "/config.ini";
        if (File.Exists(optionsFile))
        {
            data = IniRead.ReadFromIni(optionsFile);
        }
    }

    public IEnumerable<string> GetPacks(string gameType)
    {
        return data.Get(gameType + "Packs")?.Keys ?? Enumerable.Empty<string>();
    }
    public Dictionary<string, string> GetPackLanguages(string gameType)
    {
        return data.Get(gameType + "Packs") ?? new Dictionary<string, string>();
    }

    public void RemovePack(string gameType, string pack)
    {
        data.Remove(gameType + "Packs", pack);
    }
    
    public void AddPack(string gameType, string pack, string language = "")
    {
        data.Add(gameType + "Packs", pack, language);
    }
    
    // Save the configuration in memory to disk
    public void Save()
    {
        string optionsFile = Game.AppData() + "/config.ini";
        string content = data.ToString();
        try
        {
            if (!Directory.Exists(Game.AppData()))
            {
                Directory.CreateDirectory(Game.AppData());
            }
            File.WriteAllText(optionsFile, content);
        }
        catch (System.Exception)
        {
            ValkyrieDebug.Log("Warning: Unable to write to config file: " + optionsFile + System.Environment.NewLine);
        }
    }
}