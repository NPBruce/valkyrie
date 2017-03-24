using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.UI.Screens;

// Class for quest selection window
public class QuestDownload : MonoBehaviour
{
    public Dictionary<string, QuestLoader.Quest> questList;
    public WWW download;
    public string serverLocation = "https://raw.githubusercontent.com/NPBruce/valkyrie-store/master/";
    public Game game;
    IniData remoteManifest;
    IniData localManifest;

    void Start()
    {
        game = Game.Get();
        string remoteManifest = serverLocation + game.gameType.TypeName() + "/manifest.ini";
        StartCoroutine(Download(remoteManifest, delegate { ReadManifest(); }));
    }

    public void ReadManifest()
    {
        remoteManifest = IniRead.ReadFromString(download.text);

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
        DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 3), "Download " + game.gameType.QuestName());
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
        db.SetFont(game.gameType.GetHeaderFont());

        TextButton tb;
        // Start here
        int offset = 5;
        // Loop through all available quests
        // FIXME: this isn't paged Dictionary<string, Dictionary<string, string>> data;
        foreach (KeyValuePair<string, Dictionary<string, string>> kv in remoteManifest.data)
        {
            string file = kv.Key + ".valkyrie";
            // Size is 1.2 to be clear of characters with tails
            if (File.Exists(saveLocation() + "/" + file))
            {
                int localVersion = 0;
                int remoteVersion = 0;
                int.TryParse(localManifest.Get(kv.Key, "version"), out localVersion);
                int.TryParse(remoteManifest.Get(kv.Key, "version"), out remoteVersion);
                if (localVersion < remoteVersion)
                {
                    tb = new TextButton(new Vector2(2, offset), new Vector2(UIScaler.GetWidthUnits() - 4, 1.2f), "  [Update] " + kv.Value["name"], delegate { Selection(file); }, Color.blue, offset);
                    tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                    tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
                    tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0.1f);
                }
                else
                {
                    db = new DialogBox(new Vector2(2, offset), new Vector2(UIScaler.GetWidthUnits() - 4, 1.2f), "  " + kv.Value["name"], Color.grey);
                    db.AddBorder();
                    db.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.05f, 0.05f, 0.05f);
                    db.textObj.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
                }
            }
            else
            {
                tb = new TextButton(new Vector2(2, offset), new Vector2(UIScaler.GetWidthUnits() - 4, 1.2f), "  " + kv.Value["name"], delegate { Selection(file); }, Color.white, offset);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
                tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0.1f);
            }
            offset += 2;
        }

        tb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Back", delegate { Cancel(); }, Color.red);
        tb.SetFont(game.gameType.GetHeaderFont());
    }

    // Return to quest selection
    public void Cancel()
    {
        Destroyer.Dialog();
        // Get a list of available quests
        Dictionary<string, QuestLoader.Quest> ql = QuestLoader.GetQuests();

        // Pull up the quest selection page
        new QuestSelectionScreen(ql);
    }

    public void Selection(string file)
    {
        string package = serverLocation + game.gameType.TypeName() + "/" + file;
        StartCoroutine(Download(package, delegate { Save(file); }));
    }

    public void Save(string file)
    {
        QuestLoader.mkDir(saveLocation());

        // Write to disk
        using (BinaryWriter writer = new BinaryWriter(File.Open(saveLocation() + "/" + file, FileMode.Create)))
        {
            writer.Write(download.bytes);
            writer.Close();
        }
        Destroyer.Dialog();
        DrawList();
    }

    public string saveLocation()
    {
        return System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie/Download";
    }

    // Return to main menu
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
}
