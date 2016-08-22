using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class QuestData
{
    public Dictionary<string, QuestComponent> components;
    public Event start;
    List<string> files;
    Game game;

    public QuestData(string path, Game game_set)
    {
        Debug.Log("Loading quest from: \"" + path + "\"");

        game = game_set;

        components = new Dictionary<string, QuestComponent>();

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
            components.Add(name, c);
        }
        if (name.IndexOf(Event.type) == 0)
        {
            Event c = new Event(name, content);
            components.Add(name, c);
            if (name.Equals("EventStart"))
                start = c;
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


            string imagePath = @"file://" + tileType.image;

            Sprite tileSprite;

            WWW www = new WWW(imagePath);
            Texture2D newTex = new Texture2D(256, 256, TextureFormat.DXT5, false);
            www.LoadImageIntoTexture(newTex);

            GameObject tile = new GameObject(name);

            Canvas[] canvii = GameObject.FindObjectsOfType<Canvas>();
            Canvas board = canvii[0];
            foreach(Canvas c in canvii)
            {
                if(c.name.Equals("BoardCanvas"))
                {
                    board = c;
                }
            }
            tile.transform.parent = board.transform;

            image = tile.AddComponent<UnityEngine.UI.Image>();
            tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            image.color = Color.clear;
            image.sprite = tileSprite;
            tile.transform.Translate(Vector3.right * ((newTex.width / 2) - tileType.left), Space.World);
            tile.transform.Translate(Vector3.down * ((newTex.height / 2) - tileType.top), Space.World);
            tile.transform.Translate(new Vector3((float)0.5, (float)0.5, 0) * 105, Space.World);
            image.rectTransform.sizeDelta = new Vector2(newTex.width, newTex.height);

            tile.transform.RotateAround(Vector3.zero, Vector3.forward, rotation);
            tile.transform.Translate(new Vector3(x, y, 0) * 105, Space.World);
            //image.color = Color.white;
        }
    }

    public class Event : QuestComponent
    {
        new public static string type = "Event";
        public string text = "";
        public string nextEvent = "";
        public string failEvent = "";
        public int gold = 0;
        public string[] addComponents;
        public string[] removeComponents;

        public Event(string name, Dictionary<string, string> data) : base(name, data)
        {
            if (data.ContainsKey("text"))
            {
                text = data["text"];
            }
            if (data.ContainsKey("event"))
            {
                nextEvent = data["event"];
            }
            if (data.ContainsKey("failevent"))
            {
                failEvent = data["failevent"];
            }
            if (data.ContainsKey("failevent"))
            {
                gold = int.Parse(data["failevent"]);
            }
            if (data.ContainsKey("add"))
            {
                addComponents = data["add"].Split(' ');
            }
            else
            {
                addComponents = new string[0];
            }
            if (data.ContainsKey("remove"))
            {
                removeComponents = data["removeComponents"].Split(' ');
            }
            else
            {
                removeComponents = new string[0];
            }
        }
    }

    public class QuestComponent
    {
        public float x = 0;
        public float y = 0;
        public static string type = "";
        public string name;
        public UnityEngine.UI.Image image;

        public QuestComponent(string nameIn, Dictionary<string, string> data)
        {
            name = nameIn;
            if (data.ContainsKey("xposition"))
            {
                x = float.Parse(data["xposition"]);
            }

            if (data.ContainsKey("yposition"))
            {
                y = float.Parse(data["yposition"]);
            }
        }

        public void setVisible(bool vis)
        {
            if (image == null)
                return;
            if (vis)
                image.color = Color.white;
            else
                image.color = Color.clear;
        }

        public bool getVisible()
        {
            if (image == null)
                return false;
            if (image.color.a == 0)
                return false;
            return true;
        }
    }
}
