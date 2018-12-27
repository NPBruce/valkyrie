using Assets.Scripts.Content;
using Assets.Scripts.UI.Screens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using ValkyrieTools;

public class QuestsManager
{
    // Ini content for all remote quests
    //   key : quest name
    //   value : Quest object
    public Dictionary<string, QuestData.Quest> remote_quests_data = null;

    // Ini content for all local quests (only required when offline)
    //   key : quest name
    //   value : Quest object
    public Dictionary<string, QuestData.Quest> local_quests_data = null;

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
    public bool download_done = false;


    public QuestsManager()
	{
        remote_quests_data = new Dictionary<string, QuestData.Quest>();

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
        if (error)
        {
            error_download = true;
            error_download_description = data;
            return;
        }

        download_done = true;

        // Parse ini
        IniData remoteManifest = IniRead.ReadFromString(data);
        foreach (KeyValuePair<string, Dictionary<string, string>> quest_kv in remoteManifest.data)
        {
            remote_quests_data.Add(quest_kv.Key, new QuestData.Quest(quest_kv.Value));
        }

        if (remote_quests_data.Count == 0)
        {
            Debug.Log("ERROR: Quest list is empty\n");
            return;
        }

        CheckLocalAvailability();

        SortQuests();
    }

    private void CheckLocalAvailability()
    {
        // List of all locally available quests (sorted by name)
        List<string> local_quests_name = new List<string>();

        // Check list of package (note: this does not check package content)
        string[] archives = Directory.GetFiles(ContentData.DownloadPath(), "*.valkyrie", SearchOption.AllDirectories);
        foreach (string f in archives)
        {
            local_quests_name.Add(f.ToLower());
        }

        local_quests_name.Sort();

        // Update download status for each questData
        foreach (KeyValuePair<string, QuestData.Quest> quest_data in remote_quests_data)
        {
            string pkg_name = quest_data.Key.ToLower() + ".valkyrie";
            quest_data.Value.downloaded = local_quests_name.Contains(pkg_name);
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
    public void SortQuests(bool isLocalQuest=false)
    {
        Game game = Game.Get();

        quests_sorted_by_rating = new SortedList<float, string>(new DuplicateKeyComparer<float>());
        quests_sorted_by_name = new SortedList<string, string>(new DuplicateKeyComparer<string>());
        quests_sorted_by_difficulty = new SortedList<float, string>(new DuplicateKeyComparer<float>());
        quests_sorted_by_duration = new SortedList<int, string>(new DuplicateKeyComparer<int>());
        quests_sorted_by_date = new SortedList<string, string>(new DuplicateKeyComparer<string>());

        if(isLocalQuest)
        {
            foreach (KeyValuePair<string, QuestData.Quest> quest_data in local_quests_data)
            {
                quests_sorted_by_name.Add(quest_data.Key, quest_data.Key);
                quests_sorted_by_difficulty.Add(quest_data.Value.difficulty, quest_data.Key);
                quests_sorted_by_duration.Add(quest_data.Value.lengthMax, quest_data.Key);
                // TODO support by date
                // quests_sorted_by_date.Add(quest_data.Value.difficulty, quest_data.Key);
            }
        }
        else
        {
            if (game.stats != null && game.stats.scenarios_stats != null) // we should wait for stats to be available
            {
                foreach (KeyValuePair<string, QuestData.Quest> quest_data in remote_quests_data)
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

                    // select default language or default language for sort by name
                    if(quest_data.Value.languages_name.Keys.Contains(game.currentLang))
                        quests_sorted_by_name.Add(quest_data.Value.languages_name[game.currentLang], quest_data.Key);
                    else
                        quests_sorted_by_name.Add(quest_data.Value.languages_name[quest_data.Value.defaultLanguage], quest_data.Key);

                    quests_sorted_by_difficulty.Add(quest_data.Value.difficulty, quest_data.Key);
                    quests_sorted_by_duration.Add(quest_data.Value.lengthMax, quest_data.Key);

                    // TODO support by date
                    // quests_sorted_by_date.Add(quest_data.Value.difficulty, quest_data.Key);

                }
            }
        }
    }

    public List<string> GetList(string sortOrder)
    {
        List<string> ret;

        switch(sortOrder)
        {
            case "rating":
                ret = quests_sorted_by_rating.Values.ToList();
                break;

            case "name":
                ret = quests_sorted_by_name.Values.ToList();
                break;

            case "difficulty":
                ret = quests_sorted_by_difficulty.Values.ToList();
                break;

            case "duration":
                ret = quests_sorted_by_duration.Values.ToList();
                break;

            case "date":
                ret = quests_sorted_by_date.Values.ToList();
                break;

            default:
                ret = quests_sorted_by_rating.Values.ToList();
                break;
        }


        return ret;
    }

    public QuestData.Quest getQuestData(string key)
    {
        if (local_quests_data != null)
            return local_quests_data[key];
        else
            return remote_quests_data[key];
    }

    // --- Management of local quests, when offline ---
    // TODO : support sort options for local list of quests
    public List<string> GetLocalList()
    {
        return local_quests_data.Select(d => d.Key).ToList<string>();
    }

    public void loadAllLocalQuests()
    {
        local_quests_data = QuestLoader.GetQuests();
        SortQuests(true);
    }

}

// Class for quest selection window
public class QuestDownload2 : MonoBehaviour
{
    private WWW download;

    /// <summary>
    /// Download required files then draw screen
    /// </summary>
    void Start()
    {
        new LoadingScreen(new StringKey("val", "DOWNLOAD_LIST").Translate());
    }

    /// <summary>
    /// Select to download
    /// </summary>
    /// <param name="key">Quest name to download</param>
    public void Download(string key)
    {
        string package = Game.Get().questsList.remote_quests_data[key].package_url + key + ".valkyrie";
        StartCoroutine(Download(package, delegate { Save(key); }));
    }

    /// <summary>
    /// Called after download finished to save to disk
    /// </summary>
    /// <param name="key">Quest name to download</param>
    private void Save(string key)
    {
        QuestLoader.mkDir(ContentData.DownloadPath());

        // Write to disk
        using (BinaryWriter writer = new BinaryWriter(File.Open(ContentData.DownloadPath() + Path.DirectorySeparatorChar + key + ".valkyrie", FileMode.Create)))
        {
            writer.Write(download.bytes);
            writer.Close();
        }

        Game.Get().questsList.remote_quests_data[key].downloaded = true;
    }

    /// <summary>
    /// Download and call function
    /// </summary>
    /// <param name="file">Path to download</param>
    /// <param name="call">function to call on completion</param>
    private IEnumerator Download(string file, UnityEngine.Events.UnityAction call)
    {
        download = new WWW(file);
        yield return download;
        if (!string.IsNullOrEmpty(download.error))
        {
            // fixme not fatal
            ValkyrieDebug.Log("Error while downloading :" + file);
            ValkyrieDebug.Log(download.error);
            //Application.Quit();
        } else
        {
            call();
        }
    }
}