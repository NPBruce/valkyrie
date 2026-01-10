using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI.Screens;
using Assets.Scripts.UI;

// Special class for the Menu button present while in a quest
public class ToolsButton
{
    private StringKey TOOLS = new StringKey("val", "TOOLS");

    public ToolsButton()
    {
        Game game = Game.Get();
        if (!game.editMode) return;

        UIElement ui = new UIElement(Game.QUESTUI);
        ui.SetLocation(UIScaler.GetRight(-6), 0, 6, 1);
        ui.SetText(new StringKey("val", "COMPONENTS"));
        ui.SetButton(delegate { QuestEditorData.TypeSelect(); });
        new UIElementBorder(ui);

        ui = new UIElement(Game.QUESTUI);
        ui.SetLocation(UIScaler.GetRight(-10), 0, 4, 1);
        ui.SetText(TOOLS);
        ui.SetButton(EditorTools.Create);
        new UIElementBorder(ui);

        ui = new UIElement(Game.QUESTUI);
        ui.SetLocation(UIScaler.GetRight(-18), 0, 8, 1);
        ui.SetText(new StringKey("val", "SAVE_TEST"));
        ui.SetButton(Test);
        new UIElementBorder(ui);
    }

    private int heroCount = 0;

    public void Test()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null) return;

        Game game = Game.Get();
        int min = game.CurrentQuest.qd.quest.minHero;
        int max = game.CurrentQuest.qd.quest.maxHero;

        string val = "";
        Dictionary<string, string> savedData = game.config.data.Get("Valkyrie");
        if (savedData != null && savedData.ContainsKey("QuestEditorHeroCount"))
        {
            val = savedData["QuestEditorHeroCount"];
        }

        if (!int.TryParse(val, out heroCount))
        {
            heroCount = min;
        }

        if (heroCount < min) heroCount = min;
        if (heroCount > max) heroCount = max;

        DrawHeroSelection();
    }

    public void DrawHeroSelection()
    {
        Game game = Game.Get();
        int min = game.CurrentQuest.qd.quest.minHero;
        int max = game.CurrentQuest.qd.quest.maxHero;

        // Border
        UIElement ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-10), 9, 20, 10);
        new UIElementBorder(ui);

        // Label
        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-9), 10, 18, 2);
        if (game.gameType is MoMGameType)
        {
            ui.SetText(new StringKey("val", "QUEST_EDITOR_INVESTIGATOR_COUNT_LABEL"));
        }
        else
        {
            ui.SetText(new StringKey("val", "QUEST_EDITOR_HERO_COUNT_LABEL"));
        }

        // Minus
        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-5), 13, 2, 2);
        if (heroCount > min)
        {
            ui.SetButton(HeroCountDec);
            ui.SetText(CommonStringKeys.MINUS);
            new UIElementBorder(ui);
        }
        else
        {
            ui.SetText(CommonStringKeys.MINUS, Color.grey);
            new UIElementBorder(ui, Color.grey);
        }

        // Count
        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-2), 13, 4, 2);
        ui.SetText(heroCount.ToString());
        new UIElementBorder(ui);

        // Plus
        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(3), 13, 2, 2);
        if (heroCount < max)
        {
            ui.SetButton(HeroCountInc);
            ui.SetText(CommonStringKeys.PLUS);
            new UIElementBorder(ui);
        }
        else
        {
            ui.SetText(CommonStringKeys.PLUS, Color.grey);
            new UIElementBorder(ui, Color.grey);
        }

        // Start
        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-9), 16, 8, 2);
        ui.SetText(new StringKey("val", "START"));
        ui.SetButton(StartTest);
        new UIElementBorder(ui);

        // Cancel
        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(1), 16, 8, 2);
        ui.SetText(CommonStringKeys.CANCEL);
        ui.SetButton(CancelTest);
        new UIElementBorder(ui);
    }

    public void HeroCountInc()
    {
        heroCount++;
        Destroyer.Dialog();
        DrawHeroSelection();
    }

    public void HeroCountDec()
    {
        heroCount--;
        Destroyer.Dialog();
        DrawHeroSelection();
    }

    public void CancelTest()
    {
        Destroyer.Dialog();
    }

    public void StartTest()
    {
        Game game = Game.Get();
        Destroyer.Dialog();

        game.config.data.Add("Valkyrie", "QuestEditorHeroCount", heroCount.ToString());
        game.config.Save();

        QuestEditor.Save();

        string path = game.CurrentQuest.questPath;
        Destroyer.Destroy();

        // All content data has been loaded by editor, cleanup everything
        game.cd = new ContentData(game.gameType.DataDirectory());
        // Load the base content
        game.ContentLoader.LoadContentID("");
        // Load current configuration
        Dictionary<string, string> packs = game.config.data.Get(game.gameType.TypeName() + "Packs");
        if (packs != null)
        {
            foreach (KeyValuePair<string, string> kv in packs)
            {
                game.ContentLoader.LoadContentID(kv.Key);
            }
        }

        game.testMode = true;
        // Fetch all of the quest data and initialise the quest
        game.CurrentQuest = new Quest(new QuestData.Quest(path));
        game.heroCanvas.SetupUI();

        // int heroCount = Random.Range(game.CurrentQuest.qd.quest.minHero, game.CurrentQuest.qd.quest.maxHero + 1);

        List<HeroData> hOptions = new List<HeroData>(game.cd.Values<HeroData>());
        for (int i = 0; i < heroCount; i++)
        {
            game.CurrentQuest.heroes[i].heroData = hOptions[Random.Range(0, hOptions.Count)];
            game.CurrentQuest.vars.SetValue("#" + game.CurrentQuest.heroes[i].heroData.sectionName, 1);
            hOptions.Remove(game.CurrentQuest.heroes[i].heroData);
        }

        // Starting morale is number of heros
        game.CurrentQuest.vars.SetValue("$%morale", heroCount);
        // Set quest flag based on hero count
        game.CurrentQuest.vars.SetValue("#heroes", heroCount);
        game.CurrentQuest.heroesSelected = true;

        // Clear off heros if not required
        if (!game.gameType.DisplayHeroes())
        {
            game.heroCanvas.Clean();
        }
        else
        {
            game.heroCanvas.UpdateImages();
            game.heroCanvas.UpdateStatus();
        }

        // Draw morale if required
        if (game.gameType is D2EGameType)
        {
            new ClassSelectionScreen();
        }
        else
        {
            new InvestigatorItems();
        }
    }
}

