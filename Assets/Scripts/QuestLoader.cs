using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class QuestLoader {

    public static Dictionary<string, Quest> GetQuests()
    {
        string dataLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie";
        string questLocation = dataLocation + "/Quests";

        if (!Directory.Exists(dataLocation))
        {
            try
            {
                Directory.CreateDirectory(dataLocation);
            }
            catch (System.Exception)
            {
                Debug.Log("Error: Unable to create directory: " + dataLocation);
                Application.Quit();
            }
        }
        if (!Directory.Exists(questLocation))
        {
            try
            {
                Directory.CreateDirectory(questLocation);
            }
            catch (System.Exception)
            {
                Debug.Log("Error: Unable to create directory: " + questLocation);
                Application.Quit();
            }
        }

        Dictionary<string, Quest> quests = new Dictionary<string, Quest>();

        string[] questDirectories = Directory.GetDirectories(questLocation);
        foreach (string p in questDirectories)
        {
            // All packs must have a quest.ini, otherwise ignore
            if (File.Exists(p + "/quest.ini"))
            {
                Quest q = new Quest(p);
                if (!q.name.Equals(""))
                {
                    quests.Add(p, q);
                }
            }
        }

        return quests;
    }

    public class Quest
    {
        public string path;
        public string name = "";
        public string description;

        public Quest(string p)
        {
            path = p;
            IniData d = IniRead.ReadFromIni(p + "/quest.ini");
            if (d == null)
            {
                Debug.Log("Warning: Invalid quest:" + p + "/quest.ini!");
                return;
            }

            name = d.Get("Quest", "name");
            if (name.Equals(""))
            {
                Debug.Log("Warning: Failed to get name data out of " + p + "/content_pack.ini!");
                return;
            }

            // Missing description is OK
            description = d.Get("Quest", "description");
        }
    }
}
