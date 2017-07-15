using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
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

    QuestEditorTextEdit sourceFileText;

    UIElementEditable commentUIE;

    // The editor scroll area;
    public UIElementScrollVertical scrollArea;

    // Update redraws the selection UI
    virtual public void Update()
    {
        game = Game.Get();

        float scrollPos = -14.5f * UIScaler.GetPixelsPerUnit();
        if (scrollArea != null && !scrollArea.ObjectDestroyed())
        {
            scrollPos = scrollArea.GetScrollPosition();
        }
        Clean();

        AddScrollArea();

        float offset = 0;
        offset = DrawComponentSelection(offset);

        offset = AddSubComponents(offset);

        offset = AddSource(offset);

        offset = AddComment(offset);

        scrollArea.SetScrollSize(offset);
        scrollArea.SetScrollPosition(scrollPos);

        UIElement ui = new UIElement(Game.EDITOR);
        ui.SetLocation(0, 0, 1, 1);
        ui.SetText("<b>⇦</b>", Color.blue);
        ui.SetButton(delegate { QuestEditorData.Back(); });
        ui.SetBGColor(Color.black);
        new UIElementBorder(ui, Color.blue);

        AddTitle();
    }

    protected virtual void AddTitle()
    {
        UIElement ui = new UIElement(Game.EDITOR);
        ui.SetLocation(1, 0, 20, 1);
        ui.SetText(name);
        ui.SetButton(delegate { QuestEditorData.TypeSelect(component.typeDynamic); });
        ui.SetBGColor(Color.black);
        new UIElementBorder(ui);
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
        scrollArea = new UIElementScrollVertical(Game.EDITOR);
        scrollArea.SetLocation(0, 1, 21, 29);
        new UIElementBorder(scrollArea);
    }

    virtual public float DrawComponentSelection(float offset)
    {
        offset += 1;
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(2, offset, 5, 1);
        ui.SetText(new StringKey("val", "RENAME"));
        ui.SetButton(delegate { Rename(); });
        new UIElementBorder(ui);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(13, offset, 5, 1);
        ui.SetText(new StringKey("val", "DELETE"), Color.red);
        ui.SetButton(delegate { Delete(); });
        new UIElementBorder(ui, Color.red);

        return offset + 2;
    }

    virtual public float AddSubComponents(float offset)
    {
        return offset;
    }

    public void Delete()
    {
        // Border
        UIElement ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-6.5f), 1, 13, 4);
        new UIElementBorder(ui);

        // Heading
        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-6f), 1, 12, 1);
        ui.SetText(new StringKey("val", "CONFIRM"));

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-5.5f), 3, 6, 1);
        ui.SetText(CommonStringKeys.DELETE, Color.red);
        ui.SetButton(delegate { ConfirmDelete(); });
        ui.SetBGColor(new Color(0.0f, 0.03f, 0f));
        new UIElementBorder(ui, Color.red);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(1.5f), 3, 4, 1);
        ui.SetText(CommonStringKeys.CANCEL);
        ui.SetButton(delegate { Destroyer.Dialog(); });
        ui.SetBGColor(new Color(0.03f, 0, 0f));
        new UIElementBorder(ui);
    }

    public void ConfirmDelete()
    {
        QuestEditorData.DeleteCurrentComponent();
    }

    virtual public float AddComment(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset++, 5, 1);
        ui.SetText(new StringKey("val", "X_COLON", (new StringKey("val", "COMMENT"))));

        commentUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
        commentUIE.SetLocation(0.5f, offset, 19, 15);
        commentUIE.SetText(component.comment);
        offset += commentUIE.HeightToTextPadding(1);
        commentUIE.SetButton(delegate { SetComment(); });
        new UIElementBorder(commentUIE);

        return offset + 1;
    }

    virtual public float AddSource(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 5, 1);
        ui.SetText(new StringKey("val", "X_COLON", (new StringKey("val", "SOURCE"))));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(5, offset, 14.5f, 1);
        ui.SetText(component.source.Replace("\\n", "\n"));
        ui.SetButton(delegate { ChangeSource(); });
        new UIElementBorder(ui);

        return offset + 2;
    }

    public void SetComment()
    {
        component.comment = commentUIE.GetText().Replace("\n", "\\n").Replace("\r", "\\n");
        Update();
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

        LocalizationRead.dicts["qst"].RenamePrefix(component.sectionName + ".", name + ".");

        // Old Localization Entryes need to be renamed? Maybe not
        // Change all entrys related with old name to key new name
        //LocalizationRead.dicts["qst"].ChangeReference(component.sectionName, name);

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
        UIWindowSelectionList select = new UIWindowSelectionList(SelectSource, new StringKey("val", "SELECT", new StringKey("val", "FILE")));

        select.AddItem("{NEW:File}");
        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;
        foreach(string s in Directory.GetFiles(relativePath, "*.ini", SearchOption.AllDirectories))
        {
            select.AddItem(s.Substring(relativePath.Length + 1));
        }
        select.Draw();
    }

    public void SelectSource(string source)
    {
        if (source.Equals("{NEW:File}"))
        {
            sourceFileText = new QuestEditorTextEdit(new StringKey("val", "FILE"), "", delegate { NewSource(); });
            sourceFileText.EditText();
        }
        else
        {
            SetSource(source);
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