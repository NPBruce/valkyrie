using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ContentData {

    ContentPack[] AllPacks;

    ContentData(string path)
    {
        string[] contentDirectories = Directory.GetDirectories(path);

        foreach(string p in contentDirectories)
        {
            if (File.Exists(p + "content_pack.ini"))
            {
                ContentPack pack = new ContentPack();
                IniData d = IniRead.ReadFromIni(p + "content_pack.ini");
                pack.name = d.Get("ContentPack", "name");
                pack.image = p + d.Get("ContentPack", "image");
                pack.description = d.Get("ContentPack", "description");
                List<string> files = new List<string>(d.Get("ContentPackData").Keys);
                files.Add(p + "content_pack.ini");
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

