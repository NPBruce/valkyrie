using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ValkyrieTools;

// Class for managing token and door operation
// One object is created and attached to the token canvas
public class TokenBoard : MonoBehaviour {

    public List<TokenControl> tc;
    // Use this for initialization
    void Awake() {
        Clear();
    }

    // Used when ending a quest
    public void Clear()
    {
        tc = new List<TokenControl>();
    }

    // Add a door
    public void Add(Quest.Door d)
    {
        tc.Add(new TokenControl(d));
    }

    // Add a token
    public void Add(Quest.Token t)
    {
        tc.Add(new TokenControl(t));
    }

    // Add a UI
    public void Add(Quest.UI ui)
    {
        tc.Add(new TokenControl(ui));
    }

    // Class for tokens and doors that will get the onClick event
    public class TokenControl
    {
        Quest.BoardComponent c;

        // Initialise from a door
        public TokenControl(Quest.BoardComponent component)
        {
            // If we are in the editor we don't add the buttons
            if (Game.Get().editMode) return;

            c = component;
            UnityEngine.UI.Button button = c.unityObject.AddComponent<UnityEngine.UI.Button>();
            button.interactable = true;
            button.onClick.AddListener(delegate { startEvent(); });
            c = component;
        }

        // On click the tokens start an event
        public void startEvent()
        {
            Game game = Game.Get();
            // If in horror phase ignore
            if (game.quest.phase != Quest.MoMPhase.investigator) return;
            // If a dialog is open ignore
            if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
                return;
            // Spawn a window with the door/token info
            game.quest.eManager.QueueEvent(c.GetEvent().sectionName);
        }

    }

    // Monsters are only on the board during an event
    public void AddMonster(EventManager.MonsterEvent me)
    {
        Game game = Game.Get();
        int count = 0;
        // Get number of heroes
        foreach (Quest.Hero h in game.quest.heroes)
        {
            if (h.heroData != null) count++;
        }

        if (game.gameType is MoMGameType)
        {
            Texture2D newTex = ContentData.FileToTexture(me.cMonster.image);
            AddPlacedMonsterImg("", newTex, 1, 1, me.qEvent.location.x, me.qEvent.location.y);
        }
        // Check for a placement list at this hero count
        else if (me.qMonster.placement[count].Length == 0)
        {
            if (me.cMonster.ContainsTrait("lieutenant"))
            {
                Texture2D newTex = ContentData.FileToTexture(me.cMonster.image);
                AddPlacedMonsterImg("", newTex, 1, 1, me.qEvent.location.x, me.qEvent.location.y);
            }
            else
            {
                // group placement
                AddAreaMonster(me.qMonster);
            }
        }
        else
        {
            // Individual monster placement
            AddPlacedMonsters(me, count);
        }
    }

    // Add individual monster placements for hero count
    public void AddPlacedMonsters(EventManager.MonsterEvent me, int count)
    {
        // Get monster placement image
        Texture2D newTex = ContentData.FileToTexture(me.cMonster.imagePlace);

        // Check load worked
        if (newTex == null)
        {
            ValkyrieDebug.Log("Error: Cannot load monster image");
            Application.Quit();
        }

        // Get placement dimensions
        int x = 1;
        int y = 1;

        if (me.cMonster.ContainsTrait("medium") || me.cMonster.ContainsTrait("huge"))
        {
            x = 2;
        }
        if (me.cMonster.ContainsTrait("huge") || me.cMonster.ContainsTrait("massive"))
        {
            y = 2;
        }
        if (me.cMonster.ContainsTrait("massive"))
        {
            x = 3;
        }

        // All all placements
        foreach (string s in me.qMonster.placement[count])
        {
            AddPlacedMonsterImg(s, newTex, x, y);
        }
    }

    // Add a placement image
    public void AddPlacedMonsterImg(string place, Texture2D newTex, int sizeX, int sizeY, float posX = 0, float posY = 0)
    {
        Game game = Game.Get();
        Sprite iconSprite;

        // Check that placement name exists
        if (!game.quest.qd.components.ContainsKey(place) && place.Length > 0)
        {
            ValkyrieDebug.Log("Error: Invalid moster place: " + place);
            Application.Quit();
        }

        // Create object
        GameObject circleObject = new GameObject("MonsterSpawnBorder" + place);
        GameObject gameObject = new GameObject("MonsterSpawn" + place);
        GameObject borderObject = new GameObject("MonsterSpawnBorder" + place);
        gameObject.tag = Game.DIALOG;
        borderObject.tag = Game.DIALOG;
        circleObject.tag = Game.DIALOG;
        circleObject.transform.SetParent(game.tokenCanvas.transform);
        gameObject.transform.SetParent(game.tokenCanvas.transform);
        borderObject.transform.SetParent(game.tokenCanvas.transform);

        // Create the image
        UnityEngine.UI.Image image = gameObject.AddComponent<UnityEngine.UI.Image>();
        iconSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
        image.sprite = iconSprite;
        image.rectTransform.sizeDelta = new Vector2(sizeX, sizeY);

        UnityEngine.UI.Image iconFrame = null;
        if (game.gameType is D2EGameType)
        {
            // Move to get the top left square corner at 0,0
            gameObject.transform.Translate(Vector3.right * (float)(sizeX - 1) / 2f, Space.World);
            gameObject.transform.Translate(Vector3.down * (float)(sizeY - 1) / 2f, Space.World);
            borderObject.transform.Translate(Vector3.right * (float)(sizeX - 1) / 2f, Space.World);
            borderObject.transform.Translate(Vector3.down * (float)(sizeY - 1) / 2f, Space.World);

            image.rectTransform.sizeDelta = new Vector2(sizeX * 0.83f, sizeY * 0.83f);

            borderObject.AddComponent<CanvasRenderer>();
            borderObject.AddComponent<RectTransform>();

            iconFrame = borderObject.AddComponent<UnityEngine.UI.Image>();
            Texture2D frameTex = Resources.Load("sprites/borders/Frame_Monster_1x1") as Texture2D;
            if (sizeX == 3)
            {
                frameTex = Resources.Load("sprites/borders/Frame_Monster_2x3") as Texture2D;
            }
            if (sizeX == 2 && sizeY == 1)
            {
                frameTex = Resources.Load("sprites/borders/Frame_Monster_1x2") as Texture2D;
            }
            iconFrame.sprite = Sprite.Create(frameTex, new Rect(0, 0, frameTex.width, frameTex.height), Vector2.zero, 1);
            iconFrame.rectTransform.sizeDelta = new Vector2(sizeX, sizeY);
        }

        if (place.Length > 0)
        {
            QuestData.MPlace mp = game.quest.qd.components[place] as QuestData.MPlace;
            posX = mp.location.x;
            posY = mp.location.y;
            
            circleObject.transform.Translate(Vector3.right * (float)(sizeX - 1) / 2f, Space.World);
            circleObject.transform.Translate(Vector3.down * (float)(sizeY - 1) / 2f, Space.World);

            circleObject.AddComponent<CanvasRenderer>();
            circleObject.AddComponent<RectTransform>();

            UnityEngine.UI.Image iconCicle = circleObject.AddComponent<UnityEngine.UI.Image>();
            Texture2D circleTex = Resources.Load("sprites/target") as Texture2D;
            if (sizeX == 2 && sizeY == 1)
            {
                circleTex = Resources.Load("sprites/borders/Empty_Monster_1x2") as Texture2D;
            }
            iconCicle.sprite = Sprite.Create(circleTex, new Rect(0, 0, circleTex.width, circleTex.height), Vector2.zero, 1);
            iconCicle.rectTransform.sizeDelta = new Vector2(sizeX * 1.08f, sizeY * 1.08f);

            if (mp.master)
            {
                iconCicle.color = Color.red;
            }

            if (mp.rotate)
            {
                gameObject.transform.RotateAround(Vector3.zero, Vector3.forward, -90);
                iconFrame.transform.RotateAround(Vector3.zero, Vector3.forward, -90);
                iconCicle.transform.RotateAround(Vector3.zero, Vector3.forward, -90);
            }
        }
        // Move to square
        gameObject.transform.Translate(new Vector3(posX, posY, 0), Space.World);
        borderObject.transform.Translate(new Vector3(posX, posY, 0), Space.World);
        circleObject.transform.Translate(new Vector3(posX, posY, 0), Space.World);
    }


    // Add a signal to place a monster group
    public void AddAreaMonster(QuestData.Spawn m)
    {
        Game game = Game.Get();
        Sprite tileSprite;
        Texture2D newTex = Resources.Load("sprites/target") as Texture2D;
        // Check load worked
        if (newTex == null)
        {
            ValkyrieDebug.Log("Error: Cannot load monster image");
            Application.Quit();
        }

        // Create object
        GameObject gameObject = new GameObject("MonsterSpawn");
        gameObject.tag = Game.DIALOG;

        gameObject.transform.SetParent(game.tokenCanvas.transform);

        // Create the image
        UnityEngine.UI.Image image = gameObject.AddComponent<UnityEngine.UI.Image>();
        tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
        image.color = Color.red;
        image.sprite = tileSprite;
        image.rectTransform.sizeDelta = new Vector2(1f, 1f);
        // Move to square (105 units per square)
        gameObject.transform.Translate(new Vector3(m.location.x, m.location.y, 0), Space.World);

        // Add pulser
        gameObject.AddComponent<SpritePulser>();
    }

    // Add highlight for event
    public void AddHighlight(QuestData.Event e)
    {
        string item = "";
        int items = 0;
        foreach (string s in e.addComponents)
        {
            if (s.IndexOf("QItem") == 0)
            {
                item = s;
                items++;
            }
        }
        if (items != 1 || !Game.Get().quest.itemSelect.ContainsKey(item))
        {
            AddHighlight(e.location);
        }
        else
        {
            AddItem(e.location, item);
        }
    }

    // Add an item
    public void AddItem(Vector2 location, string item)
    {
        Game game = Game.Get();
        // Create object
        GameObject itemObject = new GameObject("item" + item);
        itemObject.tag = Game.DIALOG;
        itemObject.transform.SetParent(game.tokenCanvas.transform);

        // Create the image
        Texture2D newTex = ContentData.FileToTexture(game.cd.items[game.quest.itemSelect[item]].image);
        UnityEngine.UI.Image image = itemObject.AddComponent<UnityEngine.UI.Image>();
        Sprite iconSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
        image.sprite = iconSprite;
        image.rectTransform.sizeDelta = new Vector2(1, 1);

        // Move to square
        itemObject.transform.Translate(new Vector3(location.x, location.y, 0), Space.World);
    }

    // Draw a highlight at location
    public void AddHighlight(Vector2 location, string id="", string tag="dialog")
    {
        Sprite tileSprite;
        Texture2D newTex = Resources.Load("sprites/target") as Texture2D;
        // Check load worked
        if (newTex == null)
        {
            ValkyrieDebug.Log("Error: Cannot load monster image");
            Application.Quit();
        }

        // Create object
        GameObject gameObject = new GameObject("Highlight" + id);
        gameObject.tag = tag;

        Game game = Game.Get();
        gameObject.transform.SetParent(game.tokenCanvas.transform);

        // Create the image
        UnityEngine.UI.Image image = gameObject.AddComponent<UnityEngine.UI.Image>();
        tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
        // Set door colour
        image.sprite = tileSprite;
        image.rectTransform.sizeDelta = new Vector2(1f, 1f);
        // Move to square (105 units per square)
        gameObject.transform.Translate(new Vector3(location.x, location.y, 0), Space.World);

        // Add pulser
        gameObject.AddComponent<SpritePulser>();
    }
}

