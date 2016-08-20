using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class QuestData
{
    public List<Tile> tiles;
    List<string> files;
    Game game;

    public QuestData(string path, Game game_set)
    {
        Debug.Log("Loading quest from: \"" + path + "\"");

        game = game_set;

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
            d = IniRead.ReadFromIni(f);
            foreach (KeyValuePair<string, Dictionary<string, string>> section in d.data)
            {
                AddData(section.Key, section.Value, Path.GetDirectoryName(f));
            }
        }
    }

    void AddData(string name, Dictionary<string, string> content, string path)
    {
        if (name.IndexOf(Tile.type) == 0)
        {
            Tile c = new Tile(name, content, game);
            tiles.Add(c);
        }
    }

    public class Tile : QuestComponent
    {
        public TileSideData tileType;
        new public static string type = "Tile";
        public int rotation = 0;

        public Tile(string name, Dictionary<string, string> data, Game game) : base(name, data)
        {
            if (data.ContainsKey("rotation"))
            {
                rotation = int.Parse(data["rotation"]);
            }
            if (data.ContainsKey("side"))
            {
                tileType = game.cd.tileSides[data["side"]];
            }
        }
    }

    public class QuestComponent
    {
        public int x = 0;
        public int y = 0;
        public static string type = "";
        public bool visible = false;

        public QuestComponent(string name, Dictionary<string, string> data)
        {
            if (data.ContainsKey("xposition"))
            {
                x = int.Parse(data["xposition"]);
            }

            if (data.ContainsKey("yposition"))
            {
                y = int.Parse(data["yposition"]);
            }
        }
    }
}
