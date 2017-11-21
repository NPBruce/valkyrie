using UnityEngine;
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
    Dictionary<string, Texture2D> textures;

    /// <summary>
    /// Download required files then draw screen
    /// </summary>
    void Start()
    {
        new LoadingScreen(new StringKey("val", "DOWNLOAD_LIST").Translate());
        game = Game.Get();
        textures = new Dictionary<string, Texture2D>();
        remoteQuests = new List<RemoteQuest>();
        string remoteManifest = GetServerLocation() + "manifest.ini";
        StartCoroutine(Download(remoteManifest, DownloadManifest));
    }

    /// <summary>
    /// Get the default server list location
    /// </summary>
    /// <returns>the path to the remote files</returns>
    public static string GetServerLocation()
    {
        string[] text = File.ReadAllLines(ContentData.ContentPath() + "../text/download.txt");
        return text[0] + Game.Get().gameType.TypeName() + "/";
    }

    /// <summary>
    /// Parse the downloaded remote manifest and start download of individual quest files
    /// </summary>
    public void DownloadManifest()
    {
        if (download.error != null) Application.Quit();
        IniData remoteManifest = IniRead.ReadFromString(download.text);
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
        string remoteDict = GetServerLocation() + "Localization.txt";
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

            bool exists = File.Exists(saveLocation() + "/" + file);
            bool update = true;
            if (exists)
            {
                string localHash = localManifest.Get(rq.name, "version");
                string remoteHash = rq.GetData("version");

                update = !localHash.Equals(remoteHash);
            }

            Color bg = Color.white;
            if (exists)
            {
                bg = new Color(0.7f, 0.7f, 1f);
                if (!update)
                {
                    bg = new Color(0.1f, 0.1f, 0.1f);
                }
            }

            // Frame
            ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetLocation(0.95f, offset, UIScaler.GetWidthUnits() - 4.9f, 3.1f);
            ui.SetBGColor(bg);
            if (update) ui.SetButton(delegate { Selection(rq); });
            offset += 0.05f;

            // Draw Image
            ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetLocation(1, offset, 3, 3);
            ui.SetBGColor(bg);
            if (update) ui.SetButton(delegate { Selection(rq); });

            if (rq.image != null)
            {
                ui.SetImage(rq.image);
            }

            ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetBGColor(Color.clear);
            ui.SetLocation(4, offset, UIScaler.GetWidthUnits() - 8, 3f);
            ui.SetTextPadding(1.2f);
            if (update && exists)
            {
                ui.SetText(new StringKey("val", "QUEST_NAME_UPDATE", questName), Color.black);
            }
            else
            {
                ui.SetText(questName, Color.black);
            }
            if (update) ui.SetButton(delegate { Selection(rq); });
            ui.SetTextAlignment(TextAnchor.MiddleLeft);
            ui.SetFontSize(Mathf.RoundToInt(UIScaler.GetSmallFont() * 1.3f));

            // Duration
            int lengthMax = 0;
            int.TryParse(rq.GetData("lengthmax"), out lengthMax);
            if (lengthMax > 0)
            {
                int lengthMin = 0;
                int.TryParse(rq.GetData("lengthmin"), out lengthMin);

                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(UIScaler.GetRight(-11), offset, 2, 1);
                ui.SetText(lengthMin.ToString(), Color.black);
                ui.SetBGColor(Color.clear);

                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(UIScaler.GetRight(-9), offset, 1, 1);
                ui.SetText("-", Color.black);
                ui.SetBGColor(Color.clear);

                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(UIScaler.GetRight(-8), offset, 2, 1);
                ui.SetText(lengthMax.ToString(), Color.black);
                ui.SetBGColor(Color.clear);
            }

            // Difficulty
            float difficulty = 0;
            float.TryParse(rq.GetData("difficulty"), out difficulty);
            if (difficulty != 0)
            {
                string symbol = "π"; // will
                if (game.gameType is MoMGameType)
                {
                    symbol = new StringKey("val", "ICON_SUCCESS_RESULT").Translate();
                }
                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(UIScaler.GetRight(-13), offset + 1, 9, 2);
                ui.SetText(symbol + symbol + symbol + symbol + symbol, Color.black);
                ui.SetBGColor(Color.clear);
                ui.SetFontSize(UIScaler.GetMediumFont());

                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(UIScaler.GetRight(-11.95f) + (difficulty * 6.9f), offset + 1, (1 - difficulty) * 6.9f, 2);
                Color filter = bg;
                filter.a = 0.7f;
                ui.SetBGColor(filter);
            }

            // Size is 1.2 to be clear of characters with tails
            if (exists)
            {
                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(((UIScaler.GetWidthUnits() - 3) / 2) - 4, offset + 2.5f, 8, 1.2f);
                ui.SetBGColor(new Color(0.7f, 0, 0));
                ui.SetText(CommonStringKeys.DELETE, Color.black);
                ui.SetButton(delegate { Delete(file); });
                offset += 0.5f;
            }
            offset += 4;
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
            if (File.Exists(saveLocation() + "/" + file))
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
        string toDelete = saveLocation() + "/" + file;
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
        using (BinaryWriter writer = new BinaryWriter(File.Open(saveLocation() + "/" + rq.name + ".valkyrie", FileMode.Create)))
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
    /// Get save directory without trailing '/'
    /// </summary>
    /// <returns>localtion to save packages</returns>
    public string saveLocation()
    {
        return Game.AppData() + "/Download";
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
                    qd.StartCoroutine(qd.Download(path + data["image"], delegate { ImageFetched(qd.download, call); }));
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
