using UnityEngine;
using System.Collections;
using System.IO;
using Assets.Scripts.UI.Screens;
using Assets.Scripts.Content;
using ValkyrieTools;

// Class for quest selection window
public class QuestDownload : MonoBehaviour
{
    public WWW download;
    public Game game;
    string key = "";

    void Start()
    {
        game = Game.Get();

        if(key=="")
        {
            Debug.Log("Download key is not set, this should not happen");
            return;
        }

        QuestData.Quest q = game.questsList.getQuestData(key);

        string package = q.package_url + key + ".valkyrie";
        StartCoroutine(Download(package, delegate { Save(key); }));
    }

    /// <summary>
    /// Set download key so it's available when start is called (after following Unity cycle )
    /// </summary>
    public void Download(string p_key)
    {
        key = p_key;
    }

    /// <summary>
    /// Called after download finished to save to disk
    /// </summary>
    /// <param name="key">Quest id</param>
    public void Save(string key)
    {
        QuestData.Quest q = game.questsList.getQuestData(key);
        QuestLoader.mkDir(saveLocation());

        // Write to disk
        using (BinaryWriter writer = new BinaryWriter(File.Open(saveLocation() + Path.DirectorySeparatorChar + key + ".valkyrie", FileMode.Create)))
        {
            writer.Write(download.bytes);
            writer.Close();
        }

        IniData localManifest = IniRead.ReadFromString("");
        if (File.Exists(saveLocation() + "/manifest.ini"))
        {
            localManifest = IniRead.ReadFromIni(saveLocation() + "/manifest.ini");
        }

        localManifest.Remove(key);

        IniData downloaded_quest = IniRead.ReadFromString(q.ToString());
        localManifest.Add(key, downloaded_quest.data["Quest"]);

        if (File.Exists(saveLocation() + "/manifest.ini"))
        {
            File.Delete(saveLocation() + "/manifest.ini");
        }
        File.WriteAllText(saveLocation() + "/manifest.ini", localManifest.ToString());

        // update quest status : downloaded/updated
        game.questsList.SetAvailable(key);

        Destroyer.Dialog();
        game.questSelectionScreen.Show();
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
        new LoadingScreen(download, new StringKey("val", "DOWNLOAD_PACKAGE").Translate());
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
}
