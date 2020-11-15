using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
using System.IO;

// Super class for all editor selectable components
// Handles UI and editing
public class EditorComponent {

    private readonly StringKey TESTS = new StringKey("val", "TESTS");
    private readonly StringKey VAR = new StringKey("val", "VAR");
    private readonly StringKey OP = new StringKey("val", "OP");
    private readonly StringKey VALUE = new StringKey("val", "VALUE");
    private readonly StringKey ASSIGN = new StringKey("val", "ASSIGN");
    private readonly StringKey VAR_NAME = new StringKey("val", "VAR_NAME");
    private readonly StringKey NUMBER = new StringKey("val", "NUMBER");
    private readonly StringKey AND = new StringKey("val", "AND");
    private readonly StringKey OR = new StringKey("val", "OR");

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
    public QuestEditorTextEdit varText;

    UIElementEditable commentUIE;

    // The editor scroll area;
    public UIElementScrollVertical scrollArea;

    // Update redraws the selection UI
    virtual public void Update()
    {
        RefreshReference();

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
        ui.SetText("<b>⇦</b>", Color.cyan);
        ui.SetTextAlignment(TextAnchor.LowerCenter);
        ui.SetButton(delegate { QuestEditorData.Back(); });
        ui.SetBGColor(Color.black);
        new UIElementBorder(ui, Color.cyan);

        AddTitle();
    }

    protected virtual void RefreshReference()
    {
        component = Game.Get().quest.qd.components[name];
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
        Game.Get().quest.ChangeAlphaAll();
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
        ui.SetText(CommonStringKeys.RENAME);
        ui.SetButton(delegate { Rename(); });
        new UIElementBorder(ui);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(13, offset, 5, 1);
        ui.SetText(CommonStringKeys.DELETE, Color.red);
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
        ui.SetText(CommonStringKeys.CONFIRM);

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
        commentUIE.SetText(component.comment.Replace("\\n", "\n"));
        offset += commentUIE.HeightToTextPadding(1);
        commentUIE.SetButton(delegate { SetComment(); });
        new UIElementBorder(commentUIE);

        return offset + 1;
    }

    virtual public float AddSource(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 5, 1);
        ui.SetText(new StringKey("val", "X_COLON", (CommonStringKeys.SOURCE)));

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

    virtual public float AddEventVarConditionComponents(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 18, 1);
        ui.SetText(new StringKey("val", "X_COLON", TESTS));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(18.5f, offset, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddTestOp(); });
        new UIElementBorder(ui, Color.green);

        if (component.tests.VarTestsComponents.Count > 0)
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0.5f, offset, 1, 1);
            ui.SetText(CommonStringKeys.PLUS, Color.green);
            ui.SetButton(delegate { SelectAddParenthesis(); });
            new UIElementBorder(ui, Color.green);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(1.5f, offset, 2, 1);
            ui.SetText("(...)");
        }

        offset++;

        int component_index = 0;
        foreach (VarTestsComponent tc in component.tests.VarTestsComponents)
        {
            if (tc is VarOperation)
            {
                int tmp_index = component_index;

                // only display arrows if item can be moved
                if (component_index != (component.tests.VarTestsComponents.Count - 1)
                   && component.tests.VarTestsComponents.Count > 1
                   && component.tests.VarTestsComponents.FindIndex(component_index + 1, x => x.GetClassVarTestsComponentType() == VarTestsLogicalOperator.GetVarTestsComponentType()) != -1
                   )
                {
                    ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                    ui.SetLocation(0.5f, offset, 1, 1);
                    ui.SetText(CommonStringKeys.DOWN, Color.yellow);
                    ui.SetTextAlignment(TextAnchor.LowerCenter);
                    ui.SetButton(delegate { component.tests.moveComponent(tmp_index, false); Update(); });
                    new UIElementBorder(ui, Color.yellow);
                }

                if (component_index != 0
                    && component.tests.VarTestsComponents.FindLastIndex(component_index - 1, x => x.GetClassVarTestsComponentType() == VarTestsLogicalOperator.GetVarTestsComponentType()) != -1
                )
                {
                    ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                    ui.SetLocation(1.5f, offset, 1, 1);
                    ui.SetText(CommonStringKeys.UP, Color.yellow);
                    ui.SetTextAlignment(TextAnchor.LowerCenter);
                    ui.SetButton(delegate { component.tests.moveComponent(tmp_index, true); Update(); });
                    new UIElementBorder(ui, Color.yellow);
                }

                VarOperation tmp = (VarOperation)tc;
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(2.5f, offset, 8.5f, 1);
                ui.SetText(tmp.var);
                new UIElementBorder(ui);

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(11f, offset, 2, 1);
                ui.SetText(tmp.operation);
                ui.SetButton(delegate { SetTestOpp(tmp); });
                new UIElementBorder(ui);

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(13f, offset, 5.5f, 1);
                ui.SetText(tmp.value);
                ui.SetButton(delegate { SetValue(tmp); });
                new UIElementBorder(ui);

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(18.5f, offset++, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate { RemoveOp(tmp_index); });
                new UIElementBorder(ui, Color.red);
            }

            if (tc is VarTestsLogicalOperator)
            {
                VarTestsLogicalOperator tmp = (VarTestsLogicalOperator)tc;

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(10f, offset, 4, 1);
                if (tmp.op.Equals("AND"))
                    ui.SetText(AND);
                else if (tmp.op.Equals("OR"))
                    ui.SetText(OR);
                ui.SetButton(delegate { tmp.NextLogicalOperator(); Update(); });
                new UIElementBorder(ui);
                offset++;
            }

            if (tc is VarTestsParenthesis)
            {
                int tmp_index = component_index;
                VarTestsParenthesis tp = (VarTestsParenthesis)tc;

                if (component_index != (component.tests.VarTestsComponents.Count - 1)
                    && component.tests.VarTestsComponents.FindIndex(component_index + 1, x => x.GetClassVarTestsComponentType() == VarOperation.GetVarTestsComponentType()) != -1
                    )
                {
                    if (tp.parenthesis == "(")
                    {
                        int valid_index = component.tests.FindNextValidPosition(component_index, false);
                        if (valid_index != -1
                            && component.tests.FindClosingParenthesis(valid_index) != -1)
                        {
                            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                            ui.SetLocation(0.5f, offset, 1, 1);
                            ui.SetText(CommonStringKeys.DOWN, Color.yellow);
                            ui.SetTextAlignment(TextAnchor.LowerCenter);
                            ui.SetButton(delegate { component.tests.moveComponent(tmp_index, false); Update(); });
                            new UIElementBorder(ui, Color.yellow);
                        }
                    }
                    else if (tp.parenthesis == ")")
                    {
                        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                        ui.SetLocation(0.5f, offset, 1, 1);
                        ui.SetText(CommonStringKeys.DOWN, Color.yellow);
                        ui.SetTextAlignment(TextAnchor.LowerCenter);
                        ui.SetButton(delegate { component.tests.moveComponent(tmp_index, false); Update(); });
                        new UIElementBorder(ui, Color.yellow);
                    }
                }

                if (component_index != 0
                    && component.tests.VarTestsComponents.FindLastIndex(component_index - 1, x => x.GetClassVarTestsComponentType() == VarOperation.GetVarTestsComponentType()) != -1
                    )
                {
                    if (tp.parenthesis == "(")
                    {
                        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                        ui.SetLocation(1.5f, offset, 1, 1);
                        ui.SetText(CommonStringKeys.UP, Color.yellow);
                        ui.SetTextAlignment(TextAnchor.LowerCenter);
                        ui.SetButton(delegate { component.tests.moveComponent(tmp_index, true); Update(); });
                        new UIElementBorder(ui, Color.yellow);
                    }
                    else if (tp.parenthesis == ")")
                    {
                        int valid_index = component.tests.FindNextValidPosition(component_index, true);
                        if (valid_index != -1
                            && component.tests.FindOpeningParenthesis(valid_index) != -1)
                        {
                            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                            ui.SetLocation(1.5f, offset, 1, 1);
                            ui.SetText(CommonStringKeys.UP, Color.yellow);
                            ui.SetTextAlignment(TextAnchor.LowerCenter);
                            ui.SetButton(delegate { component.tests.moveComponent(tmp_index, true); Update(); });
                            new UIElementBorder(ui, Color.yellow);
                        }
                    }
                }

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(2.5f, offset, 2, 1);
                ui.SetText(tp.parenthesis);
                new UIElementBorder(ui);

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(4.5f, offset, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate { component.tests.Remove(tmp_index); Update(); });
                new UIElementBorder(ui, Color.red);

                offset++;
            }

            component_index++;
        }
        return offset + 1;
    }

    virtual public float AddEventVarOperationComponents(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 18, 1);
        ui.SetText(new StringKey("val", "X_COLON", ASSIGN));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(18.5f, offset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddAssignOp(); });
        new UIElementBorder(ui, Color.green);

        foreach (VarOperation op in component.operations)
        {
            VarOperation tmp = op;
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0.5f, offset, 8.5f, 1);
            ui.SetText(op.var);
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(9, offset, 2, 1);
            ui.SetText(op.operation);
            ui.SetButton(delegate { SetAssignOpp(tmp); });
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(11, offset, 7.5f, 1);
            ui.SetText(op.value);
            ui.SetButton(delegate { SetValue(tmp); });
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(18.5f, offset++, 1, 1);
            ui.SetText(CommonStringKeys.MINUS, Color.red);
            ui.SetButton(delegate { RemoveOp(tmp); });
            new UIElementBorder(ui, Color.red);
        }
        return offset + 1;
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
        // Remove all not allowed characters from name
        string newName = System.Text.RegularExpressions.Regex.Replace(rename.value, "[^A-Za-z0-9_]", "");
        // Must have a name
        if (newName == string.Empty) return;
        // Add type
        string baseName = component.typeDynamic + newName;
        // Find first available unique name
        string name = baseName;
        // If nothing has changed, skip renaming
        if (component.sectionName.Equals(baseName, System.StringComparison.Ordinal)) return;
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
        UIWindowSelectionList select = new UIWindowSelectionList(SelectSource, new StringKey("val", "SELECT", CommonStringKeys.FILE));

        select.AddItem("{NEW:File}", true);
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
            sourceFileText = new QuestEditorTextEdit(CommonStringKeys.FILE, "", delegate { NewSource(); });
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

    public void AddTestOp()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(delegate (string s) { SelectAddOp(s); }, new StringKey("val", "SELECT", VAR));

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { "Quest" });
        select.AddItem("{" + CommonStringKeys.NEW.Translate() + "}", "{NEW}", traits, true);

        AddQuestVars(select);

        traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { "#" });

        select.AddItem("#monsters", traits);
        select.AddItem("#heroes", traits);
        select.AddItem("#round", traits);
        select.AddItem("#eliminated", traits);
        foreach (ContentData.ContentPack pack in Game.Get().cd.allPacks)
        {
            if (pack.id.Length > 0)
            {
                select.AddItem("#" + pack.id, traits);
            }
        }
        foreach (HeroData hero in Game.Get().cd.Values<HeroData>())
        {
            if (hero.sectionName.Length > 0)
            {
                select.AddItem("#" + hero.sectionName, traits);
            }
        }
        select.Draw();
    }

    public void AddAssignOp()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(delegate (string s) { SelectAddOp(s, false); }, new StringKey("val", "SELECT", VAR));

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { "Quest" });
        select.AddItem("{" + CommonStringKeys.NEW.Translate() + "}", "{NEW}", traits, true);

        AddQuestVars(select);

        select.Draw();
    }

    public void AddQuestVars(UIWindowSelectionListTraits list)
    {
        HashSet<string> vars = new HashSet<string>();
        HashSet<string> dollarVars = new HashSet<string>();

        Game game = Game.Get();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                QuestData.Event e = kv.Value as QuestData.Event;
                foreach (string s in ExtractVarsFromEvent(e))
                {
                    if (s[0] != '$')
                    {
                        vars.Add(s);
                    }
                }
            }
        }

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { "Quest" });
        foreach (string s in vars)
        {
            list.AddItem(s, traits);
        }


        foreach (PerilData e in game.cd.Values<PerilData>())
        {
            foreach (string s in ExtractVarsFromEvent(e))
            {
                if (s[0] == '$')
                {
                    dollarVars.Add(s);
                }
            }
        }

        traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { "$" });
        foreach (string s in dollarVars)
        {
            list.AddItem(s, traits);
        }
    }

    public static HashSet<string> ExtractVarsFromEvent(QuestData.Event e)
    {
        HashSet<string> vars = new HashSet<string>();
        foreach (VarOperation op in e.operations)
        {
            vars.Add(op.var);
            if (op.value.Length > 0 && op.value[0] != '#' && !char.IsNumber(op.value[0]) && op.value[0] != '-' && op.value[0] != '.')
            {
                vars.Add(op.value);
            }
        }

        if (e.tests == null) return vars;

        foreach (VarTestsComponent tc in e.tests.VarTestsComponents)
        {
            if (tc is VarOperation)
            {
                VarOperation op = (VarOperation)tc;
                if (op.var.Length > 0 && op.var[0] != '#')
                {
                    vars.Add(op.var);
                }
                if (op.value.Length > 0 && op.value[0] != '#' && !char.IsNumber(op.value[0]) && op.value[0] != '-' && op.value[0] != '.')
                {
                    vars.Add(op.value);
                }
            }
        }
        return vars;
    }

    public void SelectAddParenthesis(bool test = true)
    {
        component.tests.Add(new VarTestsParenthesis(")"));
        component.tests.Add(new VarTestsParenthesis("("));
        Update();
    }

    public void SelectAddOp(string var, bool test = true)
    {
        VarOperation op = new VarOperation();
        op.var = var;
        op.operation = "=";
        if (test)
        {
            op.operation = ">";
        }
        op.value = "0";

        if (op.var.Equals("{NEW}"))
        {
            // Var name doesn localize
            varText = new QuestEditorTextEdit(VAR_NAME, "", delegate { NewVar(op, test); });
            varText.EditText();
        }
        else
        {
            if (test)
            {
                if (component.tests.VarTestsComponents.Count == 0)
                {
                    component.tests.Add(op);
                }
                else
                {
                    component.tests.Add(new VarTestsLogicalOperator());
                    component.tests.Add(op);
                }
            }
            else
            {
                component.operations.Add(op);
            }
            Update();
        }
    }

    public void NewVar(VarOperation op, bool test)
    {
        op.var = System.Text.RegularExpressions.Regex.Replace(varText.value, "[^A-Za-z0-9_]", "");
        if (op.var.Length > 0)
        {
            if (varText.value[0] == '%')
            {
                op.var = '%' + op.var;
            }
            if (varText.value[0] == '@')
            {
                op.var = '@' + op.var;
            }
            if (char.IsNumber(op.var[0]) || op.var[0] == '-' || op.var[0] == '.')
            {
                op.var = "var" + op.var;
            }
            if (test)
            {
                if (component.tests.VarTestsComponents.Count == 0)
                {
                    component.tests.Add(op);
                }
                else
                {
                    component.tests.Add(new VarTestsLogicalOperator());
                    component.tests.Add(op);
                }
            }
            else
            {
                component.operations.Add(op);
            }
        }
        Update();
    }

    public void SetTestOpp(VarOperation op)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionList select = new UIWindowSelectionList(delegate (string s) { SelectSetOp(op, s); }, new StringKey("val", "SELECT", OP));

        select.AddItem("==");
        select.AddItem("!=");
        select.AddItem(">=");
        select.AddItem("<=");
        select.AddItem(">");
        select.AddItem("<");

        select.Draw();
    }

    public void SetAssignOpp(VarOperation op)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionList select = new UIWindowSelectionList(delegate (string s) { SelectSetOp(op, s); }, new StringKey("val", "SELECT", OP));

        select.AddItem("=");
        select.AddItem("+");
        select.AddItem("-");
        select.AddItem("*");
        select.AddItem("/");
        select.AddItem("%");

        select.Draw();
    }

    public void SelectSetOp(VarOperation op, string operation)
    {
        op.operation = operation;
        Update();
    }

    public void SetValue(VarOperation op)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(delegate (string s) { SelectSetValue(op, s); }, new StringKey("val", "SELECT", VALUE));

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { "Quest" });
        select.AddItem("{" + CommonStringKeys.NUMBER.Translate() + "}", "{NUMBER}", traits, true);

        AddQuestVars(select);

        traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { "#" });

        select.AddItem("#monsters", traits);
        select.AddItem("#heroes", traits);
        select.AddItem("#round", traits);
        select.AddItem("#eliminated", traits);
        foreach (ContentData.ContentPack pack in Game.Get().cd.allPacks)
        {
            if (pack.id.Length > 0)
            {
                select.AddItem("#" + pack.id, traits);
            }
        }
        select.Draw();
    }


    public void SelectSetValue(VarOperation op, string value)
    {
        if (value.Equals("{NUMBER}"))
        {
            // Vars doesnt localize
            varText = new QuestEditorTextEdit(
                new StringKey("val", "X_COLON", NUMBER),
                "", delegate { SetNumValue(op); });
            varText.EditText();
        }
        else
        {
            op.value = value;
            Update();
        }
    }

    public void SetNumValue(VarOperation op)
    {

        if (varText.value.StartsWith("#rand"))
        {
            // rand integer value

            string randLimit = varText.value.Substring(5);
            int value;
            int.TryParse(randLimit, out value);

            // The minimal random number is 1
            if (value == 0)
            {
                value = 1;
            }

            op.value = "#rand" + value.ToString();

        }
        else
        {
            // float value
            float value;
            float.TryParse(varText.value, out value);
            op.value = value.ToString();
        }
        Update();
    }

    // only tests element are removed by index
    public void RemoveOp(int index)
    {
        if (index < component.tests.VarTestsComponents.Count)
            component.tests.Remove(index);
        Update();
    }

    // only operations element are removed by operation
    public void RemoveOp(VarOperation op)
    {
        if (component.operations.Contains(op))
            component.operations.Remove(op);
        Update();
    }
}