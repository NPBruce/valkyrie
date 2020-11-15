using System;
using Assets.Scripts.UI.Screens;
using ValkyrieTools;

public class GameStateManager
{
    // This function takes us back to the main menu
    public static void MainMenu()
    {
        // Destroy everything
        Destroyer.Destroy();
        
        Game game = Game.Get();
        // All content data has been loaded by editor, cleanup everything
        game.cd = new ContentData(game.gameType.DataDirectory());
        // Load the base content - pack will be loaded later if required
        game.ContentLoader.LoadContentID("");

        new MainMenuScreen();
    }

    public static class Quest
    {
        // This takes us to the quest select screen
        public static void List()
        {
            // Destroy everything
            Destroyer.Destroy();
            Game game = Game.Get();
            game.SelectQuest();
        }

        // Starts the provided quest
        public static void Start(QuestData.Quest quest)
        {
            ValkyrieDebug.Log("INFO: Moving to hero selection screen");
            Destroyer.Destroy();
            Game.Get().StartQuest(quest);
        }

        // Restarts the current quest
        public static void Restart()
        {
            Game game = Game.Get();
            if (!GetCurrentQuest(game, out QuestData.Quest currentQuest))
            {
                // Failsafe. Go to quest selection if there's no valid quest loaded 
                List();
                return;
            }

            // Go back to quest details
            Destroyer.Destroy();
            new QuestDetailsScreen(currentQuest);
        }

        // Restarts the current quest from Hero Selection
        public static void RestartFromHeroSelection()
        {
            Game game = Game.Get();

            if (!GetCurrentQuest(game, out QuestData.Quest currentQuest))
            {
                // Failsafe. Go to quest selection if there's no valid quest loaded 
                List();
                return;
            }

            Start(currentQuest);
        }
    }

    public static class Editor
    {
        public static void EditQuest(string path)
        {
            ValkyrieDebug.Log("Starting Editor" + Environment.NewLine);

            Destroyer.Destroy();

            Game.Get().audioControl.StopMusic();
            QuestEditor.Begin(path);
        }

        public static void EditCurrentQuest()
        {
            var game = Game.Get();
            if (!GetCurrentQuestPath(game, out var currentQuestPath))
            {
                // Failsafe. Go to main menu if there's no valid quest loaded 
                MainMenu();
                return;
            }

            ValkyrieDebug.Log("Starting Editor" + Environment.NewLine);

            Destroyer.Destroy();

            game.audioControl.StopMusic();
            QuestEditor.Begin(currentQuestPath);
        }
    }

    private static bool GetCurrentQuestPath(Game game, out string currentQuestPath)
    {
        currentQuestPath = game.quest?.originalPath;
        if (currentQuestPath == null)
        {
            return false;
        }

        return true;
    }

    private static bool GetCurrentQuest(Game game, out QuestData.Quest currentQuest)
    {
        if (!GetCurrentQuestPath(game, out string questPath))
        {
            currentQuest = null;
            return false;
        }

        currentQuest = new QuestData.Quest(questPath);
        return true;
    }
}