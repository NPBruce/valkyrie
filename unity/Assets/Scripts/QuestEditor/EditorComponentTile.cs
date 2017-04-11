using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentTile : EditorComponent
{
    QuestData.Tile tileComponent;
    EditorSelectionList tileESL;

    public EditorComponentTile(string nameIn) : base()
    {
        Game game = Game.Get();
        tileComponent = game.quest.qd.components[nameIn] as QuestData.Tile;
        component = tileComponent;
        name = component.sectionName;
        Update();
    }
    
    override public void Update()
    {
        base.Update();
        Game game = Game.Get();
        CameraController.SetCamera(tileComponent.location);

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(3, 1), CommonStringKeys.TILE, delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 0), new Vector2(16, 1), 
            new StringKey(name.Substring("Tile".Length),false), delegate { QuestEditorData.ListTile(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), CommonStringKeys.E, delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 2), new Vector2(20, 1), new StringKey(tileComponent.tileSideName,false), delegate { ChangeTileSide(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        DialogBox db = new DialogBox(new Vector2(0, 4), new Vector2(4, 1), CommonStringKeys.POSITION);
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 4), new Vector2(1, 1), CommonStringKeys.POSITION_SNAP, delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 6), new Vector2(8, 1),
            new StringKey("val","ROTATION",new StringKey(tileComponent.rotation.ToString(),false)), delegate { TileRotate(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        game.tokenBoard.AddHighlight(tileComponent.location, "TileAnchor", "editor");

        game.quest.ChangeAlpha(tileComponent.sectionName, 1f);
    }

    public void ChangeTileSide()
    {
        Game game = Game.Get();

        // Work out what sides are used
        HashSet<string> usedSides = new HashSet<string>();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            QuestData.Tile t = kv.Value as QuestData.Tile;
            if (t != null)
            {
                usedSides.Add(t.tileSideName);
                usedSides.Add(game.cd.tileSides[t.tileSideName].reverse);
            }
        }

        List<EditorSelectionList.SelectionListEntry> sides = new List<EditorSelectionList.SelectionListEntry>();
        foreach (KeyValuePair<string, TileSideData> kv in game.cd.tileSides)
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

            if (usedSides.Contains(kv.Key))
            {
                sides.Add(new EditorSelectionList.SelectionListEntry(display, sets, Color.grey));
            }
            else
            {
                sides.Add(new EditorSelectionList.SelectionListEntry(display, sets, Color.white));
            }
        }
        tileESL = new EditorSelectionList(
            new StringKey("val","SELECT",CommonStringKeys.TILE), sides, delegate { SelectTileSide(); });
        tileESL.SelectItem();
    }

    public void SelectTileSide()
    {
        Game game = Game.Get();
        tileComponent.tileSideName = tileESL.selection.Split(" ".ToCharArray())[0];
        game.quest.Remove(tileComponent.sectionName);
        game.quest.Add(tileComponent.sectionName);
        Update();
    }

    public void TileRotate()
    {
        if (tileComponent.rotation == 0)
        {
            tileComponent.rotation = 90;
        }
        else if (tileComponent.rotation > 0 && tileComponent.rotation <= 100)
        {
            tileComponent.rotation = 180;
        }
        else if (tileComponent.rotation > 100 && tileComponent.rotation <= 190)
        {
            tileComponent.rotation = 270;
        }
        else
        {
            tileComponent.rotation = 0;
        }

        Game game = Game.Get();
        game.quest.Remove(tileComponent.sectionName);
        game.quest.Add(tileComponent.sectionName);

        Update();
    }
}
