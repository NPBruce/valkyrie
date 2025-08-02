using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using ValkyrieTools;

// This class provides functions to load and save games.
class SaveManager
{
    public static string minValkyieVersion = "0.7.3";

    // This gets the path to the save game file.  Only one file is used/supported per game type.
    public static string SaveFile(int num = 0)
    {
        string number = num.ToString();
        if (num == 0) number = "Auto";
        return Path.Combine(ContentData.GameTypePath, "Save") + Path.DirectorySeparatorChar + "save" + number + ".vSave";
    }

    // This saves the current game to disk.  Will overwrite any previous saves
    public static void Save(int num = 0, bool quit = false)
    {
        // wait to not overwrite save.ini and screen capture
        ZipManager.Wait4PreviousSave();
        Game.Get().cc.TakeScreenshot(delegate { SaveWithScreen(num, quit); });
    }

    private static void SaveWithScreen(int num, bool quit = false)
    {
        Game game = Game.Get();
        try
        {
            if (!Directory.Exists(Game.AppData()))
            {
                Directory.CreateDirectory(Game.AppData());
            }
            if (!Directory.Exists(ContentData.GameTypePath))
            {
                Directory.CreateDirectory(ContentData.GameTypePath);
            }
            if (!Directory.Exists(Path.Combine(ContentData.GameTypePath, "Save")))
            {
                Directory.CreateDirectory(Path.Combine(ContentData.GameTypePath, "Save"));
            }
            string tempValkyriePath = ContentData.TempValkyriePath;
            if (!Directory.Exists(tempValkyriePath))
            {
                Directory.CreateDirectory(tempValkyriePath);
            }
            File.WriteAllText(Path.Combine(tempValkyriePath, "save.ini"), game.CurrentQuest.ToString());

            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            float scale = 4f / 30f;

            var targetSizeX = Mathf.RoundToInt(screenSize.x * scale);
            var targetSizeY = Mathf.RoundToInt(screenSize.y * scale);
            var outTex = ResizeScreenShotTexture(game.cc.screenShot, targetSizeX, targetSizeY);
            
            File.WriteAllBytes(Path.Combine(tempValkyriePath, "image.png"), outTex.EncodeToPNG());

            // Check if we should update the zip file or write a new one with quest content
            // first autosave is a new zip file, following autosave just update the zip
            bool zip_update = false;
            if (num==0 && game.CurrentQuest.firstAutoSaveDone)
            {
                zip_update = true;
            }
            else if (num == 0)
            {
                game.CurrentQuest.firstAutoSaveDone = true;
            }

            // Quest content can be in original path, or savegame path
            string quest_content_path;
            if(game.CurrentQuest.fromSavegame)
            {
                quest_content_path = ContentData.ValkyrieLoadQuestPath;
            }
            else
            {
                quest_content_path = game.CurrentQuest.originalPath;
            }

            // zip in a separate thread
            ZipManager.WriteZipAsync(tempValkyriePath, quest_content_path, SaveFile(num), zip_update);
        }
        catch (IOException e)
        {
            ValkyrieDebug.Log("Warning: Unable to write to save file. " + e.Message);
        }
        if (quit)
        {
            ZipManager.Wait4PreviousSave();
            GameStateManager.MainMenu();
        }
    }

    private static Texture2D ResizeScreenShotTexture(Texture2D source, int targetSizeX, int targetSizeY)
    {
        var currentTarget = RenderTexture.active;
        Texture2D result = new Texture2D(targetSizeX,targetSizeY);
        try
        {
            RenderTexture helper = new RenderTexture(targetSizeX, targetSizeY, 24);
            RenderTexture.active = helper;
            Graphics.Blit(source, helper);
            result.ReadPixels(new Rect(0, 0, targetSizeX, targetSizeY), 0, 0);
            result.Apply();
        }
        catch (Exception e)
        {
            ValkyrieDebug.Log($"Failed to resize screenshot: {e.Message}");
            ValkyrieDebug.Log(e.StackTrace);
            
        }
        finally
        {
            RenderTexture.active = currentTarget;
        }
        return result;
    }

    // Check if a save game exists for the current game type
    public static bool SaveExists()
    {
        for (int i = 0; i < 4; i++)
        {
            if (File.Exists(SaveFile(i)))
            {
                return true;
            }
        }
        return false;
    }

    // Load a saved game, does nothing if file does not exist
    public static void Load(int num = 0)
    {
        Game game = Game.Get();
        try
        {
            if (File.Exists(SaveFile(num)))
            {
                if (!Directory.Exists(ContentData.TempValkyriePath))
                {
                    Directory.CreateDirectory(ContentData.TempValkyriePath);
                }
                string valkyrieLoadPath = Path.Combine(ContentData.TempValkyriePath, "Load");

                if (!Directory.Exists(valkyrieLoadPath))
                {
                    Directory.CreateDirectory(valkyrieLoadPath);
                }

                Directory.Delete(valkyrieLoadPath, true);
                ZipManager.Extract(valkyrieLoadPath, SaveFile(num), ZipManager.Extract_mode.ZIPMANAGER_EXTRACT_FULL);

                string savefile_content = File.ReadAllText(Path.Combine(valkyrieLoadPath, "save.ini"));
                IniData saveData = IniRead.ReadFromString(savefile_content);

                // when loading a quest, path should always be $TMP/load/quest/$subquest/quest.ini
                // Make sure it is when loading a quest saved for the first time, as in that case it is the original load path
                string questLoadPath = Path.GetDirectoryName(saveData.Get("Quest", "path"));
                string questOriginalPath = saveData.Get("Quest", "originalpath");

                // loading a quest saved for the first time
                if (questLoadPath.Contains(questOriginalPath))
                {
                    questLoadPath = questLoadPath.Replace(questOriginalPath, ContentData.ValkyrieLoadQuestPath);
                }
                
                // Check that quest in save is valid
                QuestData.Quest q = new QuestData.Quest(questLoadPath);
                if (!q.valid)
                {
                    ValkyrieDebug.Log("Error: save contains unsupported quest version." + Environment.NewLine);
                    GameStateManager.MainMenu();
                    return;
                }
                saveData.data["Quest"]["path"] = Path.Combine(questLoadPath, "quest.ini");

                if (VersionManager.VersionNewer(game.version, saveData.Get("Quest", "valkyrie")))
                {
                    ValkyrieDebug.Log("Error: save is from a future version." + Environment.NewLine);
                    GameStateManager.MainMenu();
                    return;
                }

                if (!VersionManager.VersionNewerOrEqual(minValkyieVersion, saveData.Get("Quest", "valkyrie")))
                {
                    ValkyrieDebug.Log("Error: save is from an old unsupported version." + Environment.NewLine);
                    GameStateManager.MainMenu();
                    return;
                }

                Destroyer.Dialog();

                // Load content that the save game uses
                Dictionary<string, string> packs = saveData.Get("Packs");
                foreach (KeyValuePair<string, string> kv in packs)
                {
                    // Support for save games from 1.2 and older
                    if (kv.Key.Equals("FA"))
                    {
                        game.ContentLoader.LoadContentID("FAI");
                        game.ContentLoader.LoadContentID("FAM");
                        game.ContentLoader.LoadContentID("FAT");
                    }
                    if (kv.Key.Equals("CotW"))
                    {
                        game.ContentLoader.LoadContentID("CotWI");
                        game.ContentLoader.LoadContentID("CotWM");
                        game.ContentLoader.LoadContentID("CotWT");
                    }
                    if (kv.Key.Equals("MoM1E"))
                    {
                        game.ContentLoader.LoadContentID("MoM1EI");
                        game.ContentLoader.LoadContentID("MoM1EM");
                        game.ContentLoader.LoadContentID("MoM1ET");
                    }
                    else
                    {
                        game.ContentLoader.LoadContentID(kv.Key);
                    }
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
                new SkillButton();
                new InventoryButton();
                game.stageUI = new NextStageButton();
            }
        }
        catch (IOException e)
        {
            ValkyrieDebug.Log("Error: Unable to open save file: " + SaveFile(num) + " " + e.Message);
            Application.Quit();
        }
    }

    public static List<SaveData> GetSaves()
    {
        List<SaveData> saves = new List<SaveData>();
        for (int i = 0; i < 4; i++)
        {
            saves.Add(new SaveData(i));
        }
        return saves;
    }

    public class SaveData
    {
        public bool valid = false;
        public string quest_name;
        public DateTime saveTime;
        public Texture2D image;

        public SaveData(int num = 0)
        {
            Game game = Game.Get();
            if (!File.Exists(SaveFile(num))) return;
            try
            {
                if (!Directory.Exists(ContentData.TempValkyriePath))
                {
                    Directory.CreateDirectory(ContentData.TempValkyriePath);
                }
                string valkyrieLoadPath = Path.Combine(ContentData.TempValkyriePath, "Preload");
                if (!Directory.Exists(valkyrieLoadPath))
                {
                    Directory.CreateDirectory(valkyrieLoadPath);
                }

                ZipManager.Extract(valkyrieLoadPath, SaveFile(num), ZipManager.Extract_mode.ZIPMANAGER_EXTRACT_SAVE_INI_PIC);

                image = ContentData.FileToTexture(Path.Combine(valkyrieLoadPath, "image.png"));

                string data = File.ReadAllText(Path.Combine(valkyrieLoadPath, "save.ini"));
                IniData saveData = IniRead.ReadFromString(data);

                // when loading a quest, path should always be $TMP/load/quest/$subquest/quest.ini
                // Make sure it is when loading a quest saved for the first time, as in that case it is the original load path
                string questLoadPath = Path.GetDirectoryName(saveData.Get("Quest", "path"));
                string questOriginalPath = saveData.Get("Quest", "originalpath");

                // loading a quest saved for the first time
                if (questLoadPath.Contains(questOriginalPath))
                {
                    questLoadPath = questLoadPath.Replace(questOriginalPath, ContentData.ValkyrieLoadQuestPath);
                }

                // use preload path rather than load
                questLoadPath = questLoadPath.Replace(ContentData.ValkyrieLoadPath, ContentData.ValkyriePreloadPath);
                QuestData.Quest q = new QuestData.Quest(questLoadPath);
                if (!q.valid)
                {
                    ValkyrieDebug.Log("Warning: Save " + num + " contains unsupported quest version." + Environment.NewLine);
                    return;
                }

                quest_name = saveData.Get("Quest", "questname");

                if (VersionManager.VersionNewer(game.version, saveData.Get("Quest", "valkyrie")))
                {
                    ValkyrieDebug.Log("Warning: Save " + num + " is from a future version." + Environment.NewLine);
                    return;
                }

                if (!VersionManager.VersionNewerOrEqual(minValkyieVersion, saveData.Get("Quest", "valkyrie")))
                {
                    ValkyrieDebug.Log("Warning: Save " + num + " is from an old unsupported version." + Environment.NewLine);
                    return;
                }

                saveTime = DateTime.Parse(saveData.Get("Quest", "time"));

                valid = true;
            }
            catch (Exception e)
            {
                ValkyrieDebug.Log("Warning: Unable to open save file: " + SaveFile(num) + "\nException: " + e.ToString());
            }
        }
    }
}

