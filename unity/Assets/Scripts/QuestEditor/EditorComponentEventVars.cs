using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentEventVars : EditorComponent
{
    private readonly StringKey TESTS = new StringKey("val","TESTS");
    private readonly StringKey VAR = new StringKey("val", "VAR");
    private readonly StringKey OP = new StringKey("val", "OP");
    private readonly StringKey VALUE = new StringKey("val", "VALUE");
    private readonly StringKey ASSIGN = new StringKey("val", "ASSIGN");
    private readonly StringKey VAR_NAME = new StringKey("val", "VAR_NAME");
    private readonly StringKey NUMBER = new StringKey("val", "NUMBER");

    QuestData.Event eventComponent;
    EditorSelectionList varESL;
    QuestEditorTextEdit varText;


    public EditorComponentEventVars(string nameIn) : base()
    {
        Game game = Game.Get();
        eventComponent = game.quest.qd.components[nameIn] as QuestData.Event;
        component = eventComponent;
        name = component.sectionName;
        Update();
    }
    
    override public void Update()
    {
        base.Update();
        if (eventComponent.locationSpecified)
        {
            CameraController.SetCamera(eventComponent.location);
        }
        Game game = Game.Get();

        string type = QuestData.Event.type;
        if (eventComponent is QuestData.Door)
        {
            type = QuestData.Door.type;
        }
        if (eventComponent is QuestData.Spawn)
        {
            type = QuestData.Spawn.type;
        }
        if (eventComponent is QuestData.Token)
        {
            type = QuestData.Token.type;
        }

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), 
            new StringKey(null,type,false), delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1),
            new StringKey(null,name.Substring(type.Length),false), delegate { QuestEditorData.ListEvent(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), CommonStringKeys.E, delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");
        
        float offset = 2;
        DialogBox db = new DialogBox(new Vector2(0, offset), new Vector2(19, 1), 
            new StringKey("val","X_COLON",TESTS));
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, offset), new Vector2(1, 1), 
            CommonStringKeys.PLUS, delegate { AddTestOp(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        offset++;
        foreach (QuestData.Event.VarOperation op in eventComponent.conditions)
        {
            QuestData.Event.VarOperation tmp = op;
            db = new DialogBox(new Vector2(0, offset), new Vector2(9, 1),
                new StringKey(null,op.var,false));
            db.AddBorder();
            db.ApplyTag("editor");
            tb = new TextButton(new Vector2(9, offset), new Vector2(2, 1),
                new StringKey(null,op.operation, false), delegate { SetTestOpp(tmp); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
            tb = new TextButton(new Vector2(11, offset), new Vector2(8, 1),
                new StringKey(null,op.value, false), delegate { SetValue(tmp); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
            tb = new TextButton(new Vector2(19, offset), new Vector2(1, 1), 
                CommonStringKeys.MINUS, delegate { RemoveOp(tmp); }, Color.red);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
            offset++;
        }

        offset++;
        db = new DialogBox(new Vector2(0, offset), new Vector2(19, 1),
            new StringKey("val","X_COLON",ASSIGN));
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, offset), new Vector2(1, 1), 
            CommonStringKeys.PLUS, delegate { AddAssignOp(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        offset++;
        foreach (QuestData.Event.VarOperation op in eventComponent.operations)
        {
            QuestData.Event.VarOperation tmp = op;
            db = new DialogBox(new Vector2(0, offset), new Vector2(9, 1),
                new StringKey(null,op.var,false));
            db.AddBorder();
            db.ApplyTag("editor");
            tb = new TextButton(new Vector2(9, offset), new Vector2(2, 1),
                new StringKey(null,op.operation, false), delegate { SetAssignOpp(tmp); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
            tb = new TextButton(new Vector2(11, offset), new Vector2(8, 1),
                new StringKey(null,op.value, false), delegate { SetValue(tmp); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
            tb = new TextButton(new Vector2(19, offset), new Vector2(1, 1), 
                CommonStringKeys.MINUS, delegate { RemoveOp(tmp); }, Color.red);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
            offset++;
        }

        if (eventComponent.locationSpecified)
        {
            game.tokenBoard.AddHighlight(eventComponent.location, "EventLoc", "editor");
        }
    }

    public void AddTestOp()
    {
        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        list.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyTraitItem("{" + CommonStringKeys.NEW.Translate() + "}", "{NEW}", "Quest"));
        list.AddRange(GetQuestVars());

        list.Add(new EditorSelectionList.SelectionListEntry("#monsters", "#"));
        list.Add(new EditorSelectionList.SelectionListEntry("#heroes", "#"));
        list.Add(new EditorSelectionList.SelectionListEntry("#round", "#"));
        list.Add(new EditorSelectionList.SelectionListEntry("#fire", "#"));
        list.Add(new EditorSelectionList.SelectionListEntry("#eliminated", "#"));
        foreach (ContentData.ContentPack pack in Game.Get().cd.allPacks)
        {
            if (pack.id.Length > 0)
            {
                list.Add(new EditorSelectionList.SelectionListEntry("#" + pack.id, "#"));
            }
        }

        varESL = new EditorSelectionList(new StringKey("val", "SELECT", VAR), list, delegate { SelectAddOp(); });
        varESL.SelectItem();
    }

    public void AddAssignOp()
    {
        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        list.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyTraitItem("{" + CommonStringKeys.NEW.Translate() + "}", "{NEW}", "Quest"));
        list.AddRange(GetQuestVars());
        varESL = new EditorSelectionList(new StringKey("val", "SELECT", VAR), list, delegate { SelectAddOp(false); });
        varESL.SelectItem();
    }

    public List<EditorSelectionList.SelectionListEntry> GetQuestVars()
    {
        HashSet<string> vars = new HashSet<string>();
        HashSet<string> dollarVars = new HashSet<string>();
        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();

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
        foreach (string s in vars)
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s, "Quest"));
        }


        foreach (PerilData e in game.cd.perils.Values)
        {
            foreach (string s in ExtractVarsFromEvent(e))
            {
                if (s[0] == '$')
                {
                    dollarVars.Add(s);
                }
            }
        }
        foreach (string s in dollarVars)
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s, "$"));
        }

        return list;
    }

    private HashSet<string> ExtractVarsFromEvent(QuestData.Event e)
    {
        HashSet<string> vars = new HashSet<string>();
        foreach (QuestData.Event.VarOperation op in e.operations)
        {
            vars.Add(op.var);
            if (op.value.Length > 0 && op.value[0] != '#' && !char.IsNumber(op.value[0]) && op.value[0] != '-' && op.value[0] != '.')
            {
                vars.Add(op.value);
            }
        }
        foreach (QuestData.Event.VarOperation op in e.conditions)
        {
            if (op.var.Length > 0 && op.var[0] != '#')
            {
                vars.Add(op.var);
            }
            if (op.value.Length > 0 && op.value[0] != '#' && !char.IsNumber(op.value[0]) && op.value[0] != '-' && op.value[0] != '.')
            {
                vars.Add(op.value);
            }
        }
        return vars;
    }

    public void SelectAddOp(bool test = true)
    {
        QuestData.Event.VarOperation op = new QuestData.Event.VarOperation();
        op.var = varESL.selection;
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
                eventComponent.conditions.Add(op);
            }
            else
            {
                eventComponent.operations.Add(op);
            }
            Update();
        }
    }

    public void NewVar(QuestData.Event.VarOperation op, bool test)
    {
        op.var = System.Text.RegularExpressions.Regex.Replace(varText.value, "[^A-Za-z0-9_]", "");
        if (op.var.Length > 0)
        {
            if (char.IsNumber(op.var[0]) || op.var[0] == '-' || op.var[0] == '.')
            {
                op.var = "var" + op.var;
            }
            if (test)
            {
                eventComponent.conditions.Add(op);
            }
            else
            {
                eventComponent.operations.Add(op);
            }
        }
        Update();
    }

    public void SetTestOpp(QuestData.Event.VarOperation op)
    {
        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        list.Add(new EditorSelectionList.SelectionListEntry("=="));
        list.Add(new EditorSelectionList.SelectionListEntry("!="));
        list.Add(new EditorSelectionList.SelectionListEntry(">="));
        list.Add(new EditorSelectionList.SelectionListEntry("<="));
        list.Add(new EditorSelectionList.SelectionListEntry(">"));
        list.Add(new EditorSelectionList.SelectionListEntry("<"));
        varESL = new EditorSelectionList(new StringKey("val", "SELECT", OP), list, delegate { SelectSetOp(op); });
        varESL.SelectItem();
    }

    public void SetAssignOpp(QuestData.Event.VarOperation op)
    {
        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        list.Add(new EditorSelectionList.SelectionListEntry("="));
        list.Add(new EditorSelectionList.SelectionListEntry("+"));
        list.Add(new EditorSelectionList.SelectionListEntry("-"));
        list.Add(new EditorSelectionList.SelectionListEntry("*"));
        list.Add(new EditorSelectionList.SelectionListEntry("/"));
        varESL = new EditorSelectionList(new StringKey("val", "SELECT", OP), list, delegate { SelectSetOp(op); });
        varESL.SelectItem();
    }

    public void SelectSetOp(QuestData.Event.VarOperation op)
    {
        op.operation = varESL.selection;;
        Update();
    }

    public void SetValue(QuestData.Event.VarOperation op)
    {
        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        list.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyTraitItem("{" + CommonStringKeys.NUMBER.Translate() + "}", "{NUMBER}", "Quest"));
        list.AddRange(GetQuestVars());

        list.Add(new EditorSelectionList.SelectionListEntry("#monsters", "Valkyrie"));
        list.Add(new EditorSelectionList.SelectionListEntry("#heroes", "Valkyrie"));
        list.Add(new EditorSelectionList.SelectionListEntry("#round", "Valkyrie"));
        list.Add(new EditorSelectionList.SelectionListEntry("#fire", "Valkyrie"));
        list.Add(new EditorSelectionList.SelectionListEntry("#eliminated", "Valkyrie"));
        foreach (ContentData.ContentPack pack in Game.Get().cd.allPacks)
        {
            if (pack.id.Length > 0)
            {
                list.Add(new EditorSelectionList.SelectionListEntry("#" + pack.id, "Valkyrie"));
            }
        }

        varESL = new EditorSelectionList(new StringKey("val","SELECT",VALUE), list, delegate { SelectSetValue(op); });
        varESL.SelectItem();
    }


    public void SelectSetValue(QuestData.Event.VarOperation op)
    {
        if (varESL.selection.Equals("{NUMBER}"))
        {
            // Vars doesnt localize
            varText = new QuestEditorTextEdit(
                new StringKey("val","X_COLON",NUMBER), 
                "", delegate { SetNumValue(op); });
            varText.EditText();
        }
        else
        {
            op.value = varESL.selection;
            Update();
        }
    }

    public void SetNumValue(QuestData.Event.VarOperation op)
    {
        float value = 0;
        float.TryParse(varText.value, out value);
        op.value = value.ToString();
        Update();
    }

    public void RemoveOp(QuestData.Event.VarOperation op)
    {
        if (eventComponent.operations.Contains(op))
            eventComponent.operations.Remove(op);
        if (eventComponent.conditions.Contains(op))
            eventComponent.conditions.Remove(op);
        Update();
    }
}
