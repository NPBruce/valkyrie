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
            new Vector2(0, offset), new Vector2(20, 1), 
            game.quest.qd.quest.name.Translate(), false, 
            delegate { UpdateQuestName(); });
        nameDBE.background.transform.parent = scrollArea.transform;
        nameDBE.ApplyTag(Game.EDITOR);
        nameDBE.AddBorder();

        offset += 2;

        descriptionDBE = new PaneledDialogBoxEditable(
            new Vector2(0, offset), new Vector2(20, 6), 
            game.quest.qd.quest.description.Translate(true),
            delegate { UpdateQuestDesc(); });
        descriptionDBE.background.transform.parent = scrollArea.transform;
        descriptionDBE.ApplyTag(Game.EDITOR);
        descriptionDBE.AddBorder();

        offset += 7;

        if (game.quest.qd.quest.hidden)
        {
            tb = new TextButton(new Vector2(0, offset), new Vector2(8, 1), HIDDEN, delegate { ToggleHidden(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.transform.parent = scrollArea.transform;
            tb.ApplyTag(Game.EDITOR);
        }
        else
        {
            tb = new TextButton(new Vector2(0, offset), new Vector2(8, 1), ACTIVE, delegate { ToggleHidden(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.transform.parent = scrollArea.transform;
            tb.ApplyTag(Game.EDITOR);
        }

        offset += 2;

        DialogBox db = new DialogBox(new Vector2(0, offset), new Vector2(9, 1), REQUIRED_EXPANSIONS);
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(9, offset), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { QuestAddPack(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        offset += 1;
        int index;
        for (index = 0; index < 15; index++)
        {
            if (game.quest.qd.quest.packs.Length > index)
            {
                int i = index;
                db = new DialogBox(new Vector2(0, offset), new Vector2(9, 1), 
                    new StringKey("val", game.quest.qd.quest.packs[index]));
                db.AddBorder();
                db.background.transform.parent = scrollArea.transform;
                db.ApplyTag(Game.EDITOR);
                tb = new TextButton(new Vector2(9, offset), new Vector2(1, 1),
                    CommonStringKeys.MINUS, delegate { QuestRemovePack(i); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.background.transform.parent = scrollArea.transform;
                tb.ApplyTag(Game.EDITOR);
                offset += 1;
            }
        }
        offset += 1;

        return offset;
    }

    override public float DrawComponentSelection(float offset)
    {
        // Back button
        TextButton tb = new TextButton(new Vector2(0, offset), new Vector2(5, 1), CommonStringKeys.BACK, delegate { QuestEditorData.Back(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        offset += 2;

        tb = new TextButton(new Vector2(0, offset), new Vector2(6, 1), 
            CommonStringKeys.QUEST, delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        return offset + 2;
    }

    public void UpdateQuestName()
    {
        Game game = Game.Get();

        if (nameDBE.CheckTextChangedAndNotEmpty())
        {
            LocalizationRead.updateScenarioText(game.quest.qd.quest.name_key, nameDBE.Text);
        }
    }

    public void UpdateQuestDesc()
    {
        Game game = Game.Get();

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
        Game game = Game.Get();
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
        Game game = Game.Get();
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
        Game game = Game.Get();
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
}
