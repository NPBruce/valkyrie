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
    public static string serverLocation = "https://raw.githubusercontent.com/NPBruce/valkyrie-store/";
    public Game game;
    List<RemoteQuest> remoteQuests = new List<RemoteQuest>();
    IniData localManifest;
    DictionaryI18n localizationDict;
    Dictionary<string, Texture2D> textures;

    void Start()
    {
        game = Game.Get();
        textures = new Dictionary<string, Texture2D>();
        // For development builds use the development branch of the store
        if (char.IsNumber(game.version[game.version.Length - 1]))
        {
            serverLocation += "master/";
        }
        else
        {
            serverLocation += "development/";
        }
        string remoteManifest = serverLocation + game.gameType.TypeName() + "/manifest.ini";
        StartCoroutine(Download(remoteManifest, DownloadManifest));
    }

    public void DownloadManifest()
    {
        IniData remoteManifest = IniRead.ReadFromString(download.text);
        foreach (KeyValuePair<string, Dictionary<string, string>> kv in remoteManifest.data)
        {
            remoteQuests.Add(new RemoteQuest(kv));
        }
        DownloadQuestFiles();
    }

    public void DownloadQuestFiles()
    {
        foreach (RemoteQuest rq in remotequests)
        {
            if(FetchContent(this, DownloadQuestFiles))
            {
                string remoteDict = serverLocation + game.gameType.TypeName() + "/Localization.txt";
                StartCoroutine(Download(remoteDict, ReadDict));
            }
        }
    }

    public void ReadDict()
    {
        localizationDict = LocalizationRead.ReadFromString(download.text, DictionaryI18n.DEFAULT_LANG, game.currentLang);
        DrawList();
    }

    public void DrawList()
    {
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
            LocalizationRead.AddDictionary("qst", localizationDict);
            string questName = new StringKey("qst", rq.name + ".name").Translate();

            int remoteFormat = 0;
            int.TryParse(rq.GetData("format"), out remoteFormat);
            bool formatOK = (remoteFormat >= QuestData.Quest.minumumFormat) && (remoteFormat <= QuestData.Quest.currentFormat);

            if (!formatOK) continue;

            bool exists = File.Exists(saveLocation() + "/" + file);
            bool update = true;
            if (exists)
            {
                string localHash = localManifest.Get(kv.Key, "version");
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

            if (if (rq.image != null)
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
                ui.SetLocation(UIScaler.GetRight(-12), offset + 1, 7, 2);
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

    public void Selection(RemoteQuest rq)
    {
        string package = rq.path + rq.name + ".valkyrie";
        StartCoroutine(Download(package, delegate { Save(rq); }));
    }

    public void Delete(string file)
    {
        string toDelete = saveLocation() + "/" + file;
        File.Delete(toDelete);
        Destroyer.Dialog();
        DrawList();
    }

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

    public string saveLocation()
    {
        return Game.AppData() + "/Download";
    }

    public IEnumerator Download(string file, UnityEngine.Events.UnityAction call)
    {
        download = new WWW(file);
        yield return download;
        if (!string.IsNullOrEmpty(download.error))
        {
            // fixme not fatal
            ValkyrieDebug.Log(download.error);
            Application.Quit();
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

        public RemoteQuest(KeyValuePair<string, Dictionary<string, string>> kv)
        {
            name = kv.Key;
            path = QuestDownload.serverLocation + Game.Get().gameType.TypeName();
            data = kv.Value;
            if (data.ContainsKey("external"))
            {
                path = data["external"];
            }
        }

        public string GetData(string key)
        {
            if (data.ContainsKey(key)) return data[key];
            return "";
        }

        public bool FetchContent(QuestDownload qd, UnityEngine.Events.UnityAction call)
        {
            if (data.ContainsKey("external"))
            {
                if (iniError) return true;

                public StartCoroutine(qd.Download(path + name + ".ini", delegate { IniFetched(qd.download, call); }));
                return false;
            }
            if (data.ContainsKey("image") && data["image"].length > 0)
            {
                if (image == null && !imageError)
                {
                    public StartCoroutine(qd.Download(path + data["image"], delegate { ImageFetched(qd.download, call); }));
                    return false;
                }
            }
            return true;
        }

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
            call;
        }

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
            call;
        }
    }
}
