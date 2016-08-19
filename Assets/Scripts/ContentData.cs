using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ContentData {

    List<ContentPack> allPacks;
    public Dictionary<string, TileSideData> tileSides;

    public ContentData(string path)
    {
        Debug.Log("Searching for content in: \"" + path + "\"");
        string[] contentDirectories = Directory.GetDirectories(path);
        tileSides = new Dictionary<string, TileSideData>();

        allPacks = new List<ContentPack>();
        foreach(string p in contentDirectories)
        {
            if (File.Exists(p + "/content_pack.ini"))
            {
                ContentPack pack = new ContentPack();
                IniData d = IniRead.ReadFromIni(p + "/content_pack.ini");
                pack.name = d.Get("ContentPack", "name");
                pack.image = p + "/" + d.Get("ContentPack", "image");
                pack.description = d.Get("ContentPack", "description");
                List<string> files = new List<string>();
                files.Add(p + "/content_pack.ini");
                foreach(string file in  d.Get("ContentPackData").Keys)
                {
                    files.Add(p + "/" + file);
                }
                pack.iniFiles = files;
                allPacks.Add(pack);
            }
        }
   }


    public List<string> GetPacks()
    {
        List<string> names = new List<string>();
        foreach(ContentPack cp in allPacks)
        {
            names.Add(cp.name);
        }
        return names;
    }

    public void LoadContent(string name)
    {
        foreach (ContentPack cp in allPacks)
        {
            if(cp.name.Equals(name))
            {
                LoadContent(cp);
            }
        }
    }

    void LoadContent(ContentPack cp)
    {
        foreach(string ini in cp.iniFiles)
        {
            IniData d = IniRead.ReadFromIni(ini);
            foreach(KeyValuePair<string, Dictionary<string, string>> section in d.data)
            {
                AddContent(section.Key, section.Value, Path.GetDirectoryName(ini));
            }
        }
    }

    void AddContent(string name, Dictionary<string, string> content, string path)
    {
        if(name.IndexOf(TileSideData.type) == 0)
        {
            TileSideData d = new TileSideData(name, content, path);
            if(!tileSides.ContainsKey(d.name))
            {
                tileSides.Add(d.name, d);
            }
            else if(tileSides[d.name].priority < d.priority)
            {
                tileSides.Remove(d.name);
                tileSides.Add(d.name, d);
            }
        }
    }

    class ContentPack
    {
        public string name;
        public string image;
        public string description;
        public List<string> iniFiles;
    }
}

public class TileSideData : GenericData
{
    public int top;
    public int left;
    public static new string type = "TileSide";

    public TileSideData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
    {
        if (content.ContainsKey("top"))
        {
            top = int.Parse(content["top"]);
        }
        else
        {
            top = 0;
        }

        if (content.ContainsKey("left"))
        {
            priority = int.Parse(content["left"]);
        }
        else
        {
            left = 0;
        }
    }
}


public class GenericData
{
    public string name;
    public string[] traits;
    public string image;
    public int priority;
    public static string type = "";

    public GenericData(string name_ini, Dictionary<string, string> content, string path, string type)
    {
        if (content.ContainsKey("name"))
        {
            name = content["name"];
        }
        else
        {
            name = name_ini.Substring(type.Length);
        }
        if (content.ContainsKey("priority"))
        {
            priority = int.Parse(content["priority"]);
        }
        else
        {
            priority = 0;
        }

        if (content.ContainsKey("traits"))
        {
            traits = content["traits"].Split(" ".ToCharArray()) ;
        }
        else
        {
            traits = new string[0];
        }

        if (content.ContainsKey("image"))
        {
            image = path + "/" + content["image"];
        }
        else
        {
            image = "";
        }

    }
}