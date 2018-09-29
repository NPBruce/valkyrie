using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// HTTPManager is a simplified class to download/upload text files</summary>
/// <remarks>
/// Multiple files can be downloaded at the same time,  with one coroutine per download. Only one GameObject will be created for all connections.</remarks>
class HTTPManager
{
    private static GameObject network_go = null;

    /// <summary>
    /// Download a text file, and call action() when done.</summary>
    /// <param name="url">Url of the file to download</param>
    /// <param name="action">Callback with content in string or error in case of problem, and bool containing download status (true:success).</param>
    public static void Get(string url, Action<string, bool> action)
    {
        if (network_go == null)
        {
            network_go = new GameObject("NetworkManager");
            network_go.tag = Game.BG_TASKS;
        }

        //Use WebClient Class
        DataDownloader dd = network_go.AddComponent<DataDownloader>();

        dd.DownloadAsync(url, action);
    }

    /// <summary>
    /// HTTP POST content to a URL, and call action() when done.</summary>
    /// <param name="url">Url of the form to upload to</param>
    /// <param name="content">Content to POST</param>
    /// <param name="action">Callback with error in string in case of problem, and bool containing upload status (true:success).</param>
    public static void Upload(string url, WWWForm content, Action<string, bool> action)
    {
        if (network_go == null)
        {
            network_go = new GameObject("NetworkManager");
            network_go.tag = Game.BG_TASKS;
        }

        //Use WebClient Class
        DataUploader du = network_go.AddComponent<DataUploader>();

        du.PostFormAsync(url, content, action);
    }

}


class DataDownloader : MonoBehaviour
{
    private Uri uri = null;
    private Action<string, bool> callback_action;

    public void DownloadAsync(string url, Action<string, bool> action)
    {
        uri = new Uri(url);
        callback_action = action;

        StartCoroutine(GetData());
    }

    private IEnumerator GetData()
    {
        UnityWebRequest www_get = UnityWebRequest.Get(uri);
        yield return www_get.SendWebRequest();

        if (www_get.isNetworkError)
        {
            // Most probably a connection error
            callback_action("ERROR NETWORK", true);
            Debug.Log("Error downloading data : most probably a connectivity issue (please check your internet connection)");
        }
        else if (www_get.isHttpError)
        {
            // Most probably a connection error
            callback_action(www_get.error + " " + www_get.responseCode, true);
            Debug.Log("Error downloading data : most probably a connection error (server error)");
        }
        else
        {
            // download OK
            callback_action(www_get.downloadHandler.text, false);
        }
    }
}


class DataUploader : MonoBehaviour
{
    private Uri uri = null;
    private WWWForm formFields = null;
    private Action<string, bool> callback_action = null;

    public void PostFormAsync(string url, WWWForm content, Action<string, bool> action)
    {
        uri = new Uri(url);
        formFields = content;
        callback_action = action;

        StartCoroutine(PostForm());
    }

    private IEnumerator PostForm()
    {
        UnityWebRequest www_post = UnityWebRequest.Post(uri, formFields);

        yield return www_post.SendWebRequest();

        if (www_post.isNetworkError)
        {
            // Most probably a connection error
            callback_action("ERROR NETWORK", true);
            Debug.Log("Error uploading data : most probably a connectivity issue (please check your internet connection)");
        }
        else if (www_post.isHttpError)
        {
            // Most probably a connection error
            callback_action(www_post.error + " " + www_post.responseCode, true);
            Debug.Log("Error uploading data : most probably a connection error (server error)");
        }
        else
        {
            // download OK
            callback_action(www_post.downloadHandler.text, false);
        }

    }
}