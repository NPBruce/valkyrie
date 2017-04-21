using Assets.Scripts.Content;
﻿using UnityEngine;
using System.Collections.Generic;

// Next stage button is used by MoM to move between investigators and monsters
public class LogWindow
{
    public Dictionary<string, DialogBoxEditable> valueDBE;

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
        game.cc.panDisable = true;
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

        UnityEngine.UI.Mask mask = db.background.AddComponent<UnityEngine.UI.Mask>();

        new TextButton(new Vector2(UIScaler.GetHCenter(-3f), 25f), new Vector2(6, 2), CommonStringKeys.CLOSE, delegate { Destroyer.Dialog(); });

        if (developerToggle)
        {
            DrawVarList();
        }
    }

    public void DrawVarList()
    {

        DialogBox db = new DialogBox(new Vector2(UIScaler.GetHCenter(2f), 0.5f), new Vector2(16, 24.5f), StringKey.NULL);
        db.AddBorder();
        db.background.AddComponent<UnityEngine.UI.Mask>();
        UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

        GameObject scrollArea = new GameObject("scroll");
        RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
        scrollArea.transform.parent = db.background.transform;
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 16f * UIScaler.GetPixelsPerUnit());
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 1);

        scrollRect.content = scrollInnerRect;
        scrollRect.horizontal = false;

        // List of vars
        float offset = 1;
        valueDBE = new Dictionary<string, DialogBoxEditable>();
        foreach (KeyValuePair<string, float> kv in Game.Get().quest.vars.vars)
        {
            string key = kv.Key;

            db = new DialogBox(
                new Vector2(UIScaler.GetHCenter(2.5f), offset), new Vector2(12, 1.2f), 
                new StringKey(null, key, false), Color.black, Color.white);
            db.textObj.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
            db.background.transform.parent = scrollArea.transform;
            db.AddBorder();
            // Variables value modify dont need localization
            DialogBoxEditable dbe = new DialogBoxEditable(
                new Vector2(UIScaler.GetHCenter(14.5f), offset), new Vector2(3, 1.2f), 
                kv.Value.ToString(),
                delegate { UpdateValue(key); }, Color.black, Color.white);
            dbe.setMaterialAndBackgroundTransformParent((Material)Resources.Load("Fonts/FontMaterial"),scrollArea.transform);
            dbe.AddBorder();
            valueDBE.Add(key, dbe);

            offset += 1.4f;
        }
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (offset - 1) * UIScaler.GetPixelsPerUnit());
    }

    public void UpdateValue(string key)
    {
        float value;
        float.TryParse(valueDBE[key].Text, out value);
        Game.Get().quest.vars.SetValue(key, value);
        Destroyer.Dialog();
        Update();
    }
}
