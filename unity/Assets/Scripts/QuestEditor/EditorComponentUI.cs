﻿using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentUI : EditorComponent
{
    QuestData.UI uiComponent;
    EditorSelectionList imageList;
    DialogBoxEditable locXDBE;
    DialogBoxEditable locYDBE;
    DialogBoxEditable sizeDBE;
    DialogBoxEditable aspectDBE;
    PaneledDialogBoxEditable textDBE;
    DialogBoxEditable textSizeDBE;
    EditorSelectionList colorList;

    private readonly StringKey SELECT_IMAGE = new StringKey("val", "SELECT_IMAGE");

    public EditorComponentUI(string nameIn) : base()
    {
        Game game = Game.Get();
        uiComponent = game.quest.qd.components[nameIn] as QuestData.UI;
        component = uiComponent;
        name = component.sectionName;
        Update();
    }

    override public void Update()
    {
        base.Update();
        Game game = Game.Get();

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(2, 1), CommonStringKeys.UI, delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(2, 0), new Vector2(17, 1),
            new StringKey(null, name.Substring("UI".Length), false), delegate { QuestEditorData.ListUI(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), CommonStringKeys.E, delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(0, 2), new Vector2(20, 1),
            new StringKey(null, uiComponent.imageName, false), delegate { SetImage(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.EDITOR);

        DialogBox db = new DialogBox(new Vector2(0, 4), new Vector2(6, 1), new StringKey("val", "UNITS"));
        db.ApplyTag(Game.EDITOR);

        if (uiComponent.verticalUnits)
        {
            tb = new TextButton(new Vector2(6, 4), new Vector2(6, 1), new StringKey("val", "VERTICAL"), delegate { ChangeUnits(); });
        }
        else
        {
            tb = new TextButton(new Vector2(6, 4), new Vector2(6, 1), new StringKey("val", "HORIZONTAL"), delegate { ChangeUnits(); });
        }
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.EDITOR);

        db = new DialogBox(new Vector2(0, 6), new Vector2(4, 1), new StringKey("val", "ALIGN"));
        db.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(4, 7), new Vector2(1, 1), new StringKey(null, "┏", false), delegate { SetAlign(-1, -1); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(5, 7), new Vector2(1, 1), new StringKey(null, "━", false), delegate { SetAlign(0, -1); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(6, 7), new Vector2(1, 1), new StringKey(null, "┓", false), delegate { SetAlign(1, -1); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(4, 8), new Vector2(1, 1), new StringKey(null, "┃", false), delegate { SetAlign(-1, 0); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(5, 8), new Vector2(1, 1), new StringKey(null, "╋", false), delegate { SetAlign(0, 0); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(6, 8), new Vector2(1, 1), new StringKey(null, "┃", false), delegate { SetAlign(1, 0); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(4, 9), new Vector2(1, 1), new StringKey(null, "┗", false), delegate { SetAlign(-1, 1); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(5, 9), new Vector2(1, 1), new StringKey(null, "━", false), delegate { SetAlign(0, 1); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(6, 9), new Vector2(1, 1), new StringKey(null, "┛", false), delegate { SetAlign(1, 1); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.EDITOR);

        db = new DialogBox(new Vector2(0, 11), new Vector2(10, 1), new StringKey("val", "POSITION"));
        db.ApplyTag(Game.EDITOR);

        db = new DialogBox(new Vector2(0, 12), new Vector2(2, 1), new StringKey(null, "X:", false));
        db.ApplyTag(Game.EDITOR);

        locXDBE = new DialogBoxEditable(new Vector2(2, 12), new Vector2(3, 1),
            uiComponent.location.x.ToString(), false, delegate { UpdateNumbers(); });

        locXDBE.ApplyTag(Game.EDITOR);
        locXDBE.AddBorder();

        db = new DialogBox(new Vector2(5, 12), new Vector2(2, 1), new StringKey(null, "Y:", false));
        db.ApplyTag(Game.EDITOR);

        locYDBE = new DialogBoxEditable(new Vector2(7, 12), new Vector2(3, 1),
            uiComponent.location.y.ToString(), false, delegate { UpdateNumbers(); });
        locYDBE.ApplyTag(Game.EDITOR);
        locYDBE.AddBorder();

        db = new DialogBox(new Vector2(0, 13), new Vector2(5, 1), new StringKey("val", "SIZE"));
        db.ApplyTag(Game.EDITOR);

        sizeDBE = new DialogBoxEditable(new Vector2(5, 13), new Vector2(3, 1),
            uiComponent.size.ToString(), false, delegate { UpdateNumbers(); });
        sizeDBE.ApplyTag(Game.EDITOR);
        sizeDBE.AddBorder();

        if (uiComponent.imageName.Length == 0)
        {
            db = new DialogBox(new Vector2(10, 13), new Vector2(5, 1), new StringKey("val", "ASPECT"));
            db.ApplyTag(Game.EDITOR);

            aspectDBE = new DialogBoxEditable(new Vector2(15, 13), new Vector2(3, 1),
                uiComponent.aspect.ToString(), false, delegate { UpdateNumbers(); });
            aspectDBE.ApplyTag(Game.EDITOR);
            aspectDBE.AddBorder();

            textDBE = new PaneledDialogBoxEditable(
                new Vector2(0, 15), new Vector2(20, 6),
                game.quest.qd.quest.description.Translate(true),
                delegate { UpdateText(); });
            textDBE.ApplyTag(Game.EDITOR);
            textDBE.AddBorder();

            db = new DialogBox(new Vector2(0, 22), new Vector2(7, 1), new StringKey("val", "TEXT_SIZE"));
            db.ApplyTag(Game.EDITOR);

            textSizeDBE = new DialogBoxEditable(new Vector2(7, 22), new Vector2(3, 1),
                uiComponent.textSize.ToString(), false, delegate { UpdateTextSize(); });
            textSizeDBE.ApplyTag(Game.EDITOR);
            textSizeDBE.AddBorder();

            db = new DialogBox(new Vector2(10, 22), new Vector2(5, 1), new StringKey("val", "COLOR"));
            db.ApplyTag(Game.EDITOR);

            tb = new TextButton(new Vector2(15, 22), new Vector2(5, 1), new StringKey(null, uiComponent.textColor, false), delegate { SetColour(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag(Game.EDITOR);

            if (uiComponent.border)
            {
                tb = new TextButton(new Vector2(0, 23), new Vector2(8, 1), new StringKey("val", "BORDER"), delegate { ToggleBorder(); });
            }
            else
            {
                tb = new TextButton(new Vector2(0, 23), new Vector2(8, 1), new StringKey("val", "NO_BORDER"), delegate { ToggleBorder(); });
            }
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag(Game.EDITOR);
        }

        tb = new TextButton(new Vector2(0, 25), new Vector2(8, 1), CommonStringKeys.EVENT, delegate { QuestEditorData.SelectAsEvent(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.EDITOR);

        game.quest.ChangeAlpha(uiComponent.sectionName, 1f);

        // Create a grey zone outside of the 16x9 boundary
        // Find quest UI panel
        GameObject panel = GameObject.Find("QuestUIPanel");
        if (panel == null)
        {
            // Create UI Panel
            panel = new GameObject("QuestUIPanel");
            panel.tag = Game.BOARD;
            panel.transform.parent = game.uICanvas.transform;
            panel.transform.SetAsFirstSibling();
            panel.AddComponent<RectTransform>();
            panel.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, Screen.height);
            panel.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, Screen.width);
        }

        // Create objects
        GameObject unityObject = new GameObject("greyzonea");
        unityObject.tag = Game.EDITOR;
        unityObject.transform.parent = panel.transform;
        UnityEngine.UI.Image panela = unityObject.AddComponent<UnityEngine.UI.Image>();
        panela.color = new Color(1f, 1f, 1f, 0.3f);
        unityObject = new GameObject("greyzoneb");
        unityObject.tag = Game.EDITOR;
        unityObject.transform.parent = panel.transform;
        UnityEngine.UI.Image panelb = unityObject.AddComponent<UnityEngine.UI.Image>();
        panelb.color = new Color(1f, 1f, 1f, 0.3f);
        panelb.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
        panelb.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);

        if (uiComponent.verticalUnits)
        {
            // Size bars for wider screens
            // Position and Scale assume a 16x9 aspect
            float templateWidth = (float)Screen.height * 16f / 10f;
            float hOffset = (float)Screen.width - templateWidth;
            panela.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, Screen.height);

            if (uiComponent.hAlign < 0)
            {
                panela.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, hOffset);
            }
            else if (uiComponent.hAlign > 0)
            {
                panela.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, hOffset);
            }
            else
            {
                panela.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, hOffset / 2);
                panelb.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, hOffset / 2);
                panelb.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, Screen.height);
            }
        }
        else
        {
            // letterboxing for taller screens
            // Position and Scale assume a 16x9 aspect
            float templateHeight = (float)Screen.width * 9f / 16f;
            float vOffset = (float)Screen.height - templateHeight;
            panela.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, Screen.width);

            if (uiComponent.vAlign < 0)
            {
                panela.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, vOffset);
            }
            else if (uiComponent.vAlign > 0)
            {
                panela.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, vOffset);
            }
            else
            {
                panela.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, vOffset / 2);
                panelb.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, vOffset / 2);
                panelb.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, Screen.width);
            }
        }
    }

    public void SetImage()
    {
        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;
        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        list.Add(new EditorSelectionList.SelectionListEntry(""));
        foreach (string s in Directory.GetFiles(relativePath, "*.png", SearchOption.AllDirectories))
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s.Substring(relativePath.Length + 1), "File"));
        }
        foreach (string s in Directory.GetFiles(relativePath, "*.jpg", SearchOption.AllDirectories))
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s.Substring(relativePath.Length + 1), "File"));
        }
        foreach (KeyValuePair<string, ImageData> kv in Game.Get().cd.images)
        {
            list.Add(new EditorSelectionList.SelectionListEntry(kv.Key, "FFG"));
        }
        imageList = new EditorSelectionList(SELECT_IMAGE, list, delegate { SelectImage(); });
        imageList.SelectItem();
    }

    public void SelectImage()
    {
        uiComponent.imageName = imageList.selection;
        Game.Get().quest.Remove(uiComponent.sectionName);
        Game.Get().quest.Add(uiComponent.sectionName);
        if (uiComponent.imageName.Length > 0)
        {
            LocalizationRead.scenarioDict.Remove(uiComponent.uitext_key);
            uiComponent.border = false;
            uiComponent.aspect = 1;
        }
        else
        {
            LocalizationRead.updateScenarioText(uiComponent.uitext_key, "");
        }
        Update();
    }

    public void ChangeUnits()
    {
        uiComponent.verticalUnits = !uiComponent.verticalUnits;
        Game.Get().quest.Remove(uiComponent.sectionName);
        Game.Get().quest.Add(uiComponent.sectionName);
        Update();
    }

    public void SetAlign(int x, int y)
    {
        uiComponent.hAlign = x;
        uiComponent.vAlign = y;
        Game.Get().quest.Remove(uiComponent.sectionName);
        Game.Get().quest.Add(uiComponent.sectionName);
        Update();
    }

    public void UpdateNumbers()
    {
        if (!locXDBE.Text.Equals(""))
        {
            float.TryParse(locXDBE.Text, out uiComponent.location.x);
        }
        if (!locYDBE.Text.Equals(""))
        {
            float.TryParse(locYDBE.Text, out uiComponent.location.y);
        }
        if (!sizeDBE.Text.Equals(""))
        {
            float.TryParse(sizeDBE.Text, out uiComponent.size);
        }
        if (aspectDBE != null && !aspectDBE.Text.Equals(""))
        {
            float.TryParse(aspectDBE.Text, out uiComponent.aspect);
        }
        Game.Get().quest.Remove(uiComponent.sectionName);
        Game.Get().quest.Add(uiComponent.sectionName);
        Update();
    }

    public void UpdateText()
    {
        Game game = Game.Get();

        if (!textDBE.Text.Equals(""))
        {
            LocalizationRead.updateScenarioText(uiComponent.uitext_key, textDBE.Text);
        }
        Game.Get().quest.Remove(uiComponent.sectionName);
        Game.Get().quest.Add(uiComponent.sectionName);
        Update();
    }

    public void UpdateTextSize()
    {
        float.TryParse(textSizeDBE.Text, out uiComponent.textSize);
        Game.Get().quest.Remove(uiComponent.sectionName);
        Game.Get().quest.Add(uiComponent.sectionName);
        Update();
    }

    public void SetColour()
    {
        List<EditorSelectionList.SelectionListEntry> colours = new List<EditorSelectionList.SelectionListEntry>();
        foreach (KeyValuePair<string, string> kv in ColorUtil.LookUp())
        {
            colours.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(kv.Key));
        }
        colorList = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, colours, delegate { SelectColour(); });
        colorList.SelectItem();
    }

    public void SelectColour()
    {
        uiComponent.textColor = colorList.selection;
        Game.Get().quest.Remove(uiComponent.sectionName);
        Game.Get().quest.Add(uiComponent.sectionName);
        Update();
    }

    public void ToggleBorder()
    {
        uiComponent.border = !uiComponent.border;
        Update();
    }
}
