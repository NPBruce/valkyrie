using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;
using System.IO;

public class EditorComponentPuzzle : EditorComponent
{
    private readonly StringKey PUZZLE = new StringKey("val", "PUZZLE");
    private readonly StringKey PUZZLE_CLASS = new StringKey("val", "PUZZLE_CLASS");
    private readonly StringKey PUZZLE_CLASS_SELECT = new StringKey("val", "PUZZLE_CLASS_SELECT");
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
        //Game game = Game.Get();

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(3, 1), PUZZLE, delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(
            new Vector2(3, 0), new Vector2(16, 1), 
            new StringKey(null,name.Substring("Puzzle".Length),false), 
            delegate { QuestEditorData.ListPuzzle(); });

        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), CommonStringKeys.E, delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.EDITOR);

        DialogBox db = new DialogBox(new Vector2(0, 2), new Vector2(3, 1),
            new StringKey("val", "X_COLON", PUZZLE_CLASS));
        db.ApplyTag(Game.EDITOR);

        // Translate puzzle type trait
        tb = new TextButton(new Vector2(5, 2), new Vector2(8, 1), 
            new StringKey("val",puzzleComponent.puzzleClass), delegate { Class(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.EDITOR);

        db = new DialogBox(new Vector2(0, 4), new Vector2(4, 1),
            new StringKey("val", "X_COLON", CommonStringKeys.SKILL));
        db.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(5, 4), new Vector2(6, 1), 
            new StringKey(null, puzzleComponent.skill,false), delegate { Skill(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.EDITOR);

        db = new DialogBox(new Vector2(0, 6), new Vector2(4, 1),
            new StringKey("val", "X_COLON", PUZZLE_LEVEL));
        db.ApplyTag(Game.EDITOR);

        // Numbers dont need translation
        levelDBE = new DialogBoxEditable(new Vector2(5, 6), new Vector2(2, 1), 
            puzzleComponent.puzzleLevel.ToString(), false, delegate { UpdateLevel(); });
        levelDBE.ApplyTag(Game.EDITOR);
        levelDBE.AddBorder();

        if (!puzzleComponent.puzzleClass.Equals("slide"))
        {
            db = new DialogBox(new Vector2(0, 8), new Vector2(5, 1),
                new StringKey("val", "X_COLON", PUZZLE_ALT_LEVEL));
            db.ApplyTag(Game.EDITOR);

            // Numbers dont need translation
            altLevelDBE = new DialogBoxEditable(new Vector2(5, 8), new Vector2(2, 1), 
                puzzleComponent.puzzleAltLevel.ToString(), false, delegate { UpdateAltLevel(); });
            altLevelDBE.ApplyTag(Game.EDITOR);
            altLevelDBE.AddBorder();

            db = new DialogBox(new Vector2(0, 10), new Vector2(3, 1),
                new StringKey("val", "X_COLON", IMAGE));
            db.ApplyTag(Game.EDITOR);

            tb = new TextButton(new Vector2(5, 10), new Vector2(8, 1), 
                new StringKey(null, puzzleComponent.imageType,false), delegate { Image(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag(Game.EDITOR);
        }

        tb = new TextButton(new Vector2(0, 12), new Vector2(8, 1), CommonStringKeys.EVENT, delegate { QuestEditorData.SelectAsEvent(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.EDITOR);
    }

    public void Class()
    {
        List<EditorSelectionList.SelectionListEntry> puzzleClass = new List<EditorSelectionList.SelectionListEntry>();
        puzzleClass.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem("slide"));
        puzzleClass.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem("code"));
        puzzleClass.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem("image"));
        classList = new EditorSelectionList(PUZZLE_CLASS_SELECT, puzzleClass, delegate { SelectClass(); });
        classList.SelectItem();
    }

    public void SelectClass()
    {
        if (!puzzleComponent.puzzleClass.Equals(classList.selection))
        {
            puzzleComponent.imageType = "";
        }
        // the selection has the key (ie:{val:PUZZLE_SLIDE_CLASS}) so we can build the StringKey.
        puzzleComponent.puzzleClass = classList.selection;
        Update();
    }

    public void Skill()
    {
        List<EditorSelectionList.SelectionListEntry> skill = new List<EditorSelectionList.SelectionListEntry>();
        skill.Add(new EditorSelectionList.SelectionListEntry("{will} " + EventManager.OutputSymbolReplace("{will}")));
        skill.Add(new EditorSelectionList.SelectionListEntry("{strength} " + EventManager.OutputSymbolReplace("{strength}")));
        skill.Add(new EditorSelectionList.SelectionListEntry("{agility} " + EventManager.OutputSymbolReplace("{agility}")));
        skill.Add(new EditorSelectionList.SelectionListEntry("{lore} " + EventManager.OutputSymbolReplace("{lore}")));
        skill.Add(new EditorSelectionList.SelectionListEntry("{influence} " + EventManager.OutputSymbolReplace("{influence}")));
        skill.Add(new EditorSelectionList.SelectionListEntry("{observation} " + EventManager.OutputSymbolReplace("{observation}")));
        skillList = new EditorSelectionList(PUZZLE_SELECT_SKILL, skill, delegate { SelectSkill(); });
        skillList.SelectItem();
    }

    public void SelectSkill()
    {
        puzzleComponent.skill = skillList.selection.Substring(0, skillList.selection.IndexOf(" "));
        Update();
    }

    public void UpdateLevel()
    {
        int.TryParse(levelDBE.Text, out puzzleComponent.puzzleLevel);
        Update();
    }

    public void UpdateAltLevel()
    {
        int.TryParse(altLevelDBE.Text, out puzzleComponent.puzzleAltLevel);
        Update();
    }
    
    public void Image()
    {
        List<EditorSelectionList.SelectionListEntry> puzzleImage = new List<EditorSelectionList.SelectionListEntry>();
        if (puzzleComponent.puzzleClass.Equals("code"))
        {
            puzzleImage.Add(new EditorSelectionList.SelectionListEntry(""));
            puzzleImage.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(new StringKey("val", "SYMBOL").Translate(), "symbol"));
        }
        else
        {
            string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;
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
