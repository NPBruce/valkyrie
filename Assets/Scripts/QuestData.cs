using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// Class to manage all data for the current quest
public class QuestData
{
    // All components in the quest
    public Dictionary<string, QuestComponent> components;

    // A list of flags that have been set during the quest
    public List<string> flags;

    // A dictionary of heros that have been selected in events
    public Dictionary<string, List<Game.Hero>> heroSelection;

    // List of ini files containing quest data
    List<string> files;
    Game game;

    public QuestData(QuestLoader.Quest q)
    {
        LoadQuestData(q.path + "/quest.ini");
    }

    // Read all data files and populate components for quest
    public QuestData(string path)
    {
        LoadQuestData(path);
    }

    public void LoadQuestData(string path)
    {
        Debug.Log("Loading quest from: \"" + path + "\"");
        game = GameObject.FindObjectOfType<Game>();

        components = new Dictionary<string, QuestComponent>();
        flags = new List<string>();
        heroSelection = new Dictionary<string, List<Game.Hero>>();

        // Read the main quest file
        IniData d = IniRead.ReadFromIni(path);
        // Failure to read quest is fatal
        if(d == null)
        {
            Debug.Log("Failed to load quest from: \"" + path + "\"");
            Application.Quit();
        }

        // List of data files
        files = new List<string>();
        // The main data file is included
        files.Add(path);

        // Find others (no addition files is not fatal)
        if(d.Get("QuestData") == null)
        {
            Debug.Log("QuestData section missing in: \"" + path + "\"");
        }
        else
        {
            foreach (string file in d.Get("QuestData").Keys)
            {
                // path is relative to the main file (absolute not supported)
                files.Add(Path.GetDirectoryName(path) + "/" + file);
            }
        }

        foreach (string f in files)
        {
            // Read each file
            d = IniRead.ReadFromIni(f);
            // Failure to read a file is fatal
            if (d == null)
            {
                Debug.Log("Unable to read quest file: \"" + f + "\"");
                Application.Quit();
            }
            foreach (KeyValuePair<string, Dictionary<string, string>> section in d.data)
            {
                // Add the section to our quest data
                AddData(section.Key, section.Value, Path.GetDirectoryName(f));
            }
        }
    }

    // Add a section from an ini file to the quest data.  Duplicates are not allowed
    void AddData(string name, Dictionary<string, string> content, string path)
    {
        // Fatal error on duplicates
        if(components.ContainsKey(name))
        {
            Debug.Log("Duplicate component in quest: " + name);
            Application.Quit();
        }

        // Check for known types and create
        if (name.IndexOf(Tile.type) == 0)
        {
            Tile c = new Tile(name, content, game);
            components.Add(name, c);
        }
        if (name.IndexOf(Door.type) == 0)
        {
            Door c = new Door(name, content, game);
            components.Add(name, c);
        }
        if (name.IndexOf(Token.type) == 0)
        {
            Token c = new Token(name, content, game);
            components.Add(name, c);
        }
        if (name.IndexOf(Event.type) == 0)
        {
            Event c = new Event(name, content);
            components.Add(name, c);
        }
        if (name.IndexOf(Monster.type) == 0)
        {
            Monster c = new Monster(name, content, game);
            components.Add(name, c);
        }
        // If not known ignore
    }

    // Class for Tile components (use TileSide content data)
    public class Tile : QuestComponent
    {
        public TileSideData tileType;
        new public static string type = "Tile";
        public int rotation = 0;

        public Tile(string name, Dictionary<string, string> data, Game game) : base(name, data)
        {
            // Get rotation if specified
            if (data.ContainsKey("rotation"))
            {
                rotation = int.Parse(data["rotation"]);
            }
            
            // Find the tileside that is used
            if (data.ContainsKey("side"))
            {
                // 'TileSide' prefix is optional, test both
                if (game.cd.tileSides.ContainsKey(data["side"]))
                {
                    tileType = game.cd.tileSides[data["side"]];
                }
                else if (game.cd.tileSides.ContainsKey("TileSide" + data["side"]))
                {
                    tileType = game.cd.tileSides["TileSide" + data["side"]];
                }
                else
                {
                    // Fatal if not found
                    Debug.Log("Error: Failed to located TileSide: " + data["side"] + "in quest component: " + name);
                    Application.Quit();
                }
            }
            else
            {
                // Fatal if missing
                Debug.Log("Error: No TileSide specified in quest component: " + name);
                Application.Quit();
            }

            // Attempt to load image
            string imagePath = @"file://" + tileType.image;
            Sprite tileSprite;
            WWW www = null;
            Texture2D newTex = null;
            try
            {
                www = new WWW(imagePath);
                newTex = new Texture2D(256, 256, TextureFormat.DXT5, false);
                www.LoadImageIntoTexture(newTex);
            }
            catch (System.Exception)
            {
                // Fatal if missing
                Debug.Log("Error: cannot open image file for TileSide: " + imagePath);
                Application.Quit();
            }

            GameObject tile = new GameObject(name);

            // Locate board canvas to add tile
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

            // Add image to object
            image = tile.AddComponent<UnityEngine.UI.Image>();
            // Create sprite from texture
            tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            // Set to transparent initially
            image.color = new Color(1, 1, 1, 0);
            // Set image sprite
            image.sprite = tileSprite;
            // Move to get the top left square corner at 0,0
            tile.transform.Translate(Vector3.right * ((newTex.width / 2) - tileType.left), Space.World);
            tile.transform.Translate(Vector3.down * ((newTex.height / 2) - tileType.top), Space.World);
            // Move to get the middle of the top left square at 0,0 (squares are 105 units)
            tile.transform.Translate(new Vector3(-(float)0.5, (float)0.5, 0) * 105, Space.World);
            // Set the size to the image size (images are assumed to be 105px per square)
            image.rectTransform.sizeDelta = new Vector2(newTex.width, newTex.height);

            // Rotate around 0,0 rotation amount
            tile.transform.RotateAround(Vector3.zero, Vector3.forward, rotation);
            // Move tile into target location (spaces are 105 units, Space.World is needed because tile has been rotated)
            tile.transform.Translate(new Vector3(location.x, location.y, 0) * 105, Space.World);
        }
    }

    // Doors are like tokens but placed differently and have different defaults
    public class Door : Event
    {
        new public static string type = "Door";
        public int rotation = 0;
        public float[] colour = { 1f, 1f, 1f };
        public GameObject gameObject;

        public Door(string name, Dictionary<string, string> data, Game game) : base(name, data)
        {
            // Doors are cancelable because you can select then cancel
            cancelable = true;

            if (data.ContainsKey("rotation"))
            {
                rotation = int.Parse(data["rotation"]);
            }

            // color is only supported as a hexadecimal "#RRGGBB" format
            if (data.ContainsKey("color"))
            {
                if ((data["color"].Length != 7) || (data["color"][0] != '#'))
                {
                    Debug.Log("Warning: Door color must be in #RRGGBB format in: " + name);
                }
                else
                {
                    colour[0] = System.Convert.ToInt32(data["color"].Substring(1, 2), 16);
                    colour[1] = System.Convert.ToInt32(data["color"].Substring(3, 2), 16);
                    colour[2] = System.Convert.ToInt32(data["color"].Substring(5, 2), 16);
                }
            }

            if (text.Equals(""))
            {
                text = "You can open this door with an \"Open Door\" action.";
            }
        }

        public override void SetVisible(bool vis)
        {
            if(!vis)
            {
                GameObject go = GameObject.Find("Object" + name);
                if (go != null)
                {
                    Object.Destroy(go);
                }
                return;
            }

            Sprite tileSprite;
            Texture2D newTex = Resources.Load("sprites/door") as Texture2D;
            // Check load worked
            if (newTex == null)
            {
                Debug.Log("Error: Cannot load door image");
                Application.Quit();
            }

            // Create object
            gameObject = new GameObject("Object" + name);

            // Find the token canvas to add to
            Canvas[] canvii = GameObject.FindObjectsOfType<Canvas>();
            Canvas board = canvii[0];
            foreach (Canvas c in canvii)
            {
                if (c.name.Equals("TokenCanvas"))
                {
                    board = c;
                }
            }
            gameObject.transform.parent = board.transform;

            // Create the image
            image = gameObject.AddComponent<UnityEngine.UI.Image>();
            tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            // Set door colour
            image.color = new Color(colour[0] / 255, colour[1] / 255, colour[2] / 255, 1);
            image.sprite = tileSprite;
            image.rectTransform.sizeDelta = new Vector2(newTex.width, newTex.height);
            // Rotate as required
            gameObject.transform.RotateAround(Vector3.zero, Vector3.forward, rotation);
            // Move to square (105 units per square)
            gameObject.transform.Translate(new Vector3(-(float)0.5, (float)0.5, 0) * 105, Space.World);
            gameObject.transform.Translate(new Vector3(location.x, location.y, 0) * 105, Space.World);
            
            // Find the token canvas script object and add this as a button
            TokenCanvas tc = GameObject.FindObjectOfType<TokenCanvas>();
            tc.add(this);
        }
    }

    // Tokens are events that are tied to a token placed on the board
    public class Token : Event
    {
        new public static string type = "Token";
        public GameObject gameObject;

        public Token(string name, Dictionary<string, string> data, Game game) : base(name, data)
        {
            // Tokens are cancelable because you can select then cancel
            cancelable = true;

            // default token type is search, this is the image asset name
            string typeName = "search-token";
            if (data.ContainsKey("type"))
            {
                typeName = data["type"].ToLower();
            }

            Sprite tileSprite;
            Texture2D newTex = Resources.Load("sprites/tokens/" + typeName) as Texture2D;
            // Check if we can find the token image
            if (newTex == null)
            {
                Debug.Log("Warning: Quest component " + name + " is using missing token type: " + typeName);
                // Use search token instead
                newTex = Resources.Load("sprites/tokens/search-token") as Texture2D;
                // If we still can't load it then fatal error
                if (newTex == null)
                {
                    Debug.Log("Error: Cannot load search token \"sprites/tokens/search-token\"");
                    Application.Quit();
                }
            }

            // Create object
            gameObject = new GameObject(name);

            // Find the token canvas to add to
            Canvas[] canvii = GameObject.FindObjectsOfType<Canvas>();
            Canvas board = canvii[0];
            foreach (Canvas c in canvii)
            {
                if (c.name.Equals("TokenCanvas"))
                {
                    board = c;
                }
            }
            gameObject.transform.parent = board.transform;

            // Create the image
            image = gameObject.AddComponent<UnityEngine.UI.Image>();
            tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            image.color = new Color(1, 1, 1, 0);
            image.sprite = tileSprite;
            image.rectTransform.sizeDelta = new Vector2((int)((float)newTex.width * (float)0.8), (int)((float)newTex.height * (float)0.8));
            // Move to square (105 units per square)
            gameObject.transform.Translate(new Vector3(location.x, location.y, 0) * 105, Space.World);

            // Find the token canvas script object and add this as a button
            TokenCanvas tc = GameObject.FindObjectOfType<TokenCanvas>();
            tc.add(this);
        }
    }


    // Monster items are monster group placement events
    public class Monster : Event
    {
        new public static string type = "Monster";
        public MonsterData mData;

        public Monster(string name, Dictionary<string, string> data, Game game) : base(name, data)
        {
            // Monster type must be specified
            if (!data.ContainsKey("monster"))
            {
                Debug.Log("Error: No monster type specified in event: " + name);
                Application.Quit();
            }

            // Monster type must exist in content packs, 'Monster' is optional
            if (game.cd.monsters.ContainsKey(data["monster"]))
            {
                mData = game.cd.monsters[data["monster"]];
            }
            if (game.cd.monsters.ContainsKey("Monster"+data["monster"]))
            {
                mData = game.cd.monsters["Monster" + data["monster"]];
            }
            // Not found, throw error
            if(mData == null)
            {
                Debug.Log("Error: Unknown monster type: " + data["monster"] + " specified in event: " + name);
                Application.Quit();
            }
        }
    }


    // Events are used to create dialogs that control the quest
    public class Event : QuestComponent
    {
        new public static string type = "Event";
        public string text = "";
        public string trigger = "";
        public string[] nextEvent;
        public string[] failEvent;
        public string heroListName = "";
        public int gold = 0;
        public int minHeroes = 0;
        public int maxHeroes = 0;
        public string[] addComponents;
        public string[] removeComponents;
        public string[] flags;
        public string[] setFlags;
        public string[] clearFlags;
        public bool cancelable = false;

        public Event(string name, Dictionary<string, string> data) : base(name, data)
        {
            // Text to be displayed
            if (data.ContainsKey("text"))
            {
                text = data["text"];
            }

            // Events to trigger on confirm or success
            if (data.ContainsKey("event"))
            {
                nextEvent = data["event"].Split(' ');
            }
            else
            {
                nextEvent = new string[0];
            }

            // Events to trigger on confirm or success
            if (data.ContainsKey("failevent"))
            {
                failEvent = data["failevent"].Split(' ');
            }
            else
            {
                failEvent = new string[0];
            }

            // Heros from another event can be hilighted
            if (data.ContainsKey("hero"))
            {
                heroListName = data["hero"];
            }

            // alter party gold (currently unused)
            if (data.ContainsKey("gold"))
            {
                gold = int.Parse(data["gold"]);
            }
            
            // minimum heros required to be selected for event
            if (data.ContainsKey("minhero"))
            {
                minHeroes = int.Parse(data["minhero"]);
            }

            // maximum heros selectable for event (0 disables)
            if (data.ContainsKey("maxhero"))
            {
                maxHeroes = int.Parse(data["maxhero"]);
            }

            // Display hidden components (space separated list)
            if (data.ContainsKey("add"))
            {
                addComponents = data["add"].Split(' ');
            }
            else
            {
                addComponents = new string[0];
            }

            // Hide components (space separated list)
            if (data.ContainsKey("remove"))
            {
                removeComponents = data["remove"].Split(' ');
            }
            else
            {
                removeComponents = new string[0];
            }

            // trigger event on condition
            if (data.ContainsKey("trigger"))
            {
                trigger = data["trigger"];
            }

            // Flags required to trigger (space separated list)
            if (data.ContainsKey("flags"))
            {
                flags = data["flags"].Split(' ');
            }
            else
            {
                flags = new string[0];
            }

            // Flags to set trigger (space separated list)
            if (data.ContainsKey("set"))
            {
                setFlags = data["set"].Split(' ');
            }
            else
            {
                setFlags = new string[0];
            }

            // Flags to clear trigger (space separated list)
            if (data.ContainsKey("clear"))
            {
                clearFlags = data["clear"].Split(' ');
            }
            else
            {
                clearFlags = new string[0];
            }
        }
    }

    // Super class for all quest components
    public class QuestComponent
    {
        // location on the board in squares
        public Vector2 location;
        // Has a location been speficied?
        public bool locationSpecified;
        // type for sub classes
        public static string type = "";
        // name of section in ini file
        public string name;
        // image for display
        public UnityEngine.UI.Image image;

        // Construct from ini data
        public QuestComponent(string nameIn, Dictionary<string, string> data)
        {
            name = nameIn;

            // Default to 0, 0 unless specified
            location = new Vector2(0, 0);
            locationSpecified = false;
            if (data.ContainsKey("xposition"))
            {
                locationSpecified = true;
                location.x = float.Parse(data["xposition"]);
            }

            if (data.ContainsKey("yposition"))
            {
                locationSpecified = true;
                location.y = float.Parse(data["yposition"]);
            }
        }

        // items are invisible by default, can toggle visibility
        virtual public void SetVisible(bool vis)
        {
            if (image == null)
                return;
            if (vis)
                image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
            else
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        }

        // return visibility of image
        virtual public bool GetVisible()
        {
            if (image == null)
                return false;
            if (image.color.a == 0)
                return false;
            return true;
        }
    }
}
