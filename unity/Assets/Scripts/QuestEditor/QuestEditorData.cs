using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

// This class manages the Quest editor Interface
// FIXME: Rename, not a good name any more
public class QuestEditorData {

    private readonly StringKey COMPONENT_TO_DELETE = new StringKey("val", "COMPONENT_TO_DELETE");
    // When a selection list is raised it is stored here
    // This allows the return value to be fetched later
    public EditorSelectionList esl;
    // This is the currently selected component
    public EditorComponent selection;
    // The selection stack is used for the 'back' button
    public Stack<EditorComponent> selectionStack;

    // Start the editor
    public QuestEditorData()
    {
        Game.Get().qed = this;       
        selectionStack = new Stack<EditorComponent>();
        // Start at the quest component
        SelectQuest();
    }

    // Update component selection
    // Used to save selection history
    public void NewSelection(EditorComponent c)
    {
        // selection starts at null
        if (selection != null)
        {
            selectionStack.Push(selection);
        }
        selection = c;
    }

    // Go back in the selection stack
    public static void Back()
    {
        Game game = Game.Get();
        // Check if there is anything to go back to
        if (game.qed.selectionStack.Count == 0)
        {
            // Reset on quest selection
            game.qed.selection = new EditorComponentQuest();
            return;
        }
        game.qed.selection = game.qed.selectionStack.Pop();
        // Quest is OK
        if (game.qed.selection is EditorComponentQuest)
        {
            game.qed.selection.Update();
        }
        else if (game.quest.qd.components.ContainsKey(game.qed.selection.name))
        {
            // Existing component
            game.qed.selection.Update();
        }
        else
        {
            // History item has been deleted/renamed, go back further
            Back();
        }
    }

    // Open component selection top level
    // Menu for selection of all component types, includes delete options
    public static void TypeSelect()
    {
        Game game = Game.Get();
        if (GameObject.FindGameObjectWithTag("dialog") != null)
        {
            return;
        }

        // Border
        DialogBox db = new DialogBox(new Vector2(21, 0), new Vector2(18, 26), StringKey.NULL);
        db.AddBorder();

        // Heading
        db = new DialogBox(new Vector2(21, 0), new Vector2(17, 1), 
            new StringKey("val","SELECT",CommonStringKeys.TYPE)
            );

        // Buttons for each component type (and delete buttons)
        float offset = 2;
        TextButton tb = new TextButton(new Vector2(22, offset), new Vector2(9, 1), CommonStringKeys.QUEST, delegate { SelectQuest(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        offset += 2;
        tb = new TextButton(new Vector2(22, offset), new Vector2(9, 1), CommonStringKeys.TILE, delegate { ListTile(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(32, offset), new Vector2(6, 1), CommonStringKeys.DELETE, delegate { game.qed.DeleteComponent("Tile"); }, Color.red);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        offset += 2;
        tb = new TextButton(new Vector2(22, offset), new Vector2(9, 1), CommonStringKeys.TOKEN, delegate { ListToken(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(32, offset), new Vector2(6, 1), CommonStringKeys.DELETE, delegate { game.qed.DeleteComponent("Token"); }, Color.red);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        offset += 2;
        tb = new TextButton(new Vector2(22, offset), new Vector2(9, 1), CommonStringKeys.SPAWN, delegate { ListSpawn(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(32, offset), new Vector2(6, 1), CommonStringKeys.DELETE, delegate { game.qed.DeleteComponent("Spawn"); }, Color.red);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        offset += 2;
        tb = new TextButton(new Vector2(22, offset), new Vector2(9, 1), CommonStringKeys.EVENT, delegate { ListEvent(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(32, offset), new Vector2(6, 1), CommonStringKeys.DELETE, delegate { game.qed.DeleteComponent("Event"); }, Color.red);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        offset += 2;
        tb = new TextButton(new Vector2(22, offset), new Vector2(9, 1), CommonStringKeys.CUSTOM_MONSTER, delegate { ListCustomMonster(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(32, offset), new Vector2(6, 1), CommonStringKeys.DELETE, delegate { game.qed.DeleteComponent("CustomMonster"); }, Color.red);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        offset += 2;
        tb = new TextButton(new Vector2(22, offset), new Vector2(9, 1), CommonStringKeys.ACTIVATION, delegate { ListActivation(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(32, offset), new Vector2(6, 1), CommonStringKeys.DELETE, delegate { game.qed.DeleteComponent("Activation"); }, Color.red);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        if (game.gameType is D2EGameType)
        {
            offset += 2;
            tb = new TextButton(new Vector2(22, offset), new Vector2(9, 1), CommonStringKeys.DOOR, delegate { ListDoor(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

            tb = new TextButton(new Vector2(32, offset), new Vector2(6, 1), CommonStringKeys.DELETE, delegate { game.qed.DeleteComponent("Door"); }, Color.red);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

            offset += 2;
            tb = new TextButton(new Vector2(22, offset), new Vector2(9, 1), CommonStringKeys.MPLACE, delegate { ListMPlace(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

            tb = new TextButton(new Vector2(32, offset), new Vector2(6, 1), CommonStringKeys.DELETE, delegate { game.qed.DeleteComponent("MPlace"); }, Color.red);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        }

        if (game.gameType is MoMGameType)
        {
            offset += 2;
            tb = new TextButton(new Vector2(22, offset), new Vector2(9, 1), CommonStringKeys.PUZZLE, delegate { ListPuzzle(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

            tb = new TextButton(new Vector2(32, offset), new Vector2(6, 1), CommonStringKeys.DELETE, delegate { game.qed.DeleteComponent("Puzzle"); }, Color.red);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        
            offset += 2;
            tb = new TextButton(new Vector2(22, offset), new Vector2(9, 1), CommonStringKeys.STARTING_ITEM, delegate { ListStartingItem(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

            tb = new TextButton(new Vector2(32, offset), new Vector2(6, 1), CommonStringKeys.DELETE, delegate { game.qed.DeleteComponent("StartingItem"); }, Color.red);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        }

        offset += 2;
        tb = new TextButton(new Vector2(25.5f, offset), new Vector2(9, 1), CommonStringKeys.CANCEL, delegate { Cancel(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
    }

    // Create selection list for tiles
    public static void ListTile()
    {
        Game game = Game.Get();

        List<EditorSelectionList.SelectionListEntry> tiles = new List<EditorSelectionList.SelectionListEntry>();
        // This magic string is picked up later for object creation
        tiles.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(
            new StringKey("val","NEW_X",CommonStringKeys.TILE).Translate(),"{NEW:Tile}"));
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Tile)
            {
                tiles.Add(new EditorSelectionList.SelectionListEntry(kv.Key));
            }
        }
        game.qed.esl = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, tiles, delegate { game.qed.SelectComponent(); });
        game.qed.esl.SelectItem();
    }

    // Create selection list for Doors
    public static void ListDoor()
    {
        Game game = Game.Get();

        List<EditorSelectionList.SelectionListEntry> doors = new List<EditorSelectionList.SelectionListEntry>();
        // This magic string is picked up later for object creation
        doors.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(
            new StringKey("val","NEW_X",CommonStringKeys.DOOR).Translate(),"{NEW:Door}"));
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Door)
            {
                doors.Add(new EditorSelectionList.SelectionListEntry(kv.Key));
            }
        }
        game.qed.esl = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, doors, delegate { game.qed.SelectComponent(); });
        game.qed.esl.SelectItem();
    }

    // Create selection list for tokens
    public static void ListToken()
    {
        Game game = Game.Get();

        List<EditorSelectionList.SelectionListEntry> tokens = new List<EditorSelectionList.SelectionListEntry>();
        // This magic string is picked up later for object creation
        tokens.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(
            new StringKey("val","NEW_X",CommonStringKeys.TOKEN).Translate(),"{NEW:Token}"));
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Token)
            {
                tokens.Add(new EditorSelectionList.SelectionListEntry(kv.Key));
            }
        }
        game.qed.esl = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, tokens, delegate { game.qed.SelectComponent(); });
        game.qed.esl.SelectItem();
    }

    // Create selection list for monster spawns
    public static void ListSpawn()
    {
        Game game = Game.Get();

        List<EditorSelectionList.SelectionListEntry> monsters = new List<EditorSelectionList.SelectionListEntry>();
        // This magic string is picked up later for object creation
        monsters.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(
            new StringKey("val","NEW_X",CommonStringKeys.SPAWN).Translate() ,"{NEW:Spawn}"));
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Spawn)
            {
                monsters.Add(new EditorSelectionList.SelectionListEntry(kv.Key));
            }
        }
        game.qed.esl = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, monsters, delegate { game.qed.SelectComponent(); });
        game.qed.esl.SelectItem();
    }

    // Create selection list for mplaces
    public static void ListMPlace()
    {
        Game game = Game.Get();

        List<EditorSelectionList.SelectionListEntry> mplaces = new List<EditorSelectionList.SelectionListEntry>();
        // This magic string is picked up later for object creation
        mplaces.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(
            new StringKey("val","NEW_X",CommonStringKeys.MPLACE).Translate(),"{NEW:MPlace}"));
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.MPlace)
            {
                mplaces.Add(new EditorSelectionList.SelectionListEntry(kv.Key));
            }
        }
        game.qed.esl = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, mplaces, delegate { game.qed.SelectComponent(); });
        game.qed.esl.SelectItem();
    }

    // Create selection list for events
    public static void ListEvent()
    {
        Game game = Game.Get();

        List<EditorSelectionList.SelectionListEntry> events = new List<EditorSelectionList.SelectionListEntry>();
        // This magic string is picked up later for object creation
        events.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(
            new StringKey("val","NEW_X",CommonStringKeys.EVENT).Translate(),"{NEW:Event}"));
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                if (!kv.Value.GetType().IsSubclassOf(typeof(QuestData.Event)))
                {
                    events.Add(new EditorSelectionList.SelectionListEntry(kv.Key));
                }
            }
        }
        game.qed.esl = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, events, delegate { game.qed.SelectComponent(); });
        game.qed.esl.SelectItem();
    }

    // Create selection list for events
    public static void ListPuzzle()
    {
        Game game = Game.Get();

        List<EditorSelectionList.SelectionListEntry> puzzle = new List<EditorSelectionList.SelectionListEntry>();
        // This magic string is picked up later for object creation
        puzzle.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(
            new StringKey("val","NEW_X",CommonStringKeys.PUZZLE).Translate(),"{NEW:Puzzle}"));
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Puzzle)
            {
                puzzle.Add(new EditorSelectionList.SelectionListEntry(kv.Key));
            }
        }
        game.qed.esl = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, puzzle, delegate { game.qed.SelectComponent(); });
        game.qed.esl.SelectItem();
    }

    // Create selection list for items
    public static void ListStartingItem()
    {
        Game game = Game.Get();

        List<EditorSelectionList.SelectionListEntry> items = new List<EditorSelectionList.SelectionListEntry>();
        // This magic string is picked up later for object creation
        items.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(
            new StringKey("val","NEW_X",CommonStringKeys.STARTING_ITEM).Translate(),"{NEW:StartingItem}"));
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.StartingItem)
            {
                items.Add(new EditorSelectionList.SelectionListEntry(kv.Key));
            }
        }

        game.qed.esl = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, items, delegate { game.qed.SelectComponent(); });
        game.qed.esl.SelectItem();
    }

    // Create selection list for unique monsters
    public static void ListCustomMonster()
    {
        Game game = Game.Get();

        List<EditorSelectionList.SelectionListEntry> monsters = new List<EditorSelectionList.SelectionListEntry>();
        // This magic string is picked up later for object creation
        monsters.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(
            new StringKey("val","NEW_X",CommonStringKeys.CUSTOM_MONSTER).Translate(),"{NEW:CustomMonster}"));
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.CustomMonster)
            {
                monsters.Add(new EditorSelectionList.SelectionListEntry(kv.Key));
            }
        }

        game.qed.esl = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, monsters, delegate { game.qed.SelectComponent(); });
        game.qed.esl.SelectItem();
    }

    // Create selection list for unique monsters
    public static void ListActivation()
    {
        Game game = Game.Get();

        List<EditorSelectionList.SelectionListEntry> activations = new List<EditorSelectionList.SelectionListEntry>();
        // This magic string is picked up later for object creation
        activations.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(
            new StringKey("val","NEW_X",CommonStringKeys.ACTIVATION).Translate(),"{NEW:Activation}"));
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Activation)
            {
                activations.Add(new EditorSelectionList.SelectionListEntry(kv.Key));
            }
        }

        game.qed.esl = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, activations, delegate { game.qed.SelectComponent(); });
        game.qed.esl.SelectItem();
    }

    // Select a component from a list
    public void SelectComponent()
    {
        SelectComponent(esl.selection);
    }

    // Select a component for editing
    public static void SelectComponent(string name)
    {
        Game game = Game.Get();
        QuestEditorData qed = game.qed;

        // Quest is a special component
        if (name.Equals("Quest"))
        {
            SelectQuest();
            return;
        }
        // These are special strings for creating new objects
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
        if (name.Equals("{NEW:Spawn}"))
        {
            qed.NewSpawn();
            return;
        }
        if (name.Equals("{NEW:MPlace}"))
        {
            qed.NewMPlace();
            return;
        }
        if (name.Equals("{NEW:StartingItem}"))
        {
            qed.NewStartingItem();
            return;
        }
        if (name.Equals("{NEW:CustomMonster}"))
        {
            qed.NewCustomMonster();
            return;
        }
        if (name.Equals("{NEW:Activation}"))
        {
            qed.NewActivation();
            return;
        }
        if (name.Equals("{NEW:Event}"))
        {
            qed.NewEvent();
            return;
        }
        if (name.Equals("{NEW:Puzzle}"))
        {
            qed.NewPuzzle();
            return;
        }
        // This may happen to due rename/delete
        if (!game.quest.qd.components.ContainsKey(name))
        {
            SelectQuest();
        }

        // Determine the component type and select
        if (game.quest.qd.components[name] is QuestData.Tile)
        {
            SelectAsTile(name);
            return;
        }

        if (game.quest.qd.components[name] is QuestData.Door)
        {
            SelectAsDoor(name);
            return;
        }
        if (game.quest.qd.components[name] is QuestData.Token)
        {
            SelectAsToken(name);
            return;
        }
        if (game.quest.qd.components[name] is QuestData.Spawn)
        {
            SelectAsSpawn(name);
            return;
        }
        if (game.quest.qd.components[name] is QuestData.MPlace)
        {
            SelectAsMPlace(name);
            return;
        }
        if (game.quest.qd.components[name] is QuestData.Puzzle)
        {
            SelectAsPuzzle(name);
            return;
        }
        if (game.quest.qd.components[name] is QuestData.StartingItem)
        {
            SelectAsStartingItem(name);
            return;
        }
        if (game.quest.qd.components[name] is QuestData.CustomMonster)
        {
            SelectAsCustomMonster(name);
            return;
        }
        if (game.quest.qd.components[name] is QuestData.Activation)
        {
            SelectAsActivation(name);
            return;
        }
        if (game.quest.qd.components[name] is QuestData.Event)
        {
            SelectAsEvent(name);
            return;
        }
    }

    public static void SelectQuest()
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentQuest());
    }

    public static void SelectAsTile(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentTile(name));
    }

    public static void SelectAsDoor(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentDoor(name));
    }

    public static void SelectAsToken(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentToken(name));
    }

    // Events, tokens, doors and monsters can all be openned as events
    // and as nextevent/vars
    public static void SelectAsEvent(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentEvent(name));
    }

    // Events, tokens, doors and monsters can all be openned as events
    // and as  nextevent/vars
    public static void SelectAsEventVars(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentEventVars(name));
    }

    // Events, tokens, doors and monsters can all be openned as events
    // and as  nextevent/vars
    public static void SelectAsEventNextEvent(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentEventNextEvent(name));
    }

    public static void SelectAsSpawn(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentSpawn(name));
    }

    // Mosters can be opened as a placement list page
    public static void SelectAsSpawnPlacement(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentSpawnPlacement(name));
    }

    public static void SelectAsMPlace(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentMPlace(name));
    }

    public static void SelectAsPuzzle(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentPuzzle(name));
    }

    public static void SelectAsStartingItem(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentStartingItem(name));
    }
    public static void SelectAsCustomMonster(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentCustomMonster(name));
    }
    public static void SelectAsActivation(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentActivation(name));
    }

    // Create a new tile, use next available number
    public void NewTile()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("Tile" + index))
        {
            index++;
        }
        QuestData.Tile tile = new QuestData.Tile("Tile" + index);
        game.quest.qd.components.Add("Tile" + index, tile);

        CameraController cc = GameObject.FindObjectOfType<CameraController>();
        tile.location.x = game.gameType.TileRound() * Mathf.Round(cc.gameObject.transform.position.x / game.gameType.TileRound());
        tile.location.y = game.gameType.TileRound() * Mathf.Round(cc.gameObject.transform.position.y / game.gameType.TileRound());

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
        QuestData.Door door = new QuestData.Door("Door" + index);
        game.quest.qd.components.Add("Door" + index, door);

        CameraController cc = GameObject.FindObjectOfType<CameraController>();
        door.location.x = game.gameType.SelectionRound() * Mathf.Round(cc.gameObject.transform.position.x / game.gameType.SelectionRound());
        door.location.y = game.gameType.SelectionRound() * Mathf.Round(cc.gameObject.transform.position.y / game.gameType.SelectionRound());

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
        QuestData.Token token = new QuestData.Token("Token" + index);
        game.quest.qd.components.Add("Token" + index, token);

        CameraController cc = GameObject.FindObjectOfType<CameraController>();
        token.location.x = game.gameType.SelectionRound() * Mathf.Round(cc.gameObject.transform.position.x / game.gameType.SelectionRound());
        token.location.y = game.gameType.SelectionRound() * Mathf.Round(cc.gameObject.transform.position.y / game.gameType.SelectionRound());

        game.quest.Add("Token" + index);
        SelectComponent("Token" + index);
    }

    public void NewSpawn()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("Spawn" + index))
        {
            index++;
        }
        game.quest.qd.components.Add("Spawn" + index, new QuestData.Spawn("Spawn" + index));
        SelectComponent("Spawn" + index);
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

    public void NewPuzzle()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("Puzzle" + index))
        {
            index++;
        }
        game.quest.qd.components.Add("Puzzle" + index, new QuestData.Puzzle("Puzzle" + index));
        SelectComponent("Puzzle" + index);
    }
    
    public void NewStartingItem()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("StartingItem" + index))
        {
            index++;
        }
        game.quest.qd.components.Add("StartingItem" + index, new QuestData.StartingItem("StartingItem" + index));
        SelectComponent("StartingItem" + index);
    }

    public void NewCustomMonster()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("CustomMonster" + index))
        {
            index++;
        }
        game.quest.qd.components.Add("CustomMonster" + index, new QuestData.CustomMonster("CustomMonster" + index));
        SelectComponent("CustomMonster" + index);
    }

    public void NewActivation()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("Activation" + index))
        {
            index++;
        }
        game.quest.qd.components.Add("Activation" + index, new QuestData.Activation("Activation" + index));
        SelectComponent("Activation" + index);
    }

    // Delete a component by type
    public void DeleteComponent(string type)
    {
        Game game = Game.Get();

        List<EditorSelectionList.SelectionListEntry> toDelete = new List<EditorSelectionList.SelectionListEntry>();

        // List all components of this type
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Key.IndexOf(type) == 0)
            {
                toDelete.Add(new EditorSelectionList.SelectionListEntry(kv.Key));
                toDelete.Add(new EditorSelectionList.SelectionListEntry(""));
            }
        }
        // Create list for user
        esl = new EditorSelectionList(COMPONENT_TO_DELETE, toDelete, delegate { SelectToDelete(); });
        esl.SelectItem();
    }

    // Delete a component
    public void DeleteComponent()
    {
        Game game = Game.Get();

        List<EditorSelectionList.SelectionListEntry> toDelete = new List<EditorSelectionList.SelectionListEntry>();

        // List all components
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            toDelete.Add(new EditorSelectionList.SelectionListEntry(kv.Key));
            toDelete.Add(new EditorSelectionList.SelectionListEntry(""));
        }
        // Create list for user
        esl = new EditorSelectionList(COMPONENT_TO_DELETE, toDelete, delegate { SelectToDelete(); });
        esl.SelectItem();
    }

    // Item selected from list for deletion
    public void SelectToDelete()
    {
        Game game = Game.Get();

        // Remove all references to the deleted component
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            kv.Value.RemoveReference(esl.selection);
        }

        // Remove the component
        if (game.quest.qd.components.ContainsKey(esl.selection))
        {
            game.quest.qd.components.Remove(esl.selection);
        }

        LocalizationRead.scenarioDict.RemoveKeyPrefix(esl.selection);

        // Clean up the current quest environment
        game.quest.Remove(esl.selection);

        Destroyer.Dialog();

        // If we deleted the selected item, go back to the last item
        if (selection.name.Equals(esl.selection))
        {
            Back();
        }
    }

    // Cancel a component selection, clean up
    public static void Cancel()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
    }

    // This is called game
    public void MouseDown()
    {
        selection.MouseDown();
    }
}
