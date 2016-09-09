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

    public List<Hero> heroes;
    public List<Monster> monsters;

    public int round = 1;
    public int morale = 0;

    public bool heroesSelected = false;

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

        // Populate null hero list, these can then be selected as hero types
        heroes = new List<Hero>();
        for (int i = 1; i < 5; i++)
        {
            heroes.Add(new Hero(null, i));
        }
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
            unityObject.transform.Translate(Vector3.right * ((newTex.width / 2) - cTile.left), Space.World);
            unityObject.transform.Translate(Vector3.down * ((newTex.height / 2) - cTile.top), Space.World);
            // Move to get the middle of the top left square at 0,0 (squares are 105 units)
            unityObject.transform.Translate(new Vector3(-(float)0.5, (float)0.5, 0) * 105, Space.World);
            // Set the size to the image size (images are assumed to be 105px per square)
            image.rectTransform.sizeDelta = new Vector2(newTex.width, newTex.height);

            // Rotate around 0,0 rotation amount
            unityObject.transform.RotateAround(Vector3.zero, Vector3.forward, qTile.rotation);
            // Move tile into target location (spaces are 105 units, Space.World is needed because tile has been rotated)
            unityObject.transform.Translate(new Vector3(qTile.location.x, qTile.location.y, 0) * 105, Space.World);
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

            Texture2D newTex = Resources.Load("sprites/tokens/" + qToken.spriteName) as Texture2D;
            // Check if we can find the token image
            if (newTex == null)
            {
                Debug.Log("Warning: Quest component " + qToken.name + " is using missing token type: " + qToken.spriteName);
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
            unityObject = new GameObject("Object" + qToken.name);
            unityObject.tag = "board";

            unityObject.transform.parent = game.tokenCanvas.transform;

            // Create the image
            image = unityObject.AddComponent<UnityEngine.UI.Image>();
            Sprite tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            image.color = Color.white;
            image.sprite = tileSprite;
            image.rectTransform.sizeDelta = new Vector2((int)((float)newTex.width * (float)0.8), (int)((float)newTex.height * (float)0.8));
            // Move to square (105 units per square)
            unityObject.transform.Translate(new Vector3(qToken.location.x, qToken.location.y, 0) * 105, Space.World);

            game.tokenBoard.add(this);
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
            image.rectTransform.sizeDelta = new Vector2(newTex.width, newTex.height);
            // Rotate as required
            unityObject.transform.RotateAround(Vector3.zero, Vector3.forward, qDoor.rotation);
            // Move to square (105 units per square)
            unityObject.transform.Translate(new Vector3(-(float)0.5, (float)0.5, 0) * 105, Space.World);
            unityObject.transform.Translate(new Vector3(qDoor.location.x, qDoor.location.y, 0) * 105, Space.World);

            SetColor(qDoor.colourName);

            game.tokenBoard.add(this);
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
        public ActivationData currentActivation;

        // Initialise from monster event
        public Monster(EventManager.MonsterEvent monsterEvent)
        {
            monsterData = monsterEvent.cMonster;
            unique = monsterEvent.qMonster.unique;
            uniqueTitle = monsterEvent.GetUniqueTitle();
            uniqueText = monsterEvent.qMonster.uniqueText;
        }
    }
}

