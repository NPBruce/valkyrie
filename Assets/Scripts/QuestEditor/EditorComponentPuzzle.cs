using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentPuzzle : EditorComponent
{
    private readonly StringKey PUZZLE_GUESS = new StringKey("val", "PUZZLE_GUESS");
    private readonly StringKey PUZZLE = new StringKey("val", "PUZZLE");
    private readonly StringKey PUZZLE_CLASS_DOTS = new StringKey("val", "PUZZLE_CLASS_DOTS");
    private readonly StringKey PUZZLE_CLASS_SELECT = new StringKey("val", "PUZZLE_CLASS_SELECT");
    private readonly StringKey ICON_SUCCESS_RESULT = new StringKey("val", "ICON_SUCCESS_RESULT");
    private readonly StringKey ICON_INVESTIGATION_RESULT = new StringKey("val", "ICON_INVESTIGATION_RESULT");
    private readonly StringKey PUZZLE_LEVEL_DOTS = new StringKey("val", "PUZZLE_LEVEL_DOTS");
    private readonly StringKey PUZZLE_IMAGE_DOTS = new StringKey("val", "PUZZLE_IMAGE_DOTS");
    private readonly StringKey PUZZLE_ALT_LEVEL_DOTS = new StringKey("val", "PUZZLE_ALT_LEVEL_DOTS");
    private readonly StringKey PUZZLE_SELECT_SKILL = new StringKey("val", "PUZZLE_SELECT_SKILL");
    private readonly StringKey PUZZLE_SELECT_IMAGE = new StringKey("val", "PUZZLE_SELECT_IMAGE");

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

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(3, 1), PUZZLE, delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(
            new Vector2(3, 0), new Vector2(16, 1), 
            new StringKey(name.Substring("Puzzle".Length),false), 
            delegate { QuestEditorData.ListPuzzle(); });

        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), CommonStringKeys.E, delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        DialogBox db = new DialogBox(new Vector2(0, 2), new Vector2(3, 1), PUZZLE_CLASS_DOTS);
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 2), new Vector2(8, 1), puzzleComponent.puzzleClass, delegate { Class(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 4), new Vector2(3, 1), CommonStringKeys.SKILL_DOTS);
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 4), new Vector2(2, 1), puzzleComponent.skill, delegate { Skill(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 6), new Vector2(3, 1), PUZZLE_LEVEL_DOTS);
        db.ApplyTag("editor");

        levelDBE = new DialogBoxEditable(new Vector2(3, 6), new Vector2(2, 1), puzzleComponent.puzzleLevel.ToString(), delegate { UpdateLevel(); });
        levelDBE.ApplyTag("editor");
        levelDBE.AddBorder();

        if (!puzzleComponent.puzzleClass.key.Equals("PUZZLE_SLIDE_CLASS"))
        {
            db = new DialogBox(new Vector2(0, 8), new Vector2(3, 1), PUZZLE_ALT_LEVEL_DOTS);
            db.ApplyTag("editor");

            altLevelDBE = new DialogBoxEditable(new Vector2(3, 8), new Vector2(2, 1), puzzleComponent.puzzleAltLevel.ToString(), delegate { UpdateAltLevel(); });
            altLevelDBE.ApplyTag("editor");
            altLevelDBE.AddBorder();

            if (puzzleComponent.puzzleClass.key.Equals("PUZZLE_IMAGE_CLASS"))
            {
                db = new DialogBox(new Vector2(0, 10), new Vector2(3, 1), PUZZLE_IMAGE_DOTS);
                db.ApplyTag("editor");

                tb = new TextButton(new Vector2(3, 10), new Vector2(8, 1), puzzleComponent.imageType, delegate { Image(); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        tb = new TextButton(new Vector2(0, 12), new Vector2(8, 1), CommonStringKeys.EVENT, delegate { QuestEditorData.SelectAsEvent(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");
    }

    public void Class()
    {
        List<EditorSelectionList.SelectionListEntry> puzzleClass = new List<EditorSelectionList.SelectionListEntry>();
        puzzleClass.Add(new EditorSelectionList.SelectionListEntry(new StringKey("val", "PUZZLE_SLIDE_CLASS")));
        puzzleClass.Add(new EditorSelectionList.SelectionListEntry(new StringKey("val", "PUZZLE_CODE_CLASS")));
        puzzleClass.Add(new EditorSelectionList.SelectionListEntry(new StringKey("val", "PUZZLE_IMAGE_CLASS")));
        classList = new EditorSelectionList(PUZZLE_CLASS_SELECT, puzzleClass, delegate { SelectClass(); });
        classList.SelectItem();
    }

    public void SelectClass()
    {
        puzzleComponent.puzzleClass = classList.selection;
        if (!puzzleComponent.puzzleClass.key.Equals("PUZZLE_IMAGE_CLASS"))
        {
            puzzleComponent.imageType = StringKey.NULL;
        }
        Update();
    }

    public void Skill()
    {
        List<EditorSelectionList.SelectionListEntry> skills = new List<EditorSelectionList.SelectionListEntry>();
        skills.Add(new EditorSelectionList.SelectionListEntry(new StringKey("val", "ICON_SKILL_WILL")));
        skills.Add(new EditorSelectionList.SelectionListEntry(new StringKey("val", "ICON_SKILL_STRENGTH")));
        skills.Add(new EditorSelectionList.SelectionListEntry(new StringKey("val", "ICON_SKILL_AGILITY")));
        skills.Add(new EditorSelectionList.SelectionListEntry(new StringKey("val", "ICON_SKILL_LORE")));
        skills.Add(new EditorSelectionList.SelectionListEntry(new StringKey("val", "ICON_SKILL_INFLUENCE")));
        skills.Add(new EditorSelectionList.SelectionListEntry(new StringKey("val", "ICON_SKILL_OBSERVATION")));
        skillList = new EditorSelectionList(PUZZLE_SELECT_SKILL, skills, delegate { SelectSkill(); });
        skillList.SelectItem();
    }

    public void SelectSkill()
    {
        puzzleComponent.skill = skillList.selection;
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
        List<EditorSelectionList.SelectionListEntry> puzzleImage = new List<EditorSelectionList.SelectionListEntry>();
        foreach (KeyValuePair<string, PuzzleData> kv in Game.Get().cd.puzzles)
        {
            puzzleImage.Add(new EditorSelectionList.SelectionListEntry(new StringKey(kv.Key,false)));
        }
        imageList = new EditorSelectionList(PUZZLE_SELECT_IMAGE, puzzleImage, delegate { SelectImage(); });
        imageList.SelectItem();
    }

    public void SelectImage()
    {
        puzzleComponent.imageType = imageList.selection;
        Update();
    }
}
