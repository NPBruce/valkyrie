using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorComponentUniqueMonster : EditorComponent
{
    QuestData.UniqueMonster monsterComponent;
    DialogBoxEditable nameDBE;
    DialogBoxEditable infoDBE;
    DialogBoxEditable healthDBE;
    EditorSelectionList baseESL;

    public EditorComponentUniqueMonster(string nameIn) : base()
    {
        Game game = Game.Get();
        monsterComponent = game.quest.qd.components[nameIn] as QuestData.UniqueMonster;
        component = monsterComponent;
        name = component.sectionName;
        Update();
    }
    
    override public void Update()
    {
        base.Update();
        Game game = Game.Get();

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(6, 1), "UniqueMonster", delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(6, 0), new Vector2(13, 1), name.Substring("UniqueMonster".Length), delegate { QuestEditorData.ListUniqueMonster(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");


        DialogBox db = new DialogBox(new Vector2(0, 2), new Vector2(3, 1), "Base:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 2), new Vector2(18, 1), monsterComponent.baseMonster, delegate { SetBase(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 4), new Vector2(3, 1), "Name:");
        db.ApplyTag("editor");
        if (monsterComponent.baseMonster.Length == 0 || monsterComponent.monsterName.Length > 0)
        {
            nameDBE = new DialogBoxEditable(new Vector2(3, 4), new Vector2(14, 1), monsterComponent.monsterName, delegate { UpdateName(); });
            nameDBE.ApplyTag("editor");
            nameDBE.AddBorder();
            if (monsterComponent.baseMonster.Length > 0)
            {
                tb = new TextButton(new Vector2(17, 4), new Vector2(3, 1), "Reset", delegate { ClearName(); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }
        else
        {
            tb = new TextButton(new Vector2(17, 4), new Vector2(3, 1), "Set", delegate { SetName(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
        }

        //string imagePath
        //string imagePlace

        db = new DialogBox(new Vector2(0, 6), new Vector2(17, 1), "Info:");
        db.ApplyTag("editor");
        if (monsterComponent.baseMonster.Length == 0 || monsterComponent.info.key.Length > 0)
        {
            infoDBE = new DialogBoxEditable(new Vector2(0, 7), new Vector2(20, 8), monsterComponent.info.Translate(), delegate { UpdateInfo(); });
            infoDBE.ApplyTag("editor");
            infoDBE.AddBorder();
            if (monsterComponent.baseMonster.Length > 0)
            {
                tb = new TextButton(new Vector2(17, 6), new Vector2(3, 1), "Reset", delegate { ClearInfo(); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }
        else
        {
            tb = new TextButton(new Vector2(17, 6), new Vector2(3, 1), "Set", delegate { SetInfo(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
        }

        //string[] activations
        //string[] traits

        db = new DialogBox(new Vector2(0, 15), new Vector2(3, 1), "Health:");
        db.ApplyTag("editor");
        if (monsterComponent.baseMonster.Length == 0 || monsterComponent.healthDefined)
        {
            healthDBE = new DialogBoxEditable(new Vector2(3, 15), new Vector2(14, 1), monsterComponent.health.ToString(), delegate { UpdateHealth(); });
            healthDBE.ApplyTag("editor");
            healthDBE.AddBorder();
            if (monsterComponent.baseMonster.Length > 0)
            {
                tb = new TextButton(new Vector2(17, 15), new Vector2(3, 1), "Reset", delegate { ClearHealth(); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }
        else
        {
            tb = new TextButton(new Vector2(17, 15), new Vector2(3, 1), "Set", delegate { SetHealth(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
        }
    }

    public void SetBase()
    {
        List<EditorSelectionList.SelectionListEntry> baseMonster = new List<EditorSelectionList.SelectionListEntry>();

        Game game = Game.Get();
        baseMonster.Add(new EditorSelectionList.SelectionListEntry("{NONE}"));
        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            string display = kv.Key;
            foreach (string s in kv.Value.sets)
            {
                display += " " + s;
            }
            baseMonster.Add(new EditorSelectionList.SelectionListEntry(display));
        }

        baseESL = new EditorSelectionList("Select Event", baseMonster, delegate { SelectSetBase(); });
        baseESL.SelectItem();
    }

    public void SelectSetBase()
    {
        if (baseESL.selection.Equals("{NONE}"))
        {
            monsterComponent.baseMonster = "";
            if (monsterComponent.monsterName.Length == 0)
            {
                SetName();
            }
            if (monsterComponent.info.key.Length == 0)
            {
                SetInfo();
            }
            if (!monsterComponent.healthDefined)
            {
                SetHealth();
            }
        }
        else
        {
            monsterComponent.baseMonster = baseESL.selection.Split(" ".ToCharArray())[0];
        }
        Update();
    }

    public void UpdateName()
    {
        if (!nameDBE.uiInput.text.Equals(""))
        {
            monsterComponent.monsterName = nameDBE.uiInput.text;
        }
    }

    public void ClearName()
    {
        monsterComponent.monsterName = "";
        Update();
    }

    public void SetName()
    {
        monsterComponent.monsterName = "Monster Name";
        Update();
    }

    public void UpdateInfo()
    {
        if (!infoDBE.uiInput.text.Equals(""))
        {
            monsterComponent.info = new Assets.Scripts.Content.StringKey(infoDBE.uiInput.text);
        }
    }

    public void ClearInfo()
    {
        monsterComponent.info = Assets.Scripts.Content.StringKey.EmptyStringKey;
        Update();
    }

    public void SetInfo()
    {
        monsterComponent.info = new Assets.Scripts.Content.StringKey("Monster Info");
        Update();
    }

    public void UpdateHealth()
    {
        int.TryParse(healthDBE.uiInput.text, out monsterComponent.health);
    }

    public void ClearHealth()
    {
        monsterComponent.health = 0;
        monsterComponent.healthDefined = false;
        Update();
    }

    public void SetHealth()
    {
        monsterComponent.health = 0;
        monsterComponent.healthDefined = true;
        Update();
    }
}
