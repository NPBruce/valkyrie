using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentUI : EditorComponent
{
    QuestData.UI uiComponent;
    EditorSelectionList imageList;
    DialogBoxEditable locXDBE;
    DialogBoxEditable locYDBE;
    DialogBoxEditable sizeXDBE;
    DialogBoxEditable sizeYDBE;

    private readonly StringKey SELECT_IMAGE = new StringKey("val", "SELECT_IMAGE");

    public EditorComponentUI(string nameIn) : base()
    {
        Game game = Game.Get();
        uiComponent = game.quest.qd.components[nameIn] as QuestData.UI;
        component = uiComponent;
        name = component.sectionName;
        Update();
    }

    override public void Update()
    {
        base.Update();
        Game game = Game.Get();

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(2, 1), CommonStringKeys.UI, delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(2, 0), new Vector2(17, 1),
            new StringKey(null, name.Substring("UI".Length), false), delegate { QuestEditorData.ListUI(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), CommonStringKeys.E, delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 2), new Vector2(20, 1),
            new StringKey(null, uiComponent.imageName, false), delegate { SetImage(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        DialogBox db = new DialogBox(new Vector2(0, 4), new Vector2(10, 1), new StringKey("val", "POSITION"));
        db.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 5), new Vector2(2, 1), new StringKey(null, "X:", false));
        db.ApplyTag("editor");

        locXDBE = new DialogBoxEditable(new Vector2(2, 5), new Vector2(3, 1),
            uiComponent.location.x.ToString(), delegate { UpdateNumbers(); });
        locXDBE.ApplyTag("editor");
        locXDBE.AddBorder();

        db = new DialogBox(new Vector2(5, 5), new Vector2(2, 1), new StringKey(null, "Y:", false));
        db.ApplyTag("editor");

        locYDBE = new DialogBoxEditable(new Vector2(7, 5), new Vector2(3, 1),
            uiComponent.location.y.ToString(), delegate { UpdateNumbers(); });
        locYDBE.ApplyTag("editor");
        locYDBE.AddBorder();

        db = new DialogBox(new Vector2(0, 6), new Vector2(10, 1), new StringKey("val", "SIZE"));
        db.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 7), new Vector2(2, 1), new StringKey(null, "X:", false));
        db.ApplyTag("editor");

        sizeXDBE = new DialogBoxEditable(new Vector2(2, 7), new Vector2(3, 1),
            uiComponent.size.x.ToString(), delegate { UpdateNumbers(); });
        sizeXDBE.ApplyTag("editor");
        sizeXDBE.AddBorder();

        db = new DialogBox(new Vector2(5, 7), new Vector2(2, 1), new StringKey(null, "Y:", false));
        db.ApplyTag("editor");

        sizeYDBE = new DialogBoxEditable(new Vector2(7, 7), new Vector2(3, 1),
            uiComponent.size.y.ToString(), delegate { UpdateNumbers(); });
        sizeYDBE.ApplyTag("editor");
        sizeYDBE.AddBorder();

        tb = new TextButton(new Vector2(0, 9), new Vector2(8, 1), CommonStringKeys.EVENT, delegate { QuestEditorData.SelectAsEvent(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");
    }

    public void SetImage()
    {
        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;
        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        foreach (string s in Directory.GetFiles(relativePath, "*.png", SearchOption.AllDirectories))
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s.Substring(relativePath.Length + 1)));
        }
        foreach (string s in Directory.GetFiles(relativePath, "*.jpg", SearchOption.AllDirectories))
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s.Substring(relativePath.Length + 1)));
        }
        imageList = new EditorSelectionList(SELECT_IMAGE, list, delegate { SelectImage(); });
        imageList.SelectItem();
    }

    public void SelectImage()
    {
        uiComponent.imageName = imageList.selection;
        Update();
    }

    public void UpdateNumbers()
    {
        if (!locXDBE.Text.Equals(""))
        {
            float.TryParse(locXDBE.Text, out uiComponent.location.x);
        }
        if (!locYDBE.Text.Equals(""))
        {
            float.TryParse(locYDBE.Text, out uiComponent.location.y);
        }
        if (!sizeXDBE.Text.Equals(""))
        {
            float.TryParse(sizeXDBE.Text, out uiComponent.size.x);
        }
        if (!sizeYDBE.Text.Equals(""))
        {
            float.TryParse(sizeYDBE.Text, out uiComponent.size.y);
        }
        Update();
    }
}
