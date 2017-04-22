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
    EditorSelectionList colorESL;

    private readonly StringKey QUOTA = new StringKey("val","QUOTA");
    private readonly StringKey BUTTONS = new StringKey("val","BUTTONS");

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

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), 
            new StringKey(null,type,false), 
            delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1), 
            new StringKey(null,name.Substring(type.Length),false), 
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
        tb = new TextButton(new Vector2(0, 1), new Vector2(3, 1), new StringKey("val",randomButton), delegate { ToggleRandom(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        DialogBox db = new DialogBox(new Vector2(3, 1), new Vector2(3, 1), new StringKey("val","X_COLON",QUOTA));
        db.ApplyTag("editor");

        // Quota dont need translation
        quotaDBE = new DialogBoxEditable(
            new Vector2(6, 1), new Vector2(2, 1),
            eventComponent.quota.ToString(), delegate { SetQuota(); });
        quotaDBE.ApplyTag("editor");
        quotaDBE.AddBorder();

        db = new DialogBox(new Vector2(8, 1), new Vector2(11, 1), new StringKey("val","X_COLON",BUTTONS));
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 1), new Vector2(1, 1),
            CommonStringKeys.PLUS, delegate { AddButton(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        int offset = 2;
        int button = 1;
        int index = 0;
        int lastButtonOffset = 0;
        buttonDBE = new List<DialogBoxEditable>();
        foreach (List<string> l in eventComponent.nextEvent)
        {
            lastButtonOffset = offset;
            int buttonTmp = button++;

            StringKey buttonLabel = eventComponent.buttons[buttonTmp - 1];
            string colorRGB = ColorUtil.FromName(eventComponent.buttonColors[buttonTmp - 1]);
            Color c = Color.white;
            c[0] = (float)System.Convert.ToInt32(colorRGB.Substring(1, 2), 16) / 255f;
            c[1] = (float)System.Convert.ToInt32(colorRGB.Substring(3, 2), 16) / 255f;
            c[2] = (float)System.Convert.ToInt32(colorRGB.Substring(5, 2), 16) / 255f;

            tb = new TextButton(new Vector2(0, offset), new Vector2(3, 1),
                new StringKey("val", "COLOR"), delegate { SetButtonColor(buttonTmp); }, c);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");

            DialogBoxEditable buttonEdit = new DialogBoxEditable(
                new Vector2(3, offset++), new Vector2(16, 1), 
                buttonLabel.Translate(),
                delegate { UpdateButtonLabel(buttonTmp); });

            buttonEdit.ApplyTag("editor");
            buttonEdit.AddBorder();
            buttonDBE.Add(buttonEdit);

            index = 0;
            foreach (string s in l)
            {
                int i = index++;
                tb = new TextButton(new Vector2(0, offset), new Vector2(1, 1),
                    CommonStringKeys.PLUS, delegate { AddEvent(i, buttonTmp); }, Color.green);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
                db = new DialogBox(new Vector2(1, offset), new Vector2(18, 1), 
                    new StringKey(null,s,false));
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(19, offset++), new Vector2(1, 1),
                    CommonStringKeys.MINUS, delegate { RemoveEvent(i, buttonTmp); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
            int tmp = index;
            tb = new TextButton(new Vector2(0, offset), new Vector2(1, 1),
                CommonStringKeys.PLUS, delegate { AddEvent(tmp, buttonTmp); }, Color.green);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
            offset++;
        }

        if (lastButtonOffset != 0)
        {
            tb = new TextButton(new Vector2(19, lastButtonOffset), new Vector2(1, 1),
                CommonStringKeys.MINUS, delegate { RemoveButton(); }, Color.red);
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
        int.TryParse(quotaDBE.Text, out eventComponent.quota);
        Update();
    }

    public void AddButton()
    {
        int count = eventComponent.nextEvent.Count + 1;
        eventComponent.nextEvent.Add(new List<string>());
        eventComponent.buttons.Add(eventComponent.genQuery("button" + count));
        eventComponent.buttonColors.Add("white");
        LocalizationRead.updateScenarioText(eventComponent.genKey("button" + count), "Button " + count);
        Update();
    }

    public void RemoveButton()
    {
        int count = eventComponent.nextEvent.Count;
        eventComponent.nextEvent.RemoveAt(count - 1);
        eventComponent.buttons.RemoveAt(count - 1);
        eventComponent.buttonColors.RemoveAt(count - 1);
        LocalizationRead.scenarioDict.Remove(eventComponent.genKey("button" + count));
        Update();
    }

    public void SetButtonColor(int number)
    {
        List<EditorSelectionList.SelectionListEntry> colours = new List<EditorSelectionList.SelectionListEntry>();
        foreach (KeyValuePair<string, string> kv in ColorUtil.LookUp())
        {
            colours.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(kv.Key));
        }
        colorESL = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, colours, delegate { SelectButtonColour(number); });
        colorESL.SelectItem();
    }

    public void SelectButtonColour(int number)
    {
        eventComponent.buttonColors[number - 1] = colorESL.selection;
        Update();
    }

    public void UpdateButtonLabel(int number)
    {
        if (!buttonDBE[number - 1].Text.Equals(""))
        {
            LocalizationRead.updateScenarioText(eventComponent.genKey("button" + number), buttonDBE[number - 1].Text);
        }
    }

    public void AddEvent(int index, int button)
    {
        List<EditorSelectionList.SelectionListEntry> events = new List<EditorSelectionList.SelectionListEntry>();

        Game game = Game.Get();
        events.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(
            new StringKey("val","NEW_X",CommonStringKeys.EVENT).Translate(),"{NEW:Event}"));
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
}
