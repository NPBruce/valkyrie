using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using System.IO;

// Super class for all editor selectable components
// Handles UI and editing
public class EditorComponent {

    // Reference to the selected component
    public QuestData.QuestComponent component;
    // These are used to latch if a position button has been pressed
    public bool gettingPosition = false;
    public bool gettingPositionSnap = false;
    // The name of the component
    public string name;

    public Game game;
    // This is used for creating the component rename dialog
    QuestEditorTextEdit rename;
    private readonly StringKey COMPONENT_NAME = new StringKey("val","COMPONENT_NAME");

    EditorSelectionList sourceESL;
    QuestEditorTextEdit sourceFileText;
    DialogBoxEditable commentDBE;

    // The editor scroll area;
    public GameObject scrollArea;
    public RectTransform scrollInnerRect;

    // Update redraws the selection UI
    virtual public void Update()
    {
        game = Game.Get();
        float scrollPos = -15 * UIScaler.GetPixelsPerUnit();
        if (scrollArea != null)
        {
            scrollPos = scrollArea.GetComponent<RectTransform>().anchoredPosition.y - scrollInnerRect.rect.y;
        }
        Clean();

        AddScrollArea();

        float offset = 0;
        offset = DrawComponentSelection(offset);

        offset = AddSubComponents(offset);

        offset = AddSource(offset);

        offset = AddComment(offset);

        if (offset < 30) offset = 30;
        SetScrollLimit(offset);
        scrollArea.GetComponent<RectTransform>().anchoredPosition = new Vector2(10 * UIScaler.GetPixelsPerUnit(), scrollPos + scrollInnerRect.rect.y);
    }
    public void Clean()
    {
        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            Object.Destroy(go);

        // Clean up everything marked as 'editor'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.EDITOR))
            Object.Destroy(go);

        // Dim all components, this component will be made solid later
        Game.Get().quest.ChangeAlphaAll(0.2f);
    }

    public void AddScrollArea()
    {
        DialogBox db = new DialogBox(new Vector2(0, 0), new Vector2(20, 30), StringKey.NULL);
        UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();
        db.ApplyTag(Game.EDITOR);

        scrollArea = new GameObject("scroll");
        scrollInnerRect = scrollArea.AddComponent<RectTransform>();
        scrollArea.transform.SetParent(db.background.transform);
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, (20 * UIScaler.GetPixelsPerUnit()));
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 1);

        GameObject scrollBarObj = new GameObject("scrollbar");
        scrollBarObj.transform.SetParent(db.background.transform);
        RectTransform scrollBarRect = scrollBarObj.AddComponent<RectTransform>();
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 30 * UIScaler.GetPixelsPerUnit());
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 20 * UIScaler.GetPixelsPerUnit(), 1 * UIScaler.GetPixelsPerUnit());
        UnityEngine.UI.Scrollbar scrollBar = scrollBarObj.AddComponent<UnityEngine.UI.Scrollbar>();
        scrollBar.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
        scrollRect.verticalScrollbar = scrollBar;

        GameObject scrollBarHandle = new GameObject("scrollbarhandle");
        scrollBarHandle.transform.SetParent(scrollBarObj.transform);
        //RectTransform scrollBarHandleRect = scrollBarHandle.AddComponent<RectTransform>();
        scrollBarHandle.AddComponent<UnityEngine.UI.Image>();
        scrollBarHandle.GetComponent<UnityEngine.UI.Image>().color = new Color(0.7f, 0.7f, 0.7f);
        scrollBar.handleRect = scrollBarHandle.GetComponent<RectTransform>();
        scrollBar.handleRect.offsetMin = Vector2.zero;
        scrollBar.handleRect.offsetMax = Vector2.zero;

        scrollRect.content = scrollInnerRect;
        scrollRect.horizontal = false;
        scrollRect.scrollSensitivity = 27f;
    }

    virtual public float DrawComponentSelection(float offset)
    {
        TextButton tb = new TextButton(new Vector2(0, offset), new Vector2(20, 1),
            new StringKey(null, name, false), delegate { QuestEditorData.ListType(component.typeDynamic); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.SetParent(scrollArea.transform);
        tb.ApplyTag(Game.EDITOR);
        offset += 2;

        tb = new TextButton(new Vector2(2, offset), new Vector2(5, 1), new StringKey("val", "RENAME"), delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.SetParent(scrollArea.transform);
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(13, offset), new Vector2(5, 1), new StringKey("val", "DELETE"), delegate { Delete(); }, Color.red);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.SetParent(scrollArea.transform);
        tb.ApplyTag(Game.EDITOR);

        return offset + 2;
    }

    virtual public float AddSubComponents(float offset)
    {
        return offset;
    }

    public void Delete()
    {
        // Border
        DialogBox db = new DialogBox(new Vector2(UIScaler.GetHCenter(-6.5f), 1), new Vector2(13, 4), StringKey.NULL);
        db.AddBorder();

        // Heading
        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-6f), 1), new Vector2(12, 1), new StringKey("val", "CONFIRM"));

        TextButton tb = new TextButton(new Vector2(UIScaler.GetHCenter(-5.5f), 3), new Vector2(6, 1), CommonStringKeys.DELETE, delegate { ConfirmDelete(); }, Color.red);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.0f, 0.03f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(UIScaler.GetHCenter(1.5f), 3), new Vector2(4, 1), CommonStringKeys.CANCEL, delegate { Destroyer.Dialog(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont(); ;
    }

    public void ConfirmDelete()
    {
        QuestEditorData.DeleteCurrentComponent();
    }

    virtual public float AddComment(float offset)
    {
        DialogBox db = new DialogBox(new Vector2(0, offset++), new Vector2(5, 1), new StringKey("val","X_COLON",(new StringKey("val", "COMMENT"))));
        db.background.transform.SetParent(scrollArea.transform);
        db.ApplyTag(Game.EDITOR);

        commentDBE = new DialogBoxEditable(
            new Vector2(0.5f, offset), new Vector2(19, 5),
            component.comment, false, 
            delegate { SetComment(); });
        commentDBE.background.transform.SetParent(scrollArea.transform);
        commentDBE.ApplyTag(Game.EDITOR);
        commentDBE.AddBorder();

        return offset + 6;
    }

    virtual public float AddSource(float offset)
    {
        DialogBox db = new DialogBox(new Vector2(0, offset), new Vector2(5, 1), new StringKey("val", "X_COLON", (new StringKey("val", "SOURCE"))));
        db.background.transform.SetParent(scrollArea.transform);
        db.ApplyTag(Game.EDITOR);

        TextButton tb = new TextButton(new Vector2(5, offset), new Vector2(14.5f, 1), new StringKey(null, component.source), delegate { ChangeSource(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.SetParent(scrollArea.transform);
        tb.ApplyTag(Game.EDITOR);

        return offset + 2;
    }

    public void SetComment()
    {
        component.comment = commentDBE.Text.Replace("\n", "\\n").Replace("\r", "\\n");
        Update();
    }

    public void SetScrollLimit(float limit)
    {
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, limit * UIScaler.GetPixelsPerUnit());
    }

    // This is called by the editor
    virtual public void MouseDown()
    {
        Game game = Game.Get();
        // Are we looking for a position?
        if (!gettingPosition) return;

        // Get the location
        component.location = game.cc.GetMouseBoardPlane();
        if (gettingPositionSnap)
        {
            // Get a rounded location
            component.location = game.cc.GetMouseBoardRounded(game.gameType.SelectionRound());
            if (component is QuestData.Tile)
            {
                // Tiles have special rounding
                component.location = game.cc.GetMouseBoardRounded(game.gameType.TileRound());
            }
        }
        // Unlatch
        gettingPosition = false;
        // Redraw component
        Game.Get().quest.Remove(component.sectionName);
        Game.Get().quest.Add(component.sectionName);
        // Update UI
        Update();
    }

    virtual public void GetPosition(bool snap=true)
    {
        // Set latch, wait for button press
        gettingPosition = true;
        gettingPositionSnap = snap;
    }

    // Open a dialog to rename this component
    public void Rename()
    {
        string name = component.sectionName.Substring(component.typeDynamic.Length);
        //The component name wont be translated but all name relative keys need to be updated
        rename =  new QuestEditorTextEdit(COMPONENT_NAME, name,delegate { RenameFinished(); });
        rename.EditText();
    }

    // Item renamed
    public void RenameFinished()
    {
        // Trim non alpha numeric
        string newName = System.Text.RegularExpressions.Regex.Replace(rename.value, "[^A-Za-z0-9_]", "");
        // Must have a name
        if (newName.Equals("")) return;
        // Add type
        string baseName = component.typeDynamic + newName;
        // Find first available unique name
        string name = baseName;
        Game game = Game.Get();
        int i = 0;
        while (game.quest.qd.components.ContainsKey(name))
        {
            name = baseName + i++;
        }

        // Update all references to this component
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            kv.Value.ChangeReference(component.sectionName, name);
        }

        LocalizationRead.scenarioDict.RenamePrefix(component.sectionName + ".", name + ".");

        // Old Localization Entryes need to be renamed? Maybe not
        // Change all entrys related with old name to key new name
        //LocalizationRead.scenarioDict.ChangeReference(component.sectionName, name);

        // Remove component by old name
        game.quest.qd.components.Remove(component.sectionName);
        game.quest.Remove(component.sectionName);
        component.sectionName = name;
        // Add component with new name
        game.quest.qd.components.Add(component.sectionName, component);
        game.quest.Add(component.sectionName);
        // Reselect with new name
        QuestEditorData.SelectComponent(component.sectionName);
    }

    public void ChangeSource()
    {
        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;
        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();

        list.Add(new EditorSelectionList.SelectionListEntry("{NEW:File}"));
        foreach (string s in Directory.GetFiles(relativePath, "*.ini", SearchOption.AllDirectories))
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s.Substring(relativePath.Length + 1)));
        }

        sourceESL = new EditorSelectionList(new StringKey("val", "SELECT", new StringKey("val", "FILE")), list, delegate { SelectSource(); });
        sourceESL.SelectItem();
    }

    public void SelectSource()
    {
        if (sourceESL.selection.Equals("{NEW:File}"))
        {
            sourceFileText = new QuestEditorTextEdit(new StringKey("val", "FILE"), "", delegate { NewSource(); });
            sourceFileText.EditText();
        }
        else
        {
            SetSource(sourceESL.selection);
        }
    }

    public void NewSource()
    {
        string s = sourceFileText.value;
        if (!s.Substring(s.Length - 4, 4).Equals(".ini"))
        {
            s += ".ini";
        }
        SetSource(s);
    }

    public void SetSource(string source)
    {
        component.source = source;
        Update();
    }
}