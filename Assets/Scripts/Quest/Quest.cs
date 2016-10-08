using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// Class to manage all data for the current quest
public class Quest
{
    // QuestData
    public QuestData qd;

    // components on the board (tiles, tokens, doors)
    public Dictionary<string, BoardComponent> boardItems;

    // A list of flags that have been set during the quest
    public HashSet<string> flags;

    // A dictionary of heros that have been selected in events
    public Dictionary<string, List<Quest.Hero>> heroSelection;

    // Event manager handles the events
    public EventManager eManager;

    public List<QuestData.Event.DelayedEvent> delayedEvents;

    public List<Hero> heroes;
    public List<Monster> monsters;

    public Stack<string> undo;

    public int round = 1;
    public int morale = 0;
    public float threat = 0;
    public bool horrorPhase = false;

    public bool heroesSelected = false;
    public bool minorPeril = false;
    public bool majorPeril = false;
    public bool deadlyPeril = false;

    public Game game;

    public Quest(QuestLoader.Quest q)
    {
        game = Game.Get();

        // This happens anyway but we need it to be here before the following code is executed
        game.quest = this;

        qd = new QuestData(q);
        boardItems = new Dictionary<string, BoardComponent>();
        flags = new HashSet<string>();
        monsters = new List<Monster>();
        heroSelection = new Dictionary<string, List<Quest.Hero>>();
        eManager = new EventManager();
        delayedEvents = new List<QuestData.Event.DelayedEvent>();
        undo = new Stack<string>();

        // Populate null hero list, these can then be selected as hero types
        heroes = new List<Hero>();
        for (int i = 1; i <= game.gameType.MaxHeroes(); i++)
        {
            heroes.Add(new Hero(null, i));
        }

        Dictionary<string, string> packs = game.config.data.Get(game.gameType.TypeName() + "Packs");
        if (packs != null)
        {
            foreach (KeyValuePair<string, string> kv in packs)
            {
                flags.Add("#" + kv.Key);
            }
        }
    }

    public Quest(string save)
    {
        LoadQuest(IniRead.ReadFromString(save));
    }

    public Quest(IniData saveData)
    {
        LoadQuest(saveData);
    }

    public void LoadQuest(IniData saveData)
    {
        game = Game.Get();

        // This happens anyway but we need it to be here before the following code is executed (also needed for loading saves)
        game.quest = this;

        int.TryParse(saveData.Get("Quest", "round"), out round);
        int.TryParse(saveData.Get("Quest", "morale"), out morale);
        bool.TryParse(saveData.Get("Quest", "heroesSelected"), out heroesSelected);
        bool.TryParse(saveData.Get("Quest", "horror"), out horrorPhase);
        bool.TryParse(saveData.Get("Quest", "minorPeril"), out minorPeril);
        bool.TryParse(saveData.Get("Quest", "majorPeril"), out majorPeril);
        bool.TryParse(saveData.Get("Quest", "deadlyPeril"), out deadlyPeril);

        delayedEvents = new List<QuestData.Event.DelayedEvent>();
        string[] saveDelayed = saveData.Get("Quest", "DelayedEvents").Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string de in saveDelayed)
        {
            delayedEvents.Add(new QuestData.Event.DelayedEvent(de));
        }

        string questPath = saveData.Get("Quest", "path");
        qd = new QuestData(questPath);

        game.tokenBoard.Clear();
        // Clean up everything marked as 'board'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("board"))
            Object.Destroy(go);

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
        }

        flags = new HashSet<string>();
        Dictionary<string, string> saveFlags = saveData.Get("Flags");
        foreach (KeyValuePair<string, string> kv in saveFlags)
        {
            flags.Add(kv.Key);
        }

        eManager = new EventManager();
        undo = new Stack<string>();

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

        heroSelection = new Dictionary<string, List<Hero>>();
        Dictionary<string, string> saveSelection = saveData.Get("HeroSelection");
        foreach (KeyValuePair<string, string> kv in saveSelection)
        {
            string[] selectHeroes = kv.Value.Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            List<Hero> heroList = new List<Hero>();

            foreach (string s in selectHeroes)
            {
                foreach (Hero h in heroes)
                {
                    int id;
                    int.TryParse(s, out id);
                    if (id == h.id)
                    {
                        heroList.Add(h);
                    }
                }
            }
            heroSelection.Add(kv.Key, heroList);
        }
        game.monsterCanvas.UpdateList();
        game.heroCanvas.UpdateStatus();
    }

    public void Save()
    {
        undo.Push(ToString());
    }

    public void Undo()
    {
        if (undo.Count == 0) return;
        Quest oldQuest = new Quest(undo.Pop());
        oldQuest.undo = undo;
    }

    // This function adjusts morale.  We don't write directly so that NoMorale can be triggered
    public void AdjustMorale(int m)
    {
        Game game = Game.Get();
        morale += m;
        if (morale < 0)
        {
            morale = 0;
            game.moraleDisplay.Update();
            eManager.EventTriggerType("NoMorale");
        }
        game.moraleDisplay.Update();
    }

    public Hero GetRandomHero()
    {
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

    public void Add(string[] names)
    {
        foreach (string s in names)
        {
            Add(s);
        }
    }

    public void Add(string name)
    {
        if (!game.quest.qd.components.ContainsKey(name))
        {
            Debug.Log("Error: Unable to create missing quest component: " + name);
            Application.Quit();
        }
        QuestData.QuestComponent qc = game.quest.qd.components[name];

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
    }

    public void Remove(string[] names)
    {
        foreach (string s in names)
        {
            Remove(s);
        }
    }

    public void Remove(string name)
    {
        if (!boardItems.ContainsKey(name)) return;

        boardItems[name].Remove();
        boardItems.Remove(name);
    }

    public void RemoveAll()
    {
        foreach (KeyValuePair<string, BoardComponent> kv in boardItems)
        {
            kv.Value.Remove();
        }

        boardItems.Clear();
    }

    public void ChangeAlpha(string name, float alpha)
    {
        if (!boardItems.ContainsKey(name)) return;
        boardItems[name].SetVisible(alpha);
    }

    public void ChangeAlphaAll(float alpha)
    {
        foreach (KeyValuePair<string, BoardComponent> kv in boardItems)
        {
            kv.Value.SetVisible(alpha);
        }
    }

    override public string ToString()
    {
        string nl = System.Environment.NewLine;
        string r = "[Quest]" + nl;

        r += "path=" + qd.questPath + nl;
        r += "round=" + round+ nl;
        r += "morale=" + morale + nl;
        r += "threat=" + threat + nl;
        r += "horror=" + horrorPhase + nl;
        r += "heroesSelected=" + heroesSelected + nl;
        r += "minorPeril=" + minorPeril + nl;
        r += "majorPeril=" + majorPeril + nl;
        r += "deadlyPeril=" + deadlyPeril + nl;
        r += "DelayedEvents=";

        foreach (QuestData.Event.DelayedEvent de in delayedEvents)
        {
            r += de.delay + ":" + de.eventName + " ";
        }
        r += nl;

        r += "[Packs]" + nl;
        foreach (string pack in game.cd.GetEnabledPackIDs())
        {
            r += pack + nl;
        }

        r += "[Board]" + nl;
        foreach (KeyValuePair<string, BoardComponent> kv in boardItems)
        {
            r += kv.Key + nl;
        }

        r += "[Flags]" + nl;
        foreach (string s in flags)
        {
            r += s + nl;
        }

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

        foreach (Hero h in heroes)
        {
            r += h.ToString();
        }

        foreach (Monster m in monsters)
        {
            r += m.ToString();
        }

        return r;
    }

// Class for Tile components (use TileSide content data)
public class Tile : BoardComponent
    {
        public QuestData.Tile qTile;
        public TileSideData cTile;

        public Tile(QuestData.Tile questTile, Game gameObject) : base(gameObject)
        {
            qTile = questTile;

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
                Debug.Log("Error: Failed to located TileSide: " + qTile.tileSideName + " in quest component: " + qTile.name);
                Application.Quit();
            }

            // Attempt to load image
            Texture2D newTex = ContentData.FileToTexture(game.cd.tileSides[qTile.tileSideName].image);
            if (newTex == null)
            {
                // Fatal if missing
                Debug.Log("Error: cannot open image file for TileSide: " + game.cd.tileSides[qTile.tileSideName].image);
                Application.Quit();
            }

            unityObject = new GameObject("Object" + qTile.name);
            unityObject.tag = "board";
            unityObject.transform.parent = game.boardCanvas.transform;

            // Add image to object
            image = unityObject.AddComponent<UnityEngine.UI.Image>();
            // Create sprite from texture
            Sprite tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            // Set image sprite
            image.sprite = tileSprite;
            // Move to get the top left square corner at 0,0
            float vPPS = game.cd.tileSides[qTile.tileSideName].pxPerSquare;
            float hPPS = vPPS;
            // manual aspect control
            if (game.cd.tileSides[qTile.tileSideName].aspect != 0)
            {
                hPPS = (vPPS * newTex.width / newTex.height) / game.cd.tileSides[qTile.tileSideName].aspect;
            }

            unityObject.transform.Translate(Vector3.right * ((newTex.width / 2) - cTile.left) / hPPS, Space.World);
            unityObject.transform.Translate(Vector3.down * ((newTex.height / 2) - cTile.top) / vPPS, Space.World);
            // Move to get the middle of the top left square at 0,0 (squares are 105 units)
            // We don't do this for MoM because it spaces differently
            if (game.gameType.TileOnGrid())
            {
                unityObject.transform.Translate(new Vector3(-(float)0.5, (float)0.5, 0), Space.World);
            }
            // Set the size to the image size
            image.rectTransform.sizeDelta = new Vector2((float)newTex.width / hPPS, (float)newTex.height / vPPS);

            // Rotate around 0,0 rotation amount
            unityObject.transform.RotateAround(Vector3.zero, Vector3.forward, qTile.rotation);
            // Move tile into target location (spaces are 105 units, Space.World is needed because tile has been rotated)
            unityObject.transform.Translate(new Vector3(qTile.location.x, qTile.location.y, 0), Space.World);
        }

        public override void Remove()
        {
            Object.Destroy(unityObject);
        }

        public override QuestData.Event GetEvent()
        {
            return null;
        }
    }

    // Tokens are events that are tied to a token placed on the board
    public class Token : BoardComponent
    {

        public QuestData.Token qToken;

        public Token(QuestData.Token questToken, Game gameObject) : base(gameObject)
        {
            qToken = questToken;

            string tokenName = qToken.tokenName;
            if (!game.cd.tokens.ContainsKey(tokenName))
            {
                Debug.Log("Warning: Quest component " + qToken.name + " is using missing token type: " + tokenName);
                // Catch for older quests with different types (0.4.0 or older)
                if (game.cd.tokens.ContainsKey("TokenSearch"))
                {
                    tokenName = "TokenSearch";
                }
            }
            Vector2 texPos = new Vector2(game.cd.tokens[tokenName].x, game.cd.tokens[tokenName].y);
            Vector2 texSize = new Vector2(game.cd.tokens[tokenName].width, game.cd.tokens[tokenName].height);
            Texture2D newTex = ContentData.FileToTexture(game.cd.tokens[tokenName].image, texPos, texSize);

            // Create object
            unityObject = new GameObject("Object" + qToken.name);
            unityObject.tag = "board";

            unityObject.transform.parent = game.tokenCanvas.transform;

            // Create the image
            image = unityObject.AddComponent<UnityEngine.UI.Image>();
            Sprite tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            image.color = Color.white;
            image.sprite = tileSprite;
            image.rectTransform.sizeDelta = new Vector2(1f, 1f);
            // Move to square (105 units per square)
            unityObject.transform.Translate(new Vector3(qToken.location.x, qToken.location.y, 0), Space.World);

            game.tokenBoard.Add(this);
        }

        public override QuestData.Event GetEvent()
        {
            return qToken;
        }

        public override void Remove()
        {
            Object.Destroy(unityObject);
        }
    }

    // Doors are like tokens but placed differently and have different defaults
    public class Door : BoardComponent
    {
        public QuestData.Door qDoor;

        public Door(QuestData.Door questDoor, Game gameObject) : base(gameObject)
        {
            qDoor = questDoor;
            Texture2D newTex = Resources.Load("sprites/door") as Texture2D;
            // Check load worked
            if (newTex == null)
            {
                Debug.Log("Error: Cannot load door image");
                Application.Quit();
            }

            // Create object
            unityObject = new GameObject("Object" + qDoor.name);
            unityObject.tag = "board";

            unityObject.transform.parent = game.tokenCanvas.transform;

            // Create the image
            image = unityObject.AddComponent<UnityEngine.UI.Image>();
            Sprite tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            // Set door colour
            image.sprite = tileSprite;
            image.rectTransform.sizeDelta = new Vector2(0.4f, 1.6f);
            // Rotate as required
            unityObject.transform.RotateAround(Vector3.zero, Vector3.forward, qDoor.rotation);
            // Move to square (105 units per square)
            unityObject.transform.Translate(new Vector3(-(float)0.5, (float)0.5, 0), Space.World);
            unityObject.transform.Translate(new Vector3(qDoor.location.x, qDoor.location.y, 0), Space.World);

            SetColor(qDoor.colourName);

            game.tokenBoard.Add(this);
        }

        public void SetColor(string colorName)
        {
            string colorRGB = ColorUtil.FromName(colorName);
            if ((colorRGB.Length != 7) || (colorRGB[0] != '#'))
            {
                Debug.Log("Warning: Door color must be in #RRGGBB format or a known name in: " + qDoor.name);
            }

            Color colour = Color.white;
            colour[0] = (float)System.Convert.ToInt32(colorRGB.Substring(1, 2), 16) / 255f;
            colour[1] = (float)System.Convert.ToInt32(colorRGB.Substring(3, 2), 16) / 255f;
            colour[2] = (float)System.Convert.ToInt32(colorRGB.Substring(5, 2), 16) / 255f;
            image.color = colour;
        }


        public override void Remove()
        {
            Object.Destroy(unityObject);
        }

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

        public BoardComponent(Game gameObject)
        {
            game = gameObject;
        }

        abstract public void Remove();

        abstract public QuestData.Event GetEvent();

        virtual public void SetVisible(float alpha)
        {
            if (image == null)
                return;
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
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

        public Hero(HeroData h, int i)
        {
            heroData = h;
            id = i;
        }

        public Hero(Dictionary<string, string> data)
        {
            bool.TryParse(data["activated"], out activated);
            bool.TryParse(data["defeated"], out defeated);
            int.TryParse(data["id"], out id);

            Game game = Game.Get();
            if (data.ContainsKey("type"))
            {
                foreach (KeyValuePair<string, HeroData> hd in game.cd.heros)
                {
                    if (hd.Value.sectionName.Equals(data["type"]))
                    {
                        heroData = hd.Value;
                    }
                }
            }
        }

        override public string ToString()
        {
            string nl = System.Environment.NewLine;

            string r = "[Hero" + id + "]" + nl;
            r += "id=" + id + nl;
            r += "activated=" + activated + nl;
            r += "defeated=" + defeated + nl;
            if (heroData != null)
            {
                r += "type=" + heroData.sectionName + nl;
            }

            return r;
        }
    }

    // Class for holding current monster status
    public class Monster
    {
        public MonsterData monsterData;
        public bool activated = false;
        public bool minionStarted = false;
        public bool masterStarted = false;
        public bool unique = false;
        public string uniqueText = "";
        public string uniqueTitle = "";
        // Activation is reset each round so that master/minion are the same and forcing doesn't re roll
        public ActivationInstance currentActivation;

        // Initialise from monster event
        public Monster(EventManager.MonsterEvent monsterEvent)
        {
            monsterData = monsterEvent.cMonster;
            unique = monsterEvent.qMonster.unique;
            uniqueTitle = monsterEvent.GetUniqueTitle();
            uniqueText = monsterEvent.qMonster.uniqueText;
        }

        public Monster(Dictionary<string, string> data)
        {
            bool.TryParse(data["activated"], out activated);
            bool.TryParse(data["minionStarted"], out minionStarted);
            bool.TryParse(data["masterStarted"], out masterStarted);
            bool.TryParse(data["unique"], out unique);
            uniqueText = data["uniqueText"];
            uniqueTitle = data["uniqueTitle"];

            Game game = Game.Get();
            if (game.cd.monsters.ContainsKey(data["type"]))
            {
                monsterData = game.cd.monsters[data["type"]];
            }
            if (game.quest.qd.components.ContainsKey(data["type"]) && game.quest.qd.components[data["type"]] is QuestData.UniqueMonster)
            {
                monsterData = new QuestMonster(game.quest.qd.components[data["type"]] as QuestData.UniqueMonster);
            }

            if (data.ContainsKey("activation"))
            {
                ActivationData saveActivation = null;
                if (game.cd.activations.ContainsKey(data["activation"]))
                {
                    saveActivation = game.cd.activations[data["activation"]];
                }
                if (game.quest.qd.components.ContainsKey(data["activation"]))
                {
                    saveActivation = new QuestActivation(game.quest.qd.components[data["activation"]] as QuestData.Activation);
                }
                currentActivation = new ActivationInstance(saveActivation, monsterData.name);
            }
        }

        public void NewActivation(ActivationData contentActivation)
        {
            currentActivation = new ActivationInstance(contentActivation, monsterData.name);
        }

        public class ActivationInstance
        {
            public ActivationData ad;
            public string effect;

            public ActivationInstance(ActivationData contentActivation, string monsterName)
            {
                ad = contentActivation;
                effect = ad.ability.Replace("{0}", Game.Get().quest.GetRandomHero().heroData.name);
                effect = effect.Replace("{1}", monsterName);
                effect.Replace("\\n", "\n");
            }
        }

        override public string ToString()
        {
            string nl = System.Environment.NewLine;

            string r = "[Monster" + monsterData.sectionName + "]" + nl;
            r += "activated=" + activated + nl;
            r += "type=" + monsterData.sectionName + nl;
            r += "minionStarted=" + minionStarted + nl;
            r += "masterStarted=" + masterStarted + nl;
            r += "unique=" + unique + nl;
            r += "uniqueText=" + uniqueText + nl;
            r += "uniqueTitle=" + uniqueTitle + nl;
            if (currentActivation != null)
            {
                r += "activation=" + currentActivation.ad.sectionName + nl;
            }
            return r;
        }
    }
}

