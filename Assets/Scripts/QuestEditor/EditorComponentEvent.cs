using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorComponentEvent : EditorComponent
{
    QuestData.Event eventComponent;

    DialogBoxEditable eventTextDBE;
    EditorSelectionList triggerESL;
    EditorSelectionList highlightESL;
    EditorSelectionList heroCountESL;
    EditorSelectionList visibilityESL;

    public EditorComponentEvent(string nameIn) : base()
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

        DialogBox db = new DialogBox(new Vector2(0, 2), new Vector2(4, 1), "Position");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 2), new Vector2(1, 1), "><", delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(5, 2), new Vector2(1, 1), "~", delegate { GetPosition(false); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        if (!eventComponent.GetType().IsSubclassOf(typeof(QuestData.Event)))
        {
            if (eventComponent.minCam)
            {
                tb = new TextButton(new Vector2(7, 2), new Vector2(4, 1), "Min Cam", delegate { PositionTypeCycle(); });
            }
            else if (eventComponent.maxCam)
            {
                tb = new TextButton(new Vector2(7, 2), new Vector2(4, 1), "Max Cam", delegate { PositionTypeCycle(); });
            }
            else if (!eventComponent.locationSpecified)
            {
                tb = new TextButton(new Vector2(7, 2), new Vector2(4, 1), "Unused", delegate { PositionTypeCycle(); });
            }
            else if (eventComponent.highlight)
            {
                tb = new TextButton(new Vector2(7, 2), new Vector2(4, 1), "Highlight", delegate { PositionTypeCycle(); });
            }
            else
            {
                tb = new TextButton(new Vector2(7, 2), new Vector2(4, 1), "Camera", delegate { PositionTypeCycle(); });
            }
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
        }

        tb = new TextButton(new Vector2(12, 2), new Vector2(3, 1), "Vars", delegate { QuestEditorData.SelectAsEventVars(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(16, 2), new Vector2(4, 1), "Next Events", delegate { QuestEditorData.SelectAsEventNextEvent(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 3), new Vector2(20, 1), "Dialog:");
        db.ApplyTag("editor");

        eventTextDBE = new DialogBoxEditable(new Vector2(0, 4), new Vector2(20, 8), eventComponent.originalText, delegate { UpdateText(); });
        eventTextDBE.ApplyTag("editor");
        eventTextDBE.AddBorder();

        db = new DialogBox(new Vector2(0, 12), new Vector2(4, 1), "Trigger:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 12), new Vector2(8, 1), eventComponent.trigger, delegate { SetTrigger(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 13), new Vector2(4, 1), "Selection:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 13), new Vector2(8, 1), eventComponent.heroListName, delegate { SetHighlight(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(12, 12), new Vector2(2, 1), "Min");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(12, 13), new Vector2(2, 1), eventComponent.minHeroes.ToString(), delegate { SetHeroCount(false); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(14, 12), new Vector2(2, 1), "Max");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(14, 13), new Vector2(2, 1), eventComponent.maxHeroes.ToString(), delegate { SetHeroCount(true); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 15), new Vector2(9, 1), "Add Components:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(9, 15), new Vector2(1, 1), "+", delegate { AddVisibility(true); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        int offset = 16;
        int index;
        for (index = 0; index < 12; index++)
        {
            if (eventComponent.addComponents.Length > index)
            {
                int i = index;
                db = new DialogBox(new Vector2(0, offset), new Vector2(9, 1), eventComponent.addComponents[index]);
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(9, offset++), new Vector2(1, 1), "-", delegate { RemoveVisibility(i, true); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        db = new DialogBox(new Vector2(10, 15), new Vector2(9, 1), "Remove Components:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 15), new Vector2(1, 1), "+", delegate { AddVisibility(false); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        offset = 16;
        for (index = 0; index < 12; index++)
        {
            if (eventComponent.removeComponents.Length > index)
            {
                int i = index;
                db = new DialogBox(new Vector2(10, offset), new Vector2(9, 1), eventComponent.removeComponents[index]);
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(19, offset++), new Vector2(1, 1), "-", delegate { RemoveVisibility(i, false); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        if (eventComponent.locationSpecified || eventComponent.maxCam || eventComponent.minCam)
        {
            game.tokenBoard.AddHighlight(eventComponent.location, "EventLoc", "editor");
        }
    }


    public void PositionTypeCycle()
    {
        if (eventComponent.minCam)
        {
            eventComponent.locationSpecified = false;
            eventComponent.highlight = false;
            eventComponent.maxCam = true;
            eventComponent.minCam = false;
        }
        else if (eventComponent.maxCam)
        {
            eventComponent.locationSpecified = false;
            eventComponent.highlight = false;
            eventComponent.maxCam = false;
            eventComponent.minCam = false;
        }
        else if (eventComponent.highlight)
        {
            eventComponent.locationSpecified = false;
            eventComponent.highlight = false;
            eventComponent.maxCam = false;
            eventComponent.minCam = true;
        }
        else if (eventComponent.locationSpecified)
        {
            eventComponent.locationSpecified = true;
            eventComponent.highlight = true;
            eventComponent.maxCam = false;
            eventComponent.minCam = false;
        }
        else
        {
            eventComponent.locationSpecified = true;
            eventComponent.highlight = false;
            eventComponent.maxCam = false;
            eventComponent.minCam = false;
        }
        Update();
    }

    public void UpdateText()
    {
        if (!eventTextDBE.uiInput.text.Equals(""))
        {
            eventComponent.originalText = eventTextDBE.uiInput.text;
        }
    }

    public void SetTrigger()
    {
        Game game = Game.Get();
        List<EditorSelectionList.SelectionListEntry> triggers = new List<EditorSelectionList.SelectionListEntry>();
        triggers.Add(new EditorSelectionList.SelectionListEntry("", Color.white));

        bool startPresent = false;
        bool noMorale = false;
        bool eliminated = false;
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            QuestData.Event e = kv.Value as QuestData.Event;
            if (e != null)
            {
                if (e.trigger.Equals("EventStart"))
                {
                    startPresent = true;
                }
                if (e.trigger.Equals("NoMorale"))
                {
                    noMorale = true;
                }
                if (e.trigger.Equals("Eliminated"))
                {
                    eliminated = true;
                }
            }
        }

        if (startPresent)
        {
            triggers.Add(new EditorSelectionList.SelectionListEntry("EventStart", Color.grey));
        }
        else
        {
            triggers.Add(new EditorSelectionList.SelectionListEntry("EventStart"));
        }

        if (noMorale)
        {
            triggers.Add(new EditorSelectionList.SelectionListEntry("NoMorale", Color.grey));
        }
        else
        {
            triggers.Add(new EditorSelectionList.SelectionListEntry("NoMorale"));
        }

        if (eliminated)
        {
            triggers.Add(new EditorSelectionList.SelectionListEntry("Eliminated", Color.grey));
        }
        else
        {
            triggers.Add(new EditorSelectionList.SelectionListEntry("Eliminated"));
        }

        triggers.Add(new EditorSelectionList.SelectionListEntry("Mythos"));

        triggers.Add(new EditorSelectionList.SelectionListEntry("EndRound", "Round"));

        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            triggers.Add(new EditorSelectionList.SelectionListEntry("Defeated" + kv.Key, "Defeated"));
            triggers.Add(new EditorSelectionList.SelectionListEntry("DefeatedUnique" + kv.Key, "Defeated"));
        }

        for (int i = 1; i <= 40; i++)
        {
            triggers.Add(new EditorSelectionList.SelectionListEntry("EndRound" + i, "Round"));
        }

        triggerESL = new EditorSelectionList("Select Trigger", triggers, delegate { SelectEventTrigger(); });
        triggerESL.SelectItem();
    }

    public void SelectEventTrigger()
    {
        eventComponent.trigger = triggerESL.selection;
        Update();
    }

    public void SetHighlight()
    {
        List<EditorSelectionList.SelectionListEntry> events = new List<EditorSelectionList.SelectionListEntry>();
        events.Add(new EditorSelectionList.SelectionListEntry(""));

        Game game = Game.Get();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                events.Add(new EditorSelectionList.SelectionListEntry(kv.Key, kv.Value.typeDynamic));
            }
        }

        highlightESL = new EditorSelectionList("Select Event", events, delegate { SelectEventHighlight(); });
        highlightESL.SelectItem();
    }

    public void SelectEventHighlight()
    {
        eventComponent.heroListName = highlightESL.selection;
        Update();
    }

    public void SetHeroCount(bool max)
    {
        List<EditorSelectionList.SelectionListEntry> num = new List<EditorSelectionList.SelectionListEntry>();
        for (int i = 0; i <= Game.Get().gameType.MaxHeroes(); i++)
        {
            num.Add(new EditorSelectionList.SelectionListEntry(i.ToString()));
        }

        heroCountESL = new EditorSelectionList("Select Number", num, delegate { SelectEventHeroCount(max); });
        heroCountESL.SelectItem();
    }

    public void SelectEventHeroCount(bool max)
    {
        if (max)
        {
            int.TryParse(heroCountESL.selection, out eventComponent.maxHeroes);
        }
        else
        {
            int.TryParse(heroCountESL.selection, out eventComponent.minHeroes);
        }
        Update();
    }

    public void AddVisibility(bool add)
    {
        List<EditorSelectionList.SelectionListEntry> components = new List<EditorSelectionList.SelectionListEntry>();

        Game game = Game.Get();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Door || kv.Value is QuestData.Tile || kv.Value is QuestData.Token)
            components.Add(new EditorSelectionList.SelectionListEntry(kv.Key, kv.Value.typeDynamic));
        }

        visibilityESL = new EditorSelectionList("Select Event", components, delegate { SelectAddVisibility(add); });
        visibilityESL.SelectItem();
    }

    public void SelectAddVisibility(bool add)
    {
        string[] oldC = null;

        if(add)
        {
            oldC = eventComponent.addComponents;
        }
        else
        {
            oldC = eventComponent.removeComponents;
        }
        string[] newC = new string[oldC.Length + 1];
        int i;
        for (i = 0; i < oldC.Length; i++)
        {
            newC[i] = oldC[i];
        }

        newC[i] = visibilityESL.selection;

        if (add)
        {
            eventComponent.addComponents = newC;
        }
        else
        {
            eventComponent.removeComponents = newC;
        }
        Update();
    }

    public void RemoveVisibility(int index, bool add)
    {
        string[] oldC = null;

        if (add)
        {
            oldC = eventComponent.addComponents;
        }
        else
        {
            oldC = eventComponent.removeComponents;
        }

        string[] newC = new string[oldC.Length - 1];

        int j = 0;
        for (int i = 0; i < oldC.Length; i++)
        {
            if (i != index)
            {
                newC[j++] = oldC[i];
            }
        }

        if (add)
        {
            eventComponent.addComponents = newC;
        }
        else
        {
            eventComponent.removeComponents = newC;
        }
        Update();
    }
}
