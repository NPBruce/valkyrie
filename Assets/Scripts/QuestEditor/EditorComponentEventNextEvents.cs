using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentEventNextEvent : EditorComponent
{
    QuestData.Event eventComponent;
    List<DialogBoxEditable> buttonDBE;
    DialogBoxEditable quotaDBE;
    EditorSelectionList addEventESL;

    private readonly StringKey QUOTA = new StringKey("val","QUOTA");
    private readonly StringKey BUTTONS = new StringKey("val","BUTTONS");
    private readonly StringKey DELAYED_EVENTS = new StringKey("val", "DELAYED_EVENTS");

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
        if (eventComponent is QuestData.Spawn)
        {
            type = QuestData.Spawn.type;
        }
        if (eventComponent is QuestData.Token)
        {
            type = QuestData.Token.type;
        }

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), new StringKey(type,false), 
            delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1), 
            new StringKey(name.Substring(type.Length),false), 
            delegate { QuestEditorData.ListEvent(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1),
            CommonStringKeys.E, delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        string randomButton = "Ordered";
        if (eventComponent.randomEvents) randomButton = "Random";
        tb = new TextButton(new Vector2(0, 1), new Vector2(3, 1), new StringKey(randomButton,false), delegate { ToggleRandom(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        DialogBox db = new DialogBox(new Vector2(3, 1), new Vector2(3, 1), new StringKey("val","X_COLON",QUOTA));
        db.ApplyTag("editor");

        quotaDBE = new DialogBoxEditable(new Vector2(6, 1), new Vector2(2, 1), eventComponent.quota.ToString(), delegate { SetQuota(); });
        quotaDBE.ApplyTag("editor");
        quotaDBE.AddBorder();

        db = new DialogBox(new Vector2(8, 1), new Vector2(11, 1), new StringKey("val","X_COLON",BUTTONS));
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 1), new Vector2(1, 1),
            CommonStringKeys.PLUS, delegate { AddButton(0); }, Color.green);
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

            tb = new TextButton(new Vector2(17, offset++), new Vector2(1, 1),
                CommonStringKeys.MINUS, delegate { RemoveButton(buttonTmp); }, Color.red);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");

            index = 0;
            foreach (string s in l)
            {
                int i = index++;
                tb = new TextButton(new Vector2(0, offset), new Vector2(1, 1),
                    CommonStringKeys.PLUS, delegate { AddEvent(i, buttonTmp); }, Color.green);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
                db = new DialogBox(new Vector2(1, offset), new Vector2(17, 1), new StringKey(s,false));
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(18, offset++), new Vector2(1, 1),
                    CommonStringKeys.MINUS, delegate { RemoveEvent(i, buttonTmp); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
            int tmp = index;
            tb = new TextButton(new Vector2(0, offset), new Vector2(1, 1),
                CommonStringKeys.PLUS, delegate { AddEvent(tmp, buttonTmp); }, Color.green);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");

            tb = new TextButton(new Vector2(19, offset++), new Vector2(1, 1), 
                CommonStringKeys.PLUS, delegate { AddButton(buttonTmp); }, Color.green);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
            offset++;
        }

        if (eventComponent.delayedEvents.Count > 0)
        {
            db = new DialogBox(new Vector2(0, offset++), new Vector2(20, 1), DELAYED_EVENTS, Color.red);
            db.ApplyTag("editor");

            index = 0;
            foreach (QuestData.Event.DelayedEvent de in eventComponent.delayedEvents)
            {
                int i = index++;
                db = new DialogBox(new Vector2(0, offset), new Vector2(2, 1), 
                    new StringKey(de.delay.ToString(),false));
                db.AddBorder();
                db.ApplyTag("editor");

                db = new DialogBox(new Vector2(2, offset), new Vector2(17, 1), 
                    new StringKey(de.eventName,false));
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(19, offset++), new Vector2(1, 1), 
                    CommonStringKeys.MINUS, delegate { RemoveDelayedEvent(i); }, Color.red);
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
        events.Add(new EditorSelectionList.SelectionListEntry("{NEW:Event}"));
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                events.Add(new EditorSelectionList.SelectionListEntry(kv.Key, kv.Value.typeDynamic));
            }
        }

        addEventESL = new EditorSelectionList(new StringKey("val","SELECT",CommonStringKeys.EVENT), 
            events, delegate { SelectAddEvent(index, button); });
        addEventESL.SelectItem();
    }

    public void SelectAddEvent(int index, int button)
    {
        string toAdd = addEventESL.selection;
        if (addEventESL.selection.Equals("{NEW:Event}"))
        {
            int i = 0;
            while (Game.Get().quest.qd.components.ContainsKey("Event" + i))
            {
                i++;
            }
            toAdd = "Event" + i;
            Game.Get().quest.qd.components.Add(toAdd, new QuestData.Event(toAdd));
        }
        eventComponent.nextEvent[button - 1].Insert(index, toAdd);
        Update();
    }

    public void RemoveEvent(int index, int button)
    {
        eventComponent.nextEvent[button - 1].RemoveAt(index);
        Update();
    }

    public void RemoveDelayedEvent(int i)
    {
        eventComponent.delayedEvents.RemoveAt(i);
        Update();
    }
}
