﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.UI.Screens;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
using ValkyrieTools;

// Class for quest selection window
public class QuestDownload : MonoBehaviour
{
    public Dictionary<string, QuestData.Quest> questList;

    public WWW download;
    public Game game;
    List<RemoteQuest> remoteQuests;
    IniData localManifest;

    /// <summary>
    /// Download required files then draw screen
    /// </summary>
    void Start()
    {
        new LoadingScreen(new StringKey("val", "DOWNLOAD_LIST").Translate());
        game = Game.Get();
        remoteQuests = new List<RemoteQuest>();
        string remoteManifest = GetServerLocation() + "manifest.ini";
        HTTPManager.Get(remoteManifest, DownloadManifest_callback);
    }

    /// <summary>
    /// Get the default server list location
    /// </summary>
    /// <returns>the path to the remote files</returns>
    public static string GetServerLocation()
    {
        string[] text = File.ReadAllLines(ContentData.ContentPath() + "../text/download.txt");
        return text[0] + Game.Get().gameType.TypeName() + Path.DirectorySeparatorChar;
    }

    /// <summary>
    /// Parse the downloaded remote manifest and start download of individual quest files
    /// </summary>
    public void DownloadManifest_callback(string data, bool error)
    {
        if (error)
        {
            // Hide loading screen
            Destroyer.Dialog();

            // draw error message
            float error_string_width = 0;
            UIElement ui = new UIElement();
            if (data == "ERROR NETWORK")
            {
                StringKey ERROR_NETWORK = new StringKey("val", "ERROR_NETWORK");
                ui.SetText(ERROR_NETWORK, Color.red);
                error_string_width = ui.GetStringWidth(ERROR_NETWORK, UIScaler.GetMediumFont());
            }
            else
            {
                StringKey ERROR_HTTP = new StringKey("val", "ERROR_HTTP", game.stats.error_download_description);
                ui.SetText(ERROR_HTTP, Color.red);
                error_string_width = ui.GetStringWidth(ERROR_HTTP, UIScaler.GetMediumFont());
            }
            ui.SetLocation(UIScaler.GetHCenter() - (error_string_width / 2f), UIScaler.GetVCenter(), error_string_width, 2.4f);
            ui.SetTextAlignment(TextAnchor.MiddleCenter);
            ui.SetFontSize(UIScaler.GetLargeFont());
            ui.SetBGColor(Color.clear);

            // draw return button
            ui = new UIElement();
            ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
            ui.SetText(CommonStringKeys.BACK, Color.red);
            ui.SetButton(delegate { Cancel(); });
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            new UIElementBorder(ui, Color.red);

            return;
        }

        IniData remoteManifest = IniRead.ReadFromString(data);
        foreach (KeyValuePair<string, Dictionary<string, string>> kv in remoteManifest.data)
        {
            remoteQuests.Add(new RemoteQuest(kv));
        }
        DownloadQuestFiles();
    }

    /// <summary>
    /// Called to check if there are any more quest components to download, when finished Draw content
    /// </summary>
    public void DownloadQuestFiles()
    {
        foreach (RemoteQuest rq in remoteQuests)
        {
            if (!rq.FetchContent(this, DownloadQuestFiles)) return;
        }
        DrawList();
    }

    /// <summary>
    /// Draw download options screen
    /// </summary>
    public void DrawList()
    {
        Destroyer.Dialog();
        localManifest = IniRead.ReadFromString("");
        if (File.Exists(saveLocation() + "/manifest.ini"))
        {
            localManifest = IniRead.ReadFromIni(saveLocation() + "/manifest.ini");
        }

        // Heading
        UIElement ui = new UIElement();
        ui.SetLocation(2, 1, UIScaler.GetWidthUnits() - 4, 3);
        ui.SetText(new StringKey("val", "QUEST_NAME_DOWNLOAD", game.gameType.QuestName()));
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetLargeFont());

        UIElementScrollVertical scrollArea = new UIElementScrollVertical();
        scrollArea.SetLocation(1, 5, UIScaler.GetWidthUnits() - 2f, 21f);
        new UIElementBorder(scrollArea);

        // Start here
        float offset = 0;
        // Loop through all available quests
        foreach (RemoteQuest rq in remoteQuests)
        {
            string file = rq.name + ".valkyrie";
            string questName = rq.GetData("name." + game.currentLang);
            if (questName.Length == 0)
            {
                questName = rq.GetData("name." + rq.GetData("defaultlanguage"));
            }
            if (questName.Length == 0)
            {
                questName = rq.name;
            }

            int remoteFormat = 0;
            int.TryParse(rq.GetData("format"), out remoteFormat);
            bool formatOK = (remoteFormat >= QuestData.Quest.minumumFormat) && (remoteFormat <= QuestData.Quest.currentFormat);

            if (!formatOK) continue;

            bool exists = File.Exists(saveLocation() + Path.DirectorySeparatorChar + file);
            bool update = true;
            if (exists)
            {
                string localHash = localManifest.Get(rq.name, "version");
                string remoteHash = rq.GetData("version");

                update = !localHash.Equals(remoteHash);
            }

            bool has_stats_bar = false;


            Color bg = Color.white;
            Color text_color = Color.black;
            if (exists)
            {
                if (update)
                {
                    // light pink
                    bg = new Color(0.7f, 0.7f, 1f);
                    text_color = Color.black;
                }
                else
                {
                    // dark grey
                    bg = new Color(0.1f, 0.1f, 0.1f);
                    text_color = Color.grey;
                }
            }

            // Frame
            ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetLocation(0.95f, offset, UIScaler.GetWidthUnits() - 4.9f, 3.6f);
            ui.SetBGColor(bg);
            if (update) ui.SetButton(delegate { Selection(rq); });
            offset += 0.05f;
            new UIElementBorder(ui, Color.grey);

            // Draw Image
            ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetLocation(1, offset, 3.5f, 3.5f);
            ui.SetBGColor(bg);
            if (update) ui.SetButton(delegate { Selection(rq); });

            if (rq.image != null)
            {
                ui.SetImage(rq.image);
            }

            ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetBGColor(Color.clear);
            ui.SetLocation(5, offset, UIScaler.GetWidthUnits() - 8, 2.5f);
            ui.SetTextPadding(1.2f);
            if (update && exists)
            {
                ui.SetText(new StringKey("val", "QUEST_NAME_UPDATE", questName), text_color);
            }
            else
            {
                ui.SetText(questName, text_color);
            }
            if (update) ui.SetButton(delegate { Selection(rq); });
            ui.SetTextAlignment(TextAnchor.MiddleLeft);
            ui.SetFontSize(Mathf.RoundToInt(UIScaler.GetSmallFont() * 1.4f));

            // Duration
            int lengthMax = 0;
            int.TryParse(rq.GetData("lengthmax"), out lengthMax);
            if (lengthMax > 0)
            {
                int lengthMin = 0;
                int.TryParse(rq.GetData("lengthmin"), out lengthMin);


                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(7f, offset + 2.3f, 4, 1);
                ui.SetText(new StringKey("val", "DURATION"), text_color);
                ui.SetTextAlignment(TextAnchor.MiddleLeft);
                ui.SetBGColor(Color.clear);
                if (update) ui.SetButton(delegate { Selection(rq); });

                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(11f, offset + 2.3f, 5, 1);
                ui.SetText(lengthMin + "  -  " + lengthMax, text_color);
                ui.SetTextAlignment(TextAnchor.MiddleLeft);
                ui.SetBGColor(Color.clear);
                if (update) ui.SetButton(delegate { Selection(rq); });

            }

            // Difficulty
            float difficulty = 0f;
            float.TryParse(rq.GetData("difficulty"), out difficulty);
            if (difficulty != 0)
            {
                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(UIScaler.GetHCenter() - 5.5f, offset + 2.3f, 6, 1);
                ui.SetText(new StringKey("val", "DIFFICULTY"), text_color);
                if (update) ui.SetButton(delegate { Selection(rq); });
                ui.SetTextAlignment(TextAnchor.MiddleRight);
                ui.SetBGColor(Color.clear);

                string symbol = "π"; // will
                if (game.gameType is MoMGameType)
                {
                    symbol = new StringKey("val", "ICON_SUCCESS_RESULT").Translate();
                }

                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(UIScaler.GetHCenter(), offset + 1.8f, 9, 2);
                ui.SetText(symbol + symbol + symbol + symbol + symbol, text_color);
                ui.SetBGColor(Color.clear);
                ui.SetFontSize(UIScaler.GetMediumFont());
                if (update) ui.SetButton(delegate { Selection(rq); });

                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(UIScaler.GetHCenter() + 1.05f + (difficulty * 6.9f), offset + 1.8f, (1 - difficulty) * 6.9f, 1.6f);
                Color filter = bg;
                filter.a = 0.7f;
                ui.SetBGColor(filter);
                if (update) ui.SetButton(delegate { Selection(rq); });

            }

            // Statistics
            string filename = file.ToLower();
            if (game.stats != null && game.stats.scenarios_stats != null && game.stats.scenarios_stats.ContainsKey(filename))
            {
                ScenarioStats q_stats = game.stats.scenarios_stats[filename];
                int win_ratio = (int)(q_stats.scenario_avg_win_ratio * 100);

                StringKey STATS_AVERAGE_WIN_RATIO    = new StringKey("val", "STATS_AVERAGE_WIN_RATIO", win_ratio);
                StringKey STATS_NO_AVERAGE_WIN_RATIO = new StringKey("val", "STATS_NO_AVERAGE_WIN_RATIO", win_ratio);
                StringKey STATS_NB_USER_REVIEWS      = new StringKey("val", "STATS_NB_USER_REVIEWS", q_stats.scenario_play_count);
                StringKey STATS_AVERAGE_DURATION     = new StringKey("val", "STATS_AVERAGE_DURATION", (int)(q_stats.scenario_avg_duration));
                StringKey STATS_NO_AVERAGE_DURATION  = new StringKey("val", "STATS_NO_AVERAGE_DURATION");

                //  rating
                string symbol = "★";
                if (game.gameType is MoMGameType)
                {
                    symbol = new StringKey("val", "ICON_TENTACLE").Translate();
                }
                float rating = q_stats.scenario_avg_rating / 10;
                float score_text_width = 0;

                ui = new UIElement(scrollArea.GetScrollTransform());

                ui.SetText(symbol + symbol + symbol + symbol + symbol, text_color);
                score_text_width = ui.GetStringWidth(symbol + symbol + symbol + symbol + symbol, (int)System.Math.Round(UIScaler.GetMediumFont() * 1.4f)) + 1;
                ui.SetLocation(UIScaler.GetRight(-12f), offset + 0.6f, score_text_width, 2);
                ui.SetBGColor(Color.clear);
                ui.SetFontSize((int)System.Math.Round(UIScaler.GetMediumFont() * 1.4f));
                ui.SetTextAlignment(TextAnchor.MiddleLeft);
                if (update) ui.SetButton(delegate { Selection(rq); });

                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(UIScaler.GetRight(-12) + (rating * (score_text_width - 1)), offset + 0.6f, (1 - rating) * score_text_width, 2);
                Color filter = bg;
                filter.a = 0.7f;
                ui.SetBGColor(filter);
                if (update) ui.SetButton(delegate { Selection(rq); });

                //  Number of user reviews
                float user_review_text_width = 0;
                ui = new UIElement(scrollArea.GetScrollTransform());
                user_review_text_width = ui.GetStringWidth(STATS_NB_USER_REVIEWS, UIScaler.GetSmallFont()) + 1;
                ui.SetText(STATS_NB_USER_REVIEWS, text_color);
                ui.SetLocation(UIScaler.GetRight(-12) + (score_text_width / 2) - (user_review_text_width / 2), offset + 2.3f, user_review_text_width, 1);
                ui.SetTextAlignment(TextAnchor.MiddleLeft);
                ui.SetBGColor(Color.clear);
                ui.SetFontSize(UIScaler.GetSmallFont());
                if (update) ui.SetButton(delegate { Selection(rq); });

                if (q_stats.scenario_avg_duration > 0 || win_ratio >= 0)
                {
                    has_stats_bar = true;

                    // Additional information in frame
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(3.5f + 1f, offset + 3.6f, UIScaler.GetWidthUnits() - 4.9f - 3.5f - 0.05f, 1.2f);
                    if (exists)
                        ui.SetBGColor(bg);
                    else
                        ui.SetBGColor(new Color(0.8f, 0.8f, 0.8f));
                    if (update) ui.SetButton(delegate { Selection(rq); });

                    //  average duration
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(6f, offset + 3.8f, 14, 1);
                    if (q_stats.scenario_avg_duration > 0)
                        ui.SetText(STATS_AVERAGE_DURATION, text_color);
                    else
                        ui.SetText(STATS_NO_AVERAGE_DURATION, text_color);
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetBGColor(Color.clear);
                    if (update) ui.SetButton(delegate { Selection(rq); });

                    //  average win ratio
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(UIScaler.GetHCenter() - 5.5f, offset + 3.8f, 15, 1);
                    if (win_ratio >= 0)
                        ui.SetText(STATS_AVERAGE_WIN_RATIO, text_color);
                    else
                        ui.SetText(STATS_NO_AVERAGE_WIN_RATIO, text_color);
                    ui.SetBGColor(Color.clear);
                    ui.SetTextAlignment(TextAnchor.MiddleCenter);
                    if (update) ui.SetButton(delegate { Selection(rq); });
                }
            }

            // Size is 1.2 to be clear of characters with tails
            if (exists)
            {
                float string_width = 0;

                if (update)
                {
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetText(CommonStringKeys.UPDATE, Color.black);
                    string_width = ui.GetStringWidth(CommonStringKeys.UPDATE, UIScaler.GetSmallFont()) + 1.3f;
                    ui.SetButton(delegate { Delete(file); Selection(rq); });
                    ui.SetLocation(0.95f, offset + 3.6f, string_width, 1.2f);
                    ui.SetBGColor(new Color(0, 0.5f, 0.68f)); // 0080AF  
                    new UIElementBorder(ui, new Color(0, 0.3f, 0.43f));  // 00516f
                }

                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetText(CommonStringKeys.DELETE, Color.black);
                string_width = ui.GetStringWidth(CommonStringKeys.DELETE, UIScaler.GetSmallFont()) + 1.3f;
                ui.SetButton(delegate { Delete(file); });
                ui.SetLocation(0.95f + UIScaler.GetWidthUnits() - 4.9f - string_width, offset + 3.6f, string_width, 1.2f);
                ui.SetBGColor(new Color(0.7f, 0, 0));
                new UIElementBorder(ui, new Color(0.45f, 0, 0));
            }

            if (has_stats_bar || exists) offset += 1.2f;

            offset += 4.6f;
        }

        foreach (KeyValuePair<string, Dictionary<string, string>> kv in localManifest.data)
        {
            // Only looking for files missing from remote
            bool onRemote = false;
            foreach (RemoteQuest rq in remoteQuests)
            {
                if (rq.name.Equals(kv.Key)) onRemote = true;
            }
            if (onRemote) continue;

            string type = localManifest.Get(kv.Key, "type");

            // Only looking for packages of this game type
            if (!game.gameType.TypeName().Equals(type)) continue;

            string file = kv.Key + ".valkyrie";
            // Size is 1.2 to be clear of characters with tails
            if (File.Exists(saveLocation() + Path.DirectorySeparatorChar + file))
            {
                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(1, offset, UIScaler.GetWidthUnits() - 8, 1.2f);
                ui.SetTextPadding(1.2f);
                ui.SetText(file, Color.black);
                ui.SetBGColor(new Color(0.1f, 0.1f, 0.1f));
                ui.SetTextAlignment(TextAnchor.MiddleLeft);

                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(UIScaler.GetWidthUnits() - 12, offset, 8, 1.2f);
                ui.SetText(CommonStringKeys.DELETE, Color.black);
                ui.SetTextAlignment(TextAnchor.MiddleLeft);
                ui.SetButton(delegate { Delete(file); });
                ui.SetBGColor(new Color(0.7f, 0, 0));
                offset += 2;
            }
        }

        scrollArea.SetScrollSize(offset);

        ui = new UIElement();
        ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
        ui.SetText(CommonStringKeys.BACK, Color.red);
        ui.SetButton(delegate { Cancel(); });
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui, Color.red);
    }

    // Return to quest selection
    public void Cancel()
    {
        Destroyer.Dialog();
        // Get a list of available quests
        Dictionary<string, QuestData.Quest> ql = QuestLoader.GetQuests();

        // Pull up the quest selection page
        new QuestSelectionScreen(ql);
    }

    /// <summary>
    /// Select to download
    /// </summary>
    /// <param name="rq">Remote quest to download</param>
    public void Selection(RemoteQuest rq)
    {
        new LoadingScreen(download, new StringKey("val", "DOWNLOAD_PACKAGE").Translate());
        string package = rq.path + rq.name + ".valkyrie";
        StartCoroutine(Download(package, delegate { Save(rq); }));
    }

    /// <summary>
    /// Select to delete
    /// </summary>
    /// <param file="file">File name to delete</param>
    public void Delete(string file)
    {
        string toDelete = saveLocation() + Path.DirectorySeparatorChar + file;
        File.Delete(toDelete);
        Destroyer.Dialog();
        DrawList();
    }

    /// <summary>
    /// Called after download finished to save to disk
    /// </summary>
    /// <param name="rq">Remote quest to save</param>
    public void Save(RemoteQuest rq)
    {
        QuestLoader.mkDir(saveLocation());

        // Write to disk
        using (BinaryWriter writer = new BinaryWriter(File.Open(saveLocation() + Path.DirectorySeparatorChar + rq.name + ".valkyrie", FileMode.Create)))
        {
            writer.Write(download.bytes);
            writer.Close();
        }

        localManifest.Remove(rq.name);
        localManifest.Add(rq.name, rq.data);

        if (File.Exists(saveLocation() + "/manifest.ini"))
        {
            File.Delete(saveLocation() + "/manifest.ini");
        }
        File.WriteAllText(saveLocation() + "/manifest.ini", localManifest.ToString());

        Destroyer.Dialog();
        DrawList();
    }

    /// <summary>
    /// Get download directory without trailing '/'
    /// </summary>
    /// <returns>location to save packages</returns>
    public string saveLocation()
    {
        return ContentData.DownloadPath();
    }

    /// <summary>
    /// Download and call function
    /// </summary>
    /// <param name="file">Path to download</param>
    /// <param name="call">function to call on completion</param>
    public IEnumerator Download(string file, UnityEngine.Events.UnityAction call)
    {
        download = new WWW(file);
        yield return download;
        if (!string.IsNullOrEmpty(download.error))
        {
            // fixme not fatal
            ValkyrieDebug.Log("Error while downloading :" + file);
            ValkyrieDebug.Log(download.error);
            //Application.Quit();
        }
        call();
    }

    public class RemoteQuest
    {
        public string path;
        public string name;
        public Texture2D image;
        bool imageError = false;
        bool iniError = false;
        public Dictionary<string, string> data;

        /// <summary>
        /// Constuct remote quest object from manifest data
        /// </summary>
        /// <param name="kv">manifest data</param>
        public RemoteQuest(KeyValuePair<string, Dictionary<string, string>> kv)
        {
            name = kv.Key;
            path = QuestDownload.GetServerLocation();
            data = kv.Value;
            if (data.ContainsKey("external"))
            {
                path = data["external"];
            }
        }

        /// <summary>
        /// Extract manifest value
        /// </summary>
        /// <param name="key">manifest key</param>
        /// <returns>Value or empty string if not present</returns>
        public string GetData(string key)
        {
            if (data.ContainsKey(key)) return data[key];
            return "";
        }

        /// <summary>
        /// Fetch next required file
        /// </summary>
        /// <param name="qd">QuestDownloadObeject to use</param>
        /// <param name="call">Function to call after download</param>
        /// <returns>true if no downloads required</returns>
        public bool FetchContent(QuestDownload qd, UnityEngine.Events.UnityAction call)
        {
            if (data.ContainsKey("external"))
            {
                if (iniError) return true;

                qd.StartCoroutine(qd.Download(path + name + ".ini", delegate { IniFetched(qd.download, call); }));
                return false;
            }
            if (data.ContainsKey("image") && data["image"].Length > 0)
            {
                if (image == null && !imageError)
                {
                    string file = path + data["image"].Replace('\\', '/');
                    qd.StartCoroutine(qd.Download(file, delegate { ImageFetched(qd.download, call); }));
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Parse downloaded ini data
        /// </summary>
        /// <param name="download">download object</param>
        /// <param name="call">Function to call after parse</param>
        public void IniFetched(WWW download, UnityEngine.Events.UnityAction call)
        {
            if (download.error == null)
            {
                IniData remoteManifest = IniRead.ReadFromString(download.text);
                data = remoteManifest.Get("Quest");
            }
            else
            {
                iniError = true;
            }
            call();
        }

        /// <summary>
        /// Parse downloaded image data
        /// </summary>
        /// <param name="download">download object</param>
        /// <param name="call">Function to call after parse</param>
        public void ImageFetched(WWW download, UnityEngine.Events.UnityAction call)
        {
            if (download.error == null)
            {
                image = download.texture;
            }
            else
            {
                imageError = true;
            }
            call();
        }
    }
}
