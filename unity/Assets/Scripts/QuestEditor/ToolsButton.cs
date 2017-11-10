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

    public void Test()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null) return;

        QuestEditor.Save();

        Game game = Game.Get();
        string path = game.quest.questPath;
        Destroyer.Destroy();

        game.cd = new ContentData(game.gameType.DataDirectory());
        foreach (string pack in game.cd.GetPacks())
        {
            game.cd.LoadContent(pack);
        }

        game.testMode = true;
        // Fetch all of the quest data and initialise the quest
        game.quest = new Quest(new QuestData.Quest(path));
        game.heroCanvas.SetupUI();

        int heroCount = Random.Range(game.quest.qd.quest.minHero, game.quest.qd.quest.maxHero + 1);

        List<HeroData> hOptions = new List<HeroData>(game.cd.heroes.Values);
        for (int i = 0; i < heroCount; i++)
        {
            game.quest.heroes[i].heroData = hOptions[Random.Range(0, hOptions.Count)];
            game.quest.vars.SetValue("#" + game.quest.heroes[i].heroData.sectionName, 1);
            hOptions.Remove(game.quest.heroes[i].heroData);
        }

        // Starting morale is number of heros
        game.quest.vars.SetValue("$%morale", heroCount);
        // Set quest flag based on hero count
        game.quest.vars.SetValue("#heroes", heroCount);
        game.quest.heroesSelected = true;

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
