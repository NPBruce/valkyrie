using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class QuestData
{
    List<Tile> tiles;
    List<string> files;

    public QuestData(string path)
    {
        Debug.Log("Loading quest from: \"" + path + "\"");

        tiles = new List<Tile>();

        IniData d = IniRead.ReadFromIni(path);
        files = new List<string>();
        files.Add(path);
        foreach (string file in d.Get("QuestData").Keys)
        {
            files.Add(Path.GetDirectoryName(path) + "/" + file);
        }

        foreach (string f in files)
        {
            d = IniRead.ReadFromIni(path);
            LoadQuestData(d);
        }
    }

    void LoadQuestData(IniData d)
    {
    }

    class Tile
    {
        int x;
        int y;
        bool visible = false;
        TileSideData type;
    }
}
