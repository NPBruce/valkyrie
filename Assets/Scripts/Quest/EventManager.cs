/*using System.Collections.Generic;

class EventManager
{
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
*/