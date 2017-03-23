using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorComponentUniqueMonster : EditorComponent
{
    QuestData.UniqueMonster monsterComponent;
    DialogBoxEditable nameDBE;
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
    }

    public void SetBase()
    {
        List<string> baseMonster = new List<string>();

        Game game = Game.Get();
        baseMonster.Add("{NONE}");
        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            string display = kv.Key;
            foreach (string s in kv.Value.sets)
            {
                display += " " + s;
            }
            baseMonster.Add(display);
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
    }

    public void SetName()
    {
        monsterComponent.monsterName = "Monster Name";
    }
        /*public string imagePath = "";
        public string imagePlace = "";
        public string info = "";
        public string[] activations;
        public string[] traits;
        public int health = 0;
        public bool healthDefined = false;*/
}
