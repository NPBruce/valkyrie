using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentMonster : EditorComponent
{
    private readonly StringKey POSITION_TYPE_UNUSED = new StringKey("val", "POSITION_TYPE_UNUSED");
    private readonly StringKey POSITION_TYPE_HIGHLIGHT = new StringKey("val", "POSITION_TYPE_HIGHLIGHT");
    private readonly StringKey MONSTER_UNIQUE = new StringKey("val", "MONSTER_UNIQUE");
    private readonly StringKey MONSTER_NORMAL = new StringKey("val", "MONSTER_NORMAL");

    private readonly StringKey UNIQUE_TITLE_DOTS = new StringKey("val", "UNIQUE_TITLE_DOTS");
    private readonly StringKey UNIQUE_INFO_DOTS = new StringKey("val", "UNIQUE_INFO_DOTS");
    private readonly StringKey TYPES_DOTS = new StringKey("val", "TYPES_DOTS");


    QuestData.Monster monsterComponent;

    DialogBoxEditable uniqueTitleDBE;
    DialogBoxEditable uniqueTextDBE;

    EditorSelectionList monsterTypeESL;
    EditorSelectionList monsterTraitESL;

    public EditorComponentMonster(string nameIn) : base()
    {
        Game game = Game.Get();
        monsterComponent = game.quest.qd.components[nameIn] as QuestData.Monster;
        component = monsterComponent;
        name = component.sectionName;
        Update();
    }
    
    override public void Update()
    {
        base.Update();
        CameraController.SetCamera(monsterComponent.location);
        Game game = Game.Get();

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), CommonStringKeys.MONSTER, delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1), new StringKey(name.Substring("Monster".Length),false), delegate { QuestEditorData.ListMonster(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), CommonStringKeys.E, delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");


        DialogBox db = new DialogBox(new Vector2(0, 2), new Vector2(4, 1), CommonStringKeys.POSITION);
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 2), new Vector2(1, 1), CommonStringKeys.POSITION_SNAP, delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(5, 2), new Vector2(1, 1), CommonStringKeys.POSITION_FREE, delegate { GetPosition(false); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        if (!monsterComponent.locationSpecified)
        {
            tb = new TextButton(new Vector2(7, 2), new Vector2(4, 1), POSITION_TYPE_UNUSED, delegate { PositionTypeCycle(); });
        }
        else
        {
            tb = new TextButton(new Vector2(7, 2), new Vector2(4, 1), POSITION_TYPE_HIGHLIGHT, delegate { PositionTypeCycle(); });
        }
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 4), new Vector2(8, 1), CommonStringKeys.EVENT , delegate { QuestEditorData.SelectAsEvent(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");
        tb = new TextButton(new Vector2(12, 4), new Vector2(8, 1), CommonStringKeys.PLACEMENT, delegate { QuestEditorData.SelectAsMonsterPlacement(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        if (monsterComponent.unique)
        {
            tb = new TextButton(new Vector2(0, 6), new Vector2(8, 1), MONSTER_UNIQUE, delegate { UniqueToggle(); });
        }
        else
        {
            tb = new TextButton(new Vector2(0, 6), new Vector2(8, 1), MONSTER_NORMAL, delegate { UniqueToggle(); });
        }
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 8), new Vector2(5, 1), UNIQUE_TITLE_DOTS);
        db.ApplyTag("editor");

        uniqueTitleDBE = new DialogBoxEditable(new Vector2(5, 8), new Vector2(15, 1), monsterComponent.uniqueTitle, delegate { UpdateUniqueTitle(); });
        uniqueTitleDBE.ApplyTag("editor");
        uniqueTitleDBE.AddBorder();

        db = new DialogBox(new Vector2(0, 10), new Vector2(20, 1), UNIQUE_INFO_DOTS);
        db.ApplyTag("editor");

        uniqueTextDBE = new DialogBoxEditable(new Vector2(0, 11), new Vector2(20, 8), monsterComponent.uniqueText, delegate { UpdateUniqueText(); });
        uniqueTextDBE.ApplyTag("editor");
        uniqueTextDBE.AddBorder();

        db = new DialogBox(new Vector2(0, 20), new Vector2(3, 1), TYPES_DOTS);
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(12, 20), new Vector2(1, 1), CommonStringKeys.PLUS , delegate { MonsterTypeAdd(0); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        for (int i = 0; i < 8; i++)
        {
            if (monsterComponent.mTypes.Length > i)
            {
                int mSlot = i;
                string mName = monsterComponent.mTypes[i];
                if (mName.IndexOf("Monster") == 0)
                {
                    mName = mName.Substring("Monster".Length);
                }

                tb = new TextButton(new Vector2(0, 21 + i), new Vector2(1, 1), CommonStringKeys.MINUS, delegate { MonsterTypeRemove(mSlot); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");

                tb = new TextButton(new Vector2(1, 21 + i), new Vector2(11, 1), new StringKey(mName,false), delegate { MonsterTypeReplace(mSlot); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");

                tb = new TextButton(new Vector2(12, 21 + i), new Vector2(1, 1), CommonStringKeys.MINUS , delegate { MonsterTypeAdd(mSlot + 1); }, Color.green);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }


        db = new DialogBox(new Vector2(14, 20), new Vector2(3, 1), CommonStringKeys.TRAITS_DOTS);
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 20), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { MonsterTraitsAdd(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        for (int i = 0; i < 8; i++)
        {
            if (monsterComponent.mTraits.Length > i)
            {
                int mSlot = i;
                string mName = monsterComponent.mTraits[i];

                tb = new TextButton(new Vector2(14, 21 + i), new Vector2(1, 1), CommonStringKeys.MINUS, delegate { MonsterTraitsRemove(mSlot); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");

                tb = new TextButton(new Vector2(15, 21 + i), new Vector2(5, 1), new StringKey(mName,false), delegate { MonsterTraitReplace(mSlot); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        game.tokenBoard.AddHighlight(monsterComponent.location, "MonsterLoc", "editor");
    }

    public void PositionTypeCycle()
    {
        monsterComponent.locationSpecified = !monsterComponent.locationSpecified;
        Update();
    }

    public void UniqueToggle()
    {
        monsterComponent.unique = !monsterComponent.unique;
        Update();
    }


    public void UpdateUniqueTitle()
    {
        monsterComponent.uniqueTitle = uniqueTitleDBE.uiInput.text;
    }

    public void UpdateUniqueText()
    {
        monsterComponent.uniqueText = uniqueTextDBE.uiInput.text;
    }

    public void MonsterTypeAdd(int pos)
    {
        Game game = Game.Get();
        List<EditorSelectionList.SelectionListEntry> monsters = new List<EditorSelectionList.SelectionListEntry>();

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.UniqueMonster)
            {
                monsters.Add(new EditorSelectionList.SelectionListEntry(kv.Key, "Quest"));
            }
        }

        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            string display = kv.Key;
            List<string> sets = new List<string>(kv.Value.traits);
            foreach (string s in kv.Value.sets)
            {
                if (s.Length == 0)
                {
                    sets.Add("base");
                }
                else
                {
                    display += " " + s;
                    sets.Add(s);
                }
            }
            monsters.Add(new EditorSelectionList.SelectionListEntry(display, sets));
        }
        monsterTypeESL = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, monsters, delegate { SelectMonsterType(pos); });
        monsterTypeESL.SelectItem();
    }

    public void MonsterTypeReplace(int pos)
    {
        Game game = Game.Get();
        List<EditorSelectionList.SelectionListEntry> monsters = new List<EditorSelectionList.SelectionListEntry>();

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.UniqueMonster)
            {
                monsters.Add(new EditorSelectionList.SelectionListEntry(kv.Key, "Quest"));
            }
        }

        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            string display = kv.Key;
            List<string> sets = new List<string>(kv.Value.traits);
            foreach (string s in kv.Value.sets)
            {
                if (s.Length == 0)
                {
                    sets.Add("base");
                }
                else
                {
                    display += " " + s;
                    sets.Add(s);
                }
            }
            monsters.Add(new EditorSelectionList.SelectionListEntry(display, sets));
        }
        monsterTypeESL = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, monsters, delegate { SelectMonsterType(pos, true); });
        monsterTypeESL.SelectItem();
    }

    public void SelectMonsterType(int pos, bool replace = false)
    {
        if (replace)
        {
            monsterComponent.mTypes[pos] = monsterTypeESL.selection.Split(" ".ToCharArray())[0];
        }
        else
        {
            string[] newM = new string[monsterComponent.mTypes.Length + 1];

            int j = 0;
            for (int i = 0; i < newM.Length; i++)
            {
                if (j == pos && i == j)
                {
                    newM[i] = monsterTypeESL.selection.Split(" ".ToCharArray())[0];
                }
                else
                {
                    newM[i] = monsterComponent.mTypes[j];
                    j++;
                }
            }
            monsterComponent.mTypes = newM;
        }
        Update();
    }

    public void MonsterTypeRemove(int pos)
    {
        if ((monsterComponent.mTypes.Length == 1) && (monsterComponent.mTraits.Length == 0))
        {
            return;
        }

        string[] newM = new string[monsterComponent.mTypes.Length - 1];

        int j = 0;
        for (int i = 0; i < monsterComponent.mTypes.Length; i++)
        {
            if (i != pos || i != j)
            {
                newM[j] = monsterComponent.mTypes[i];
                j++;
            }
        }
        monsterComponent.mTypes = newM;
        Update();
    }

    public void MonsterTraitReplace(int pos)
    {
        Game game = Game.Get();
        HashSet<string> traits = new HashSet<string>();
        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            foreach (string s in kv.Value.traits)
            {
                traits.Add(s);
            }
        }
        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        foreach (string s in traits)
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s));
        }
        monsterTraitESL = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, list, delegate { SelectMonsterTraitReplace(pos); });
        monsterTraitESL.SelectItem();
    }

    public void SelectMonsterTraitReplace(int pos)
    {
        monsterComponent.mTraits[pos] = monsterTraitESL.selection;
        Update();
    }

    public void MonsterTraitsAdd()
    {
        Game game = Game.Get();
        HashSet<string> traits = new HashSet<string>();
        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            foreach (string s in kv.Value.traits)
            {
                traits.Add(s);
            }
        }

        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        foreach (string s in traits)
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s));
        }
        monsterTraitESL = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, list, delegate { SelectMonsterTrait(); });
        monsterTraitESL.SelectItem();
    }

    public void SelectMonsterTrait()
    {
        string[] newM = new string[monsterComponent.mTraits.Length + 1];

        int i;
        for (i = 0; i < monsterComponent.mTraits.Length; i++)
        {
            newM[i] = monsterComponent.mTraits[i];
        }

        newM[i] = monsterTraitESL.selection;
        monsterComponent.mTraits = newM;
        Update();
    }

    public void MonsterTraitsRemove(int pos)
    {
        if ((monsterComponent.mTypes.Length == 0) && (monsterComponent.mTraits.Length == 1))
        {
            return;
        }

        string[] newM = new string[monsterComponent.mTraits.Length - 1];

        int j = 0;
        for (int i = 0; i < monsterComponent.mTraits.Length; i++)
        {
            if (i != pos || i != j)
            {
                newM[j] = monsterComponent.mTraits[i];
                j++;
            }
        }
        monsterComponent.mTraits = newM;
        Update();
    }
}
