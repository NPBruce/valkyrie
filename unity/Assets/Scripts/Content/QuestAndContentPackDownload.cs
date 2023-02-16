using UnityEngine;
using System.Collections;
using System.IO;
using Assets.Scripts.UI.Screens;
using Assets.Scripts.Content;
using ValkyrieTools;
using Assets.Scripts;
using System.Linq;

// Class for quest selection window
public class QuestAndContentPackDownload : MonoBehaviour
{
    public WWW download;
    public Game game;
    string key = "";
    bool isContentPack;

    void Start()
    {
        game = Game.Get();

        if(key=="")
        {
            Debug.Log("Download key is not set, this should not happen");
            return;
        }

        if(isContentPack)
        {
            var q = game.remoteContentPackManager.remote_RemoteContentPack_data.Values.FirstOrDefault(p => p.identifier.Equals(key));
            if(q == null)
            {
                Debug.Log($"Could not find package {key} locally.");
                return;
            }

            string package = q.package_url + key + ValkyrieConstants.ContentPackDownloadContainerExtension;
            StartCoroutine(Download(package, delegate { Save(key, isContentPack); }));
        }
        else
        {
            QuestData.Quest q = game.questsList.GetQuestData(key);

            string package = q.package_url + key + ValkyrieConstants.ScenarioDownloadContainerExtension;
            StartCoroutine(Download(package, delegate { Save(key, isContentPack); }));
        }        
    }

    /// <summary>
    /// Set download key so it's available when start is called (after following Unity cycle )
    /// </summary>
    public void Download(string p_key, bool p_isContainer)
    {
        key = p_key;
        isContentPack = p_isContainer;
    }

    /// <summary>
    /// Called after download finished to save to disk
    /// </summary>
    /// <param name="key">Quest id</param>
    public void Save(string key, bool isContentPack)
    {
        // in case of error during download, do nothing
        if (!string.IsNullOrEmpty(download.error) || download.bytesDownloaded <= 0)
            return;

        // Write to disk
        string path = ContentData.DownloadPath();
        if(isContentPack)
        {
            path = ContentData.CustomContentPackPath();
        }
        else
        {
            path = ContentData.DownloadPath();
        }

        ExtractManager.mkDir(path);

        string filePath;
        if(isContentPack)
        {
            filePath = path + Path.DirectorySeparatorChar + key + ValkyrieConstants.ContentPackDownloadContainerExtension;
        }
        else
        {
            filePath = path + Path.DirectorySeparatorChar + key + ValkyrieConstants.ScenarioDownloadContainerExtension;
        }

        using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
        {
            writer.Write(download.bytes);
            writer.Close();
        }

        //TODO update local list of content packs and current status 
        if(isContentPack)
        {
            game.remoteContentPackManager.SetContentPackAvailability(key, true);
        }
        else
        {
            game.questsList.SetQuestAvailability(key, true);
        }        

        // cleanup screen and go back to parent screen
        Destroyer.Dialog();
        if(isContentPack)
        {
            ContentLoader.AddNewContentPack(game, filePath);
            new ContentSelectDownloadScreen();
        }
        else
        {
            game.questSelectionScreen.Show();
        }
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
