using UnityEngine;
using System.Collections;
using Assets.Scripts.Content;

// Special class for the Menu button present while in a quest
public class ToolsButton
{
    private StringKey TOOLS = new StringKey("val", "TOOLS");

    public ToolsButton()
    {
        Game game = Game.Get();
        if (!game.editMode) return;

        TextButton tb = new TextButton(new Vector2(UIScaler.GetRight(-6), 0), new Vector2(6, 1), new StringKey("val", "COMPONENTS"), delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.QUESTUI);

        tb = new TextButton(new Vector2(UIScaler.GetRight(-10), 0), new Vector2(4, 1), TOOLS, delegate { EditorTools.Create(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.QUESTUI);

        tb = new TextButton(new Vector2(UIScaler.GetRight(-14), 0), new Vector2(4, 1), new StringKey("val", "TEST"), delegate { Test(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.QUESTUI);
    }

    public void Test()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null) return;

        Game game = Game.Get();
        QuestData.Quest q = game.quest.qd.quest;
        Destroyer.Destroy();

        game.cd = new ContentData(gameType.DataDirectory());
        foreach (string pack in cd.GetPacks())
        {
            cd.LoadContent(pack);
        }

        game.testMode = true;
        // Fetch all of the quest data and initialise the quest
        game.quest = new Quest(q);
        game.heroCanvas.SetupUI();

        int heroCount = Random.Range(game.quest.qd.quest.minHero, game.quest.qd.quest.maxHero + 1);

        List<HeroData> hOptions = new List<HeroData>(game.cd.heroes);
        for (int i = 0; i < heroCount; i++)
        {
            game.quest.heroes[i].heroData = hOptions[Random.Range(0, hOptions.Count)];
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
            UpdateImages();
            UpdateStatus();
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
