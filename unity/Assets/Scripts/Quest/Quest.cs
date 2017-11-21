using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
using ValkyrieTools;
using System.IO;

// Class to manage all data for the current quest
public class Quest
{
    /// <summary>
    /// QuestData
    /// </summary>
    public QuestData qd;

    /// <summary>
    /// Original Quest Path
    /// </summary>
    public string questPath = "";

    /// <summary>
    /// components on the board (tiles, tokens, doors)
    /// </summary>
    public Dictionary<string, BoardComponent> boardItems;

    /// <summary>
    /// vars for the quest 
    /// </summary>
    public VarManager vars;

    // A list of items that have been given to the investigators
    public HashSet<string> items;

    // Dictionary of shops and items
    public Dictionary<string, List<string>> shops;

    // The open shop
    public string activeShop = "";

    // Dictionary of picked items
    public Dictionary<string, string> itemSelect;

    // Dictionary item inspect events
    public Dictionary<string, string> itemInspect;

    // A dictionary of heros that have been selected in events
    public Dictionary<string, List<Quest.Hero>> heroSelection;

    // A dictionary of puzzle state
    public Dictionary<string, Puzzle> puzzle;

    // A count of successes from events
    public Dictionary<string, int> eventQuota;

    // Event manager handles the events
    public EventManager eManager;

    /// <summary>
    /// List of heros and their status
    /// </summary>
    public List<Hero> heroes;

    /// <summary>
    /// List of active monsters and their status
    /// </summary>
    public List<Monster> monsters;

    // Stack of saved game state for undo
    public Stack<string> undo;

    // Event Log
    public List<LogEntry> log;

    // Dictionary of picked monster types
    public Dictionary<string, string> monsterSelect;

    // game state variables
    public MoMPhase phase = MoMPhase.investigator;

    // This is true once heros are selected and the quest is started
    public bool heroesSelected = false;

    // A list of music if custom music has been selected - used for save games
    public List<string> music = new List<string>();

    // Reference back to the game object
    public Game game;

    // Construct a new quest from quest data
    public Quest(QuestData.Quest q)
    {
        game = Game.Get();

        // This happens anyway but we need it to be here before the following code is executed
        game.quest = this;

        // Static data from the quest file
        qd = new QuestData(q);
        questPath = Path.GetDirectoryName(qd.questPath);

        // Initialise data
        boardItems = new Dictionary<string, BoardComponent>();
        vars = new VarManager();
        items = new HashSet<string>();
        shops = new Dictionary<string, List<string>>();
        itemSelect = new Dictionary<string, string>();
        itemInspect = new Dictionary<string, string>();
        monsters = new List<Monster>();
        heroSelection = new Dictionary<string, List<Quest.Hero>>();
        puzzle = new Dictionary<string, Puzzle>();
        eventQuota = new Dictionary<string, int>();
        undo = new Stack<string>();
        log = new List<LogEntry>();
        monsterSelect = new Dictionary<string, string>();

        GenerateItemSelection();
        GenerateMonsterSelection();
        eManager = new EventManager();

        // Populate null hero list, these can then be selected as hero types
        heroes = new List<Hero>();
        for (int i = 1; i <= qd.quest.maxHero; i++)
        {
            heroes.Add(new Hero(null, i));
        }

        // Set quest vars for selected expansions
        foreach (string s in game.cd.GetLoadedPackIDs())
        {
            if (s.Length > 0)
            {
                vars.SetValue("#" + s, 1);
            }
        }
        // Depreciated support for quest formats < 6
        if (game.cd.GetLoadedPackIDs().Contains("MoM1ET") && game.cd.GetLoadedPackIDs().Contains("MoM1EI") && game.cd.GetLoadedPackIDs().Contains("MoM1EM"))
        {
            vars.SetValue("#MoM1E", 1);
        }
        if (game.cd.GetLoadedPackIDs().Contains("CotWT") && game.cd.GetLoadedPackIDs().Contains("CotWI") && game.cd.GetLoadedPackIDs().Contains("CotWM"))
        {
            vars.SetValue("#CotW", 1);
        }
        if (game.cd.GetLoadedPackIDs().Contains("FAT") && game.cd.GetLoadedPackIDs().Contains("FAI") && game.cd.GetLoadedPackIDs().Contains("FAM"))
        {
            vars.SetValue("#FA", 1);
        }

        vars.SetValue("#round", 1);
    }

    public void GenerateItemSelection()
    {
        // Clear shops
        shops = new Dictionary<string, List<string>>();

        // Clear item matches
        itemSelect = new Dictionary<string, string>();

        // Determine fame level
        int fame = 1;
        if (vars.GetValue("$%fame") >= vars.GetValue("$%famenoteworthy")) fame = 2;
        if (vars.GetValue("$%fame") >= vars.GetValue("$%fameimpressive")) fame = 3;
        if (vars.GetValue("$%fame") >= vars.GetValue("$%famecelebrated")) fame = 4;
        if (vars.GetValue("$%fame") >= vars.GetValue("$%fameheroic")) fame = 5;
        if (vars.GetValue("$%fame") >= vars.GetValue("$%famelegendary")) fame = 6;

        // Determine monster types
        bool progress = false;
        bool force = false;
        bool done = false;
        while (!done)
        {
            progress = false;
            foreach (KeyValuePair<string, QuestData.QuestComponent> kv in qd.components)
            {
                QuestData.QItem qItem = kv.Value as QuestData.QItem;
                if (qItem != null)
                {
                    progress |= AttemptItemMatch(qItem, fame, force);
                    if (progress && force) force = false;
                }
            }
            if (!progress && !force)
            {
                force = true;
            }
            else if (!progress && force)
            {
                done = true;
            }
        }
        vars.SetValue("$restock", 0);
    }


    public bool AttemptItemMatch(QuestData.QItem qItem, int fame, bool force = true)
    {
        if (itemSelect.ContainsKey(qItem.sectionName))
        {
            return false;
        }
        if ((qItem.traitpool.Length + qItem.traits.Length) == 0)
        {
            foreach (string t in qItem.itemName)
            {
                if (itemSelect.ContainsKey(t))
                {
                    itemSelect.Add(qItem.sectionName, itemSelect[t]);
                    return true;
                }
                if (t.IndexOf("QItem") == 0)
                {
                    return false;
                }
                // Item type might exist in content packs
                else if (game.cd.items.ContainsKey(t))
                {
                    itemSelect.Add(qItem.sectionName, t);
                    return true;
                }
            }
        }
        else
        {
            // List of exclusions
            List<string> exclude = new List<string>();
            foreach (string t in qItem.itemName)
            {
                if (itemSelect.ContainsKey(t))
                {
                    exclude.Add(itemSelect[t]);
                }
                else if (t.IndexOf("QItem") == 0 && !force)
                {
                    return false;
                }
                else
                {
                    exclude.Add(t);
                }
            }
            exclude.AddRange(items);

            // Start a list of matches
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, ItemData> kv in game.cd.items)
            {
                bool allFound = true;
                foreach (string t in qItem.traits)
                {
                    // Does the item have this trait?
                    if (!kv.Value.ContainsTrait(t))
                    {
                        // Trait missing, exclude monster
                        allFound = false;
                    }
                }

                // Must have one of these traits
                bool oneFound = (qItem.traitpool.Length == 0);
                foreach (string t in qItem.traitpool)
                {
                    // Does the item have this trait?
                    if (kv.Value.ContainsTrait(t))
                    {
                        oneFound = true;
                    }
                }

                bool excludeBool = false;
                foreach (string t in exclude)
                {
                    if (t.Equals(kv.Key)) excludeBool = true;
                }

                bool fameOK = true;
                if (kv.Value.minFame > 0)
                {
                    if (kv.Value.minFame > fame) fameOK = false;
                    if (kv.Value.maxFame < fame) fameOK = false;
                }
                foreach (string t in exclude)
                {
                    if (t.Equals(kv.Key)) excludeBool = true;
                }

                // item has all traits
                if (allFound && oneFound && !excludeBool && fameOK)
                {
                    list.Add(kv.Key);
                }
            }

            if (list.Count == 0)
            {
                game.quest.log.Add(new Quest.LogEntry("Warning: Unable to find an item for QItem: " + qItem.sectionName, true));
                return false;
            }

            // Pick item at random from candidates
            itemSelect.Add(qItem.sectionName, list[Random.Range(0, list.Count)]);
            return true;
        }
        return false;
    }

    public void GenerateMonsterSelection()
    {
        // Determine monster types
        bool progress = false;
        bool force = false;
        bool done = false;
        while (!done)
        {
            progress = false;
            foreach (KeyValuePair<string, QuestData.QuestComponent> kv in qd.components)
            {
                QuestData.Spawn qs = kv.Value as QuestData.Spawn;
                if (qs != null)
                {
                    progress |= AttemptMonsterMatch(qs, force);
                    if (progress && force) force = false;
                }
            }
            if (!progress && !force)
            {
                force = true;
            }
            else if (!progress && force)
            {
                done = true;
            }
        }
    }

    public bool AttemptMonsterMatch(QuestData.Spawn spawn, bool force = true)
    {
        if (monsterSelect.ContainsKey(spawn.sectionName))
        {
            return false;
        }
        if ((spawn.mTraitsPool.Length + spawn.mTraitsRequired.Length) == 0)
        {
            foreach (string t in spawn.mTypes)
            {
                if (monsterSelect.ContainsKey(t))
                {
                    monsterSelect.Add(spawn.sectionName, monsterSelect[t]);
                    return true;
                }
                if (t.IndexOf("Spawn") == 0)
                {
                    return false;
                }
                string monster = t;
                if (monster.IndexOf("Monster") != 0 && monster.IndexOf("CustomMonster") != 0)
                {
                    monster = "Monster" + monster;
                }
                // Monster type might be a unique for this quest
                if (game.quest.qd.components.ContainsKey(monster))
                {
                    monsterSelect.Add(spawn.sectionName, monster);
                    return true;
                }
                // Monster type might exist in content packs, 'Monster' is optional
                else if (game.cd.monsters.ContainsKey(monster))
                {
                    monsterSelect.Add(spawn.sectionName, monster);
                    return true;
                }
            }
        }
        else
        {
            // List of exclusions
            List<string> exclude = new List<string>();
            foreach (string t in spawn.mTypes)
            {
                if (monsterSelect.ContainsKey(t))
                {
                    exclude.Add(monsterSelect[t]);
                }
                else if (t.IndexOf("Spawn") == 0 && !force)
                {
                    return false;
                }
                else
                {
                    exclude.Add(t);
                }
            }

            // Start a list of matches
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
            {
                bool allFound = true;
                foreach (string t in spawn.mTraitsRequired)
                {
                    // Does the monster have this trait?
                    if (!kv.Value.ContainsTrait(t))
                    {
                        // Trait missing, exclude monster
                        allFound = false;
                    }
                }

                // Must have one of these traits
                bool oneFound = (spawn.mTraitsPool.Length == 0);
                foreach (string t in spawn.mTraitsPool)
                {
                    // Does the monster have this trait?
                    if (kv.Value.ContainsTrait(t))
                    {
                        oneFound = true;
                    }
                }

                bool excludeBool = false;
                foreach (string t in exclude)
                {
                    if (t.Equals(kv.Key)) excludeBool = true;
                }

                // Monster has all traits
                if (allFound && oneFound && !excludeBool)
                {
                    list.Add(kv.Key);
                }
            }

            foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
            {
                QuestData.CustomMonster cm = kv.Value as QuestData.CustomMonster;
                if (cm == null) continue;

                MonsterData baseMonster = null;
                string[] traits = cm.traits;
                // Check for content data monster defined as base
                if (game.cd.monsters.ContainsKey(cm.baseMonster))
                {
                    baseMonster = game.cd.monsters[cm.baseMonster];
                    if (traits.Length == 0)
                    {
                        traits = baseMonster.traits;
                    }
                }

                bool allFound = true;
                foreach (string t in spawn.mTraitsRequired)
                {
                    // Does the monster have this trait?
                    if (!InArray(traits, t))
                    {
                        // Trait missing, exclude monster
                        allFound = false;
                    }
                }

                // Must have one of these traits
                bool oneFound = (spawn.mTraitsPool.Length == 0);
                foreach (string t in spawn.mTraitsPool)
                {
                    // Does the monster have this trait?
                    if (InArray(traits, t))
                    {
                        oneFound = true;
                    }
                }

                bool excludeBool = false;
                foreach (string t in exclude)
                {
                    if (t.Equals(kv.Key)) excludeBool = true;
                }

                // Monster has all traits
                if (allFound && oneFound && !excludeBool)
                {
                    list.Add(kv.Key);
                }
            }

            if (list.Count == 0)
            {
                ValkyrieDebug.Log("Error: Unable to find monster of traits specified in event: " + spawn.sectionName);
                Destroyer.MainMenu();
                return false;
            }

            // Pick monster at random from candidates
            monsterSelect.Add(spawn.sectionName, list[Random.Range(0, list.Count)]);
            return true;
        }
        return false;
    }

    public static bool InArray(string[] array, string item)
    {
        foreach (string s in array)
        {
            if (s.Equals(item)) return true;
        }
        return false;
    }

    // Construct a quest from save data string
    // Used for undo
    public Quest(string save)
    {
        LoadQuest(IniRead.ReadFromString(save));
    }

    // Construct a quest from save iniData
    public Quest(IniData saveData)
    {
        LoadQuest(saveData);
    }

    public void ChangeQuest(string path)
    {
        Game game = Game.Get();

        // Clean up everything marked as 'board'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.BOARD))
            Object.Destroy(go);
        game.tokenBoard.tc.Clear();

        phase = MoMPhase.investigator;
        game.cc.gameObject.transform.position = new Vector3(0, 0, -8);

        game.cc.minLimit = false;
        game.cc.maxLimit = false;

        // Set static quest data
        qd = new QuestData(questPath + "/" + path);
        questPath = Path.GetDirectoryName(qd.questPath);

        // Extract packages in case needed
        QuestLoader.ExtractPackages(Game.AppData());

        vars.TrimQuest();

        undo = new Stack<string>();

        // Initialise data
        boardItems = new Dictionary<string, BoardComponent>();
        monsters = new List<Monster>();
        heroSelection = new Dictionary<string, List<Quest.Hero>>();
        puzzle = new Dictionary<string, Puzzle>();
        eventQuota = new Dictionary<string, int>();
        undo = new Stack<string>();
        monsterSelect = new Dictionary<string, string>();
        itemSelect = new Dictionary<string, string>();
        itemInspect = new Dictionary<string, string>();
        shops = new Dictionary<string, List<string>>();
        activeShop = "";

        GenerateMonsterSelection();
        GenerateItemSelection();
        eManager = new EventManager();

        // Set quest vars for selected expansions
        foreach (string s in game.cd.GetLoadedPackIDs())
        {
            if (s.Length > 0)
            {
                vars.SetValue("#" + s, 1);
            }
        }
        vars.SetValue("#round", 1);

        // Set quest flag based on hero count
        int heroCount = 0;
        foreach (Quest.Hero h in heroes)
        {
            h.activated = false;
            h.defeated = false;
            h.selected = false;
            if (h.heroData != null) {
                heroCount++;
                // Create variable to value 1 for each selected Hero
                game.quest.vars.SetValue("#" + h.heroData.sectionName, 1);
            }
        }
        game.quest.vars.SetValue("#heroes", heroCount);

        List<string> music = new List<string>();
        foreach (AudioData ad in game.cd.audio.Values)
        {
            if (ad.ContainsTrait("quest")) music.Add(ad.file);
        }
        game.audioControl.Music(music);

        // Update the screen
        game.monsterCanvas.UpdateList();
        game.heroCanvas.UpdateStatus();

        // Start round events
        eManager.EventTriggerType("StartRound", false);
        // Start the quest (top of stack)
        eManager.EventTriggerType("EventStart", false);
        eManager.TriggerEvent();
        SaveManager.Save(0);
    }

    // Read save data
    public void LoadQuest(IniData saveData)
    {
        game = Game.Get();

        // This happens anyway but we need it to be here before the following code is executed (also needed for loading saves)
        game.quest = this;

        // Start quest music
        List<string> music = new List<string>();
        if (saveData.Get("Music") != null)
        {
            music = new List<string>(saveData.Get("Music").Values);
            List<string> toPlay = new List<string>();
            foreach (string s in music)
            {
                if (game.cd.audio.ContainsKey(s))
                {
                    toPlay.Add(game.cd.audio[s].file);
                }
                else
                {
                    toPlay.Add(Path.GetDirectoryName(qd.questPath) + "/" + s);
                }
            }
            game.audioControl.Music(toPlay, false);
        }
        else
        {
            foreach (AudioData ad in game.cd.audio.Values)
            {
                if (ad.ContainsTrait("quest")) music.Add(ad.file);
            }
            game.audioControl.Music(music);
        }

        // Get state
        bool.TryParse(saveData.Get("Quest", "heroesSelected"), out heroesSelected);
        bool horror;
        bool.TryParse(saveData.Get("Quest", "horror"), out horror);
        if (horror)
        {
            phase = MoMPhase.horror;
        }

        // Set camera
        float camx, camy, camz;
        float.TryParse(saveData.Get("Quest", "camx"), out camx);
        float.TryParse(saveData.Get("Quest", "camy"), out camy);
        float.TryParse(saveData.Get("Quest", "camz"), out camz);
        game.cc.gameObject.transform.position = new Vector3(camx, camy, camz);

        game.cc.minLimit = false;
        if (saveData.Get("Quest", "camxmin").Length > 0)
        {
            game.cc.minLimit = true;
            int.TryParse(saveData.Get("Quest", "camxmin"), out game.cc.minPanX);
            int.TryParse(saveData.Get("Quest", "camymin"), out game.cc.minPanY);
        }

        game.cc.maxLimit = false;
        if (saveData.Get("Quest", "camxmax").Length > 0)
        {
            game.cc.maxLimit = true;
            int.TryParse(saveData.Get("Quest", "camxmax"), out game.cc.maxPanX);
            int.TryParse(saveData.Get("Quest", "camymax"), out game.cc.maxPanY);
        }

        shops = new Dictionary<string, List<string>>();
        if (saveData.Get("Shops") != null)
        {
            foreach (KeyValuePair<string, string> kv in saveData.Get("Shops"))
            {
                List<string> shopList = new List<string>();
                foreach (string s in kv.Value.Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries))
                {
                    shopList.Add(s);
                }
                shops.Add(kv.Key, shopList);
            }
        }

        activeShop = "";
        if (saveData.Get("ActiveShop") != null)
        {
            foreach (KeyValuePair<string, string> kv in saveData.Get("ActiveShop"))
            {
                activeShop = kv.Key;
            }
        }

        // Restore event log
        log = new List<LogEntry>();
        foreach (KeyValuePair<string, string> kv in saveData.Get("Log"))
        {
            log.Add(new LogEntry(kv.Key, kv.Value));
        }

        Dictionary<string, string> saveVars = saveData.Get("Vars");
        vars = new VarManager(saveVars);

        itemSelect = saveData.Get("SelectItem");
        if (itemSelect == null)
        {
            // Support old saves
            itemSelect = new Dictionary<string, string>();
            GenerateItemSelection();
        }

        itemInspect = saveData.Get("ItemInspect");
        if (itemInspect == null)
        {
            itemInspect = new Dictionary<string, string>();
        }

        // Set items
        items = new HashSet<string>();
        Dictionary<string, string> saveItems = saveData.Get("Items");
        foreach (KeyValuePair<string, string> kv in saveItems)
        {
            items.Add(kv.Key);
        }

        // Set static quest data
        qd = new QuestData(saveData.Get("Quest", "path"));

        questPath = saveData.Get("Quest", "originalpath");

        monsterSelect = saveData.Get("SelectMonster");
        if (monsterSelect == null)
        {
            // Support old saves
            monsterSelect = new Dictionary<string, string>();
            GenerateMonsterSelection();
        }

        // Clear all tokens
        game.tokenBoard.Clear();
        // Clean up everything marked as 'board'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.BOARD))
            Object.Destroy(go);

        // Repopulate items on the baord
        boardItems = new Dictionary<string, BoardComponent>();
        Dictionary<string, string> saveBoard = saveData.Get("Board");
        foreach (KeyValuePair<string, string> kv in saveBoard)
        {
            if (kv.Key.IndexOf("Door") == 0)
            {
                boardItems.Add(kv.Key, new Door(qd.components[kv.Key] as QuestData.Door, game));
            }
            if (kv.Key.IndexOf("Token") == 0)
            {
                boardItems.Add(kv.Key, new Token(qd.components[kv.Key] as QuestData.Token, game));
            }
            if (kv.Key.IndexOf("Tile") == 0)
            {
                boardItems.Add(kv.Key, new Tile(qd.components[kv.Key] as QuestData.Tile, game));
            }
            if (kv.Key.IndexOf("UI") == 0)
            {
                boardItems.Add(kv.Key, new UI(qd.components[kv.Key] as QuestData.UI, game));
            }
            if (kv.Key.IndexOf("#shop") == 0)
            {
                boardItems.Add(kv.Key, new ShopInterface(new List<string>(), Game.Get(), activeShop));
            }
        }

        // Clean undo stack (we don't save undo stack)
        // When performing undo this is replaced later
        undo = new Stack<string>();

        // Fetch hero state
        heroes = new List<Hero>();
        monsters = new List<Monster>();
        foreach (KeyValuePair<string, Dictionary<string, string>> kv in saveData.data)
        {
            if (kv.Key.IndexOf("Hero") == 0 && kv.Key.IndexOf("HeroSelection") != 0)
            {
                heroes.Add(new Hero(kv.Value));
            }
        }

        // Monsters must be after heros because some activations refer to heros
        foreach (KeyValuePair<string, Dictionary<string, string>> kv in saveData.data)
        {
            if (kv.Key.IndexOf("Monster") == 0)
            {
                monsters.Add(new Monster(kv.Value));
            }
        }

        // Restore hero selections
        heroSelection = new Dictionary<string, List<Hero>>();
        Dictionary<string, string> saveSelection = saveData.Get("HeroSelection");
        foreach (KeyValuePair<string, string> kv in saveSelection)
        {
            // List of selected heroes
            string[] selectHeroes = kv.Value.Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            List<Hero> heroList = new List<Hero>();

            foreach (string s in selectHeroes)
            {
                foreach (Hero h in heroes)
                {
                    // Match hero id
                    int id;
                    int.TryParse(s, out id);
                    if (id == h.id)
                    {
                        heroList.Add(h);
                    }
                }
            }
            // Add this selection
            heroSelection.Add(kv.Key, heroList);
        }

        puzzle = new Dictionary<string, Puzzle>();
        foreach (KeyValuePair<string, Dictionary<string, string>> kv in saveData.data)
        {
            if (kv.Key.IndexOf("PuzzleSlide") == 0)
            {
                puzzle.Add(kv.Key.Substring("PuzzleSlide".Length, kv.Key.Length - "PuzzleSlide".Length), new PuzzleSlide(kv.Value));
            }
            if (kv.Key.IndexOf("PuzzleCode") == 0)
            {
                puzzle.Add(kv.Key.Substring("PuzzleCode".Length, kv.Key.Length - "PuzzleCode".Length), new PuzzleCode(kv.Value));
            }
            if (kv.Key.IndexOf("PuzzleImage") == 0)
            {
                puzzle.Add(kv.Key.Substring("PuzzleImage".Length, kv.Key.Length - "PuzzleImage".Length), new PuzzleImage(kv.Value));
            }
            if (kv.Key.IndexOf("PuzzleTower") == 0)
            {
                puzzle.Add(kv.Key.Substring("PuzzleTower".Length, kv.Key.Length - "PuzzleTower".Length), new PuzzleTower(kv.Value));
            }
        }
        // Restore event quotas
        eventQuota = new Dictionary<string, int>();
        foreach (KeyValuePair<string, string> kv in saveData.Get("EventQuota"))
        {
            int value;
            int.TryParse(kv.Value, out value);
            eventQuota.Add(kv.Key, value);
        }

        eManager = new EventManager(saveData.Get("EventManager"));

        // Update the screen
        game.stageUI.Update();
        game.monsterCanvas.UpdateList();
        game.heroCanvas.UpdateStatus();
    }

    // Save to the undo stack
    // This is called on user actions (such as defeated monsters, heros activated)
    public void Save()
    {
        undo.Push(ToString());
    }

    // Load from the undo stack
    public void Undo()
    {
        // Nothing to undo
        if (undo.Count == 0) return;
        // Load the old state.  This will also set game.quest
        Quest oldQuest = new Quest(undo.Pop());
        // Transfer the undo stack to the loaded state
        oldQuest.undo = undo;
    }

    // This function adjusts morale.  We don't write directly so that NoMorale can be triggered
    // Delay is used if we can't raise the nomorale event at this point (need to recall this later)
    public void AdjustMorale(int m, bool delay = false)
    {
        Game game = Game.Get();
        
        float morale = vars.GetValue("$%morale") + m;
        vars.SetValue("$%morale", morale);        

        // Test for no morale ending
        if (morale < 0)
        {
            morale = 0;
            game.moraleDisplay.Update();
            // Hold loss during activation
            if (delay)
            {
                morale = -1;
                return;
            }
            eManager.EventTriggerType("NoMorale");
        }
        game.moraleDisplay.Update();
    }

    // Function to return a hero at random
    public Hero GetRandomHero()
    {
        // We have to create a list with the null heroes trimmed
        List<Hero> hList = new List<Hero>();
        foreach (Hero h in heroes)
        {
            if (h.heroData != null)
            {
                hList.Add(h);
            }
        }
        return hList[Random.Range(0, hList.Count)];
    }

    public int GetHeroCount()
    {
        int count = 0;
        foreach (Hero h in heroes)
        {
            if (h.heroData != null)
            {
                count++;
            }
        }
        return count;
    }

    // Add a list of components (token, tile, etc)
    public void Add(string[] names, bool shop = false)
    {
        foreach (string s in names)
        {
            Add(s, shop);
        }
    }

    // Add a component to the board
    public void Add(string name, bool shop = false)
    {
        // Check that the component is valid
        if (!game.quest.qd.components.ContainsKey(name))
        {
            ValkyrieDebug.Log("Error: Unable to create missing quest component: " + name);
            Application.Quit();
        }
        // Add to active list
        QuestData.QuestComponent qc = game.quest.qd.components[name];

        if (boardItems.ContainsKey(name)) return;

        // Add to board
        if (qc is QuestData.Tile)
        {
            boardItems.Add(name, new Tile((QuestData.Tile)qc, game));
        }
        if (qc is QuestData.Door)
        {
            boardItems.Add(name, new Door((QuestData.Door)qc, game));
        }
        if (qc is QuestData.Token)
        {
            boardItems.Add(name, new Token((QuestData.Token)qc, game));
        }
        if (qc is QuestData.UI)
        {
            boardItems.Add(name, new UI((QuestData.UI)qc, game));
        }
        if (qc is QuestData.QItem && !shop)
        {
            if (itemSelect.ContainsKey(name))
            {
                items.Add(itemSelect[name]);
                if (((QuestData.QItem)qc).inspect.Length > 0)
                {
                    if (game.quest.itemInspect.ContainsKey(itemSelect[name]))
                    {
                        game.quest.itemInspect.Remove(itemSelect[name]);
                    }
                    game.quest.itemInspect.Add(itemSelect[name], ((QuestData.QItem)qc).inspect);
                }
            }
        }
    }

    // Remove a list of active components
    public void Remove(string[] names)
    {
        foreach (string s in names)
        {
            Remove(s);
        }
    }

    // Remove an activate component
    public void Remove(string name)
    {
        if (boardItems.ContainsKey(name))
        {
            boardItems[name].Remove();
            boardItems.Remove(name);
        }
        if (monsterSelect.ContainsKey(name))
        {
            List<Monster> toRemove = new List<Monster>();
            foreach (Monster m in monsters)
            {
                if (m.monsterData.sectionName.Equals(monsterSelect[name]))
                {
                    toRemove.Add(m);
                }
            }

            foreach (Monster m in toRemove)
            {
                monsters.Remove(m);
                game.monsterCanvas.UpdateList();
                game.quest.vars.SetValue("#monsters", game.quest.monsters.Count);
            }
        }
        if (itemSelect.ContainsKey(name) && items.Contains(itemSelect[name]))
        {
            items.Remove(itemSelect[name]);
            if (itemInspect.ContainsKey(itemSelect[name]))
            {
                itemInspect.Remove(itemSelect[name]);
            }
        }
        if (name.Equals("#monsters"))
        {
            monsters.Clear();
            game.monsterCanvas.UpdateList();
            game.quest.vars.SetValue("#monsters", 0);
        }
        if (name.Equals("#boardcomponents"))
        {
            foreach (BoardComponent bc in boardItems.Values)
            {
                bc.Remove();
            }
            boardItems.Clear();
        }
    }

    public bool UIItemsPresent()
    {
        foreach (BoardComponent c in boardItems.Values)
        {
            if (c is UI) return true;
            if (c is ShopInterface) return true;
        }
        return false;
    }

    // Remove all active components
    public void RemoveAll()
    {
        foreach (KeyValuePair<string, BoardComponent> kv in boardItems)
        {
            kv.Value.Remove();
        }

        boardItems.Clear();
    }

    // Change the transparency of a component on the board
    public void ChangeAlpha(string name, float alpha)
    {
        // Check is component is active
        if (!boardItems.ContainsKey(name)) return;
        boardItems[name].SetVisible(alpha);
    }

    // Change the transparency of all baord components
    public void ChangeAlphaAll(float alpha)
    {
        foreach (KeyValuePair<string, BoardComponent> kv in boardItems)
        {
            kv.Value.SetVisible(alpha);
        }
    }

    public void Update()
    {
        foreach (KeyValuePair<string, BoardComponent> kv in boardItems)
        {
            kv.Value.UpdateAlpha(Time.deltaTime);
        }
    }

    // Save the quest state to a string for save games and undo
    override public string ToString()
    {
        //Game game = Game.Get();
        string nl = System.Environment.NewLine;
        // General quest state block
        string r = "[Quest]" + nl;

        r += "time=" + System.DateTime.Now.ToString() + nl;

        // Save valkyrie version
        r += "valkyrie=" + game.version + nl;

        r += "path=" + qd.questPath + nl;
        r += "originalpath=" + questPath + nl;
        if (phase == MoMPhase.horror)
        {
            r += "horror=true" + nl;
        }
        else
        {
            r += "horror=false" + nl;
        }
        r += "heroesSelected=" + heroesSelected + nl;
        r += "camx=" + game.cc.gameObject.transform.position.x + nl;
        r += "camy=" + game.cc.gameObject.transform.position.y + nl;
        r += "camz=" + game.cc.gameObject.transform.position.z + nl;
        if (game.cc.minLimit)
        {
            r += "camxmin=" + game.cc.minPanX + nl;
            r += "camymin=" + game.cc.minPanY + nl;
        }
        if (game.cc.maxLimit)
        {
            r += "camxmax=" + game.cc.maxPanX + nl;
            r += "camymax=" + game.cc.maxPanY + nl;
        }

        r += "[Packs]" + nl;
        foreach (string pack in game.cd.GetLoadedPackIDs())
        {
            r += pack + nl;
        }

        r += "[Board]" + nl;
        foreach (KeyValuePair<string, BoardComponent> kv in boardItems)
        {
            r += kv.Key + nl;
        }

        r += vars.ToString();

        r += "[Items]" + nl;
        foreach (string s in items)
        {
            r += s + nl;
        }

        // Hero selection is a list of selections, each with space separated hero lists
        r += "[HeroSelection]" + nl;
        foreach (KeyValuePair<string, List<Quest.Hero>> kv in heroSelection)
        {
            r += kv.Key + "=";
            foreach (Quest.Hero h in kv.Value)
            {
                r += h.id + " ";
            }
            r = r.Substring(0, r.Length - 1) + nl;
        }

        // Save event quotas
        r += "[EventQuota]" + nl;
        foreach (KeyValuePair<string, int> kv in eventQuota)
        {
            r += kv.Key + "=" + kv.Value + nl;
        }

        // Save hero state
        foreach (Hero h in heroes)
        {
            r += h.ToString();
        }

        // Save monster state
        foreach (Monster m in monsters)
        {
            r += m.ToString();
        }

        foreach (KeyValuePair<string, Puzzle> kv in puzzle)
        {
            r += kv.Value.ToString(kv.Key);
        }

        r += "[Log]" + nl;
        int i = 0;
        foreach (LogEntry e in log)
        {
            r += e.ToString(i++);
        }

        r += "[SelectMonster]" + nl;
        foreach (KeyValuePair<string, string> kv in monsterSelect)
        {
            r += kv.Key + "=" + kv.Value + nl;
        }

        r += "[SelectItem]" + nl;
        foreach (KeyValuePair<string, string> kv in itemSelect)
        {
            r += kv.Key + "=" + kv.Value + nl;
        }

        r += "[ItemInspect]" + nl;
        foreach (KeyValuePair<string, string> kv in itemInspect)
        {
            r += kv.Key + "=" + kv.Value + nl;
        }

        if (activeShop.Length > 0)
        {
            r += "[ActiveShop]" + nl;
            r += activeShop + nl;
        }

        r += eManager.ToString();

        r += "[Shops]" + nl;
        foreach (KeyValuePair<string, List<string>> kv in shops)
        {
            r += kv.Key + "=";
            foreach (string s in kv.Value)
            {
                r += s + " ";
            }
            r = r.Substring(0, r.Length - 1) + nl;
        }

        if (music.Count > 0)
        {
            r += "[Music]" + nl;
            for (int j = 0; j < music.Count; j++)
            {
                r += "track" + j + "=" + music[j] + nl;
            }
            r += nl;
        }

        return r;
    }

    // Class for Tile components (use TileSide content data)
    public class Tile : BoardComponent
    {
        // This is the quest information
        public QuestData.Tile qTile;
        // This is the component information
        public TileSideData cTile;

        // Construct with data from the quest, pass Game for speed
        public Tile(QuestData.Tile questTile, Game gameObject) : base(gameObject)
        {
            qTile = questTile;

            // Search for tile in content
            if (game.cd.tileSides.ContainsKey(qTile.tileSideName))
            {
                cTile = game.cd.tileSides[qTile.tileSideName];
            }
            else if (game.cd.tileSides.ContainsKey("TileSide" + qTile.tileSideName))
            {
                cTile = game.cd.tileSides["TileSide" + qTile.tileSideName];
            }
            else
            {
                // Fatal if not found
                ValkyrieDebug.Log("Error: Failed to located TileSide: " + qTile.tileSideName + " in quest component: " + qTile.sectionName);
                Application.Quit();
            }

            // Attempt to load image
            Texture2D newTex = ContentData.FileToTexture(game.cd.tileSides[qTile.tileSideName].image);
            if (newTex == null)
            {
                // Fatal if missing
                ValkyrieDebug.Log("Error: cannot open image file for TileSide: " + game.cd.tileSides[qTile.tileSideName].image);
                Application.Quit();
            }

            // Create a unity object for the tile
            unityObject = new GameObject("Object" + qTile.sectionName);
            unityObject.tag = Game.BOARD;
            unityObject.transform.SetParent(game.boardCanvas.transform);

            // Add image to object
            image = unityObject.AddComponent<UnityEngine.UI.Image>();
            // Create sprite from texture
            Sprite tileSprite = null;
            if (game.gameType is MoMGameType)
            {
                // This is faster
                tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect);
            }
            else
            {
                tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            }
            // Set image sprite
            image.sprite = tileSprite;
            // Move to get the top left square corner at 0,0
            float vPPS = game.cd.tileSides[qTile.tileSideName].pxPerSquare;
            float hPPS = vPPS;
            // manual aspect control
            // We need this for the 3x2 MoM tiles because they don't have square pixels!!
            if (game.cd.tileSides[qTile.tileSideName].aspect != 0)
            {
                hPPS = (vPPS * newTex.width / newTex.height) / game.cd.tileSides[qTile.tileSideName].aspect;
            }

            // Perform alignment move
            unityObject.transform.Translate(Vector3.right * ((newTex.width / 2) - cTile.left) / hPPS, Space.World);
            unityObject.transform.Translate(Vector3.down * ((newTex.height / 2) - cTile.top) / vPPS, Space.World);
            // Move to get the middle of the top left square at 0,0
            // We don't do this for MoM because it spaces differently
            if (game.gameType.TileOnGrid())
            {
                unityObject.transform.Translate(new Vector3(-(float)0.5, (float)0.5, 0), Space.World);
            }
            // Set the size to the image size
            image.rectTransform.sizeDelta = new Vector2((float)newTex.width / hPPS, (float)newTex.height / vPPS);

            // Rotate around 0,0 rotation amount
            unityObject.transform.RotateAround(Vector3.zero, Vector3.forward, qTile.rotation);
            // Move tile into target location (Space.World is needed because tile has been rotated)
            unityObject.transform.Translate(new Vector3(qTile.location.x, qTile.location.y, 0), Space.World);
            image.color = new Color(1, 1, 1, 0);
        }

        // Remove this tile
        public override void Remove()
        {
            Object.Destroy(unityObject);
        }

        // Tiles are not interactive, no event
        public override QuestData.Event GetEvent()
        {
            return null;
        }
    }

    // Tokens are events that are tied to a token placed on the board
    public class Token : BoardComponent
    {
        // Quest info on the token
        public QuestData.Token qToken;

        // Construct with quest info and reference to Game
        public Token(QuestData.Token questToken, Game gameObject) : base(gameObject)
        {
            qToken = questToken;

            string tokenName = qToken.tokenName;
            // Check that token exists
            if (!game.cd.tokens.ContainsKey(tokenName))
            {
                game.quest.log.Add(new Quest.LogEntry("Warning: Quest component " + qToken.sectionName + " is using missing token type: " + tokenName, true));
                // Catch for older quests with different types (0.4.0 or older)
                if (game.cd.tokens.ContainsKey("TokenSearch"))
                {
                    tokenName = "TokenSearch";
                }
            }

            // Get texture for token
            Vector2 texPos = new Vector2(game.cd.tokens[tokenName].x, game.cd.tokens[tokenName].y);
            Vector2 texSize = new Vector2(game.cd.tokens[tokenName].width, game.cd.tokens[tokenName].height);
            Texture2D newTex = ContentData.FileToTexture(game.cd.tokens[tokenName].image, texPos, texSize);

            // Create object
            unityObject = new GameObject("Object" + qToken.sectionName);
            unityObject.tag = Game.BOARD;

            unityObject.transform.SetParent(game.tokenCanvas.transform);

            // Create the image
            image = unityObject.AddComponent<UnityEngine.UI.Image>();
            Sprite tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            image.color = new Color(1, 1, 1, 0);
            image.sprite = tileSprite;

            float PPS = game.cd.tokens[tokenName].pxPerSquare;
            if (PPS == 0)
            {
                PPS = (float)newTex.width;
            }

            // Set the size to the image size
            image.rectTransform.sizeDelta = new Vector2((float)newTex.width / PPS, (float)newTex.height / PPS);
            // Rotate around 0,0 rotation amount
            unityObject.transform.RotateAround(Vector3.zero, Vector3.forward, qToken.rotation);
            // Move to square
            unityObject.transform.Translate(new Vector3(qToken.location.x, qToken.location.y, 0), Space.World);

            game.tokenBoard.Add(this);
        }

        // Tokens have an associated event to start on press
        public override QuestData.Event GetEvent()
        {
            return qToken;
        }

        // Clean up
        public override void Remove()
        {
            Object.Destroy(unityObject);
        }
    }

    // Tokens are events that are tied to a token placed on the board
    public class UI : BoardComponent
    {
        // Quest info on the token
        public QuestData.UI qUI;
        public UIElementBorder border;

        UnityEngine.UI.Text uiText;

        // Construct with quest info and reference to Game
        public UI(QuestData.UI questUI, Game gameObject) : base(gameObject)
        {
            qUI = questUI;

            // Find quest UI panel
            GameObject panel = GameObject.Find("QuestUIPanel");
            if (panel == null)
            {
                // Create UI Panel
                panel = new GameObject("QuestUIPanel");
                panel.tag = Game.BOARD;
                panel.transform.SetParent(game.uICanvas.transform);
                panel.transform.SetAsFirstSibling();
                panel.AddComponent<RectTransform>();
                panel.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, Screen.height);
                panel.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, Screen.width);
            }

            Texture2D newTex = null;
            if (game.cd.images.ContainsKey(qUI.imageName))
            {
                Vector2 texPos = new Vector2(game.cd.images[qUI.imageName].x, game.cd.images[qUI.imageName].y);
                Vector2 texSize = new Vector2(game.cd.images[qUI.imageName].width, game.cd.images[qUI.imageName].height);
                newTex = ContentData.FileToTexture(game.cd.images[qUI.imageName].image, texPos, texSize);
            }
            else if (qUI.imageName.Length > 0)
            {
                newTex = ContentData.FileToTexture(Path.GetDirectoryName(game.quest.qd.questPath) + "/" + qUI.imageName);
            }

            // Create object
            unityObject = new GameObject("Object" + qUI.sectionName);
            unityObject.tag = Game.BOARD;

            unityObject.transform.SetParent(panel.transform);

            float aspect = 1;
            RectTransform rectTransform = unityObject.AddComponent<RectTransform>();
            if (qUI.imageName.Length == 0)
            {
                uiText = unityObject.AddComponent<UnityEngine.UI.Text>();
                uiText.text = GetText();
                uiText.alignment = TextAnchor.MiddleCenter;
                uiText.font = game.gameType.GetFont();
                uiText.material = uiText.font.material;
                uiText.fontSize = Mathf.RoundToInt(UIScaler.GetPixelsPerUnit() * qUI.textSize);
                SetColor(qUI.textColor);
                aspect = qUI.aspect;
            }
            else
            {
                // Create the image
                image = unityObject.AddComponent<UnityEngine.UI.Image>();
                Sprite tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
                image.color = new Color(1, 1, 1, 0);
                image.sprite = tileSprite;
                aspect = (float)newTex.width / (float)newTex.height;
            }

            float unitScale = Screen.width;
            float hSize = qUI.size * unitScale;
            float vSize = hSize / aspect;
            if (qUI.verticalUnits)
            {
                unitScale = Screen.height;
                vSize = qUI.size * unitScale;
                hSize = vSize * aspect;
            }

            float hOffset = qUI.location.x * unitScale;
            float vOffset = qUI.location.y * unitScale;

            if (qUI.hAlign < 0)
            {
                rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, hOffset, hSize);
            }
            else if (qUI.hAlign > 0)
            {
                rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, hOffset, hSize);
            }
            else
            {
                rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, hOffset + ((Screen.width - hSize) / 2f), hSize);
            }

            if (qUI.vAlign < 0)
            {
                rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, vOffset, vSize);
            }
            else if (qUI.vAlign > 0)
            {
                rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, vOffset, vSize);
            }
            else
            {
                rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, vOffset + ((Screen.height - vSize) / 2f), vSize);
            }

            if (qUI.border)
            {
                border = new UIElementBorder(unityObject.transform, rectTransform, Game.BOARD, uiText.color);
            }

            game.tokenBoard.Add(this);
        }

        // Tokens have an associated event to start on press
        public override QuestData.Event GetEvent()
        {
            return qUI;
        }

        override public void SetColor(Color c)
        {
            if (image != null && image.gameObject != null) image.color = c;
            if (uiText != null && uiText.gameObject != null) uiText.color = c;
            if (border != null) border.SetColor(c);
        }

        override public Color GetColor()
        {
            if (image != null) return image.color;
            if (uiText != null) return uiText.color;
            return Color.clear;
        }

        // Set visible can control the transparency level of the component
        public override void SetVisible(float alpha)
        {
            targetAlpha = alpha;
            // Hide in editor
            if (targetAlpha < 0.5f)
            {
                targetAlpha = 0;
            }
        }

        // Get the text to display on the UI
        virtual public string GetText()
        {
            string text = qUI.uiText.Translate(true);

            // Fix new lines and replace symbol text with special characters
            return EventManager.OutputSymbolReplace(text).Replace("\\n", "\n");
        }

        // Clean up
        public override void Remove()
        {
            Object.Destroy(unityObject);
        }
    }

    // Doors are like tokens but placed differently and have different defaults
    // Note that MoM Explore tokens are tokens and do not use this
    public class Door : BoardComponent
    {
        public QuestData.Door qDoor;

        // Constuct with quest data and reference to Game
        public Door(QuestData.Door questDoor, Game gameObject) : base(gameObject)
        {
            qDoor = questDoor;

            // Load door texture, should be game specific
            Texture2D newTex = Resources.Load("sprites/door") as Texture2D;
            // Check load worked
            if (newTex == null)
            {
                ValkyrieDebug.Log("Error: Cannot load door image");
                Application.Quit();
            }

            // Create object
            unityObject = new GameObject("Object" + qDoor.sectionName);
            unityObject.tag = Game.BOARD;

            unityObject.transform.SetParent(game.tokenCanvas.transform);

            // Create the image
            image = unityObject.AddComponent<UnityEngine.UI.Image>();
            Sprite tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect);
            // Set door colour
            image.sprite = tileSprite;
            image.rectTransform.sizeDelta = new Vector2(0.4f, 1.6f);
            // Rotate as required
            unityObject.transform.RotateAround(Vector3.zero, Vector3.forward, qDoor.rotation);
            // Move to square
            unityObject.transform.Translate(new Vector3(-(float)0.5, (float)0.5, 0), Space.World);
            unityObject.transform.Translate(new Vector3(qDoor.location.x, qDoor.location.y, 0), Space.World);

            // Set the texture colour from quest data
            SetColor(qDoor.colourName);

            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);

            game.tokenBoard.Add(this);
        }

        // Clean up
        public override void Remove()
        {
            Object.Destroy(unityObject);
        }

        // Doors have events that start when pressed
        public override QuestData.Event GetEvent()
        {
            return qDoor;
        }
    }


    // Super class for all quest components
    abstract public class BoardComponent
    {
        // image for display
        public UnityEngine.UI.Image image;

        // Game object
        public Game game;

        public GameObject unityObject;

        // Target alpha
        public float targetAlpha = 1f;

        public BoardComponent(Game gameObject)
        {
            game = gameObject;
        }

        abstract public void Remove();

        abstract public QuestData.Event GetEvent();

        // Set visible can control the transparency level of the component
        virtual public void SetVisible(float alpha)
        {
            targetAlpha = alpha;
        }

        // Set visible can control the transparency level of the component
        virtual public void UpdateAlpha(float time)
        {
            float alpha = GetColor().a;
            float distUpdate = time;
            float distRemain = targetAlpha - alpha;
            if (distRemain > distUpdate)
            {
                alpha += distUpdate;
            }
            else if (-distRemain > distUpdate)
            {
                alpha -= distUpdate;
            }
            else
            {
                alpha = targetAlpha;
            }
            SetColor(new Color(GetColor().r, GetColor().g, GetColor().b, alpha));
        }

        virtual public void SetColor(Color c)
        {
            if (image == null)
                return;
            if (image.gameObject == null)
                return;
            image.color = c;
        }

        virtual public Color GetColor()
        {
            if (image != null) return image.color;
            return Color.clear;
        }

        // Function to set color from string
        public void SetColor(string colorName)
        {
            // Translate from name to #RRGGBB, will return input if already #RRGGBB
            string colorRGB = ColorUtil.FromName(colorName);
            // Check format is valid
            if ((colorRGB.Length != 7) || (colorRGB[0] != '#'))
            {
                game.quest.log.Add(new Quest.LogEntry("Warning: Color must be in #RRGGBB format or a known name: " + colorName, true));
            }

            // State with white (used for alpha)
            Color colour = Color.white;
            // Hexadecimal to float convert (0x00-0xFF -> 0.0-1.0)
            colour[0] = (float)System.Convert.ToInt32(colorRGB.Substring(1, 2), 16) / 255f;
            colour[1] = (float)System.Convert.ToInt32(colorRGB.Substring(3, 2), 16) / 255f;
            colour[2] = (float)System.Convert.ToInt32(colorRGB.Substring(5, 2), 16) / 255f;
            SetColor(colour);
        }
    }

    // Class for holding current hero status
    public class Hero
    {
        // This can be null if not selected
        public HeroData heroData;
        public bool activated = false;
        public bool defeated = false;
        //  Heros are in a list so they need ID... maybe at some point this can move to an array
        public int id = 0;
        // Used for events that can select or highlight heros
        public bool selected;
        public string className = "";
        public string hybridClass = "";
        public List<string> skills;
        public int xp = 0;

        // Constuct with content hero data and an index for hero
        public Hero(HeroData h, int i)
        {
            heroData = h;
            id = i;
            skills = new List<string>();
        }

        // Construct with saved data
        public Hero(Dictionary<string, string> data)
        {
            bool.TryParse(data["activated"], out activated);
            bool.TryParse(data["defeated"], out defeated);
            int.TryParse(data["id"], out id);
            if (data.ContainsKey("class"))
            {
                string[] classes = data["class"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                className = classes[0];
                if (classes.Length > 1)
                {
                    hybridClass = classes[1];
                }
            }

            Game game = Game.Get();
            // Saved content reference, look it up
            if (data.ContainsKey("type"))
            {
                foreach (KeyValuePair<string, HeroData> hd in game.cd.heroes)
                {
                    if (hd.Value.sectionName.Equals(data["type"]))
                    {
                        heroData = hd.Value;
                    }
                }
            }
            skills = new List<string>();
            if (data.ContainsKey("skills"))
            {
                skills.AddRange(data["skills"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries));
            }
            if (data.ContainsKey("xp"))
            {
                int.TryParse(data["xp"], out xp);
            }
        }

        public int AvailableXP()
        {
            Game game = Game.Get();
            int aXP = xp + Mathf.RoundToInt(game.quest.vars.GetValue("$%xp"));
            foreach (string s in skills)
            {
                aXP -= game.cd.skills[s].xp;
            }
            return aXP;
        }

        // Save hero to string for saves/undo
        override public string ToString()
        {
            string nl = System.Environment.NewLine;

            string r = "[Hero" + id + "]" + nl;
            r += "id=" + id + nl;
            r += "activated=" + activated + nl;
            r += "defeated=" + defeated + nl;
            if (className.Length > 0)
            {
                r += "class=" + className + " " + hybridClass + nl;
            }
            if (heroData != null)
            {
                r += "type=" + heroData.sectionName + nl;
            }
            if (skills.Count > 0)
            {
                r += "skills=";
                foreach (string s in skills)
                {
                    r += s + ' ';
                }
                r += nl;
            }

            if (xp != 0)
            {
                r += "xp=" + xp + nl;
            }

            return r + nl;
        }
    }

    // Class for holding current monster status
    public class Monster
    {
        // Content Data
        public MonsterData monsterData;
        // What dulpicate number is the monster?
        public int duplicate = 0;

        // State
        public bool activated = false;
        public bool minionStarted = false;
        public bool masterStarted = false;
        // Accumulated damage
        public int damage = 0;

        // Quest specific info
        public bool unique = false;
        public StringKey uniqueText = StringKey.NULL;
        public StringKey uniqueTitle = StringKey.NULL;

        public int healthMod = 0;

        // Activation is reset each round so that master/minion use the same data and forcing doesn't re roll
        // Note that in RtL forcing activation WILL reroll the selected activation
        public ActivationInstance currentActivation;

        // Initialise from monster event
        // When an event adds a monster group this is called
        public Monster(EventManager.MonsterEvent monsterEvent)
        {
            monsterData = monsterEvent.cMonster;
            unique = monsterEvent.qMonster.unique;
            uniqueTitle = monsterEvent.GetUniqueTitle();
            uniqueText = monsterEvent.qMonster.uniqueText;
            healthMod = Mathf.RoundToInt(monsterEvent.qMonster.uniqueHealthBase + (Game.Get().quest.GetHeroCount() * monsterEvent.qMonster.uniqueHealthHero));

            Game game = Game.Get();
            HashSet<int> dupe = new HashSet<int>();
            foreach (Monster m in game.quest.monsters)
            {
                if (m.monsterData == monsterData || m.duplicate != 0)
                {
                    dupe.Add(m.duplicate);
                }
            }

            while (dupe.Contains(duplicate))
            {
                duplicate++;
            }
        }

        // Create new activation
        public void NewActivation(ActivationData contentActivation)
        {
            currentActivation = new ActivationInstance(contentActivation, monsterData.name.Translate());
        }

        // Construct from save data
        public Monster(Dictionary<string, string> data)
        {
            bool.TryParse(data["activated"], out activated);
            bool.TryParse(data["minionStarted"], out minionStarted);
            bool.TryParse(data["masterStarted"], out masterStarted);
            bool.TryParse(data["unique"], out unique);
            int.TryParse(data["damage"], out damage);
            int.TryParse(data["duplicate"], out duplicate);

            if (data.ContainsKey("healthmod"))
            {
                int.TryParse(data["healthmod"], out healthMod);
            }

            uniqueText = new StringKey(data["uniqueText"]);
            uniqueTitle = new StringKey(data["uniqueTitle"]);

            // Support old saves (deprectiated)
            if (data["type"].IndexOf("UniqueMonster") == 0)
            {
                data["type"] = "CustomMonster" + data["type"].Substring("UniqueMonster".Length);
            }

            // Find base type
            Game game = Game.Get();
            if (game.cd.monsters.ContainsKey(data["type"]))
            {
                monsterData = game.cd.monsters[data["type"]];
            }
            // Check if type is a special quest specific type
            if (game.quest.qd.components.ContainsKey(data["type"]) && game.quest.qd.components[data["type"]] is QuestData.CustomMonster)
            {
                monsterData = new QuestMonster(game.quest.qd.components[data["type"]] as QuestData.CustomMonster);
            }

            // If we have already selected an activation find it
            if (data.ContainsKey("activation"))
            {
                ActivationData saveActivation = null;
                if (game.cd.activations.ContainsKey(data["activation"]))
                {
                    saveActivation = game.cd.activations[data["activation"]];
                }
                // Activations can be specific to the quest
                if (game.quest.qd.components.ContainsKey(data["activation"]))
                {
                    saveActivation = new QuestActivation(game.quest.qd.components[data["activation"]] as QuestData.Activation);
                }
                currentActivation = new ActivationInstance(saveActivation, monsterData.name.Translate());
            }
        }

        public string GetIdentifier()
        {
            return monsterData.sectionName + ":" + duplicate;
        }

        public static Monster GetMonster(string identifier)
        {
            Game game = Game.Get();
            string[] parts = identifier.Split(':');
            if (parts.Length != 2) return null;
            int d = 0;
            int.TryParse(parts[1], out d);
            foreach (Monster m in game.quest.monsters)
            {
                if (m.monsterData.sectionName.Equals(parts[0]) && m.duplicate == d)
                {
                    return m;
                }
            }
            return null;
        }

        public int GetHealth()
        {
            return Mathf.RoundToInt(monsterData.healthBase + (Game.Get().quest.GetHeroCount() * monsterData.healthPerHero)) + healthMod;
        }

        // Activation instance is requresd to track variables in the activation
        public class ActivationInstance
        {
            public ActivationData ad;
            // String is populated on creation of the activation
            public string effect;
            public string move;
            public string minionActions;
            public string masterActions;

            // Construct activation
            public ActivationInstance(ActivationData contentActivation, string monsterName)
            {
                ad = contentActivation;
                minionActions = EventManager.OutputSymbolReplace(ad.minionActions.Translate().Replace("{0}", monsterName));
                masterActions = EventManager.OutputSymbolReplace(ad.masterActions.Translate().Replace("{0}", monsterName));

                // Fill in hero, monster names
                // Note: Random hero selection is NOT kept on load/undo FIXME
                if (Game.Get().gameType is MoMGameType)
                {
                    effect = ad.ability.Translate().Replace("{0}", monsterName);
                    move = ad.move.Translate().Replace("{0}", monsterName);
                    move = EventManager.OutputSymbolReplace(move).Replace("\\n", "\n");
                }
                else
                {
                    effect = ad.ability.Translate().Replace("{0}", Game.Get().quest.GetRandomHero().heroData.name.Translate());
                    effect = effect.Replace("{1}", monsterName);
                }
                // Fix new lines
                effect = EventManager.OutputSymbolReplace(effect).Replace("\\n", "\n");
            }
        }

        // Save monster data to string
        override public string ToString()
        {
            string nl = System.Environment.NewLine;

            // Section name must be unique
            string r = "[Monster" + monsterData.sectionName + duplicate + "]" + nl;
            r += "activated=" + activated + nl;
            r += "type=" + monsterData.sectionName + nl;
            r += "minionStarted=" + minionStarted + nl;
            r += "masterStarted=" + masterStarted + nl;
            r += "unique=" + unique + nl;
            r += "uniqueText=" + uniqueText + nl;
            r += "uniqueTitle=" + uniqueTitle + nl;
            r += "damage=" + damage + nl;
            r += "duplicate=" + duplicate + nl;
            r += "healthmod=" + healthMod + nl;
            // Save the activation (currently doesn't save the effect string)
            if (currentActivation != null)
            {
                r += "activation=" + currentActivation.ad.sectionName + nl;
            }
            return r;
        }
    }

    public enum MoMPhase
    {
        investigator,
        mythos,
        monsters,
        horror
    }

    public class LogEntry
    {
        string entry;
        bool valkyrie = false;
        bool editor = false;

        public LogEntry(string e, bool editorIn = false, bool valkyrieIn = false)
        {
            entry = e;
            editor = editorIn;
            valkyrie = valkyrieIn;
        }

        public LogEntry(string type, string e)
        {
            if (type.IndexOf("valkyrie") == 0)
            {
                valkyrie = true;
            }
            if (type.IndexOf("editor") == 0)
            {
                editor = true;
            }
            entry = e;
        }

        public string ToString(int id)
        {
            string r = "";
            if (valkyrie)
            {
                r += "valkyrie" + id + "=";
            }
            else if (editor)
            {
                r += "editor" + id + "=";
            }
            else
            {
                r += "quest" + id + "=";
            }
            r += entry.Replace("\n", "\\n") + System.Environment.NewLine;
            return r;
        }

        public string GetEntry(bool editorSet = false)
        {
            if (valkyrie && !Application.isEditor)
            {
                return "";
            }
            if (editor && !editorSet)
            {
                return "";
            }
            return entry.Replace("\\n", "\n") + "\n\n";
        }
    }
}

