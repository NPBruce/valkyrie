using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

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
        DialogBox db = new DialogBox(new Vector2(0, offset), new Vector2(4, 1), new StringKey("val", "X_COLON", CommonStringKeys.POSITION));
        db.background.transform.SetParent(scrollArea.transform);
        db.ApplyTag(Game.EDITOR);

        TextButton tb = new TextButton(new Vector2(4, offset), new Vector2(4, 1), CommonStringKeys.POSITION_SNAP, delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.SetParent(scrollArea.transform);
        tb.ApplyTag(Game.EDITOR);

        return offset + 2;
    }

    override public float AddSubEventComponents(float offset)
    {
        doorComponent = component as QuestData.Door;

        DialogBox db = new DialogBox(new Vector2(0, offset), new Vector2(6, 1), new StringKey("val", "X_COLON", new StringKey("val", "ROTATION")));
        db.background.transform.SetParent(scrollArea.transform);
        db.ApplyTag(Game.EDITOR);
        TextButton tb = new TextButton(new Vector2(6, offset), new Vector2(3, 1),
            new StringKey(null, doorComponent.rotation.ToString() + "˚", false), delegate { Rotate(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.SetParent(scrollArea.transform);
        tb.ApplyTag(Game.EDITOR);
        offset += 2;

        tb = new TextButton(new Vector2(0.5f, offset), new Vector2(8, 1), COLOR, delegate { Colour(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.SetParent(scrollArea.transform);
        tb.ApplyTag(Game.EDITOR);
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
