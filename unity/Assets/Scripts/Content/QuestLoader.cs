using UnityEngine;
using System.Collections.Generic;
using System.IO;
using ValkyrieTools;
using Assets.Scripts;
using Assets.Scripts.Content;

// Class for getting lists of quest with details
public class QuestLoader {

    // Return a dictionary of all available quests
    public static Dictionary<string, QuestData.Quest> GetQuests(bool getHidden = false)
    {
        Dictionary<string, QuestData.Quest> quests = new Dictionary<string, QuestData.Quest>();

        Game game = Game.Get();
        // Look in the user application data directory
        string dataLocation = Game.AppData();
        ExtractManager.mkDir(dataLocation);
        ExtractManager.mkDir(ContentData.DownloadPath());

        // Get a list of downloaded quest not packed
        List<string> questDirectories = GetUnpackedQuests(ContentData.DownloadPath());

        // Extract only required files from downloaded packages 
        ExtractManager.ExtractPackages(ContentData.DownloadPath());

        // Get the list of extracted packages
        questDirectories.AddRange(GetUnpackedQuests(ContentData.TempValkyriePath));

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
        questDirectories.AddRange(GetUnpackedQuests(dataLocation));
        
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

    public static System.Threading.Tasks.Task<Dictionary<string, QuestData.Quest>> GetQuestsAsync(QuestData.QuestLoaderContext context, bool getHidden = false)
    {
        return System.Threading.Tasks.Task.Run(() =>
        {
            Dictionary<string, QuestData.Quest> quests = new Dictionary<string, QuestData.Quest>();

            // Look in the user application data directory
            string dataLocation = Game.AppData();
            ExtractManager.mkDir(dataLocation);
            ExtractManager.mkDir(ContentData.DownloadPath());

            // Get a list of downloaded quest not packed
            List<string> questDirectories = GetUnpackedQuests(ContentData.DownloadPath());

            // Extract only required files from downloaded packages 
            ExtractManager.ExtractPackages(ContentData.DownloadPath());

            // Get the list of extracted packages
            questDirectories.AddRange(GetUnpackedQuests(ContentData.TempValkyriePath));

            // Add the list of editor quest
            if (context.isMoM)
            {
                dataLocation += "/MoM/Editor";
            }
            // Logic for other game types based on string check if needed
            if (context.gameType == "D2E")
            {
                dataLocation += "/D2E/Editor";
            }
            if (context.gameType == "IA")
            {
                dataLocation += "/IA/Editor";
            }
            
            questDirectories.AddRange(GetUnpackedQuests(dataLocation));

            // Go through all directories
            foreach (string p in questDirectories)
            {
                // load quest with context
                QuestData.Quest q = new QuestData.Quest(p, context);
                // Check quest is valid and of the right type
                if (q.valid && q.type.Equals(context.gameType))
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
        });
    }

    // Return a single quest, quest name is without file extension
    public static QuestData.Quest GetSingleQuest(string questName, bool getHidden = false)
    {
        QuestData.Quest quest = null;

        Game game = Game.Get();
        // Look in the user application data directory
        string dataLocation = Game.AppData();
        ExtractManager.mkDir(dataLocation);
        ExtractManager.mkDir(ContentData.DownloadPath());

        string path = ContentData.DownloadPath() + Path.DirectorySeparatorChar + questName + ValkyrieConstants.ScenarioDownloadContainerExtension;
        ExtractManager.ExtractSinglePackagePartial(path);

        // load quest
        QuestData.Quest q = new QuestData.Quest(Path.Combine(ContentData.TempValkyriePath, Path.GetFileName(path)));
        // Check quest is valid and of the right type
        if (q.valid && q.type.Equals(game.gameType.TypeName()))
        {
            // Is the quest hidden?
            if (!q.hidden || getHidden)
            {
                // Add quest to quest list
                quest = q;
            }
        }

        // Return list of available quests
        return quest;
    }

    // Return list of quests available in the user path (includes packages)
    public static Dictionary<string, QuestData.Quest> GetUserQuests()
    {
        Dictionary<string, QuestData.Quest> quests = new Dictionary<string, QuestData.Quest>();

        // Read user application data for quests
        string dataLocation = Game.AppData();
        ExtractManager.mkDir(dataLocation);
        List<string> questDirectories = GetUnpackedQuests(dataLocation);

        // Read extracted packages
        questDirectories.AddRange(GetUnpackedQuests(ContentData.TempValkyriePath));

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
        ExtractManager.mkDir(dataLocation);
        List<string> questDirectories = GetUnpackedQuests(dataLocation);

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

    // Get list of directories with quests at a path (unpacked quests)
    public static List<string> GetUnpackedQuests(string path)
    {
        List<string> quests = new List<string>();

        if (!Directory.Exists(path))
        {
            return quests;
        }

        // Get all directories at path
        List<string> questDirectories = ExtractManager.DirList(path);
        foreach (string p in questDirectories)
        {
            // All packs must have a quest.ini, otherwise ignore
            if (File.Exists(p + ValkyrieConstants.QuestIniFilePath))
            {
                    quests.Add(p);
            }
        }

        return quests;
    }
}
