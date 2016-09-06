using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestEditorData {

    public EditorSelectionList esl;
    public QuestData.QuestComponent selection;
    public Stack<QuestData.QuestComponent> selectionStack;
    public QuestEditorTextEdit te;
    public bool gettingPosition = false;
    public bool gettingMinPosition = false;
    public bool gettingMaxPosition = false;
    public bool backTriggered = false;
    public DialogBoxEditable dbe1;
    public DialogBoxEditable dbe2;

    public void NewSelection(QuestData.QuestComponent c)
    {
        if (!backTriggered)
        {
            if (selection != c)
            {
                selectionStack.Push(selection);
            }
        }
        backTriggered = false;
        selection = c;
    }

    public QuestEditorData()
    {
        selectionStack = new Stack<QuestData.QuestComponent>();
        SelectQuest();
    }

    public void TypeSelect()
    {
        if (GameObject.FindGameObjectWithTag("dialog") != null)
        {
            return;
        }

        DialogBox db = new DialogBox(new Vector2(21, 0), new Vector2(18, 18), "");
        db.AddBorder();

        db = new DialogBox(new Vector2(21, 0), new Vector2(17, 1), "Select Type");

        TextButton tb = new TextButton(new Vector2(22, 2), new Vector2(9, 1), "Quest", delegate { SelectQuest(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(22, 4), new Vector2(9, 1), "Tile", delegate { ListTile(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(32, 4), new Vector2(6, 1), "Delete", delegate { DeleteComponent("Tile"); }, Color.red);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(22, 6), new Vector2(9, 1), "Door", delegate { ListDoor(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(32, 6), new Vector2(6, 1), "Delete", delegate { DeleteComponent("Door"); }, Color.red);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(22, 8), new Vector2(9, 1), "Token", delegate { ListToken(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(32, 8), new Vector2(6, 1), "Delete", delegate { DeleteComponent("Token"); }, Color.red);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(22, 10), new Vector2(9, 1), "Monster", delegate { ListMonster(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(32, 10), new Vector2(6, 1), "Delete", delegate { DeleteComponent("Monster"); }, Color.red);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(22, 12), new Vector2(9, 1), "MPlace", delegate { ListMPlace(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(32, 12), new Vector2(6, 1), "Delete", delegate { DeleteComponent("MPlace"); }, Color.red);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(22, 14), new Vector2(9, 1), "Event", delegate { ListEvent(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(32, 14), new Vector2(6, 1), "Delete", delegate { DeleteComponent("Event"); }, Color.red);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(25.5f, 16), new Vector2(9, 1), "Cancel", delegate { Cancel(); });
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

        // Clean up everything marked as 'board'
        // This means we have to reload everything, but otherwise we end up with ghost objects.  This solution is good enough
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("board"))
            Object.Destroy(go);

        Game game = Game.Get();
        foreach (KeyValuePair<string, QuestData.QuestComponent> qc in game.qd.components)
        {
            qc.Value.Draw();
            qc.Value.SetVisible(.2f);
        }
    }

    public void Back()
    {
        if (selectionStack.Count == 0)
        {
            return;
        }
        QuestData.QuestComponent qc = selectionStack.Pop();
        backTriggered = true;
        if (qc == null)
        {
            SelectQuest();
        }
        else
        {
            SelectComponent(qc.name);
        }
    }

    public void SelectQuest()
    {
        Clean();
        Game game = Game.Get();
        NewSelection(null);
        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), "Quest", delegate { TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        dbe1 = new DialogBoxEditable(new Vector2(0, 2), new Vector2(20, 1), game.qd.quest.name, delegate { UpdateQuestName(); });
        dbe1.ApplyTag("editor");
        dbe1.AddBorder();

        dbe2 = new DialogBoxEditable(new Vector2(0, 4), new Vector2(20, 6), game.qd.quest.description, delegate { UpdateQuestDesc(); });
        dbe2.ApplyTag("editor");
        dbe2.AddBorder();

        tb = new TextButton(new Vector2(0, 11), new Vector2(8, 1), ">< Min Camera", delegate { GetMinPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 13), new Vector2(8, 1), ">< Max Camera", delegate { GetMaxPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 29), new Vector2(3, 1), "Back", delegate { Back(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        game.tokenBoard.AddHighlight(new Vector2(game.qd.quest.minPanX, game.qd.quest.minPanY), "CamMin", "editor");
        game.tokenBoard.AddHighlight(new Vector2(game.qd.quest.maxPanX, game.qd.quest.maxPanY), "CamMax", "editor");
    }

    public void UpdateQuestName()
    {
        Game game = Game.Get();

        if (!dbe1.uiInput.text.Equals(""))
            game.qd.quest.name = dbe1.uiInput.text;
    }

    public void UpdateQuestDesc()
    {
        Game game = Game.Get();

        if (!dbe2.uiInput.text.Equals(""))
            game.qd.quest.description = dbe2.uiInput.text;
    }

    public void SelectTile(string name)
    {
        Clean();
        Game game = Game.Get();
        QuestData.Tile t = game.qd.components[name] as QuestData.Tile;
        NewSelection(t);
        CameraController.SetCamera(t.location);

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(3, 1), "Tile", delegate { TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 0), new Vector2(16, 1), name.Substring("Tile".Length), delegate { ListTile(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { RenameComponent(); });
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

        tb = new TextButton(new Vector2(0, 29), new Vector2(3, 1), "Back", delegate { Back(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        game.tokenBoard.AddHighlight(t.location, "TileAnchor", "editor");

        t.SetVisible(1f);
    }

    public void GetPosition()
    {
        gettingPosition = true;
    }

    public void GetMinPosition()
    {
        gettingMinPosition = true;
    }

    public void GetMaxPosition()
    {
        gettingMaxPosition = true;
    }

    public void MouseDown()
    {
        if (gettingPosition)
        {
            selection.location = CameraController.GetMouseTile();
            gettingPosition = false;
            SelectComponent(selection.name);
        }
        if (gettingMaxPosition)
        {
            Game game = Game.Get();
            game.qd.quest.SetMaxCam(CameraController.GetMouseTile());
            SelectQuest();
            gettingMaxPosition = false;
        }
        if (gettingMinPosition)
        {
            Game game = Game.Get();
            game.qd.quest.SetMinCam(CameraController.GetMouseTile());
            SelectQuest();
            gettingMinPosition = false;
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


    public void DeleteComponent(string type)
    {
        Game game = Game.Get();

        List<string> sides = new List<string>();

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.qd.components)
        {
            if (kv.Key.IndexOf(type) == 0)
            {
                sides.Add(kv.Key);
                sides.Add("");
            }
        }
        esl = new EditorSelectionList("Component to Delete:", sides, delegate { SelectToDelete(); });
        esl.SelectItem();
    }

    public void DeleteComponent()
    {
        Game game = Game.Get();

        List<string> sides = new List<string>();

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.qd.components)
        {
            sides.Add(kv.Key);
            sides.Add("");
        }
        esl = new EditorSelectionList("Component to Delete:", sides, delegate { SelectToDelete(); });
        esl.SelectItem();
    }

    public void SelectToDelete()
    {
        Game game = Game.Get();

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.qd.components)
        {
            kv.Value.RemoveReference(esl.selection);
        }

        if (game.qd.components.ContainsKey(esl.selection))
        {
            game.qd.components.Remove(esl.selection);
        }
        SelectQuest();
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
        NewSelection(d);
        CameraController.SetCamera(d.location);

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(3, 1), "Door", delegate { TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 0), new Vector2(16, 1), name.Substring("Door".Length), delegate { ListDoor(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { RenameComponent(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 2), new Vector2(8, 1), ">< Position", delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 4), new Vector2(8, 1), "Rotation (" + d.rotation + ")", delegate { DoorRotate(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 6), new Vector2(8, 1), "Color", delegate { DoorColour(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 8), new Vector2(8, 1), "Event", delegate { SelectEvent(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 29), new Vector2(3, 1), "Back", delegate { Back(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        game.tokenBoard.AddHighlight(d.location, "DoorAnchor", "editor");

        d.SetVisible(1f);
    }

    public void SelectToken(string name)
    {
        Clean();
        Game game = Game.Get();
        QuestData.Token t = game.qd.components[name] as QuestData.Token;
        NewSelection(t);
        CameraController.SetCamera(t.location);

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), "Token", delegate { TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1), name.Substring("Token".Length), delegate { ListToken(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { RenameComponent(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 2), new Vector2(8, 1), ">< Position", delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 4), new Vector2(8, 1), t.spriteName, delegate { TokenType(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 6), new Vector2(8, 1), "Event", delegate { SelectEvent(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 29), new Vector2(3, 1), "Back", delegate { Back(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        t.SetVisible(1f);
    }

    public void TokenType()
    {
        esl = new EditorSelectionList("Select Token", GetTokenNames(), delegate { SelectTokenType(); });
        esl.SelectItem();
    }

    public void SelectTokenType()
    {
        QuestData.Token t = selection as QuestData.Token;
        t.spriteName = esl.selection;
        SelectComponent(t.name);
    }

    public void SelectMonster(string name)
    {
        Clean();
        Game game = Game.Get();
        QuestData.Monster m = game.qd.components[name] as QuestData.Monster;
        NewSelection(m);
        CameraController.SetCamera(m.location);

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), "Monster", delegate { TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1), name.Substring("Monster".Length), delegate { ListMonster(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { RenameComponent(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 2), new Vector2(8, 1), ">< Position", delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 4), new Vector2(8, 1), "Event", delegate { SelectEvent(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(12, 4), new Vector2(8, 1), "Placement", delegate { SelectMonsterPlacements(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        if (m.unique)
        {
            tb = new TextButton(new Vector2(0, 6), new Vector2(8, 1), "Unique", delegate { MonsterUnique(); });
        }
        else
        {
            tb = new TextButton(new Vector2(0, 6), new Vector2(8, 1), "Normal", delegate { MonsterUnique(); });
        }
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        DialogBox db = new DialogBox(new Vector2(0, 8), new Vector2(5, 1), "Unique Title:");
        db.ApplyTag("editor");

        dbe1 = new DialogBoxEditable(new Vector2(5, 8), new Vector2(15, 1), m.uniqueTitleOriginal, delegate { UpdateUniqueTitle(); });
        dbe1.ApplyTag("editor");
        dbe1.AddBorder();

        db = new DialogBox(new Vector2(0, 10), new Vector2(20, 1), "Unique Information:");
        db.ApplyTag("editor");

        dbe2 = new DialogBoxEditable(new Vector2(0, 11), new Vector2(20, 8), m.uniqueText, delegate { UpdateUniqueText(); });
        dbe2.ApplyTag("editor");
        dbe2.AddBorder();

        db = new DialogBox(new Vector2(0, 20), new Vector2(3, 1), "Types:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(12, 20), new Vector2(1, 1), "+", delegate { MonsterTypeAdd(0); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        for (int i = 0; i < 8; i++)
        {
            if (m.mTypes.Length > i)
            {
                int mSlot = i;
                string mName = m.mTypes[i];
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
            if (m.mTraits.Length > i)
            {
                int mSlot = i;
                string mName = m.mTraits[i];

                tb = new TextButton(new Vector2(14, 21 + i), new Vector2(1, 1), "-", delegate { MonsterTraitsRemove(mSlot); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");

                tb = new TextButton(new Vector2(15, 21 + i), new Vector2(5, 1), mName, delegate { MonsterTraitReplace(mSlot); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        tb = new TextButton(new Vector2(0, 29), new Vector2(3, 1), "Back", delegate { Back(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        game.tokenBoard.AddHighlight(m.location, "MonsterLoc", "editor");

        m.SetVisible(1f);
    }

    public void UpdateUniqueTitle()
    {
        QuestData.Monster m = selection as QuestData.Monster;
        m.uniqueTitleOriginal = dbe1.uiInput.text;
    }

    public void UpdateUniqueText()
    {
        QuestData.Monster m = selection as QuestData.Monster;
        m.uniqueText = dbe1.uiInput.text;
    }

    public void SelectMonsterPlacements()
    {
        QuestData.Monster m = selection as QuestData.Monster;
        Clean();
        CameraController.SetCamera(m.location);

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), "Monster", delegate { TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1), m.name.Substring("Monster".Length), delegate { ListMonster(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { RenameComponent(); });
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
                if (m.placement[heroes].Length > i)
                {
                    int mSlot = i;
                    string place = m.placement[heroes][i];

                    tb = new TextButton(new Vector2(0, offset), new Vector2(1, 1), "-", delegate { MonsterPlaceRemove(h, mSlot); }, Color.red);
                    tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                    tb.ApplyTag("editor");

                    tb = new TextButton(new Vector2(1, offset), new Vector2(19, 1), place, delegate { SelectMPlace(place); });
                    tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                    tb.ApplyTag("editor");
                }
                offset++;
            }
        }
        tb = new TextButton(new Vector2(0, 29), new Vector2(3, 1), "Back", delegate { SelectMonster(m.name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");
    }

    public void MonsterUnique()
    {
        QuestData.Monster m = selection as QuestData.Monster;
        m.unique = !m.unique;
        SelectComponent(m.name);
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
        esl = new EditorSelectionList("Select Item", list, delegate { SelectMonsterTrait(pos); });
        esl.SelectItem();
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
        esl = new EditorSelectionList("Select Item", list, delegate { SelectMonsterTrait(); });
        esl.SelectItem();
    }

    public void SelectMonsterTrait()
    {
        QuestData.Monster m = selection as QuestData.Monster;
        string[] newM = new string[m.mTraits.Length + 1];

        int i;
        for (i = 0; i < m.mTraits.Length; i++)
        {
            newM[i] = m.mTraits[i];
        }

        newM[i] = esl.selection;
        m.mTraits = newM;
        SelectComponent(m.name);
    }

    public void SelectMonsterTrait(int pos)
    {
        QuestData.Monster m = selection as QuestData.Monster;
        m.mTraits[pos] = esl.selection;
        SelectComponent(m.name);
    }

    public void MonsterTraitsRemove(int pos)
    {
        QuestData.Monster m = selection as QuestData.Monster;

        if ((m.mTypes.Length == 0) && (m.mTraits.Length == 1))
        {
            return;
        }

        string[] newM = new string[m.mTraits.Length - 1];

        int j = 0;
        for (int i = 0; i < m.mTraits.Length; i++)
        {
            if (i != pos || i != j)
            {
                newM[j] = m.mTraits[i];
                j++;
            }
        }
        m.mTraits = newM;
        SelectComponent(m.name);
    }

    public void MonsterTypeRemove(int pos)
    {
        QuestData.Monster m = selection as QuestData.Monster;

        if ((m.mTypes.Length == 1) && (m.mTraits.Length == 0))
        {
            return;
        }

        string[] newM = new string[m.mTypes.Length - 1];

        int j = 0;
        for (int i = 0; i < m.mTypes.Length; i++)
        {
            if (i != pos || i != j)
            {
                newM[j] = m.mTypes[i];
                j++;
            }
        }
        m.mTypes = newM;
        SelectComponent(m.name);
    }

    public void MonsterPlaceAdd(int heroes)
    {
        Game game = Game.Get();

        List<string> mplaces = new List<string>();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.qd.components)
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
        esl = new EditorSelectionList("Select Item", mplaces, delegate { MonsterPlaceAddSelection(heroes); });
        esl.SelectItem();
    }

    public void MonsterPlaceAddSelection(int heroes)
    {
        QuestData.Monster m = selection as QuestData.Monster;
        string[] newM = new string[m.placement[heroes].Length + 1];
        int i;
        for (i = 0; i < m.placement[heroes].Length; i++)
        {
            newM[i] = m.placement[heroes][i];
        }

        newM[i] = esl.selection;
        m.placement[heroes] = newM;
        SelectMonsterPlacements();
    }

    public void MonsterPlaceRemove(int heroes, int pos)
    {
        QuestData.Monster m = selection as QuestData.Monster;
        string[] newM = new string[m.placement[heroes].Length - 1];

        int j = 0;
        for (int i = 0; i < m.placement[heroes].Length; i++)
        {
            if (i != pos || i != j)
            {
                newM[j] = m.placement[heroes][i];
                j++;
            }
        }
        m.placement[heroes] = newM;
        SelectMonsterPlacements();
    }

    public void MonsterTypeAdd(int pos)
    {
        Game game = Game.Get();
        List<string> monsters = new List<string>();
        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            monsters.Add(kv.Key);
        }
        esl = new EditorSelectionList("Select Item", monsters, delegate { SelectMonsterType(pos); });
        esl.SelectItem();
    }

    public void MonsterTypeReplace(int pos)
    {
        Game game = Game.Get();
        List<string> monsters = new List<string>();
        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            monsters.Add(kv.Key);
        }
        esl = new EditorSelectionList("Select Item", monsters, delegate { SelectMonsterType(pos, true); });
        esl.SelectItem();
    }

    public void SelectMonsterType(int pos, bool replace = false)
    {
        QuestData.Monster m = selection as QuestData.Monster;
        if (replace)
        {
            m.mTypes[pos] = esl.selection;
        }
        else
        {
            string[] newM = new string[m.mTypes.Length + 1];

            int j = 0;
            for (int i = 0; i < newM.Length; i++)
            {
                if (j == pos && i == j)
                {
                    newM[i] = esl.selection;
                }
                else
                {
                    newM[i] = m.mTypes[j];
                    j++;
                }
            }
            m.mTypes = newM;
        }
        SelectComponent(m.name);
    }

    public void SelectMPlace(string name)
    {
        Clean();
        Game game = Game.Get();
        QuestData.MPlace m = game.qd.components[name] as QuestData.MPlace;
        NewSelection(m);
        CameraController.SetCamera(m.location);

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), "MPlace", delegate { TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1), name.Substring("MPlace".Length), delegate { ListMPlace(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { RenameComponent(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 2), new Vector2(8, 1), ">< Position", delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        string r = "Right";
        if (m.rotate) r = "Down";
        tb = new TextButton(new Vector2(0, 4), new Vector2(8, 1), "Rotate: " + r, delegate { RotateMPlace(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        string mast = "Minion";
        if (m.master) mast = "Master";
        tb = new TextButton(new Vector2(0, 6), new Vector2(8, 1), mast, delegate { MasterMPlace(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 29), new Vector2(3, 1), "Back", delegate { Back(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        game.tokenBoard.AddHighlight(m.location, "MonsterLoc", "editor");

        m.SetVisible(1f);
    }

    public void RenameComponent()
    {
        string name = selection.name.Substring(selection.typeDynamic.Length);
        te =  new QuestEditorTextEdit("Component Name:", name, delegate { RenameComponentFinished(); });
        te.EditText();
    }

    public void RenameComponentFinished()
    {
        string newName = System.Text.RegularExpressions.Regex.Replace(te.value, "[^A-Za-z0-9_]", "");
        if (newName.Equals("")) return;
        string baseName = selection.typeDynamic + newName;
        string name = baseName;
        Game game = Game.Get();
        int i = 0;
        while (game.qd.components.ContainsKey(name))
        {
            name = baseName + i++;
        }

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.qd.components)
        {
            kv.Value.ChangeReference(selection.name, name);
        }

        game.qd.components.Remove(selection.name);
        selection.name = name;
        game.qd.components.Add(selection.name, selection);
        SelectComponent(name);
    }


    public void RotateMPlace()
    {
        QuestData.MPlace d = selection as QuestData.MPlace;
        d.rotate = !d.rotate;
        SelectComponent(d.name);
    }

    public void MasterMPlace()
    {
        QuestData.MPlace d = selection as QuestData.MPlace;
        d.master = !d.master;
        SelectComponent(d.name);
    }


    public void SelectEvent(string name)
    {
        Clean();
        Game game = Game.Get();
        QuestData.Event e = game.qd.components[name] as QuestData.Event;
        NewSelection(e);
        if (e.highlight)
        {
            CameraController.SetCamera(e.location);
        }

        string type = QuestData.Event.type;
        if (e is QuestData.Door)
        {
            type = QuestData.Door.type;
        }
        if (e is QuestData.Monster)
        {
            type = QuestData.Monster.type;
        }
        if (e is QuestData.Token)
        {
            type = QuestData.Token.type;
        }

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), type, delegate { TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1), name.Substring(type.Length), delegate { ListEvent(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { RenameComponent(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 2), new Vector2(7, 1), ">< Position", delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        if (!e.GetType().IsSubclassOf(typeof(QuestData.Event)))
        {
            if (!e.locationSpecified)
            {
                tb = new TextButton(new Vector2(7, 2), new Vector2(4, 1), "Unused", delegate { EventPosition(); });
            }
            else if (e.highlight)
            {
                tb = new TextButton(new Vector2(7, 2), new Vector2(4, 1), "Highlight", delegate { EventPosition(); });
            }
            else
            {
                tb = new TextButton(new Vector2(7, 2), new Vector2(4, 1), "Camera", delegate { EventPosition(); });
            }
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
        }

        tb = new TextButton(new Vector2(12, 2), new Vector2(8, 1), "Flags/Events", delegate { SelectEventPageTwo(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        DialogBox db = new DialogBox(new Vector2(0, 3), new Vector2(20, 1), "Dialog:");
        db.ApplyTag("editor");

        dbe1 = new DialogBoxEditable(new Vector2(0, 4), new Vector2(20, 8), e.originalText, delegate { UpdateEventText(); });
        dbe1.ApplyTag("editor");
        dbe1.AddBorder();

        db = new DialogBox(new Vector2(0, 12), new Vector2(8, 1), "Trigger:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 13), new Vector2(8, 1), e.trigger, delegate { EventTrigger(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(8, 12), new Vector2(8, 1), "Selection:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(8, 13), new Vector2(8, 1), e.heroListName, delegate { EventHighlight(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(16, 12), new Vector2(2, 1), "Min");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(16, 13), new Vector2(2, 1), e.minHeroes.ToString(), delegate { EventHeroCount(false); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(18, 12), new Vector2(2, 1), "Max");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(18, 13), new Vector2(2, 1), e.maxHeroes.ToString(), delegate { EventHeroCount(true); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 15), new Vector2(9, 1), "Add Components:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(9, 15), new Vector2(1, 1), "+", delegate { EventAddVisibility(true); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        int offset = 16;
        int index;
        for (index = 0; index < 12; index++)
        {
            if (e.addComponents.Length > index)
            {
                int i = index;
                db = new DialogBox(new Vector2(0, offset), new Vector2(9, 1), e.addComponents[index]);
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(9, offset++), new Vector2(1, 1), "-", delegate { EventRemoveVisibility(i, true); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        db = new DialogBox(new Vector2(10, 15), new Vector2(9, 1), "Remove Components:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 15), new Vector2(1, 1), "+", delegate { EventAddVisibility(false); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        offset = 16;
        for (index = 0; index < 12; index++)
        {
            if (e.removeComponents.Length > index)
            {
                int i = index;
                db = new DialogBox(new Vector2(10, offset), new Vector2(9, 1), e.removeComponents[index]);
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(19, offset++), new Vector2(1, 1), "-", delegate { EventRemoveVisibility(i, false); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        if (!type.Equals(QuestData.Event.type))
        {
            tb = new TextButton(new Vector2(0, 29), new Vector2(3, 1), "Back", delegate { SelectComponent(name); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
        }
        else
        {
            tb = new TextButton(new Vector2(0, 29), new Vector2(3, 1), "Back", delegate { Back(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
        }

        if (e.locationSpecified)
        {
            game.tokenBoard.AddHighlight(e.location, "EventLoc", "editor");
        }

        e.SetVisible(1f);
    }

    public void UpdateEventText()
    {
        if (!dbe1.uiInput.text.Equals(""))
        {
            QuestData.Event e = selection as QuestData.Event;
            e.originalText = dbe1.uiInput.text;
        }
    }

    public void SelectEventPageTwo()
    {
        Clean();
        Game game = Game.Get();
        QuestData.Event e = selection as QuestData.Event;

        string type = QuestData.Event.type;
        if (e is QuestData.Door)
        {
            type = QuestData.Door.type;
        }
        if (e is QuestData.Monster)
        {
            type = QuestData.Monster.type;
        }
        if (e is QuestData.Token)
        {
            type = QuestData.Token.type;
        }

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), type, delegate { TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1), e.name.Substring(type.Length), delegate { ListEvent(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { RenameComponent(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        DialogBox db = new DialogBox(new Vector2(0, 1), new Vector2(19, 1), "Trigger Events:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 1), new Vector2(1, 1), "+", delegate { EventAddEvent(0); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        int offset = 2;
        int index;
        for (index = 0; index < 8; index++)
        {
            if (e.nextEvent.Length > index)
            {
                int i = index;
                tb = new TextButton(new Vector2(0, offset), new Vector2(1, 1), "-", delegate { EventRemoveEvent(i); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
                db = new DialogBox(new Vector2(1, offset), new Vector2(18, 1), e.nextEvent[index]);
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(19, offset++), new Vector2(1, 1), "+", delegate { EventAddEvent(i + 1); }, Color.green);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        db = new DialogBox(new Vector2(0, 10), new Vector2(19, 1), "Fail Events:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 10), new Vector2(1, 1), "+", delegate { EventAddEvent(0, true); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        offset = 11;
        for (index = 0; index < 12; index++)
        {
            if (e.failEvent.Length > index)
            {
                int i = index;
                tb = new TextButton(new Vector2(0, offset), new Vector2(1, 1), "-", delegate { EventRemoveEvent(i, true); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
                db = new DialogBox(new Vector2(1, offset), new Vector2(18, 1), e.failEvent[index]);
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(19, offset++), new Vector2(1, 1), "+", delegate { EventAddEvent(i + 1, true); }, Color.green);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }


        db = new DialogBox(new Vector2(0, 19), new Vector2(5, 1), "Flags:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(5, 19), new Vector2(1, 1), "+", delegate { EventFlagAdd("flag"); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        offset = 20;
        for (index = 0; index < 8; index++)
        {
            if (e.flags.Length > index)
            {
                int i = index;
                db = new DialogBox(new Vector2(0, offset), new Vector2(5, 1), e.flags[index]);
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(5, offset++), new Vector2(1, 1), "-", delegate { EventFlagRemove(i); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        db = new DialogBox(new Vector2(7, 19), new Vector2(5, 1), "Set:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(12, 19), new Vector2(1, 1), "+", delegate { EventFlagAdd("set"); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        offset = 20;
        for (index = 0; index < 8; index++)
        {
            if (e.setFlags.Length > index)
            {
                int i = index;
                db = new DialogBox(new Vector2(7, offset), new Vector2(5, 1), e.setFlags[index]);
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(12, offset++), new Vector2(1, 1), "-", delegate { EventFlagSetRemove(i); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        db = new DialogBox(new Vector2(14, 19), new Vector2(5, 1), "Clear:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 19), new Vector2(1, 1), "+", delegate { EventFlagAdd("clear"); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        offset = 20;
        for (index = 0; index < 8; index++)
        {
            if (e.clearFlags.Length > index)
            {
                int i = index;
                db = new DialogBox(new Vector2(14, offset), new Vector2(5, 1), e.clearFlags[index]);
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(19, offset++), new Vector2(1, 1), "-", delegate { EventFlagClearRemove(i); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        tb = new TextButton(new Vector2(0, 29), new Vector2(3, 1), "Back", delegate { SelectEvent(e.name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        if (e.locationSpecified)
        {
            game.tokenBoard.AddHighlight(e.location, "EventLoc", "editor");
        }

        e.SetVisible(1f);
    }

    public void EventRemoveEvent(int index, bool fail = false)
    {
        QuestData.Event e = selection as QuestData.Event;

        if (fail)
        {
            string[] events = new string[e.failEvent.Length - 1];

            int j = 0;
            for (int i = 0; i <  e.failEvent.Length; i++)
            {
                if (i != index)
                {
                    events[j++] = e.failEvent[i];
                }
            }
            e.failEvent = events;
        }
        else
        {
            string[] events = new string[e.nextEvent.Length - 1];

            int j = 0;
            for (int i = 0; i < e.nextEvent.Length; i++)
            {
                if (i != index)
                {
                    events[j++] = e.nextEvent[i];
                }
            }
            e.nextEvent = events;
        }

        SelectEventPageTwo();
    }

    public void EventAddEvent(int index, bool fail = false)
    {
        List<string> events = new List<string>();

        Game game = Game.Get();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                events.Add(kv.Key);
            }
        }

        esl = new EditorSelectionList("Select Event", events, delegate { SelectEventAddEvent(index, fail); });
        esl.SelectItem();
    }

    public void SelectEventAddEvent(int index, bool fail)
    {
        QuestData.Event e = selection as QuestData.Event;

        if (fail)
        {
            System.Array.Resize(ref e.failEvent, e.failEvent.Length + 1);

            for (int i = e.failEvent.Length - 1; i >= 0; i--)
            {
                if (i > index)
                {
                    e.failEvent[i] = e.failEvent[i - 1];
                }
                if (i == index)
                {
                    e.failEvent[i] = esl.selection;
                }
            }
        }
        else
        {
            System.Array.Resize(ref e.nextEvent, e.nextEvent.Length + 1);

            for (int i = e.nextEvent.Length - 1; i >= 0; i--)
            {
                if (i > index)
                {
                    e.nextEvent[i] = e.nextEvent[i - 1];

                }
                if (i == index)
                {
                    e.nextEvent[i] = esl.selection;
                }
            }
        }

        SelectEventPageTwo();
    }

    public void EventFlagRemove(int index)
    {
        QuestData.Event e = selection as QuestData.Event;

        string[] flags = new string[e.flags.Length - 1];
        int j = 0;
        for (int i = 0; i < e.flags.Length; i++)
        {
            if (i != index)
            {
                flags[j++] = e.flags[i];
            }
        }
        e.flags = flags;

        SelectEventPageTwo();
    }

    public void EventFlagSetRemove(int index)
    {
        QuestData.Event e = selection as QuestData.Event;

        string[] flags = new string[e.setFlags.Length - 1];
        int j = 0;
        for (int i = 0; i < e.setFlags.Length; i++)
        {
            if (i != index)
            {
                flags[j++] = e.setFlags[i];
            }
        }
        e.setFlags = flags;

        SelectEventPageTwo();
    }

    public void EventFlagClearRemove(int index)
    {
        QuestData.Event e = selection as QuestData.Event;

        string[] flags = new string[e.clearFlags.Length - 1];
        int j = 0;
        for (int i = 0; i < e.clearFlags.Length; i++)
        {
            if (i != index)
            {
                flags[j++] = e.clearFlags[i];
            }
        }
        e.clearFlags = flags;

        SelectEventPageTwo();
    }

    public void EventFlagAdd(string type)
    {
        HashSet<string> flags = new HashSet<string>();
        flags.Add("{NEW}");

        Game game = Game.Get();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                QuestData.Event e = kv.Value as QuestData.Event;
                foreach (string s in e.flags) flags.Add(s);
                foreach (string s in e.setFlags) flags.Add(s);
                foreach (string s in e.clearFlags) flags.Add(s);
            }
        }
        List<string> list = new List<string>(flags);
        esl = new EditorSelectionList("Select Flag", list, delegate { SelectEventFlagAdd(type); });
        esl.SelectItem();
    }

    public void SelectEventFlagAdd(string type)
    {
        QuestData.Event e = selection as QuestData.Event;

        if (esl.selection.Equals("{NEW}"))
        {
            te = new QuestEditorTextEdit("Flag Name:", "", delegate { NewFlag(type); });
            te.EditText();
            return;
        }

        if (type.Equals("flag"))
        {
            System.Array.Resize(ref e.flags, e.flags.Length + 1);
            e.flags[e.flags.Length - 1] = esl.selection;
        }

        if (type.Equals("set"))
        {
            System.Array.Resize(ref e.setFlags, e.setFlags.Length + 1);
            e.setFlags[e.setFlags.Length - 1] = esl.selection;
        }

        if (type.Equals("clear"))
        {
            System.Array.Resize(ref e.clearFlags, e.clearFlags.Length + 1);
            e.clearFlags[e.clearFlags.Length - 1] = esl.selection;
        }

        SelectEventPageTwo();
    }

    public void NewFlag(string type)
    {
        QuestData.Event e = selection as QuestData.Event;
        string newName = System.Text.RegularExpressions.Regex.Replace(te.value, "[^A-Za-z0-9_]", "");

        if (newName.Equals("")) return;
        if (type.Equals("flag"))
        {
            System.Array.Resize(ref e.flags, e.flags.Length + 1);
            e.flags[e.flags.Length - 1] = newName;
        }

        if (type.Equals("set"))
        {
            System.Array.Resize(ref e.setFlags, e.setFlags.Length + 1);
            e.setFlags[e.setFlags.Length - 1] = newName;
        }

        if (type.Equals("clear"))
        {
            System.Array.Resize(ref e.clearFlags, e.clearFlags.Length + 1);
            e.clearFlags[e.clearFlags.Length - 1] = newName;
        }

        SelectEventPageTwo();
    }

    public void EventAddVisibility(bool add)
    {
        List<string> components = new List<string>();

        Game game = Game.Get();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.qd.components)
        {
            components.Add(kv.Key);
        }

        esl = new EditorSelectionList("Select Event", components, delegate { SelectEventAddVisibility(add); });
        esl.SelectItem();
    }

    public void SelectEventAddVisibility(bool add)
    {
        QuestData.Event e = selection as QuestData.Event;

        string[] oldC = null;

        if(add)
        {
            oldC = e.addComponents;
        }
        else
        {
            oldC = e.removeComponents;
        }
        string[] newC = new string[oldC.Length + 1];
        int i;
        for (i = 0; i < oldC.Length; i++)
        {
            newC[i] = oldC[i];
        }

        newC[i] = esl.selection;

        if (add)
        {
            e.addComponents = newC;
        }
        else
        {
            e.removeComponents = newC;
        }

        SelectComponent(e.name);
    }

    public void EventRemoveVisibility(int index, bool add)
    {
        QuestData.Event e = selection as QuestData.Event;

        string[] oldC = null;

        if (add)
        {
            oldC = e.addComponents;
        }
        else
        {
            oldC = e.removeComponents;
        }

        string[] newC = new string[oldC.Length - 1];

        int j = 0;
        for (int i = 0; i < oldC.Length; i++)
        {
            if (i != index)
            {
                newC[j++] = oldC[i];
            }
        }

        if (add)
        {
            e.addComponents = newC;
        }
        else
        {
            e.removeComponents = newC;
        }

        SelectComponent(e.name);
    }

    public void EventHeroCount(bool max)
    {
        List<string> num = new List<string>();
        num.Add("0");
        num.Add("1");
        num.Add("2");
        num.Add("3");
        num.Add("4");

        esl = new EditorSelectionList("Select Number", num, delegate { SelectEventHeroCount(max); });
        esl.SelectItem();
    }

    public void SelectEventHeroCount(bool max)
    {
        QuestData.Event e = selection as QuestData.Event;
        if (max)
        {
            e.maxHeroes = int.Parse(esl.selection);
        }
        else
        {
            e.minHeroes = int.Parse(esl.selection);
        }
        SelectComponent(e.name);
    }

    public void EventHighlight()
    {
        List<string> events = new List<string>();
        events.Add("");

        Game game = Game.Get();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                events.Add(kv.Key);
            }
        }

        esl = new EditorSelectionList("Select Event", events, delegate { SelectEventHighlight(); });
        esl.SelectItem();
    }

    public void SelectEventHighlight()
    {
        QuestData.Event e = selection as QuestData.Event;
        e.heroListName = esl.selection;
        SelectComponent(e.name);
    }

    public void EventTrigger()
    {
        List<string> triggers = new List<string>();
        triggers.Add("");
        triggers.Add("EventStart");
        triggers.Add("EndRound");
        triggers.Add("NoMorale");

        Game game = Game.Get();
        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            triggers.Add("Defeated" + kv.Key);
            triggers.Add("DefeatedUnique" + kv.Key);
        }

        esl = new EditorSelectionList("Select Trigger", triggers, delegate { SelectEventTrigger(); });
        esl.SelectItem();
    }

    public void SelectEventTrigger()
    {
        QuestData.Event e = selection as QuestData.Event;
        e.trigger = esl.selection;
        SelectComponent(e.name);
    }

    public void EventPosition()
    {
        QuestData.Event e = selection as QuestData.Event;
        if (!e.locationSpecified)
        {
            e.locationSpecified = true;
            e.highlight = false;
        }
        else if (!e.highlight)
        {
            e.locationSpecified = true;
            e.highlight = true;
        }
        else
        {
            e.locationSpecified = false;
            e.highlight = false;
        }
        SelectEvent(e.name);
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
        tiles.Add("{NEW:Tile}");
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
        doors.Add("{NEW:Door}");
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
        Game game = Game.Get();
        int index = 0;

        while (game.qd.components.ContainsKey("Door" + index))
        {
            index++;
        }
        game.qd.components.Add("Door" + index, new QuestData.Door("Door" + index));
        SelectComponent("Door" + index);
    }

    public void ListToken()
    {
        Game game = Game.Get();

        List<string> tokens = new List<string>();
        tokens.Add("{NEW:Token}");
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.qd.components)
        {
            if (kv.Value is QuestData.Token)
            {
                tokens.Add(kv.Key);
            }
        }

        if (tokens.Count == 0)
        {
            return;
        }
        esl = new EditorSelectionList("Select Item", tokens, delegate { SelectComponent(); });
        esl.SelectItem();
    }

    public void NewToken()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.qd.components.ContainsKey("Token" + index))
        {
            index++;
        }
        game.qd.components.Add("Token" + index, new QuestData.Token("Token" + index));
        SelectComponent("Token" + index);
    }

    public void ListMonster()
    {
        Game game = Game.Get();

        List<string> monsters = new List<string>();
        monsters.Add("{NEW:Monster}");
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.qd.components)
        {
            if (kv.Value is QuestData.Monster)
            {
                monsters.Add(kv.Key);
            }
        }

        if (monsters.Count == 0)
        {
            return;
        }
        esl = new EditorSelectionList("Select Item", monsters, delegate { SelectComponent(); });
        esl.SelectItem();
    }

    public void NewMonster()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.qd.components.ContainsKey("Monster" + index))
        {
            index++;
        }
        game.qd.components.Add("Monster" + index, new QuestData.Monster("Monster" + index));
        SelectComponent("Monster" + index);
    }

    public void ListMPlace()
    {
        Game game = Game.Get();

        List<string> mplaces = new List<string>();
        mplaces.Add("{NEW:MPlace}");
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.qd.components)
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
        esl = new EditorSelectionList("Select Item", mplaces, delegate { SelectComponent(); });
        esl.SelectItem();
    }

    public void NewMPlace()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.qd.components.ContainsKey("MPlace" + index))
        {
            index++;
        }
        game.qd.components.Add("MPlace" + index, new QuestData.MPlace("MPlace" + index));
        SelectComponent("MPlace" + index);
    }

    public void ListEvent()
    {
        Game game = Game.Get();

        List<string> events = new List<string>();
        events.Add("{NEW:Event}");
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                if (!kv.Value.GetType().IsSubclassOf(typeof(QuestData.Event)))
                {
                    events.Add(kv.Key);
                }
            }
        }

        if (events.Count == 0)
        {
            return;
        }
        esl = new EditorSelectionList("Select Item", events, delegate { SelectComponent(); });
        esl.SelectItem();
    }

    public void NewEvent()
    {
        Game game = Game.Get();
        int index = 0;

        while (game.qd.components.ContainsKey("Event" + index))
        {
            index++;
        }
        game.qd.components.Add("Event" + index, new QuestData.Event("Event" + index));
        SelectComponent("Event" + index);
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
        if (name.Equals("{NEW:Tile}"))
        {
            NewTile();
            return;
        }
        if (name.Equals("{NEW:Door}"))
        {
            NewDoor();
            return;
        }
        if (name.Equals("{NEW:Token}"))
        {
            NewToken();
            return;
        }
        if (name.Equals("{NEW:Monster}"))
        {
            NewMonster();
            return;
        }
        if (name.Equals("{NEW:MPlace}"))
        {
            NewMPlace();
            return;
        }
        if (name.Equals("{NEW:Event}"))
        {
            NewEvent();
            return;
        }

        Game game = Game.Get();

        // This can happen to due rename/delete
        if (!game.qd.components.ContainsKey(name))
        {
            SelectQuest();
        }

        if (game.qd.components[name] is QuestData.Tile)
        {
            SelectTile(name);
            return;
        }

        if (game.qd.components[name] is QuestData.Door)
        {
            SelectDoor(name);
            return;
        }
        if (game.qd.components[name] is QuestData.Token)
        {
            SelectToken(name);
            return;
        }
        if (game.qd.components[name] is QuestData.Monster)
        {
            SelectMonster(name);
            return;
        }
        if (game.qd.components[name] is QuestData.MPlace)
        {
            SelectMPlace(name);
            return;
        }
        if (game.qd.components[name] is QuestData.Event)
        {
            SelectEvent(name);
            return;
        }
    }

    public static void Cancel()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
    }

    public static List<string> GetTokenNames()
    {
        List<string> names = new List<string>();
        names.Add("Search-Token");
        names.Add("search-token-special");
        names.Add("objective-token-black");
        names.Add("objective-token-blue");
        names.Add("objective-token-green");
        names.Add("objective-token-red");
        names.Add("objective-token-white");
        names.Add("rubble-token-clipped");
        names.Add("villager-token-man");
        names.Add("villager-tokens-woman");
        names.Add("tokensunstone");
        return names;
    }
}
