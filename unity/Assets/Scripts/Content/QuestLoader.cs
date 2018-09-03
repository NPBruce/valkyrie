using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;
using ValkyrieTools;

// Class for getting lists of quest with details
// TODO: work out why this is so slow
public class QuestLoader {

    // Return a dictionary of all available quests
    public static Dictionary<string, QuestData.Quest> GetQuests(bool getHidden = false)
    {
        Dictionary<string, QuestData.Quest> quests = new Dictionary<string, QuestData.Quest>();

        Game game = Game.Get();
        // Look in the user application data directory
        string dataLocation = Game.AppData();
        mkDir(dataLocation);
        CleanTemp();

        // Get a list of downloaded quest not packed
        List<string> questDirectories = GetQuests(ContentData.DownloadPath());

        // Extract only required files from downloaded packages 
        ExtractPackages(ContentData.DownloadPath());

        // Get the list of extracted packages
        questDirectories.AddRange(GetQuests(ContentData.TempValyriePath));

        // Add the list of editor quest
        if (game.gameType is MoMGameType)
        {
            dataLocation += "/MoM/Editor";
        }
        if (game.gameType is D2EGameType)
        {
            dataLocation += "/D2E/Editor";
        }
        if (game.gameType is IAGameType)
        {
            dataLocation += "/IA/Editor";
        }
        questDirectories.AddRange(GetQuests(dataLocation));
        
        // Go through all directories
        foreach (string p in questDirectories)
        {
            // load quest
            QuestData.Quest q = new QuestData.Quest(p);
            // Check quest is valid and of the right type
            if (q.valid && q.type.Equals(game.gameType.TypeName()))
            {
                // Is the quest hidden?
                if (!q.hidden || getHidden)
                {
                    // Add quest to quest list
                    quests.Add(p, q);
                }
            }
        }

        // Return list of available quests
        return quests;
    }

    // Return list of quests available in the user path (includes packages)
    public static Dictionary<string, QuestData.Quest> GetUserQuests()
    {
        Dictionary<string, QuestData.Quest> quests = new Dictionary<string, QuestData.Quest>();

        // Clean up extracted packages
        CleanTemp();

        // Read user application data for quests
        string dataLocation = Game.AppData();
        mkDir(dataLocation);
        List<string> questDirectories = GetQuests(dataLocation);

        // Read extracted packages
        questDirectories.AddRange(GetQuests(ContentData.TempValyriePath));

        // go through all found quests
        foreach (string p in questDirectories)
        {
            // read quest
            QuestData.Quest q = new QuestData.Quest(p);
            // Check if valid and correct type
            if (q.valid && q.type.Equals(Game.Get().gameType.TypeName()))
            {
                quests.Add(p, q);
            }
        }

        return quests;
    }

    // Return list of quests available in the user path unpackaged (editable)
    public static Dictionary<string, QuestData.Quest> GetUserUnpackedQuests()
    {
        var quests = new Dictionary<string, QuestData.Quest>();

        // Read user application data for quests
        string dataLocation = Game.AppData();
        mkDir(dataLocation);
        List<string> questDirectories = GetQuests(dataLocation);

        string tempPath = ContentData.TempPath;
        string gameType = Game.Get().gameType.TypeName();
        // go through all found quests
        foreach (string p in questDirectories)
        {
            // Android stores the temp path in the data dir, we don't want the extracted scenarios from there
            if (p.StartsWith(tempPath, System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            // read quest
            var q = new QuestData.Quest(p);
            // Check if valid and correct type
            if (q.valid && q.type.Equals(gameType))
            {
                quests.Add(p, q);
            }
        }

        return quests;
    }

    // Get list of directories with quests at a path
    public static List<string> GetQuests(string path)
    {
        List<string> quests = new List<string>();

        if (!Directory.Exists(path))
        {
            return quests;
        }

        // Get all directories at path
        List<string> questDirectories = DirList(path);
        foreach (string p in questDirectories)
        {
            // All packs must have a quest.ini, otherwise ignore
            if (File.Exists(p + "/quest.ini"))
            {
                    quests.Add(p);
            }
        }

        return quests;
    }

    /// <summary>
    /// Fully extract one single package, before starting a quest, and save package filename
    /// </summary>
    /// <param name="path">path of the file to extract</param>
    public static void ExtractFullPackage(string path)
    {
        // Extract into temp
        string tempValkyriePath = ContentData.TempValyriePath;
        mkDir(tempValkyriePath);
        string extractedPath = Path.Combine(tempValkyriePath, Path.GetFileName(path));
        if (!Directory.Exists(extractedPath))
        {
            // should not be possible, just in case
            mkDir(extractedPath);
        }

        try
        {
            ZipFile zip = ZipFile.Read(path);
            zip.ExtractAll(extractedPath, ExtractExistingFileAction.OverwriteSilently);
            zip.Dispose();
        }
        catch (System.Exception)
        {
            ValkyrieDebug.Log("Warning: Unable to read file: " + extractedPath);
        }

        // remember each package name, and use it when loading a quest
        string nl = System.Environment.NewLine;
        System.IO.File.AppendAllText(extractedPath + Path.DirectorySeparatorChar + "quest.ini",
                                     nl + "[Package]" + nl + "filename=" + Path.GetFileName(path) + nl,
                                     System.Text.Encoding.UTF8);
    }

    /// <summary>
    /// Partial extract of all package in a directory, to list quests,  and save package filename
    /// </summary>
    /// <param name="path">path of the directory containing .valkyrie package</param>
    public static void ExtractPackages(string path)
    {
        // Find all packages at path
        string[] archives = Directory.GetFiles(path, "*.valkyrie", SearchOption.AllDirectories);
        // Extract all packages
        foreach (string f in archives)
        {
            // Extract into temp
            string tempValkyriePath = ContentData.TempValyriePath;
            mkDir(tempValkyriePath);
            string extractedPath = Path.Combine(tempValkyriePath, Path.GetFileName(f));
            if (Directory.Exists(extractedPath))
            {
                try
                {
                    Directory.Delete(extractedPath, true);
                }
                catch (System.Exception)
                {
                    ValkyrieDebug.Log("Warning: Unable to remove old temporary files: " + extractedPath);
                }
            }
            mkDir(extractedPath);

            try
            {
                ZipFile zip = ZipFile.Read(f);
                zip.ExtractSelectedEntries("name = quest.ini", null, extractedPath, ExtractExistingFileAction.OverwriteSilently);
                zip.ExtractSelectedEntries("name = Localization.*.txt", null, extractedPath, ExtractExistingFileAction.OverwriteSilently);
                Dictionary<string, string> iniData = IniRead.ReadFromIni(extractedPath + "/quest.ini", "Quest");
                if (iniData.ContainsKey("image"))
                    zip.ExtractSelectedEntries("name = " + iniData["image"], null, extractedPath, ExtractExistingFileAction.OverwriteSilently);
                zip.Dispose();
            }
            catch (System.Exception)
            {
                ValkyrieDebug.Log("Warning: Unable to read file: " + extractedPath);
            }

            // remember each package name, and use it when loading a quest
            string nl = System.Environment.NewLine;
            System.IO.File.AppendAllText(extractedPath + Path.DirectorySeparatorChar + "quest.ini",
                                        nl + "[Package]" + nl + "filename="+Path.GetFileName(f) + nl,
                                         System.Text.Encoding.UTF8);
        }
    }

    // Attempt to create a directory
    public static void mkDir(string p)
    {
        if (!Directory.Exists(p))
        {
            try
            {
                Directory.CreateDirectory(p);
            }
            catch (System.Exception)
            {
                ValkyrieDebug.Log("Error: Unable to create directory: " + p);
                Application.Quit();
            }
        }
    }

    // Return a list of directories at a path (recursive)
    public static List<string> DirList(string path)
    {
        return DirList(path, new List<string>());
    }

    // Add to list of directories at a path (recursive)
    public static List<string> DirList(string path, List<string> l)
    {
        List<string> list = new List<string>(l);

        foreach (string s in Directory.GetDirectories(path))
        {
            list = DirList(s, list);
            list.Add(s);
        }

        return list;
    }

    public static void CleanTemp()
    {
        // Nothing to do if no temporary files
        string tempValkyriePath = ContentData.TempValyriePath;
        if (!Directory.Exists(tempValkyriePath))
        {
            return;
        }

        try
        {
            Directory.Delete(tempValkyriePath, true);
        }
        catch
        {
            ValkyrieDebug.Log("Warning: Unable to remove temporary files.");
        }
    }
}
