using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;
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
        return Game.AppData() + "/" + game.gameType.TypeName() + "/Save/save" + number + ".vSave";
    }

    // This saves the current game to disk.  Will overwrite any previous saves
    public static void Save(int num = 0, bool quit = false)
    {
        Game.Get().cc.TakeScreenshot(delegate { SaveWithScreen(num, quit); });
    }

    public static void SaveWithScreen(int num, bool quit = false)
    {
        Game game = Game.Get();
        try
        {
            if (!Directory.Exists(Game.AppData()))
            {
                Directory.CreateDirectory(Game.AppData());
            }
            if (!Directory.Exists(Game.AppData() + "/" + game.gameType.TypeName()))
            {
                Directory.CreateDirectory(Game.AppData() + "/" + game.gameType.TypeName());
            }
            if (!Directory.Exists(Game.AppData() + "/" + game.gameType.TypeName() + "/Save"))
            {
                Directory.CreateDirectory(Game.AppData() + "/" + game.gameType.TypeName() + "/Save");
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

            ZipFile zip = new ZipFile();
            zip.AddFile(Path.Combine(tempValkyriePath, "save.ini"), "");
            zip.AddFile(Path.Combine(tempValkyriePath, "image.png"), "");
            zip.AddDirectory(Path.GetDirectoryName(game.quest.qd.questPath), "quest");
            zip.Save(SaveFile(num));
        }
        catch (System.IO.IOException e)
        {
            ValkyrieDebug.Log("Warning: Unable to write to save file. " + e.Message);
        }
        if (quit)
        {
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
                ZipFile zip = ZipFile.Read(SaveFile(num));
                zip.ExtractAll(valkyrieLoadPath);
                zip.Dispose();

                // Check that quest in save is valid
                QuestData.Quest q = new QuestData.Quest(Path.Combine(valkyrieLoadPath, "quest"));
                if (!q.valid)
                {
                    ValkyrieDebug.Log("Error: save contains unsupported quest version." + System.Environment.NewLine);
                    Destroyer.MainMenu();
                    return;
                }

                string data = File.ReadAllText(Path.Combine(valkyrieLoadPath, "save.ini"));

                IniData saveData = IniRead.ReadFromString(data);

                saveData.data["Quest"]["path"] = Path.Combine(valkyrieLoadPath, "quest" + Path.DirectorySeparatorChar + "quest.ini");

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
        public string quest;
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
                string valkyrieLoadPath = Path.Combine(ContentData.TempValyriePath, "Load");
                if (!Directory.Exists(valkyrieLoadPath))
                {
                    Directory.CreateDirectory(valkyrieLoadPath);
                }

                Directory.Delete(valkyrieLoadPath, true);
                ZipFile zip = ZipFile.Read(SaveFile(num));
                zip.ExtractAll(valkyrieLoadPath);
                zip.Dispose();

                image = ContentData.FileToTexture(Path.Combine(valkyrieLoadPath, "image.png"));

                // Check that quest in save is valid
                QuestData.Quest q = new QuestData.Quest(Path.Combine(valkyrieLoadPath, "quest"));
                if (!q.valid)
                {
                    ValkyrieDebug.Log("Warning: Save " + num + " contains unsupported quest version." + System.Environment.NewLine);
                    return;
                }

                DictionaryI18n tmpDict = LocalizationRead.selectDictionary("qst");
                LocalizationRead.AddDictionary("qst", q.localizationDict);
                quest = q.name.Translate();
                LocalizationRead.AddDictionary("qst", tmpDict);

                string data = File.ReadAllText(Path.Combine(valkyrieLoadPath, "save.ini"));

                IniData saveData = IniRead.ReadFromString(data);

                saveData.Get("Quest", "valkyrie");

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
                ValkyrieDebug.Log("Warning: Unable to open save file: " + SaveFile(num) + " " + e.Message);
            }
        }
    }
}

