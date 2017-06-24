using Assets.Scripts.Content;
using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.UI;

// Next stage button is used by MoM to move between investigators and monsters
public class SkillWindow
{
    // Construct and display
    public SkillWindow()
    {
        Update();
    }

    public void Update(int hero = 0)
    {
        Destroyer.Dialog();
        Game game = Game.Get();

        UIElement ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-15), 1, 30, 6);
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-17), 7, 34, 17);
        new UIElementBorder(ui);

        // Add a title to the page
        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-6), 1, 12, 3);
        ui.SetText(new StringKey("val", "SELECT_SKILLS"));
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetLargeFont());

        // Get all heros
        int heroCount = 0;
        // Count number of selected heroes
        foreach (Quest.Hero h in game.quest.heroes)
        {
            if (h.heroData != null) heroCount++;
        }

        float xOffset = UIScaler.GetHCenter(-11);
        if (heroCount < 4) xOffset += 3f;
        if (heroCount < 3) xOffset += 3f;

        int availableXP = 0;
        for (int i = 0; i < heroCount; i++)
        {
            int tmp = i;

            ui = new UIElement();
            ui.SetLocation(xOffset, 3.5f, 4, 4);
            ui.SetButton(delegate { Update(tmp); });
            Texture2D heroTex = ContentData.FileToTexture(game.quest.heroes[i].heroData.image);
            ui.SetImage(heroTex);
            if (i != hero)
            {
                ui.SetBGColor(new Color(0.3f, 0.3f, 0.3f));
            }

            availableXP = game.quest.heroes[i].AvailableXP();
            if (availableXP != 0)
            {
                ui = new UIElement();
                ui.SetLocation(xOffset + 2, 5.5f, 2, 2);
                ui.SetText(availableXP.ToString(), Color.blue);
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetButton(delegate { Update(tmp); });
                new UIElementBorder(ui, Color.blue);
            }

            xOffset += 6f;
        }


        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-16), 8.5f, 32, 5);
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-16), 8.5f, 2, 5);
        ui.SetText("1");
        ui.SetFontSize(UIScaler.GetLargeFont());
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-16), 13.5f, 32, 5);
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-16), 13.5f, 2, 5);
        ui.SetText("2");
        ui.SetFontSize(UIScaler.GetLargeFont());
        new UIElementBorder(ui);

        string hybridClass = game.quest.heroes[hero].hybridClass;
        if (hybridClass.Length > 0)
        {
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-16.5f), 18.5f, 11, 5);
            new UIElementBorder(ui);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-16.5f), 18.5f, 2, 5);
            ui.SetText("1");
            ui.SetFontSize(UIScaler.GetLargeFont());
            new UIElementBorder(ui);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-5.5f), 18.5f, 11, 5);
            new UIElementBorder(ui);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-5.5f), 18.5f, 2, 5);
            ui.SetText("2");
            ui.SetFontSize(UIScaler.GetLargeFont());
            new UIElementBorder(ui);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(5.5f), 18.5f, 11, 5);
            new UIElementBorder(ui);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(5.5f), 18.5f, 2, 5);
            ui.SetText("3");
            ui.SetFontSize(UIScaler.GetLargeFont());
            new UIElementBorder(ui);
        }
        else
        {
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-16), 18.5f, 32, 5);
            new UIElementBorder(ui);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-16), 18.5f, 2, 5);
            ui.SetText("3");
            ui.SetFontSize(UIScaler.GetLargeFont());
            new UIElementBorder(ui);
        }

        float[] xOffsetArray = new float[4];
        xOffsetArray[1] = UIScaler.GetHCenter(-13);
        xOffsetArray[2] = UIScaler.GetHCenter(-13);
        xOffsetArray[3] = UIScaler.GetHCenter(-8);
        float yOffset = 4;

        availableXP = game.quest.heroes[hero].AvailableXP();
        foreach (SkillData s in game.cd.skills.Values)
        {
            if (s.xp == 0) continue;
            if (game.quest.heroes[hero].className.Length == 0) continue;

            Color buttonColor = new Color(0.4f, 0.4f, 0.4f);
            if (game.quest.heroes[hero].skills.Contains(s.sectionName))
            {
                buttonColor = Color.green;
            }
            else if (s.xp <= availableXP)
            {
                buttonColor = Color.white;
            }

            string skill = s.sectionName;
            if (s.sectionName.IndexOf("Skill" + game.quest.heroes[hero].className.Substring("Class".Length)) == 0)
            {
                if (hybridClass.Length > 0 && s.xp == 3) continue;

                ui = new UIElement();
                ui.SetLocation(xOffsetArray[s.xp], yOffset + (s.xp * 5), 8, 4);
                ui.SetText(s.name, buttonColor);
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetButton(delegate { SelectSkill(hero, skill); });
                new UIElementBorder(ui, buttonColor);
                xOffsetArray[s.xp] += 10;
                continue;
            }

            if (hybridClass.Length == 0) continue;
            if (s.sectionName.IndexOf("Skill" + hybridClass.Substring("Class".Length)) != 0) continue;
            
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-25f) + (s.xp * 11f), yOffset + 15, 8, 4);
            ui.SetText(s.name, buttonColor);
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(delegate { SelectSkill(hero, skill); });
            new UIElementBorder(ui, buttonColor);
        }

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-4f), 24.5f, 8, 2);
        ui.SetText(CommonStringKeys.CLOSE);
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Destroyer.Dialog);
        new UIElementBorder(ui);
    }

    public void SelectSkill(int hero, string skill)
    {
        Game game = Game.Get();
        List<string> skills = game.quest.heroes[hero].skills;

        if (skills.Contains(skill))
        {
            skills.Remove(skill);
        }
        else
        {
            if (game.quest.heroes[hero].AvailableXP() < game.cd.skills[skill].xp) return;
            skills.Add(skill);
        }
        Update(hero);
    }
}
