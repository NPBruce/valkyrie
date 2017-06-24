using Assets.Scripts.Content;
﻿using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.UI;

// Next stage button is used by MoM to move between investigators and monsters
public class LogWindow
{
    public Dictionary<string, UIElementEditable> valueUIE;

    public bool developerToggle = false;

    // Construct and display
    public LogWindow()
    {
        developerToggle = Game.Get().testMode;
        Update();
    }

    public void Update(bool toggle = false)
    {
        Destroyer.Dialog();

        developerToggle ^= toggle;

        float textWidth = 28;
        float textStart = UIScaler.GetHCenter(-14.5f);
        if (developerToggle)
        {
            textWidth = 20;
            textStart = UIScaler.GetHCenter(-18.5f);
        }

        Game game = Game.Get();
        game.logWindow = this;
        // white background because font rendering is broken
        UIElement ui = new UIElement();
        ui.SetLocation(textStart, 0.5f, textWidth, 24.5f);
        ui.SetBGColor(Color.white);
        ui = new UIElement();
        ui.SetLocation(textStart + textWidth, 0.5f, 1, 24.5f);

        UIElementScrollVertical scrollArea = new UIElementScrollVertical();
        scrollArea.SetLocation(textStart, 0.5f, textWidth + 1, 24.5f);
        scrollArea.SetBGColor(Color.clear);
        new UIElementBorder(scrollArea);

        float offset = 0.5f;
        foreach (Quest.LogEntry e in game.quest.log)
        {
            string entry = e.GetEntry(developerToggle).Trim('\n');
            if (entry.Length == 0) continue;
            ui = new UIElement(scrollArea.GetScrollTransform());
            float height = UIElement.GetStringHeight(entry, textWidth);
            ui.SetLocation(0, offset, textWidth, height);
            ui.SetText(entry, Color.black);
            ui.SetBGColor(Color.white);

            offset += height;
        }
        scrollArea.SetScrollSize(offset);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-3f), 25, 6, 2);
        ui.SetText(CommonStringKeys.CLOSE);
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Destroyer.Dialog);
        new UIElementBorder(ui);

        if (developerToggle)
        {
            DrawVarList();
        }
    }

    public void DrawVarList()
    {
        UIElementScrollVertical scrollArea = new UIElementScrollVertical();
        scrollArea.SetLocation(UIScaler.GetHCenter(2.5f), 0.5f, 16, 24.5f);
        new UIElementBorder(scrollArea);

        // List of vars
        float offset = 0.1f;
        valueUIE = new Dictionary<string, UIElementEditable>();
        foreach (KeyValuePair<string, float> kv in Game.Get().quest.vars.vars)
        {
            string key = kv.Key;

            UIElement ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetLocation(0, offset, 12, 1.2f);
            ui.SetBGColor(Color.white);
            ui.SetText(key, Color.black);
            ui.SetButton(delegate { UpdateValue(key); });
            new UIElementBorder(ui);

            UIElementEditable uie = new UIElementEditable(scrollArea.GetScrollTransform());
            uie.SetLocation(12.5f, offset, 2.5f, 1.2f);
            uie.SetBGColor(Color.white);
            uie.SetText(kv.Value.ToString(), Color.black);
            uie.SetSingleLine();
            uie.SetButton(delegate { UpdateValue(key); });
            new UIElementBorder(uie);
            valueUIE.Add(key, uie);

            offset += 1.4f;
        }
        scrollArea.SetScrollSize(offset);
    }

    public void UpdateValue(string key)
    {
        float value;
        float.TryParse(valueUIE[key].GetText(), out value);
        Game.Get().quest.vars.SetValue(key, value);
        Destroyer.Dialog();
        Update();
    }
}
