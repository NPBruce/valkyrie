using UnityEngine;
using Assets.Scripts.Content;
using System.IO;
using System.Collections.Generic;
using Assets.Scripts.UI;

public class ReorderComponents
{
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

        UIWindowSelectionList select = new UIWindowSelectionList(ReorderSource, new StringKey("val", "SELECT", new StringKey("val", "FILE")));
        foreach (string s in sources)
        {
            select.AddItem(s);
        }
        select.Draw();
    }

    public void ReorderSource(string s)
    {
        source = s;
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
        UIElementScrollVertical scrollArea = new UIElementScrollVertical();
        scrollArea.SetLocation(UIScaler.GetHCenter(-11.5f), 2, 23, 25);
        new UIElementBorder(scrollArea);

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
                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(21f, offset, 1, 1);
                ui.SetBGColor(Color.green);
                ui.SetText("▽", Color.black);
                ui.SetButton(delegate { IncComponent(tmp); });
                offset += 1.05f;

                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(0, offset, 1, 1);
                ui.SetBGColor(Color.green);
                ui.SetText("△", Color.black);
                ui.SetButton(delegate { IncComponent(tmp); });
            }
            ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetLocation(1.05f, offset, 19.9f, 1);
            ui.SetBGColor(Color.white);
            ui.SetText(c.sectionName, Color.black);
            names.Add(ui);
            first = false;
        }
        offset += 1.05f;

        if (offset < 25) offset = 25;

        scrollArea.SetScrollSize(offset);

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