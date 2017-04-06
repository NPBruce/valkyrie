using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentSpawnPlacement : EditorComponent
{
    QuestData.Spawn monsterComponent;
    EditorSelectionList monsterPlaceESL;

    public EditorComponentSpawnPlacement(string nameIn) : base()
    {
        Game game = Game.Get();
        monsterComponent = game.quest.qd.components[nameIn] as QuestData.Spawn;
        component = monsterComponent;
        name = component.sectionName;
        Update();
    }
    
    override public void Update()
    {
        base.Update();
        CameraController.SetCamera(monsterComponent.location);
        Game game = Game.Get();

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(3, 1), CommonStringKeys.SPAWN, delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 0), new Vector2(16, 1), 
        new StringKey(name.Substring("Spawn".Length),false), delegate { QuestEditorData.ListSpawn(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), CommonStringKeys.E, delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        int offset = 1;
        DialogBox db = null;
        for (int heroes = 2; heroes < 5; heroes++)
        {
            int h = heroes;
            db = new DialogBox(new Vector2(0, offset), new Vector2(5, 1), new StringKey("val", "NUMBER_HEROS", new StringKey(heroes.ToString(),false)));
            db.ApplyTag("editor");

            tb = new TextButton(new Vector2(19, offset++), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { MonsterPlaceAdd(h); }, Color.green);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");

            for (int i = 0; i < 8; i++)
            {
                if (monsterComponent.placement[heroes].Length > i)
                {
                    int mSlot = i;
                    string place = monsterComponent.placement[heroes][i];

                    tb = new TextButton(new Vector2(0, offset), new Vector2(1, 1), CommonStringKeys.MINUS, delegate { MonsterPlaceRemove(h, mSlot); }, Color.red);
                    tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                    tb.ApplyTag("editor");

                    tb = new TextButton(new Vector2(1, offset), new Vector2(19, 1), new StringKey(place,false), delegate { QuestEditorData.SelectComponent(place); });
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

        List<EditorSelectionList.SelectionListEntry> mplaces = new List<EditorSelectionList.SelectionListEntry>();
        mplaces.Add(new EditorSelectionList.SelectionListEntry("{NEW:MPlace}"));
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.MPlace)
            {
                mplaces.Add(new EditorSelectionList.SelectionListEntry(kv.Key));
            }
        }

        if (mplaces.Count == 0)
        {
            return;
        }
        monsterPlaceESL = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, mplaces, delegate { MonsterPlaceAddSelection(heroes); });
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
