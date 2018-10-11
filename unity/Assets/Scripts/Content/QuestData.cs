﻿using UnityEngine;
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
        questPath = q.path + Path.DirectorySeparatorChar + "quest.ini";
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
        iniFiles.Add("quest.ini");

        // Quest is a special component, must be in quest.ini
        if (questIniData.Get("Quest") == null)
        {
            ValkyrieDebug.Log("Error: Quest section missing from quest.ini");
            return;
        }
        quest = new Quest(questIniData.Get("Quest"));

        // Find others (no addition files is not fatal)
        if (questIniData.Get("QuestData") != null)
        {
            foreach (string file in questIniData.Get("QuestData").Keys)
            {
                if (file != null && file.Length > 0)
                {
                    // path is relative to the main file (absolute not supported)
                    iniFiles.Add(file);
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
                    localizationFiles.Add(Path.GetDirectoryName(questPath) + Path.DirectorySeparatorChar + file);
                }
            }
        }
        else
        {
            ValkyrieDebug.Log("No QuestText extra files");
        }

        // Reset scenario dict
        DictionaryI18n qstDict = new DictionaryI18n(game.currentLang);
        foreach (string file in localizationFiles)
        {
            qstDict.AddDataFromFile(file);
        }
        LocalizationRead.AddDictionary("qst", qstDict);

        foreach (string f in iniFiles)
        {
            string fullPath = Path.Combine(Path.GetDirectoryName(questPath), f);
            // Read each file
            questIniData = IniRead.ReadFromIni(fullPath);
            // Failure to read a file is fatal
            if (questIniData == null)
            {
                ValkyrieDebug.Log("Unable to read quest file: \"" + fullPath + "\"");
                Application.Quit();
            }

            rename = new Dictionary<string, string>();
            // Loop through all ini sections
            foreach (KeyValuePair<string, Dictionary<string, string>> section in questIniData.data)
            {
                // Add the section to our quest data
                AddData(section.Key, section.Value, f);
            }

            // Update all references to this component
            foreach (QuestComponent qc in components.Values)
            {
                foreach (KeyValuePair<string, string> kv in rename)
                {
                    qc.ChangeReference(kv.Key, kv.Value);
                    LocalizationRead.dicts["qst"].RenamePrefix(kv.Key + ".", kv.Value + ".");
                }
            }
        }
    }

    // Add a section from an ini file to the quest data.  Duplicates are not allowed
    void AddData(string name, Dictionary<string, string> content, string source)
    {
        // Fatal error on duplicates
        if(components.ContainsKey(name))
        {
            ValkyrieDebug.Log("Duplicate component in quest: " + name);
            Application.Quit();
        }

        // Check for known types and create
        if (name.IndexOf(Tile.type) == 0)
        {
            Tile c = new Tile(name, content, source);
            components.Add(name, c);
        }
        if (name.IndexOf(Door.type) == 0)
        {
            Door c = new Door(name, content, game, source);
            components.Add(name, c);
        }
        if (name.IndexOf(Token.type) == 0)
        {
            Token c = new Token(name, content, game, source);
            components.Add(name, c);
        }
        if (name.IndexOf(UI.type) == 0)
        {
            UI c = new UI(name, content, game, source);
            components.Add(name, c);
        }
        if (name.IndexOf(Event.type) == 0)
        {
            Event c = new Event(name, content, source, quest.format);
            components.Add(name, c);
        }
        if (name.IndexOf(Spawn.type) == 0)
        {
            Spawn c = new Spawn(name, content, game, source);
            components.Add(name, c);
        }
        if (name.IndexOf(MPlace.type) == 0)
        {
            MPlace c = new MPlace(name, content, source);
            components.Add(name, c);
        }
        if (name.IndexOf(QItem.type) == 0)
        {
            QItem c = new QItem(name, content, source);
            components.Add(name, c);
        }
        // Depreciated (format 3)
        if (name.IndexOf("StartingItem") == 0)
        {
            string fixedName = "QItem" + name.Substring("StartingItem".Length);
            QItem c = new QItem(fixedName, content, source);
            components.Add(fixedName, c);
        }
        if (name.IndexOf(Puzzle.type) == 0)
        {
            Puzzle c = new Puzzle(name, content, source);
            components.Add(name, c);
        }
        if (name.IndexOf(CustomMonster.type) == 0)
        {
            CustomMonster c = new CustomMonster(name, content, source);
            components.Add(name, c);
        }
        if (name.IndexOf(Activation.type) == 0)
        {
            Activation c = new Activation(name, content, source);
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
            source = "tiles.ini";
            foreach (KeyValuePair<string, TileSideData> kv in game.cd.tileSides)
            {
                tileSideName = kv.Key;
                break;
            }
        }

        // Create tile from ini data
        public Tile(string name, Dictionary<string, string> data, string path) : base(name, data, path)
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
            source = "door.ini";
            locationSpecified = true;
            typeDynamic = type;
            cancelable = true;
        }

        // Create from ini data
        public Door(string name, Dictionary<string, string> data, Game game, string path) : base(name, data, path, Quest.currentFormat)
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
            source = "tokens.ini";
            locationSpecified = true;
            typeDynamic = type;
            tokenName = "TokenSearch";
            cancelable = true;
        }

        // Create from ini data
        public Token(string name, Dictionary<string, string> data, Game game, string path) : base(name, data, path, Quest.currentFormat)
        {
            locationSpecified = true;
            typeDynamic = type;
            // Tokens are cancelable because you can select then cancel
            cancelable = true;

            // Tokens don't have conditions
            conditions = new List<VarOperation>();

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


    // UI is an image/button that is displayed to the user
    public class UI : Event
    {
        new public static string type = "UI";
        public string imageName = "";
        public bool verticalUnits = false;
        public int hAlign = 0;
        public int vAlign = 0;
        public float size = 1;
        public float textSize = 1;
        public string textColor = "white";
        public float aspect = 1;
        public bool border = false;

        public string uitext_key { get { return genKey("uitext"); } }

        public StringKey uiText { get { return genQuery("uitext"); } }

        // Create new with name (used by editor)
        public UI(string s) : base(s)
        {
            source = "ui.ini";
            locationSpecified = true;
            typeDynamic = type;
            cancelable = true;
        }

        // Create from ini data
        public UI(string name, Dictionary<string, string> data, Game game, string path) : base(name, data, path, Quest.currentFormat)
        {
            locationSpecified = true;
            typeDynamic = type;
            // Tokens are cancelable because you can select then cancel
            cancelable = true;

            if (data.ContainsKey("image"))
            {
                string value = data["image"];
                imageName = value != null ? value.Replace('\\', '/') : value;
            }

            if (data.ContainsKey("vunits"))
            {
                bool.TryParse(data["vunits"], out verticalUnits);
            }

            if (data.ContainsKey("size"))
            {
                float.TryParse(data["size"], out size);
            }

            if (data.ContainsKey("textsize"))
            {
                float.TryParse(data["textsize"], out textSize);
            }

            if (data.ContainsKey("textaspect"))
            {
                float.TryParse(data["textaspect"], out aspect);
            }

            if (data.ContainsKey("textcolor"))
            {
                textColor = data["textcolor"];
            }

            if (data.ContainsKey("halign"))
            {
                if (data["halign"].Equals("left"))
                {
                    hAlign = -1;
                }
                if (data["halign"].Equals("right"))
                {
                    hAlign = 1;
                }
            }

            if (data.ContainsKey("valign"))
            {
                if (data["valign"].Equals("top"))
                {
                    vAlign = -1;
                }
                if (data["valign"].Equals("bottom"))
                {
                    vAlign = 1;
                }
            }

            if (data.ContainsKey("border"))
            {
                bool.TryParse(data["border"], out border);
            }
        }

        // Save to string (for editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            r += "image=" + imageName + nl;
            r += "size=" + size + nl;

            if (textSize != 1)
            {
                r += "textsize=" + textSize + nl;
            }

            if (!textColor.Equals("white"))
            {
                r += "textcolor=" + textColor + nl;
            }

            if (verticalUnits)
            {
                r += "vunits=" + verticalUnits + nl;
            }

            if (border)
            {
                r += "border=" + border + nl;
            }

            if (aspect != 1)
            {
                r += "textaspect=" + aspect + nl;
            }

            if (hAlign < 0)
            {
                r += "halign=left" + nl;
            }
            if (hAlign > 0)
            {
                r += "halign=right" + nl;
            }

            if (vAlign < 0)
            {
                r += "valign=top" + nl;
            }
            if (vAlign > 0)
            {
                r += "valign=bottom" + nl;
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
            source = "spawns.ini";
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
        public Spawn(string name, Dictionary<string, string> data, Game game, string path) : base(name, data, path, Quest.currentFormat)
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
            }
            // If any were replaced with "", remove them
            mTypes = RemoveFromArray(mTypes, "");
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
            if(uniqueHealthBase != 0 && !unique)
            {
                r += "uniquehealth=" + uniqueHealthBase + nl;
            }
            if(uniqueHealthHero != 0 && !unique)
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
        public string quotaVar = "";
        public string audio = "";
        public List<string> music;

        public string text_key { get { return genKey("text"); } }

        virtual public StringKey text { get { return genQuery("text"); } }

        // Create a new event with name (editor)
        public Event(string s) : base(s)
        {
            source = "events.ini";
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
            music = new List<string>();
        }

        // Create event from ini data
        public Event(string name, Dictionary<string, string> data, string path, int format) : base(name, data, path)
        {
            typeDynamic = type;

            if (data.ContainsKey("display"))
            {
                bool.TryParse(data["display"], out display);
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

            // Displayed events must have a button
            if (display && buttonCount == 0)
            {
                buttonCount = 1;
            }

            for (int buttonNum = 1; buttonNum <= buttonCount; buttonNum++)
            {
                buttons.Add(genQuery("button" + buttonNum));
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
                    buttonColors.Add("white");
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
                if (data["quota"].Length > 0 && !char.IsNumber(data["quota"][0]))
                {
                    quotaVar = data["quota"];
                }
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
            // Backwards support for format < 8
            if (format <= 8 && sectionName.StartsWith("EventEnd"))
            {
                operations.Add(new VarOperation("$end,=,1"));

                Game.Get().quest.questHasEnded = true;
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
            if (data.ContainsKey("audio"))
            {
                string value = data["audio"];
                audio = value != null ? value.Replace('\\', '/') : value;
            }
            music = new List<string>();
            if (data.ContainsKey("music"))
            {
                music = new List<string>(data["music"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries));
                for (int i = 0; i < music.Count; i++)
                {
                    music[i] = music[i].Replace('\\', '/');
                }
            }
        }

        // Check all references when a component name is changed
        override public void ChangeReference(string oldName, string newName)
        {
            if (sectionName.Equals(oldName) && newName != "")
            {
                for (int i = 1; i <= buttons.Count; i++)
                {
                    buttons[i - 1] = new StringKey("qst", newName + '.' + "button" + i);
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
            if (quotaVar.Length > 0)
            {
                r += "quota=" + quotaVar + nl;
            }
            else if (quota != 0)
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

            if (music.Count > 0)
            {
                r += "music=";
                foreach (string s in music)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
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

                // Support old internal var names (depreciated, format 3)
                var = UpdateVarName(var);
                value = UpdateVarName(value);
            }

            override public string ToString()
            {
                return var + ',' + operation + ',' + value;
            }

            private string UpdateVarName(string s)
            {
                if (s.Equals("#fire")) return "$fire";
                return s;
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
            source = "mplaces.ini";
            locationSpecified = true;
            typeDynamic = type;
        }

        // Load mplace from ini data
        public MPlace(string name, Dictionary<string, string> data, string path) : base(name, data, path)
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
            source = "puzzles.ini";
            typeDynamic = type;
            nextEvent.Add(new List<string>());
            buttonColors.Add("white");
            buttons.Add(genQuery("button1"));
            LocalizationRead.updateScenarioText(genKey("button1"),
                new StringKey("val","PUZZLE_GUESS").Translate());
        }

        // Construct from ini data
        public Puzzle(string name, Dictionary<string, string> data, string path) : base(name, data, path, Quest.currentFormat)
        {
            typeDynamic = type;

            if (data.ContainsKey("class"))
            {
                puzzleClass = data["class"];
            }
            if (data.ContainsKey("image"))
            {
                string value = data["image"];
                imageType = value != null ? value.Replace('\\', '/') : value;
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
        // Source file
        public string source = "";
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
        public QuestComponent(string nameIn, Dictionary<string, string> data, string sourceIn)
        {
            typeDynamic = type;
            sectionName = nameIn;
            source = sourceIn;

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
        public string imagePath = "";
        public string imagePlace = "";
        public string[] activations;
        public string[] traits;
        public string path = "";
        public float healthBase = 0;
        public float healthPerHero = 0;
        public bool healthDefined = false;
        public string evadeEvent = "";
        public string horrorEvent = "";
        public int horror = 0;
        public bool horrorDefined = false;
        public int awareness = 0;
        public bool awarenessDefined = false;
        public Dictionary<string, List<StringKey>> investigatorAttacks = new Dictionary<string, List<StringKey>>();

        public string monstername_key { get { return genKey("monstername"); } }
        public string info_key { get { return genKey("info"); } }

        public StringKey monsterName { get { return genQuery("monstername"); } }
        public StringKey info { get { return genQuery("info"); } }

        // Create new with name (editor)
        public CustomMonster(string s) : base(s)
        {
            source = "monsters.ini";
            LocalizationRead.updateScenarioText(monstername_key, sectionName);
            LocalizationRead.updateScenarioText(info_key, "-");
            activations = new string[0];
            traits = new string[0];
            typeDynamic = type;
        }

        // Create from ini data
        public CustomMonster(string iniName, Dictionary<string, string> data, string pathIn) : base(iniName, data, pathIn)
        {
            typeDynamic = type;
            path = Path.GetDirectoryName(pathIn);
            // Get base derived monster type
            if (data.ContainsKey("base"))
            {
                baseMonster = data["base"];
            }
            
            traits = new string[0];
            if (data.ContainsKey("traits"))
            {
                traits = data["traits"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }

            if (data.ContainsKey("image"))
            {
                string value = data["image"];
                imagePath = value != null ? value.Replace('\\', '/') : value;
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

            if (data.ContainsKey("evadeevent"))
            {
                evadeEvent = data["evadeevent"];
            }
            if (data.ContainsKey("horrorevent"))
            {
                horrorEvent = data["horrorevent"];
            }

            if (data.ContainsKey("horror"))
            {
                horrorDefined = true;
                int.TryParse(data["horror"], out horror);
            }
            if (data.ContainsKey("awareness"))
            {
                awarenessDefined = true;
                int.TryParse(data["awareness"], out awareness);
            }

            if (data.ContainsKey("attacks"))
            {
                foreach(string typeEntry in data["attacks"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries))
                {
                    string type = typeEntry;
                    int typeCount = 1;
                    int typeSeparator = typeEntry.IndexOf(':');
                    if (typeSeparator >= 0)
                    {
                        type = typeEntry.Substring(0, typeSeparator);
                        int.TryParse(typeEntry.Substring(typeSeparator + 1), out typeCount);
                    }

                    if (!investigatorAttacks.ContainsKey(type))
                    {
                        investigatorAttacks.Add(type, new List<StringKey>());
                    }

                    for (int i = 1; i <= typeCount; i++)
                    {
                        investigatorAttacks[type].Add(genQuery("Attack_" + type + "_" + i));
                    }
                }
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
            return path + Path.DirectorySeparatorChar + imagePath;
        }
        public string GetImagePlacePath()
        {
            if (imagePlace.Length == 0)
            {
                // this will use the base monster type
                return "";
            }
            return path + Path.DirectorySeparatorChar + imagePlace;
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
            if (evadeEvent.Length > 0)
            {
                r.Append("evadeevent=").AppendLine(evadeEvent);
            }
            if (horrorEvent.Length > 0)
            {
                r.Append("horrorevent=").AppendLine(horrorEvent);
            }

            if (horrorDefined)
            {
                r.Append("horror=").AppendLine(horror.ToString());
            }
            if (awarenessDefined)
            {
                r.Append("awareness=").AppendLine(awareness.ToString());
            }

            if (investigatorAttacks.Count > 0)
            {
                string attacksLine = "attacks=";
                foreach (string type in investigatorAttacks.Keys)
                {
                    attacksLine += type + ':' + investigatorAttacks[type].Count + " ";
                }
                r.AppendLine(attacksLine.Substring(0, attacksLine.Length - 1));
            }

            return r.ToString();
        }

        // When changing the name placement event need to update in array
        override public void ChangeReference(string oldName, string newName)
        {
            for (int i = 0; i < activations.Length; i++)
            {
                if (activations[i].Equals(oldName))
                {
                    activations[i] = newName;
                }
                else
                {
                    if (oldName.Equals("Activation" + activations[i]))
                    {
                        if (newName.IndexOf("Activation") == 0)
                        {
                            activations[i] = newName.Substring("Activation".Length);
                        }
                        if (newName.Length == 0)
                        {
                            activations[i] = "";
                        }
                    }
                }
            }
            // If any were replaced with "", remove them
            activations = RemoveFromArray(activations, "");

            if (evadeEvent.Equals(oldName))
            {
                evadeEvent = newName;
            }
            if (horrorEvent.Equals(oldName))
            {
                horrorEvent = newName;
            }
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
            source = "monsters.ini";
            LocalizationRead.updateScenarioText(ability_key, "-");
            typeDynamic = type;
        }

        // Create from ini data
        public Activation(string name, Dictionary<string, string> data, string path) : base(name, data, path)
        {
            typeDynamic = type;
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
    public class QItem : QuestComponent
    {
        new public static string type = "QItem";
        public string[] itemName;
        public string[] traits;
        public string[] traitpool;
        public bool starting = false;
        public string inspect = "";

        // Create new (editor)
        public QItem(string s) : base(s)
        {
            source = "items.ini";
            typeDynamic = type;
            itemName = new string[0];
            traits = new string[1];
            traitpool = new string[0];
            traits[0] = "weapon";
        }

        // Create from ini data
        public QItem(string name, Dictionary<string, string> data, string path) : base(name, data, path)
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

            if (data.ContainsKey("starting"))
            {
                bool.TryParse(data["starting"], out starting);
            }
            else
            {
                // Depreciated (Format 3)
                starting = true;
            }

            if (data.ContainsKey("traits"))
            {
                traits = data["traits"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                traits = new string[0];
            }
            if (data.ContainsKey("traitpool"))
            {
                traitpool = data["traitpool"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                traitpool = new string[0];
            }
            if (data.ContainsKey("inspect"))
            {
                inspect = data["inspect"];
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

            // Depreciated (Format 3) - To make default false
            r += "starting=" + starting + nl;

            if (traits.Length > 0)
            {
                r += "traits=";
                foreach (string s in traits)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            if (traitpool.Length > 0)
            {
                r += "traitpool=";
                foreach (string s in traitpool)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }

            if (inspect.Length > 0)
            {
                r += "inspect=" + inspect + nl;
            }
            return r;
        }

        // When changing the name placement event need to update in array
        override public void ChangeReference(string oldName, string newName)
        {
            if (inspect.Equals(oldName))
            {
                inspect = newName;
            }
            for (int i = 0; i < itemName.Length; i++)
            {
                if (itemName[i].Equals(oldName))
                {
                    itemName[i] = newName;
                }
            }
            // If any were replaced with "", remove them
            itemName = RemoveFromArray(itemName, "");
        }
    }

    // Quest ini component has special data
    public class Quest
    {
        public static int minumumFormat = 4;
        // Increment during changes, and again at release
        public static int currentFormat = 9;
        public int format = 0;
        public bool hidden = false;
        public bool valid = false;
        public string path = "";
        // quest type (MoM, D2E)
        public string type;
        // Content packs required for quest
        public string[] packs;
        // Default language for the text
        public string defaultLanguage = "English";
        // raw localization dictionary
        public DictionaryI18n localizationDict = null;

        public string image = "";

        public int minHero = 2;
        public int maxHero = 5;
        public float difficulty = 0;
        public int lengthMin = 0;
        public int lengthMax = 0;

        public string name_key { get { return "quest.name"; } }
        public string description_key { get { return "quest.description"; } }
        public string authors_key { get { return "quest.authors"; } }

        public StringKey name { get { return new StringKey("qst", name_key); } }
        public StringKey description { get { return new StringKey("qst", description_key); } }
        public StringKey authors { get { return new StringKey("qst", authors_key); } }

        // Create from path
        public Quest(string pathIn)
        {
            path = pathIn;
            if (path.EndsWith("\\") || path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 1);
            }

            Dictionary<string, string> iniData = IniRead.ReadFromIni(path + Path.DirectorySeparatorChar + "quest.ini", "Quest");
            if (iniData == null)
            {
                ValkyrieDebug.Log("Could not load the quest.ini file in " + path + Path.DirectorySeparatorChar + "quest.ini");
                valid = false;
                return;
            }

            // do not parse the content of a quest from another game type
            if (iniData.ContainsKey("type") && iniData["type"] != Game.Get().gameType.TypeName())
            {
                valid = false;
                return;
            }

            //Read the localization data
            Dictionary<string, string> localizationData = IniRead.ReadFromIni(path + Path.DirectorySeparatorChar + "quest.ini", "QuestText");

            localizationDict = new DictionaryI18n(defaultLanguage);
            foreach (string file in localizationData.Keys)
            {
                localizationDict.AddDataFromFile(path + '/' + file);
            }

            valid = Populate(iniData);
        }

        // Create from ini data
        public Quest(Dictionary<string, string> iniData)
        {
            localizationDict = LocalizationRead.dicts["qst"];
            if (localizationDict == null)
            {
                localizationDict = new DictionaryI18n(new string[1] { ".," + Game.Get().currentLang }, defaultLanguage);
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

            type = "";
            if (iniData.ContainsKey("type"))
            {
                type = iniData["type"];
            }

            if (iniData.ContainsKey("packs"))
            {
                packs = iniData["packs"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                // Depreciated Format 5
                List<string> newPacks = new List<string>();
                foreach (string s in packs)
                {
                    if (s.Equals("MoM1E"))
                    {
                        newPacks.Add("MoM1ET");
                        newPacks.Add("MoM1EI");
                        newPacks.Add("MoM1EM");
                    }
                    else if (s.Equals("FA"))
                    {
                        newPacks.Add("FAT");
                        newPacks.Add("FAI");
                        newPacks.Add("FAM");
                    }
                    else if (s.Equals("CotW"))
                    {
                        newPacks.Add("CotWT");
                        newPacks.Add("CotWI");
                        newPacks.Add("CotWM");
                    }
                    else
                    {
                        newPacks.Add(s);
                    }
                }
                packs = newPacks.ToArray();
            }
            else
            {
                packs = new string[0];
            }

            if (iniData.ContainsKey("defaultlanguage"))
            {
                defaultLanguage = iniData["defaultlanguage"];
                localizationDict.defaultLanguage = defaultLanguage;
            }

            if (iniData.ContainsKey("hidden"))
            {
                bool.TryParse(iniData["hidden"], out hidden);
            }

            if (iniData.ContainsKey("minhero"))
            {
                int.TryParse(iniData["minhero"], out minHero);
            }
            if (minHero < 1) minHero = 1;

            maxHero = Game.Get().gameType.DefaultHeroes();
            if (iniData.ContainsKey("maxhero"))
            {
                int.TryParse(iniData["maxhero"], out maxHero);
            }
            if (maxHero > Game.Get().gameType.MaxHeroes())
            {
                maxHero = Game.Get().gameType.MaxHeroes();
            }

            if (iniData.ContainsKey("difficulty"))
            {
                float.TryParse(iniData["difficulty"], out difficulty);
            }

            if (iniData.ContainsKey("lengthmin"))
            {
                int.TryParse(iniData["lengthmin"], out lengthMin);
            }
            if (iniData.ContainsKey("lengthmax"))
            {
                int.TryParse(iniData["lengthmax"], out lengthMax);
            }

            if (iniData.ContainsKey("image"))
            {
                string value = iniData["image"];
                image = value != null ? value.Replace('\\', '/') : value;
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
            r.Append("hidden=").AppendLine(hidden.ToString());
            r.Append("type=").AppendLine(Game.Get().gameType.TypeName());
            r.Append("defaultlanguage=").AppendLine(defaultLanguage);
            if (packs.Length > 0)
            {
                r.Append("packs=");
                r.AppendLine(string.Join(" ", packs));
            }

            if (minHero != 2)
            {
                r.Append("minhero=").AppendLine(minHero.ToString());
            }
            if (maxHero != Game.Get().gameType.DefaultHeroes())
            {
                r.Append("maxhero=").AppendLine(maxHero.ToString());
            }

            if (difficulty != 0)
            {
                r.Append("difficulty=").AppendLine(difficulty.ToString());
            }

            if (lengthMin != 0)
            {
                r.Append("lengthmin=").AppendLine(lengthMin.ToString());
            }
            if (lengthMax != 0)
            {
                r.Append("lengthmax=").AppendLine(lengthMax.ToString());
            }
            if (image.Length > 0)
            {
                r.Append("image=").AppendLine(image);
            }

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
