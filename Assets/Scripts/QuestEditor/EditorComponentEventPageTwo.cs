using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorComponentEventPageTwo : EditorComponent
{
    QuestData.Event eventComponent;
    List<DialogBoxEditable> buttonDBE;
    List<DialogBoxEditable> delayedEventsDBE;
    EditorSelectionList addEventESL;
    EditorSelectionList delayedEventESL;
    EditorSelectionList flagsESL;
    QuestEditorTextEdit newFlagText;

    public EditorComponentEventPageTwo(string nameIn) : base()
    {
        Game game = Game.Get();
        eventComponent = game.quest.qd.components[nameIn] as QuestData.Event;
        component = eventComponent;
        name = component.name;
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

        DialogBox db = new DialogBox(new Vector2(0, 1), new Vector2(6, 1), "Selection:");
        db.ApplyTag("editor");

        string randomButton = "Ordered";
        if (eventComponent.randomEvents) randomButton = "Random";
        tb = new TextButton(new Vector2(6, 1), new Vector2(3, 1), randomButton, delegate { ToggleRandom(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(10, 1), new Vector2(9, 1), "Buttons:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 1), new Vector2(1, 1), "+", delegate { AddButton(0); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        int offset = 2;
        int button = 1;
        int index = 0;
        buttonDBE = new List<DialogBoxEditable>();
        foreach (List<string> l in eventComponent.nextEvent)
        {
            int buttonTmp = button++;
            string buttonLabel = eventComponent.buttons[buttonTmp - 1];

            DialogBoxEditable buttonEdit = new DialogBoxEditable(new Vector2(2, offset), new Vector2(15, 1), buttonLabel, delegate { UpdateButtonLabel(buttonTmp); });
            buttonEdit.ApplyTag("editor");
            buttonEdit.AddBorder();
            buttonDBE.Add(buttonEdit);

            tb = new TextButton(new Vector2(17, offset++), new Vector2(1, 1), "-", delegate { RemoveButton(buttonTmp); }, Color.red);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");

            index = 0;
            foreach (string s in l)
            {
                int i = index++;
                tb = new TextButton(new Vector2(0, offset), new Vector2(1, 1), "+", delegate { AddEvent(i, buttonTmp); }, Color.green);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
                db = new DialogBox(new Vector2(1, offset), new Vector2(17, 1), s);
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(18, offset++), new Vector2(1, 1), "-", delegate { RemoveEvent(i, buttonTmp); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
            int tmp = index;
            tb = new TextButton(new Vector2(0, offset), new Vector2(1, 1), "+", delegate { AddEvent(tmp, buttonTmp); }, Color.green);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");

            tb = new TextButton(new Vector2(19, offset++), new Vector2(1, 1), "+", delegate { AddButton(buttonTmp); }, Color.green);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
            offset++;
        }

        offset++;
        db = new DialogBox(new Vector2(1, offset), new Vector2(10, 1), "Delayed Events:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, offset++), new Vector2(1, 1), "+", delegate { AddDelayedEvent(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        index = 0;
        delayedEventsDBE = new List<DialogBoxEditable>();
        foreach (QuestData.Event.DelayedEvent de in eventComponent.delayedEvents)
        {
            int i = index++;

            DialogBoxEditable dbeDelay = new DialogBoxEditable(new Vector2(0, offset), new Vector2(2, 1), de.delay.ToString(), delegate { SetDelayedEvent(i); });
            dbeDelay.ApplyTag("editor");
            dbeDelay.AddBorder();
            delayedEventsDBE.Add(dbeDelay);

            db = new DialogBox(new Vector2(2, offset), new Vector2(17, 1), de.eventName);
            db.AddBorder();
            db.ApplyTag("editor");
            tb = new TextButton(new Vector2(19, offset++), new Vector2(1, 1), "-", delegate { RemoveDelayedEvent(i); }, Color.red);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
        }

        offset++;
        db = new DialogBox(new Vector2(0, offset), new Vector2(5, 1), "Flags:");
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

    public void ToggleRandom()
    {
        eventComponent.randomEvents = !eventComponent.randomEvents;
        Update();
    }

    public void AddButton(int number)
    {
        eventComponent.nextEvent.Insert(number, new List<string>());
        if (eventComponent.nextEvent.Count == 1)
        {
            eventComponent.buttons.Add("Confirm");
        }
        else if (eventComponent.nextEvent.Count == 2 && eventComponent.buttons[0].Equals("Confirm"))
        {
            eventComponent.buttons[0] = "Pass";
            eventComponent.buttons.Add("Fail");
        }
        else
        {
            eventComponent.buttons.Insert(number, "Button " + (number + 1));
        }
        Update();
    }

    public void RemoveButton(int number)
    {
        eventComponent.nextEvent.RemoveAt(number - 1);
        eventComponent.buttons.RemoveAt(number - 1);
        Update();
    }

    public void UpdateButtonLabel(int number)
    {
        eventComponent.buttons[number - 1] = buttonDBE[number - 1].uiInput.text;
    }

    public void AddEvent(int index, int button)
    {
        List<string> events = new List<string>();

        Game game = Game.Get();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                events.Add(kv.Key);
            }
        }

        addEventESL = new EditorSelectionList("Select Event", events, delegate { SelectAddEvent(index, button); });
        addEventESL.SelectItem();
    }

    public void SelectAddEvent(int index, int button)
    {
        eventComponent.nextEvent[button - 1].Insert(index, addEventESL.selection);
        Update();
    }

    public void RemoveEvent(int index, int button)
    {
        eventComponent.nextEvent[button - 1].RemoveAt(index);
        Update();
    }

    public void AddDelayedEvent()
    {
        List<string> events = new List<string>();

        Game game = Game.Get();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                events.Add(kv.Key);
            }
        }

        delayedEventESL = new EditorSelectionList("Select Event", events, delegate { SelectAddDelayedEvent(); });
        delayedEventESL.SelectItem();
    }

    public void SelectAddDelayedEvent()
    {
        eventComponent.delayedEvents.Add(new QuestData.Event.DelayedEvent(0, delayedEventESL.selection));
        Update();
    }

    public void SetDelayedEvent(int i)
    {
        int.TryParse(delayedEventsDBE[i].uiInput.text, out eventComponent.delayedEvents[i].delay);
        Update();
    }

    public void RemoveDelayedEvent(int i)
    {
        eventComponent.delayedEvents.RemoveAt(i);
        Update();
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

        List<string> list = new List<string>(flags);
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
