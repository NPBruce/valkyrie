using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorComponentEventVars : EditorComponent
{
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
        if (eventComponent is QuestData.Monster)
        {
            type = QuestData.Monster.type;
        }
        if (eventComponent is QuestData.Token)
        {
            type = QuestData.Token.type;
        }

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), type, delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1), name.Substring(type.Length), delegate { QuestEditorData.ListEvent(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");
        
        float offset = 2;
        DialogBox db = new DialogBox(new Vector2(0, offset), new Vector2(19, 1), "Tests:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, offset), new Vector2(1, 1), "+", delegate { AddTestOp(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        offset++;
        foreach (QuestData.Event.VarOperation op in eventComponent.operations)
        {
            if (!(op.operation.Equals("+") || op.operation.Equals("-") || op.operation.Equals("=")))
            {
                QuestData.Event.VarOperation tmp = op;
                db = new DialogBox(new Vector2(0, offset), new Vector2(9, 1), op.var);
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(9, offset), new Vector2(2, 1), op.operation, delegate { SetTestOpp(tmp); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
                tb = new TextButton(new Vector2(11, offset), new Vector2(8, 1), op.value, delegate { SetValue(tmp); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
                tb = new TextButton(new Vector2(19, offset), new Vector2(1, 1), "-", delegate { RemoveOp(tmp); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
                offset++;
            }
        }

        offset++;
        db = new DialogBox(new Vector2(0, offset), new Vector2(19, 1), "Assign:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, offset), new Vector2(1, 1), "+", delegate { AddAssignOp(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        offset++;
        foreach (QuestData.Event.VarOperation op in eventComponent.operations)
        {
            if (op.operation.Equals("+")
                || op.operation.Equals("-")
                || op.operation.Equals("*")
                || op.operation.Equals("/")
                || op.operation.Equals("="))
            {
                QuestData.Event.VarOperation tmp = op;
                db = new DialogBox(new Vector2(0, offset), new Vector2(9, 1), op.var);
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(9, offset), new Vector2(2, 1), op.operation, delegate { SetAssignOpp(tmp); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
                tb = new TextButton(new Vector2(11, offset), new Vector2(8, 1), op.value, delegate { SetValue(tmp); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
                tb = new TextButton(new Vector2(19, offset), new Vector2(1, 1), "-", delegate { RemoveOp(tmp); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
                offset++;
            }
        }

        if (eventComponent.locationSpecified)
        {
            game.tokenBoard.AddHighlight(eventComponent.location, "EventLoc", "editor");
        }
    }

    public void AddTestOp()
    {
        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        list.Add(new EditorSelectionList.SelectionListEntry("{NEW}"));
        foreach (string s in GetQuestVars())
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s));
        }
        varESL = new EditorSelectionList("Select Var", list, delegate { SelectAddOp(); });
        varESL.SelectItem();
    }

    public void AddAssignOp()
    {
        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        list.Add(new EditorSelectionList.SelectionListEntry("{NEW}", "Quest"));
        foreach (string s in GetQuestVars())
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s, "Quest"));
        }

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
        varESL = new EditorSelectionList("Select Var", list, delegate { SelectAddOp(false); });
        varESL.SelectItem();
    }

    public HashSet<string> GetQuestVars()
    {
        HashSet<string> vars = new HashSet<string>();

        Game game = Game.Get();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                QuestData.Event e = kv.Value as QuestData.Event;
                foreach (QuestData.Event.VarOperation op in eventComponent.operations)
                {
                    if (op.var.Length > 0 && op.var[0] != '#')
                    {
                        vars.Add(op.var);
                    }
                    if (op.value.Length > 0 && op.value[0] != '#' && !char.IsNumber(op.value[0]))
                    {
                        vars.Add(op.var);
                    }
                }
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
            op.operation = "==";
        }
        op.value = "0";

        if (op.var.Equals("{NEW}"))
        {
            varText = new QuestEditorTextEdit("Var Name:", "", delegate { NewVar(op); });
            varText.EditText();
        }
        else
        {
            eventComponent.operations.Add(op);
            Update();
        }
    }

    public void NewVar(QuestData.Event.VarOperation op)
    {
        op.var = System.Text.RegularExpressions.Regex.Replace(varText.value, "[^A-Za-z0-9_]", "");
        if (op.var.Length > 0)
        {
            eventComponent.operations.Add(op);
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
        varESL = new EditorSelectionList("Select Op", list, delegate { SelectSetOp(op); });
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
        varESL = new EditorSelectionList("Select Op", list, delegate { SelectSetOp(op); });
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
        list.Add(new EditorSelectionList.SelectionListEntry("{NUMBER}", "Quest"));
        foreach (string s in GetQuestVars())
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s, "Quest"));
        }

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

        varESL = new EditorSelectionList("Select Value", list, delegate { SelectSetValue(op); });
        varESL.SelectItem();
    }


    public void SelectSetValue(QuestData.Event.VarOperation op)
    {
        op.value = varESL.selection;

        if (op.var.Equals("{NUMBER}"))
        {
            varText = new QuestEditorTextEdit("Number:", "", delegate { SetNumValue(op); });
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
        eventComponent.operations.Remove(op);
        Update();
    }
}
