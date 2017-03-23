using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorComponentPuzzle : EditorComponent
{
    QuestData.Puzzle puzzleComponent;
    EditorSelectionList classList;
    EditorSelectionList imageList;
    EditorSelectionList skillList;
    DialogBoxEditable levelDBE;
    DialogBoxEditable altLevelDBE;

    public EditorComponentPuzzle(string nameIn) : base()
    {
        Game game = Game.Get();
        puzzleComponent = game.quest.qd.components[nameIn] as QuestData.Puzzle;
        component = puzzleComponent;
        name = component.sectionName;
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

        db = new DialogBox(new Vector2(0, 4), new Vector2(3, 1), "Skill:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 4), new Vector2(2, 1), EventManager.SymbolReplace(puzzleComponent.skill), delegate { Skill(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 6), new Vector2(3, 1), "Level:");
        db.ApplyTag("editor");

        levelDBE = new DialogBoxEditable(new Vector2(3, 6), new Vector2(2, 1), puzzleComponent.puzzleLevel.ToString(), delegate { UpdateLevel(); });
        levelDBE.ApplyTag("editor");
        levelDBE.AddBorder();

        if (!puzzleComponent.puzzleClass.Equals("slide"))
        {
            db = new DialogBox(new Vector2(0, 8), new Vector2(3, 1), "Alt Level:");
            db.ApplyTag("editor");

            altLevelDBE = new DialogBoxEditable(new Vector2(3, 8), new Vector2(2, 1), puzzleComponent.puzzleAltLevel.ToString(), delegate { UpdateAltLevel(); });
            altLevelDBE.ApplyTag("editor");
            altLevelDBE.AddBorder();

            if (puzzleComponent.puzzleClass.Equals("image"))
            {
                db = new DialogBox(new Vector2(0, 10), new Vector2(3, 1), "Image:");
                db.ApplyTag("editor");

                tb = new TextButton(new Vector2(3, 10), new Vector2(8, 1), puzzleComponent.imageType, delegate { Image(); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        tb = new TextButton(new Vector2(0, 12), new Vector2(8, 1), "Event", delegate { QuestEditorData.SelectAsEvent(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");
    }

    public void Class()
    {
        List<string> puzzleClass = new List<string>();
        puzzleClass.Add("slide");
        puzzleClass.Add("code");
        puzzleClass.Add("image");
        classList = new EditorSelectionList("Select Class", puzzleClass, delegate { SelectClass(); });
        classList.SelectItem();
    }

    public void SelectClass()
    {
        puzzleComponent.puzzleClass = classList.selection;
        if (!puzzleComponent.puzzleClass.Equals("image"))
        {
            puzzleComponent.imageType = "";
        }
        Update();
    }

    public void Skill()
    {
        List<string> skill = new List<string>();
        skill.Add("{will} " + EventManager.SymbolReplace("{will}"));
        skill.Add("{strength} " + EventManager.SymbolReplace("{strength}"));
        skill.Add("{agility} " + EventManager.SymbolReplace("{agility}"));
        skill.Add("{lore} " + EventManager.SymbolReplace("{lore}"));
        skill.Add("{influence} " + EventManager.SymbolReplace("{influence}"));
        skill.Add("{observation} " + EventManager.SymbolReplace("{observation}"));
        skillList = new EditorSelectionList("Select Skill", skill, delegate { SelectSkill(); });
        skillList.SelectItem();
    }

    public void SelectSkill()
    {
        puzzleComponent.skill = skillList.selection.Split(" ".ToCharArray())[0];
        Update();
    }

    public void UpdateLevel()
    {
        int.TryParse(levelDBE.uiInput.text, out puzzleComponent.puzzleLevel);
        Update();
    }

    public void UpdateAltLevel()
    {
        int.TryParse(altLevelDBE.uiInput.text, out puzzleComponent.puzzleAltLevel);
        Update();
    }
    
    public void Image()
    {
        List<string> puzzleImage = new List<string>();
        foreach (KeyValuePair<string, PuzzleData> kv in Game.Get().cd.puzzles)
        {
            puzzleImage.Add(kv.Key);
        }
        imageList = new EditorSelectionList("Select Image", puzzleImage, delegate { SelectImage(); });
        imageList.SelectItem();
    }

    public void SelectImage()
    {
        puzzleComponent.imageType = imageList.selection;
        Update();
    }
}
