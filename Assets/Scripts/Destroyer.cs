using UnityEngine;
using Assets.Scripts.UI.Screens;

// This is a helper class because we often need to clean things up.
public class Destroyer {

    // This function takes us back to the main menu
    public static void MainMenu()
    {
        // Destroy everything
        Destroy();
        new MainMenuScreen();
    }

    // This takes us to the quest select screen
    public static void QuestSelect()
    {
        // Destroy everything
        Destroy();
        Game game = Game.Get();
        game.SelectQuest();
    }

    // Destroy everything.  This still keeps game type, Valkyrie must be restarted to swap games
    public static void Destroy()
    {
        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        // Clean up everything marked as 'monsters'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("monsters"))
            Object.Destroy(go);

        // Clean up everything marked as 'heroselect'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("heroselect"))
            Object.Destroy(go);

        // Clean up everything marked as 'board'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("board"))
            Object.Destroy(go);

        // Clean up everything marked as 'questui'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("questui"))
            Object.Destroy(go);

        // Clean up everything marked as 'editor'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("editor"))
            Object.Destroy(go);

        // Clean up everything marked as 'uiphase'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("uiphase"))
            Object.Destroy(go);

        Game game = Game.Get();

        game.heroCanvas.Clean();
        game.cc.maxLimit = false;
        game.cc.minLimit = false;

        // Clear up all data
        game.cd = null;
        game.quest = null;
        game.qed = null;
        game.moraleDisplay = null;
        if (game.tokenBoard.tc != null)
        {
            game.tokenBoard.tc.Clear();
        }
        game.editMode = false;
    }

    // All dialogs that are to be acknoledged/cancled are marked as 'dialog' and are often destroyed
    public static void Dialog()
    {
        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
        Game.Get().cc.panDisable = false;
        Game.Get().logWindow = null;
    }
}
