using UnityEngine;
using System.Collections.Generic;
using System.IO;

// This class provides functions to load and save games.
class SaveManager
{
    // This gets the path to the save game file.  Only one file is used/supported per game type.
    public static string SaveFile()
    {
        Game game = Game.Get();
        return System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie/Save/save" + game.gameType.TypeName() + ".ini";
    }

    // This saves the current game to disk.  Will overwrite any previous saves
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
            ValkyrieDebug.Log("Warning: Unable to write to save file.");
        }
    }

    // Check if a save game exists for the current game type
    public static bool SaveExists()
    {
        return File.Exists(SaveFile());
    }

    // Load a saved game, does nothing if file does not exist
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

                // Restart contend data so we can select from save
                game.cd = new ContentData(game.gameType.DataDirectory());
                // Check if we found anything
                if (game.cd.GetPacks().Count == 0)
                {
                    ValkyrieDebug.Log("Error: Failed to find any content packs, please check that you have them present in: " + game.gameType.DataDirectory() + System.Environment.NewLine);
                    Application.Quit();
                }

                game.cd.LoadContentID("");
                // Load the base content

                // Load content that the save game uses
                Dictionary<string, string> packs = saveData.Get("Packs");
                foreach (KeyValuePair<string, string> kv in packs)
                {
                    game.cd.LoadContentID("");
                    game.cd.LoadContentID(kv.Key);
                }

                // This loads the game
                new Quest(saveData);

                // Draw things on the screen
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
                game.stageUI = new NextStageButton();
            }
        }
        catch (System.Exception)
        {
            ValkyrieDebug.Log("Error: Unable to open save file: " + SaveFile());
            Application.Quit();
        }
    }
}
