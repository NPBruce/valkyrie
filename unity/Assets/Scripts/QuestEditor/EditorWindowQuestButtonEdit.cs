using System;
using System.Collections.Generic;
using System.Linq;
using Content;
using Assets.Scripts.UI;
using UnityEngine;

public class EditorWindowQuestButtonEdit
{
    private readonly StringKey title;
    private readonly QuestButtonData newButton;
    private readonly QuestButtonData originalButton;
    private readonly Action<QuestButtonData> okAction;
    private static readonly List<QuestButtonAction> QUEST_BUTTON_ACTIONS = Enum.GetValues(typeof(QuestButtonAction)).Cast<QuestButtonAction>().ToList();

    public EditorWindowQuestButtonEdit(StringKey title, QuestButtonData questButton, Action<QuestButtonData> okAction)
    {
        this.title = title;
        this.originalButton = questButton;
        this.newButton = new QuestButtonData(questButton.Label, questButton.EventNames, new VarTests(questButton.Condition.VarTestsComponents.ToList()), questButton.RawConditionFailedAction);
        this.okAction = okAction;
    }

    public void Draw()
    {
        Update();
    }

    public void Update()
    {
        // Border
        UIElement ui = new UIElement();
        var width = 22f;
        float leftBorder = UIScaler.GetHCenter(-11);
        ui.SetLocation(leftBorder, 2, width, 26);
        new UIElementBorder(ui);


        // Title
        ui = new UIElement();
        ui.SetLocation(leftBorder + 1, 2, 20, 1);
        ui.SetText(title);

        var scrollArea = new UIElementScrollVertical(Game.EDITOR);
        scrollArea.SetLocation(leftBorder, 3, width, 20);
        scrollArea.SetScrollSize(newButton.Condition.VarTestsComponents.Count + 4);
        new UIElementBorder(scrollArea);

        EditorComponentVarTestsUtil.AddEventVarConditionComponents(scrollArea.GetScrollTransform(), 1.5f, 1.5f, newButton, Update);

        ui = new UIElement();
        ui.SetLocation(leftBorder, 23, width, 5);
        new UIElementBorder(ui);
        
        var optionWidth = (width - 1f) / QUEST_BUTTON_ACTIONS.Count;
        for (int i = 0; i < QUEST_BUTTON_ACTIONS.Count; i++)
        {
            var action = QUEST_BUTTON_ACTIONS[i];
            // None button
            ui = new UIElement();
            ui.SetLocation(leftBorder + 0.5f + i * optionWidth, 24, optionWidth, 1);
            Color buttonColor = newButton.ConditionFailedAction == action ? Color.white : Color.gray;
            ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
            ui.SetText(action.ToString(), buttonColor);
            ui.SetButton(delegate
            {
                this.newButton.RawConditionFailedAction = action;
                Update();
            });
            new UIElementBorder(ui, buttonColor);

        }

        // Cancel button
        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-10f), 26, 9, 1);
        ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
        ui.SetText(CommonStringKeys.CANCEL);
        ui.SetButton(delegate
        {
            Destroyer.Dialog();
            okAction.Invoke(originalButton);
        });
        new UIElementBorder(ui);

        // OK button
        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(1), 26, 9, 1);
        ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
        ui.SetText(CommonStringKeys.OK);
        ui.SetButton(delegate
        {
            if (!newButton.HasCondition)
            {
                this.newButton.RawConditionFailedAction = null;
            }
            Destroyer.Dialog();
            okAction.Invoke(newButton);
        });
        new UIElementBorder(ui);
    }
}