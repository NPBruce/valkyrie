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

public class RemoteContentPackManager
{
    // Ini content for all remote quests
    //   key : quest name
    //   value : Quest object
    public Dictionary<string, RemoteContentPack> remote_RemoteContentPack_data = null;

    // Ini content for all local quests (only required when offline)
    //   key : quest name
    //   value : Quest object
    public Dictionary<string, RemoteContentPack> local_RemoteContentPack_data = null;

    // List of all quests sorted from small to high (should be displayed the other way)
    //   key : sort value 
    //   value : Quest package name
    SortedList<string, string> RemoteContentPacks_sorted_by_author = null;
    SortedList<string, string> RemoteContentPacks_sorted_by_name = null;
    SortedList<float, string>  RemoteContentPacks_sorted_by_difficulty = null;
    SortedList<int, string> RemoteContentPacks_sorted_by_duration = null;

    SortedList<float, string> RemoteContentPacks_sorted_by_rating = null;
    SortedList<System.DateTime, string> RemoteContentPacks_sorted_by_date = null;
    SortedList<float, string> RemoteContentPacks_sorted_by_win_ratio = null;
    SortedList<float, string> RemoteContentPacks_sorted_by_avg_duration = null;

    // status of download of quest list
    public bool error_download = false;
    public string error_download_description = "";

    // Callback when download is done
    Action<bool> cb_download = null;

    // Current mode to get quest list
    // Default is local, it changes when file has been downloaded
    // It can also be set by user
    public enum RemoteContentPackListMode { ONLINE, LOCAL, DOWNLOADING, ERROR_DOWNLOAD };
    public RemoteContentPackListMode content_pack_list_Mode = RemoteContentPackListMode.LOCAL;
    bool force_local_RemoteContentPack = false;

    public static string GetCustomCategory()
    {
        return Game.Get().gameType.TypeName() + ValkyrieConstants.customCategoryName;
    }

    public RemoteContentPackManager()
	{
        remote_RemoteContentPack_data = new Dictionary<string, RemoteContentPack>();

        Game game = Game.Get();

        // -- Download remote content pack list INI file --
        if (game.gameType.TypeName() == "MoM")
        {
            HTTPManager.Get("https://drive.google.com/uc?id=1qGpXhwvQNsSOcT6CAd45RLB-zFI_cEou&export=download", RemoteContentPackDownload_callback);
            content_pack_list_Mode = RemoteContentPackListMode.DOWNLOADING;
        }
        else if (game.gameType.TypeName() == "D2E")
        {
            HTTPManager.Get("https://drive.google.com/uc?id=1qGpXhwvQNsSOcT6CAd45RLB-zFI_cEou&export=download", RemoteContentPackDownload_callback);
            content_pack_list_Mode = RemoteContentPackListMode.DOWNLOADING;
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
    public void SetMode(RemoteContentPackListMode qlm)
    {
        content_pack_list_Mode = qlm;

        if (qlm == RemoteContentPackListMode.ONLINE)
        {
            force_local_RemoteContentPack = false;
        }
        else
        {
            force_local_RemoteContentPack = true;
        }
    }

    private void RemoteContentPackDownload_callback(string data, bool error, System.Uri uri)
    {
        if (error)
        {
            error_download = true;
            error_download_description = data;
            // Callback to display screen
            if (cb_download != null)
                cb_download(false);

            content_pack_list_Mode = RemoteContentPackListMode.ERROR_DOWNLOAD;

            return;
        }

        if(!force_local_RemoteContentPack)
            content_pack_list_Mode = RemoteContentPackListMode.ONLINE;

        // Parse ini
        IniData remoteManifest = IniRead.ReadFromString(data);
        foreach (KeyValuePair<string, Dictionary<string, string>> RemoteContentPack_kv in remoteManifest.data)
        {
            var remoteContentPack = new RemoteContentPack(RemoteContentPack_kv.Key, RemoteContentPack_kv.Value);
            remote_RemoteContentPack_data.Add(RemoteContentPack_kv.Key, remoteContentPack);
        }

        if (remote_RemoteContentPack_data.Count == 0)
        {
            Debug.Log("ERROR: RemoteContentPack list is empty\n");
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
        ManifestManager manager = new ManifestManager(ContentData.CustomContentPackPath());
        IniData localManifest = manager.GetLocalContentPackManifestIniData();

        if (localManifest == null)
            return;

        // Update download status for each RemoteContentPackData and check if update is available
        foreach (KeyValuePair<string, RemoteContentPack> RemoteContentPack_Data in remote_RemoteContentPack_data)
        {
            if (localManifest.data.ContainsKey(RemoteContentPack_Data.Key))
            {
                RemoteContentPack_Data.Value.downloaded = true;
                RemoteContentPack_Data.Value.update_available = (localManifest.data[RemoteContentPack_Data.Key]["version"] != RemoteContentPack_Data.Value.version);
            }
        }
    }

    public void SetContetPackAvailability(string key, bool isAvailable)
    {
        string saveLocation = ContentData.DownloadPath();

        ManifestManager manager = new ManifestManager(saveLocation);
        var localManifest = manager.GetLocalQuestManifestIniData();

        if (isAvailable)
        {
            IniData downloaded_RemoteContentPack = IniRead.ReadFromString(remote_RemoteContentPack_data[key].ToString());
            localManifest.Remove(key);
            localManifest.Add(key, downloaded_RemoteContentPack.data["Quest"]);
        }
        else
        {
            if(localManifest.Get(key) != null )
                localManifest.Remove(key);
            // we need to delete /temp and reload list
            UnloadLocalRemoteContentPacks();
        }

        if (File.Exists(saveLocation + ValkyrieConstants.ScenarioManifestPath))
        {
            File.Delete(saveLocation + ValkyrieConstants.ScenarioManifestPath);
        }
        File.WriteAllText(saveLocation + ValkyrieConstants.ScenarioManifestPath, localManifest.ToString());

        // update status quest
        remote_RemoteContentPack_data[key].downloaded = isAvailable;
        remote_RemoteContentPack_data[key].update_available = false;
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
    //public void SortRemoteContentPacks()
    //{
    //    Game game = Game.Get();

    //    if(RemoteContentPacks_sorted_by_author==null)
    //    { 
    //        RemoteContentPacks_sorted_by_author = new SortedList<string, string>(new DuplicateKeyComparer<string>());
    //        RemoteContentPacks_sorted_by_name = new SortedList<string, string>(new DuplicateKeyComparer<string>());
    //        RemoteContentPacks_sorted_by_difficulty = new SortedList<float, string>(new DuplicateKeyComparer<float>());
    //        RemoteContentPacks_sorted_by_duration = new SortedList<int, string>(new DuplicateKeyComparer<int>());

    //        RemoteContentPacks_sorted_by_rating = new SortedList<float, string>(new DuplicateKeyComparer<float>());
    //        RemoteContentPacks_sorted_by_date = new SortedList<System.DateTime, string>(new DuplicateKeyComparer<System.DateTime>());
    //        RemoteContentPacks_sorted_by_avg_duration = new SortedList<float, string>(new DuplicateKeyComparer<float>());
    //        RemoteContentPacks_sorted_by_win_ratio = new SortedList<float, string>(new DuplicateKeyComparer<float>());
    //    }
    //    else
    //    {
    //        RemoteContentPacks_sorted_by_author.Clear();
    //        RemoteContentPacks_sorted_by_name.Clear();
    //        RemoteContentPacks_sorted_by_difficulty.Clear();
    //        RemoteContentPacks_sorted_by_duration.Clear();

    //        RemoteContentPacks_sorted_by_rating.Clear();
    //        RemoteContentPacks_sorted_by_date.Clear();
    //        RemoteContentPacks_sorted_by_avg_duration.Clear();
    //        RemoteContentPacks_sorted_by_win_ratio.Clear();
    //    }

    //    if (content_pack_list_Mode != RemoteContentPackListMode.ONLINE || force_local_RemoteContentPack)
    //    {
    //        if(local_RemoteContentPack_data==null)
    //        {
    //            ValkyrieDebug.Log("INFO: No list of local content packs available");
    //            return;
    //        }
    //        ValkyrieDebug.Log("INFO: Sorting through "+ local_RemoteContentPack_data.Count + " local content packs");

    //        //foreach (KeyValuePair<string, RemoteContentPack> quest_data in local_RemoteContentPack_data)
    //        //{
    //        //    LocalizationRead.AddDictionary("qst", quest_data.Value.localizationDict);
    //        //    RemoteContentPacks_sorted_by_author.Add(quest_data.Value.GetShortAuthor(), quest_data.Key);
    //        //    RemoteContentPacks_sorted_by_name.Add(quest_data.Value.name.Translate(), quest_data.Key);
    //        //    RemoteContentPacks_sorted_by_difficulty.Add(quest_data.Value.difficulty, quest_data.Key);
    //        //    RemoteContentPacks_sorted_by_duration.Add(quest_data.Value.lengthMax, quest_data.Key);
    //        //}
    //    }
    //    else
    //    {
    //        if (game.stats != null && game.stats.scenarios_stats != null) // we should wait for stats to be available
    //        {
    //            if (remote_RemoteContentPack_data == null)
    //            {
    //                ValkyrieDebug.Log("INFO: No list of external content packs available");
    //                return;
    //            }

    //            ValkyrieDebug.Log("INFO: Sorting through stats data available for " + remote_RemoteContentPack_data.Count + " scenarios");

    //            foreach (KeyValuePair<string, RemoteContentPack> quest_data in remote_RemoteContentPack_data)
    //            {
    //                string pkg_name = quest_data.Key.ToLower() + ValkyrieConstants.ScenarioDownloadContainerExtension;
    //                if (game.stats.scenarios_stats.ContainsKey(pkg_name))
    //                {
    //                    RemoteContentPacks_sorted_by_rating.Add(game.stats.scenarios_stats[pkg_name].scenario_avg_rating, quest_data.Key);
    //                    RemoteContentPacks_sorted_by_avg_duration.Add(game.stats.scenarios_stats[pkg_name].scenario_avg_duration, quest_data.Key);
    //                    RemoteContentPacks_sorted_by_win_ratio.Add(game.stats.scenarios_stats[pkg_name].scenario_avg_win_ratio, quest_data.Key);
    //                }
    //                else
    //                {
    //                    RemoteContentPacks_sorted_by_rating.Add(0.0f, quest_data.Key);
    //                    RemoteContentPacks_sorted_by_avg_duration.Add(0.0f, quest_data.Key);
    //                    RemoteContentPacks_sorted_by_win_ratio.Add(0.0f, quest_data.Key);
    //                }

    //                // Use player selected language or scenario default language for sort by name
    //                if(quest_data.Value.languages_name.Keys.Contains(game.currentLang))
    //                    RemoteContentPacks_sorted_by_name.Add(quest_data.Value.languages_name[game.currentLang], quest_data.Key);
    //                else
    //                    RemoteContentPacks_sorted_by_name.Add(quest_data.Value.languages_name[quest_data.Value.defaultLanguage], quest_data.Key);
    //                RemoteContentPacks_sorted_by_difficulty.Add(quest_data.Value.difficulty, quest_data.Key);
    //                RemoteContentPacks_sorted_by_duration.Add(quest_data.Value.lengthMax, quest_data.Key);
    //                RemoteContentPacks_sorted_by_date.Add(quest_data.Value.latest_update, quest_data.Key);
    //                RemoteContentPacks_sorted_by_author.Add(quest_data.Value.GetShortAuthor(), quest_data.Key);
    //            }
    //        }
    //    }
    //}

    public List<string> GetList(string sortOrder)
    {
        List<string> ret;

        switch(sortOrder)
        {
           case "author":
                ret = RemoteContentPacks_sorted_by_author.Values.ToList();
                break;

            case "name":
                ret = RemoteContentPacks_sorted_by_name.Values.ToList();
                break;

            case "difficulty":
                ret = RemoteContentPacks_sorted_by_difficulty.Values.ToList();
                break;

            case "duration":
                ret = RemoteContentPacks_sorted_by_duration.Values.ToList();
                break;

            case "rating":
                ret = RemoteContentPacks_sorted_by_rating.Values.ToList();
                break;

            case "date":
                ret = RemoteContentPacks_sorted_by_date.Values.ToList();
                break;

            case "average_win_ratio":
                ret = RemoteContentPacks_sorted_by_win_ratio.Values.ToList();
                break;

            case "average_duration":
                ret = RemoteContentPacks_sorted_by_avg_duration.Values.ToList();
                break;

            default:
                Debug.Log("Setting an unknown sort type, this should not happen");
                ret = RemoteContentPacks_sorted_by_rating.Values.ToList();
                break;
        }

        return ret;
    }

    public RemoteContentPack GetContentPackData(string key)
    {
        if (content_pack_list_Mode == RemoteContentPackListMode.ONLINE && !force_local_RemoteContentPack)
            return remote_RemoteContentPack_data[key];
        else
            return local_RemoteContentPack_data[key];
    }

    // --- Management of local quests, when offline ---
    //public void LoadAllLocalRemoteContentPacks()
    //{
    //    if (local_RemoteContentPack_data == null)
    //    {
    //        // Clean up temporary files
    //        UnloadLocalRemoteContentPacks();
    //        // extract and load local quest
    //        local_RemoteContentPack_data = QuestLoader.GetQuests();
    //    }
    //}

    public void UnloadLocalRemoteContentPacks()
    {
        if (local_RemoteContentPack_data != null)
        {
            // Clean up temporary files
            ExtractManager.CleanTemp();
            local_RemoteContentPack_data = null;
        }
    }

    private void RemoteQuestsListDownload_cb(bool is_available)
    {
        //DrawOnlineModeButton();
    }
}
