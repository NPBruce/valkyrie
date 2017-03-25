using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;

// This class provides functions to load and save games.
class SaveManager
{
    // This gets the path to the save game file.  Only one file is used/supported per game type.
    public static string SaveFile()
    {
        Game game = Game.Get();
        return System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie/Save/save" + game.gameType.TypeName() + ".vSave";
    }

    // This saves the current game to disk.  Will overwrite any previous saves
    public static void Save()
    {
        Game game = Game.Get();
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

            QuestLoader.CleanTemp();
            if (!Directory.Exists(Path.GetTempPath() + "/Valkyrie"))
            {
                Directory.CreateDirectory(Path.GetTempPath() + "/Valkyrie");
            }
            File.WriteAllText(Path.GetTempPath() + "/Valkyrie/save.ini", game.quest.ToString());
            ZipFile zip = new ZipFile();
            zip.AddFile(Path.GetTempPath() + "/Valkyrie/save.ini", "");
            zip.AddDirectory(Path.GetDirectoryName(game.quest.qd.questPath), "quest");
            zip.Save(SaveFile());
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
                if (!Directory.Exists(Path.GetTempPath() + "/Valkyrie"))
                {
                    Directory.CreateDirectory(Path.GetTempPath() + "/Valkyrie");
                }
                if (!Directory.Exists(Path.GetTempPath() + "/Valkyrie/Load"))
                {
                    Directory.CreateDirectory(Path.GetTempPath() + "/Valkyrie/Load");
                }

                ZipFile zip = ZipFile.Read(SaveFile());
                zip.ExtractAll(Path.GetTempPath() + "/Valkyrie/Load");

                string data = File.ReadAllText(Path.GetTempPath() + "/Valkyrie/Load/save.ini");

                IniData saveData = IniRead.ReadFromString(data);
                saveData.data["Quest"]["path"] = Path.GetTempPath() + "/Valkyrie/Load/quest/quest.ini";

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
