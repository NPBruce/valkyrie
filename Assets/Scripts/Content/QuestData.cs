using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Content;

// Class to manage all static data for the current quest
public class QuestData
{
    // All components in the quest
    public Dictionary<string, QuestComponent> components;

    // Custom activations
    public Dictionary<string, ActivationData> questActivations;

    // List of ini files containing quest data
    List<string> files;

    // Location of the quest.ini file
    public string questPath = "";

    // Data from 'Quest' section
    public Quest quest;

    Game game;

    // Create from quest loader entry
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

    // Populate data
    public void LoadQuestData()
    {
        ValkyrieDebug.Log("Loading quest from: \"" + questPath + "\"" + System.Environment.NewLine);
        game = Game.Get();

        components = new Dictionary<string, QuestComponent>();
        questActivations = new Dictionary<string, ActivationData>();

        // Read the main quest file
        IniData questIniData = IniRead.ReadFromIni(questPath);
        // Failure to read quest is fatal
        if(questIniData == null)
        {
            ValkyrieDebug.Log("Failed to load quest from: \"" + questPath + "\"");
            Application.Quit();
        }

        // List of data files
        files = new List<string>();
        // The main data file is included
        files.Add(questPath);

        // Find others (no addition files is not fatal)
        if(questIniData.Get("QuestData") != null)
        {
            foreach (string file in questIniData.Get("QuestData").Keys)
            {
                // path is relative to the main file (absolute not supported)
                files.Add(Path.GetDirectoryName(questPath) + "/" + file);
            }
        }

        foreach (string f in files)
        {
            // Read each file
            questIniData = IniRead.ReadFromIni(f);
            // Failure to read a file is fatal
            if (questIniData == null)
            {
                ValkyrieDebug.Log("Unable to read quest file: \"" + f + "\"");
                Application.Quit();
            }
            // Loop through all ini sections
            foreach (KeyValuePair<string, Dictionary<string, string>> section in questIniData.data)
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
            ValkyrieDebug.Log("Duplicate component in quest: " + name);
            Application.Quit();
        }

        // Quest is a special component
        if (name.Equals("Quest"))
        {
            quest = new Quest(content);
        }
        // Check for known types and create
        if (name.IndexOf(Tile.type) == 0)
        {
            Tile c = new Tile(name, content);
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
        if (name.IndexOf(Item.type) == 0)
        {
            Item c = new Item(name, content);
            components.Add(name, c);
        }
        if (name.IndexOf(Puzzle.type) == 0)
        {
            Puzzle c = new Puzzle(name, content);
            components.Add(name, c);
        }
        if (name.IndexOf("UniqueMonster") == 0)
        {
            UniqueMonster c = new UniqueMonster(name, content, path);
            components.Add(name, c);
        }
        if (name.IndexOf("Activation") == 0)
        {
            Activation c = new Activation(name, content);
            components.Add(name, c);
        }
        // If not known ignore
    }

    // Class for Tile components (use TileSide content data)
    public class Tile : QuestComponent
    {
        new public static string type = "Tile";
        public int rotation = 0;
        public string tileSideName;

        // Create new with name (used by editor)
        public Tile(string s) : base(s)
        {
            locationSpecified = true;
            typeDynamic = type;
            Game game = Game.Get();
            foreach (KeyValuePair<string, TileSideData> kv in game.cd.tileSides)
            {
                tileSideName = kv.Key;
            }
        }

        // Create tile from ini data
        public Tile(string name, Dictionary<string, string> data) : base(name, data)
        {
            // Tiles must have a location
            locationSpecified = true;
            typeDynamic = type;
            // Get rotation if specified
            if (data.ContainsKey("rotation"))
            {
                int.TryParse(data["rotation"], out rotation);
            }

            // Find the tileside that is used
            if (data.ContainsKey("side"))
            {
                tileSideName = data["side"];
                // 'TileSide' prefix is optional, test both
                if (tileSideName.IndexOf("TileSide") != 0)
                {
                    tileSideName = "TileSide" + tileSideName;
                }
            }
            else
            {
                // Fatal if missing
                ValkyrieDebug.Log("Error: No TileSide specified in quest component: " + name);
                Application.Quit();
            }
        }

        // Save to ini string (used by editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            r += "side=" + tileSideName + nl;
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
        public GameObject gameObject;
        public string colourName = "white";

        // Create new with name (used by editor)
        public Door(string s) : base(s)
        {
            locationSpecified = true;
            typeDynamic = type;
            text = "You can open this door with an \"Open Door\" action.";
            cancelable = true;
        }

        // Create from ini data
        public Door(string name, Dictionary<string, string> data, Game game) : base(name, data)
        {
            locationSpecified = true;
            typeDynamic = type;
            // Doors are cancelable because you can select then cancel
            cancelable = true;

            if (data.ContainsKey("rotation"))
            {
                int.TryParse(data["rotation"], out rotation);
            }

            // color is only supported as a hexadecimal "#RRGGBB" format
            if (data.ContainsKey("color"))
            {
                colourName = data["color"];
            }
        }

        // Save to string
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
        public int rotation = 0;
        public string tokenName;

        // Create new with name (used by editor)
        public Token(string s) : base(s)
        {
            locationSpecified = true;
            typeDynamic = type;
            tokenName = "TokenSearch";
            cancelable = true;
        }

        // Create from ini data
        public Token(string name, Dictionary<string, string> data, Game game) : base(name, data)
        {
            locationSpecified = true;
            typeDynamic = type;
            // Tokens are cancelable because you can select then cancel
            cancelable = true;

            // default token type is TokenSearch, this is content data name
            tokenName = "TokenSearch";
            if (data.ContainsKey("type"))
            {
                tokenName = data["type"];
            }
            // Get rotation if specified
            if (data.ContainsKey("rotation"))
            {
                int.TryParse(data["rotation"], out rotation);
            }
        }

        // Save to string (for editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            if(!tokenName.Equals("TokenSearch"))
            {
                r += "type=" + tokenName + nl;
            }
            if (rotation != 0)
            {
                r += "rotation=" + rotation + nl;
            }
            return r;
        }
    }


    // Monster items are monster group placement events
    public class Monster : Event
    {
        new public static string type = "Monster";
        // Array of placements by hero count
        public string[][] placement;
        public bool unique = false;
        public string uniqueTitle = "";
        public string uniqueText = "";
        public string[] mTypes;
        public string[] mTraits;

        // Create new with name (used by editor)
        public Monster(string s) : base(s)
        {
            // Location defaults to specified
            locationSpecified = true;
            typeDynamic = type;
            Game game = Game.Get();
            mTypes = new string[1];
            // This gets the last type available, because we need something
            foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
            {
                mTypes[0] = kv.Key;
            }
            mTraits = new string[0];

            // Initialise array
            placement = new string[game.gameType.MaxHeroes() + 1][];
            for (int i = 0; i < placement.Length; i++)
            {
                placement[i] = new string[0];
            }
        }

        // Create from ini data
        public Monster(string name, Dictionary<string, string> data, Game game) : base(name, data)
        {
            typeDynamic = type;
            // First try to a list of specific types
            if (data.ContainsKey("monster"))
            {
                mTypes = data["monster"].Split(' ');
            }
            else
            {
                mTypes = new string[0];
            }

            // A list of traits to match
            mTraits = new string[0];
            if (data.ContainsKey("traits"))
            {
                mTraits = data["traits"].Split(' ');
            }

            // Array of placements by hero count
            placement = new string[game.gameType.MaxHeroes() + 1][];
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
                bool.TryParse(data["unique"], out unique);
            }
            if (data.ContainsKey("uniquetitle"))
            {
                uniqueTitle = data["uniquetitle"];
            }
            if (data.ContainsKey("uniquetext"))
            {
                uniqueText = data["uniquetext"];
            }
        }

        // When changing the name placement event need to update in array
        override public void ChangeReference(string oldName, string newName)
        {
            for (int j = 0; j < placement.Length; j++)
            {
                for (int i = 0; i < placement[j].Length; i++)
                {
                    // Placement used is being renamed
                    if (placement[j][i].Equals(oldName))
                    {
                        placement[j][i] = newName;
                    }
                }
                // If any were replaced with "", remove them
                placement[j] = RemoveFromArray(placement[j], "");
            }
        }

        // Save to string (editor)
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
            if (!uniqueTitle.Equals(""))
            {
                r += "uniquetitle=\"" + uniqueTitle + "\"" + nl;
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
        public List<string> buttons;
        public string trigger = "";
        public List<List<string>> nextEvent;
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
        public float threat;
        public bool absoluteThreat = false;
        public List<DelayedEvent> delayedEvents;
        public bool randomEvents = false;
        public bool minCam = false;
        public bool maxCam = false;
        public int quota = 0;

        // Create a new event with name (editor)
        public Event(string s) : base(s)
        {
            typeDynamic = type;
            nextEvent = new List<List<string>>();
            buttons = new List<string>();
            addComponents = new string[0];
            removeComponents = new string[0];
            flags = new string[0];
            setFlags = new string[0];
            clearFlags = new string[0];
            threat = 0;
            delayedEvents = new List<DelayedEvent>();
            minCam = false;
            maxCam = false;
        }

        // Create event from ini data
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
                bool.TryParse(data["highlight"], out highlight);
            }

            nextEvent = new List<List<string>>();
            buttons = new List<string>();
            int buttonNum = 1;
            bool moreEvents = true;
            while (moreEvents)
            {
                if (data.ContainsKey("button" + buttonNum))
                {
                    buttons.Add(data["button" + buttonNum]);

                    if (data.ContainsKey("event" + buttonNum))
                    {
                        if (data["event" + buttonNum].Trim().Length > 0)
                        {
                            nextEvent.Add(new List<string>(data["event" + buttonNum].Split(' ')));
                        }
                        else
                        {
                            nextEvent.Add(new List<string>());
                        }
                    }
                    else
                    {
                        nextEvent.Add(new List<string>());
                    }
                }
                else
                {
                    moreEvents = false;
                }
                buttonNum++;
            }

            // Legacy support
            if (nextEvent.Count == 0)
            {
                if (data.ContainsKey("event"))
                {
                    nextEvent.Add(new List<string>(data["event"].Split(' ')));
                }
                else
                {
                    nextEvent.Add(new List<string>());
                }

                if (data.ContainsKey("confirmtext"))
                {
                    buttons.Add(data["confirmtext"]);
                }
                else if (data.ContainsKey("failevent"))
                {
                    buttons.Add("Pass");
                }
                else
                {
                    buttons.Add("Confirm");
                }

                if (data.ContainsKey("failevent"))
                {
                    nextEvent.Add(new List<string>(data["failevent"].Split(' ')));
                    if (data.ContainsKey("failtext"))
                    {
                        buttons.Add(data["failtext"]);
                    }
                    else
                    {
                        buttons.Add("Fail");
                    }
                }
            }

            // Heros from another event can be hilighted
            if (data.ContainsKey("hero"))
            {
                heroListName = data["hero"];
            }

            // alter party gold (currently unused)
            if (data.ContainsKey("gold"))
            {
                int.TryParse(data["gold"], out gold);
            }
            
            // Success quota
            if (data.ContainsKey("quota"))
            {
                int.TryParse(data["quota"], out quota);
            }
            
            // minimum heros required to be selected for event
            if (data.ContainsKey("minhero"))
            {
                int.TryParse(data["minhero"], out minHeroes);
            }

            // maximum heros selectable for event (0 disables)
            if (data.ContainsKey("maxhero"))
            {
                int.TryParse(data["maxhero"], out maxHeroes);
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

            // Threat modifier
            if (data.ContainsKey("threat"))
            {
                if (data["threat"].Length != 0)
                {
                    // '@' at the start makes modifier absolute
                    if (data["threat"][0].Equals('@'))
                    {
                        absoluteThreat = true;
                        float.TryParse(data["threat"].Substring(1), out threat);
                    }
                    else
                    {
                        float.TryParse(data["threat"], out threat);
                    }
                }
            }

            // Read list of delayed events
            delayedEvents = new List<DelayedEvent>();
            if (data.ContainsKey("delayedevents"))
            {
                // List is space separated
                string[] de = data["delayedevents"].Split(' ');
                foreach (string s in de)
                {
                    int delay;
                    // : separates delay and event name
                    int.TryParse(s.Substring(0, s.IndexOf(":")), out delay);
                    string eventName = s.Substring(s.IndexOf(":") + 1);
                    delayedEvents.Add(new DelayedEvent(delay, eventName));
                }
            }
            // Randomise next event setting
            if (data.ContainsKey("randomevents"))
            {
                bool.TryParse(data["randomevents"], out randomEvents);
            }
            // Randomise next event setting
            if (data.ContainsKey("mincam"))
            {
                locationSpecified = false;
                bool.TryParse(data["mincam"], out minCam);
            }
            // Randomise next event setting
            if (data.ContainsKey("maxcam"))
            {
                locationSpecified = false;
                bool.TryParse(data["maxcam"], out maxCam);
            }
        }

        // Check all references when a component name is changed
        override public void ChangeReference(string oldName, string newName)
        {
            // hero list event is changed
            if (heroListName.Equals(oldName))
            {
                heroListName = newName;
            }
            // a next event is changed
            for (int i = 0; i < nextEvent.Count; i++)
            {
                for (int j = 0; j < nextEvent[i].Count; j++)
                {
                    if (nextEvent[i][j].Equals(oldName))
                    {
                        nextEvent[i][j] = newName;
                    }
                }
            }
            // If next event is deleted, trim array
            for (int i = 0; i < nextEvent.Count; i++)
            {
                bool removed = true;
                while (removed)
                {
                    removed = nextEvent[i].Remove("");
                }
            }

            // component to add renamed
            for (int i = 0; i < addComponents.Length; i++)
            {
                if (addComponents[i].Equals(oldName))
                {
                    addComponents[i] = newName;
                }
            }
            // If component is deleted, trim array
            addComponents = RemoveFromArray(addComponents, "");

            // component to remove renamed
            for (int i = 0; i < removeComponents.Length; i++)
            {
                if (removeComponents[i].Equals(oldName))
                {
                    removeComponents[i] = newName;
                }
            }
            // If component is deleted, trim array
            removeComponents = RemoveFromArray(removeComponents, "");

            // delayed event renamed, create new list and move across
            List<DelayedEvent> deList = new List<DelayedEvent>();
            foreach (DelayedEvent de in delayedEvents)
            {
                if (de.eventName.Equals(oldName))
                {
                    if (newName.Length > 0)
                    {
                        deList.Add(new DelayedEvent(de.delay, newName));
                    }
                }
                else
                {
                    deList.Add(de);
                }
            }
            // Update list
            delayedEvents = deList;
        }

        // Save event to string (editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            r += "text=\"" + originalText + "\"" + nl;

            if (highlight)
            {
                r += "highlight=true" + nl;
            }

            int buttonNum = 1;
            foreach (List<string> l in nextEvent)
            {
                r += "event" + buttonNum++ + "=";
                foreach (string s in l)
                {
                    r += s + " ";
                }
                if (l.Count > 0)
                {
                    r = r.Substring(0, r.Length - 1);
                }
                r += nl;
            }

            buttonNum = 1;
            foreach (string s in buttons)
            {
                r += "button" + buttonNum++ + "=\"" + s + "\"" + nl;
            }

            if (!heroListName.Equals(""))
            {
                r += "hero=" + heroListName + nl;
            }
            if (gold != 0)
            {
                r += "gold=" + gold + nl;
            }
            if (quota != 0)
            {
                r += "quota=" + quota + nl;
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

            if (delayedEvents.Count > 0)
            {
                r += "delayedevents=";
                foreach (DelayedEvent de in delayedEvents)
                {
                    r += de.delay + ":" + de.eventName + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }

            if (threat != 0)
            {
                r += "threat=";
                if (absoluteThreat)
                {
                    r += "@";
                }
                r += threat + nl;
            }
            if (randomEvents)
            {
                r += "randomevents=true" + nl;
            }
            // Randomise next event setting
            if (minCam)
            {
                r += "mincam=true" + nl;
            }
            if (maxCam)
            {
                r += "maxcam=true" + nl;
            }
            if (maxCam || minCam)
            {
                r += "xposition=" + location.x + nl;
                r += "yposition=" + location.y + nl;
            }
            return r;
        }

        // Delayed events have a name and delay value
        public class DelayedEvent
        {
            public string eventName = "";
            public int delay = 0;

            public DelayedEvent(int d, string e)
            {
                delay = d;
                eventName = e;
            }

            public DelayedEvent(string data)
            {
                int colon = data.IndexOf(":");
                if (colon == -1)
                {
                    return;
                }
                int.TryParse(data.Substring(0, colon), out delay);
                eventName = data.Substring(colon + 1);
            }
        }
    }




    // MPlaces are used to position individual monsters
    public class MPlace : QuestComponent
    {
        public bool master = false;
        new public static string type = "MPlace";
        public bool rotate = false;

        // Create a new mplace with name (editor)
        public MPlace(string s) : base(s)
        {
            locationSpecified = true;
            typeDynamic = type;
        }

        // Load mplace from ini data
        public MPlace(string name, Dictionary<string, string> data) : base(name, data)
        {
            // Must have a location
            locationSpecified = true;
            typeDynamic = type;
            master = false;
            if (data.ContainsKey("master"))
            {
                bool.TryParse(data["master"], out master);
            }
            if (data.ContainsKey("rotate"))
            {
                bool.TryParse(data["rotate"], out rotate);
            }
        }

        // Save to string (editor)
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

    // Puzzle component
    public class Puzzle : Event
    {
        new public static string type = "Puzzle";
        public string puzzleClass = "slide";
        public string skill = "{observation}";
        public int puzzleLevel = 4;
        public int puzzleAltLevel = 3;
        public string imageType = "";

        // Create a new puzzle with name (editor)
        public Puzzle(string s) : base(s)
        {
            typeDynamic = type;
        }

        // Construct from ini data
        public Puzzle(string name, Dictionary<string, string> data) : base(name, data)
        {
            typeDynamic = type;

            if (data.ContainsKey("class"))
            {
                puzzleClass = data["class"];
            }
            if (data.ContainsKey("image"))
            {
                imageType = data["image"];
            }
            if (data.ContainsKey("skill"))
            {
                skill = data["skill"];
            }
            if (data.ContainsKey("puzzlelevel"))
            {
                int.TryParse(data["puzzlelevel"], out puzzleLevel);
            }
            if (data.ContainsKey("puzzlealtlevel"))
            {
                int.TryParse(data["puzzlealtlevel"], out puzzleAltLevel);
            }
        }

        // Save to string (editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            if (!puzzleClass.Equals("slide"))
            {
                r += "class=" + puzzleClass + nl;
            }
            if (!skill.Equals("{observation}"))
            {
                r += "skill=" + skill + nl;
            }
            if (!imageType.Equals(""))
            {
                r += "image=" + imageType + nl;
            }
            if (puzzleLevel != 4)
            {
                r += "puzzlelevel=" + puzzleLevel + nl;
            }
            if (puzzleAltLevel != 3)
            {
                r += "puzzlealtlevel=" + puzzleAltLevel + nl;
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
        public string sectionName;
        // image for display
        public UnityEngine.UI.Image image;

        // Create new component in editor
        public QuestComponent(string nameIn)
        {
            typeDynamic = type;
            sectionName = nameIn;
            location = Vector2.zero;
        }

        // Construct from ini data
        public QuestComponent(string nameIn, Dictionary<string, string> data)
        {
            typeDynamic = type;
            sectionName = nameIn;

            // Default to 0, 0 unless specified
            location = new Vector2(0, 0);
            locationSpecified = false;
            if (data.ContainsKey("xposition"))
            {
                locationSpecified = true;
                float.TryParse(data["xposition"], out location.x);
            }

            if (data.ContainsKey("yposition"))
            {
                locationSpecified = true;
                float.TryParse(data["yposition"], out location.y);
            }
        }

        // Helper function to remove an element form an array
        public static string[] RemoveFromArray(string[] array, string element)
        {
            // Count how many elements remain
            int count = 0;
            foreach (string s in array)
            {
                if (!s.Equals(element)) count++;
            }

            // Create new array
            string[] trimArray = new string[count];

            // Index through old array, storing in new array
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

        // Used to rename components
        virtual public void ChangeReference(string oldName, string newName)
        {

        }

        // Used to delete components
        virtual public void RemoveReference(string refName)
        {
            // Rename to "" is taken to be delete
            ChangeReference(refName, "");
        }

        // Save to string (editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = "[" + sectionName + "]" + nl;
            if (locationSpecified)
            {
                r += "xposition=" + location.x + nl;
                r += "yposition=" + location.y + nl;
            }

            return r;
        }
    }

    // Monster defined in the quest
    public class UniqueMonster : QuestComponent
    {
        new public static string type = "UniqueMonster";
        // A bast type is used for default values
        public string baseMonster = "";
        public string monsterName = "";
        public string imagePath = "";
        public string imagePlace = "";
        public StringKey info = StringKey.EmptyStringKey;
        public string[] activations;
        public string[] traits;
        public string path = "";
        public int health = 0;
        public bool healthDefined = false;

        // Create new with name (editor)
        public UniqueMonster(string s) : base(s)
        {
            monsterName = sectionName;
            activations = new string[0];
            traits = new string[0];
            typeDynamic = type;
        }

        // Create from ini data
        public UniqueMonster(string name, Dictionary<string, string> data, string pathIn) : base(name, data)
        {
            typeDynamic = type;
            path = pathIn;
            // Get base derived monster type
            if (data.ContainsKey("base"))
            {
                baseMonster = data["base"];
            }

            monsterName = name;
            if (data.ContainsKey("name"))
            {
                monsterName = data["name"];
            }

            traits = new string[0];
            if (data.ContainsKey("traits"))
            {
                traits = data["traits"].Split(" ".ToCharArray());
            }

            if (data.ContainsKey("image"))
            {
                imagePath = data["image"];
            }

            if (data.ContainsKey("info"))
            {
                info = new StringKey(data["info"]);
            }

            imagePlace = imagePath;
            if (data.ContainsKey("imageplace"))
            {
                imagePlace = data["imageplace"];
            }

            activations = new string[0];
            if (data.ContainsKey("activation"))
            {
                activations = data["activation"].Split(' ');
            }

            if (data.ContainsKey("health"))
            {
                healthDefined = true;
                int.TryParse(data["health"], out health);
            }
        }

        // get path of monster image
        public string GetImagePath()
        {
            if (imagePath.Length == 0)
            {
                // this will use the base monster type
                return "";
            }
            return path + "/" + imagePath;
        }
        public string GetImagePlacePath()
        {
            if (imagePlace.Length == 0)
            {
                // this will use the base monster type
                return "";
            }
            return path + "/" + imagePlace;
        }

        // Save to string (editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            if (baseMonster.Length > 0)
            {
                r += "base=" + baseMonster + nl;
            }
            if (monsterName.Length > 0)
            {
                r += "name=" + monsterName + nl;
            }
            if (traits.Length > 0)
            {
                r += "traits=";
                foreach (string s in traits)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            if (info != null)
            {
                r += "info=" + info + nl;
            }
            if (imagePath.Length > 0)
            {
                r += "image=" + info + nl;
            }
            if (imagePlace.Length > 0)
            {
                r += "imageplace=" + info + nl;
            }
            if (activations.Length > 0)
            {
                r += "activation=";
                foreach (string s in activations)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            if (healthDefined)
            {
                r += "health=" + health.ToString() + nl;
            }
            return r;
        }
    }

    // Quest defined Monster activation
    public class Activation : QuestComponent
    {
        new public static string type = "Activation";
        //TODO: abilities are loaded from ffg strings, but it can be edited
        // for ffg abilities this field will be a key but for edited ability
        // after localization for quests, all abilityes will be keys.
        public StringKey ability = StringKey.EmptyStringKey;
        // same as ability
        public StringKey minionActions = StringKey.EmptyStringKey;
        // same as ability
        public StringKey masterActions = StringKey.EmptyStringKey;
        public bool minionFirst = false;
        public bool masterFirst = false;
        // same as ability
        public StringKey moveButton = StringKey.EmptyStringKey;
        // same as ability
        public StringKey move = StringKey.EmptyStringKey;

        // Create new (editor)
        public Activation(string s) : base(s)
        {
            typeDynamic = type;
        }

        // Create from ini data
        public Activation(string name, Dictionary<string, string> data) : base(name, data)
        {
            typeDynamic = type;
            if (data.ContainsKey("ability"))
            {
                ability = new StringKey(data["ability"]);
            }
            if (data.ContainsKey("master"))
            {
                masterActions = new StringKey(data["master"]);
            }
            if (data.ContainsKey("minion"))
            {
                minionActions = new StringKey(data["minion"]);
            }
            if (data.ContainsKey("move"))
            {
                move = new StringKey(data["move"]);
            }
            if (data.ContainsKey("movebutton"))
            {
                moveButton = new StringKey(data["movebutton"]);
            }
            if (data.ContainsKey("minionfirst"))
            {
                bool.TryParse(data["minionfirst"], out minionFirst);
            }
            if (data.ContainsKey("masterfirst"))
            {
                bool.TryParse(data["masterfirst"], out masterFirst);
            }
        }

        // Save to string
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            if (ability != null)
            {
                r += "ability=" + ability + nl;
            }
            if (masterActions != null)
            {
                r += "master=" + masterActions + nl;
            }
            if (minionActions != null)
            {
                r += "minion=" + minionActions + nl;
            }
            if (move.key.Length > 0)
            {
                r += "move=" + move + nl;
            }
            if (moveButton.key.Length > 0)
            {
                r += "movebutton=" + moveButton + nl;
            }
            if (minionFirst)
            {
                r += "minionfirst=" + minionFirst.ToString() + nl;
            }
            if (masterFirst)
            {
                r += "masterfirst=" + masterFirst.ToString() + nl;
            }
            return r;
        }
    }


    // Scenario starting item
    public class Item : QuestComponent
    {
        new public static string type = "Item";
        public string[] itemName;
        public string[] traits;

        // Create new (editor)
        public Item(string s) : base(s)
        {
            typeDynamic = type;
            itemName = new string[0];
            traits = new string[1];
            traits[0] = "weapon";
        }

        // Create from ini data
        public Item(string name, Dictionary<string, string> data) : base(name, data)
        {
            typeDynamic = type;
            if (data.ContainsKey("itemname"))
            {
                itemName = data["itemname"].Split(' ');
            }
            else
            {
                itemName = new string[0];
            }
            if (data.ContainsKey("traits"))
            {
                traits = data["traits"].Split(' ');
            }
            else
            {
                traits = new string[0];
            }
        }

        // Save to string
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            if (itemName.Length > 0)
            {
                r += "itemname=";
                foreach (string s in itemName)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            if (traits.Length > 0)
            {
                r += "traits=";
                foreach (string s in traits)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            return r;
        }
    }


    // Quest ini component has special data
    public class Quest
    {
        // Quest name
        public string name = "";
        // Quest description (currently unused)
        public string description = "";
        // quest type (MoM, D2E)
        public string type;
        // threat levels to trigger perils
        public int minorPeril = 7;
        public int majorPeril = 10;
        public int deadlyPeril = 12;
        // Content packs required for quest
        public string[] packs;

        // Create from ini data
        public Quest(Dictionary<string, string> iniData)
        {
            if (iniData.ContainsKey("name"))
            {
                name = iniData["name"];
            }

            // Default to D2E to support historical quests
            type = "D2E";
            if (iniData.ContainsKey("type"))
            {
                type = iniData["type"];
            }
            if (iniData.ContainsKey("description"))
            {
                description = iniData["description"];
            }
            if (iniData.ContainsKey("minorperil"))
            {
                int.TryParse(iniData["minorperil"], out minorPeril);
            }
            if (iniData.ContainsKey("majorperil"))
            {
                int.TryParse(iniData["majorperil"], out majorPeril);
            }
            if (iniData.ContainsKey("deadlyperil"))
            {
                int.TryParse(iniData["deadlyperil"], out deadlyPeril);
            }
            if (iniData.ContainsKey("packs"))
            {
                packs = iniData["packs"].Split(' ');
            }
            else
            {
                packs = new string[0];
            }
        }

        // Save to string (editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = "[Quest]" + nl;
            r += "name=" + name + nl;
            r += "description=" + description + nl;
            // Set this so that old quests have a type applied
            r += "type=" + Game.Get().gameType.TypeName() + nl;
            if (minorPeril != 7)
            {
                r += "minorperil=" + minorPeril + nl;
            }
            if (majorPeril != 10)
            {
                r += "majorperil=" + majorPeril + nl;
            }
            if (deadlyPeril != 12)
            {
                r += "deadlyperil=" + deadlyPeril + nl;
            }
            if (packs.Length > 0)
            {
                r += "packs=";
                foreach (string s in packs)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }

            return r;
        }
    }
}
