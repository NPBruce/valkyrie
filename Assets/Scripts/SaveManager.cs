using UnityEngine;
using System.Collections.Generic;
using System.IO;

class SaveManager
{
    public static string SaveFile()
    {
        Game game = Game.Get();
        return System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie/Save/save" + game.gameType.GetType() + ".ini";
    }

    public static void Save()
    {
        try
        {
            if (!Directory.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie"))
            {
                Directory.CreateDirectory(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie");
            }
            if (!Directory.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie/Save"))
            {
                Directory.CreateDirectory(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie/Save");
            }
            File.WriteAllText(SaveFile(), Game.Get().quest.ToString());
        }
        catch (System.Exception)
        {
            Debug.Log("Warning: Unable to write to save file.");
        }
    }

    public static bool SaveExists()
    {
        return File.Exists(SaveFile());
    }

    public static void Load()
    {
        try
        {
            if (File.Exists(SaveFile()))
            {
                string data = File.ReadAllText(SaveFile());
                new Quest(data);
            }
        }
        catch (System.Exception)
        {
            Debug.Log("Error: Unable to open save file: " + SaveFile());
            Application.Quit();
        }
    }
}
