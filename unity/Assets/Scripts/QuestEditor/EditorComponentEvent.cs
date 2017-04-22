using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentEvent : EditorComponent
{
    private readonly StringKey MIN_CAM = new StringKey("val", "MIN_CAM");
    private readonly StringKey MAX_CAM = new StringKey("val", "MAX_CAM");
    private readonly StringKey UNUSED = new StringKey("val", "UNUSED");
    private readonly StringKey HIGHLIGHT = new StringKey("val", "HIGHLIGHT");
    private readonly StringKey CAMERA = new StringKey("val", "CAMERA");
    private readonly StringKey REMOVE_COMPONENTS = new StringKey("val", "REMOVE_COMPONENTS");
    private readonly StringKey ADD_COMPONENTS = new StringKey("val", "ADD_COMPONENTS");
    private readonly StringKey MAX = new StringKey("val", "MAX");
    private readonly StringKey MIN = new StringKey("val", "MIN");
    private readonly StringKey DIALOG = new StringKey("val", "DIALOG");
    private readonly StringKey NEXT_EVENTS = new StringKey("val", "NEXT_EVENTS");
    private readonly StringKey VARS = new StringKey("val", "VARS");
    private readonly StringKey SELECTION = new StringKey("val", "SELECTION");
    private readonly StringKey AUDIO = new StringKey("val", "AUDIO");

    QuestData.Event eventComponent;

    DialogBoxEditable eventTextDBE;
    EditorSelectionList triggerESL;
    EditorSelectionList highlightESL;
    EditorSelectionList heroCountESL;
    EditorSelectionList visibilityESL;
    EditorSelectionList audioESL;

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
        if (eventComponent is QuestData.Spawn)
        {
            type = QuestData.Spawn.type;
        }
        if (eventComponent is QuestData.Token)
        {
            type = QuestData.Token.type;
        }
        if (eventComponent is QuestData.Puzzle)
        {
            type = QuestData.Puzzle.type;
        }

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), 
            new StringKey(null,type,false), delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1), 
            new StringKey(null,name.Substring(type.Length),false), 
            delegate { QuestEditorData.ListEvent(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), CommonStringKeys.E, delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        DialogBox db = new DialogBox(new Vector2(0, 2), new Vector2(4, 1), CommonStringKeys.POSITION);
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 2), new Vector2(1, 1), 
            CommonStringKeys.POSITION_SNAP, delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(5, 2), new Vector2(1, 1), 
            CommonStringKeys.POSITION_FREE, delegate { GetPosition(false); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        if (!eventComponent.GetType().IsSubclassOf(typeof(QuestData.Event)))
        {
            if (eventComponent.minCam)
            {
                tb = new TextButton(new Vector2(7, 2), new Vector2(4, 1), MIN_CAM, delegate { PositionTypeCycle(); });
            }
            else if (eventComponent.maxCam)
            {
                tb = new TextButton(new Vector2(7, 2), new Vector2(4, 1), MAX_CAM, delegate { PositionTypeCycle(); });
            }
            else if (!eventComponent.locationSpecified)
            {
                tb = new TextButton(new Vector2(7, 2), new Vector2(4, 1), UNUSED, delegate { PositionTypeCycle(); });
            }
            else if (eventComponent.highlight)
            {
                tb = new TextButton(new Vector2(7, 2), new Vector2(4, 1), HIGHLIGHT, delegate { PositionTypeCycle(); });
            }
            else
            {
                tb = new TextButton(new Vector2(7, 2), new Vector2(4, 1), CAMERA, delegate { PositionTypeCycle(); });
            }
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
        }

        tb = new TextButton(new Vector2(12, 2), new Vector2(3, 1), VARS, delegate { QuestEditorData.SelectAsEventVars(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(16, 2), new Vector2(4, 1), NEXT_EVENTS, delegate { QuestEditorData.SelectAsEventNextEvent(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 3), new Vector2(20, 1), new StringKey("val","X_COLON", DIALOG));
        db.ApplyTag("editor");

        eventTextDBE = new DialogBoxEditable(
            new Vector2(0, 4), new Vector2(20, 8), 
            eventComponent.text.Translate(true),
            delegate { UpdateText(); });
        eventTextDBE.ApplyTag("editor");
        eventTextDBE.AddBorder();

        db = new DialogBox(new Vector2(0, 12), new Vector2(4, 1), new StringKey("val","X_COLON",CommonStringKeys.TRIGGER));
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 12), new Vector2(10, 1), 
            new StringKey(null,eventComponent.trigger,false), delegate { SetTrigger(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 13), new Vector2(4, 1), 
            new StringKey("val", "X_COLON", AUDIO));
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 13), new Vector2(10, 1), 
            new StringKey(null,eventComponent.audio,false), delegate { SetAudio(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        if (game.gameType is D2EGameType)
        {
            db = new DialogBox(new Vector2(0, 14), new Vector2(4, 1), new StringKey("val", "X_COLON", SELECTION));
            db.ApplyTag("editor");

            tb = new TextButton(new Vector2(4, 14), new Vector2(8, 1), 
                new StringKey(null,eventComponent.heroListName,false), delegate { SetHighlight(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");

            db = new DialogBox(new Vector2(12, 14), new Vector2(2, 1), MIN);
            db.ApplyTag("editor");

            tb = new TextButton(new Vector2(14, 14), new Vector2(2, 1), eventComponent.minHeroes, delegate { SetHeroCount(false); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");

            db = new DialogBox(new Vector2(16, 14), new Vector2(2, 1), MAX);
            db.ApplyTag("editor");

            tb = new TextButton(new Vector2(18, 14), new Vector2(2, 1), eventComponent.maxHeroes, delegate { SetHeroCount(true); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
        }

        db = new DialogBox(new Vector2(0, 16), new Vector2(9, 1), ADD_COMPONENTS);
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(9, 16), new Vector2(1, 1), CommonStringKeys.PLUS, 
            delegate { AddVisibility(true); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        int offset = 17;
        int index;
        for (index = 0; index < 12; index++)
        {
            if (eventComponent.addComponents.Length > index)
            {
                int i = index;
                db = new DialogBox(new Vector2(0, offset), new Vector2(9, 1), 
                    new StringKey(null,eventComponent.addComponents[index],false)
                    );
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(9, offset++), new Vector2(1, 1), CommonStringKeys.MINUS, 
                    delegate { RemoveVisibility(i, true); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        db = new DialogBox(new Vector2(10, 16), new Vector2(9, 1), REMOVE_COMPONENTS);
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 16), new Vector2(1, 1), 
            CommonStringKeys.PLUS, 
            delegate { AddVisibility(false); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        offset = 17;
        for (index = 0; index < 12; index++)
        {
            if (eventComponent.removeComponents.Length > index)
            {
                int i = index;
                db = new DialogBox(new Vector2(10, offset), new Vector2(9, 1), 
                    new StringKey(null,eventComponent.removeComponents[index],false)
                    );
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(19, offset++), new Vector2(1, 1), 
                    CommonStringKeys.MINUS, 
                    delegate { RemoveVisibility(i, false); }, Color.red);
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
        if (!eventTextDBE.Text.Equals(""))
        {
            LocalizationRead.updateScenarioText(eventComponent.text_key, eventTextDBE.Text);
            eventComponent.display = true;
            if (eventComponent.buttons.Count == 0)
            {
                eventComponent.buttons.Add(eventComponent.genQuery("button1"));
                eventComponent.nextEvent.Add(new List<string>());
                eventComponent.buttonColors.Add("white");
                LocalizationRead.updateScenarioText(eventComponent.genKey("button1"), "Continue");
            }
        }
        else
        {
            LocalizationRead.scenarioDict.Remove(eventComponent.text_key);
            eventComponent.display = false;
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
            triggers.Add(new EditorSelectionList.SelectionListEntry("EventStart", "General", Color.grey));
        }
        else
        {
            triggers.Add(new EditorSelectionList.SelectionListEntry("EventStart", "General"));
        }

        if (noMorale)
        {
            triggers.Add(new EditorSelectionList.SelectionListEntry("NoMorale", "General", Color.grey));
        }
        else
        {
            triggers.Add(new EditorSelectionList.SelectionListEntry("NoMorale", "General"));
        }

        if (eliminated)
        {
            triggers.Add(new EditorSelectionList.SelectionListEntry("Eliminated", "General", Color.grey));
        }
        else
        {
            triggers.Add(new EditorSelectionList.SelectionListEntry("Eliminated", "General"));
        }

        triggers.Add(new EditorSelectionList.SelectionListEntry("Mythos", "General"));

        triggers.Add(new EditorSelectionList.SelectionListEntry("EndRound", "General"));

        triggers.Add(new EditorSelectionList.SelectionListEntry("StartRound", "General"));

        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            triggers.Add(new EditorSelectionList.SelectionListEntry("Defeated" + kv.Key, "Monsters"));
            triggers.Add(new EditorSelectionList.SelectionListEntry("DefeatedUnique" + kv.Key, "Monsters"));
        }

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.CustomMonster)
            {
                triggers.Add(new EditorSelectionList.SelectionListEntry("Defeated" + kv.Key, "Quest"));
            }
        }

        triggerESL = new EditorSelectionList(
                        new StringKey("val", "SELECT", CommonStringKeys.TRIGGER),
                        triggers, delegate { SelectEventTrigger(); });
        triggerESL.SelectItem();
    }

    public void SelectEventTrigger()
    {
        eventComponent.trigger = triggerESL.selection;
        Update();
    }

    public void SetAudio()
    {
        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;
        Game game = Game.Get();
        List<EditorSelectionList.SelectionListEntry> audio = new List<EditorSelectionList.SelectionListEntry>();
        audio.Add(new EditorSelectionList.SelectionListEntry("", Color.white));

        foreach (string s in Directory.GetFiles(relativePath, "*.ogg", SearchOption.AllDirectories))
        {
            audio.Add(new EditorSelectionList.SelectionListEntry(s.Substring(relativePath.Length + 1), "File"));
        }

        foreach (KeyValuePair<string, AudioData> kv in game.cd.audio)
        {
            audio.Add(new EditorSelectionList.SelectionListEntry(kv.Key, new List<string>(kv.Value.traits)));
        }

        audioESL = new EditorSelectionList(new StringKey("val","SELECT",new StringKey("val","AUDIO")), audio, delegate { SelectEventAudio(); });
        audioESL.SelectItem();
    }

    public void SelectEventAudio()
    {
        Game game = Game.Get();
        eventComponent.audio = audioESL.selection;
        if (game.cd.audio.ContainsKey(eventComponent.audio))
        {
            game.audioControl.Play(game.cd.audio[eventComponent.audio].file);
        }
        else
        {
            string path = Path.GetDirectoryName(Game.Get().quest.qd.questPath) + "/" + eventComponent.audio;
            game.audioControl.Play(path);
        }
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

        highlightESL = new EditorSelectionList(
            new StringKey("val", "SELECT", CommonStringKeys.EVENT),
            events, delegate { SelectEventHighlight(); });
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

        heroCountESL = new EditorSelectionList(
            new StringKey("val", "SELECT", CommonStringKeys.NUMBER), 
            num, delegate { SelectEventHeroCount(max); });
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
        if (!add)
        {
            components.Add(new EditorSelectionList.SelectionListEntry("#boardcomponents", "Special"));
            components.Add(new EditorSelectionList.SelectionListEntry("#monsters", "Special"));
        }
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Door || kv.Value is QuestData.Tile || kv.Value is QuestData.Token)
            {
                components.Add(new EditorSelectionList.SelectionListEntry(kv.Key, kv.Value.typeDynamic));
            }
            if (kv.Value is QuestData.Spawn && !add)
            {
                components.Add(new EditorSelectionList.SelectionListEntry(kv.Key, kv.Value.typeDynamic));
            }
        }

        visibilityESL = new EditorSelectionList( 
            new StringKey("val","SELECT",CommonStringKeys.TILE),
            components, delegate { SelectAddVisibility(add); });
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
