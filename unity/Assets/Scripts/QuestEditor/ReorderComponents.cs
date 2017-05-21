using UnityEngine;
using Assets.Scripts.Content;
using System.IO;
using System.Collections.Generic;
using Assets.Scripts.UI;

public class ReorderComponents
{
    EditorSelectionList sourceESL;
    string source = "";
    List<UIElement> names;

    public ReorderComponents()
    {
        Game game = Game.Get();

        HashSet<string> sources = new HashSet<string>();
        foreach (QuestData.QuestComponent c in game.quest.qd.components.Values)
        {
            if (!(c is PerilData)) sources.Add(c.source);
        }

        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        foreach (string s in sources)
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s));
        }

        sourceESL = new EditorSelectionList(new StringKey("val", "SELECT", new StringKey("val", "FILE")), list, delegate { ReorderSource(); });
        sourceESL.SelectItem();
    }

    public void ReorderSource()
    {
        source = sourceESL.selection;
        Draw();
    }

    public void Draw()
    {
        Game game = Game.Get();
        Destroyer.Dialog();

        // Border
        UIElement ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-12.5f), 0, 25, 30);
        new UIElementBorder(ui);

        // Title
        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-12), 0, 24, 1);
        ui.SetText(source);

        // Scroll BG
        DialogBox db = new DialogBox(new Vector2(UIScaler.GetHCenter(-11.5f), 2), new Vector2(23, 25), StringKey.NULL);
        db.AddBorder();
        db.background.AddComponent<UnityEngine.UI.Mask>();
        UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

        GameObject scrollArea = new GameObject("scroll");
        RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
        scrollArea.transform.SetParent(db.background.transform);
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 22 * UIScaler.GetPixelsPerUnit());
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 1);

        GameObject scrollBarObj = new GameObject("scrollbar");
        scrollBarObj.transform.SetParent(db.background.transform);
        RectTransform scrollBarRect = scrollBarObj.AddComponent<RectTransform>();
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 25 * UIScaler.GetPixelsPerUnit());
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 22 * UIScaler.GetPixelsPerUnit(), 1 * UIScaler.GetPixelsPerUnit());
        UnityEngine.UI.Scrollbar scrollBar = scrollBarObj.AddComponent<UnityEngine.UI.Scrollbar>();
        scrollBar.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
        scrollRect.verticalScrollbar = scrollBar;

        GameObject scrollBarHandle = new GameObject("scrollbarhandle");
        scrollBarHandle.transform.SetParent(scrollBarObj.transform);
        scrollBarHandle.AddComponent<UnityEngine.UI.Image>();
        scrollBarHandle.GetComponent<UnityEngine.UI.Image>().color = new Color(0.7f, 0.7f, 0.7f);
        scrollBar.handleRect = scrollBarHandle.GetComponent<RectTransform>();
        scrollBar.handleRect.offsetMin = Vector2.zero;
        scrollBar.handleRect.offsetMax = Vector2.zero;

        scrollRect.content = scrollInnerRect;
        scrollRect.horizontal = false;
        scrollRect.scrollSensitivity = 27f;

        bool first = true;
        float offset = 0;
        names = new List<UIElement>();
        int index = 0;
        foreach (QuestData.QuestComponent c in game.quest.qd.components.Values)
        {
            if (!c.source.Equals(source)) continue;

            string name = c.sectionName;

            int tmp = index++;
            if (!first)
            {
                ui = new UIElement(scrollArea.transform);
                ui.SetLocation(21f, offset, 1, 1);
                ui.SetBGColor(Color.green);
                ui.SetText("▽", Color.black);
                ui.SetButton(delegate { IncComponent(tmp); });
                offset += 1.05f;

                ui = new UIElement(scrollArea.transform);
                ui.SetLocation(0, offset, 1, 1);
                ui.SetBGColor(Color.green);
                ui.SetText("△", Color.black);
                ui.SetButton(delegate { IncComponent(tmp); });
            }
            ui = new UIElement(scrollArea.transform);
            ui.SetLocation(1.05f, offset, 19.9f, 1);
            ui.SetBGColor(Color.white);
            ui.SetText(c.sectionName, Color.black);
            names.Add(ui);
            first = false;
        }
        offset += 1.05f;

        if (offset < 25) offset = 25;

        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, offset * UIScaler.GetPixelsPerUnit());

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-4.5f), 28, 9, 1);
        ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
        ui.SetText(CommonStringKeys.FINISHED);
        ui.SetButton(delegate { Destroyer.Dialog(); });
        new UIElementBorder(ui);
    }

    public void Update()
    {
        int i = 0;
        foreach (QuestData.QuestComponent c in Game.Get().quest.qd.components.Values)
        {
            if (c.source.Equals(source))
            {
                names[i++].SetText(c.sectionName, Color.black);
            }
        }
    }

    public void IncComponent(int index)
    {
        string name = names[index].GetText();
        Game game = Game.Get();
        Dictionary<string, QuestData.QuestComponent> preDict = new Dictionary<string, QuestData.QuestComponent>();
        List<QuestData.QuestComponent> postList = new List<QuestData.QuestComponent>();
        foreach (QuestData.QuestComponent c in game.quest.qd.components.Values)
        {
            if (c.sectionName.Equals(name))
            {
                preDict.Add(c.sectionName, c);
            }
            else
            {
                if (c.source.Equals(game.quest.qd.components[name].source))
                {
                    foreach (QuestData.QuestComponent post in postList)
                    {
                        preDict.Add(post.sectionName, post);
                    }
                    postList = new List<QuestData.QuestComponent>();
                }
                postList.Add(c);
            }
        }

        foreach (QuestData.QuestComponent post in postList)
        {
            preDict.Add(post.sectionName, post);
        }

        game.quest.qd.components = preDict;
        Update();
    }
}