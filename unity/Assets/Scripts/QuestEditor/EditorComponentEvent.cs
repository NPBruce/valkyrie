using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

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
    private readonly StringKey MUSIC = new StringKey("val", "MUSIC");
    private readonly StringKey CONTINUE = new StringKey("val", "CONTINUE");
    private readonly StringKey QUOTA = new StringKey("val","QUOTA");
    private readonly StringKey BUTTONS = new StringKey("val","BUTTONS");
    private readonly StringKey BUTTON = new StringKey("val", "BUTTON");
    private readonly StringKey TESTS = new StringKey("val","TESTS");
    private readonly StringKey VAR = new StringKey("val", "VAR");
    private readonly StringKey OP = new StringKey("val", "OP");
    private readonly StringKey VALUE = new StringKey("val", "VALUE");
    private readonly StringKey ASSIGN = new StringKey("val", "ASSIGN");
    private readonly StringKey VAR_NAME = new StringKey("val", "VAR_NAME");
    private readonly StringKey NUMBER = new StringKey("val", "NUMBER");

    QuestData.Event eventComponent;

    PaneledDialogBoxEditable eventTextDBE;
    EditorSelectionList triggerESL;
    EditorSelectionList highlightESL;
    EditorSelectionList heroCountESL;
    EditorSelectionList visibilityESL;
    EditorSelectionList audioESL;
    EditorSelectionList musicESL;
    List<DialogBoxEditable> buttonDBE;
    DialogBoxEditable quotaDBE;
    EditorSelectionList addEventESL;
    EditorSelectionList colorESL;
    EditorSelectionList varESL;
    QuestEditorTextEdit varText;

    public EditorComponentEvent(string nameIn) : base()
    {
        Game game = Game.Get();
        eventComponent = game.quest.qd.components[nameIn] as QuestData.Event;
        component = eventComponent;
        name = component.sectionName;
        Update();
    }
    
    override public float AddSubComponents(float offset)
    {
        offset = AddPosition(offset);

        offset = AddSubEventComponents(offset);

        offset = AddEventDialog(offset);

        offset = AddEventTrigger(offset);

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", AUDIO));

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(4, offset++, 10, 1);
        ui.SetText(eventComponent.audio);
        ui.SetButton(delegate { SetAudio(); });
        new UIElementBorder(ui);

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(1.5f, offset, 10, 1);
        ui.SetText(new StringKey("val", "X_COLON", MUSIC));

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(11.5f, offset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddMusic(0); });
        new UIElementBorder(ui, Color.green);

        int index;
        for (index = 0; index < eventComponent.music.Count; index++)
        {
            int i = index;
            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(0.5f, offset, 1, 1);
            ui.SetText(CommonStringKeys.MINUS, Color.red);
            ui.SetButton(delegate { RemoveMusic(i); });
            new UIElementBorder(ui, Color.red);

            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(1.5f, offset, 10, 1);
            ui.SetText(eventComponent.music[index]);
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(11.5f, offset++, 1, 1);
            ui.SetText(CommonStringKeys.PLUS, Color.green);
            ui.SetButton(delegate { AddMusic(i + 1); });
            new UIElementBorder(ui, Color.green);
        }
        offset++;

        if (game.gameType is D2EGameType)
        {
            offset = AddHeroSelection(offset);
        }

        float componentsOffset = offset;

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0, offset, 9, 1);
        ui.SetText(ADD_COMPONENTS);

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(9, offset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddVisibility(true); });
        new UIElementBorder(ui, Color.green);

        for (index = 0; index < eventComponent.addComponents.Length; index++)
        {
            int i = index;

            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(0.5f, offset, 8.5f, 1);
            ui.SetText(eventComponent.addComponents[index]);
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(9, offset++, 1, 1);
            ui.SetText(CommonStringKeys.MINUS, Color.red);
            ui.SetButton(delegate { RemoveVisibility(i, true); });
            new UIElementBorder(ui, Color.red);
        }

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(10, componentsOffset, 8.5f, 1);
        ui.SetText(REMOVE_COMPONENTS);

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(18.5f, componentsOffset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddVisibility(false); });
        new UIElementBorder(ui, Color.green);

        for (index = 0; index < eventComponent.removeComponents.Length; index++)
        {
            int i = index;
            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(10, componentsOffset, 8.5f, 1);
            ui.SetText(eventComponent.removeComponents[index]);
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(18.5f, componentsOffset++, 1, 1);
            ui.SetText(CommonStringKeys.MINUS, Color.red);
            ui.SetButton(delegate { RemoveVisibility(i, false); });
            new UIElementBorder(ui, Color.red);
        }

        if (componentsOffset > offset) offset = componentsOffset;
        offset++;

        offset = AddNextEventComponents(offset);

        offset = AddEventVarComponents(offset);

        Highlight();
        return offset;
    }

    virtual public float AddPosition(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.POSITION));

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(4, offset, 4, 1);
        ui.SetText(CommonStringKeys.POSITION_SNAP);
        ui.SetButton(delegate { GetPosition(); });
        new UIElementBorder(ui);

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(9, offset, 4, 1);
        ui.SetText(CommonStringKeys.POSITION_FREE);
        ui.SetButton(delegate { GetPosition(false); });
        new UIElementBorder(ui);

        AddLocationType(offset);

        return offset + 2;
    }

    virtual public void AddLocationType(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(14, offset, 4, 1);
        ui.SetButton(delegate { PositionTypeCycle(); });
        new UIElementBorder(ui);
        if (eventComponent.minCam)
        {
            ui.SetText(MIN_CAM);
        }
        else if (eventComponent.maxCam)
        {
            ui.SetText(MAX_CAM);
        }
        else if (!eventComponent.locationSpecified)
        {
            ui.SetText(UNUSED);
        }
        else if (eventComponent.highlight)
        {
            ui.SetText(HIGHLIGHT);
        }
        else
        {
            ui.SetText(CAMERA);
        }
    }

    virtual public float AddSubEventComponents(float offset)
    {
        return offset;
    }

    virtual public float AddEventDialog(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0, offset++, 20, 1);
        ui.SetText(new StringKey("val", "X_COLON", DIALOG));

        eventTextDBE = new PaneledDialogBoxEditable(
            new Vector2(0.5f, offset), new Vector2(19, 8), 
            eventComponent.text.Translate(true),
            delegate { UpdateText(); });
        eventTextDBE.ApplyTag(Game.EDITOR);
        eventTextDBE.background.transform.SetParent(scrollArea.transform);
        eventTextDBE.AddBorder();
        return offset + 9;
    }

    virtual public float AddEventTrigger(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.TRIGGER));

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(4, offset, 10, 1);
        ui.SetText(eventComponent.trigger);
        ui.SetButton(delegate { SetTrigger(); });
        new UIElementBorder(ui);
        return offset + 2;
    }

    virtual public float AddHeroSelection(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0, offset, 6, 1);
        ui.SetText(new StringKey("val", "X_COLON", SELECTION));

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(6, offset++, 12, 1);
        ui.SetText(eventComponent.heroListName);
        ui.SetButton(delegate { SetHighlight(); });
        new UIElementBorder(ui);

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(6, offset, 3, 1);
        ui.SetText(new StringKey("val", "X_COLON", MIN));

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(9, offset, 2, 1);
        ui.SetText(eventComponent.minHeroes.ToString());
        ui.SetButton(delegate { SetHeroCount(false); });
        new UIElementBorder(ui);

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(11, offset, 3, 1);
        ui.SetText(new StringKey("val", "X_COLON", MAX));

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(14, offset, 2, 1);
        ui.SetText(eventComponent.maxHeroes.ToString());
        ui.SetButton(delegate { SetHeroCount(true); });
        new UIElementBorder(ui);
        return offset + 2;
    }

    virtual public float AddNextEventComponents(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", QUOTA));

        if (eventComponent.quotaVar.Length == 0)
        {
            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(4, offset, 5, 1);
            ui.SetText(new StringKey("val", "NUMBER"));
            ui.SetButton(delegate { SetQuotaVar(); });
            new UIElementBorder(ui);

            // Quota dont need translation
            quotaDBE = new DialogBoxEditable(
                new Vector2(9, offset), new Vector2(2, 1),
                eventComponent.quota.ToString(), false, 
                delegate { SetQuota(); });
            quotaDBE.background.transform.SetParent(scrollArea.transform);
            quotaDBE.ApplyTag(Game.EDITOR);
            quotaDBE.AddBorder();
        }
        else
        {
            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(4, offset, 5, 1);
            ui.SetText(new StringKey("val", "VAR"));
            ui.SetButton(delegate { SetQuotaInt(); });
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(9, offset, 9, 1);
            ui.SetText(eventComponent.quotaVar);
            ui.SetButton(delegate { SetQuotaVar(); });
            new UIElementBorder(ui);
        }
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0, offset, 8, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "NEXT_EVENTS")));

        string randomButton = "Ordered";
        if (eventComponent.randomEvents) randomButton = "Random";
        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(8, offset, 4, 1);
        ui.SetText(new StringKey("val", randomButton));
        ui.SetButton(delegate { ToggleRandom(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(1.5f, offset, 17, 1);
        ui.SetText(new StringKey("val", "X_COLON", BUTTONS));

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(18.5f, offset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddButton(); });
        new UIElementBorder(ui, Color.green);

        int button = 1;
        int index = 0;
        float lastButtonOffset = 0;
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

            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(0.5f, offset, 3, 1);
            ui.SetText(new StringKey("val", "COLOR"), c);
            ui.SetButton(delegate { SetButtonColor(buttonTmp); });
            new UIElementBorder(ui, c);

            DialogBoxEditable buttonEdit = new DialogBoxEditable(
                new Vector2(3.5f, offset++), new Vector2(15, 1), 
                buttonLabel.Translate(), false,
                delegate { UpdateButtonLabel(buttonTmp); });
            buttonEdit.background.transform.SetParent(scrollArea.transform);
            buttonEdit.ApplyTag(Game.EDITOR);
            buttonEdit.AddBorder();
            buttonDBE.Add(buttonEdit);

            index = 0;
            foreach (string s in l)
            {
                int i = index++;
                string tmpName = s;
                ui = new UIElement(Game.EDITOR, scrollArea.transform);
                ui.SetLocation(0.5f, offset, 1, 1);
                ui.SetText(CommonStringKeys.PLUS, Color.green);
                ui.SetButton(delegate { AddEvent(i, buttonTmp); });
                new UIElementBorder(ui, Color.green);

                ui = new UIElement(Game.EDITOR, scrollArea.transform);
                ui.SetLocation(1.5f, offset, 17, 1);
                ui.SetText(s);
                ui.SetButton(delegate { QuestEditorData.SelectComponent(tmpName); });
                new UIElementBorder(ui);

                ui = new UIElement(Game.EDITOR, scrollArea.transform);
                ui.SetLocation(18.5f, offset++, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate { RemoveEvent(i, buttonTmp); });
                new UIElementBorder(ui, Color.red);
            }

            int tmp = index;
            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(0.5f, offset++, 1, 1);
            ui.SetText(CommonStringKeys.PLUS, Color.green);
            ui.SetButton(delegate { AddEvent(tmp, buttonTmp); });
            new UIElementBorder(ui, Color.green);
        }

        if (lastButtonOffset != 0)
        {
            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(18.5f, lastButtonOffset, 1, 1);
            ui.SetText(CommonStringKeys.MINUS, Color.red);
            ui.SetButton(delegate { RemoveButton(); });
            new UIElementBorder(ui, Color.red);
        }

        return offset + 1;
    }

    virtual public float AddEventVarComponents(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0.5f, offset, 18, 1);
        ui.SetText(new StringKey("val", "X_COLON", TESTS));

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(18.5f, offset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddTestOp(); });
        new UIElementBorder(ui, Color.green);

        foreach (QuestData.Event.VarOperation op in eventComponent.conditions)
        {
            QuestData.Event.VarOperation tmp = op;
            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(0.5f, offset, 8.5f, 1);
            ui.SetText(op.var);
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(9, offset, 2, 1);
            ui.SetText(op.operation);
            ui.SetButton(delegate { SetTestOpp(tmp); });
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(11, offset, 7.5f, 1);
            ui.SetText(op.value);
            ui.SetButton(delegate { SetValue(tmp); });
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(18.5f, offset++, 1, 1);
            ui.SetText(CommonStringKeys.MINUS, Color.red);
            ui.SetButton(delegate { RemoveOp(tmp); });
            new UIElementBorder(ui, Color.red);
        }
        offset++;

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0.5f, offset, 18, 1);
        ui.SetText(new StringKey("val", "X_COLON", ASSIGN));

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(18.5f, offset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddAssignOp(); });
        new UIElementBorder(ui, Color.green);

        foreach (QuestData.Event.VarOperation op in eventComponent.operations)
        {
            QuestData.Event.VarOperation tmp = op;
            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(0.5f, offset, 8.5f, 1);
            ui.SetText(op.var);
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(9, offset, 2, 1);
            ui.SetText(op.operation);
            ui.SetButton(delegate { SetAssignOpp(tmp); });
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(11, offset, 7.5f, 1);
            ui.SetText(op.value);
            ui.SetButton(delegate { SetValue(tmp); });
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(18.5f, offset++, 1, 1);
            ui.SetText(CommonStringKeys.MINUS, Color.red);
            ui.SetButton(delegate { RemoveOp(tmp); });
            new UIElementBorder(ui, Color.red);
        }

        return offset + 1;
    }

    virtual public void Highlight()
    {
        if (eventComponent.locationSpecified || eventComponent.maxCam || eventComponent.minCam)
        {
            CameraController.SetCamera(eventComponent.location);
            game.tokenBoard.AddHighlight(eventComponent.location, "EventLoc", Game.EDITOR);
        }
    }

    virtual public void PositionTypeCycle()
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
        if (eventTextDBE.CheckTextChangedAndNotEmpty())
        {
            LocalizationRead.updateScenarioText(eventComponent.text_key, eventTextDBE.Text);
            eventComponent.display = true;
            if (eventComponent.buttons.Count == 0)
            {
                eventComponent.buttons.Add(eventComponent.genQuery("button1"));
                eventComponent.nextEvent.Add(new List<string>());
                eventComponent.buttonColors.Add("white");
                LocalizationRead.updateScenarioText(eventComponent.genKey("button1"), 
                    CONTINUE.Translate());
            }
        }
        else if (eventTextDBE.CheckTextEmptied())
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

        HashSet<string> vars = new HashSet<string>();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.CustomMonster)
            {
                triggers.Add(new EditorSelectionList.SelectionListEntry("Defeated" + kv.Key, "Quest"));
            }

            if (kv.Value is QuestData.Event)
            {
                QuestData.Event e = kv.Value as QuestData.Event;
                foreach (string s in ExtractVarsFromEvent(e))
                {
                    if (s[0] == '@')
                    {
                        vars.Add(s);
                    }
                }
            }
        }

        foreach (string s in vars)
        {
            triggers.Add(new EditorSelectionList.SelectionListEntry("Var" + s.Substring(1), "Vars"));
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

    public void AddMusic(int index)
    {
        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;
        Game game = Game.Get();
        List<EditorSelectionList.SelectionListEntry> audio = new List<EditorSelectionList.SelectionListEntry>();
        foreach (string s in Directory.GetFiles(relativePath, "*.ogg", SearchOption.AllDirectories))
        {
            audio.Add(new EditorSelectionList.SelectionListEntry(s.Substring(relativePath.Length + 1), "File"));
        }

        foreach (KeyValuePair<string, AudioData> kv in game.cd.audio)
        {
            audio.Add(new EditorSelectionList.SelectionListEntry(kv.Key, new List<string>(kv.Value.traits)));
        }

        musicESL = new EditorSelectionList(new StringKey("val", "SELECT", new StringKey("val", "AUDIO")), audio, delegate { SelectMusic(index); });
        musicESL.SelectItem();
    }

    public void SelectMusic(int index)
    {
        eventComponent.music.Insert(index, musicESL.selection);
        Update();
    }

    public void RemoveMusic(int index)
    {
        eventComponent.music.RemoveAt(index);
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
            components.Add(new EditorSelectionList.SelectionListEntry("#shop", "Special"));
        }
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Door || kv.Value is QuestData.Tile || kv.Value is QuestData.Token || kv.Value is QuestData.UI)
            {
                components.Add(new EditorSelectionList.SelectionListEntry(kv.Key, kv.Value.typeDynamic));
            }
            if (kv.Value is QuestData.Spawn && !add)
            {
                components.Add(new EditorSelectionList.SelectionListEntry(kv.Key, kv.Value.typeDynamic));
            }
            if (kv.Value is QuestData.QItem && add)
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

    public void SetQuotaInt()
    {
        eventComponent.quotaVar = "";
        Update();
    }

    public void SetQuotaVar()
    {
        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        list.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyTraitItem("{" + CommonStringKeys.NEW.Translate() + "}", "{NEW}", "Quest"));
        list.AddRange(GetQuestVars());
        varESL = new EditorSelectionList(new StringKey("val", "SELECT", VAR), list, delegate { SelectQuotaVar(); });
        varESL.SelectItem();
    }

    public void SelectQuotaVar()
    {
        if (varESL.selection.Equals("{NEW}"))
        {
            varText = new QuestEditorTextEdit(VAR_NAME, "", delegate { NewQuotaVar(); });
            varText.EditText();
        }
        else
        {
            eventComponent.quotaVar = varESL.selection;
            eventComponent.quota = 0;
            Update();
        }
    }

    public void NewQuotaVar()
    {
        string var = System.Text.RegularExpressions.Regex.Replace(varText.value, "[^A-Za-z0-9_]", "");
        if (var.Length > 0)
        {
            if (varText.value[0] == '%')
            {
                var = '%' + var;
            }
            if (varText.value[0] == '@')
            {
                var = '@' + var;
            }
            if (char.IsNumber(var[0]) || var[0] == '-' || var[0] == '.')
            {
                var = "var" + var;
            }
            eventComponent.quotaVar = var;
            eventComponent.quota = 0;
        }
        Update();
    }

    public void AddButton()
    {
        int count = eventComponent.nextEvent.Count + 1;
        eventComponent.nextEvent.Add(new List<string>());
        eventComponent.buttons.Add(eventComponent.genQuery("button" + count));
        eventComponent.buttonColors.Add("white");
        LocalizationRead.updateScenarioText(eventComponent.genKey("button" + count), BUTTON.Translate() + count);
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
        if (buttonDBE[number - 1].CheckTextChangedAndNotEmpty())
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

    public void AddTestOp()
    {
        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        list.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyTraitItem("{" + CommonStringKeys.NEW.Translate() + "}", "{NEW}", "Quest"));
        list.AddRange(GetQuestVars());

        list.Add(new EditorSelectionList.SelectionListEntry("#monsters", "#"));
        list.Add(new EditorSelectionList.SelectionListEntry("#heroes", "#"));
        list.Add(new EditorSelectionList.SelectionListEntry("#round", "#"));
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

    public static HashSet<string> ExtractVarsFromEvent(QuestData.Event e)
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

        list.Add(new EditorSelectionList.SelectionListEntry("#monsters", "#"));
        list.Add(new EditorSelectionList.SelectionListEntry("#heroes", "#"));
        list.Add(new EditorSelectionList.SelectionListEntry("#round", "#"));
        list.Add(new EditorSelectionList.SelectionListEntry("#eliminated", "#"));
        foreach (ContentData.ContentPack pack in Game.Get().cd.allPacks)
        {
            if (pack.id.Length > 0)
            {
                list.Add(new EditorSelectionList.SelectionListEntry("#" + pack.id, "#"));
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
