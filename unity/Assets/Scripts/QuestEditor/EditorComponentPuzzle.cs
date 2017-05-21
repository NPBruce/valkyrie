using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
using System.IO;

public class EditorComponentPuzzle : EditorComponentEvent
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


    public EditorComponentPuzzle(string nameIn) : base(nameIn)
    {
    }

    override public float AddPosition(float offset)
    {
        return offset;
    }
    
    override public float AddSubEventComponents(float offset)
    {
        puzzleComponent = component as QuestData.Puzzle;

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0, offset, 3, 1);
        ui.SetText(new StringKey("val", "X_COLON", PUZZLE_CLASS));

        // Translate puzzle type trait
        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(5, offset, 8, 1);
        ui.SetText(new StringKey("val", puzzleComponent.puzzleClass));
        ui.SetButton(delegate { Class(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.SKILL));

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(5, offset, 6, 1);
        ui.SetText(puzzleComponent.skill);
        ui.SetButton(delegate { Skill(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", PUZZLE_LEVEL));

        // Numbers dont need translation
        levelDBE = new DialogBoxEditable(new Vector2(5, offset), new Vector2(2, 1), 
            puzzleComponent.puzzleLevel.ToString(), false, delegate { UpdateLevel(); });
        levelDBE.background.transform.SetParent(scrollArea.transform);
        levelDBE.ApplyTag(Game.EDITOR);
        levelDBE.AddBorder();
        offset += 2;

        if (!puzzleComponent.puzzleClass.Equals("slide"))
        {
            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(0, offset, 5, 1);
            ui.SetText(new StringKey("val", "X_COLON", PUZZLE_ALT_LEVEL));

            // Numbers dont need translation
            altLevelDBE = new DialogBoxEditable(new Vector2(5, offset), new Vector2(2, 1), 
                puzzleComponent.puzzleAltLevel.ToString(), false, delegate { UpdateAltLevel(); });
            altLevelDBE.background.transform.SetParent(scrollArea.transform);
            altLevelDBE.ApplyTag(Game.EDITOR);
            altLevelDBE.AddBorder();
            offset += 2;

            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(0, offset, 3, 1);
            ui.SetText(new StringKey("val", "X_COLON", IMAGE));

            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(5, offset, 8, 1);
            ui.SetText(puzzleComponent.imageType);
            ui.SetButton(delegate { Image(); });
            new UIElementBorder(ui);
            offset += 2;
        }

        return offset;
    }
    
    override public float AddEventDialog(float offset)
    {
        return offset;
    }

    override public void Highlight()
    {
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
