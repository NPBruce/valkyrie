using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

public class EditorComponentUI : EditorComponentEvent
{
    QuestData.UI uiComponent;

    UIElementEditable locXUIE;
    UIElementEditable locYUIE;
    UIElementEditable sizeUIE;
    UIElementEditable aspectUIE;
    UIElementEditable textSizeUIE;
    UIElementEditablePaneled textUIE;

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

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4.5f, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "IMAGE")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4.5f, offset, 15, 1);
        ui.SetText(uiComponent.imageName);
        ui.SetButton(delegate { SetImage(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 6, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "UNITS")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(6, offset, 6, 1);
        ui.SetButton(delegate { ChangeUnits(); });
        new UIElementBorder(ui);
        if (uiComponent.verticalUnits)
        {
            ui.SetText(new StringKey("val", "VERTICAL"));
        }
        else
        {
            ui.SetText(new StringKey("val", "HORIZONTAL"));
        }
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset++, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "ALIGN")));

        DrawAlignSelection(offset, -1, -1, "┏");
        DrawAlignSelection(offset, 0, -1, "━");
        DrawAlignSelection(offset, 1, -1, "┓");

        DrawAlignSelection(offset, -1, 0, "┃");
        DrawAlignSelection(offset, 0, 0, "╋");
        DrawAlignSelection(offset, 1, 0, "┃");

        DrawAlignSelection(offset, -1, 1, "┗");
        DrawAlignSelection(offset, 0, 1, "━");
        DrawAlignSelection(offset, 1, 1, "┛");
        offset += 3;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset++, 10, 1);
        ui.SetText(new StringKey("val", "POSITION"));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 2, 1);
        ui.SetText("X:");

        locXUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
        locXUIE.SetLocation(2, offset, 3, 1);
        locXUIE.SetText(uiComponent.location.x.ToString());
        locXUIE.SetSingleLine();
        locXUIE.SetButton(delegate { UpdateNumbers(); });
        new UIElementBorder(locXUIE);

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(5, offset, 2, 1);
        ui.SetText("Y:");

        locYUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
        locYUIE.SetLocation(7, offset, 3, 1);
        locYUIE.SetText(uiComponent.location.y.ToString());
        locYUIE.SetSingleLine();
        locYUIE.SetButton(delegate { UpdateNumbers(); });
        new UIElementBorder(locYUIE);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 5, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "SIZE")));

        sizeUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
        sizeUIE.SetLocation(5, offset, 3, 1);
        sizeUIE.SetText(uiComponent.size.ToString());
        sizeUIE.SetSingleLine();
        sizeUIE.SetButton(delegate { UpdateNumbers(); });
        new UIElementBorder(sizeUIE);

        if (uiComponent.imageName.Length == 0)
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(10, offset, 5, 1);
            ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "ASPECT")));

            aspectUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
            aspectUIE.SetLocation(15, offset, 3, 1);
            aspectUIE.SetText(uiComponent.aspect.ToString());
            aspectUIE.SetSingleLine();
            aspectUIE.SetButton(delegate { UpdateNumbers(); });
            new UIElementBorder(aspectUIE);
            offset += 2;

            textUIE = new UIElementEditablePaneled(Game.EDITOR, scrollArea.GetScrollTransform());
            textUIE.SetLocation(0.5f, offset, 19, 38);
            textUIE.SetText(uiComponent.uiText.Translate(true));
            offset += textUIE.HeightToTextPadding(1);
            textUIE.SetButton(delegate { UpdateUIText(); });
            new UIElementBorder(textUIE);
            offset += 1;

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0, offset, 7, 1);
            ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "TEXT_SIZE")));

            textSizeUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
            textSizeUIE.SetLocation(7, offset, 3, 1);
            textSizeUIE.SetText(uiComponent.textSize.ToString());
            textSizeUIE.SetSingleLine();
            textSizeUIE.SetButton(delegate { UpdateTextSize(); });
            new UIElementBorder(textSizeUIE);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(10, offset, 4.5f, 1);
            ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "COLOR")));

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(14.5f, offset, 5, 1);
            ui.SetText(uiComponent.textColor);
            ui.SetButton(delegate { SetColour(); });
            new UIElementBorder(ui);
            offset += 2;

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0.5f, offset, 8, 1);
            ui.SetText(uiComponent.textColor);
            ui.SetButton(delegate { ToggleBorder(); });
            new UIElementBorder(ui);
            if (uiComponent.border)
            {
                ui.SetText(new StringKey("val", "BORDER"));
            }
            else
            {
                ui.SetText(new StringKey("val", "NO_BORDER"));
            }
        }
        offset += 2;

        DrawUIComponent();

        return offset;
    }

    public void DrawAlignSelection(float offset, int x, int y, string label)
    {
        Color selected = (uiComponent.hAlign == x && uiComponent.vAlign == y) ? Color.white : new Color(0.3f, 0.3f, 0.3f);
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(5 + x, offset + y, 1, 1);
        ui.SetText(label, selected);
        ui.SetButton(delegate { SetAlign(x, y); });
        new UIElementBorder(ui, selected);
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
            panel.transform.SetParent(game.uICanvas.transform);
            panel.transform.SetAsFirstSibling();
            panel.AddComponent<RectTransform>();
            panel.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, Screen.height);
            panel.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, Screen.width);
        }

        // Create objects
        GameObject unityObject = new GameObject("greyzonea");
        unityObject.tag = Game.EDITOR;
        unityObject.transform.SetParent(panel.transform);
        UnityEngine.UI.Image panela = unityObject.AddComponent<UnityEngine.UI.Image>();
        panela.color = new Color(1f, 1f, 1f, 0.3f);
        unityObject = new GameObject("greyzoneb");
        unityObject.tag = Game.EDITOR;
        unityObject.transform.SetParent(panel.transform);
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

    override public float AddEventTrigger(float offset)
    {
        return offset;
    }

    public void SetImage()
    {
        UIWindowSelectionListImage select = new UIWindowSelectionListImage(SelectImage, SELECT_IMAGE.Translate());
        select.AddItem("{NONE}", "");

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(new StringKey("val", "SOURCE").Translate(), new string[] { new StringKey("val", "FILE").Translate() });
        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;
        foreach (string s in Directory.GetFiles(relativePath, "*.png", SearchOption.AllDirectories))
        {
            select.AddItem(s.Substring(relativePath.Length + 1), traits);
        }
        foreach (string s in Directory.GetFiles(relativePath, "*.jpg", SearchOption.AllDirectories))
        {
            select.AddItem(s.Substring(relativePath.Length + 1), traits);
        }
        foreach (KeyValuePair<string, ImageData> kv in Game.Get().cd.images)
        {
            select.AddItem(kv.Value);
        }
        select.ExcludeExpansions();
        select.Draw();
    }

    public void SelectImage(string image)
    {
        uiComponent.imageName = image;
        Game.Get().quest.Remove(uiComponent.sectionName);
        Game.Get().quest.Add(uiComponent.sectionName);
        if (uiComponent.imageName.Length > 0)
        {
            LocalizationRead.dicts["qst"].Remove(uiComponent.uitext_key);
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
        if (!locXUIE.GetText().Equals(""))
        {
            float.TryParse(locXUIE.GetText(), out uiComponent.location.x);
        }
        if (!locYUIE.GetText().Equals(""))
        {
            float.TryParse(locYUIE.GetText(), out uiComponent.location.y);
        }
        if (!sizeUIE.GetText().Equals(""))
        {
            float.TryParse(sizeUIE.GetText(), out uiComponent.size);
        }
        if (aspectUIE != null && !aspectUIE.GetText().Equals(""))
        {
            float.TryParse(aspectUIE.GetText(), out uiComponent.aspect);
        }
        Game.Get().quest.Remove(uiComponent.sectionName);
        Game.Get().quest.Add(uiComponent.sectionName);
        Update();
    }

    public void UpdateUIText()
    {
        Game game = Game.Get();

        if (!textUIE.Empty() && textUIE.Changed())
        {
            LocalizationRead.updateScenarioText(uiComponent.uitext_key, textUIE.GetText());
        }
        Game.Get().quest.Remove(uiComponent.sectionName);
        Game.Get().quest.Add(uiComponent.sectionName);
        Update();
    }

    public void UpdateTextSize()
    {
        float.TryParse(textSizeUIE.GetText(), out uiComponent.textSize);
        Game.Get().quest.Remove(uiComponent.sectionName);
        Game.Get().quest.Add(uiComponent.sectionName);
        Update();
    }

    public void SetColour()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();

        UIWindowSelectionList select = new UIWindowSelectionList(SelectColour, CommonStringKeys.SELECT_ITEM);
        foreach (KeyValuePair<string, string> kv in ColorUtil.LookUp())
        {
            select.AddItem(new StringKey("val", kv.Key));
        }
        select.Draw();
    }

    public void SelectColour(string color)
    {
        uiComponent.textColor = color;
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
