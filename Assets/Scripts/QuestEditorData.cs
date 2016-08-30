using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestEditorData {

    public EditorSelectionList esl;
    public QuestData.QuestComponent selection;
    public bool gettingPosition = false;

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

        // Clean up everything marked as 'editor'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("editor"))
            Object.Destroy(go);

        Game game = Game.Get();
        foreach (KeyValuePair<string, QuestData.QuestComponent> qc in game.qd.components)
        {
            qc.Value.SetVisible(false);
            qc.Value.Draw();
            qc.Value.SetVisible(.2f);
        }
    }

    public void SelectQuest()
    {
        Clean();
        Game game = Game.Get();
        selection = null;
        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), "Quest", delegate { TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 2), new Vector2(20, 1), game.qd.quest.name, delegate { Cancel(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 4), new Vector2(20, 6), game.qd.quest.description, delegate { Cancel(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 11), new Vector2(8, 1), ">< Min Camera", delegate { Cancel(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 13), new Vector2(8, 1), ">< Max Camera", delegate { Cancel(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");
    }


    public void SelectTile(string name)
    {
        Clean();
        Game game = Game.Get();
        QuestData.Tile t = game.qd.components[name] as QuestData.Tile;
        selection = t;

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(3, 1), "Tile", delegate { TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 0), new Vector2(16, 1), name.Substring("Tile".Length), delegate { ListTile(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { Cancel(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 2), new Vector2(20, 1), t.tileType.sectionName, delegate { ChangeTileSide(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 4), new Vector2(8, 1), ">< Position", delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 6), new Vector2(8, 1), "Rotation (" + t.rotation + ")", delegate { TileRotate(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        t.SetVisible(1f);
    }

    public void GetPosition()
    {
        gettingPosition = true;
    }

    public void MouseDown()
    {
        if (gettingPosition)
        {
            selection.location = CameraController.GetMouseTile();
            gettingPosition = false;
            SelectComponent(selection.name);
        }
    }


    public void TileRotate()
    {
        QuestData.Tile t = selection as QuestData.Tile;
        if (t.rotation == 0)
        {
            t.rotation = 90;
        }
        else if (t.rotation > 0 && t.rotation <= 100)
        {
            t.rotation = 180;
        }
        else if (t.rotation > 100 && t.rotation <= 190)
        {
            t.rotation = 270;
        }
        else
        {
            t.rotation = 0;
        }
        SelectTile(t.name);
    }

    public void ChangeTileSide()
    {
        Game game = Game.Get();

        List<string> sides = new List<string>();

        foreach (KeyValuePair<string, TileSideData> kv in game.cd.tileSides)
        {
            sides.Add(kv.Key);
        }
        esl = new EditorSelectionList("Select Tile Side", sides, delegate { SelectTileSide(); });
        esl.SelectItem();
    }

    public void SelectTileSide()
    {
        Game game = Game.Get();
        QuestData.Tile t = selection as QuestData.Tile;
        t.tileType = game.cd.tileSides[esl.selection];
        SelectTile(t.name);
    }

    public void SelectDoor(string name)
    {
        Clean();
        Game game = Game.Get();
        QuestData.Door d = game.qd.components[name] as QuestData.Door;
        selection = d;

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(3, 1), "Door", delegate { TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 0), new Vector2(16, 1), name.Substring("Door".Length), delegate { ListDoor(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { Cancel(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 2), new Vector2(8, 1), ">< Position", delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 4), new Vector2(8, 1), "Rotation (" + d.rotation + ")", delegate { DoorRotate(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 4), new Vector2(10, 1), "Color", delegate { DoorColour(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        d.SetVisible(1f);
    }

    public void DoorRotate()
    {
        QuestData.Door d = selection as QuestData.Door;
        if (d.rotation == 0)
        {
            d.rotation = 90;
        }
        else
        {
            d.rotation = 0;
        }
        SelectDoor(d.name);
    }


    public void DoorColour()
    {
        List<string> colours = new List<string>();
        foreach (KeyValuePair<string, string> kv in ColorUtil.LookUp())
        {
            colours.Add(kv.Key);
        }
        esl = new EditorSelectionList("Select Item", colours, delegate { SelectDoorColour(); });
        esl.SelectItem();
    }

    public void SelectDoorColour()
    {
        QuestData.Door d = selection as QuestData.Door;
        d.SetColor(esl.selection);
        SelectComponent(d.name);
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
        Game game = Game.Get();
        int index = 0;

        while (game.qd.components.ContainsKey("Tile" + index))
        {
            index++;
        }
        game.qd.components.Add("Tile" + index, new QuestData.Tile("Tile" + index));
        SelectComponent("Tile" + index);
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

        if (game.qd.components[name] is QuestData.Door)
        {
            SelectDoor(name);
        }
    }

    public static void Cancel()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
    }
}
