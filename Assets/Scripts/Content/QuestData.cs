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
    public Dictionary<string, List<Round.Hero>> heroSelection;

    // List of ini files containing quest data
    List<string> files;

    // Location of the quest.ini file
    public string questPath = "";

    // Data from 'Quest' section
    public Quest quest;

    Game game;

    public QuestData(QuestLoader.Quest q)
    {
        questPath = q.path + "/quest.ini";
        LoadQuestData();
    }

    // Read all data files and populate components for quest
    public QuestData(string path)
    {
        questPath = path;
        LoadQuestData();
    }

    public void LoadQuestData()
    {
        Debug.Log("Loading quest from: \"" + questPath + "\"" + System.Environment.NewLine);
        game = Game.Get();

        components = new Dictionary<string, QuestComponent>();
        flags = new List<string>();
        heroSelection = new Dictionary<string, List<Round.Hero>>();

        // Read the main quest file
        IniData d = IniRead.ReadFromIni(questPath);
        // Failure to read quest is fatal
        if(d == null)
        {
            Debug.Log("Failed to load quest from: \"" + questPath + "\"");
            Application.Quit();
        }

        // List of data files
        files = new List<string>();
        // The main data file is included
        files.Add(questPath);

        // Find others (no addition files is not fatal)
        if(d.Get("QuestData") != null)
        {
            foreach (string file in d.Get("QuestData").Keys)
            {
                // path is relative to the main file (absolute not supported)
                files.Add(Path.GetDirectoryName(questPath) + "/" + file);
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

        if (name.Equals("Quest"))
        {
            quest = new Quest(content);
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
        if (name.IndexOf(MPlace.type) == 0)
        {
            MPlace c = new MPlace(name, content);
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

        public Tile(string s) : base(s)
        {
            locationSpecified = true;
            Game game = Game.Get();
            foreach (KeyValuePair<string, TileSideData> kv in game.cd.tileSides)
            {
                typeDynamic = type;
                tileType = kv.Value;
            }

        }

        public Tile(string name, Dictionary<string, string> data, Game game) : base(name, data)
        {
            locationSpecified = true;
            typeDynamic = type;
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
                    Debug.Log("Error: Failed to located TileSide: " + data["side"] + " in quest component: " + name);
                    Application.Quit();
                }
            }
            else
            {
                // Fatal if missing
                Debug.Log("Error: No TileSide specified in quest component: " + name);
                Application.Quit();
            }

            Draw();
        }

        public override void Draw()
        {
            GameObject go = GameObject.Find("Object" + name);
            if (go != null)
            {
                Object.Destroy(go);
            }

            Game game = Game.Get();

            // Attempt to load image
            string imagePath = tileType.image;
            Texture2D newTex = ContentData.FileToTexture(imagePath);
            Sprite tileSprite;
            if (newTex == null)
            {
                // Fatal if missing
                Debug.Log("Error: cannot open image file for TileSide: " + imagePath);
                Application.Quit();
            }

            GameObject tile = new GameObject("Object" + name);
            tile.tag = "board";
            tile.transform.parent = game.boardCanvas.transform;

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

        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            r += "side=" + tileType.sectionName + nl;
            if (rotation != 0)
            {
                r += "rotation=" + rotation + nl;
            }
            return r;
        }
    }

    // Doors are like tokens but placed differently and have different defaults
    public class Door : Event
    {
        new public static string type = "Door";
        public int rotation = 0;
        public Color colour = Color.white;
        public GameObject gameObject;
        public string colourName = "white";

        public Door(string s) : base(s)
        {
            locationSpecified = true;
            typeDynamic = type;
        }

        public Door(string name, Dictionary<string, string> data, Game game) : base(name, data)
        {
            locationSpecified = true;
            typeDynamic = type;
            // Doors are cancelable because you can select then cancel
            cancelable = true;

            if (data.ContainsKey("rotation"))
            {
                rotation = int.Parse(data["rotation"]);
            }

            // color is only supported as a hexadecimal "#RRGGBB" format
            if (data.ContainsKey("color"))
            {
                colourName = data["color"];
                SetColor(colourName);
            }

            if (text.Equals(""))
            {
                text = "You can open this door with an \"Open Door\" action.";
            }
        }

        public void SetColor(string s)
        {
            colourName = s;
            string colorRGB = ColorUtil.FromName(s);
            if ((colorRGB.Length != 7) || (colorRGB[0] != '#'))
            {
                Debug.Log("Warning: Door color must be in #RRGGBB format or a known name in: " + name);
            }
            else
            {
                colour[0] = (float)System.Convert.ToInt32(colorRGB.Substring(1, 2), 16) / 255f;
                colour[1] = (float)System.Convert.ToInt32(colorRGB.Substring(3, 2), 16) / 255f;
                colour[2] = (float)System.Convert.ToInt32(colorRGB.Substring(5, 2), 16) / 255f;
            }
        }

        public override void Draw()
        {
            GameObject go = GameObject.Find("Object" + name);
            if (go != null)
            {
                Object.Destroy(go);
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
            gameObject.tag = "board";

            Game game = Game.Get();
            gameObject.transform.parent = game.tokenCanvas.transform;

            // Create the image
            image = gameObject.AddComponent<UnityEngine.UI.Image>();
            tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            // Set door colour
            image.color = new Color(colour[0], colour[1], colour[2], 1);
            image.sprite = tileSprite;
            image.rectTransform.sizeDelta = new Vector2(newTex.width, newTex.height);
            // Rotate as required
            gameObject.transform.RotateAround(Vector3.zero, Vector3.forward, rotation);
            // Move to square (105 units per square)
            gameObject.transform.Translate(new Vector3(-(float)0.5, (float)0.5, 0) * 105, Space.World);
            gameObject.transform.Translate(new Vector3(location.x, location.y, 0) * 105, Space.World);
        }

        public override void SetVisible(bool vis)
        {
            GameObject go = GameObject.Find("Object" + name);
            if (go != null)
            {
                Object.Destroy(go);
            }

            if (!vis) return;

            Draw();
            Game game = Game.Get();
            game.tokenBoard.add(this);
        }

        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            if (!colourName.Equals("white"))
            {
                r += "color=" + colourName + nl;
            }
            if (rotation != 0)
            {
                r += "rotation=" + rotation + nl;
            }
            return r;
        }
    }

    // Tokens are events that are tied to a token placed on the board
    public class Token : Event
    {
        new public static string type = "Token";
        public GameObject gameObject;
        public string spriteName;

        public Token(string s) : base(s)
        {
            locationSpecified = true;
            typeDynamic = type;
            spriteName = "search-token";
        }

        public Token(string name, Dictionary<string, string> data, Game game) : base(name, data)
        {
            locationSpecified = true;
            typeDynamic = type;
            // Tokens are cancelable because you can select then cancel
            cancelable = true;

            // default token type is search, this is the image asset name
            spriteName = "search-token";
            if (data.ContainsKey("type"))
            {
                spriteName = data["type"].ToLower();
            }
        }

        public override void Draw()
        {
            Sprite tileSprite;
            Texture2D newTex = Resources.Load("sprites/tokens/" + spriteName) as Texture2D;
            // Check if we can find the token image
            if (newTex == null)
            {
                Debug.Log("Warning: Quest component " + name + " is using missing token type: " + spriteName);
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
            gameObject = new GameObject("Object" + name);
            gameObject.tag = "board";

            Game game = Game.Get();
            gameObject.transform.parent = game.tokenCanvas.transform;

            // Create the image
            image = gameObject.AddComponent<UnityEngine.UI.Image>();
            tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            image.color = Color.white;
            image.sprite = tileSprite;
            image.rectTransform.sizeDelta = new Vector2((int)((float)newTex.width * (float)0.8), (int)((float)newTex.height * (float)0.8));
            // Move to square (105 units per square)
            gameObject.transform.Translate(new Vector3(location.x, location.y, 0) * 105, Space.World);
        }

        public override void SetVisible(bool vis)
        {
            GameObject go = GameObject.Find("Object" + name);
            if (go != null)
            {
                Object.Destroy(go);
            }
            if (!vis) return;

            Draw();

            Game game = Game.Get();
            game.tokenBoard.add(this);
        }

        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            if(!spriteName.Equals("search-token"))
            {
                r += "type=" + spriteName + nl;
            }
            return r;
        }
    }


    // Monster items are monster group placement events
    public class Monster : Event
    {
        new public static string type = "Monster";
        public MonsterData mData;
        public string[][] placement;
        public bool unique = false;
        public string uniqueTitle = "";
        public string uniqueTitleOriginal = "";
        public string uniqueText = "";
        public string[] mTypes;
        public string[] mTraits;

        public Monster(string s) : base(s)
        {
            locationSpecified = true;
            typeDynamic = type;
            Game game = Game.Get();
            foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
            {
                mData = kv.Value;
            }
            mTypes = new string[1];
            mTypes[0] = mData.sectionName;
            mTraits = new string[0];

            placement = new string[5][];
            for (int i = 0; i < placement.Length; i++)
            {
                placement[i] = new string[0];
            }
        }

        public Monster(string name, Dictionary<string, string> data, Game game) : base(name, data)
        {
            locationSpecified = true;
            typeDynamic = type;
            //First try to a list of specific types
            if (data.ContainsKey("monster"))
            {
                mTypes = data["monster"].Split(' ');
            }
            else
            {
                mTypes = new string[0];
            }

            // Next try to find a type that is valid
            foreach (string t in mTypes)
            {
                // Monster type must exist in content packs, 'Monster' is optional
                if (game.cd.monsters.ContainsKey(t) && mData == null)
                {
                    mData = game.cd.monsters[t];
                }
                else if (game.cd.monsters.ContainsKey("Monster" + t) && mData == null)
                {
                    mData = game.cd.monsters["Monster" + t];
                }
            }

            // If we didn't find anything try by trait
            mTraits = new string[0];
            if (mData == null)
            {
                if (data.ContainsKey("traits"))
                {
                    mTraits = data["traits"].Split(' ');
                }
                else
                {
                    Debug.Log("Error: Cannot find monster and no traits provided: " + data["monster"] + " specified in event: " + name);
                    Application.Quit();
                }

                List<MonsterData> list = new List<MonsterData>();
                foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
                {
                    bool allFound = true;
                    foreach (string t in mTraits)
                    {
                        bool found = false;
                        foreach (string mt in kv.Value.traits)
                        {
                            if (mt.Equals(t)) found = true;
                        }
                        if (found == false) allFound = false;
                    }
                    if (allFound)
                    {
                        list.Add(kv.Value);
                    }
                }

                // Not found, throw error
                if (list.Count == 0)
                {
                    Debug.Log("Error: Unable to find monster of traits specified in event: " + name);
                    Application.Quit();
                }

                mData = list[Random.Range(0, list.Count)];
            }
            text = text.Replace("{type}", mData.name);

            placement = new string[5][];
            for (int i = 0; i < placement.Length; i++)
            {
                placement[i] = new string[0];
                if (data.ContainsKey("placement" + i))
                {
                    placement[i] = data["placement" + i].Split(' ');
                }
            }

            if (data.ContainsKey("unique"))
            {
                unique = bool.Parse(data["unique"]);
            }
            if (data.ContainsKey("uniquetitle"))
            {
                uniqueTitleOriginal = data["uniquetitle"];
                uniqueTitle = uniqueTitleOriginal.Replace("{type}", mData.name);
            }
            if (uniqueTitle.Equals(""))
            {
                uniqueTitle = "Master " + mData.name;
            }
            if (data.ContainsKey("uniquetext"))
            {
                uniqueText = data["uniquetext"];
            }
        }

        override public void ChangeReference(string oldName, string newName)
        {
            for (int j = 0; j < placement.Length; j++)
            {
                for (int i = 0; i < placement[j].Length; i++)
                {
                    if (placement[j][i].Equals(oldName))
                    {
                        placement[j][i] = newName;
                    }
                }
                placement[j] = RemoveFromArray(placement[j], "");
            }
        }

        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            int textStart = r.IndexOf("text=");
            int textEnd = r.IndexOf("\n", textStart);
            r = r.Substring(0, textStart) + "text=\"" + originalText + "\"" + r.Substring(textEnd);

            if (mTypes.Length > 0)
            {
                r += "monster=";
                foreach (string s in mTypes)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            if (mTraits.Length > 0)
            {
                r += "traits=";
                foreach (string s in mTraits)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            for(int i = 0; i < placement.Length; i++)
            {
                if (placement[i].Length > 0)
                {
                    r += "placement" + i + "=";
                    foreach (string s in placement[i])
                    {
                        r += s + " ";
                    }
                    r = r.Substring(0, r.Length - 1) + nl;
                }
            }
            if(unique)
            {
                r += "unique=true" + nl;
            }
            if (!uniqueTitleOriginal.Equals(""))
            {
                r += "uniquetitle=\"" + uniqueTitleOriginal + "\"" + nl;
            }
            if (!uniqueText.Equals(""))
            {
                r += "uniquetext=\"" + uniqueText + "\"" + nl;
            }

            return r;
        }
    }


    // Events are used to create dialogs that control the quest
    public class Event : QuestComponent
    {
        new public static string type = "Event";
        public string text = "";
        public string originalText = "";
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
        public bool highlight = false;

        public Event(string s) : base(s)
        {
            typeDynamic = type;
            nextEvent = new string[0];
            failEvent = new string[0];
            addComponents = new string[0];
            removeComponents = new string[0];
            flags = new string[0];
            setFlags = new string[0];
            clearFlags = new string[0];
        }

        public Event(string name, Dictionary<string, string> data) : base(name, data)
        {
            typeDynamic = type;
            // Text to be displayed
            if (data.ContainsKey("text"))
            {
                text = data["text"];
            }
            originalText = text;

            // Should the target location by highlighted?
            if (data.ContainsKey("highlight"))
            {
                highlight = bool.Parse(data["highlight"]);
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

        override public void ChangeReference(string oldName, string newName)
        {
            if (heroListName.Equals(oldName))
            {
                heroListName = newName;
            }
            for (int i = 0; i < nextEvent.Length; i++)
            {
                if (nextEvent[i].Equals(oldName))
                {
                    nextEvent[i] = newName;
                }
            }
            nextEvent = RemoveFromArray(nextEvent, "");

            for (int i = 0; i < failEvent.Length; i++)
            {
                if (failEvent[i].Equals(oldName))
                {
                    failEvent[i] = newName;
                }
            }
            failEvent = RemoveFromArray(failEvent, "");
            for (int i = 0; i < addComponents.Length; i++)
            {
                if (addComponents[i].Equals(oldName))
                {
                    addComponents[i] = newName;
                }
            }
            addComponents = RemoveFromArray(addComponents, "");
            for (int i = 0; i < removeComponents.Length; i++)
            {
                if (removeComponents[i].Equals(oldName))
                {
                    removeComponents[i] = newName;
                }
            }
            removeComponents = RemoveFromArray(removeComponents, "");
        }

        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            r += "text=\"" + originalText + "\"" + nl;
            
            if (highlight)
            {
                r += "highlight=true" + nl;
            }
            if (nextEvent.Length > 0)
            {
                r += "event=";
                foreach (string s in nextEvent)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            if (failEvent.Length > 0)
            {
                r += "failevent=";
                foreach (string s in failEvent)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            if (!heroListName.Equals(""))
            {
                r += "hero=" + heroListName + nl;
            }
            if (gold != 0)
            {
                r += "gold=" + gold + nl;
            }
            if (minHeroes != 0)
            {
                r += "minhero=" + minHeroes + nl;
            }
            if (maxHeroes != 0)
            {
                r += "maxhero=" + maxHeroes + nl;
            }
            if (addComponents.Length > 0)
            {
                r += "add=";
                foreach (string s in addComponents)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            if (removeComponents.Length > 0)
            {
                r += "remove=";
                foreach (string s in removeComponents)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            if (!trigger.Equals(""))
            {
                r += "trigger=" + trigger + nl;
            }
            if (flags.Length > 0)
            {
                r += "flags=";
                foreach (string s in flags)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            if (setFlags.Length > 0)
            {
                r += "set=";
                foreach (string s in setFlags)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            if (clearFlags.Length > 0)
            {
                r += "clear=";
                foreach (string s in clearFlags)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            return r;
        }
    }




    // Events are used to create dialogs that control the quest
    public class MPlace : QuestComponent
    {
        public bool master = false;
        new public static string type = "MPlace";
        public bool rotate = false;


        public MPlace(string s) : base(s)
        {
            locationSpecified = true;
            typeDynamic = type;
        }

        public MPlace(string name, Dictionary<string, string> data) : base(name, data)
        {
            locationSpecified = true;
            typeDynamic = type;
            master = false;
            if (data.ContainsKey("master"))
            {
                master = bool.Parse(data["master"]);
            }
            if (data.ContainsKey("rotate"))
            {
                rotate = bool.Parse(data["rotate"]);
            }
        }

        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();
            if (master)
            {
                r += "master=true" + nl;
            }
            if (rotate)
            {
                r += "rotate=true" + nl;
            }
            return r;
        }
    }

    // Super class for all quest components
    public class QuestComponent
    {
        // location on the board in squares
        public Vector2 location;
        // Has a location been speficied?
        public bool locationSpecified = false;
        // type for sub classes
        public static string type = "";
        public string typeDynamic = "";
        // name of section in ini file
        public string name;
        // image for display
        public UnityEngine.UI.Image image;

        public QuestComponent(string nameIn)
        {
            typeDynamic = type;
            name = nameIn;
            location = Vector2.zero;
        }

        // Construct from ini data
        public QuestComponent(string nameIn, Dictionary<string, string> data)
        {
            typeDynamic = type;
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

        public static string[] RemoveFromArray(string[] array, string element)
        {
            int count = 0;
            foreach (string s in array)
            {
                if (!s.Equals(element)) count++;
            }

            string[] trimArray = new string[count];

            int j = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (!array[i].Equals(element))
                {
                    trimArray[j++] = array[i];
                }
            }

            return trimArray;
        }

        virtual public void ChangeReference(string oldName, string newName)
        {

        }

        virtual public void RemoveReference(string refName)
        {
            ChangeReference(refName, "");
        }

        // items are invisible by default, can toggle visibility
        virtual public void Draw()
        {
            if(image == null)
                return;
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
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

        // items are invisible by default, can toggle visibility
        virtual public void SetVisible(float vis)
        {
            if (image == null)
                return;
            image.color = new Color(image.color.r, image.color.g, image.color.b, vis);
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

        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = "[" + name + "]" + nl;
            if (locationSpecified)
            {
                r += "xposition=" + location.x + nl;
                r += "yposition=" + location.y + nl;
            }

            return r;
        }
    }

    public class Quest
    {
        public string name = "";
        public string description = "";
        public int minPanX;
        public int minPanY;
        public int maxPanX;
        public int maxPanY;

        public Quest(Dictionary<string, string> data)
        {
            maxPanX = 20;
            maxPanY = 20;
            minPanX = -20;
            minPanY = -20;

            if (data.ContainsKey("name"))
            {
                name = data["name"];
            }
            if (data.ContainsKey("description"))
            {
                description = data["description"];
            }

            if (data.ContainsKey("maxpanx"))
            {
                maxPanX = int.Parse(data["maxpanx"]);
            }
            if (data.ContainsKey("maxpany"))
            {
                maxPanY = int.Parse(data["maxpany"]);
            }
            if (data.ContainsKey("minpanx"))
            {
                minPanX = int.Parse(data["minpanx"]);
            }
            if (data.ContainsKey("minpany"))
            {
                minPanY = int.Parse(data["minpany"]);
            }

            CameraController.SetCameraMin(new Vector2(minPanX, minPanY));
            CameraController.SetCameraMax(new Vector2(maxPanX, maxPanY));
        }

        public void SetMaxCam(Vector2 pos)
        {
            maxPanX = Mathf.RoundToInt(pos.x);
            maxPanY = Mathf.RoundToInt(pos.y);
            CameraController.SetCameraMax(new Vector2(maxPanX, maxPanY));
        }

        public void SetMinCam(Vector2 pos)
        {
            minPanX = Mathf.RoundToInt(pos.x);
            minPanY = Mathf.RoundToInt(pos.y);
            CameraController.SetCameraMin(new Vector2(minPanX, minPanY));
        }

        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = "[Quest]" + nl;
            r += "name=" + name + nl;
            r += "description=\"" + description + "\"" + nl;
            if (minPanY != -20)
            {
                r += "minpany=" + minPanY + nl;
            }
            if (minPanX != -20)
            {
                r += "minpanx=" + minPanX + nl;
            }
            if (maxPanX != -20)
            {
                r += "maxpanx=" + maxPanX + nl;
            }
            if (maxPanY != -20)
            {
                r += "maxpany=" + maxPanY + nl;
            }
            return r;
        }
    }
}

