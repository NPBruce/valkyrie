using UnityEngine;
using System.Text;
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
    
    override public float AddSubComponents(float offset)
    {
        Game game = Game.Get();
        CameraController.SetCamera(tileComponent.location);

        DialogBox db = new DialogBox(new Vector2(0, offset), new Vector2(4.5f, 1), new StringKey("val", "X_COLON", new StringKey("val", "IMAGE")));
        db.background.transform.SetParent(scrollArea.transform);
        db.ApplyTag(Game.EDITOR);

        TextButton tb = new TextButton(new Vector2(4.5f, offset), new Vector2(15f, 1), 
            new StringKey(null, tileComponent.tileSideName,false), delegate { ChangeTileSide(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.SetParent(scrollArea.transform);
        tb.ApplyTag(Game.EDITOR);

        offset += 2;

        db = new DialogBox(new Vector2(0, offset), new Vector2(4, 1), new StringKey("val", "X_COLON", CommonStringKeys.POSITION));
        db.background.transform.SetParent(scrollArea.transform);
        db.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(4, offset), new Vector2(4, 1), CommonStringKeys.POSITION_SNAP, delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.SetParent(scrollArea.transform);
        tb.ApplyTag(Game.EDITOR);

        offset += 2;

        db = new DialogBox(new Vector2(0, offset), new Vector2(6, 1), new StringKey("val", "X_COLON", new StringKey("val", "ROTATION")));
        db.background.transform.SetParent(scrollArea.transform);
        db.ApplyTag(Game.EDITOR);
        tb = new TextButton(new Vector2(6, offset), new Vector2(3, 1),
            new StringKey(null, tileComponent.rotation.ToString() + "˚", false), delegate { TileRotate(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.SetParent(scrollArea.transform);
        tb.ApplyTag(Game.EDITOR);

        offset += 2;

        game.tokenBoard.AddHighlight(tileComponent.location, "TileAnchor", Game.EDITOR);

        game.quest.ChangeAlpha(tileComponent.sectionName, 1f);

        return offset;
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
            StringBuilder display = new StringBuilder().Append(kv.Key);
            StringBuilder localizedDisplay = new StringBuilder().Append(kv.Value.name.Translate());
            List<string> traits = new List<string>(kv.Value.traits);
            foreach (string s in kv.Value.sets)
            {
                if (s.Length == 0)
                {
                    traits.Add("base");
                }
                else
                {
                    display.Append(" ").Append(s);
                    traits.Add(s);
                }
            }

            Color buttonColor = Color.white;

            if (usedSides.Contains(kv.Key))
            {
                buttonColor = Color.grey;
            }

            sides.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyTraitsColorItem(
                localizedDisplay.ToString(),display.ToString(), traits, buttonColor));
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
