using UnityEngine;
using System.Collections.Generic;
using System.IO;

class SaveManager
{
    public static string SaveFile()
    {
        Game game = Game.Get();
        return System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie/Save/save" + game.gameType.TypeName() + ".ini";
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
        Game game = Game.Get();
        try
        {
            if (File.Exists(SaveFile()))
            {
                string data = File.ReadAllText(SaveFile());

                IniData saveData = IniRead.ReadFromString(data);

                Destroyer.Dialog();

                game.cd = new ContentData(game.gameType.DataDirectory());
                // Check if we found anything
                if (game.cd.GetPacks().Count == 0)
                {
                    Debug.Log("Error: Failed to find any content packs, please check that you have them present in: " + game.gameType.DataDirectory() + System.Environment.NewLine);
                    Application.Quit();
                }

                game.cd.LoadContentID("");
                Dictionary<string, string> packs = saveData.Get("Packs");
                foreach (KeyValuePair<string, string> kv in packs)
                {
                    game.cd.LoadContentID("");
                    game.cd.LoadContentID(kv.Key);
                }

                new Quest(saveData);
                game.heroCanvas.SetupUI();
                game.heroCanvas.UpdateImages();
                game.heroCanvas.UpdateStatus();

                if (game.gameType.DisplayMorale())
                {
                    game.moraleDisplay = new MoraleDisplay();
                }
                if (!game.gameType.DisplayHeroes())
                {
                    game.heroCanvas.Clean();
                }

                // Create the menu button
                new MenuButton();
                new NextStageButton();
            }
        }
        catch (System.Exception)
        {
            Debug.Log("Error: Unable to open save file: " + SaveFile());
            Application.Quit();
        }
    }
}
