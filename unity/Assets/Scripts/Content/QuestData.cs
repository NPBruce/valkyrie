using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Assets.Scripts.Content;
using ValkyrieTools;

// Class to manage all static data for the current quest
public class QuestData
{
    // All components in the quest
    public Dictionary<string, QuestComponent> components;

    // Custom activations
    public Dictionary<string, ActivationData> questActivations;

    // List of ini files containing quest data
    List<string> iniFiles;
    // List of localization files containing quest texts
    List<string> localizationFiles;

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
        iniFiles = new List<string>();
        localizationFiles = new List<string>();
        // The main data file is included
        iniFiles.Add(questPath);

        // Find others (no addition files is not fatal)
        if (questIniData.Get("QuestData") != null)
        {
            foreach (string file in questIniData.Get("QuestData").Keys)
            {
                if (file != null && file.Length > 0)
                {
                    // path is relative to the main file (absolute not supported)
                    iniFiles.Add(Path.GetDirectoryName(questPath) + "/" + file);
                }
            }
        }
        else
        {
            ValkyrieDebug.Log("No QuestData extra files");
        }

        // Find Localization texts
        if (questIniData.Get("QuestText") != null)
        {
            foreach (string file in questIniData.Get("QuestText").Keys)
            {
                if (file != null && file.Length > 0)
                {
                    // path is relative to the main file (absolute not supported)
                    localizationFiles.Add(Path.GetDirectoryName(questPath) + "/" + file);
                }
            }
        }
        else
        {
            ValkyrieDebug.Log("No QuestText extra files");
        }

        // New dictionary without entries
        LocalizationRead.scenarioDict = new DictionaryI18n(
            new string[1] { DictionaryI18n.FFG_LANGS }, DictionaryI18n.DEFAULT_LANG, game.currentLang);

        foreach (string f in iniFiles)
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

        foreach (string file in localizationFiles)
        {
            LocalizationRead.scenarioDict.Add(
                LocalizationRead.ReadFromFilePath(file, quest.defaultLanguage, game.currentLang)
                );
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
        if (name.IndexOf(StartingItem.type) == 0)
        {
            StartingItem c = new StartingItem(name, content);
            components.Add(name, c);
        }
        // Depreciated (format 2)
        if (name.IndexOf("Item") == 0)
        {
            string fixedName = "StartingItem" + name.Substring("Item".Length);
            StartingItem c = new StartingItem(fixedName, content);
            components.Add(fixedName, c);
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

            tokenName = "";
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
        public float uniqueHealthBase = 0;
        public float uniqueHealthHero = 0;
        public string[] mTypes;
        public string[] mTraitsRequired;
        public string[] mTraitsPool;

        public string uniquetitle_key { get { return genKey("uniquetitle"); } }
        public string uniquetext_key { get { return genKey("uniquetext"); } }

        public StringKey uniqueTitle { get { return genQuery("uniquetitle"); } }
        public StringKey uniqueText { get { return genQuery("uniquetext"); } }

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
            // depreciated (format 2)
            if (data.ContainsKey("uniquetitle") && !data["uniquetitle"].StartsWith("{qst:"))
            {
                LocalizationRead.updateScenarioText(uniquetitle_key, data["uniquetitle"]);
            }
            // depreciated (format 2)
            if (data.ContainsKey("uniquetext") && !data["uniquetext"].StartsWith("{qst:"))
            {
                LocalizationRead.updateScenarioText(uniquetext_key, data["uniquetext"]);
            }
            if (data.ContainsKey("uniquehealth"))
            {
                float.TryParse(data["uniquehealth"], out uniqueHealthBase);
            }
            if (data.ContainsKey("uniquehealthhero"))
            {
                float.TryParse(data["uniquehealthhero"], out uniqueHealthHero);
            }
        }

        // When changing the name placement event need to update in array
        override public void ChangeReference(string oldName, string newName)
        {
            base.ChangeReference(oldName, newName);

            // it wasn't renamed yet so the _key items will point to old keys.
            // new key can be created by replacing the left part of the . whith new name
            if (sectionName.Equals(oldName) && newName != "")
            {
                AfterRenameUpdateDictionaryTextAndGenKey(uniquetitle_key, newName);
                AfterRenameUpdateDictionaryTextAndGenKey(uniquetext_key, newName);
            }

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
                r += "uniquehealth=" + uniqueHealthBase + nl;
                r += "uniquehealthhero=" + uniqueHealthHero + nl;
            }
            if(uniqueHealthBase != 0)
            {
                r += "uniquehealth=" + uniqueHealthBase + nl;
            }
            if(uniqueHealthHero != 0)
            {
                r += "uniquehealthhero=" + uniqueHealthHero + nl;
            }
            return r;
        }
    }


    // Events are used to create dialogs that control the quest
    public class Event : QuestComponent
    {
        new public static string type = "Event";

        public bool display = true;
        public List<StringKey> buttons;
        public List<string> buttonColors;
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
        public bool randomEvents = false;
        public bool minCam = false;
        public bool maxCam = false;
        public int quota = 0;
        public string audio = "";

        public string text_key { get { return genKey("text"); } }

        virtual public StringKey text { get { return genQuery("text"); } }

        // Create a new event with name (editor)
        public Event(string s) : base(s)
        {
            display = false;
            typeDynamic = type;
            nextEvent = new List<List<string>>();
            buttons = new List<StringKey>();
            buttonColors = new List<string>();
            addComponents = new string[0];
            removeComponents = new string[0];
            operations = new List<VarOperation>();
            conditions = new List<VarOperation>();
            minCam = false;
            maxCam = false;
        }

        // Create event from ini data
        public Event(string name, Dictionary<string, string> data, bool external = false) : base(name, data)
        {
            typeDynamic = type;

            if (data.ContainsKey("display"))
            {
                bool.TryParse(data["display"], out display);
            }

            // Depreciated (format 2)
            if (data.ContainsKey("text") && !data["text"].StartsWith("{qst:"))
            {
                if (data["text"].Length == 0)
                {
                    display = false;
                }
                else if (!external)
                {
                    LocalizationRead.updateScenarioText(text_key, data["text"]);
                }
            }

            // Should the target location by highlighted?
            if (data.ContainsKey("highlight"))
            {
                bool.TryParse(data["highlight"], out highlight);
            }

            nextEvent = new List<List<string>>();
            buttons = new List<StringKey>();
            buttonColors = new List<string>();

            int buttonCount = 0;
            if (data.ContainsKey("buttons"))
            {
                int.TryParse(data["buttons"], out buttonCount);
            }

            // Depreciated button count test (format 2)
            for (int buttonNum = buttonCount; buttonNum <= 10; buttonNum++)
            {
                if (data.ContainsKey("button" + buttonNum))
                {
                    buttonCount = buttonNum;
                }
                if (data.ContainsKey("event" + buttonNum))
                {
                    buttonCount = buttonNum;
                }
            }

            // Displayed events must have a button
            if (display && buttonCount == 0)
            {
                buttonCount = 1;
            }

            for (int buttonNum = 1; buttonNum <= buttonCount; buttonNum++)
            {
                buttons.Add(genQuery("button" + buttonNum));
                // Depreciated (format 2)
                if (data.ContainsKey("button" + buttonNum) && !data["button" + buttonNum].StartsWith("{qst:") && !external && display)
                {
                    LocalizationRead.updateScenarioText(genKey("button" + buttonNum), data["button" + buttonNum]);
                }

                if (data.ContainsKey("event" + buttonNum) && (data["event" + buttonNum].Trim().Length > 0))
                {
                    nextEvent.Add(new List<string>(data["event" + buttonNum].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries)));
                }
                else
                {
                    nextEvent.Add(new List<string>());
                }

                if (data.ContainsKey("buttoncolor" + buttonNum) && display)
                {
                    buttonColors.Add(data["buttoncolor" + buttonNum]);
                }
                else
                {
                    // Depreciated support for format 2
                    if (buttonCount == 2 && buttonNum == 1 && data.ContainsKey("button1") && data["button1"].Equals("Pass"))
                    {
                        buttonColors.Add("green");
                    }
                    else if (buttonCount == 2 && buttonNum == 2 && data.ContainsKey("button2") && data["button2"].Equals("Fail"))
                    {
                        buttonColors.Add("red");
                    }
                    else
                    {
                        buttonColors.Add("white");
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
            if (sectionName.Equals(oldName) && newName != "")
            {                
                AfterRenameUpdateDictionaryTextAndGenKey(text_key, newName);
                for (int pos = 0;pos < buttons.Count;pos++)
                {
                    buttons[pos] = AfterRenameUpdateDictionaryTextAndGenKey(genKey("button" + (pos + 1).ToString()), newName);
                }                 
            }

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
        }

        // Save event to string (editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            if (!display)
            {
                r += "display=false" + nl;
            }

            if (highlight)
            {
                r += "highlight=true" + nl;
            }

            r += "buttons=" + buttons.Count + nl;

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
            foreach (string s in buttonColors)
            {
                if (!s.Equals("white"))
                {
                    r += "buttoncolor" + buttonNum++ + "=\"" + s + "\"" + nl;
                }
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

                // Support old internal var names (depreciated, format 2)
                var = UpdateVarName(var);
                value = UpdateVarName(value);
            }

            override public string ToString()
            {
                return var + ',' + operation + ',' + value;
            }


            private string UpdateVarName(string s)
            {
                string prefix = "";
                if (s.Equals("perilMinor")) prefix = "$";
                if (s.Equals("perilMajor")) prefix = "$";
                if (s.Equals("perilDeadly")) prefix = "$";

                if (s.Equals("mythosHelp")) prefix = "$";
                if (s.Equals("mythosFlavor")) prefix = "$";
                if (s.Equals("mythosMinor")) prefix = "$";
                if (s.Equals("mythosMajor")) prefix = "$";
                if (s.Equals("mythosDeadly")) prefix = "$";

                if (s.Equals("mythosIndoors")) prefix = "$";
                if (s.Equals("mythosOutdoors")) prefix = "$";

                if (s.Equals("mythosStreetCorner")) prefix = "$";
                if (s.Equals("mythosAlleyCorner")) prefix = "$";
                if (s.Equals("mythosAlley")) prefix = "$";
                if (s.Equals("mythosStreet")) prefix = "$";
                if (s.Equals("mythosHall")) prefix = "$";
                if (s.Equals("mythosLibrary")) prefix = "$";
                if (s.Equals("mythosKitchen")) prefix = "$";
                if (s.Equals("mythosBallroom")) prefix = "$";
                if (s.Equals("mythosCrypt")) prefix = "$";
                if (s.Equals("mythosGraveyard")) prefix = "$";
                if (s.Equals("mythosMorgue")) prefix = "$";
                if (s.Equals("mythosBeach")) prefix = "$";
                if (s.Equals("mythosDock")) prefix = "$";
                if (s.Equals("mythosPier")) prefix = "$";
                if (s.Equals("mythosBathroom")) prefix = "$";
                if (s.Equals("mythosOffice")) prefix = "$";
                if (s.Equals("mythosStudy")) prefix = "$";
                if (s.Equals("mythosTownSquare")) prefix = "$";
                if (s.Equals("mythosLounge")) prefix = "$";
                if (s.Equals("mythosStairs")) prefix = "$";
                if (s.Equals("mythosRiver")) prefix = "$";
                if (s.Equals("mythosDiningRoom")) prefix = "$";
                if (s.Equals("mythosBedroom")) prefix = "$";
                if (s.Equals("mythosStorageRoom")) prefix = "$";
                if (s.Equals("mythosHallStairs")) prefix = "$";
                if (s.Equals("mythosAtticStorage")) prefix = "$";

                if (s.Equals("mythosDarkness")) prefix = "$";
                if (s.Equals("mythosDiscardItem")) prefix = "$";
                if (s.Equals("mythosKey")) prefix = "$";
                return prefix + s;
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
            nextEvent.Add(new List<string>());
            buttonColors.Add("white");
            buttons.Add(genQuery("button1"));
            LocalizationRead.updateScenarioText(genKey("button1"), "Complete");
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
        private static char DOT = '.';
        public string genKey(string element)
        {
            return new StringBuilder(sectionName).Append(DOT).Append(element).ToString();
        }

        public StringKey genQuery(string element)
        {
            return new StringKey("qst", sectionName + DOT + element);
        }

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

        /// <summary>
        /// Updates de dicionary with new text and generates a StringKey element
        /// </summary>
        /// <param name="key">key to update/create</param>
        /// <param name="text">text in current language</param>
        /// <returns></returns>
        protected StringKey AfterRenameUpdateDictionaryTextAndGenKey(string oldkey, string newName)
        {
            string[] split = oldkey.Split('.');
            string newKey = new StringBuilder()
                .Append(newName).Append('.').Append(split[1]).ToString();

            // update or create scenario text in current language
            LocalizationRead.replaceScenarioText(oldkey, newKey);

            //return the stringkey
            return new StringKey("qst", newKey);
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
        public string imagePath = "";
        public string imagePlace = "";
        public string[] activations;
        public string[] traits;
        public string path = "";
        public float healthBase = 0;
        public float healthPerHero = 0;
        public bool healthDefined = false;

        public string monstername_key { get { return genKey("monstername"); } }
        public string info_key { get { return genKey("info"); } }

        public StringKey monsterName { get { return genQuery("monstername"); } }
        public StringKey info { get { return genQuery("info"); } }

        // Create new with name (editor)
        public CustomMonster(string s) : base(s)
        {
            LocalizationRead.updateScenarioText(monstername_key, sectionName);
            LocalizationRead.updateScenarioText(info_key, "-");
            activations = new string[0];
            traits = new string[0];
            typeDynamic = type;
        }

        // Create from ini data
        public CustomMonster(string iniName, Dictionary<string, string> data, string pathIn) : base(iniName, data)
        {
            typeDynamic = type;
            path = pathIn;
            // Get base derived monster type
            if (data.ContainsKey("base"))
            {
                baseMonster = data["base"];
            }
            
            // Depreciated (format 2)
            if (data.ContainsKey("name") && !data["name"].StartsWith("{qst:"))
            {
                LocalizationRead.updateScenarioText(monstername_key, data["name"]);
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

            // Depreciated (format 2)
            if (data.ContainsKey("info") && !data["info"].StartsWith("{qst:"))
            {
                LocalizationRead.updateScenarioText(info_key, data["info"]);
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
                float.TryParse(data["health"], out healthBase);
            }
            if (data.ContainsKey("healthperhero"))
            {
                healthDefined = true;
                float.TryParse(data["healthperhero"], out healthPerHero);
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
            StringBuilder r = new StringBuilder().Append(base.ToString());

            if (baseMonster.Length > 0)
            {
                r.Append("base=").AppendLine(baseMonster);
            }
            if (traits.Length > 0)
            {
                r.Append("traits=").AppendLine(string.Join(" ",traits));
            }
            if (imagePath.Length > 0)
            {
                r.Append("image=").AppendLine(imagePath);
            }
            if (imagePlace.Length > 0)
            {
                r.Append("imageplace=").AppendLine(imagePlace);
            }
            if (activations.Length > 0)
            {
                r.Append("activation=").AppendLine(string.Join(" ", activations));
            }
            if (healthDefined)
            {
                r.Append("health=").AppendLine(healthBase.ToString());
                r.Append("healthperhero=").AppendLine(healthPerHero.ToString());
            }
            return r.ToString();
        }
    }

    // Quest defined Monster activation
    public class Activation : QuestComponent
    {
        new public static string type = "Activation";
        public bool minionFirst = false;
        public bool masterFirst = false;

        public string ability_key { get { return genKey("ability"); } }
        public string minion_key { get { return genKey("minion"); } }
        public string master_key { get { return genKey("master"); } }
        public string movebutton_key { get { return genKey("movebutton"); } }
        public string move_key { get { return genKey("move"); } }

        public StringKey ability { get { return genQuery("ability"); } }
        public StringKey minionActions { get { return genQuery("minion"); } }
        public StringKey masterActions { get { return genQuery("master"); } }
        public StringKey moveButton { get { return genQuery("movebutton"); } }
        public StringKey move { get { return genQuery("move"); } }

        // Create new (editor)
        public Activation(string s) : base(s)
        {
            LocalizationRead.updateScenarioText(ability_key, "-");
            typeDynamic = type;
        }

        // Create from ini data
        public Activation(string name, Dictionary<string, string> data) : base(name, data)
        {
            typeDynamic = type;
            // Depreciated (format 2)
            if (data.ContainsKey("ability") && !data["ability"].StartsWith("{qst:"))
            {
                LocalizationRead.updateScenarioText(ability_key, data["ability"]);
            }
            // Depreciated (format 2)
            if (data.ContainsKey("master") && !data["master"].StartsWith("{qst:"))
            {
                LocalizationRead.updateScenarioText(minion_key, data["master"]);
            }
            // Depreciated (format 2)
            if (data.ContainsKey("minion") && !data["minion"].StartsWith("{qst:"))
            {
                LocalizationRead.updateScenarioText(master_key, data["minion"]);
            }
            // Depreciated (format 2)
            if (data.ContainsKey("move") && !data["move"].StartsWith("{qst:"))
            {
                LocalizationRead.updateScenarioText(movebutton_key, data["move"]);
            }
            // Depreciated (format 2)
            if (data.ContainsKey("movebutton") && !data["movebutton"].StartsWith("{qst:"))
            {
                LocalizationRead.updateScenarioText(move_key, data["movebutton"]);
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
    public class StartingItem : QuestComponent
    {
        new public static string type = "StartingItem";
        public string[] itemName;
        public string[] traits;

        // Create new (editor)
        public StartingItem(string s) : base(s)
        {
            typeDynamic = type;
            itemName = new string[0];
            traits = new string[1];
            traits[0] = "weapon";
        }

        // Create from ini data
        public StartingItem(string name, Dictionary<string, string> data) : base(name, data)
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
        public static int minumumFormat = 1;
        // Increment during changes, and again at release
        public static int currentFormat = 3;
        public int format = 0;
        public bool valid = false;
        public string path = "";
        // quest type (MoM, D2E)
        public string type;
        // Content packs required for quest
        public string[] packs;
        // Default language for the text
        public string defaultLanguage = DictionaryI18n.DEFAULT_LANG;
        // raw localization dictionary
        public DictionaryI18n localizationDict;

        public string name_key { get { return "quest.name"; } }
        public string description_key { get { return "quest.description"; } }

        public StringKey name { get { return new StringKey("qst", name_key); } }
        public StringKey description { get { return new StringKey("qst", description_key); } }

        // Create from path
        public Quest(string pathIn)
        {
            path = pathIn;
            Dictionary<string, string> iniData = IniRead.ReadFromIni(path + "/quest.ini", "Quest");
            localizationDict =
                    LocalizationRead.ReadFromFilePath(path + "/Localization.txt", defaultLanguage, defaultLanguage);
            if (localizationDict == null)
            {
                localizationDict = new DictionaryI18n(
                    new string[1] { DictionaryI18n.FFG_LANGS }, defaultLanguage, defaultLanguage);
            }
            valid = Populate(iniData);
        }

        // Create from ini data
        public Quest(Dictionary<string, string> iniData)
        {
            localizationDict = LocalizationRead.scenarioDict;
            if (localizationDict == null)
            {
                localizationDict = new DictionaryI18n(
                    new string[1] { DictionaryI18n.FFG_LANGS }, defaultLanguage, defaultLanguage);
            }
            valid = Populate(iniData);
        }

        /// <summary>
        /// Create from ini data
        /// </summary>
        /// <param name="iniData">ini data to populate quest</param>
        /// <returns>true if the quest is valid</returns>
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

            // Depreciated (format 2)
            if (iniData.ContainsKey("name") && !iniData["name"].StartsWith("{qst:"))
            {
                LocalizationRead.scenarioDict = localizationDict;
                LocalizationRead.updateScenarioText(name_key, iniData["name"]);
            }

            type = "";
            if (iniData.ContainsKey("type"))
            {
                type = iniData["type"];
            }
            // Depreciated (format 2)
            if (iniData.ContainsKey("description") && !iniData["description"].StartsWith("{qst:"))
            {
                LocalizationRead.scenarioDict = localizationDict;
                LocalizationRead.updateScenarioText(description_key, iniData["description"]);
            }

            if (iniData.ContainsKey("packs"))
            {
                packs = iniData["packs"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                packs = new string[0];
            }

            if (iniData.ContainsKey("defaultlanguage"))
            {
                defaultLanguage = iniData["defaultlanguage"];
                localizationDict.setDefaultLanguage(defaultLanguage);
            }

            return true;
        }


        // Save to string (editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            StringBuilder r = new StringBuilder();
            r.AppendLine("[Quest]");
            r.Append("format=").AppendLine(currentFormat.ToString());
            r.Append("type=").AppendLine(Game.Get().gameType.TypeName());
            r.Append("defaultlanguage=").AppendLine(defaultLanguage);
            if (packs.Length > 0)
            {
                r.Append("packs=");
                r.AppendLine(string.Join(" ", packs));
            }
            r.AppendLine().AppendLine("[QuestText]").AppendLine("Localization.txt");

            return r.ToString();
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
