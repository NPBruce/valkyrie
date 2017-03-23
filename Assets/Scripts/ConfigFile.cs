using System.IO;

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
        string optionsFile = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie/config.ini";
        if (File.Exists(optionsFile))
        {
            data = IniRead.ReadFromIni(optionsFile);
        }
    }

    // Save the configuration in memory to disk
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
            ValkyrieDebug.Log("Warning: Unable to write to config file: " + optionsFile + System.Environment.NewLine);
        }
    }
}