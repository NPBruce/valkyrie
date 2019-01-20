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
    SortedList<string, string> quests_sorted_by_author = null;
    SortedList<string, string> quests_sorted_by_name = null;
    SortedList<float, string>  quests_sorted_by_difficulty = null;
    SortedList<int, string> quests_sorted_by_duration = null;

    SortedList<float, string> quests_sorted_by_rating = null;
    SortedList<System.DateTime, string> quests_sorted_by_date = null;
    SortedList<float, string> quests_sorted_by_win_ratio = null;
    SortedList<float, string> quests_sorted_by_avg_duration = null;

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

    private void QuestsDownload_callback(string data, bool error, System.Uri uri)
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
        // load information on local quests
        IniData localManifest = IniRead.ReadFromIni(ContentData.DownloadPath() + "/manifest.ini");

        if (localManifest == null)
            return;

        // Update download status for each questData and check if update is available
        foreach (KeyValuePair<string, QuestData.Quest> quest_data in remote_quests_data)
        {
            if(localManifest.data.ContainsKey(quest_data.Key))
            {
                quest_data.Value.downloaded = true;
                quest_data.Value.update_available = (localManifest.data[quest_data.Key]["version"] != quest_data.Value.version);
            }
        }
    }
    
    public void SetAvailable(string key, bool isAvailable=true)
    {
        // update list of local quest
        IniData localManifest = IniRead.ReadFromString("");
        string saveLocation = ContentData.DownloadPath();

        if (File.Exists(saveLocation + "/manifest.ini"))
        {
            localManifest = IniRead.ReadFromIni(saveLocation + "/manifest.ini");
        }

        if(isAvailable)
        {
            IniData downloaded_quest = IniRead.ReadFromString(remote_quests_data[key].ToString());
            localManifest.Remove(key);
            localManifest.Add(key, downloaded_quest.data["Quest"]);
        }
        else
        {
            localManifest.Remove(key);
        }

        if (File.Exists(saveLocation + "/manifest.ini"))
        {
            File.Delete(saveLocation + "/manifest.ini");
        }
        File.WriteAllText(saveLocation + "/manifest.ini", localManifest.ToString());

        // update status quest
        remote_quests_data[key].downloaded = isAvailable;
        remote_quests_data[key].update_available = false;
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
    public void SortQuests(bool isLocalQuest=false)
    {
        Game game = Game.Get();

        quests_sorted_by_author = new SortedList<string, string>(new DuplicateKeyComparer<string>());
        quests_sorted_by_name = new SortedList<string, string>(new DuplicateKeyComparer<string>());
        quests_sorted_by_difficulty = new SortedList<float, string>(new DuplicateKeyComparer<float>());
        quests_sorted_by_duration = new SortedList<int, string>(new DuplicateKeyComparer<int>());

        quests_sorted_by_rating = new SortedList<float, string>(new DuplicateKeyComparer<float>());
        quests_sorted_by_date = new SortedList<System.DateTime, string>(new DuplicateKeyComparer<System.DateTime>());
        quests_sorted_by_avg_duration = new SortedList<float, string>(new DuplicateKeyComparer<float>());
        quests_sorted_by_win_ratio = new SortedList<float, string>(new DuplicateKeyComparer<float>());

        if (isLocalQuest)
        {
            foreach (KeyValuePair<string, QuestData.Quest> quest_data in local_quests_data)
            {
                LocalizationRead.AddDictionary("qst", quest_data.Value.localizationDict);
                quests_sorted_by_author.Add(quest_data.Value.GetShortAuthor(), quest_data.Key);
                quests_sorted_by_name.Add(quest_data.Value.name.Translate(), quest_data.Key);
                quests_sorted_by_difficulty.Add(quest_data.Value.difficulty, quest_data.Key);
                quests_sorted_by_duration.Add(quest_data.Value.lengthMax, quest_data.Key);
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
                        quests_sorted_by_avg_duration.Add(game.stats.scenarios_stats[pkg_name].scenario_avg_duration, quest_data.Key);
                        quests_sorted_by_win_ratio.Add(game.stats.scenarios_stats[pkg_name].scenario_avg_win_ratio, quest_data.Key);
                    }
                    else
                    {
                        quests_sorted_by_rating.Add(0.0f, quest_data.Key);
                        quests_sorted_by_avg_duration.Add(0.0f, quest_data.Key);
                        quests_sorted_by_win_ratio.Add(0.0f, quest_data.Key);
                    }

                    // Use player selected language or scenario default language for sort by name
                    if(quest_data.Value.languages_name.Keys.Contains(game.currentLang))
                        quests_sorted_by_name.Add(quest_data.Value.languages_name[game.currentLang], quest_data.Key);
                    else
                        quests_sorted_by_name.Add(quest_data.Value.languages_name[quest_data.Value.defaultLanguage], quest_data.Key);
                    quests_sorted_by_difficulty.Add(quest_data.Value.difficulty, quest_data.Key);
                    quests_sorted_by_duration.Add(quest_data.Value.lengthMax, quest_data.Key);
                    quests_sorted_by_date.Add(quest_data.Value.latest_update, quest_data.Key);
                    quests_sorted_by_author.Add(quest_data.Value.GetShortAuthor(), quest_data.Key);
                }
            }
        }
    }

    public List<string> GetList(string sortOrder)
    {
        List<string> ret;

        switch(sortOrder)
        {
           case "author":
                ret = quests_sorted_by_author.Values.ToList();
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

            case "rating":
                ret = quests_sorted_by_rating.Values.ToList();
                break;

            case "date":
                ret = quests_sorted_by_date.Values.ToList();
                break;

            case "average_win_ratio":
                ret = quests_sorted_by_win_ratio.Values.ToList();
                break;

            case "average_duration":
                ret = quests_sorted_by_avg_duration.Values.ToList();
                break;

            default:
                Debug.Log("Setting an unknown sort type, this should not happen");
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
    public void loadAllLocalQuests()
    {
        local_quests_data = QuestLoader.GetQuests();
        SortQuests(true);
    }

}
