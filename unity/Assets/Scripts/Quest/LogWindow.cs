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

        DialogBox db = null;
        if (developerToggle)
        {
            db = new DialogBox(new Vector2(UIScaler.GetHCenter(-18f), 0.5f), new Vector2(20, 24.5f), 
                new StringKey(null, log, false), Color.black, new Color(1, 1, 1, 0.9f));
        }
        else
        {
            db = new DialogBox(new Vector2(UIScaler.GetHCenter(-14f), 0.5f), new Vector2(28, 24.5f), 
                new StringKey(null, log, false), Color.black, new Color(1, 1, 1, 0.9f));
        }

        db.AddBorder();
        // This material works for the mask, but only renders in black
        db.textObj.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
        UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();
        scrollRect.content = db.textObj.GetComponent<RectTransform>();
        scrollRect.horizontal = false;
        RectTransform textRect = db.textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(textRect.rect.width, db.textObj.GetComponent<UnityEngine.UI.Text>().preferredHeight);
        scrollRect.verticalNormalizedPosition = 0f;
        scrollRect.scrollSensitivity = 27f;

        UnityEngine.UI.Mask mask = db.background.AddComponent<UnityEngine.UI.Mask>();

        new TextButton(new Vector2(UIScaler.GetHCenter(-3f), 25f), new Vector2(6, 2), CommonStringKeys.CLOSE, delegate { Destroyer.Dialog(); });

        if (developerToggle)
        {
            DrawVarList();
        }
    }

    public void DrawVarList()
    {
        UIElementScrollVertical scrollArea = new UIElementScrollVertical();
        scrollArea.SetLocation(UIScaler.GetHCenter(2f), 0.5f, 16, 24.5f);
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
