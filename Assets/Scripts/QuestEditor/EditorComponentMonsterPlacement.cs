using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorComponentMonsterPlacement : EditorComponent
{
    QuestData.Monster monsterComponent;
    EditorSelectionList monsterPlaceESL;

    public EditorComponentMonsterPlacement(string name) : base()
    {
        Game game = Game.Get();
        monsterComponent = game.quest.qd.components[name] as QuestData.Monster;
        component = monsterComponent;
        name = component.name;
        Update();
    }
    
    override public Update()
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

        int offset = 1;
        DialogBox db = null;
        for (int heroes = 2; heroes < 5; heroes++)
        {
            int h = heroes;
            db = new DialogBox(new Vector2(0, offset), new Vector2(5, 1), heroes + " Heros:");
            db.ApplyTag("editor");

            tb = new TextButton(new Vector2(19, offset++), new Vector2(1, 1), "+", delegate { MonsterPlaceAdd(h); }, Color.green);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");

            for (int i = 0; i < 8; i++)
            {
                if (monsterComponent.placement[heroes].Length > i)
                {
                    int mSlot = i;
                    string place = monsterComponent.placement[heroes][i];

                    tb = new TextButton(new Vector2(0, offset), new Vector2(1, 1), "-", delegate { MonsterPlaceRemove(h, mSlot); }, Color.red);
                    tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                    tb.ApplyTag("editor");

                    tb = new TextButton(new Vector2(1, offset), new Vector2(19, 1), place, delegate { QuestEditorData.SelectComponent(place); });
                    tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                    tb.ApplyTag("editor");
                }
                offset++;
            }
        }

        game.tokenBoard.AddHighlight(monsterComponent.location, "MonsterLoc", "editor");
    }

    public void MonsterPlaceAdd(int heroes)
    {
        Game game = Game.Get();

        List<string> mplaces = new List<string>();
        mplaces.Add("{NEW:MPlace}");
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.MPlace)
            {
                mplaces.Add(kv.Key);
            }
        }

        if (mplaces.Count == 0)
        {
            return;
        }
        monsterPlaceESL = new EditorSelectionList("Select Item", mplaces, delegate { MonsterPlaceAddSelection(heroes); });
        monsterPlaceESL.SelectItem();
    }

    public void MonsterPlaceAddSelection(int heroes)
    {
        if (monsterPlaceESL.selection.Equals("{NEW:MPlace}"))
        {
            Game game = Game.Get();
            int index = 0;

            while (game.quest.qd.components.ContainsKey("MPlace" + index))
            {
                index++;
            }
            game.quest.qd.components.Add("MPlace" + index, new QuestData.MPlace("MPlace" + index));
            monsterPlaceESL.selection = "MPlace" + index;
        }

        string[] newM = new string[monsterComponent.placement[heroes].Length + 1];
        int i;
        for (i = 0; i < monsterComponent.placement[heroes].Length; i++)
        {
            newM[i] = monsterComponent.placement[heroes][i];
        }

        newM[i] = monsterPlaceESL.selection;
        monsterComponent.placement[heroes] = newM;
        Update();
    }

    public void MonsterPlaceRemove(int heroes, int pos)
    {
        string[] newM = new string[monsterComponent.placement[heroes].Length - 1];

        int j = 0;
        for (int i = 0; i < monsterComponent.placement[heroes].Length; i++)
        {
            if (i != pos || i != j)
            {
                newM[j] = monsterComponent.placement[heroes][i];
                j++;
            }
        }
        monsterComponent.placement[heroes] = newM;
        Update();
    }
}
