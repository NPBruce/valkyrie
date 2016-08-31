using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestEditorData {

    public EditorSelectionList esl;
    public QuestData.QuestComponent selection;
    public Stack<QuestData.QuestComponent> selectionStack;
    public bool gettingPosition = false;
    public bool gettingMinPosition = false;
    public bool gettingMaxPosition = false;
    public bool backTriggered = false;

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

        tb = new TextButton(new Vector2(21, 8), new Vector2(9, 1), "Token", delegate { ListToken(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(31, 8), new Vector2(9, 1), "New", delegate { NewToken(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.0f, 0.03f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(21, 10), new Vector2(9, 1), "Monster", delegate { ListMonster(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(31, 10), new Vector2(9, 1), "New", delegate { NewMonster(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.0f, 0.03f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(21, 12), new Vector2(9, 1), "MPlace", delegate { ListMPlace(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(31, 12), new Vector2(9, 1), "New", delegate { NewMPlace(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.0f, 0.03f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(21, 14), new Vector2(9, 1), "Event", delegate { ListEvent(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(31, 14), new Vector2(9, 1), "New", delegate { NewEvent(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.0f, 0.03f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(26.5f, 16), new Vector2(9, 1), "Cancel", delegate { Cancel(); });
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

        tb = new TextButton(new Vector2(0, 2), new Vector2(20, 1), game.qd.quest.name, delegate { Cancel(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 4), new Vector2(20, 6), game.qd.quest.description, delegate { Cancel(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

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

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { Cancel(); });
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

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { Cancel(); });
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

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { Cancel(); });
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

        tb = new TextButton(new Vector2(5, 8), new Vector2(15, 1), m.uniqueTitle, delegate { Cancel(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 10), new Vector2(20, 1), "Unique Information:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 11), new Vector2(20, 8), m.uniqueText, delegate { Cancel(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

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

    public void SelectMonsterPlacements()
    {
        QuestData.Monster m = selection as QuestData.Monster;
        Clean();
        Game game = Game.Get();
        CameraController.SetCamera(m.location);

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), "Monster", delegate { TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1), m.name.Substring("Monster".Length), delegate { ListMonster(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { Cancel(); });
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

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1), name.Substring("MPlace".Length), delegate { ListToken(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { Cancel(); });
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

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { Cancel(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 2), new Vector2(8, 1), ">< Position", delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        if (!e.GetType().IsSubclassOf(typeof(QuestData.Event)))
        {
            if (!e.locationSpecified)
            {
                tb = new TextButton(new Vector2(8, 2), new Vector2(8, 1), "Unused", delegate { EventPosition(); });
            }
            else if (e.highlight)
            {
                tb = new TextButton(new Vector2(8, 2), new Vector2(8, 1), "Highlight", delegate { EventPosition(); });
            }
            else
            {
                tb = new TextButton(new Vector2(8, 2), new Vector2(8, 1), "Camera", delegate { EventPosition(); });
            }
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
        }

        if (!type.Equals(QuestData.Event.type))
        {
            tb = new TextButton(new Vector2(7, 4), new Vector2(6, 1), "Back", delegate { SelectComponent(name); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
        }

        tb = new TextButton(new Vector2(0, 29), new Vector2(3, 1), "Back", delegate { Back(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        if (e.locationSpecified)
        {
            game.tokenBoard.AddHighlight(e.location, "EventLoc", "editor");
        }

        e.SetVisible(1f);
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

        Game game = Game.Get();

        if (!game.qd.components.ContainsKey(name))
        {
            Debug.Log("Error: Attempting to bring up missing component: " + name);
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
