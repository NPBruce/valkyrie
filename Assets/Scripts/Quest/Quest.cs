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
    public Dictionary<string, QuestComponent> boardItems;

    // Current events (monsters, tokens, events, peril
    public Dictionary<string, Event> events;

    // A list of flags that have been set during the quest
    public HashSet<string> flags;

    // A dictionary of heros that have been selected in events
    public Dictionary<string, List<Round.Hero>> heroSelection;

    public Game game;

    public Quest(QuestLoader.Quest q, Game gameObject)
    {
        game = gameObject;
        qd = new QuestData(q);
        boardItems = new Dictionary<string, QuestComponent>();
        events = new Dictionary<string, Event>();
        flags = new HashSet<string>();
        heroSelection = new Dictionary<string, List<Round.Hero>>();
    }

    public void Create(string name)
    {
        if (!game.qd.components.ContainsKey(name))
        {
            Debug.Log("Error: Unable to create missing quest component: " + name);
            Application.Quit();
        }
        QuestData.QuestComponent qc = game.qd.components[name];
    }


    // Class for Tile components (use TileSide content data)
    public class Tile : QuestComponent
    {
        public QuestData.Tile qTile;
        public TileSideData cTile;
        public GameObject tileObject;

        public Tile(QuestData.Tile questTile, Game gameObject) : base(gameObject)
        {
            qTile = questTile;

            if (game.cd.tileSides.ContainsKey(qTile.tileName))
            {
                cTile = game.cd.tileSides[qTile.tileName];
            }
            else if (game.cd.tileSides.ContainsKey("TileSide" + qTile.tileName))
            {
                cTile = game.cd.tileSides["TileSide" + qTile.tileName];
            }
            else
            {
                // Fatal if not found
                Debug.Log("Error: Failed to located TileSide: " + qTile.tileName + " in quest component: " + qTile.name);
                Application.Quit();
            }

            // Attempt to load image
            Texture2D newTex = ContentData.FileToTexture(qTile.tileType.image);
            if (newTex == null)
            {
                // Fatal if missing
                Debug.Log("Error: cannot open image file for TileSide: " + qTile.tileType.image);
                Application.Quit();
            }

            tileObject = new GameObject("Object" + qTile.name);
            tileObject.tag = "board";
            tileObject.transform.parent = game.boardCanvas.transform;

            // Add image to object
            image = tileObject.AddComponent<UnityEngine.UI.Image>();
            // Create sprite from texture
            Sprite tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            // Set to transparent initially
            image.color = new Color(1, 1, 1, 0);
            // Set image sprite
            image.sprite = tileSprite;
            // Move to get the top left square corner at 0,0
            tileObject.transform.Translate(Vector3.right * ((newTex.width / 2) - cTile.left), Space.World);
            tileObject.transform.Translate(Vector3.down * ((newTex.height / 2) - cTile.top), Space.World);
            // Move to get the middle of the top left square at 0,0 (squares are 105 units)
            tileObject.transform.Translate(new Vector3(-(float)0.5, (float)0.5, 0) * 105, Space.World);
            // Set the size to the image size (images are assumed to be 105px per square)
            image.rectTransform.sizeDelta = new Vector2(newTex.width, newTex.height);

            // Rotate around 0,0 rotation amount
            tileObject.transform.RotateAround(Vector3.zero, Vector3.forward, qTile.rotation);
            // Move tile into target location (spaces are 105 units, Space.World is needed because tile has been rotated)
            tileObject.transform.Translate(new Vector3(qTile.location.x, qTile.location.y, 0) * 105, Space.World);
        }

        ~Tile()
        {
            Object.Destroy(tileObject);
        }
    }

    // Doors are like tokens but placed differently and have different defaults
    public class Door : Event
    {
        public GameObject doorObject;
        public QuestData.Door qDoor;

        public Door(QuestData.Door questDoor, Game gameObject) : base(questDoor, gameObject)
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
            doorObject = new GameObject("Object" + qDoor.name);
            doorObject.tag = "board";

            doorObject.transform.parent = game.tokenCanvas.transform;

            // Create the image
            image = doorObject.AddComponent<UnityEngine.UI.Image>();
            Sprite tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            // Set door colour
            image.color = new Color(qDoor.colour[0], qDoor.colour[1], qDoor.colour[2], 1);
            image.sprite = tileSprite;
            image.rectTransform.sizeDelta = new Vector2(newTex.width, newTex.height);
            // Rotate as required
            doorObject.transform.RotateAround(Vector3.zero, Vector3.forward, qDoor.rotation);
            // Move to square (105 units per square)
            doorObject.transform.Translate(new Vector3(-(float)0.5, (float)0.5, 0) * 105, Space.World);
            doorObject.transform.Translate(new Vector3(qDoor.location.x, qDoor.location.y, 0) * 105, Space.World);

            game.tokenBoard.add(qDoor);
        }

        ~Door()
        {
            Object.Destroy(doorObject);
        }
    }

    // Tokens are events that are tied to a token placed on the board
    public class Token : Event
    {

        public GameObject tokenObject;
        public QuestData.Token qToken;

        public Token(QuestData.Token questToken, Game gameObject) : base(questToken, gameObject)
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
            tokenObject = new GameObject("Object" + qToken.name);
            tokenObject.tag = "board";

            tokenObject.transform.parent = game.tokenCanvas.transform;

            // Create the image
            image = tokenObject.AddComponent<UnityEngine.UI.Image>();
            Sprite tileSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            image.color = Color.white;
            image.sprite = tileSprite;
            image.rectTransform.sizeDelta = new Vector2((int)((float)newTex.width * (float)0.8), (int)((float)newTex.height * (float)0.8));
            // Move to square (105 units per square)
            tokenObject.transform.Translate(new Vector3(qToken.location.x, qToken.location.y, 0) * 105, Space.World);

            game.tokenBoard.add(qToken);
        }

        ~Token()
        {
            Object.Destroy(tokenObject);
        }
    }


    // Monster items are monster group placement events
    public class Monster : Event
    {
        public QuestData.Monster qMonster;
        public MonsterData cMonster;

        public Monster(QuestData.Monster monster, Game gameObject) : base(monster, gameObject)
        {
            qMonster = monster;
            // Next try to find a type that is valid
            foreach (string t in qMonster.mTypes)
            {
                // Monster type must exist in content packs, 'Monster' is optional
                if (game.cd.monsters.ContainsKey(t))
                {
                    cMonster = game.cd.monsters[t];
                }
                else if (game.cd.monsters.ContainsKey("Monster" + t))
                {
                    cMonster = game.cd.monsters["Monster" + t];
                }
            }

            // If we didn't find anything try by trait
            if (cMonster == null)
            {
                if (qMonster.mTraits.Length == 0)
                {
                    Debug.Log("Error: Cannot find monster and no traits provided in event: " + qMonster.name);
                    Application.Quit();
                }

                List<MonsterData> list = new List<MonsterData>();
                foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
                {
                    bool allFound = true;
                    foreach (string t in qMonster.mTraits)
                    {
                        if (!cMonster.ContainsTrait(t))
                        {
                            allFound = false;
                        }
                    }
                    if (allFound)
                    {
                        list.Add(kv.Value);
                    }
                }

                // Not found, throw error
                if (list.Count == 0)
                {
                    Debug.Log("Error: Unable to find monster of traits specified in event: " + qMonster.name);
                    Application.Quit();
                }

                cMonster = list[Random.Range(0, list.Count)];
            }
        }

        override public string GetText()
        {
            return base.GetText().Replace("{type}", cMonster.name);
        }

        public string GetUniqueTitle()
        {
            return qMonster.uniqueTitle.Replace("{type}", cMonster.name);
        }
    }

    // Events are used to create dialogs that control the quest
    public class Event : QuestComponent
    {
        public QuestData.Event qEvent;

        public Event(QuestData.Event questEvent, Game gameObject) : base(gameObject)
        {
            qEvent = questEvent;
        }

        virtual public string GetText()
        {
            return SymbolReplace(qEvent.text);
        }

        public static string SymbolReplace(string input)
        {
            string output = input;
            output = output.Replace("{heart}", "≥");
            output = output.Replace("{fatigue}", "∏");
            output = output.Replace("{might}", "∂");
            output = output.Replace("{will}", "π");
            output = output.Replace("{knowledge}", "∑");
            output = output.Replace("{awareness}", "μ");
            output = output.Replace("{action}", "∞");
            output = output.Replace("{shield}", "±");
            output = output.Replace("{surge}", "≥");
            return output;
        }

    }

    // Super class for all quest components
    public class QuestComponent
    {
        // image for display
        public UnityEngine.UI.Image image;

        // Game object
        public Game game;

        public QuestComponent(Game gameObject)
        {
            game = gameObject;
        }

        virtual public void SetVisible(float alpha)
        {
            if (image == null)
                return;
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
        }
    }
}

