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
        Game game = Game.Get();
        game.logWindow = this;
        // white background because font rendering is broken
        string log = "";
        foreach (Quest.LogEntry e in game.quest.log)
        {
            log += e.GetEntry(developerToggle);
        }
        log.Trim('\n');

        UIElementScrollVertical scrollArea = new UIElementScrollVertical();
        UIElement ui = new UIElement(scrollArea.GetScrollTransform());
        float height = 0;
        if (developerToggle)
        {
            scrollArea.SetLocation(UIScaler.GetHCenter(-18.5f), 0.5f, 21, 24.5f);
            height = UIElement.GetStringHeight(log, 20);
            ui.SetLocation(0, 0, 20, height);
        }
        else
        {
            height = UIElement.GetStringHeight(log, 28);
            scrollArea.SetLocation(UIScaler.GetHCenter(-14.5f), 0.5f, 29, 24.5f);
            ui.SetLocation(0, 0, 28, height);
        }
        ui.SetText(log, Color.black);
        ui.SetBGColor(Color.white);

        new UIElementBorder(scrollArea);
        scrollArea.SetScrollSize(height);

        new TextButton(new Vector2(UIScaler.GetHCenter(-3f), 25f), new Vector2(6, 2), CommonStringKeys.CLOSE, delegate { Destroyer.Dialog(); });

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
