using Assets.Scripts;
using Assets.Scripts.Content;
using Assets.Scripts.UI.Screens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using ValkyrieTools;

public class ContentPackManager
{
    // Ini content for all remote quests
    //   key : quest name
    //   value : Quest object
    public Dictionary<string, QuestData.Quest> remote_contentpack_data = null;

    // Ini content for all local quests (only required when offline)
    //   key : quest name
    //   value : Quest object
    public Dictionary<string, QuestData.Quest> local_contentpack_data = null;

    // List of all quests sorted from small to high (should be displayed the other way)
    //   key : sort value 
    //   value : Quest package name
    SortedList<string, string> contentpacks_sorted_by_author = null;
    SortedList<string, string> contentpacks_sorted_by_name = null;
    SortedList<float, string>  contentpacks_sorted_by_difficulty = null;
    SortedList<int, string> contentpacks_sorted_by_duration = null;

    SortedList<float, string> contentpacks_sorted_by_rating = null;
    SortedList<System.DateTime, string> contentpacks_sorted_by_date = null;
    SortedList<float, string> contentpacks_sorted_by_win_ratio = null;
    SortedList<float, string> contentpacks_sorted_by_avg_duration = null;

    // status of download of quest list
    public bool error_download = false;
    public string error_download_description = "";

    // Callback when download is done
    Action<bool> cb_download = null;

    // Current mode to get quest list
    // Default is local, it changes when file has been downloaded
    // It can also be set by user
    public enum ContentPackListMode { ONLINE, LOCAL, DOWNLOADING, ERROR_DOWNLOAD };
    public ContentPackListMode quest_list_mode = ContentPackListMode.LOCAL;
    bool force_local_contentPack = false;

    public ContentPackManager()
	{
        remote_contentpack_data = new Dictionary<string, QuestData.Quest>();

        Game game = Game.Get();

        // -- Download remote quests list INI file --
        if (game.gameType.TypeName() == "MoM")
        {
            HTTPManager.Get("https://drive.google.com/uc?id=19y6XlVOAfMJGQ3JMDr6uhvZyt2meVNHW&export=download", ContentPackDownload_callback);
            quest_list_mode = ContentPackListMode.DOWNLOADING;
        }
        else if (game.gameType.TypeName() == "D2E")
        {
            HTTPManager.Get("https://drive.google.com/uc?id=19y6XlVOAfMJGQ3JMDr6uhvZyt2meVNHW&export=download", ContentPackDownload_callback);
            quest_list_mode = ContentPackListMode.DOWNLOADING;
        }
        else
        {
            ValkyrieTools.ValkyrieDebug.Log("ERROR: DownloadQuests is called when no game type has been selected");
            return;
        }
    }

    public void Register_cb_download(Action<bool> cb_download_p)
    {
        cb_download = cb_download_p;
    }

    // This is called by a user action
    public void SetMode(ContentPackListMode qlm)
    {
        quest_list_mode = qlm;

        if (qlm == ContentPackListMode.ONLINE)
        {
            force_local_contentPack = false;
        }
        else
        {
            force_local_contentPack = true;
        }
    }

    private void ContentPackDownload_callback(string data, bool error, System.Uri uri)
    {
        if (error)
        {
            error_download = true;
            error_download_description = data;
            // Callback to display screen
            if (cb_download != null)
                cb_download(false);

            quest_list_mode = ContentPackListMode.ERROR_DOWNLOAD;

            return;
        }

        if(!force_local_contentPack)
            quest_list_mode = ContentPackListMode.ONLINE;

        // Parse ini
        IniData remoteManifest = IniRead.ReadFromString(data);
        foreach (KeyValuePair<string, Dictionary<string, string>> contentpack_kv in remoteManifest.data)
        {
            remote_contentpack_data.Add(contentpack_kv.Key, new QuestData.Quest(contentpack_kv.Key, contentpack_kv.Value));
        }

        if (remote_contentpack_data.Count == 0)
        {
            Debug.Log("ERROR: Contentpack list is empty\n");
            error_download = true;
            error_download_description = "ERROR: Quest list is empty";
            if (cb_download != null)
                cb_download(false);
            return;
        }

        CheckLocalAvailability();

        if (cb_download != null)
            cb_download(true);
    }

    private void CheckLocalAvailability()
    {
        ManifestManager manager = new ManifestManager(ContentData.DownloadPath());
        IniData localManifest = manager.GetLocalManifestIniData();

        if (localManifest == null)
            return;

        // Update download status for each contentPackData and check if update is available
        foreach (KeyValuePair<string, QuestData.Quest> quest_data in remote_contentpack_data)
        {
            if (localManifest.data.ContainsKey(quest_data.Key))
            {
                quest_data.Value.downloaded = true;
                quest_data.Value.update_available = (localManifest.data[quest_data.Key]["version"] != quest_data.Value.version);
            }
        }
    }

    public void SetContetPackAvailability(string key, bool isAvailable)
    {
        string saveLocation = ContentData.DownloadPath();

        ManifestManager manager = new ManifestManager(saveLocation);
        var localManifest = manager.GetLocalManifestIniData();

        if (isAvailable)
        {
            IniData downloaded_contentPack = IniRead.ReadFromString(remote_contentpack_data[key].ToString());
            localManifest.Remove(key);
            localManifest.Add(key, downloaded_contentPack.data["Quest"]);
        }
        else
        {
            if(localManifest.Get(key) != null )
                localManifest.Remove(key);
            // we need to delete /temp and reload list
            UnloadLocalContentPacks();
        }

        if (File.Exists(saveLocation + ValkyrieConstants.ScenarioManifestPath))
        {
            File.Delete(saveLocation + ValkyrieConstants.ScenarioManifestPath);
        }
        File.WriteAllText(saveLocation + ValkyrieConstants.ScenarioManifestPath, localManifest.ToString());

        // update status quest
        remote_contentpack_data[key].downloaded = isAvailable;
        remote_contentpack_data[key].update_available = false;
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
    public void SortContentPacks()
    {
        Game game = Game.Get();

        if(contentpacks_sorted_by_author==null)
        { 
            contentpacks_sorted_by_author = new SortedList<string, string>(new DuplicateKeyComparer<string>());
            contentpacks_sorted_by_name = new SortedList<string, string>(new DuplicateKeyComparer<string>());
            contentpacks_sorted_by_difficulty = new SortedList<float, string>(new DuplicateKeyComparer<float>());
            contentpacks_sorted_by_duration = new SortedList<int, string>(new DuplicateKeyComparer<int>());

            contentpacks_sorted_by_rating = new SortedList<float, string>(new DuplicateKeyComparer<float>());
            contentpacks_sorted_by_date = new SortedList<System.DateTime, string>(new DuplicateKeyComparer<System.DateTime>());
            contentpacks_sorted_by_avg_duration = new SortedList<float, string>(new DuplicateKeyComparer<float>());
            contentpacks_sorted_by_win_ratio = new SortedList<float, string>(new DuplicateKeyComparer<float>());
        }
        else
        {
            contentpacks_sorted_by_author.Clear();
            contentpacks_sorted_by_name.Clear();
            contentpacks_sorted_by_difficulty.Clear();
            contentpacks_sorted_by_duration.Clear();

            contentpacks_sorted_by_rating.Clear();
            contentpacks_sorted_by_date.Clear();
            contentpacks_sorted_by_avg_duration.Clear();
            contentpacks_sorted_by_win_ratio.Clear();
        }

        if (quest_list_mode != ContentPackListMode.ONLINE || force_local_contentPack)
        {
            if(local_contentpack_data==null)
            {
                ValkyrieDebug.Log("INFO: No list of local content packs available");
                return;
            }
            ValkyrieDebug.Log("INFO: Sorting through "+ local_contentpack_data.Count + " local content packs");

            foreach (KeyValuePair<string, QuestData.Quest> quest_data in local_contentpack_data)
            {
                LocalizationRead.AddDictionary("qst", quest_data.Value.localizationDict);
                contentpacks_sorted_by_author.Add(quest_data.Value.GetShortAuthor(), quest_data.Key);
                contentpacks_sorted_by_name.Add(quest_data.Value.name.Translate(), quest_data.Key);
                contentpacks_sorted_by_difficulty.Add(quest_data.Value.difficulty, quest_data.Key);
                contentpacks_sorted_by_duration.Add(quest_data.Value.lengthMax, quest_data.Key);
            }
        }
        else
        {
            if (game.stats != null && game.stats.scenarios_stats != null) // we should wait for stats to be available
            {
                if (remote_contentpack_data == null)
                {
                    ValkyrieDebug.Log("INFO: No list of external content packs available");
                    return;
                }

                ValkyrieDebug.Log("INFO: Sorting through stats data available for " + remote_contentpack_data.Count + " scenarios");

                foreach (KeyValuePair<string, QuestData.Quest> quest_data in remote_contentpack_data)
                {
                    string pkg_name = quest_data.Key.ToLower() + ValkyrieConstants.ScenarioDownloadContainerExtension;
                    if (game.stats.scenarios_stats.ContainsKey(pkg_name))
                    {
                        contentpacks_sorted_by_rating.Add(game.stats.scenarios_stats[pkg_name].scenario_avg_rating, quest_data.Key);
                        contentpacks_sorted_by_avg_duration.Add(game.stats.scenarios_stats[pkg_name].scenario_avg_duration, quest_data.Key);
                        contentpacks_sorted_by_win_ratio.Add(game.stats.scenarios_stats[pkg_name].scenario_avg_win_ratio, quest_data.Key);
                    }
                    else
                    {
                        contentpacks_sorted_by_rating.Add(0.0f, quest_data.Key);
                        contentpacks_sorted_by_avg_duration.Add(0.0f, quest_data.Key);
                        contentpacks_sorted_by_win_ratio.Add(0.0f, quest_data.Key);
                    }

                    // Use player selected language or scenario default language for sort by name
                    if(quest_data.Value.languages_name.Keys.Contains(game.currentLang))
                        contentpacks_sorted_by_name.Add(quest_data.Value.languages_name[game.currentLang], quest_data.Key);
                    else
                        contentpacks_sorted_by_name.Add(quest_data.Value.languages_name[quest_data.Value.defaultLanguage], quest_data.Key);
                    contentpacks_sorted_by_difficulty.Add(quest_data.Value.difficulty, quest_data.Key);
                    contentpacks_sorted_by_duration.Add(quest_data.Value.lengthMax, quest_data.Key);
                    contentpacks_sorted_by_date.Add(quest_data.Value.latest_update, quest_data.Key);
                    contentpacks_sorted_by_author.Add(quest_data.Value.GetShortAuthor(), quest_data.Key);
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
                ret = contentpacks_sorted_by_author.Values.ToList();
                break;

            case "name":
                ret = contentpacks_sorted_by_name.Values.ToList();
                break;

            case "difficulty":
                ret = contentpacks_sorted_by_difficulty.Values.ToList();
                break;

            case "duration":
                ret = contentpacks_sorted_by_duration.Values.ToList();
                break;

            case "rating":
                ret = contentpacks_sorted_by_rating.Values.ToList();
                break;

            case "date":
                ret = contentpacks_sorted_by_date.Values.ToList();
                break;

            case "average_win_ratio":
                ret = contentpacks_sorted_by_win_ratio.Values.ToList();
                break;

            case "average_duration":
                ret = contentpacks_sorted_by_avg_duration.Values.ToList();
                break;

            default:
                Debug.Log("Setting an unknown sort type, this should not happen");
                ret = contentpacks_sorted_by_rating.Values.ToList();
                break;
        }

        return ret;
    }

    public QuestData.Quest GetQuestData(string key)
    {
        if (quest_list_mode == ContentPackListMode.ONLINE && !force_local_contentPack)
            return remote_contentpack_data[key];
        else
            return local_contentpack_data[key];
    }

    // --- Management of local quests, when offline ---
    public void LoadAllLocalContentPacks()
    {
        if (local_contentpack_data == null)
        {
            // Clean up temporary files
            UnloadLocalContentPacks();
            // extract and load local quest
            local_contentpack_data = QuestLoader.GetQuests();
        }
    }

    public void UnloadLocalContentPacks()
    {
        if (local_contentpack_data != null)
        {
            // Clean up temporary files
            ExtractManager.CleanTemp();
            local_contentpack_data = null;
        }
    }
}
