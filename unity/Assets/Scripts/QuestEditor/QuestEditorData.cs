﻿using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

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

    // Start the editor from old version
    public QuestEditorData(QuestEditorData old)
    {
        Game.Get().qed = this;
        selectionStack = new Stack<EditorComponent>();
        if (old == null || old.selection == null)
        {
            // Start at the quest component
            SelectQuest();
        }
        else
        {
            selectionStack = old.selectionStack;
            selectionStack.Push(old.selection);
            Back();
        }
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
    public static void TypeSelect(string type = "")
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();
        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(SelectComponent, CommonStringKeys.SELECT_ITEM);

        select.AddNewComponentItem(CommonStringKeys.TILE);
        select.AddNewComponentItem(CommonStringKeys.TOKEN);
        select.AddNewComponentItem(CommonStringKeys.SPAWN);
        select.AddNewComponentItem(CommonStringKeys.EVENT);
        select.AddNewComponentItem(CommonStringKeys.CUSTOMMONSTER);
        select.AddNewComponentItem(CommonStringKeys.UI);
        select.AddNewComponentItem(CommonStringKeys.QITEM);
        if (game.gameType is D2EGameType)
        {
            select.AddNewComponentItem(CommonStringKeys.ACTIVATION);
            select.AddNewComponentItem(CommonStringKeys.DOOR);
            select.AddNewComponentItem(CommonStringKeys.MPLACE);
        }
        else
        {
            select.AddNewComponentItem(CommonStringKeys.PUZZLE);
        }

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { "Quest" });
        select.AddItem(CommonStringKeys.QUEST, traits);

        foreach (QuestData.QuestComponent c in game.quest.qd.components.Values)
        {
            if (!(c is PerilData))
            {
                select.AddItem(c);
            }
        }

        select.Draw();
        if (type.Length > 0)
        {
            select.SelectTrait(new StringKey("val", "TYPE").Translate(), new StringKey("val", type.ToUpper()).Translate());
        }
    }

    // Select a component for editing
    public static void SelectComponent(string name)
    {
        Game game = Game.Get();
        QuestEditorData qed = game.qed;

        // Quest is a special component
        if (name.Equals(CommonStringKeys.QUEST.key))
        {
            SelectQuest();
            return;
        }
        // These are special strings for creating new objects
        if (name.Equals("{NEW:TILE}"))
        {
            qed.NewTile();
            return;
        }
        if (name.Equals("{NEW:DOOR}"))
        {
            qed.NewDoor();
            return;
        }
        if (name.Equals("{NEW:TOKEN}"))
        {
            qed.NewToken();
            return;
        }
        if (name.Equals("{NEW:UI}"))
        {
            qed.NewUI();
            return;
        }
        if (name.Equals("{NEW:SPAWN}"))
        {
            qed.NewSpawn();
            return;
        }
        if (name.Equals("{NEW:MPLACE}"))
        {
            qed.NewMPlace();
            return;
        }
        if (name.Equals("{NEW:QITEM}"))
        {
            qed.NewItem();
            return;
        }
        if (name.Equals("{NEW:CUSTOMMONSTER}"))
        {
            qed.NewCustomMonster();
            return;
        }
        if (name.Equals("{NEW:ACTIVATION}"))
        {
            qed.NewActivation();
            return;
        }
        if (name.Equals("{NEW:EVENT}"))
        {
            qed.NewEvent();
            return;
        }
        if (name.Equals("{NEW:PUZZLE}"))
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
        if (game.quest.qd.components[name] is QuestData.UI)
        {
            SelectAsUI(name);
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
        if (game.quest.qd.components[name] is QuestData.QItem)
        {
            SelectAsItem(name);
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

    public static void SelectAsUI(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentUI(name));
    }

    public static void SelectAsEvent(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentEvent(name));
    }

    public static void SelectAsSpawn(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentSpawn(name));
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

    public static void SelectAsItem(string name)
    {
        Game game = Game.Get();
        game.qed.NewSelection(new EditorComponentItem(name));
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

    public void NewUI()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("UI" + index))
        {
            index++;
        }
        QuestData.UI ui = new QuestData.UI("UI" + index);
        game.quest.qd.components.Add("UI" + index, ui);
        SelectComponent("UI" + index);
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
    
    public void NewItem()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.quest.qd.components.ContainsKey("QItem" + index))
        {
            index++;
        }
        game.quest.qd.components.Add("QItem" + index, new QuestData.QItem("QItem" + index));
        SelectComponent("QItem" + index);
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

    // Item selected from list for deletion
    public static void DeleteCurrentComponent()
    {
        Game game = Game.Get();
        string name = game.qed.selection.name;
        Destroyer.Dialog();

        if (name.Length == 0) return;

        // Remove all references to the deleted component
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            kv.Value.RemoveReference(name);
        }

        // Remove the component
        if (game.quest.qd.components.ContainsKey(name))
        {
            game.quest.qd.components.Remove(name);
        }

        LocalizationRead.scenarioDict.RemoveKeyPrefix(name + ".");

        // Clean up the current quest environment
        game.quest.Remove(name);

        Back();
    }

    // Cancel a component selection, clean up
    public static void Cancel()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            Object.Destroy(go);
    }

    // This is called game
    public void MouseDown()
    {
        selection.MouseDown();
    }
}
