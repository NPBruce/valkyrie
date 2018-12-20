using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ValkyrieTools;

public class QuestsManager
{
    // Ini content for all remote quests
    //   key : quest name
    //   value : Quest object
    public Dictionary<string, QuestData.Quest> remote_quests = null;

    // List of all locally available quests (sorted by name)
    public List<string> local_quests = null;

    // List of all quests sorted from small to high (should be displayed the other way)
    //   key : sort value 
    //   value : Quest package name
    SortedList<float, string>  quests_sorted_by_rating = null;
    SortedList<string, string> quests_sorted_by_name = null;
    SortedList<float, string>  quests_sorted_by_difficulty = null;
    SortedList<int, string> quests_sorted_by_duration = null;
    SortedList<string, string> quests_sorted_by_date = null;

    public bool error_download = false;
    public string error_download_description = "";
    public bool download_available = false;


    public QuestsManager()
	{
        remote_quests = new Dictionary<string, QuestData.Quest>();
        local_quests = new List<string>();

        Game game = Game.Get();

        // -- Download remote quests list INI file --
        if (game.gameType.TypeName() == "MoM")
        {
            HTTPManager.Get("https://drive.google.com/uc?id=13JEtzRQ1LcCAAhKluxii0tgKDW71XODV&export=download", QuestsDownload_callback);
        }
        else if (game.gameType.TypeName() == "D2E")
        {
            HTTPManager.Get("https://drive.google.com/uc?id=1oa6NhKLUFn61RH1niPJzpFT4fG9iQFas&export=download", QuestsDownload_callback);
        }
        else
        {
            ValkyrieTools.ValkyrieDebug.Log("ERROR: DownloadQuests is called when no game type has been selected");
            return;
        }
    }

    private void QuestsDownload_callback(string data, bool error)
    {
        download_available = true;

        if (error)
        {
            error_download = true;
            error_download_description = data;
            return;
        }

        // Parse ini
        IniData remoteManifest = IniRead.ReadFromString(data);
        foreach (KeyValuePair<string, Dictionary<string, string>> quest_kv in remoteManifest.data)
        {
            remote_quests.Add(quest_kv.Key, new QuestData.Quest(quest_kv.Value));
        }

        if (remote_quests.Count == 0)
        {
            Debug.Log("ERROR: Quest list is empty\n");
            return;
        }

        UpdateLocalQuestList();

        SortQuests();
    }

    private void UpdateLocalQuestList()
    {
        // Check list of package (note: this does not check package content)
        string[] archives = Directory.GetFiles(ContentData.DownloadPath(), "*.valkyrie", SearchOption.AllDirectories);
        foreach (string f in archives)
        {
            local_quests.Add(f.ToLower());
        }

        local_quests.Sort();

        // Update status for each questData
        foreach (KeyValuePair<string, QuestData.Quest> quest_data in remote_quests)
        {
            string pkg_name = quest_data.Key.ToLower() + ".valkyrie";
            quest_data.Value.downloaded = local_quests.Contains(pkg_name);
        }

    }

    /// <summary>
    /// Comparer for comparing two keys, handling equality as beeing greater
    /// Use this Comparer e.g. with SortedLists or SortedDictionaries, that don't allow duplicate keys
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    private class DuplicateKeyComparer<TKey>
                    :
                 IComparer<TKey> where TKey : IComparable
    {
        #region IComparer<TKey> Members

        public int Compare(TKey x, TKey y)
        {
            int result = x.CompareTo(y);

            if (result == 0)
                return 1;   // Handle equality as beeing greater
            else
                return result;
        }

        #endregion
    }


    // Build sorted lists
    //   by User rating
    //   by name
    //   by Difficulty(easy to difficult);
    //   by Duration(short to long).
    //   by update / creation date  (TODO)
    public void SortQuests()
    {
        Game game = Game.Get();

        quests_sorted_by_rating = new SortedList<float, string>(new DuplicateKeyComparer<float>());
        quests_sorted_by_name = new SortedList<string, string>(new DuplicateKeyComparer<string>());
        quests_sorted_by_difficulty = new SortedList<float, string>(new DuplicateKeyComparer<float>());
        quests_sorted_by_duration = new SortedList<int, string>(new DuplicateKeyComparer<int>());
        quests_sorted_by_date = new SortedList<string, string>(new DuplicateKeyComparer<string>());

        if (game.stats != null && game.stats.scenarios_stats != null) // we should wait for stats to be available
        {
            foreach (KeyValuePair<string, QuestData.Quest> quest_data in remote_quests)
            {
                string pkg_name = quest_data.Key.ToLower() + ".valkyrie";
                if (game.stats.scenarios_stats.ContainsKey(pkg_name))
                {
                    quests_sorted_by_rating.Add(game.stats.scenarios_stats[pkg_name].scenario_avg_rating, quest_data.Key);
                }
                else
                {
                    quests_sorted_by_rating.Add(0.0f, quest_data.Key);
                }

                quests_sorted_by_name.Add(quest_data.Key, quest_data.Key);
                quests_sorted_by_difficulty.Add(quest_data.Value.difficulty, quest_data.Key);
                quests_sorted_by_duration.Add(quest_data.Value.lengthMax, quest_data.Key);

                // TODO support by date
                // quests_sorted_by_date.Add(quest_data.Value.difficulty, quest_data.Key);

            }
        }
    }


}


