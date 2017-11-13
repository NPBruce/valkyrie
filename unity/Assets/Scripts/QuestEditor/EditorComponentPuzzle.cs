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

    UIElementEditable levelUIE;
    UIElementEditable altLevelUIE;

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

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 3, 1);
        ui.SetText(new StringKey("val", "X_COLON", PUZZLE_CLASS));

        // Translate puzzle type trait
        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(5, offset, 8, 1);
        ui.SetText(new StringKey("val", puzzleComponent.puzzleClass));
        ui.SetButton(delegate { Class(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.SKILL));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(5, offset, 6, 1);
        ui.SetText(puzzleComponent.skill);
        ui.SetButton(delegate { Skill(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", PUZZLE_LEVEL));

        levelUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
        levelUIE.SetLocation(5, offset, 2, 1);
        levelUIE.SetText(puzzleComponent.puzzleLevel.ToString());
        levelUIE.SetSingleLine();
        levelUIE.SetButton(delegate { UpdateLevel(); });
        new UIElementBorder(levelUIE);
        offset += 2;

        if (puzzleComponent.puzzleClass.Equals("image") || puzzleComponent.puzzleClass.Equals("code"))
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0, offset, 5, 1);
            ui.SetText(new StringKey("val", "X_COLON", PUZZLE_ALT_LEVEL));

            altLevelUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
            altLevelUIE.SetLocation(5, offset, 2, 1);
            altLevelUIE.SetText(puzzleComponent.puzzleAltLevel.ToString());
            altLevelUIE.SetSingleLine();
            altLevelUIE.SetButton(delegate { UpdateAltLevel(); });
            new UIElementBorder(altLevelUIE);
            offset += 2;

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0, offset, 3, 1);
            ui.SetText(new StringKey("val", "X_COLON", IMAGE));

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
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
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionList select = new UIWindowSelectionList(SelectClass, PUZZLE_CLASS_SELECT);
        select.AddItem(new StringKey("val", "slide"));
        select.AddItem(new StringKey("val", "code"));
        select.AddItem(new StringKey("val", "image"));
        select.AddItem(new StringKey("val", "tower"));
        select.Draw();
    }

    public void SelectClass(string className)
    {
        if (!puzzleComponent.puzzleClass.Equals(className))
        {
            puzzleComponent.imageType = "";
        }
        // the selection has the key (ie:{val:PUZZLE_SLIDE_CLASS}) so we can build the StringKey.
        puzzleComponent.puzzleClass = className;
        Update();
    }

    public void Skill()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionList select = new UIWindowSelectionList(SelectSkill, PUZZLE_SELECT_SKILL);
        select.AddItem("{will} " + EventManager.OutputSymbolReplace("{will}"));
        select.AddItem("{strength} " + EventManager.OutputSymbolReplace("{strength}"));
        select.AddItem("{agility} " + EventManager.OutputSymbolReplace("{agility}"));
        select.AddItem("{lore} " + EventManager.OutputSymbolReplace("{lore}"));
        select.AddItem("{influence} " + EventManager.OutputSymbolReplace("{influence}"));
        select.AddItem("{observation} " + EventManager.OutputSymbolReplace("{observation}"));
        select.Draw();
    }

    public void SelectSkill(string skill)
    {
        puzzleComponent.skill = skill.Substring(0, skill.IndexOf(" "));
        Update();
    }

    public void UpdateLevel()
    {
        int.TryParse(levelUIE.GetText(), out puzzleComponent.puzzleLevel);
        Update();
    }

    public void UpdateAltLevel()
    {
        int.TryParse(altLevelUIE.GetText(), out puzzleComponent.puzzleAltLevel);
        Update();
    }
    
    public void Image()
    {
        if (puzzleComponent.puzzleClass.Equals("code"))
        {
            UIWindowSelectionList selectType = new UIWindowSelectionList(SelectImage, SELECT_IMAGE.Translate());
            selectType.AddItem("{NUMBERS}", "");
            selectType.AddItem(new StringKey("val", "SYMBOL").Translate(), "symbol");
            selectType.AddItem(new StringKey("val", "ELEMENT").Translate(), "element");
            selectType.Draw();
            return;
        }

        UIWindowSelectionListImage select = new UIWindowSelectionListImage(SelectImage, SELECT_IMAGE.Translate());

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(new StringKey("val", "SOURCE").Translate(), new string[] { new StringKey("val", "FILE").Translate() });
        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;
        foreach (string s in Directory.GetFiles(relativePath, "*.png", SearchOption.AllDirectories))
        {
            select.AddItem(s.Substring(relativePath.Length + 1), traits);
        }
        foreach (string s in Directory.GetFiles(relativePath, "*.jpg", SearchOption.AllDirectories))
        {
            select.AddItem(s.Substring(relativePath.Length + 1), traits);
        }
        foreach (KeyValuePair<string, PuzzleData> kv in Game.Get().cd.puzzles)
        {
            select.AddItem(kv.Value);
        }
        select.Draw();
    }

    public void SelectImage(string image)
    {
        puzzleComponent.imageType = image;
        Update();
    }
}
