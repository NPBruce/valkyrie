using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentUI : EditorComponentEvent
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

    public EditorComponentUI(string nameIn) : base(nameIn)
    {
    }

    override public float AddPosition(float offset)
    {
        return offset;
    }

    override public void Highlight()
    {
    }

    override public float AddSubEventComponents(float offset)
    {
        uiComponent = component as QuestData.UI;

        DialogBox db = new DialogBox(new Vector2(0, offset), new Vector2(4.5f, 1), new StringKey("val", "X_COLON", new StringKey("val", "IMAGE")));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        TextButton tb = new TextButton(new Vector2(4.5f, offset), new Vector2(15, 1),
            new StringKey(null, uiComponent.imageName, false), delegate { SetImage(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);
        offset += 2;

        db = new DialogBox(new Vector2(0, offset), new Vector2(6, 1), new StringKey("val", "X_COLON", new StringKey("val", "UNITS")));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        if (uiComponent.verticalUnits)
        {
            tb = new TextButton(new Vector2(6, offset), new Vector2(6, 1), new StringKey("val", "VERTICAL"), delegate { ChangeUnits(); });
        }
        else
        {
            tb = new TextButton(new Vector2(6, offset), new Vector2(6, 1), new StringKey("val", "HORIZONTAL"), delegate { ChangeUnits(); });
        }
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);
        offset += 2;

        db = new DialogBox(new Vector2(0, offset++), new Vector2(4, 1), new StringKey("val", "ALIGN"));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(4, offset), new Vector2(1, 1), new StringKey(null, "┏", false), delegate { SetAlign(-1, -1); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(5, offset), new Vector2(1, 1), new StringKey(null, "━", false), delegate { SetAlign(0, -1); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(6, offset++), new Vector2(1, 1), new StringKey(null, "┓", false), delegate { SetAlign(1, -1); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(4, offset), new Vector2(1, 1), new StringKey(null, "┃", false), delegate { SetAlign(-1, 0); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(5, offset), new Vector2(1, 1), new StringKey(null, "╋", false), delegate { SetAlign(0, 0); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(6, offset++), new Vector2(1, 1), new StringKey(null, "┃", false), delegate { SetAlign(1, 0); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(4, offset), new Vector2(1, 1), new StringKey(null, "┗", false), delegate { SetAlign(-1, 1); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(5, offset), new Vector2(1, 1), new StringKey(null, "━", false), delegate { SetAlign(0, 1); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(6, offset), new Vector2(1, 1), new StringKey(null, "┛", false), delegate { SetAlign(1, 1); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);
        offset += 2;

        db = new DialogBox(new Vector2(0, offset++), new Vector2(10, 1), new StringKey("val", "POSITION"));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        db = new DialogBox(new Vector2(0, offset), new Vector2(2, 1), new StringKey(null, "X:", false));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        locXDBE = new DialogBoxEditable(new Vector2(2, offset), new Vector2(3, 1),
            uiComponent.location.x.ToString(), false, delegate { UpdateNumbers(); });
        locXDBE.background.transform.parent = scrollArea.transform;
        locXDBE.ApplyTag(Game.EDITOR);
        locXDBE.AddBorder();

        db = new DialogBox(new Vector2(5, offset), new Vector2(2, 1), new StringKey(null, "Y:", false));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        locYDBE = new DialogBoxEditable(new Vector2(7, offset++), new Vector2(3, 1),
            uiComponent.location.y.ToString(), false, delegate { UpdateNumbers(); });
        locYDBE.background.transform.parent = scrollArea.transform;
        locYDBE.ApplyTag(Game.EDITOR);
        locYDBE.AddBorder();

        db = new DialogBox(new Vector2(0, offset), new Vector2(5, 1), new StringKey("val", "SIZE"));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        sizeDBE = new DialogBoxEditable(new Vector2(5, offset), new Vector2(3, 1),
            uiComponent.size.ToString(), false, delegate { UpdateNumbers(); });
        sizeDBE.background.transform.parent = scrollArea.transform;
        sizeDBE.ApplyTag(Game.EDITOR);
        sizeDBE.AddBorder();

        if (uiComponent.imageName.Length == 0)
        {
            db = new DialogBox(new Vector2(10, offset), new Vector2(5, 1), new StringKey("val", "ASPECT"));
            db.background.transform.parent = scrollArea.transform;
            db.ApplyTag(Game.EDITOR);

            aspectDBE = new DialogBoxEditable(new Vector2(15, offset), new Vector2(3, 1),
                uiComponent.aspect.ToString(), false, delegate { UpdateNumbers(); });
            aspectDBE.background.transform.parent = scrollArea.transform;
            aspectDBE.ApplyTag(Game.EDITOR);
            aspectDBE.AddBorder();
            offset += 2;

            textDBE = new PaneledDialogBoxEditable(
                new Vector2(0, offset), new Vector2(20, 6),
                uiComponent.uiText.Translate(true),
                delegate { UpdateUIText(); });
            textDBE.background.transform.parent = scrollArea.transform;
            textDBE.ApplyTag(Game.EDITOR);
            textDBE.AddBorder();
            offset += 7;

            db = new DialogBox(new Vector2(0, offset), new Vector2(7, 1), new StringKey("val", "TEXT_SIZE"));
            db.background.transform.parent = scrollArea.transform;
            db.ApplyTag(Game.EDITOR);

            textSizeDBE = new DialogBoxEditable(new Vector2(7, offset), new Vector2(3, 1),
                uiComponent.textSize.ToString(), false, delegate { UpdateTextSize(); });
            textSizeDBE.background.transform.parent = scrollArea.transform;
            textSizeDBE.ApplyTag(Game.EDITOR);
            textSizeDBE.AddBorder();

            db = new DialogBox(new Vector2(10, offset), new Vector2(5, 1), new StringKey("val", "COLOR"));
            db.background.transform.parent = scrollArea.transform;
            db.ApplyTag(Game.EDITOR);

            tb = new TextButton(new Vector2(15, offset++), new Vector2(5, 1), new StringKey(null, uiComponent.textColor, false), delegate { SetColour(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.transform.parent = scrollArea.transform;
            tb.ApplyTag(Game.EDITOR);

            if (uiComponent.border)
            {
                tb = new TextButton(new Vector2(0, offset), new Vector2(8, 1), new StringKey("val", "BORDER"), delegate { ToggleBorder(); });
            }
            else
            {
                tb = new TextButton(new Vector2(0, offset), new Vector2(8, 1), new StringKey("val", "NO_BORDER"), delegate { ToggleBorder(); });
            }
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.transform.parent = scrollArea.transform;
            tb.ApplyTag(Game.EDITOR);
        }
        offset += 2;

        DrawUIComponent();

        return offset;
    }

    public void DrawUIComponent()
    {
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

    public void UpdateUIText()
    {
        Game game = Game.Get();

        if (textDBE.CheckTextChangedAndNotEmpty())
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
