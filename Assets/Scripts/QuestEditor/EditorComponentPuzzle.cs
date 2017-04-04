using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;
using System.IO;

public class EditorComponentPuzzle : EditorComponent
{
    private readonly StringKey PUZZLE_GUESS = new StringKey("val", "PUZZLE_GUESS");
    private readonly StringKey PUZZLE = new StringKey("val", "PUZZLE");
    private readonly StringKey PUZZLE_CLASS = new StringKey("val", "PUZZLE_CLASS");
    private readonly StringKey PUZZLE_CLASS_SELECT = new StringKey("val", "PUZZLE_CLASS_SELECT");
    private readonly StringKey ICON_SUCCESS_RESULT = new StringKey("val", "ICON_SUCCESS_RESULT");
    private readonly StringKey ICON_INVESTIGATION_RESULT = new StringKey("val", "ICON_INVESTIGATION_RESULT");
    private readonly StringKey PUZZLE_LEVEL = new StringKey("val", "PUZZLE_LEVEL");
    private readonly StringKey IMAGE = new StringKey("val", "IMAGE");
    private readonly StringKey PUZZLE_ALT_LEVEL = new StringKey("val", "PUZZLE_ALT_LEVEL");
    private readonly StringKey PUZZLE_SELECT_SKILL = new StringKey("val", "PUZZLE_SELECT_SKILL");
    private readonly StringKey SELECT_IMAGE = new StringKey("val", "SELECT_IMAGE");

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

        DialogBox db = new DialogBox(new Vector2(0, 2), new Vector2(3, 1),
            new StringKey("val", "X_COLON", PUZZLE_CLASS));
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 2), new Vector2(8, 1), puzzleComponent.puzzleClass, delegate { Class(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 4), new Vector2(3, 1),
            new StringKey("val", "X_COLON", CommonStringKeys.SKILL));
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 4), new Vector2(2, 1), puzzleComponent.skill, delegate { Skill(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 6), new Vector2(3, 1),
            new StringKey("val", "X_COLON", PUZZLE_LEVEL));
        db.ApplyTag("editor");

        levelDBE = new DialogBoxEditable(new Vector2(3, 6), new Vector2(2, 1), puzzleComponent.puzzleLevel.ToString(), delegate { UpdateLevel(); });
        levelDBE.ApplyTag("editor");
        levelDBE.AddBorder();

        if (!puzzleComponent.puzzleClass.key.Equals("PUZZLE_SLIDE_CLASS"))
        {
            db = new DialogBox(new Vector2(0, 8), new Vector2(3, 1),
                new StringKey("val", "X_COLON", PUZZLE_ALT_LEVEL));
            db.ApplyTag("editor");

            altLevelDBE = new DialogBoxEditable(new Vector2(3, 8), new Vector2(2, 1), puzzleComponent.puzzleAltLevel.ToString(), delegate { UpdateAltLevel(); });
            altLevelDBE.ApplyTag("editor");
            altLevelDBE.AddBorder();

            if (puzzleComponent.puzzleClass.key.Equals("PUZZLE_IMAGE_CLASS"))
            {
                db = new DialogBox(new Vector2(0, 10), new Vector2(3, 1),
                    new StringKey("val", "X_COLON", IMAGE));
                db.ApplyTag("editor");

                tb = new TextButton(new Vector2(3, 10), new Vector2(8, 1), 
                    new StringKey(puzzleComponent.imageType,false), delegate { Image(); });
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
        puzzleClass.Add(new EditorSelectionList.SelectionListEntry(new StringKey("val", "PUZZLE_SLIDE_CLASS").key));
        puzzleClass.Add(new EditorSelectionList.SelectionListEntry(new StringKey("val", "PUZZLE_CODE_CLASS").key));
        puzzleClass.Add(new EditorSelectionList.SelectionListEntry(new StringKey("val", "PUZZLE_IMAGE_CLASS").key));
        classList = new EditorSelectionList(PUZZLE_CLASS_SELECT, puzzleClass, delegate { SelectClass(); });
        classList.SelectItem();
    }

    public void SelectClass()
    {
        // the selection has the key (ie:{val:PUZZLE_SLIDE_CLASS}) so we can build the StringKey.
        puzzleComponent.puzzleClass = new StringKey(classList.selection);
        if (!puzzleComponent.puzzleClass.key.Equals("PUZZLE_IMAGE_CLASS"))
        {
            puzzleComponent.imageType = "";
        }
        Update();
    }

    public void Skill()
    {
        List<EditorSelectionList.SelectionListEntry> skills = new List<EditorSelectionList.SelectionListEntry>();
        skills.Add(new EditorSelectionList.SelectionListEntry(new StringKey("val", "ICON_SKILL_WILL").key));
        skills.Add(new EditorSelectionList.SelectionListEntry(new StringKey("val", "ICON_SKILL_STRENGTH").key));
        skills.Add(new EditorSelectionList.SelectionListEntry(new StringKey("val", "ICON_SKILL_AGILITY").key));
        skills.Add(new EditorSelectionList.SelectionListEntry(new StringKey("val", "ICON_SKILL_LORE").key));
        skills.Add(new EditorSelectionList.SelectionListEntry(new StringKey("val", "ICON_SKILL_INFLUENCE").key));
        skills.Add(new EditorSelectionList.SelectionListEntry(new StringKey("val", "ICON_SKILL_OBSERVATION").key));
        skillList = new EditorSelectionList(PUZZLE_SELECT_SKILL, skills, delegate { SelectSkill(); });
        skillList.SelectItem();
    }

    public void SelectSkill()
    {
        puzzleComponent.skill = new StringKey(skillList.selection);
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
        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;
        List<EditorSelectionList.SelectionListEntry> puzzleImage = new List<EditorSelectionList.SelectionListEntry>();
        foreach (string s in Directory.GetFiles(relativePath, "*.png", SearchOption.AllDirectories))
        {
            puzzleImage.Add(new EditorSelectionList.SelectionListEntry(s.Substring(relativePath.Length + 1), "File"));
        }
        foreach (string s in Directory.GetFiles(relativePath, "*.jpg", SearchOption.AllDirectories))
        {
            puzzleImage.Add(new EditorSelectionList.SelectionListEntry(s.Substring(relativePath.Length + 1), "File"));
        }
        foreach (KeyValuePair<string, PuzzleData> kv in Game.Get().cd.puzzles)
        {
            puzzleImage.Add(new EditorSelectionList.SelectionListEntry(kv.Key, "MoM"));
        }
        imageList = new EditorSelectionList(SELECT_IMAGE, puzzleImage, delegate { SelectImage(); });
        imageList.SelectItem();
    }

    public void SelectImage()
    {
        puzzleComponent.imageType = imageList.selection;
        Update();
    }
}
