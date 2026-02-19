using UnityEngine;
using System.Text;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

public class EditorComponentTile : EditorComponent
{
    QuestData.Tile tileComponent;

    public EditorComponentTile(string nameIn) : base()
    {
        Game game = Game.Get();
        tileComponent = game.CurrentQuest.qd.components[nameIn] as QuestData.Tile;
        component = tileComponent;
        name = component.sectionName;
        Update();
    }

    override protected void RefreshReference()
    {
        base.RefreshReference();
        tileComponent = component as QuestData.Tile;
    }

    override public float AddSubComponents(float offset)
    {
        Game game = Game.Get();
        CameraController.SetCamera(tileComponent.location);

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "IMAGE")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset, 15, 1);
        if (tileComponent.tileSideName.Length == 0)
        {
            ui.SetText("{NONE}");
        }
        else
        {
            ui.SetText(tileComponent.tileSideName);
        }
        ui.SetButton(delegate { ChangeTileSide(); });
        new UIElementBorder(ui);
        offset += 2;

        // Custom Image
        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "CUSTOM_IMAGE")));

        if (tileComponent.customImage.Length > 0)
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(4, offset, 12, 1);
            ui.SetTextFileName(tileComponent.customImage);
            ui.SetButton(delegate { SetCustomImage(); });
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(16.5f, offset, 3, 1);
            ui.SetText(CommonStringKeys.RESET);
            ui.SetButton(delegate { ClearCustomImage(); });
            new UIElementBorder(ui);
            offset += 2;

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0, offset, 4, 1);
            ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.TOP));
            
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(4, offset, 5, 1);
            ui.SetText(tileComponent.top.ToString());
            ui.SetButton(delegate { SetTop(); });
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(10, offset, 4, 1);
            ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.LEFT));
            
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(14, offset, 5, 1);
            ui.SetText(tileComponent.left.ToString());
            ui.SetButton(delegate { SetLeft(); });
            new UIElementBorder(ui);
        }
        else
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(16.5f, offset, 3, 1);
            ui.SetText(CommonStringKeys.SET);
            ui.SetButton(delegate { SetCustomImage(); });
            new UIElementBorder(ui);
        }
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.POSITION));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset, 4, 1);
        ui.SetText(CommonStringKeys.POSITION_SNAP);
        ui.SetButton(delegate { GetPosition(); });
        new UIElementBorder(ui);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(9, offset, 4, 1);
        ui.SetText(CommonStringKeys.POSITION_FREE);
        ui.SetButton(delegate { GetPosition(false); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "ROTATION")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset, 3, 1);
        ui.SetText(tileComponent.rotation.ToString());
        ui.SetButton(delegate { TileRotate(); });
        new UIElementBorder(ui);
        offset += 2;

        game.tokenBoard.AddHighlight(tileComponent.location, "TileAnchor", Game.EDITOR);

        game.CurrentQuest.ChangeAlpha(tileComponent.sectionName, 1f);

        return offset;
    }

    public void ChangeTileSide()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();
        UIWindowSelectionListTraits select = new UIWindowSelectionListImage(SelectTileSide, new StringKey("val", "SELECT", CommonStringKeys.TILE));

        // Work out what sides are used
        HashSet<string> usedSides = new HashSet<string>();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.CurrentQuest.qd.components)
        {
            QuestData.Tile t = kv.Value as QuestData.Tile;
            if (t != null && t.tileSideName.Length > 0)
            {
                usedSides.Add(t.tileSideName);
                if (game.cd.ContainsKey<TileSideData>(t.tileSideName))
                {
                    usedSides.Add(game.cd.Get<TileSideData>(t.tileSideName).reverse);
                }
            }
        }

        foreach (KeyValuePair<string, TileSideData> kv in game.cd.GetAll<TileSideData>())
        {
            if (usedSides.Contains(kv.Key))
            {
                select.AddItem(kv.Value, new Color(0.4f, 0.4f, 1));
            }
            else
            {
                select.AddItem(kv.Value);
            }
        }
        select.ExcludeExpansions();
        select.Draw();

        // Auto selection of source
        TileSideData currentTileData = null;
        if (game.cd.ContainsKey<TileSideData>(tileComponent.tileSideName))
        {
            currentTileData = game.cd.Get<TileSideData>(tileComponent.tileSideName);
        }
        if (currentTileData != null)
        {
            string setID = "";
            if (currentTileData.sets.Count > 0)
            {
                setID = currentTileData.sets[0];
            }
            if (setID.Equals(""))
            {
                setID = "base";
            }
            select.SelectTrait(CommonStringKeys.SOURCE.Translate(), new StringKey("val", setID).Translate());
        }
    }

    public void SelectTileSide(string tile)
    {
        Game game = Game.Get();
        tileComponent.tileSideName = tile.Split(" ".ToCharArray())[0];
        tileComponent.customImage = "";
        tileComponent.top = 0;
        tileComponent.left = 0;
        game.CurrentQuest.Remove(tileComponent.sectionName);
        game.CurrentQuest.Add(tileComponent.sectionName);
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
        game.CurrentQuest.Remove(tileComponent.sectionName);
        game.CurrentQuest.Add(tileComponent.sectionName);

        game.CurrentQuest.Remove(tileComponent.sectionName);
        game.CurrentQuest.Add(tileComponent.sectionName);

        Update();
    }

    public void SetCustomImage()
    {
        base.SetCustomImage(SelectCustomImage);
    }

    public void SelectCustomImage(string image)
    {
        if (image.Equals("{NONE}"))
        {
            tileComponent.customImage = "";
            tileComponent.top = 0;
            tileComponent.left = 0;
        }
        else
        {
            tileComponent.customImage = image;
            tileComponent.tileSideName = "";
        }
        Game.Get().CurrentQuest.Remove(tileComponent.sectionName);
        Game.Get().CurrentQuest.Add(tileComponent.sectionName);
        Update();
    }

    public void ClearCustomImage()
    {
        tileComponent.customImage = "";
        tileComponent.top = 0;
        tileComponent.left = 0;
        Game.Get().CurrentQuest.Remove(tileComponent.sectionName);
        Game.Get().CurrentQuest.Add(tileComponent.sectionName);
        Update();
    }

    public void SetTop()
    {
        Game game = Game.Get();
        string initial = tileComponent.top.ToString();
        // Allow negative numbers
        initial = initial.Replace(",", ".");
        QuestEditorTextEdit db = new QuestEditorTextEdit(
            new StringKey("val", "X_COLON", CommonStringKeys.TOP), 
            initial,
            delegate(string s)
            {
                 float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out tileComponent.top); 
                 Update(); 
            });
        db.EditText();
        db.iField.characterValidation = UnityEngine.UI.InputField.CharacterValidation.Decimal;
    }

    public void SetLeft()
    {
        Game game = Game.Get();
        string initial = tileComponent.left.ToString();
        // Allow negative numbers
        initial = initial.Replace(",", ".");
        QuestEditorTextEdit db = new QuestEditorTextEdit(
            new StringKey("val", "X_COLON", CommonStringKeys.LEFT), 
            initial,
            delegate(string s)
            {
                 float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out tileComponent.left); 
                 Update(); 
            });
        db.EditText();
        db.iField.characterValidation = UnityEngine.UI.InputField.CharacterValidation.Decimal;
    }
}
