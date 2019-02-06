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

        QuestData.Quest q = game.questsList.GetQuestData(key);

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
        // in case of error during download, do nothing
        if (!string.IsNullOrEmpty(download.error) || download.bytesDownloaded <= 0)
            return;

        // Write to disk
        QuestLoader.mkDir(ContentData.DownloadPath());
        using (BinaryWriter writer = new BinaryWriter(File.Open(ContentData.DownloadPath() + Path.DirectorySeparatorChar + key + ".valkyrie", FileMode.Create)))
        {
            writer.Write(download.bytes);
            writer.Close();
        }

        // update local list of quest and current status 
        game.questsList.SetQuestAvailability(key, true);

        // cleanup screen and go back to list of quests
        Destroyer.Dialog();
        game.questSelectionScreen.Show();
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
