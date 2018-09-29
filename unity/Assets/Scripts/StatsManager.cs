using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/*
 * Form : https://goo.gl/forms/jrC9oKh8EPMMdO2l2
 * 
 * Post URL: <form action="https://docs.google.com/forms/u/1/d/e/1FAIpQLSfiFPuQOTXJI54LI-WNvn1K6qCkM5xErxJdUUJRhCZthaIqcA/formResponse" target="_self" method="POST" id="mG61Hd">
 *
 * JSON : https://drive.google.com/uc?id=1lEhwFWrryzNH6DUMbte37G1p22SyDhu9&export=download
 * 
 */

[System.Serializable]
public class Stats_JSONobject
{
    public string FileGenerationDate;
    public List<ScenarioStats> scenarios_stats;
}

[System.Serializable]
public struct ScenarioStats
{
    /* We gather the following information for each scenario:
     *    scenario_name
     *    scenario_play_count  
     *    scenario_avg_rating  from 1 (awful) to 1O (amazing)
     *    scenario_avg_duration  in minutes
     *    scenario_avg_win_ratio from 0 to 1 (in percent)
     */
    public string scenario_name;
    public int scenario_play_count;
    public float scenario_avg_rating;
    public float scenario_avg_duration;
    public float scenario_avg_win_ratio;
}

class DataDownloader : MonoBehaviour
{

    private Uri uri = null;
    private Action<string,bool> callback_action;

    public void download(string url, Action<string, bool> action)
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
            Debug.Log("Error getting stats data : most probably a connectivity issue (please check your internet connection)");
        }
        else if (www_get.isHttpError)
        {
            // Most probably a connection error
            callback_action(www_get.error + " " + www_get.responseCode, true);
            Debug.Log("Error getting stats data : most probably a connection error (server error)");
        }
        else
        {
            // download OK
            callback_action(www_get.downloadHandler.text, false);
        }

    }
}

class PublishedGameStats
{
    /* Google Form is waiting for the following datas 
     * 
     *    Scenario ID/name (automatic)
     *    Victory (manual)
     *    Your rating for this scenario (1-10)  (manual)
     *    Optional: Comments / issue report / suggestion (manual)
     *    Game duration (automatic)
     *    Number of players (automatic)
     *    List of investigators (automatic)
     *    List of Events activated (automatic)
     */

    public string scenario_name = "";
    public string quest_name = "";
    public string victory = "";
    public string rating = "";
    public string comments = "";
    public int duration = 0;
    public int players_count = 0;
    public string investigators_list = "";
    public string language_selected = "";
    public string events_list = "";


    public void Reset()
    {
        scenario_name = "";
        victory = "";
        rating = "";
        comments = "";
        duration = 0;
        players_count = 0;
        investigators_list = "";
        language_selected = "";
        events_list = "";
    }

}

class GoogleFormPostman : MonoBehaviour
{
    private Uri uri = null;
    private WWWForm formFields = null;

    public void AddFormField(string name, string value)
    {
        if (formFields == null) formFields = new WWWForm();

        //Debug.Log("INFO: stats AddFormField"+ name+":"+value);

        formFields.AddField(name, value);
    }

    public void SetURL(string url)
    {
        uri = new Uri(url);
    }

    public void PostFormAsync()
    {
        StartCoroutine(PostForm());
    }

    private IEnumerator PostForm()
    {
        UnityWebRequest www = UnityWebRequest.Post(uri, formFields);

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            // Most probably a connectivity error
            Debug.Log("Error sending stats : most probably a connectivity issue (please check your internet connection)");
        }
        else if (www.isHttpError)
        {
            // Most probably a connection error
            Debug.Log("Error sending stats : most probably a connection error");
        }
        else
        {
            Debug.Log("INFO: stats sent");
            // success! thank you
        }

    }
}

// This class provides functions to submit game stats download game stats
public class StatsManager
{

    // stats for all scenario, downloaded from JSON
    public Stats_JSONobject stats_json;
    public Dictionary<string,ScenarioStats> scenarios_stats = null;

    public bool error_download = false;
    public string error_download_description = "";
    public bool download_ongoing = false;

    // stats for current scenario, to be submitted
    private PublishedGameStats gameStats = null;
    private GameObject network_post = null;
    private GameObject network_get = null;
    private GoogleFormPostman post_client = null;
    private DataDownloader json_client = null;

    public void PrepareStats(string victory, int rating, string comments)
    {
        if (gameStats == null) gameStats = new PublishedGameStats();

        gameStats.Reset();

        gameStats.victory = victory;

        if (rating == 0)
            gameStats.rating = "not set";
        else
            gameStats.rating = rating.ToString();

        gameStats.comments = comments;

        Game game = Game.Get();
        Quest quest = game.quest;

        // quest filename is the unique id
        gameStats.scenario_name = Path.GetFileName(game.quest.originalPath);

        // language is required to see the quality of translations
        gameStats.language_selected = game.currentLang;

        // Get number of heroes
        foreach (Quest.Hero h in quest.heroes)
        {
            if (h.heroData != null)
            {
                gameStats.players_count++;
                // remove leading 'hero' before hero name
                gameStats.investigators_list += h.heroData.sectionName.Remove(0, 4) + ";";
            }
        }

        // Get the list of events
        if (quest.eventList != null)
        {
            foreach (string event_name in game.quest.eventList)
            {
                gameStats.events_list += event_name + ";";
            }
        }
        else
        {
            gameStats.events_list = "no event (old save?)";
        }

        // max cell size of Google sheet is 50k characters
        if (gameStats.events_list.Length > 50000)
        {
            gameStats.events_list.Remove(88 + (gameStats.events_list.Length - 50000), 50000);
            gameStats.events_list = "---Beginning of event list is not included to avoid exceeding google sheet max size---" + gameStats.events_list;
        }

        if (quest.duration>=0)
        {
            TimeSpan current_duration = System.DateTime.UtcNow.Subtract(quest.start_time);
            gameStats.duration = quest.duration + (int)current_duration.TotalMinutes;
        } else
        {
            gameStats.duration = 0;
        }

        gameStats.quest_name = quest.qd.quest.name.Translate();

    }



    // send data to google forms
    public void PublishData()
    {
        if (network_post == null)
        {
            //Use WebClient Class to submit a new entry
            network_post = new GameObject("StatsManager");
            network_post.tag = Game.BG_TASKS;
        }

        if (post_client==null)
        { 
            post_client = network_post.AddComponent<GoogleFormPostman>();
        }


        post_client.AddFormField("entry.1875990408", gameStats.scenario_name);
        post_client.AddFormField("entry.989998412",  gameStats.quest_name);
        post_client.AddFormField("entry.84574628",   gameStats.victory);
        post_client.AddFormField("entry.227102998",  gameStats.rating);
        post_client.AddFormField("entry.2125749314", gameStats.comments);
        post_client.AddFormField("entry.170795919",  gameStats.duration.ToString());
        post_client.AddFormField("entry.376629889",  gameStats.players_count.ToString());
        post_client.AddFormField("entry.1150567176", gameStats.investigators_list);
        post_client.AddFormField("entry.2106598722", gameStats.language_selected);
        post_client.AddFormField("entry.1047979960", gameStats.events_list);

        post_client.SetURL("https://docs.google.com/forms/u/1/d/e/1FAIpQLSfiFPuQOTXJI54LI-WNvn1K6qCkM5xErxJdUUJRhCZthaIqcA/formResponse?hl=en");

        // submit:
        post_client.PostFormAsync();
    }


    // Download JSON
    public void DownloadStats()
    {
        if (network_get == null)
        {
            //Use WebClient Class to submit a new entry
            network_get = new GameObject("StatsManager");
            network_get.tag = Game.BG_TASKS;
        }

        if (json_client == null)
        {
            json_client = network_get.AddComponent<DataDownloader>();
        }

        download_ongoing = true;
        json_client.download("https://drive.google.com/uc?id=1lEhwFWrryzNH6DUMbte37G1p22SyDhu9&export=download", StatsDownload_callback);
    }

    private void StatsDownload_callback(string data, bool error)
    {
        download_ongoing = false;

        if (error)
        {
            error_download = true;
            error_download_description = data;
            return;
        }

        stats_json = JsonUtility.FromJson<Stats_JSONobject>(data);
        scenarios_stats = new Dictionary<string, ScenarioStats>();

        if(stats_json==null)
            Debug.Log("ERROR: Stat file is empty\n");

        foreach (ScenarioStats stats in stats_json.scenarios_stats)
        {
            scenarios_stats[stats.scenario_name] = stats;
//            Debug.Log("INFO:Stat filename: " + stats.scenario_name + "\n");
        }

    }

}