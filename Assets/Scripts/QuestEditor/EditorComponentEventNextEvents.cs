using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorComponentEventNextEvent : EditorComponent
{
    QuestData.Event eventComponent;
    List<DialogBoxEditable> buttonDBE;
    List<DialogBoxEditable> delayedEventsDBE;
    DialogBoxEditable quotaDBE;
    EditorSelectionList addEventESL;
    EditorSelectionList delayedEventESL;

    public EditorComponentEventNextEvent(string nameIn) : base()
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

        string randomButton = "Ordered";
        if (eventComponent.randomEvents) randomButton = "Random";
        tb = new TextButton(new Vector2(0, 1), new Vector2(3, 1), randomButton, delegate { ToggleRandom(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        DialogBox db = new DialogBox(new Vector2(3, 1), new Vector2(3, 1), "Quota:");
        db.ApplyTag("editor");

        quotaDBE = new DialogBoxEditable(new Vector2(6, 1), new Vector2(2, 1), eventComponent.quota.ToString(), delegate { SetQuota(); });
        quotaDBE.ApplyTag("editor");
        quotaDBE.AddBorder();

        db = new DialogBox(new Vector2(8, 1), new Vector2(11, 1), "Buttons:");
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
        db = new DialogBox(new Vector2(0, offset), new Vector2(19, 1), "Delayed Events:");
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

    public void SetQuota()
    {
        int.TryParse(quotaDBE.uiInput.text, out eventComponent.quota);
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
        List<EditorSelectionList.SelectionListEntry> events = new List<EditorSelectionList.SelectionListEntry>();

        Game game = Game.Get();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                events.Add(new EditorSelectionList.SelectionListEntry(kv.Key));
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
        List<EditorSelectionList.SelectionListEntry> events = new List<EditorSelectionList.SelectionListEntry>();

        Game game = Game.Get();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                events.Add(new EditorSelectionList.SelectionListEntry(kv.Key));
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
}
