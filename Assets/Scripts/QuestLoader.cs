using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;

public class QuestLoader {

    public static Dictionary<string, Quest> GetQuests()
    {
        Dictionary<string, Quest> quests = new Dictionary<string, Quest>();

        string dataLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie";
        mkDir(dataLocation);
        List<string> questDirectories = GetQuests(dataLocation);


        if (Application.isEditor)
        {
            dataLocation = Application.dataPath + "/../../valkyrie-questdata/";
        }
        else
        {
            dataLocation = Application.dataPath + "/valkyrie-questdata/";
        }
        questDirectories.AddRange(GetQuests(dataLocation));

        questDirectories.AddRange(GetQuests(Path.GetTempPath() + "/Valkyrie"));

        foreach (string p in questDirectories)
        {
            Quest q = new Quest(p);
            if (!q.name.Equals(""))
            {
                quests.Add(p, q);
            }
        }

        return quests;
    }

    public static List<string> GetQuests(string path)
    {
        List<string> quests = new List<string>();

        if (!Directory.Exists(path))
        {
            return quests;
        }

        List<string> questDirectories = DirList(path);
        foreach (string p in questDirectories)
        {
            // All packs must have a quest.ini, otherwise ignore
            if (File.Exists(p + "/quest.ini"))
            {
                    quests.Add(p);
            }
        }

        string[] archives = Directory.GetFiles(path, "*.valkyrie", SearchOption.AllDirectories);
        foreach (string f in archives)
        {
            mkDir(Path.GetTempPath() + "/Valkyrie");
            string extractedPath = Path.GetTempPath() + "/Valkyrie/" + Path.GetFileName(f);
            if (Directory.Exists(extractedPath))
            {
                try
                {
                    Directory.Delete(extractedPath, true);
                }
                catch (System.Exception)
                {
                    Debug.Log("Warning: Unable to remove old temporary files: " + extractedPath);
                }
            }
            mkDir(extractedPath);

            try
            {
                ZipFile zip = ZipFile.Read(f);
                zip.ExtractAll(extractedPath);
            }
            catch (System.Exception)
            {
                Debug.Log("Warning: Unable to read file: " + extractedPath);
            }
        }

        return quests;
    }

    public static void mkDir(string p)
    {
        if (!Directory.Exists(p))
        {
            try
            {
                Directory.CreateDirectory(p);
            }
            catch (System.Exception)
            {
                Debug.Log("Error: Unable to create directory: " + p);
                Application.Quit();
            }
        }
    }

    public static List<string> DirList(string path)
    {
        return DirList(path, new List<string>());
    }

    public static List<string> DirList(string path, List<string> l)
    {
        List<string> list = new List<string>(l);

        foreach (string s in Directory.GetDirectories(path))
        {
            list = DirList(s, list);
            list.Add(s);
        }

        return list;
    }

    public static void CleanTemp()
    {
        // Nothing to do if no temporary files
        if(!Directory.Exists(Path.GetTempPath() + "/Valkyrie"))
        {
            return;
        }

        try
        {
            Directory.Delete(Path.GetTempPath() + "/Valkyrie", true);
        }
        catch (System.Exception)
        {
            Debug.Log("Warning: Unable to remove temporary files.");
        }
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
