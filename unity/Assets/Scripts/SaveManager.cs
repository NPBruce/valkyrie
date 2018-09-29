using UnityEngine;
using System.Collections.Generic;
using System.IO;
using ValkyrieTools;
using Assets.Scripts.Content;

// This class provides functions to load and save games.
class SaveManager
{
    public static string minValkyieVersion = "0.7.3";

    // This gets the path to the save game file.  Only one file is used/supported per game type.
    public static string SaveFile(int num = 0)
    {
        Game game = Game.Get();
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
            string tempValkyriePath = ContentData.TempValyriePath;
            if (!Directory.Exists(tempValkyriePath))
            {
                Directory.CreateDirectory(tempValkyriePath);
            }
            File.WriteAllText(Path.Combine(tempValkyriePath, "save.ini"), game.quest.ToString());

            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            Color[] screenColor = game.cc.screenShot.GetPixels(0);

            float scale = 4f / 30f;
            Texture2D outTex = new Texture2D(Mathf.RoundToInt(screenSize.x * scale), Mathf.RoundToInt(screenSize.y * scale), TextureFormat.RGB24, false);
 
            Color[] outColor = new Color[outTex.width * outTex.height];
 
            for(int i = 0; i < outColor.Length; i++)
            {
                float xX = (float)i % (float)outTex.width;
                float xY = Mathf.Floor((float)i / (float)outTex.width);
 
                Vector2 vCenter = new Vector2(xX, xY) / scale;

                int xXFrom = (int)Mathf.Max(Mathf.Floor(vCenter.x - (0.5f / scale)), 0);
                int xXTo = (int)Mathf.Min(Mathf.Ceil(vCenter.x + (0.5f / scale)), screenSize.x);
                int xYFrom = (int)Mathf.Max(Mathf.Floor(vCenter.y - (0.5f / scale)), 0);
                int xYTo = (int)Mathf.Min(Mathf.Ceil(vCenter.y + (0.5f / scale)), screenSize.y);
 
                Color oColorTemp = new Color();
                float xGridCount = 0;
                for(int iy = xYFrom; iy < xYTo; iy++)
                {
                    for(int ix = xXFrom; ix < xXTo; ix++)
                    {
                        int index = (int)(((float)iy * screenSize.x) + ix);
                        if (index >= screenColor.Length || index < 0)
                        {
                            continue;
                        }
                        oColorTemp += screenColor[index];
                        xGridCount++;
                    }
                }
                outColor[i] = oColorTemp / (float)xGridCount;
            }

            outTex.SetPixels(outColor);
            outTex.Apply();
            File.WriteAllBytes(Path.Combine(tempValkyriePath, "image.png"), outTex.EncodeToPNG());

            // Check if we should update the zip file or write a new one with quest content
            // first autosave is a new zip file, following autosave just update the zip
            bool zip_update = false;
            if (num==0 && game.quest.firstAutoSaveDone)
            {
                zip_update = true;
            }
            else
            {
                game.quest.firstAutoSaveDone = true;
            }

            // Quest content can be in original path, or savegame path
            string quest_content_path;
            if(game.quest.fromSavegame)
            {
                quest_content_path = ContentData.ValkyrieLoadQuestPath;
            }
            else
            {
                quest_content_path = game.quest.originalPath;
            }

            // zip in a separate thread
            ZipManager.WriteZipAsync(tempValkyriePath, quest_content_path, SaveFile(num), zip_update);
        }
        catch (System.IO.IOException e)
        {
            ValkyrieDebug.Log("Warning: Unable to write to save file. " + e.Message);
        }
        if (quit)
        {
            ZipManager.Wait4PreviousSave();
            Destroyer.MainMenu();
        }
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
                if (!Directory.Exists(ContentData.TempValyriePath))
                {
                    Directory.CreateDirectory(ContentData.TempValyriePath);
                }
                string valkyrieLoadPath = Path.Combine(ContentData.TempValyriePath, "Load");

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
                    ValkyrieDebug.Log("Error: save contains unsupported quest version." + System.Environment.NewLine);
                    Destroyer.MainMenu();
                    return;
                }
                saveData.data["Quest"]["path"] = Path.Combine(questLoadPath, "quest.ini");

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

                    // Support for save games from 1.2 and older
                    if (kv.Key.Equals("FA"))
                    {
                        game.cd.LoadContentID("FAI");
                        game.cd.LoadContentID("FAM");
                        game.cd.LoadContentID("FAT");
                    }
                    if (kv.Key.Equals("CotW"))
                    {
                        game.cd.LoadContentID("CotWI");
                        game.cd.LoadContentID("CotWM");
                        game.cd.LoadContentID("CotWT");
                    }
                    if (kv.Key.Equals("MoM1E"))
                    {
                        game.cd.LoadContentID("MoM1EI");
                        game.cd.LoadContentID("MoM1EM");
                        game.cd.LoadContentID("MoM1ET");
                    }
                    else
                    {
                        game.cd.LoadContentID(kv.Key);
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
        public System.DateTime saveTime;
        public Texture2D image;

        public SaveData(int num = 0)
        {
            Game game = Game.Get();
            if (!File.Exists(SaveFile(num))) return;
            try
            {
                if (!Directory.Exists(ContentData.TempValyriePath))
                {
                    Directory.CreateDirectory(ContentData.TempValyriePath);
                }
                string valkyrieLoadPath = Path.Combine(ContentData.TempValyriePath, "Preload");
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
                    ValkyrieDebug.Log("Warning: Save " + num + " contains unsupported quest version." + System.Environment.NewLine);
                    return;
                }

                // Translate quest name
                LocalizationRead.AddDictionary("qst", q.localizationDict);
                quest_name = q.name.Translate();

                if (VersionNewer(game.version, saveData.Get("Quest", "valkyrie")))
                {
                    ValkyrieDebug.Log("Warning: Save " + num + " is from a future version." + System.Environment.NewLine);
                    return;
                }

                if (!VersionNewerOrEqual(minValkyieVersion, saveData.Get("Quest", "valkyrie")))
                {
                    ValkyrieDebug.Log("Warning: Save " + num + " is from an old unsupported version." + System.Environment.NewLine);
                    return;
                }

                saveTime = System.DateTime.Parse(saveData.Get("Quest", "time"));

                valid = true;
            }
            catch (System.Exception e)
            {
                ValkyrieDebug.Log("Warning: Unable to open save file: " + SaveFile(num) + "\nException: " + e.ToString());
            }
        }
    }
}

