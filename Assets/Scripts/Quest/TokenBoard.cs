using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Class for managing token and door operation
// One object is created and attached to the token canvas
public class TokenBoard : MonoBehaviour {

    public List<TokenControl> tc;
    // Use this for initialization
    void Awake() {
        tc = new List<TokenControl>();
    }

    // Add a door
    public void add(Quest.Door d)
    {
        tc.Add(new TokenControl(d));
    }

    // Add a token
    public void add(Quest.Token t)
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
            // If a dialog is open ignore
            if (GameObject.FindGameObjectWithTag("dialog") != null)
                return;
            // Spawn a window with the door/token info
            new DialogWindow(c.GetEvent());
        }

    }

    public void AddMonster(QuestData.Monster m)
    {
        Game game = Game.Get();
        int count = 0;
        foreach (Round.Hero h in game.round.heroes)
        {
            if (h.heroData != null) count++;
        }

        if (m.placement[count].Length == 0)
        {
            AddAreaMonster(m);
        }
        else
        {
            AddPlacedMonsters(m, count);
        }
    }

    public void AddPlacedMonsters(QuestData.Monster m, int count)
    {
        Texture2D newTex = ContentData.FileToTexture(m.mData.imagePlace);

        // Check load worked
        if (newTex == null)
        {
            Debug.Log("Error: Cannot load monster image");
            Application.Quit();
        }

        int x = 1;
        int y = 1;

        if (m.mData.ContainsTrait("medium") || m.mData.ContainsTrait("huge"))
        {
            x = 2;
        }
        if (m.mData.ContainsTrait("huge") || m.mData.ContainsTrait("massive"))
        {
            y = 2;
        }
        if (m.mData.ContainsTrait("massive"))
        {
            x = 3;
        }

        foreach (string s in m.placement[count])
        {
            AddPlacedMonsterImg(s, newTex, x, y);
        }
    }

    public void AddPlacedMonsterImg(string place, Texture2D newTex, int x, int y)
    {
        Game game = Game.Get();
        Sprite tileSprite;

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
        image.rectTransform.sizeDelta = new Vector2((105f * x), (105f * y));
        // Move to get the top left square corner at 0,0
        gameObject.transform.Translate(Vector3.right * 105f * (float)(x - 1) / 2f, Space.World);
        gameObject.transform.Translate(Vector3.down * 105f * (float)(y - 1) / 2f, Space.World);

        if (mp.rotate)
        {
            gameObject.transform.RotateAround(Vector3.zero, Vector3.forward, -90);
        }
        // Move to square (105 units per square)
        gameObject.transform.Translate(new Vector3(mp.location.x, mp.location.y, 0) * 105, Space.World);
    }


    public void AddAreaMonster(QuestData.Monster m)
    {
        Game game = Game.Get();
        Sprite tileSprite;
        Texture2D newTex = Resources.Load("sprites/tokens/villager-token-man") as Texture2D;
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
        image.rectTransform.sizeDelta = new Vector2((int)((float)newTex.width * (float)0.8), (int)((float)newTex.height * (float)0.8));
        // Move to square (105 units per square)
        gameObject.transform.Translate(new Vector3(m.location.x, m.location.y, 0) * 105, Space.World);
    }

    public void AddHighlight(QuestData.Event e)
    {
        AddHighlight(e.location);
    }

    public void AddHighlight(Vector2 location, string id="", string tag="dialog")
    {
        Sprite tileSprite;
        Texture2D newTex = Resources.Load("sprites/tokens/search-token-special") as Texture2D;
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
        image.color = Color.cyan;
        image.sprite = tileSprite;
        image.rectTransform.sizeDelta = new Vector2((int)((float)newTex.width * (float)0.8), (int)((float)newTex.height * (float)0.8));
        // Move to square (105 units per square)
        gameObject.transform.Translate(new Vector3(location.x, location.y, 0) * 105, Space.World);
    }
}

