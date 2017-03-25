using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorComponentQuest : EditorComponent
{
    // When a component has editable boxes they use these, so that the value can be read
    public DialogBoxEditable dbe1;
    public DialogBoxEditable dbe2;
    // ...unless the component uses the list here instead
    public List<DialogBoxEditable> dbeList;

    EditorSelectionList packESL;

    // Quest is a special component with meta data
    public EditorComponentQuest()
    {
        component = null;
        name = "";
        Update();
    }

    override public void Update()
    {
        base.Update();
        Game game = Game.Get();
        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), "Quest", delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        dbe1 = new DialogBoxEditable(new Vector2(0, 2), new Vector2(20, 1), game.quest.qd.quest.name, delegate { UpdateQuestName(); });
        dbe1.ApplyTag("editor");
        dbe1.AddBorder();

        dbe2 = new DialogBoxEditable(new Vector2(0, 4), new Vector2(20, 6), game.quest.qd.quest.description, delegate { UpdateQuestDesc(); });
        dbe2.ApplyTag("editor");
        dbe2.AddBorder();

        DialogBox db = new DialogBox(new Vector2(0, 11), new Vector2(8, 1), "Minor Peril Level:");
        db.ApplyTag("editor");

        dbeList = new List<DialogBoxEditable>();
        DialogBoxEditable dbeTmp = new DialogBoxEditable(new Vector2(8, 11), new Vector2(3, 1), game.quest.qd.quest.minorPeril.ToString(), delegate { UpdatePeril(0); });
        dbeTmp.ApplyTag("editor");
        dbeTmp.AddBorder();
        dbeList.Add(dbeTmp);

        db = new DialogBox(new Vector2(0, 12), new Vector2(8, 1), "Major Peril Level:");
        db.ApplyTag("editor");

        dbeTmp = new DialogBoxEditable(new Vector2(8, 12), new Vector2(3, 1), game.quest.qd.quest.majorPeril.ToString(), delegate { UpdatePeril(1); });
        dbeTmp.ApplyTag("editor");
        dbeTmp.AddBorder();
        dbeList.Add(dbeTmp);

        db = new DialogBox(new Vector2(0, 13), new Vector2(8, 1), "Deadly Peril Level:");
        db.ApplyTag("editor");

        dbeTmp = new DialogBoxEditable(new Vector2(8, 13), new Vector2(3, 1), game.quest.qd.quest.deadlyPeril.ToString(), delegate { UpdatePeril(2); });
        dbeTmp.ApplyTag("editor");
        dbeTmp.AddBorder();
        dbeList.Add(dbeTmp);

        db = new DialogBox(new Vector2(0, 15), new Vector2(9, 1), "Required Expansions:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(9, 15), new Vector2(1, 1), "+", delegate { QuestAddPack(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        int offset = 16;
        int index;
        for (index = 0; index < 8; index++)
        {
            if (game.quest.qd.quest.packs.Length > index)
            {
                int i = index;
                db = new DialogBox(new Vector2(0, offset), new Vector2(9, 1), game.quest.qd.quest.packs[index]);
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(9, offset++), new Vector2(1, 1), "-", delegate { QuestRemovePack(i); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }
    }

    public void UpdateQuestName()
    {
        Game game = Game.Get();

        if (!dbe1.uiInput.text.Equals(""))
        {
            // TODO: Me must, search quest dictionary for the key and change its
            // translation to the current language
            game.quest.qd.quest.name = dbe1.uiInput.text;
        }
    }

    public void UpdateQuestDesc()
    {
        Game game = Game.Get();

        if (!dbe2.uiInput.text.Equals(""))
            game.quest.qd.quest.description = dbe2.uiInput.text;
    }

    public void UpdatePeril(int level)
    {
        if (level == 0)
        {
            int.TryParse(dbeList[level].uiInput.text, out Game.Get().quest.qd.quest.minorPeril);
        }
        if (level == 1)
        {
            int.TryParse(dbeList[level].uiInput.text, out Game.Get().quest.qd.quest.majorPeril);
        }
        if (level == 2)
        {
            int.TryParse(dbeList[level].uiInput.text, out Game.Get().quest.qd.quest.deadlyPeril);
        }
        Update();
    }

    public void QuestAddPack()
    {
        List<EditorSelectionList.SelectionListEntry> packs = new List<EditorSelectionList.SelectionListEntry>();

        foreach (ContentData.ContentPack pack in Game.Get().cd.allPacks)
        {
            if (pack.id.Length > 0)
            {
                packs.Add(new EditorSelectionList.SelectionListEntry(pack.id));
            }
        }

        packESL = new EditorSelectionList("Select Pack", packs, delegate { SelectQuestAddPack(); });
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
