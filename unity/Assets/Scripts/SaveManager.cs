using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;
using ValkyrieTools;

// This class provides functions to load and save games.
class SaveManager
{
    public static string minValkyieVersion = "0.7.3";

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
        catch (System.Exception e)
        {
            ValkyrieDebug.Log("Warning: Unable to write to save file. " + e.Message);
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

                Directory.Delete(Path.GetTempPath() + "/Valkyrie/Load", true);
                ZipFile zip = ZipFile.Read(SaveFile());
                zip.ExtractAll(Path.GetTempPath() + "/Valkyrie/Load");
                zip.Dispose();

                // Check that quest in save is valid
                QuestData.Quest q = new QuestData.Quest(Path.GetTempPath() + "/Valkyrie/Load/quest");
                if (!q.valid)
                {
                    ValkyrieDebug.Log("Error: save contains unsupported quest version." + System.Environment.NewLine);
                    Destroyer.MainMenu();
                    return;
                }

                string data = File.ReadAllText(Path.GetTempPath() + "/Valkyrie/Load/save.ini");

                IniData saveData = IniRead.ReadFromString(data);

                saveData.data["Quest"]["path"] = Path.GetTempPath() + "/Valkyrie/Load/quest/quest.ini";

                saveData.Get("Quest","valkyrie");

                if (VersionNewer(game.version, saveData.Get("Quest", "valkyrie")))
                {
                    ValkyrieDebug.Log("Error: save is from a future version." + System.Environment.NewLine);
                    Destroyer.MainMenu();
                    return;
                }

                if (!VersionNewerOrEqual(minValkyieVersion, saveData.Get("Quest", "valkyrie")))
                {
                    ValkyrieDebug.Log("Error: save is from an old unsupported version." + System.Environment.NewLine);
                    Destroyer.MainMenu();
                    return;
                }

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
                new LogButton();
                game.stageUI = new NextStageButton();
            }
        }
        catch (System.Exception e)
        {
            ValkyrieDebug.Log("Error: Unable to open save file: " + SaveFile() + " " + e.Message);
            Application.Quit();
        }
    }

    // Test version of the form a.b.c is newer or equal
    public static bool VersionNewerOrEqual(string oldVersion, string newVersion)
    {
        string oldS = System.Text.RegularExpressions.Regex.Replace(oldVersion, "[^0-9]", "");
        string newS = System.Text.RegularExpressions.Regex.Replace(newVersion, "[^0-9]", "");
        // If numbers are the same they are equal
        if (oldS.Equals(newS)) return true;
        return VersionNewer(oldVersion, newVersion);
    }

    // Test version of the form a.b.c is newer
    public static bool VersionNewer(string oldVersion, string newVersion)
    {
        // Split into components
        string[] oldV = oldVersion.Split('.');
        string[] newV = newVersion.Split('.');

        if (newVersion.Equals("")) return false;

        if (oldVersion.Equals("")) return true;

        // Different number of components
        if (oldV.Length != newV.Length)
        {
            return true;
        }
        // Check each component
        for (int i = 0; i < oldV.Length; i++)
        {
            // Strip for only numbers
            string oldS = System.Text.RegularExpressions.Regex.Replace(oldV[i], "[^0-9]", "");
            string newS = System.Text.RegularExpressions.Regex.Replace(newV[i], "[^0-9]", "");
            try
            {
                if (int.Parse(oldS) < int.Parse(newS))
                {
                    return true;
                }
                if (int.Parse(oldS) > int.Parse(newS))
                {
                    return false;
                }
            }
            catch (System.Exception)
            {
                return true;
            }
        }
        return false;
    }
}

