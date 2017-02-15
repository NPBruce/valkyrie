using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
            if (game.quest.horrorPhase) return;
            // If a dialog is open ignore
            if (GameObject.FindGameObjectWithTag("dialog") != null)
                return;
            // Spawn a window with the door/token info
            game.quest.eManager.QueueEvent(c.GetEvent().name);
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

        // Check for a placement list at this hero count
        if (me.qMonster.placement[count].Length == 0)
        {
            // group placement
            AddAreaMonster(me.qMonster);
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
            Debug.Log("Error: Cannot load monster image");
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
    public void AddPlacedMonsterImg(string place, Texture2D newTex, int x, int y)
    {
        Game game = Game.Get();
        Sprite tileSprite;

        // Check that placement name exists
        if (!game.quest.qd.components.ContainsKey(place))
        {
            Debug.Log("Error: Invalid moster place: " + place);
            Application.Quit();
        }

        QuestData.MPlace mp = game.quest.qd.components[place] as QuestData.MPlace;

        // Create object
        GameObject gameObject = new GameObject("MonsterSpawn" + place);
        gameObject.tag = "dialog";

        gameObject.transform.parent = game.tokenCanvas.transform;

        // Create the image
        UnityEngine.UI.Image image = gameObject.AddComponent<UnityEngine.UI.Image>();
        tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
        if (mp.master)
        {
            image.color = Color.red;
        }
        image.sprite = tileSprite;
        image.rectTransform.sizeDelta = new Vector2(x, y);
        // Move to get the top left square corner at 0,0
        gameObject.transform.Translate(Vector3.right * (float)(x - 1) / 2f, Space.World);
        gameObject.transform.Translate(Vector3.down * (float)(y - 1) / 2f, Space.World);

        if (mp.rotate)
        {
            gameObject.transform.RotateAround(Vector3.zero, Vector3.forward, -90);
        }
        // Move to square
        gameObject.transform.Translate(new Vector3(mp.location.x, mp.location.y, 0), Space.World);
    }


    // Add a signal to place a monster group
    public void AddAreaMonster(QuestData.Monster m)
    {
        Game game = Game.Get();
        Sprite tileSprite;
        Texture2D newTex = Resources.Load("sprites/target") as Texture2D;
        // Check load worked
        if (newTex == null)
        {
            Debug.Log("Error: Cannot load monster image");
            Application.Quit();
        }

        // Create object
        GameObject gameObject = new GameObject("MonsterSpawn");
        gameObject.tag = "dialog";

        gameObject.transform.parent = game.tokenCanvas.transform;

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
        AddHighlight(e.location);
    }

    // Draw a highlight at location
    public void AddHighlight(Vector2 location, string id="", string tag="dialog")
    {
        Sprite tileSprite;
        Texture2D newTex = Resources.Load("sprites/target") as Texture2D;
        // Check load worked
        if (newTex == null)
        {
            Debug.Log("Error: Cannot load monster image");
            Application.Quit();
        }

        // Create object
        GameObject gameObject = new GameObject("Highlight" + id);
        gameObject.tag = tag;

        Game game = Game.Get();
        gameObject.transform.parent = game.tokenCanvas.transform;

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

