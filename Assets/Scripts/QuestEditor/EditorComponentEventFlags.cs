using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorComponentEventFlags : EditorComponent
{
    QuestData.Event eventComponent;
    EditorSelectionList flagsESL;
    QuestEditorTextEdit newFlagText;

    public EditorComponentEventFlags(string nameIn) : base()
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
        DialogBox db = new DialogBox(new Vector2(0, offset), new Vector2(5, 1), "Flags:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(5, offset), new Vector2(1, 1), "+", delegate { AddFlag("flag"); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(7, offset), new Vector2(5, 1), "Set:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(12, offset), new Vector2(1, 1), "+", delegate { AddFlag("set"); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(14, offset), new Vector2(5, 1), "Clear:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, offset++), new Vector2(1, 1), "+", delegate { AddFlag("clear"); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        int index;
        for (index = 0; index < 8; index++)
        {
            if (eventComponent.flags.Length > index)
            {
                int i = index;
                db = new DialogBox(new Vector2(0, offset + index), new Vector2(5, 1), eventComponent.flags[index]);
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(5, offset + index), new Vector2(1, 1), "-", delegate { FlagRemove(i); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        for (index = 0; index < 8; index++)
        {
            if (eventComponent.setFlags.Length > index)
            {
                int i = index;
                db = new DialogBox(new Vector2(7, offset + index), new Vector2(5, 1), eventComponent.setFlags[index]);
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(12, offset + index), new Vector2(1, 1), "-", delegate { FlagSetRemove(i); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        for (index = 0; index < 8; index++)
        {
            if (eventComponent.clearFlags.Length > index)
            {
                int i = index;
                db = new DialogBox(new Vector2(14, offset + index), new Vector2(5, 1), eventComponent.clearFlags[index]);
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(19, offset + index), new Vector2(1, 1), "-", delegate { FlagClearRemove(i); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        if (eventComponent.locationSpecified)
        {
            game.tokenBoard.AddHighlight(eventComponent.location, "EventLoc", "editor");
        }
    }

    public void AddFlag(string type)
    {
        HashSet<string> flags = new HashSet<string>();
        flags.Add("{NEW}");

        Game game = Game.Get();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                QuestData.Event e = kv.Value as QuestData.Event;
                foreach (string s in e.flags)
                {
                    if (s.IndexOf("#") != 0) flags.Add(s);
                }
                foreach (string s in e.setFlags) flags.Add(s);
                foreach (string s in e.clearFlags) flags.Add(s);
            }
        }

        if (type.Equals("flag"))
        {
            flags.Add("#monsters");
            flags.Add("#2hero");
            flags.Add("#3hero");
            flags.Add("#4hero");
            flags.Add("#5hero");
            foreach (ContentData.ContentPack pack in Game.Get().cd.allPacks)
            {
                if (pack.id.Length > 0)
                {
                    flags.Add("#" + pack.id);
                }
            }
        }

        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        foreach (string s in flags)
        {
            if (type.Equals("flag"))
            {
                if (s.Length > 0 && s[0] == '#')
                {
                    list.Add(new EditorSelectionList.SelectionListEntry(s, "Valkyrie"));
                }
                else
                {
                    list.Add(new EditorSelectionList.SelectionListEntry(s, "User"));
                }
            }
            else
            {
                list.Add(new EditorSelectionList.SelectionListEntry(s));
            }
        }
        flagsESL = new EditorSelectionList("Select Flag", list, delegate { SelectAddFlag(type); });
        flagsESL.SelectItem();
    }

    public void SelectAddFlag(string type)
    {
        if (flagsESL.selection.Equals("{NEW}"))
        {
            newFlagText = new QuestEditorTextEdit("Flag Name:", "", delegate { NewFlag(type); });
            newFlagText.EditText();
        }
        else
        {
            SelectAddFlag(type, flagsESL.selection);
        }
    }

    public void SelectAddFlag(string type, string name)
    {
        if (name.Equals("")) return;
        if (type.Equals("flag"))
        {
            System.Array.Resize(ref eventComponent.flags, eventComponent.flags.Length + 1);
            eventComponent.flags[eventComponent.flags.Length - 1] = name;
        }

        if (type.Equals("set"))
        {
            System.Array.Resize(ref eventComponent.setFlags, eventComponent.setFlags.Length + 1);
            eventComponent.setFlags[eventComponent.setFlags.Length - 1] = name;
        }

        if (type.Equals("clear"))
        {
            System.Array.Resize(ref eventComponent.clearFlags, eventComponent.clearFlags.Length + 1);
            eventComponent.clearFlags[eventComponent.clearFlags.Length - 1] = name;
        }
        Update();
    }

    public void NewFlag(string type)
    {
        string name = System.Text.RegularExpressions.Regex.Replace(newFlagText.value, "[^A-Za-z0-9_]", "");
        SelectAddFlag(type, name);
    }

    public void FlagRemove(int index)
    {
        string[] flags = new string[eventComponent.flags.Length - 1];
        int j = 0;
        for (int i = 0; i < eventComponent.flags.Length; i++)
        {
            if (i != index)
            {
                flags[j++] = eventComponent.flags[i];
            }
        }
        eventComponent.flags = flags;
        Update();
    }

    public void FlagSetRemove(int index)
    {
        string[] flags = new string[eventComponent.setFlags.Length - 1];
        int j = 0;
        for (int i = 0; i < eventComponent.setFlags.Length; i++)
        {
            if (i != index)
            {
                flags[j++] = eventComponent.setFlags[i];
            }
        }
        eventComponent.setFlags = flags;
        Update();
    }

    public void FlagClearRemove(int index)
    {
        string[] flags = new string[eventComponent.clearFlags.Length - 1];
        int j = 0;
        for (int i = 0; i < eventComponent.clearFlags.Length; i++)
        {
            if (i != index)
            {
                flags[j++] = eventComponent.clearFlags[i];
            }
        }
        eventComponent.clearFlags = flags;
        Update();
    }
}
