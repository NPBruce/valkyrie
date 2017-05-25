using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
using System.IO;

public class EditorComponentQuest : EditorComponent
{
    private readonly StringKey HIDDEN = new StringKey("val", "HIDDEN");
    private readonly StringKey ACTIVE = new StringKey("val", "ACTIVE");
    private readonly StringKey SELECT_PACK = new StringKey("val", "SELECT_PACK");
    private readonly StringKey REQUIRED_EXPANSIONS = new StringKey("val", "REQUIRED_EXPANSIONS");

    // When a component has editable boxes they use these, so that the value can be read
    public UIElementEditable nameUIE;
    public UIElementEditable minHeroUIE;
    public UIElementEditable maxHeroUIE;
    public UIElementEditable difficultyUIE;
    public UIElementEditable minLengthUIE;
    public UIElementEditable maxLengthUIE;
    public UIElementEditablePaneled descriptionUIE;
    public UIElementEditablePaneled authorsUIE;

    EditorSelectionList packESL;
    EditorSelectionList imageESL;

    // Quest is a special component with meta data
    public EditorComponentQuest()
    {
        component = null;
        name = "";
        Update();
    }

    override public float AddSubComponents(float offset)
    {
        Game game = Game.Get();

        nameUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
        nameUIE.SetLocation(0.5f, offset, 19, 1);
        nameUIE.SetText(game.quest.qd.quest.name.Translate());
        nameUIE.SetButton(delegate { UpdateQuestName(); });
        nameUIE.SetSingleLine();
        new UIElementBorder(nameUIE);
        offset += 2;

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 5, 1);
        ui.SetText(new StringKey("val", "X_COLON", HIDDEN));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(5, offset, 3, 1);
        ui.SetButton(delegate { ToggleHidden(); });
        new UIElementBorder(ui);
        if (game.quest.qd.quest.hidden)
        {
            ui.SetText(new StringKey("val", "TRUE"));
        }
        else
        {
            ui.SetText(new StringKey("val", "FALSE"));
        }
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 5, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "IMAGE")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(5, offset, 12, 1);
        ui.SetButton(delegate { Image(); });
        ui.SetText(game.quest.qd.quest.image);
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset++, 8, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "DESCRIPTION")));

        descriptionUIE = new UIElementEditablePaneled(Game.EDITOR, scrollArea.GetScrollTransform());
        descriptionUIE.SetLocation(0.5f, offset, 19, 10);
        descriptionUIE.SetText(game.quest.qd.quest.description.Translate(true));
        descriptionUIE.SetButton(delegate { UpdateQuestDesc(); });
        new UIElementBorder(descriptionUIE);
        offset += 11;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset++, 8, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "AUTHORS")));


        authorsUIE = new UIElementEditablePaneled(Game.EDITOR, scrollArea.GetScrollTransform());
        authorsUIE.SetLocation(0.5f, offset, 19, 6);
        authorsUIE.SetText(game.quest.qd.quest.authors.Translate(true));
        authorsUIE.SetButton(delegate { UpdateQuestAuth(); });
        new UIElementBorder(authorsUIE);
        offset += 7;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 10, 1);
        ui.SetText(REQUIRED_EXPANSIONS);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(10.5f, offset, 1, 1);
        ui.SetButton(delegate { QuestAddPack(); });
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        new UIElementBorder(ui, Color.green);

        offset += 1;
        int index;
        for (index = 0; index < game.quest.qd.quest.packs.Length; index++)
        {
            int i = index;
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0.5f, offset, 10, 1);
            ui.SetText(new StringKey("val", game.quest.qd.quest.packs[index]));
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(10.5f, offset, 1, 1);
            ui.SetButton(delegate { QuestRemovePack(i); });
            ui.SetText(CommonStringKeys.MINUS, Color.red);
            new UIElementBorder(ui, Color.red);
            offset += 1;
        }
        offset += 1;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 7.5f, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "MIN_X", game.gameType.HeroesName())));

        minHeroUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
        minHeroUIE.SetLocation(7.5f, offset, 2, 1);
        minHeroUIE.SetText(game.quest.qd.quest.minHero.ToString());
        minHeroUIE.SetSingleLine();
        minHeroUIE.SetButton(delegate { UpdateMinHero(); });
        new UIElementBorder(minHeroUIE);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(9.5f, offset, 7.5f, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "MAX_X", game.gameType.HeroesName())));

        maxHeroUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
        maxHeroUIE.SetLocation(17, offset, 2, 1);
        maxHeroUIE.SetText(game.quest.qd.quest.maxHero.ToString());
        maxHeroUIE.SetSingleLine();
        maxHeroUIE.SetButton(delegate { UpdateMaxHero(); });
        new UIElementBorder(maxHeroUIE);
        offset +=2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 7.5f, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "MIN_X", new StringKey("val", "DURATION"))));

        minLengthUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
        minLengthUIE.SetLocation(7.5f, offset, 2, 1);
        minLengthUIE.SetText(game.quest.qd.quest.lengthMin.ToString());
        minLengthUIE.SetSingleLine();
        minLengthUIE.SetButton(delegate { UpdateMinLength(); });
        new UIElementBorder(minLengthUIE);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(9.5f, offset, 7.5f, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "MAX_X", new StringKey("val", "DURATION"))));

        maxLengthUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
        maxLengthUIE.SetLocation(17, offset, 2, 1);
        maxLengthUIE.SetText(game.quest.qd.quest.lengthMax.ToString());
        maxLengthUIE.SetSingleLine();
        maxLengthUIE.SetButton(delegate { UpdateMaxLength(); });
        new UIElementBorder(maxLengthUIE);
        offset +=2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 7.5f, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "DIFFICULTY")));

        difficultyUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
        difficultyUIE.SetLocation(7.5f, offset, 3, 1);
        difficultyUIE.SetText(game.quest.qd.quest.difficulty.ToString());
        difficultyUIE.SetSingleLine();
        difficultyUIE.SetButton(delegate { UpdateDifficulty(); });
        new UIElementBorder(difficultyUIE);
        offset +=2;

        return offset;
    }

    override public float DrawComponentSelection(float offset)
    {
        return offset + 1;
    }

    override public float AddComment(float offset)
    {
        return offset;
    }

    override public float AddSource(float offset)
    {
        return offset;
    }

    protected override void AddTitle()
    {
        UIElement ui = new UIElement(Game.EDITOR);
        ui.SetLocation(4, 0, 17, 1);
        ui.SetText(game.gameType.QuestName());
        ui.SetButton(delegate { QuestEditorData.TypeSelect(); });
        ui.SetBGColor(Color.black);
        new UIElementBorder(ui);
    }

    public void UpdateQuestName()
    {
        if (!nameUIE.Empty() && nameUIE.Changed())
        {
            LocalizationRead.updateScenarioText(game.quest.qd.quest.name_key, nameUIE.GetText());
        }
    }

    public void Image()
    {
        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        list.Add(new EditorSelectionList.SelectionListEntry(""));

        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;
        foreach (string s in Directory.GetFiles(relativePath, "*.png", SearchOption.AllDirectories))
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s.Substring(relativePath.Length + 1), "File"));
        }
        foreach (string s in Directory.GetFiles(relativePath, "*.jpg", SearchOption.AllDirectories))
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s.Substring(relativePath.Length + 1), "File"));
        }
        imageESL = new EditorSelectionList(new StringKey("val","SELECT_IMAGE"), list, delegate { SelectImage(); });
        imageESL.SelectItem();
    }

    public void SelectImage()
    {
        game.quest.qd.quest.image = imageESL.selection;
        Update();
    }

    public void UpdateQuestDesc()
    {
        if (descriptionUIE.Changed())
        {
            if (descriptionUIE.Empty())
            {
                LocalizationRead.scenarioDict.Remove(game.quest.qd.quest.description_key);
            }
            else
            {
                LocalizationRead.updateScenarioText(game.quest.qd.quest.description_key, descriptionUIE.GetText());
            }
        }
    }

    public void UpdateQuestAuth()
    {
        if (authorsUIE.Changed())
        {
            if (authorsUIE.Empty())
            {
                LocalizationRead.scenarioDict.Remove(game.quest.qd.quest.authors_key);
            }
            else
            {
                LocalizationRead.updateScenarioText(game.quest.qd.quest.authors_key, authorsUIE.GetText());
            }
        }
    }

    public void ToggleHidden()
    {
        game.quest.qd.quest.hidden = !game.quest.qd.quest.hidden;
        Update();
    }

    public void QuestAddPack()
    {
        List<EditorSelectionList.SelectionListEntry> packs = new List<EditorSelectionList.SelectionListEntry>();

        foreach (ContentData.ContentPack pack in Game.Get().cd.allPacks)
        {
            if (pack.id.Length > 0)
            {
                packs.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(pack.id));
            }
        }

        packESL = new EditorSelectionList(SELECT_PACK, packs, delegate { SelectQuestAddPack(); });
        packESL.SelectItem();
    }

    public void SelectQuestAddPack()
    {
        string[] packs = new string[game.quest.qd.quest.packs.Length + 1];
        int i;
        for (i = 0; i < game.quest.qd.quest.packs.Length; i++)
        {
            packs[i] = game.quest.qd.quest.packs[i];
        }
        packs[i] = (packESL.selection);
        game.quest.qd.quest.packs = packs;
        Update();
    }

    public void QuestRemovePack(int index)
    {
        string[] packs = new string[game.quest.qd.quest.packs.Length - 1];

        int j = 0;
        for (int i = 0; i < game.quest.qd.quest.packs.Length; i++)
        {
            if (i != index || i != j)
            {
                packs[j] = game.quest.qd.quest.packs[i];
                j++;
            }
        }
        game.quest.qd.quest.packs = packs;
        Update();
    }

    public void UpdateMinHero()
    {
        int.TryParse(minHeroUIE.GetText(), out game.quest.qd.quest.minHero);
        if (game.quest.qd.quest.minHero < 1)
        {
            game.quest.qd.quest.minHero = 1;
        }
        Update();
    }

    public void UpdateMaxHero()
    {
        int.TryParse(maxHeroUIE.GetText(), out game.quest.qd.quest.maxHero);
        if (game.quest.qd.quest.maxHero > game.gameType.MaxHeroes())
        {
            game.quest.qd.quest.maxHero = game.gameType.MaxHeroes();
        }
        Update();
    }

    public void UpdateMinLength()
    {
        int.TryParse(minLengthUIE.GetText(), out game.quest.qd.quest.lengthMin);
        Update();
    }

    public void UpdateMaxLength()
    {
        int.TryParse(maxLengthUIE.GetText(), out game.quest.qd.quest.lengthMax);
        Update();
    }

    public void UpdateDifficulty()
    {
        float.TryParse(difficultyUIE.GetText(), out game.quest.qd.quest.difficulty);
        if (game.quest.qd.quest.difficulty > 1)
        {
            game.quest.qd.quest.difficulty = 1;
        }
        if (game.quest.qd.quest.difficulty < 0)
        {
            game.quest.qd.quest.difficulty = 0;
        }
        Update();
    }
}
