using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

public class EditorComponentEvent : EditorComponent
{
    //  Not used yet
    //private readonly StringKey NEXT_EVENTS = new StringKey("val", "NEXT_EVENTS");
    //private readonly StringKey VARS = new StringKey("val", "VARS");

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
    private readonly StringKey AND = new StringKey("val", "AND");
    private readonly StringKey OR = new StringKey("val", "OR");
    
    QuestData.Event eventComponent;

    UIElementEditablePaneled eventTextUIE;
    UIElementEditable quotaUIE;
    List<UIElementEditable> buttonUIE;

    QuestEditorTextEdit varText;

    public EditorComponentEvent(string nameIn) : base()
    {
        Game game = Game.Get();
        eventComponent = game.quest.qd.components[nameIn] as QuestData.Event;
        component = eventComponent;
        name = component.sectionName;
        Update();
    }

    override protected void RefreshReference()
    {
        base.RefreshReference();
        eventComponent = component as QuestData.Event;
    }

    override public float AddSubComponents(float offset)
    {
        offset = AddPosition(offset);

        offset = AddSubEventComponents(offset);

        offset = AddEventDialog(offset);

        offset = AddEventTrigger(offset);

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", AUDIO));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset++, 10, 1);
        ui.SetText(eventComponent.audio);
        ui.SetButton(delegate { SetAudio(); });
        new UIElementBorder(ui);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(1.5f, offset, 10, 1);
        ui.SetText(new StringKey("val", "X_COLON", MUSIC));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(11.5f, offset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddMusic(0); });
        new UIElementBorder(ui, Color.green);

        int index;
        for (index = 0; index < eventComponent.music.Count; index++)
        {
            int i = index;
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0.5f, offset, 1, 1);
            ui.SetText(CommonStringKeys.MINUS, Color.red);
            ui.SetButton(delegate { RemoveMusic(i); });
            new UIElementBorder(ui, Color.red);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(1.5f, offset, 10, 1);
            ui.SetText(eventComponent.music[index]);
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(11.5f, offset++, 1, 1);
            ui.SetText(CommonStringKeys.PLUS, Color.green);
            ui.SetButton(delegate { AddMusic(i + 1); });
            new UIElementBorder(ui, Color.green);
        }
        offset++;

        if (game.gameType is D2EGameType || game.gameType is IAGameType)
        {
            offset = AddHeroSelection(offset);
        }

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 18, 1);
        ui.SetText(ADD_COMPONENTS);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(18.5f, offset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddVisibility(true); });
        new UIElementBorder(ui, Color.green);

        for (index = 0; index < eventComponent.addComponents.Length; index++)
        {
            int i = index;

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetText(eventComponent.addComponents[index]);
            ui.SetButton(delegate { AddVisibility(true, i); });
            if (game.quest.qd.components.ContainsKey(eventComponent.addComponents[i]))
            {
                ui.SetLocation(0.5f, offset, 17, 1);
                UIElement link = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                link.SetLocation(17.5f, offset, 1, 1);
                link.SetText("<b>⇨</b>", Color.cyan);
                link.SetTextAlignment(TextAnchor.LowerCenter);
                link.SetButton(delegate { QuestEditorData.SelectComponent(eventComponent.addComponents[i]); });
                new UIElementBorder(link, Color.cyan);
            }
            else
            {
                ui.SetLocation(0.5f, offset, 18, 1);
            }
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(18.5f, offset++, 1, 1);
            ui.SetText(CommonStringKeys.MINUS, Color.red);
            ui.SetButton(delegate { RemoveVisibility(i, true); });
            new UIElementBorder(ui, Color.red);
        }
        offset++;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 18, 1);
        ui.SetText(REMOVE_COMPONENTS);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(18.5f, offset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddVisibility(false); });
        new UIElementBorder(ui, Color.green);

        for (index = 0; index < eventComponent.removeComponents.Length; index++)
        {
            int i = index;
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetText(eventComponent.removeComponents[index]);
            ui.SetButton(delegate { AddVisibility(false, i); });
            if (game.quest.qd.components.ContainsKey(eventComponent.removeComponents[i]))
            {
                ui.SetLocation(0.5f, offset, 17, 1);
                UIElement link = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                link.SetLocation(17.5f, offset, 1, 1);
                link.SetText("<b>⇨</b>", Color.cyan);
                link.SetTextAlignment(TextAnchor.LowerCenter);
                link.SetButton(delegate { QuestEditorData.SelectComponent(eventComponent.removeComponents[i]); });
                new UIElementBorder(link, Color.cyan);
            }
            else
            {
                ui.SetLocation(0.5f, offset, 18, 1);
            }
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(18.5f, offset++, 1, 1);
            ui.SetText(CommonStringKeys.MINUS, Color.red);
            ui.SetButton(delegate { RemoveVisibility(i, false); });
            new UIElementBorder(ui, Color.red);
        }
        offset++;

        offset = AddNextEventComponents(offset);

        offset = AddEventVarConditionComponents(offset);

        offset = AddEventVarOperationComponents(offset);

        Highlight();
        return offset;
    }

    virtual public float AddPosition(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.POSITION));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset, 4, 1);
        ui.SetText(CommonStringKeys.POSITION_SNAP);
        ui.SetButton(delegate { GetPosition(); });
        new UIElementBorder(ui);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(9, offset, 4, 1);
        ui.SetText(CommonStringKeys.POSITION_FREE);
        ui.SetButton(delegate { GetPosition(false); });
        new UIElementBorder(ui);

        AddLocationType(offset);

        return offset + 2;
    }

    virtual public void AddLocationType(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
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
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset++, 20, 1);
        ui.SetText(new StringKey("val", "X_COLON", DIALOG));

        eventTextUIE = new UIElementEditablePaneled(Game.EDITOR, scrollArea.GetScrollTransform());
        eventTextUIE.SetLocation(0.5f, offset, 19, 18);
        eventTextUIE.SetText(eventComponent.text.Translate(true));
        offset += eventTextUIE.HeightToTextPadding(1);
        eventTextUIE.SetButton(delegate { UpdateText(); });
        new UIElementBorder(eventTextUIE);
        return offset + 1;
    }

    virtual public float AddEventTrigger(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.TRIGGER));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset, 10, 1);
        ui.SetText(eventComponent.trigger);
        ui.SetButton(delegate { SetTrigger(); });
        new UIElementBorder(ui);
        return offset + 2;
    }

    virtual public float AddHeroSelection(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 6, 1);
        ui.SetText(new StringKey("val", "X_COLON", SELECTION));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(6, offset, 12.5f, 1);
        ui.SetText(eventComponent.heroListName);
        ui.SetButton(delegate { SetHighlight(); });
        new UIElementBorder(ui);

        if (game.quest.qd.components.ContainsKey(eventComponent.heroListName))
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(18.5f, offset, 1, 1);
            ui.SetText("<b>⇨</b>", Color.cyan);
            ui.SetTextAlignment(TextAnchor.LowerCenter);
            ui.SetButton(delegate { QuestEditorData.SelectComponent(eventComponent.heroListName); });
            new UIElementBorder(ui, Color.cyan);
        }
        offset++;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(6, offset, 3, 1);
        ui.SetText(new StringKey("val", "X_COLON", MIN));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(9, offset, 2, 1);
        ui.SetText(eventComponent.minHeroes.ToString());
        ui.SetButton(delegate { SetHeroCount(false); });
        new UIElementBorder(ui);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(11, offset, 3, 1);
        ui.SetText(new StringKey("val", "X_COLON", MAX));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(14, offset, 2, 1);
        ui.SetText(eventComponent.maxHeroes.ToString());
        ui.SetButton(delegate { SetHeroCount(true); });
        new UIElementBorder(ui);
        return offset + 2;
    }

    virtual public float AddNextEventComponents(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", QUOTA));

        if (eventComponent.quotaVar.Length == 0)
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(4, offset, 5, 1);
            ui.SetText(new StringKey("val", "NUMBER"));
            ui.SetButton(delegate { SetQuotaVar(); });
            new UIElementBorder(ui);

            // Quota dont need translation
            quotaUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
            quotaUIE.SetLocation(9, offset, 2, 1);
            quotaUIE.SetText(eventComponent.quota.ToString());
            quotaUIE.SetButton(delegate { SetQuota(); });
            quotaUIE.SetSingleLine();
            new UIElementBorder(quotaUIE);
        }
        else
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(4, offset, 5, 1);
            ui.SetText(new StringKey("val", "VAR"));
            ui.SetButton(delegate { SetQuotaInt(); });
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(9, offset, 9, 1);
            ui.SetText(eventComponent.quotaVar);
            ui.SetButton(delegate { SetQuotaVar(); });
            new UIElementBorder(ui);
        }
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 8, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "NEXT_EVENTS")));

        string randomButton = "Ordered";
        if (eventComponent.randomEvents) randomButton = "Random";
        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(8, offset, 4, 1);
        ui.SetText(new StringKey("val", randomButton));
        ui.SetButton(delegate { ToggleRandom(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(1.5f, offset, 17, 1);
        ui.SetText(new StringKey("val", "X_COLON", BUTTONS));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(18.5f, offset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddButton(); });
        new UIElementBorder(ui, Color.green);

        int button = 1;
        int index = 0;
        float lastButtonOffset = 0;
        buttonUIE = new List<UIElementEditable>();
        foreach (List<string> l in eventComponent.nextEvent)
        {
            lastButtonOffset = offset;
            int buttonTmp = button++;

            StringKey buttonLabel = eventComponent.buttons[buttonTmp - 1];
            string colorRGB = ColorUtil.FromName(eventComponent.buttonColors[buttonTmp - 1]);
            Color32 c = Color.white;
            c.r = (byte)System.Convert.ToByte(colorRGB.Substring(1, 2), 16);
            c.g = (byte)System.Convert.ToByte(colorRGB.Substring(3, 2), 16);
            c.b = (byte)System.Convert.ToByte(colorRGB.Substring(5, 2), 16);
            if (colorRGB.Length == 9)
                c.a = (byte)System.Convert.ToByte(colorRGB.Substring(7, 2), 16);
            else
                c.a = 255; // opaque by default

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0.5f, offset, 3, 1);
            ui.SetText(new StringKey("val", "COLOR"), c);
            ui.SetButton(delegate { SetButtonColor(buttonTmp); });
            new UIElementBorder(ui, c);

            UIElementEditable buttonEdit = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
            buttonEdit.SetLocation(3.5f, offset++, 15, 1);
            buttonEdit.SetText(buttonLabel);
            buttonEdit.SetButton(delegate { UpdateButtonLabel(buttonTmp); });
            buttonEdit.SetSingleLine();
            new UIElementBorder(buttonEdit);
            buttonUIE.Add(buttonEdit);

            index = 0;
            foreach (string s in l)
            {
                int i = index++;
                string tmpName = s;
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(0.5f, offset, 1, 1);
                ui.SetText(CommonStringKeys.PLUS, Color.green);
                ui.SetButton(delegate { AddEvent(i, buttonTmp); });
                new UIElementBorder(ui, Color.green);

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(1.5f, offset, 16, 1);
                ui.SetText(s);
                ui.SetButton(delegate { SetEvent(i, buttonTmp); });
                new UIElementBorder(ui);

                if (game.quest.qd.components.ContainsKey(tmpName))
                {
                    UIElement link = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                    link.SetLocation(17.5f, offset, 1, 1);
                    link.SetText("<b>⇨</b>", Color.cyan);
                    link.SetTextAlignment(TextAnchor.LowerCenter);
                    link.SetButton(delegate { QuestEditorData.SelectComponent(tmpName); });
                    new UIElementBorder(link, Color.cyan);
                }

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(18.5f, offset++, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate { RemoveEvent(i, buttonTmp); });
                new UIElementBorder(ui, Color.red);
            }

            int tmp = index;
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0.5f, offset++, 1, 1);
            ui.SetText(CommonStringKeys.PLUS, Color.green);
            ui.SetButton(delegate { AddEvent(tmp, buttonTmp); });
            new UIElementBorder(ui, Color.green);
        }

        if (lastButtonOffset != 0)
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(18.5f, lastButtonOffset, 1, 1);
            ui.SetText(CommonStringKeys.MINUS, Color.red);
            ui.SetButton(delegate { RemoveButton(); });
            new UIElementBorder(ui, Color.red);
        }

        return offset + 1;
    }

    virtual public float AddEventVarConditionComponents(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 18, 1);
        ui.SetText(new StringKey("val", "X_COLON", TESTS));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(18.5f, offset, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddTestOp(); });
        new UIElementBorder(ui, Color.green);

        if(eventComponent.VarTests.VarTestsComponents.Count > 0)
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0.5f, offset, 1, 1);
            ui.SetText(CommonStringKeys.PLUS, Color.green);
            ui.SetButton(delegate { SelectAddParenthesis(); });
            new UIElementBorder(ui, Color.green);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(1.5f, offset, 2, 1);
            ui.SetText("(...)");
        }

        offset++;

        int component_index = 0;
        foreach (VarTestsComponent tc in eventComponent.VarTests.VarTestsComponents)
        {
            if(tc is VarOperation)
            {
                int tmp_index = component_index;

                // only display arrows if item can be moved
                if(component_index!=(eventComponent.VarTests.VarTestsComponents.Count-1) 
                   && eventComponent.VarTests.VarTestsComponents.Count>1
                   && eventComponent.VarTests.VarTestsComponents.FindIndex(component_index+1, x => x.GetClassVarTestsComponentType() == VarTestsLogicalOperator.GetVarTestsComponentType()) != -1
                   )
                {
                    ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                    ui.SetLocation(0.5f, offset, 1, 1);
                    ui.SetText(CommonStringKeys.DOWN, Color.yellow);
                    ui.SetTextAlignment(TextAnchor.LowerCenter);
                    ui.SetButton(delegate { eventComponent.VarTests.moveComponent(tmp_index, false); Update(); });
                    new UIElementBorder(ui, Color.yellow);
                }

                if (component_index != 0
                    && eventComponent.VarTests.VarTestsComponents.FindLastIndex(component_index - 1, x => x.GetClassVarTestsComponentType() == VarTestsLogicalOperator.GetVarTestsComponentType()) != -1
                )
                {
                    ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                    ui.SetLocation(1.5f, offset, 1, 1);
                    ui.SetText(CommonStringKeys.UP, Color.yellow);
                    ui.SetTextAlignment(TextAnchor.LowerCenter);
                    ui.SetButton(delegate { eventComponent.VarTests.moveComponent(tmp_index, true); Update(); });
                    new UIElementBorder(ui, Color.yellow);
                }

                VarOperation tmp = (VarOperation)tc;
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(2.5f, offset, 8.5f, 1);
                ui.SetText(tmp.var);
                new UIElementBorder(ui);

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(11f, offset, 2, 1);
                ui.SetText(tmp.operation);
                ui.SetButton(delegate { SetTestOpp(tmp); });
                new UIElementBorder(ui);

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(13f, offset, 5.5f, 1);
                ui.SetText(tmp.value);
                ui.SetButton(delegate { SetValue(tmp); });
                new UIElementBorder(ui);

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(18.5f, offset++, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate { RemoveOp(tmp_index); });
                new UIElementBorder(ui, Color.red);
            }

            if (tc is VarTestsLogicalOperator)
            {
                VarTestsLogicalOperator tmp = (VarTestsLogicalOperator)tc;

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(10f, offset, 4, 1);
                if(tmp.op.Equals("AND"))
                    ui.SetText(AND);
                else if (tmp.op.Equals("OR"))
                    ui.SetText(OR);
                ui.SetButton(delegate { tmp.NextLogicalOperator(); Update(); });
                new UIElementBorder(ui);
                offset++;
            }

            if (tc is VarTestsParenthesis)
            {
                int tmp_index = component_index;
                VarTestsParenthesis tp = (VarTestsParenthesis)tc;

                if (component_index != (eventComponent.VarTests.VarTestsComponents.Count - 1)
                    && eventComponent.VarTests.VarTestsComponents.FindIndex(component_index + 1, x => x.GetClassVarTestsComponentType() == VarOperation.GetVarTestsComponentType()) != -1
                    )
                {
                    if (tp.parenthesis == "(")
                    {
                        int valid_index = eventComponent.VarTests.FindNextValidPosition(component_index, false);
                        if (valid_index != -1 
                            && eventComponent.VarTests.FindClosingParenthesis(valid_index) != -1)
                        {
                            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                            ui.SetLocation(0.5f, offset, 1, 1);
                            ui.SetText(CommonStringKeys.DOWN, Color.yellow);
                            ui.SetTextAlignment(TextAnchor.LowerCenter);
                            ui.SetButton(delegate { eventComponent.VarTests.moveComponent(tmp_index, false); Update(); });
                            new UIElementBorder(ui, Color.yellow);
                        }
                    }
                    else if (tp.parenthesis == ")")
                    {
                        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                        ui.SetLocation(0.5f, offset, 1, 1);
                        ui.SetText(CommonStringKeys.DOWN, Color.yellow);
                        ui.SetTextAlignment(TextAnchor.LowerCenter);
                        ui.SetButton(delegate { eventComponent.VarTests.moveComponent(tmp_index, false); Update(); });
                        new UIElementBorder(ui, Color.yellow);
                    }
                }

                if (component_index != 0
                    && eventComponent.VarTests.VarTestsComponents.FindLastIndex(component_index - 1, x => x.GetClassVarTestsComponentType() == VarOperation.GetVarTestsComponentType()) != -1
                    )
                {
                    if (tp.parenthesis == "(")
                    {
                        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                        ui.SetLocation(1.5f, offset, 1, 1);
                        ui.SetText(CommonStringKeys.UP, Color.yellow);
                        ui.SetTextAlignment(TextAnchor.LowerCenter);
                        ui.SetButton(delegate { eventComponent.VarTests.moveComponent(tmp_index, true); Update(); });
                        new UIElementBorder(ui, Color.yellow);
                    }
                    else if (tp.parenthesis == ")")
                    {
                        int valid_index = eventComponent.VarTests.FindNextValidPosition(component_index, true);
                        if (valid_index != -1
                            && eventComponent.VarTests.FindOpeningParenthesis(valid_index) != -1)
                        {
                            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                            ui.SetLocation(1.5f, offset, 1, 1);
                            ui.SetText(CommonStringKeys.UP, Color.yellow);
                            ui.SetTextAlignment(TextAnchor.LowerCenter);
                            ui.SetButton(delegate { eventComponent.VarTests.moveComponent(tmp_index, true); Update(); });
                            new UIElementBorder(ui, Color.yellow);
                        }
                    }
                }

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(2.5f, offset, 2, 1);
                ui.SetText(tp.parenthesis);
                new UIElementBorder(ui);

                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(4.5f, offset, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate { eventComponent.VarTests.Remove(tmp_index); Update(); });
                new UIElementBorder(ui, Color.red);

                offset++;
            }

            component_index++;
        }
        return offset + 1;
    }

    virtual public float AddEventVarOperationComponents(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 18, 1);
        ui.SetText(new StringKey("val", "X_COLON", ASSIGN));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(18.5f, offset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddAssignOp(); });
        new UIElementBorder(ui, Color.green);

        foreach (VarOperation op in eventComponent.operations)
        {
            VarOperation tmp = op;
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0.5f, offset, 8.5f, 1);
            ui.SetText(op.var);
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(9, offset, 2, 1);
            ui.SetText(op.operation);
            ui.SetButton(delegate { SetAssignOpp(tmp); });
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(11, offset, 7.5f, 1);
            ui.SetText(op.value);
            ui.SetButton(delegate { SetValue(tmp); });
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
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
        if (eventTextUIE.Changed())
        {
            if (eventTextUIE.Empty())
            {
                LocalizationRead.dicts["qst"].Remove(eventComponent.text_key);
                eventComponent.display = false;
            }
            else
            {
                LocalizationRead.updateScenarioText(eventComponent.text_key, eventTextUIE.GetText());
                if (eventComponent.buttons.Count == 0)
                {
                    eventComponent.buttons.Add(eventComponent.genQuery("button1"));
                    eventComponent.nextEvent.Add(new List<string>());
                    eventComponent.buttonColors.Add("white");
                    LocalizationRead.updateScenarioText(eventComponent.genKey("button1"),
                        CONTINUE.Translate());
                }
                if (!eventComponent.display)
                {
                    eventComponent.display = true;
                    Update();
                    return;
                }
            }
            if (!eventTextUIE.HeightAtTextPadding(1))
            {
                Update();
            }
        }
    }

    public void SetTrigger()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();

        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(SelectEventTrigger, new StringKey("val", "SELECT", CommonStringKeys.TRIGGER));


        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { new StringKey("val", "GENERAL").Translate() });
        select.AddItem("{NONE}", "", traits);

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
            select.AddItem("EventStart", traits, Color.gray);
        }
        else
        {
            select.AddItem("EventStart", traits);
        }

        if (noMorale)
        {
            select.AddItem("NoMorale", traits, Color.gray);
        }
        else
        {
           select.AddItem("NoMorale", traits);
        }

        if (eliminated)
        {
            select.AddItem("Eliminated", traits, Color.gray);
        }
        else
        {
           select.AddItem("Eliminated", traits);
        }

        select.AddItem("Mythos", traits);
        select.AddItem("EndRound", traits);
        select.AddItem("StartRound", traits);

        traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { new StringKey("val", "MONSTER").Translate() });

        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            select.AddItem("Defeated" + kv.Key, traits);
            select.AddItem("DefeatedUnique" + kv.Key, traits);
        }

        HashSet<string> vars = new HashSet<string>();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.CustomMonster)
            {
                select.AddItem("Defeated" + kv.Key, traits);
                select.AddItem("DefeatedUnique" + kv.Key, traits);
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

        traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { new StringKey("val", "VARS").Translate() });

        foreach (string s in vars)
        {
            select.AddItem("Var" + s.Substring(1), traits);
        }

        select.Draw();
    }

    public void SelectEventTrigger(string trigger)
    {
        eventComponent.trigger = trigger;
        Update();
    }

    public void SetAudio()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();

        UIWindowSelectionListTraits select = new UIWindowSelectionListAudio(SelectEventAudio, new StringKey("val", "SELECT", new StringKey("val", "AUDIO")));

        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;

        select.AddItem("{NONE}", "");

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { new StringKey("val", "FILE").Translate() });

        foreach (string s in Directory.GetFiles(relativePath, "*.ogg", SearchOption.AllDirectories))
        {
            select.AddItem(s.Substring(relativePath.Length + 1), traits);
        }

        foreach (KeyValuePair<string, AudioData> kv in game.cd.audio)
        {
            traits = new Dictionary<string, IEnumerable<string>>();
            traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { "FFG" });
            traits.Add(new StringKey("val", "TRAITS").Translate(), kv.Value.traits);

            select.AddItem(kv.Key, traits);
        }
        select.ExcludeExpansions();
        select.Draw();
    }

    public void SelectEventAudio(string audio)
    {
        Game game = Game.Get();
        eventComponent.audio = audio;
        if (game.cd.audio.ContainsKey(eventComponent.audio))
        {
            game.audioControl.Play(game.cd.audio[eventComponent.audio].file);
        }
        else
        {
            string path = Path.GetDirectoryName(Game.Get().quest.qd.questPath) + Path.DirectorySeparatorChar + eventComponent.audio;
            game.audioControl.Play(path);
        }
        Update();
    }

    public void AddMusic(int index)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();

        UIWindowSelectionListTraits select = new UIWindowSelectionListAudio(delegate(string s) { SelectMusic(index, s); }, new StringKey("val", "SELECT", new StringKey("val", "AUDIO")));

        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;

        select.AddItem("{NONE}", "");

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { new StringKey("val", "FILE").Translate() });

        foreach (string s in Directory.GetFiles(relativePath, "*.ogg", SearchOption.AllDirectories))
        {
            select.AddItem(s.Substring(relativePath.Length + 1), traits);
        }

        foreach (KeyValuePair<string, AudioData> kv in game.cd.audio)
        {
            traits = new Dictionary<string, IEnumerable<string>>();
            traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { "FFG" });
            traits.Add(new StringKey("val", "TRAITS").Translate(), kv.Value.traits);

            select.AddItem(kv.Key, traits);
        }
        select.ExcludeExpansions();
        select.Draw();
    }

    public void SelectMusic(int index, string music)
    {
        eventComponent.music.Insert(index, music);
        Update();
    }

    public void RemoveMusic(int index)
    {
        eventComponent.music.RemoveAt(index);
        Update();
    }

    public void SetHighlight()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();

        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(SelectEventHighlight, new StringKey("val", "SELECT", CommonStringKeys.EVENT));

        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;

        select.AddItem("{NONE}", "");

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                select.AddItem(kv.Value);
            }
        }
        select.Draw();
    }

    public void SelectEventHighlight(string eventName)
    {
        eventComponent.heroListName = eventName;
        Update();
    }

    public void SetHeroCount(bool max)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();

        UIWindowSelectionList select = new UIWindowSelectionList(delegate(string s) { SelectEventHeroCount(max, s); }, new StringKey("val", "SELECT", CommonStringKeys.NUMBER));
        for (int i = 0; i <= game.gameType.MaxHeroes(); i++)
        {
            select.AddItem(i.ToString());
        }
        select.Draw();
    }

    public void SelectEventHeroCount(bool max, string number)
    {
        if (max)
        {
            int.TryParse(number, out eventComponent.maxHeroes);
        }
        else
        {
            int.TryParse(number, out eventComponent.minHeroes);
        }
        Update();
    }

    public void AddVisibility(bool add, int index = -1)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();

        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(delegate (string s) { SelectAddVisibility(add, index, s); }, new StringKey("val", "SELECT", new StringKey("val", "COMPONENT")));

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { "Special" });

        select.AddItem("#boardcomponents", traits);
        select.AddItem("#monsters", traits);
        select.AddItem("#shop", traits);

        if (game.gameType is D2EGameType || game.gameType is IAGameType)
        {
            select.AddNewComponentItem("Door");
        }
        select.AddNewComponentItem("Tile");
        select.AddNewComponentItem("Token");
        select.AddNewComponentItem("UI");
        select.AddNewComponentItem("QItem");

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Door || kv.Value is QuestData.Tile || kv.Value is QuestData.Token || kv.Value is QuestData.UI)
            {
                select.AddItem(kv.Value);
            }
            if (kv.Value is QuestData.Spawn)
            {
                select.AddItem(kv.Value);
            }
            if (kv.Value is QuestData.QItem)
            {
                select.AddItem(kv.Value);
            }
        }
        select.Draw();
    }

    public void SelectAddVisibility(bool add, int index, string component)
    {
        string target = component;
        int i;
        if (component.Equals("{NEW:Door}"))
        {
            i = 0;
            while (game.quest.qd.components.ContainsKey("Door" + i))
            {
                i++;
            }
            target = "Door" + i;
            QuestData.Door door = new QuestData.Door(target);
            Game.Get().quest.qd.components.Add(target, door);

            CameraController cc = GameObject.FindObjectOfType<CameraController>();
            door.location.x = game.gameType.TileRound() * Mathf.Round(cc.gameObject.transform.position.x / game.gameType.TileRound());
            door.location.y = game.gameType.TileRound() * Mathf.Round(cc.gameObject.transform.position.y / game.gameType.TileRound());

            game.quest.Add(target);
        }
        if (component.Equals("{NEW:Tile}"))
        {
            i = 0;
            while (game.quest.qd.components.ContainsKey("Tile" + i))
            {
                i++;
            }
            target = "Tile" + i;
            QuestData.Tile tile = new QuestData.Tile(target);
            Game.Get().quest.qd.components.Add(target, tile);

            CameraController cc = GameObject.FindObjectOfType<CameraController>();
            tile.location.x = game.gameType.TileRound() * Mathf.Round(cc.gameObject.transform.position.x / game.gameType.TileRound());
            tile.location.y = game.gameType.TileRound() * Mathf.Round(cc.gameObject.transform.position.y / game.gameType.TileRound());

            game.quest.Add(target);
        }
        if (component.Equals("{NEW:Token}"))
        {
            i = 0;
            while (game.quest.qd.components.ContainsKey("Token" + i))
            {
                i++;
            }
            target = "Token" + i;
            QuestData.Token token = new QuestData.Token(target);
            Game.Get().quest.qd.components.Add(target, token);

            CameraController cc = GameObject.FindObjectOfType<CameraController>();
            token.location.x = game.gameType.TileRound() * Mathf.Round(cc.gameObject.transform.position.x / game.gameType.TileRound());
            token.location.y = game.gameType.TileRound() * Mathf.Round(cc.gameObject.transform.position.y / game.gameType.TileRound());

            game.quest.Add(target);
        }
        if (component.Equals("{NEW:UI}"))
        {
            i = 0;
            while (game.quest.qd.components.ContainsKey("UI" + i))
            {
                i++;
            }
            target = "UI" + i;
            Game.Get().quest.qd.components.Add(target, new QuestData.UI(target));
        }
        if (component.Equals("{NEW:QItem}"))
        {
            i = 0;
            while (game.quest.qd.components.ContainsKey("QItem" + i))
            {
                i++;
            }
            target = "QItem" + i;
            Game.Get().quest.qd.components.Add(target, new QuestData.QItem(target));
        }

        if (index != -1)
        {
            if (add)
            {
                eventComponent.addComponents[index] = target;
            }
            else
            {
                eventComponent.removeComponents[index] = target;
            }
            Update();
            return;
        }
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
        for (i = 0; i < oldC.Length; i++)
        {
            newC[i] = oldC[i];
        }

        newC[i] = target;

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
        int.TryParse(quotaUIE.GetText(), out eventComponent.quota);
        Update();
    }

    public void SetQuotaInt()
    {
        eventComponent.quotaVar = "";
        Update();
    }

    public void SetQuotaVar()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(SelectQuotaVar, new StringKey("val", "SELECT", VAR));

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { "Quest" });
        select.AddItem("{" + CommonStringKeys.NEW.Translate() + "}", "{NEW}", traits);

        AddQuestVars(select);

        select.Draw();
    }

    public void SelectQuotaVar(string var)
    {
        if (var.Equals("{NEW}"))
        {
            varText = new QuestEditorTextEdit(VAR_NAME, "", delegate { NewQuotaVar(); });
            varText.EditText();
        }
        else
        {
            eventComponent.quotaVar = var;
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
        LocalizationRead.dicts["qst"].Remove(eventComponent.genKey("button" + count));
        Update();
    }

    public void SetButtonColor(int number)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionList select = new UIWindowSelectionList(delegate(string s) { SelectButtonColour(number, s); }, CommonStringKeys.SELECT_ITEM);

        foreach (string s in ColorUtil.LookUp().Keys)
        {
            select.AddItem(s);
        }

        select.Draw();
    }

    public void SelectButtonColour(int number, string color)
    {
        eventComponent.buttonColors[number - 1] = color;
        Update();
    }

    public void UpdateButtonLabel(int number)
    {
        if (!buttonUIE[number - 1].Empty() && buttonUIE[number - 1].Changed())
        {
            LocalizationRead.updateScenarioText(eventComponent.genKey("button" + number), buttonUIE[number - 1].GetText());
        }
    }

    public void SetEvent(int index, int button)
    {
        AddEvent(index, button, true);
    }

    public void AddEvent(int index, int button, bool replace = false)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();

        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(delegate(string s) { SelectAddEvent(index, button, replace, s); }, new StringKey("val", "SELECT", CommonStringKeys.EVENT));

        select.AddNewComponentItem("Event");
        select.AddNewComponentItem("Spawn");
        if (game.gameType is MoMGameType)
        {
            select.AddNewComponentItem("Puzzle");
        }

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                select.AddItem(kv.Value);
            }
        }

        select.Draw();
    }

    public void SelectAddEvent(int index, int button, bool replace, string eventName)
    {
        string toAdd = eventName;
        Game game = Game.Get();
        if (eventName.Equals("{NEW:Event}"))
        {
            int i = 0;
            while (game.quest.qd.components.ContainsKey("Event" + i))
            {
                i++;
            }
            toAdd = "Event" + i;
            Game.Get().quest.qd.components.Add(toAdd, new QuestData.Event(toAdd));
        }

        if (eventName.Equals("{NEW:Spawn}"))
        {
            int i = 0;
            while (game.quest.qd.components.ContainsKey("Spawn" + i))
            {
                i++;
            }
            toAdd = "Spawn" + i;
            Game.Get().quest.qd.components.Add(toAdd, new QuestData.Spawn(toAdd));
        }

        if (eventName.Equals("{NEW:Puzzle}"))
        {
            int i = 0;
            while (game.quest.qd.components.ContainsKey("Puzzle" + i))
            {
                i++;
            }
            toAdd = "Puzzle" + i;
            Game.Get().quest.qd.components.Add(toAdd, new QuestData.Puzzle(toAdd));
        }

        if (replace)
        {
            eventComponent.nextEvent[button - 1][index] = toAdd;
        }
        else
        {
            eventComponent.nextEvent[button - 1].Insert(index, toAdd);
        }
        Update();
    }

    public void RemoveEvent(int index, int button)
    {
        eventComponent.nextEvent[button - 1].RemoveAt(index);
        Update();
    }

    public void AddTestOp()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(delegate(string s) { SelectAddOp(s); }, new StringKey("val", "SELECT", VAR));

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { "Quest" });
        select.AddItem("{" + CommonStringKeys.NEW.Translate() + "}", "{NEW}", traits);

        AddQuestVars(select);

        traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { "#" });

        select.AddItem("#monsters", traits);
        select.AddItem("#heroes", traits);
        select.AddItem("#round", traits);
        select.AddItem("#eliminated", traits);
        foreach (ContentData.ContentPack pack in Game.Get().cd.allPacks)
        {
            if (pack.id.Length > 0)
            {
                select.AddItem("#" + pack.id, traits);
            }
        }
        foreach (HeroData hero in Game.Get().cd.heroes.Values)
        {
            if (hero.sectionName.Length > 0)
            {
                select.AddItem("#" + hero.sectionName, traits);
            }
        }
        select.Draw();
    }

    public void AddAssignOp()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(delegate (string s) { SelectAddOp(s, false); }, new StringKey("val", "SELECT", VAR));

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { "Quest" });
        select.AddItem("{" + CommonStringKeys.NEW.Translate() + "}", "{NEW}", traits);

        AddQuestVars(select);

        select.Draw();
    }

    public void AddQuestVars(UIWindowSelectionListTraits list)
    {
        HashSet<string> vars = new HashSet<string>();
        HashSet<string> dollarVars = new HashSet<string>();

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

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { "Quest" });
        foreach (string s in vars)
        {
            list.AddItem(s, traits);
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

        traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { "$" });
        foreach (string s in dollarVars)
        {
            list.AddItem(s, traits);
        }
    }

    public static HashSet<string> ExtractVarsFromEvent(QuestData.Event e)
    {
        HashSet<string> vars = new HashSet<string>();
        foreach (VarOperation op in e.operations)
        {
            vars.Add(op.var);
            if (op.value.Length > 0 && op.value[0] != '#' && !char.IsNumber(op.value[0]) && op.value[0] != '-' && op.value[0] != '.')
            {
                vars.Add(op.value);
            }
        }

        if (e.VarTests == null) return vars;

        foreach (VarTestsComponent tc in e.VarTests.VarTestsComponents)
        {
            if (tc is VarOperation)
            {
                VarOperation op = (VarOperation)tc;
                if (op.var.Length > 0 && op.var[0] != '#')
                {
                    vars.Add(op.var);
                }
                if (op.value.Length > 0 && op.value[0] != '#' && !char.IsNumber(op.value[0]) && op.value[0] != '-' && op.value[0] != '.')
                {
                    vars.Add(op.value);
                }
            }
        }
        return vars;
    }

    public void SelectAddParenthesis(bool test = true)
    {
        eventComponent.VarTests.Add(new VarTestsParenthesis(")"));
        eventComponent.VarTests.Add(new VarTestsParenthesis("("));
        Update();
    }

    public void SelectAddOp(string var, bool test = true)
    {
        VarOperation op = new VarOperation();
        op.var = var;
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
                if (eventComponent.VarTests.VarTestsComponents.Count == 0)
                {
                    eventComponent.VarTests.Add(op);
                }
                else
                {
                    eventComponent.VarTests.Add(new VarTestsLogicalOperator());
                    eventComponent.VarTests.Add(op);
                }
            }
            else
            {
                eventComponent.operations.Add(op);
            }
            Update();
        }
    }

    public void NewVar(VarOperation op, bool test)
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
                //eventComponent.conditions.Add(op);
                eventComponent.VarTests.Add(op);
            }
            else
            {
                eventComponent.operations.Add(op);
            }
        }
        Update();
    }

    public void SetTestOpp(VarOperation op)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionList select = new UIWindowSelectionList(delegate (string s) { SelectSetOp(op, s); }, new StringKey("val", "SELECT", OP));

        select.AddItem("==");
        select.AddItem("!=");
        select.AddItem(">=");
        select.AddItem("<=");
        select.AddItem(">");
        select.AddItem("<");

        select.Draw();
    }

    public void SetAssignOpp(VarOperation op)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionList select = new UIWindowSelectionList(delegate (string s) { SelectSetOp(op, s); }, new StringKey("val", "SELECT", OP));

        select.AddItem("=");
        select.AddItem("+");
        select.AddItem("-");
        select.AddItem("*");
        select.AddItem("/");
        select.AddItem("%");

        select.Draw();
    }

    public void SelectSetOp(VarOperation op, string operation)
    {
        op.operation = operation;
        Update();
    }

    public void SetValue(VarOperation op)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(delegate(string s) { SelectSetValue(op, s); }, new StringKey("val", "SELECT", VALUE));

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { "Quest" });
        select.AddItem("{" + CommonStringKeys.NUMBER.Translate() + "}", "{NUMBER}", traits);

        AddQuestVars(select);

        traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { "#" });

        select.AddItem("#monsters", traits);
        select.AddItem("#heroes", traits);
        select.AddItem("#round", traits);
        select.AddItem("#eliminated", traits);
        foreach (ContentData.ContentPack pack in Game.Get().cd.allPacks)
        {
            if (pack.id.Length > 0)
            {
                select.AddItem("#" + pack.id, traits);
            }
        }
        select.Draw();
    }


    public void SelectSetValue(VarOperation op, string value)
    {
        if (value.Equals("{NUMBER}"))
        {
            // Vars doesnt localize
            varText = new QuestEditorTextEdit(
                new StringKey("val","X_COLON",NUMBER), 
                "", delegate { SetNumValue(op); });
            varText.EditText();
        }
        else
        {
            op.value = value;
            Update();
        }
    }

    public void SetNumValue(VarOperation op)
    {
        
        if (varText.value.StartsWith("#rand"))
        {
            // rand integer value

            string randLimit = varText.value.Substring(5);
            int value;
            int.TryParse(randLimit, out value);

            // The minimal random number is 1
            if (value == 0)
            {
                value = 1;
            }

            op.value = "#rand" + value.ToString();
        
        } else {
            // float value
            float value;
            float.TryParse(varText.value, out value);
            op.value = value.ToString();
        }
        Update();
    }

    // only tests element are removed by index
    public void RemoveOp(int index)
    {
        if (index < eventComponent.VarTests.VarTestsComponents.Count)
            eventComponent.VarTests.Remove(index);
        Update();
    }

    // only operations element are removed by operation
    public void RemoveOp(VarOperation op)
    {
        if (eventComponent.operations.Contains(op))
            eventComponent.operations.Remove(op);
        Update();
    }
}
