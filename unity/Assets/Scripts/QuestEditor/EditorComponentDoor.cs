using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentDoor : EditorComponent
{
    private readonly StringKey COLOR = new StringKey("val", "COLOR");

    QuestData.Door doorComponent;
    // List to select door colour
    EditorSelectionList colorList;

    public EditorComponentDoor(string nameIn) : base()
    {
        Game game = Game.Get();
        doorComponent = game.quest.qd.components[nameIn] as QuestData.Door;
        component = doorComponent;
        name = component.sectionName;
        Update();
    }
    
    override public void Update()
    {
        base.Update();
        Game game = Game.Get();
        CameraController.SetCamera(doorComponent.location);

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(3, 1), 
            CommonStringKeys.DOOR, delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 0), new Vector2(16, 1), 
            new StringKey(null,name.Substring("Door".Length),false), 
            delegate { QuestEditorData.ListDoor(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), CommonStringKeys.E, delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        DialogBox db = new DialogBox(new Vector2(0, 2), new Vector2(4, 1), CommonStringKeys.POSITION);
        db.ApplyTag("editor");

        // This is a snapped position
        tb = new TextButton(new Vector2(4, 2), new Vector2(1, 1), CommonStringKeys.POSITION_SNAP, delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 4), new Vector2(8, 1),
            new StringKey("val","ROTATION",doorComponent.rotation), 
            delegate { Rotate(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 6), new Vector2(8, 1), COLOR, delegate { Colour(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 8), new Vector2(8, 1), CommonStringKeys.EVENT, delegate { QuestEditorData.SelectAsEvent(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        game.tokenBoard.AddHighlight(doorComponent.location, "DoorAnchor", "editor");

        game.quest.ChangeAlpha(doorComponent.sectionName, 1f);
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
