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

    // Dictionary of items to rename on reading
    public Dictionary<string, string> rename;

    // Data from 'Quest' section
    public Quest quest;

    Game game;

    // Create from quest loader entry
    public QuestData(QuestData.Quest q)
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

            rename = new Dictionary<string, string>();
            // Loop through all ini sections
            foreach (KeyValuePair<string, Dictionary<string, string>> section in questIniData.data)
            {
                // Add the section to our quest data
                AddData(section.Key, section.Value, Path.GetDirectoryName(f));
            }

            // Update all references to this component
            foreach (QuestComponent qc in components.Values)
            {
                foreach (KeyValuePair<string, string> kv in rename)
                {
                    qc.ChangeReference(kv.Key, kv.Value);
                }
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
        if (name.IndexOf(Spawn.type) == 0)
        {
            Spawn c = new Spawn(name, content, game);
            components.Add(name, c);
        }
        // Depreciated (format 1)
        if (name.IndexOf("Monster") == 0)
        {
            string fixedName = "Spawn" + name.Substring("Monster".Length);
            rename.Add(name, fixedName);
            Spawn c = new Spawn(fixedName, content, game);
            components.Add(fixedName, c);
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
        // Depreciated (format 1)
        if (name.IndexOf("UniqueMonster") == 0)
        {
            string fixedName = "CustomMonster" + name.Substring("UniqueMonster".Length);
            rename.Add(name, fixedName);
            CustomMonster c = new CustomMonster(fixedName, content, path);
            components.Add(fixedName, c);
        }
        if (name.IndexOf("CustomMonster") == 0)
        {
            CustomMonster c = new CustomMonster(name, content, path);
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
                break;
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
                // 'TileSide' prefix is optional, test both (Depreciated, format 0)
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

            // default token type is TokenSearch, this is content data name (Depreciated, format 0)
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

            r += "type=" + tokenName + nl;
            if (rotation != 0)
            {
                r += "rotation=" + rotation + nl;
            }
            return r;
        }
    }


    // Spawn items are monster group placement events
    public class Spawn : Event
    {
        new public static string type = "Spawn";
        // Array of placements by hero count
        public string[][] placement;
        public bool unique = false;
        public string uniqueTitle = "";
        public string uniqueText = "";
        public string[] mTypes;
        public string[] mTraitsRequired;
        public string[] mTraitsPool;

        // Create new with name (used by editor)
        public Spawn(string s) : base(s)
        {
            // Location defaults to specified
            locationSpecified = true;
            typeDynamic = type;
            Game game = Game.Get();
            mTypes = new string[1];
            // This gets the first type available, because we need something
            foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
            {
                mTypes[0] = kv.Key;
                break;
            }
            mTraitsRequired = new string[0];
            mTraitsPool = new string[0];

            // Initialise array
            placement = new string[game.gameType.MaxHeroes() + 1][];
            for (int i = 0; i < placement.Length; i++)
            {
                placement[i] = new string[0];
            }
        }

        // Create from ini data
        public Spawn(string name, Dictionary<string, string> data, Game game) : base(name, data)
        {
            typeDynamic = type;
            // First try to a list of specific types
            if (data.ContainsKey("monster"))
            {
                mTypes = data["monster"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                mTypes = new string[0];
            }

            // A list of traits to match
            mTraitsRequired = new string[0];
            if (data.ContainsKey("traits"))
            {
                mTraitsRequired = data["traits"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }

            // A list of traits to match
            mTraitsPool = new string[0];
            if (data.ContainsKey("traitpool"))
            {
                mTraitsPool = data["traitpool"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }

            // Array of placements by hero count
            placement = new string[game.gameType.MaxHeroes() + 1][];
            for (int i = 0; i < placement.Length; i++)
            {
                placement[i] = new string[0];
                if (data.ContainsKey("placement" + i))
                {
                    placement[i] = data["placement" + i].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
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
            base.ChangeReference(oldName, newName);

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

            for (int i = 0; i < mTypes.Length; i++)
            {
                // Placement used is being renamed
                if (mTypes[i].Equals(oldName) && oldName.IndexOf("Monster") != 0)
                {
                    mTypes[i] = newName;
                }
                // If any were replaced with "", remove them
                mTypes = RemoveFromArray(mTypes, "");
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
            if (mTraitsRequired.Length > 0)
            {
                r += "traits=";
                foreach (string s in mTraitsRequired)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            if (mTraitsPool.Length > 0)
            {
                r += "traitpool=";
                foreach (string s in mTraitsPool)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            for (int i = 0; i < placement.Length; i++)
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
        public int minHeroes = 0;
        public int maxHeroes = 0;
        public string[] addComponents;
        public string[] removeComponents;
        public List<VarOperation> operations;
        public List<VarOperation> conditions;
        public bool cancelable = false;
        public bool highlight = false;
        public List<DelayedEvent> delayedEvents;
        public bool randomEvents = false;
        public bool minCam = false;
        public bool maxCam = false;
        public int quota = 0;
        public string audio = "";

        // Create a new event with name (editor)
        public Event(string s) : base(s)
        {
            typeDynamic = type;
            nextEvent = new List<List<string>>();
            buttons = new List<string>();
            addComponents = new string[0];
            removeComponents = new string[0];
            operations = new List<VarOperation>();
            conditions = new List<VarOperation>();
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
                            nextEvent.Add(new List<string>(data["event" + buttonNum].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries)));
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

            // Legacy support (Depreciated, format 0)
            if (nextEvent.Count == 0)
            {
                if (data.ContainsKey("event"))
                {
                    nextEvent.Add(new List<string>(data["event"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries)));
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
                    nextEvent.Add(new List<string>(data["failevent"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries)));
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
                addComponents = data["add"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                addComponents = new string[0];
            }

            // Hide components (space separated list)
            if (data.ContainsKey("remove"))
            {
                removeComponents = data["remove"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
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

            operations = new List<VarOperation>();
            if (data.ContainsKey("operations"))
            {
                string[] array = data["operations"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in array)
                {
                    operations.Add(new VarOperation(s));
                }
            }

            conditions = new List<VarOperation>();
            if (data.ContainsKey("conditions"))
            {
                string[] array = data["conditions"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in array)
                {
                    conditions.Add(new VarOperation(s));
                }
            }

            // Flags required to trigger (space separated list Depreciated format 0)
            if (data.ContainsKey("flags"))
            {
                string[] flags = data["flags"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in flags)
                {
                    if (s.Equals("#2hero"))
                    {
                        conditions.Add(new VarOperation("#heroes,==,2"));
                    }
                    else if (s.Equals("#3hero"))
                    {
                        conditions.Add(new VarOperation("#heroes,==,3"));
                    }
                    else if (s.Equals("#4hero"))
                    {
                        conditions.Add(new VarOperation("#heroes,==,4"));
                    }
                    else if (s.Equals("#5hero"))
                    {
                        conditions.Add(new VarOperation("#heroes,==,5"));
                    }
                    else
                    {
                        if (s[0] == '#')
                        {
                            conditions.Add(new VarOperation(s + ",>,0"));
                        }
                        else
                        {
                            conditions.Add(new VarOperation("flag" + s + ",>,0"));
                        }
                    }
                }
            }

            // Flags to set (space separated list Depreciated format 0)
            if (data.ContainsKey("set"))
            {
                string[] flags = data["set"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in flags)
                {
                    operations.Add(new VarOperation("flag" + s + ",=,1"));
                }
            }

            // Flags to clear (space separated list Depreciated format 0)
            if (data.ContainsKey("clear"))
            {
                string[] flags = data["clear"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in flags)
                {
                    operations.Add(new VarOperation("flag" + s + ",=,0"));
                }
            }

            // Threat modifier (Depreciated format 0)
            if (data.ContainsKey("threat"))
            {
                if (data["threat"].Length != 0)
                {
                    float threat = 0;
                    // '@' at the start makes modifier absolute
                    if (data["threat"][0].Equals('@'))
                    {
                        float.TryParse(data["threat"].Substring(1), out threat);
                        operations.Add(new VarOperation("threat,=," + threat));
                    }
                    else
                    {
                        float.TryParse(data["threat"], out threat);
                        operations.Add(new VarOperation("threat,+," + threat));
                    }
                }
            }

            // Read list of delayed events
            delayedEvents = new List<DelayedEvent>();
            if (data.ContainsKey("delayedevents"))
            {
                // List is space separated
                string[] de = data["delayedevents"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
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
            // Randomise next event setting
            if (data.ContainsKey("audio"))
            {
                audio = data["audio"];
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

            // If CustomMonster renamed update trigger
            if (trigger.IndexOf("Defeated" + oldName) == 0)
            {
                trigger = "Defeated" + newName;
            }
            if (trigger.IndexOf("DefeatedUnique" + oldName) == 0)
            {
                trigger = "DefeatedUnique" + newName;
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

            if (operations.Count > 0)
            {
                r += "operations=";
                foreach (VarOperation o in operations)
                {
                    r += o.ToString() + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }

            if (conditions.Count > 0)
            {
                r += "conditions=";
                foreach (VarOperation o in conditions)
                {
                    r += o.ToString() + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }

            // Depreciated
            if (delayedEvents.Count > 0)
            {
                r += "delayedevents=";
                foreach (DelayedEvent de in delayedEvents)
                {
                    r += de.delay + ":" + de.eventName + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
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

            if (audio.Length > 0)
            {
                r += "audio=" + audio + nl;
            }
            return r;
        }

        public class VarOperation
        {
            public string var = "";
            public string operation = "";
            public string value = "";

            public VarOperation()
            {
            }

            public VarOperation(string inOp)
            {
                var = inOp.Split(",".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries)[0];
                operation = inOp.Split(",".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries)[1];
                value = inOp.Split(",".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries)[2];
            }

            override public string ToString()
            {
                return var + ',' + operation + ',' + value;
            }
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
        // comment for developers
        public string comment = "";

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
            if (data.ContainsKey("comment"))
            {
                comment = data["comment"];
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
            if (comment.Length > 0)
            {
                r += "comment=" + comment + nl;
            }
            return r;
        }
    }

    // Monster defined in the quest
    public class CustomMonster : QuestComponent
    {
        new public static string type = "CustomMonster";
        // A bast type is used for default values
        public string baseMonster = "";
        public string monsterName = "";
        public string imagePath = "";
        public string imagePlace = "";
        public StringKey info = StringKey.NULL;
        public string[] activations;
        public string[] traits;
        public string path = "";
        public int health = 0;
        public bool healthDefined = false;

        // Create new with name (editor)
        public CustomMonster(string s) : base(s)
        {
            monsterName = sectionName;
            activations = new string[0];
            traits = new string[0];
            typeDynamic = type;
        }

        // Create from ini data
        public CustomMonster(string name, Dictionary<string, string> data, string pathIn) : base(name, data)
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
                traits = data["traits"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }

            if (data.ContainsKey("image"))
            {
                imagePath = data["image"];
            }

            if (data.ContainsKey("info"))
            {
                info = new StringKey(data["info"], false);
            }

            imagePlace = imagePath;
            if (data.ContainsKey("imageplace"))
            {
                imagePlace = data["imageplace"];
            }

            activations = new string[0];
            if (data.ContainsKey("activation"))
            {
                activations = data["activation"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
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
        public StringKey ability = StringKey.NULL;
        // same as ability
        public StringKey minionActions = StringKey.NULL;
        // same as ability
        public StringKey masterActions = StringKey.NULL;
        public bool minionFirst = false;
        public bool masterFirst = false;
        // same as ability
        public StringKey moveButton = StringKey.NULL;
        // same as ability
        public StringKey move = StringKey.NULL;

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
                ability = new StringKey(data["ability"], false);
            }
            if (data.ContainsKey("master"))
            {
                masterActions = new StringKey(data["master"], false);
            }
            if (data.ContainsKey("minion"))
            {
                minionActions = new StringKey(data["minion"], false);
            }
            if (data.ContainsKey("move"))
            {
                move = new StringKey(data["move"], false);
            }
            if (data.ContainsKey("movebutton"))
            {
                moveButton = new StringKey(data["movebutton"], false);
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
                itemName = data["itemname"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                itemName = new string[0];
            }
            if (data.ContainsKey("traits"))
            {
                traits = data["traits"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
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
        public static int minumumFormat = 0;
        // Increment during changes, and again at release
        public static int currentFormat = 2;
        public int format = 0;
        public bool valid = false;
        public string path = "";
        // Quest name
        public string name = "";
        // Quest description (currently unused)
        public string description = "";
        // quest type (MoM, D2E)
        public string type;
        // Content packs required for quest
        public string[] packs;

        // Create from path
        public Quest(string pathIn)
        {
            path = pathIn;
            Dictionary<string, string> iniData = IniRead.ReadFromIni(path + "/quest.ini", "Quest");
            valid = Populate(iniData);
        }

        // Create from ini data
        public Quest(Dictionary<string, string> iniData)
        {
            valid = Populate(iniData);
        }

        // Create from ini data
        public bool Populate(Dictionary<string, string> iniData)
        {
            if (iniData.ContainsKey("format"))
            {
                int.TryParse(iniData["format"], out format);
            }

            if (format > currentFormat || format < minumumFormat)
            {
                return false;
            }

            if (iniData.ContainsKey("name"))
            {
                name = iniData["name"];
            }
            else
            {
                ValkyrieDebug.Log("Warning: Failed to get name data out of " + path + "/quest.ini!");
                return false;
            }

            // Default to D2E to support historical quests (Depreciated, format 0)
            type = "D2E";
            if (iniData.ContainsKey("type"))
            {
                type = iniData["type"];
            }
            if (iniData.ContainsKey("description"))
            {
                description = iniData["description"];
            }
            if (iniData.ContainsKey("packs"))
            {
                packs = iniData["packs"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                packs = new string[0];
            }

            return true;
        }

        // Save to string (editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = "[Quest]" + nl;
            r += "format=" + currentFormat + nl;
            r += "name=" + name + nl;
            r += "description=" + description + nl;
            r += "type=" + Game.Get().gameType.TypeName() + nl;
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

        public List<string> GetMissingPacks(List<string> selected)
        {
            List<string> r = new List<string>();
            foreach (string s in packs)
            {
                if (!selected.Contains(s))
                {
                    r.Add(s);
                }
            }
            return r;
        }
    }
}
