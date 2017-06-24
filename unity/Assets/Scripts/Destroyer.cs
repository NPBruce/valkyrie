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
        Resources.UnloadUnusedAssets();
        ContentData.textureCache = null;
        // Clean up everything marked as 'dialog'
        Dialog();

        // Clean up everything marked as 'monsters'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.MONSTERS))
            Object.Destroy(go);

        // Clean up shop
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.SHOP))
            Object.Destroy(go);

        // Clean up everything marked as 'heroselect'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.HEROSELECT))
            Object.Destroy(go);

        // Clean up everything marked as 'board'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.BOARD))
            Object.Destroy(go);

        // Clean up everything marked as 'questui'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.QUESTUI))
            Object.Destroy(go);

        // Clean up everything marked as 'editor'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.EDITOR))
            Object.Destroy(go);

        // Clean up everything marked as 'uiphase'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.UIPHASE))
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
        game.testMode = false;
    }

    // All dialogs that are to be acknoledged/cancled are marked as 'dialog' and are often destroyed
    public static void Dialog()
    {
        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            Object.Destroy(go);

        CameraController.panDisable = false;
        Game.Get().logWindow = null;
        Resources.UnloadUnusedAssets();
    }
}
