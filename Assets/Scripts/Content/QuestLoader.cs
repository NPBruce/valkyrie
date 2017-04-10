using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;

// Class for getting lists of quest with details
// TODO: work out why this is so slow
public class QuestLoader {

    // Return a dictionary of all available quests
    public static Dictionary<string, QuestData.Quest> GetQuests(bool checkContent = false)
    {
        Dictionary<string, QuestData.Quest> quests = new Dictionary<string, QuestData.Quest>();

        Game game = Game.Get();
        // Look in the user application data directory
        string dataLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie";
        mkDir(dataLocation);
        // Get a list of quest directories (extract found packages)
        List<string> questDirectories = GetQuests(dataLocation);

        // Add packaged quests that have been extracted
        questDirectories.AddRange(GetQuests(Path.GetTempPath() + "Valkyrie"));

        // Go through all directories
        foreach (string p in questDirectories)
        {
            // load quest
            QuestData.Quest q = new QuestData.Quest(p);
            // Check quest is valid and of the right type
            if (q.valid && q.type.Equals(game.gameType.TypeName()))
            {
                // Are all expansions selected?
                if (q.GetMissingPacks(game.cd.GetLoadedPackIDs()).Count == 0 || !checkContent)
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
        string dataLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie";
        mkDir(dataLocation);
        List<string> questDirectories = GetQuests(dataLocation);

        // Read extracted packages
        questDirectories.AddRange(GetQuests(Path.GetTempPath() + "Valkyrie"));

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
        Dictionary<string, QuestData.Quest> quests = new Dictionary<string, QuestData.Quest>();

        // Read user application data for quests
        string dataLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie";
        mkDir(dataLocation);
        List<string> questDirectories = GetQuests(dataLocation);

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

    // Get list of directories with quests at a path, and extract found packages
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

        // Find all packages at path
        string[] archives = Directory.GetFiles(path, "*.valkyrie", SearchOption.AllDirectories);
        // Extract all packages
        foreach (string f in archives)
        {
            // Extract into temp
            mkDir(Path.GetTempPath() + "/Valkyrie");
            string extractedPath = Path.GetTempPath() + "Valkyrie/" + Path.GetFileName(f);
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
                zip.ExtractAll(extractedPath);
                zip.Dispose();
            }
            catch (System.Exception)
            {
                ValkyrieDebug.Log("Warning: Unable to read file: " + extractedPath);
            }
        }

        return quests;
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
        if (!Directory.Exists(Path.GetTempPath() + "/Valkyrie"))
        {
            return;
        }

        try
        {
            Directory.Delete(Path.GetTempPath() + "/Valkyrie", true);
        }
        catch (System.Exception)
        {
            ValkyrieDebug.Log("Warning: Unable to remove temporary files.");
        }
    }
}
