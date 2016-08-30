using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestEditorData {

    public EditorSelectionList esl;

    public QuestEditorData()
    {
        SelectQuest();
    }

    public void TypeSelect()
    {
        if (GameObject.FindGameObjectWithTag("dialog") != null)
        {
            return;
        }

        DialogBox db = new DialogBox(new Vector2(21, 0), new Vector2(20, 1), "Select Type");
        db = new DialogBox(new Vector2(21, 0), new Vector2(20, 26), "");
        db.AddBorder();

        TextButton tb = new TextButton(new Vector2(21, 2), new Vector2(9, 1), "Quest", delegate { SelectQuest(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(21, 4), new Vector2(9, 1), "Tile", delegate { ListTile(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(31, 4), new Vector2(9, 1), "New", delegate { NewTile(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.0f, 0.03f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(21, 6), new Vector2(9, 1), "Door", delegate { ListDoor(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(31, 6), new Vector2(9, 1), "New", delegate { NewDoor(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.0f, 0.03f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(26.5f, 8), new Vector2(9, 1), "Cancel", delegate { Cancel(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

    }

    public void Clean()
    {
        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        // Clean up everything marked as 'questui'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("questui"))
            Object.Destroy(go);

        Game game = Game.Get();
        foreach (KeyValuePair<string, QuestData.QuestComponent> qc in game.qd.components)
        {
            qc.Value.SetVisible(true);
            qc.Value.SetVisible(.2f);
        }
    }

    public void SelectQuest()
    {
        Clean();
        Game game = Game.Get();
        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), "Quest", delegate { TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("questui");

        tb = new TextButton(new Vector2(0, 2), new Vector2(20, 1), game.qd.quest.name, delegate { Cancel(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("questui");

        tb = new TextButton(new Vector2(0, 4), new Vector2(20, 6), game.qd.quest.description, delegate { Cancel(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("questui");

        tb = new TextButton(new Vector2(0, 11), new Vector2(8, 1), ">< Min Camera", delegate { Cancel(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("questui");

        tb = new TextButton(new Vector2(0, 13), new Vector2(8, 1), ">< Max Camera", delegate { Cancel(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("questui");
    }


    public void SelectTile(string name)
    {
        Clean();
        Game game = Game.Get();
        QuestData.Tile t = game.qd.components[name] as QuestData.Tile;

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(3, 1), "Tile", delegate { TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("questui");

        tb = new TextButton(new Vector2(3, 0), new Vector2(16, 1), name.Substring("Tile".Length), delegate { ListTile(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("questui");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { Cancel(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("questui");

        tb = new TextButton(new Vector2(0, 2), new Vector2(20, 1), t.tileType.sectionName, delegate { Cancel(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("questui");

        tb = new TextButton(new Vector2(0, 4), new Vector2(8, 1), "<> Position", delegate { Cancel(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("questui");

        tb = new TextButton(new Vector2(0, 6), new Vector2(8, 1), "Rotation (" + t.rotation + ")", delegate { Cancel(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("questui");

        t.SetVisible(true);
    }

    public void ListTile()
    {
        Game game = Game.Get();

        List<string> tiles = new List<string>();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.qd.components)
        {
            if (kv.Value is QuestData.Tile)
            {
                tiles.Add(kv.Key);
            }
        }

        if (tiles.Count == 0)
        {
            return;
        }
        esl = new EditorSelectionList("Select Item", tiles, delegate { SelectComponent(); });
        esl.SelectItem();
    }

    public void NewTile()
    {

    }

    public void ListDoor()
    {
        Game game = Game.Get();

        List<string> doors = new List<string>();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.qd.components)
        {
            if (kv.Value is QuestData.Door)
            {
                doors.Add(kv.Key);
            }
        }

        if (doors.Count == 0)
        {
            return;
        }
        esl = new EditorSelectionList("Select Item", doors, delegate { SelectComponent(); });
        esl.SelectItem();
    }

    public void NewDoor()
    {

    }

    public void SelectComponent()
    {
        SelectComponent(esl.selection);
    }

    public void SelectComponent(string name)
    {
        if (name.Equals("Quest"))
        {
            SelectQuest();
            return;
        }

        Game game = Game.Get();

        if (!game.qd.components.ContainsKey(name))
        {
            Debug.Log("Error: Attempting to bring up missing component: " + name);
        }

        if (game.qd.components[name] is QuestData.Tile)
        {
            SelectTile(name);
        }
    }

    public static void Cancel()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
    }
}
