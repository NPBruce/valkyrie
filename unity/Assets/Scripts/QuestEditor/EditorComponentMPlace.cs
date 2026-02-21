using UnityEngine;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
using System.Collections.Generic;
using System.Globalization;

public class EditorComponentMPlace : EditorComponent
{
    QuestData.MPlace mPlaceComponent;

    public EditorComponentMPlace(string nameIn) : base()
    {
        Game game = Game.Get();
        mPlaceComponent = game.CurrentQuest.qd.components[nameIn] as QuestData.MPlace;
        component = mPlaceComponent;
        name = component.sectionName;
        Update();
    }

    override protected void RefreshReference()
    {
        base.RefreshReference();
        mPlaceComponent = component as QuestData.MPlace;
    }

    override public float AddSubComponents(float offset)
    {
        CameraController.SetCamera(mPlaceComponent.location);
        Game game = Game.Get();

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.POSITION));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset, 4, 1);
        ui.SetText(CommonStringKeys.POSITION_SNAP);
        ui.SetButton(delegate { GetPosition(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "ROTATION")));

        StringKey rotateKey = new StringKey("val","RIGHT");
        if (mPlaceComponent.rotate)
        {
            rotateKey = new StringKey("val", "DOWN");
        }

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset, 4, 1);
        ui.SetText(rotateKey);
        ui.SetButton(delegate { Rotate(); });
        new UIElementBorder(ui);
        offset += 2;

        StringKey mast = new StringKey("val","MONSTER_MINION");
        if (mPlaceComponent.master)
        {
            mast = new StringKey("val","MONSTER_MASTER");
        }
        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 8, 1);
        ui.SetText(mast);
        ui.SetButton(delegate { MasterToggle(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "SIZE")));
        
        StringKey sizeKey = new StringKey("val","DEFAULT");
        if (!mPlaceComponent.tokenSize.Equals(""))
        {
            if (float.TryParse(mPlaceComponent.tokenSize, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            {
                sizeKey = new StringKey(null, mPlaceComponent.tokenSize, false);
            }
            else
            {
                sizeKey = new StringKey("val", mPlaceComponent.tokenSize.ToUpper());
            }
        }

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset, 5, 1);
        ui.SetText(sizeKey);
        ui.SetButton(delegate { ClickSize(); });
        new UIElementBorder(ui);
        offset += 2;

        game.tokenBoard.AddHighlight(mPlaceComponent.location, "MonsterLoc", Game.EDITOR);

        return offset;
    }

    public void Rotate()
    {
        mPlaceComponent.rotate = !mPlaceComponent.rotate;
        Update();
    }

    public void MasterToggle()
    {
        mPlaceComponent.master = !mPlaceComponent.master;
        Update();
    }

    public void ClickSize()
    {
        UIWindowSelectionList select = new UIWindowSelectionList(SelectSize, new StringKey("val", "SELECT", new StringKey("val", "SIZE")));
        
        select.AddItem(CommonStringKeys.DEFAULT.Translate(), "");
        select.AddItem(new StringKey("val", "SMALL").Translate(), "small");
        select.AddItem(new StringKey("val", "MEDIUM").Translate(), "medium");
        select.AddItem(new StringKey("val", "HUGE").Translate(), "huge");
        select.AddItem(new StringKey("val", "MASSIVE").Translate(), "massive");
        select.AddItem(new StringKey("val", "ACTUAL").Translate(), "Actual");

        select.Draw();
    }

    public void SelectSize(string size)
    {
        mPlaceComponent.tokenSize = size;
        Update();
    }
}
