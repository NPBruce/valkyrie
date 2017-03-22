using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorComponentMonster : EditorComponent
{
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
        name = component.name;
        Update();
    }
    
    override public void Update()
    {
        base.Update();
        CameraController.SetCamera(monsterComponent.location);
        Game game = Game.Get();

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), "Monster", delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1), name.Substring("Monster".Length), delegate { QuestEditorData.ListMonster(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");


        DialogBox db = new DialogBox(new Vector2(0, 2), new Vector2(4, 1), "Position");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 2), new Vector2(1, 1), "><", delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(5, 2), new Vector2(1, 1), "~", delegate { GetPosition(false); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        if (!eventComponent.GetType().IsSubclassOf(typeof(QuestData.Event)))
        {
            if (!eventComponent.locationSpecified)
            {
                tb = new TextButton(new Vector2(7, 2), new Vector2(4, 1), "Unused", delegate { PositionTypeCycle(); });
            }
            else
            {
                tb = new TextButton(new Vector2(7, 2), new Vector2(4, 1), "Highlight", delegate { PositionTypeCycle(); });
            }
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
        }

        tb = new TextButton(new Vector2(0, 4), new Vector2(8, 1), "Event", delegate { QuestEditorData.SelectAsEvent(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");
        tb = new TextButton(new Vector2(12, 4), new Vector2(8, 1), "Placement", delegate { QuestEditorData.SelectAsMonsterPlacement(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        if (monsterComponent.unique)
        {
            tb = new TextButton(new Vector2(0, 6), new Vector2(8, 1), "Unique", delegate { UniqueToggle(); });
        }
        else
        {
            tb = new TextButton(new Vector2(0, 6), new Vector2(8, 1), "Normal", delegate { UniqueToggle(); });
        }
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 8), new Vector2(5, 1), "Unique Title:");
        db.ApplyTag("editor");

        uniqueTitleDBE = new DialogBoxEditable(new Vector2(5, 8), new Vector2(15, 1), monsterComponent.uniqueTitle, delegate { UpdateUniqueTitle(); });
        uniqueTitleDBE.ApplyTag("editor");
        uniqueTitleDBE.AddBorder();

        db = new DialogBox(new Vector2(0, 10), new Vector2(20, 1), "Unique Information:");
        db.ApplyTag("editor");

        uniqueTextDBE = new DialogBoxEditable(new Vector2(0, 11), new Vector2(20, 8), monsterComponent.uniqueText, delegate { UpdateUniqueText(); });
        uniqueTextDBE.ApplyTag("editor");
        uniqueTextDBE.AddBorder();

        db = new DialogBox(new Vector2(0, 20), new Vector2(3, 1), "Types:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(12, 20), new Vector2(1, 1), "+", delegate { MonsterTypeAdd(0); }, Color.green);
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

                tb = new TextButton(new Vector2(0, 21 + i), new Vector2(1, 1), "-", delegate { MonsterTypeRemove(mSlot); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");

                tb = new TextButton(new Vector2(1, 21 + i), new Vector2(11, 1), mName, delegate { MonsterTypeReplace(mSlot); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");

                tb = new TextButton(new Vector2(12, 21 + i), new Vector2(1, 1), "+", delegate { MonsterTypeAdd(mSlot + 1); }, Color.green);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }


        db = new DialogBox(new Vector2(14, 20), new Vector2(3, 1), "Traits:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 20), new Vector2(1, 1), "+", delegate { MonsterTraitsAdd(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        for (int i = 0; i < 8; i++)
        {
            if (monsterComponent.mTraits.Length > i)
            {
                int mSlot = i;
                string mName = monsterComponent.mTraits[i];

                tb = new TextButton(new Vector2(14, 21 + i), new Vector2(1, 1), "-", delegate { MonsterTraitsRemove(mSlot); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");

                tb = new TextButton(new Vector2(15, 21 + i), new Vector2(5, 1), mName, delegate { MonsterTraitReplace(mSlot); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        game.tokenBoard.AddHighlight(monsterComponent.location, "MonsterLoc", "editor");
    }

    public void PositionTypeCycle()
    {
        eventComponent.locationSpecified = !eventComponent.locationSpecified;
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
        List<string> monsters = new List<string>();
        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            string display = kv.Key;
            foreach (string s in kv.Value.sets)
            {
                display += " " + s;
            }
            monsters.Add(display);
        }
        monsterTypeESL = new EditorSelectionList("Select Item", monsters, delegate { SelectMonsterType(pos); });
        monsterTypeESL.SelectItem();
    }

    public void MonsterTypeReplace(int pos)
    {
        Game game = Game.Get();
        List<string> monsters = new List<string>();
        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            monsters.Add(kv.Key);
        }
        monsterTypeESL = new EditorSelectionList("Select Item", monsters, delegate { SelectMonsterType(pos, true); });
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
        List<string> list = new List<string>(traits);
        monsterTraitESL = new EditorSelectionList("Select Item", list, delegate { SelectMonsterTraitReplace(pos); });
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
        List<string> list = new List<string>(traits);
        monsterTraitESL = new EditorSelectionList("Select Item", list, delegate { SelectMonsterTrait(); });
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
