using UnityEngine;
using System.Collections;

// This is a helper class because we often need to clean things up.
public class Destroyer {

    // This function takes us back to the main menu
    public static void MainMenu()
    {
        // Destroy everything
        Destroy();
        new MainMenu();
    }

    // This takes us to the quest select screen
    public static void QuestSelect()
    {
        // Destroy everything
        Destroy();
        Game game = Game.Get();
        game.SelectQuest();
    }

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

        // Clean up everything marked as 'herodisplay'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("herodisplay"))
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

        Game game = Game.Get();

        // Clear up all data
        if (game.eventList != null)
        {
            game.eventList.Clear();
        }
        game.cd = null;
        game.qd = null;
        game.qed = null;
        game.heros = null;
        game.monsters = null;
        game.heroesSelected = false;
        game.moraleDisplay = null;
        game.tokenBoard.tc.Clear();
        game.editMode = false;
    }

    // All dialogs that are to be acknoledged/cancled are marked as 'dialog' and are often destroyed
    public static void Dialog()
    {
        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
    }
}
