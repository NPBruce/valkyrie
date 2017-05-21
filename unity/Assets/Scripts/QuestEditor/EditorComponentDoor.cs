using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

public class EditorComponentDoor : EditorComponentEvent
{
    private readonly StringKey COLOR = new StringKey("val", "COLOR");

    QuestData.Door doorComponent;
    // List to select door colour
    EditorSelectionList colorList;

    public EditorComponentDoor(string nameIn) : base(nameIn)
    {
    }

    override public float AddPosition(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.POSITION));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset, 4, 1);
        ui.SetText(CommonStringKeys.POSITION_SNAP);
        ui.SetButton(delegate { GetPosition(); });
        new UIElementBorder(ui);

        return offset + 2;
    }

    override public float AddSubEventComponents(float offset)
    {
        doorComponent = component as QuestData.Door;

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 6, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "ROTATION")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(6, offset, 3, 1);
        ui.SetText(doorComponent.rotation.ToString() + "˚");
        ui.SetButton(delegate { Rotate(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 8, 1);
        ui.SetText(COLOR);
        ui.SetButton(delegate { Colour(); });
        new UIElementBorder(ui);
        offset += 2;

        game.quest.ChangeAlpha(doorComponent.sectionName, 1f);

        return offset;
    }

    override public float AddEventTrigger(float offset)
    {
        return offset;
    }

    public void Rotate()
    {
        if (doorComponent.rotation == 0)
        {
            doorComponent.rotation = 90;
        }
        else
        {
            doorComponent.rotation = 0;
        }
        Game.Get().quest.Remove(doorComponent.sectionName);
        Game.Get().quest.Add(doorComponent.sectionName);
        Update();
    }

    public void Colour()
    {
        List<EditorSelectionList.SelectionListEntry> colours = new List<EditorSelectionList.SelectionListEntry>();
        foreach (KeyValuePair<string, string> kv in ColorUtil.LookUp())
        {
            colours.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(kv.Key));
        }
        colorList = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, colours, delegate { SelectColour(); });
        colorList.SelectItem();
    }

    public void SelectColour()
    {
        doorComponent.colourName = colorList.selection;
        Game.Get().quest.Remove(doorComponent.sectionName);
        Game.Get().quest.Add(doorComponent.sectionName);
        Update();
    }

}
