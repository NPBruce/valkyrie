using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorComponentPuzzle : EditorComponent
{
    QuestData.Puzzle puzzleComponent;
    EditorSelectionList classList;
    DialogBoxEditable levelDBE;

    public EditorComponentPuzzle(string nameIn) : base()
    {
        Game game = Game.Get();
        puzzleComponent = game.quest.qd.components[nameIn] as QuestData.Puzzle;
        component = puzzleComponent;
        name = component.name;
        Update();
    }
    
    override public void Update()
    {
        base.Update();
        Game game = Game.Get();

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(3, 1), "Puzzle", delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 0), new Vector2(16, 1), name.Substring("Puzzle".Length), delegate { QuestEditorData.ListPuzzle(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        DialogBox db = new DialogBox(new Vector2(0, 2), new Vector2(3, 1), "Class:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 2), new Vector2(8, 1), puzzleComponent.puzzleClass, delegate { Class(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 4), new Vector2(3, 1), "Level:");
        db.ApplyTag("editor");

        levelDBE = new DialogBoxEditable(new Vector2(3, 4), new Vector2(2, 1), puzzleComponent.puzzleLevel.ToString(), delegate { UpdateLevel(); });
        levelDBE.ApplyTag("editor");
        levelDBE.AddBorder();

        tb = new TextButton(new Vector2(0, 6), new Vector2(8, 1), "Event", delegate { QuestEditorData.SelectAsEvent(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");
    }

    public void Class()
    {
        List<string> puzzleClass = new List<string>();
        puzzleClass.Add("slide");
        classList = new EditorSelectionList("Select Class", puzzleClass, delegate { SelectClass(); });
        classList.SelectItem();
    }

    public void SelectClass()
    {
        puzzleComponent.puzzleClass = classList.selection;
        Update();
    }

    public void UpdateLevel()
    {
        int.TryParse(levelDBE.uiInput.text, out puzzleComponent.puzzleLevel);
        Update();
    }
}
