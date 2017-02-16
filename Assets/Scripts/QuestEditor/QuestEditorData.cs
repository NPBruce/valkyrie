using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Pretty much the entire quest editor is in this class
// It is HUGE and terrible
// And scary
public class QuestEditorData {
    // When a selection list is raised it is stored here
    // This allows the return value to be fetched later
    public EditorSelectionList esl;
    // This is the currently selected component
    public EditorComponent selection;
    // The selection stack is used for the 'back' button
    public Stack<EditorComponent> selectionStack;

    public QuestEditorData()
    {
        selectionStack = new Stack<EditorComponent>();
        // Start at the quest component
        SelectQuest();
    }

    // Update component selection
    public void NewSelection(EditorComponent c)
    {
        selectionStack.Push(selection);
        selection = c;
    }

    // Go back in the selection stack
    public static void Back()
    {
        Game game = Game.Get();
        // Check if there is anything to go back to
        if (game.qed.selectionStack.Count == 0)
        {
            return;
        }
        game.qed.selection = game.qed.selectionStack.Pop();
        game.qed.selection.Update();
    }

    // Open component selection top level
    public static void TypeSelect()
    {
        if (GameObject.FindGameObjectWithTag("dialog") != null)
        {
            return;
        }

        // Border
        DialogBox db = new DialogBox(new Vector2(21, 0), new Vector2(18, 18), "");
        db.AddBorder();

        // Heading
        db = new DialogBox(new Vector2(21, 0), new Vector2(17, 1), "Select Type");

        // Buttons for each component type (and delete buttons)
        TextButton tb = new TextButton(new Vector2(22, 2), new Vector2(9, 1), "Quest", delegate { SelectQuest(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(22, 4), new Vector2(9, 1), "Tile", delegate { ListTile(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(32, 4), new Vector2(6, 1), "Delete", delegate { DeleteComponent("Tile"); }, Color.red);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(22, 6), new Vector2(9, 1), "Door", delegate { ListDoor(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(32, 6), new Vector2(6, 1), "Delete", delegate { DeleteComponent("Door"); }, Color.red);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(22, 8), new Vector2(9, 1), "Token", delegate { ListToken(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(32, 8), new Vector2(6, 1), "Delete", delegate { DeleteComponent("Token"); }, Color.red);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(22, 10), new Vector2(9, 1), "Monster", delegate { ListMonster(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(32, 10), new Vector2(6, 1), "Delete", delegate { DeleteComponent("Monster"); }, Color.red);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(22, 12), new Vector2(9, 1), "MPlace", delegate { ListMPlace(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(32, 12), new Vector2(6, 1), "Delete", delegate { DeleteComponent("MPlace"); }, Color.red);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(22, 14), new Vector2(9, 1), "Event", delegate { ListEvent(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(32, 14), new Vector2(6, 1), "Delete", delegate { DeleteComponent("Event"); }, Color.red);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(25.5f, 16), new Vector2(9, 1), "Cancel", delegate { Cancel(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
    }

    public static void ListTile()
    {
        Game game = Game.Get();

        List<string> tiles = new List<string>();
        tiles.Add("{NEW:Tile}");
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Tile)
            {
                tiles.Add(kv.Key);
            }
        }

        if (tiles.Count == 0)
        {
            return;
        }
        game.qed.esl = new EditorSelectionList("Select Item", tiles, delegate { SelectComponent(); });
        game.qed.esl.SelectItem();
    }

    public static void ListDoor()
    {
        Game game = Game.Get();

        List<string> doors = new List<string>();
        doors.Add("{NEW:Door}");
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Door)
            {
                doors.Add(kv.Key);
            }
        }

        if (doors.Count == 0)
        {
            return;
        }
        game.qed.esl = new EditorSelectionList("Select Item", doors, delegate { SelectComponent(); });
        game.qed.esl.SelectItem();
    }

    public static void ListToken()
    {
        Game game = Game.Get();

        List<string> tokens = new List<string>();
        tokens.Add("{NEW:Token}");
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Token)
            {
                tokens.Add(kv.Key);
            }
        }

        if (tokens.Count == 0)
        {
            return;
        }
        game.qed.esl = new EditorSelectionList("Select Item", tokens, delegate { SelectComponent(); });
        game.qed.esl.SelectItem();
    }

    public static void ListMonster()
    {
        Game game = Game.Get();

        List<string> monsters = new List<string>();
        monsters.Add("{NEW:Monster}");
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Monster)
            {
                monsters.Add(kv.Key);
            }
        }

        if (monsters.Count == 0)
        {
            return;
        }
        game.qed.esl = new EditorSelectionList("Select Item", monsters, delegate { SelectComponent(); });
        game.qed.esl.SelectItem();
    }

    public static void ListMPlace()
    {
        Game game = Game.Get();

        List<string> mplaces = new List<string>();
        mplaces.Add("{NEW:MPlace}");
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.MPlace)
            {
                mplaces.Add(kv.Key);
            }
        }

        if (mplaces.Count == 0)
        {
            return;
        }
        game.qed.esl = new EditorSelectionList("Select Item", mplaces, delegate { SelectComponent(); });
        game.qed.esl.SelectItem();
    }

    public static void ListEvent()
    {
        Game game = Game.Get();

        List<string> events = new List<string>();
        events.Add("{NEW:Event}");
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                if (!kv.Value.GetType().IsSubclassOf(typeof(QuestData.Event)))
                {
                    events.Add(kv.Key);
                }
            }
        }

        if (events.Count == 0)
        {
            return;
        }
        game.qed.esl = new EditorSelectionList("Select Item", events, delegate { SelectComponent(); });
        game.qed.esl.SelectItem();
    }

    public void SelectComponent()
    {
        SelectComponent(esl.selection);
    }

    public static void SelectComponent(string name)
    {
        Game game = Game.Get();
        QuestEditorData qed = game.qed;

        if (name.Equals("Quest"))
        {
            qed.SelectQuest();
            return;
        }
        if (name.Equals("{NEW:Tile}"))
        {
            qed.NewTile();
            return;
        }
        if (name.Equals("{NEW:Door}"))
        {
            qed.NewDoor();
            return;
        }
        if (name.Equals("{NEW:Token}"))
        {
            qed.NewToken();
            return;
        }
        if (name.Equals("{NEW:Monster}"))
        {
            qed.NewMonster();
            return;
        }
        if (name.Equals("{NEW:MPlace}"))
        {
            qed.NewMPlace();
            return;
        }
        if (name.Equals("{NEW:Event}"))
        {
            qed.NewEvent();
            return;
        }

        // This can happen to due rename/delete
        if (!game.quest.qd.components.ContainsKey(name))
        {
            qed.SelectQuest();
        }

        if (game.quest.qd.components[name] is QuestData.Tile)
        {
            qed.SelectAsTile(name);
            return;
        }

        if (game.quest.qd.components[name] is QuestData.Door)
        {
            qed.SelectAsDoor(name);
            return;
        }
        if (game.quest.qd.components[name] is QuestData.Token)
        {
            qed.SelectAsToken(name);
            return;
        }
        if (game.quest.qd.components[name] is QuestData.Monster)
        {
            qed.SelectAsMonster(name);
            return;
        }
        if (game.quest.qd.components[name] is QuestData.MPlace)
        {
            qed.SelectAsMPlace(name);
            return;
        }
        if (game.quest.qd.components[name] is QuestData.Event)
        {
            qed.SelectAsEvent(name);
            return;
        }
    }

    public static void SelectQuest()
    {
        Game game = Game.Get();
        NewSelection(new EditorComponentQuest());
    }

    public static void SelectAsTile(string name)
    {
        Game game = Game.Get();
        NewSelection(new EditorComponentTile(name));
    }

    public static void SelectAsDoor(string name)
    {
        Game game = Game.Get();
        NewSelection(new EditorComponentDoor(name));
    }

    public static void SelectAsToken(string name)
    {
        Game game = Game.Get();
        NewSelection(new EditorComponentToken(name));
    }

    public static void SelectAsEvent(string name)
    {
        Game game = Game.Get();
        NewSelection(new EditorComponentEvent(name));
    }

    public static void SelectAsEventPageTwo(string name)
    {
        Game game = Game.Get();
        NewSelection(new EditorComponentEventPageTwo(name));
    }

    public static void SelectAsMonster(string name)
    {
        Game game = Game.Get();
        NewSelection(new EditorComponentMonster(name));
    }

    public static void SelectAsMonsterPlacement(string name)
    {
        Game game = Game.Get();
        NewSelection(new EditorComponentMonsterPlacement(name));
    }

    public static void SelectAsMPlace(string name)
    {
        Game game = Game.Get();
        NewSelection(new EditorComponentMPlace(name));
    }

    public void NewTile()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("Tile" + index))
        {
            index++;
        }
        game.quest.qd.components.Add("Tile" + index, new QuestData.Tile("Tile" + index));
        game.quest.Add("Tile" + index);
        SelectComponent("Tile" + index);
    }

    public void NewDoor()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("Door" + index))
        {
            index++;
        }
        game.quest.qd.components.Add("Door" + index, new QuestData.Door("Door" + index));
        game.quest.Add("Door" + index);
        SelectComponent("Door" + index);
    }

    public void NewToken()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("Token" + index))
        {
            index++;
        }
        game.quest.qd.components.Add("Token" + index, new QuestData.Token("Token" + index));
        game.quest.Add("Token" + index);
        SelectComponent("Token" + index);
    }

    public void NewMonster()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("Monster" + index))
        {
            index++;
        }
        game.quest.qd.components.Add("Monster" + index, new QuestData.Monster("Monster" + index));
        SelectComponent("Monster" + index);
    }

    public void NewMPlace()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("MPlace" + index))
        {
            index++;
        }
        game.quest.qd.components.Add("MPlace" + index, new QuestData.MPlace("MPlace" + index));
        SelectComponent("MPlace" + index);
    }

    public void NewEvent()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("Event" + index))
        {
            index++;
        }
        game.quest.qd.components.Add("Event" + index, new QuestData.Event("Event" + index));
        SelectComponent("Event" + index);
    }

    public void DeleteComponent(string type)
    {
        Game game = Game.Get();

        List<string> toDelete = new List<string>();

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Key.IndexOf(type) == 0)
            {
                toDelete.Add(kv.Key);
                toDelete.Add("");
            }
        }
        esl = new EditorSelectionList("Component to Delete:", toDelete, delegate { SelectToDelete(); });
        esl.SelectItem();
    }

    public void DeleteComponent()
    {
        Game game = Game.Get();

        List<string> toDelete = new List<string>();

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            toDelete.Add(kv.Key);
            toDelete.Add("");
        }
        esl = new EditorSelectionList("Component to Delete:", toDelete, delegate { SelectToDelete(); });
        esl.SelectItem();
    }

    public void SelectToDelete()
    {
        Game game = Game.Get();

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            kv.Value.RemoveReference(esl.selection);
        }

        if (game.quest.qd.components.ContainsKey(esl.selection))
        {
            game.quest.qd.components.Remove(esl.selection);
        }

        game.quest.Remove(esl.selection);

        if (selection.name.Equals(esl.selection))
        {
            Back();
        }
    }

    public static void Cancel()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
    }
}
