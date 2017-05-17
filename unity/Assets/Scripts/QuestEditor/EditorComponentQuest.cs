using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentQuest : EditorComponent
{
    private readonly StringKey HIDDEN = new StringKey("val", "HIDDEN");
    private readonly StringKey ACTIVE = new StringKey("val", "ACTIVE");
    private readonly StringKey SELECT_PACK = new StringKey("val", "SELECT_PACK");
    private readonly StringKey REQUIRED_EXPANSIONS = new StringKey("val", "REQUIRED_EXPANSIONS");

    // When a component has editable boxes they use these, so that the value can be read
    public DialogBoxEditable nameDBE;
    public DialogBoxEditable minHeroDBE;
    public DialogBoxEditable maxHeroDBE;
    public DialogBoxEditable difficultyDBE;
    public DialogBoxEditable minLengthDBE;
    public DialogBoxEditable maxLengthDBE;
    public PaneledDialogBoxEditable descriptionDBE;
    EditorSelectionList packESL;

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

        nameDBE = new DialogBoxEditable(
            new Vector2(0.5f, offset), new Vector2(19, 1), 
            game.quest.qd.quest.name.Translate(), false, 
            delegate { UpdateQuestName(); });
        nameDBE.background.transform.parent = scrollArea.transform;
        nameDBE.ApplyTag(Game.EDITOR);
        nameDBE.AddBorder();
        offset += 2;

        DialogBox db = new DialogBox(new Vector2(0, offset), new Vector2(5, 1), new StringKey("val", "X_COLON", HIDDEN));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        TextButton tb = null;
        if (game.quest.qd.quest.hidden)
        {
            tb = new TextButton(new Vector2(5, offset), new Vector2(3, 1), new StringKey("val", "TRUE"), delegate { ToggleHidden(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.transform.parent = scrollArea.transform;
            tb.ApplyTag(Game.EDITOR);
        }
        else
        {
            tb = new TextButton(new Vector2(5, offset), new Vector2(3, 1), new StringKey("val", "FALSE"), delegate { ToggleHidden(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.transform.parent = scrollArea.transform;
            tb.ApplyTag(Game.EDITOR);
        }
        offset += 2;

        descriptionDBE = new PaneledDialogBoxEditable(
            new Vector2(0.5f, offset), new Vector2(19, 10), 
            game.quest.qd.quest.description.Translate(true),
            delegate { UpdateQuestDesc(); });
        descriptionDBE.background.transform.parent = scrollArea.transform;
        descriptionDBE.ApplyTag(Game.EDITOR);
        descriptionDBE.AddBorder();
        offset += 11;

        db = new DialogBox(new Vector2(0.5f, offset), new Vector2(10, 1), REQUIRED_EXPANSIONS);
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(10.5f, offset), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { QuestAddPack(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        offset += 1;
        int index;
        for (index = 0; index < game.quest.qd.quest.packs.Length; index++)
        {
            int i = index;
            db = new DialogBox(new Vector2(0.5f, offset), new Vector2(10, 1), 
                new StringKey("val", game.quest.qd.quest.packs[index]));
            db.AddBorder();
            db.background.transform.parent = scrollArea.transform;
            db.ApplyTag(Game.EDITOR);
            tb = new TextButton(new Vector2(10.5f, offset), new Vector2(1, 1),
                CommonStringKeys.MINUS, delegate { QuestRemovePack(i); }, Color.red);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.transform.parent = scrollArea.transform;
            tb.ApplyTag(Game.EDITOR);
            offset += 1;
        }
        offset += 1;

        db = new DialogBox(new Vector2(0, offset), new Vector2(7.5f, 1), new StringKey("Val", "X_COLON", new StringKey("Val", "MIN_X", game.gameType.HeroesName())));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);
        
        minHeroDBE = new DialogBoxEditable(
            new Vector2(7.5f, offset), new Vector2(2, 1), 
            game.quest.qd.quest.minHero.ToString(), false, 
            delegate { UpdateMinHero(); });
        minHeroDBE.background.transform.parent = scrollArea.transform;
        minHeroDBE.ApplyTag(Game.EDITOR);
        minHeroDBE.AddBorder();

        db = new DialogBox(new Vector2(9.5f, offset), new Vector2(7.5f, 1), new StringKey("Val", "X_COLON", new StringKey("Val", "MAX_X", game.gameType.HeroesName())));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);
        
        maxHeroDBE = new DialogBoxEditable(
            new Vector2(17, offset), new Vector2(2, 1), 
            game.quest.qd.quest.maxHero.ToString(), false, 
            delegate { UpdateMaxHero(); });
        maxHeroDBE.background.transform.parent = scrollArea.transform;
        maxHeroDBE.ApplyTag(Game.EDITOR);
        maxHeroDBE.AddBorder();
        offset +=2;

        db = new DialogBox(new Vector2(0, offset), new Vector2(7.5f, 1),  new StringKey("Val", "X_COLON", new StringKey("Val", "MIN_X", new StringKey("val", "DURATION"))));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);
        
        minLengthDBE = new DialogBoxEditable(
            new Vector2(7.5f, offset), new Vector2(2, 1), 
            game.quest.qd.quest.lengthMin.ToString(), false, 
            delegate { UpdateMinLength(); });
        minLengthDBE.background.transform.parent = scrollArea.transform;
        minLengthDBE.ApplyTag(Game.EDITOR);
        minLengthDBE.AddBorder();

        db = new DialogBox(new Vector2(9.5f, offset), new Vector2(7.5f, 1),  new StringKey("Val", "X_COLON", new StringKey("Val", "MAX_X", new StringKey("val", "DURATION"))));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);
        
        maxLengthDBE = new DialogBoxEditable(
            new Vector2(17f, offset), new Vector2(2, 1), 
            game.quest.qd.quest.lengthMax.ToString(), false, 
            delegate { UpdateMaxLength(); });
        maxLengthDBE.background.transform.parent = scrollArea.transform;
        maxLengthDBE.ApplyTag(Game.EDITOR);
        maxLengthDBE.AddBorder();
        offset +=2;

        db = new DialogBox(new Vector2(9.5f, offset), new Vector2(7.5f, 1),  new StringKey("Val", "X_COLON", new StringKey("Val", "DIFFICULTY")));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);
        
        difficultyDBE = new DialogBoxEditable(
            new Vector2(17f, offset), new Vector2(3, 1), 
            game.quest.qd.quest.difficulty.ToString(), false, 
            delegate { UpdateDifficulty(); });
        difficultyDBE.background.transform.parent = scrollArea.transform;
        difficultyDBE.ApplyTag(Game.EDITOR);
        difficultyDBE.AddBorder();
        offset +=2;

        return offset;
    }

    override public float DrawComponentSelection(float offset)
    {
        // Back button
        TextButton tb = new TextButton(new Vector2(0, offset), new Vector2(4, 1), CommonStringKeys.BACK, delegate { QuestEditorData.Back(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(4, offset), new Vector2(6, 1), new StringKey("val", "COMPONENTS"), delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        DialogBox db = new DialogBox(new Vector2(10, offset), new Vector2(10, 1), new StringKey("val", "X_COLON", CommonStringKeys.QUEST));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        return offset + 2;
    }

    override public float AddComment(float offset)
    {
        return offset;
    }

    override public float AddSource(float offset)
    {
        return offset;
    }

    public void UpdateQuestName()
    {
        if (nameDBE.CheckTextChangedAndNotEmpty())
        {
            LocalizationRead.updateScenarioText(game.quest.qd.quest.name_key, nameDBE.Text);
        }
    }

    public void UpdateQuestDesc()
    {
        if (descriptionDBE.CheckTextChangedAndNotEmpty())
        {
            LocalizationRead.updateScenarioText(game.quest.qd.quest.description_key, descriptionDBE.Text);
        }
        else if (descriptionDBE.CheckTextEmptied())
        {
            LocalizationRead.scenarioDict.Remove(game.quest.qd.quest.description_key);
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
        int.TryParse(minHeroDBE.Text, out game.quest.qd.quest.minHero);
        if (game.quest.qd.quest.minHero < 1)
        {
            game.quest.qd.quest.minHero = 1;
        }
        Update();
    }

    public void UpdateMaxHero()
    {
        int.TryParse(maxHeroDBE.Text, out game.quest.qd.quest.maxHero);
        if (game.quest.qd.quest.maxHero > game.gameType.MaxHeroes())
        {
            game.quest.qd.quest.maxHero = game.gameType.MaxHeroes();
        }
        Update();
    }

    public void UpdateMinLength()
    {
        int.TryParse(minLengthDBE.Text, out game.quest.qd.quest.lengthMin);
        Update();
    }

    public void UpdateMaxLength()
    {
        int.TryParse(maxLengthDBE.Text, out game.quest.qd.quest.lengthMax);
        Update();
    }

    public void UpdateDifficulty()
    {
        float.TryParse(difficultyDBE.Text, out game.quest.qd.quest.difficulty);
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
