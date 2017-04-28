using UnityEngine;
using System.Collections;
using Assets.Scripts.Content;

// In quest game menu
public class GameMenu {
    private static readonly StringKey SAVE = new StringKey("val","SAVE");
    private static readonly StringKey MAIN_MENU = new StringKey("val", "MAIN_MENU");
    private static readonly StringKey UNDO = new StringKey("val", "UNDO");

    // Open the menu
    public static void Create()
    {
        Game game = Game.Get();
        if (GameObject.FindGameObjectWithTag("dialog") != null)
        {
            return;
        }

        if (GameObject.FindGameObjectWithTag("activation") != null)
        {
            return;
        }

        // Take screen shot for save before menu is drawn
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Texture2D screen = new Texture2D(screenSize.width, screenSize.height, TextureFormat.RGB24, false);
        screen.ReadPixels(new Rect(0, 0, screenSize.width, screenSize.height), 0, 0);
        screen.Apply());


        // Border around menu items
        DialogBox db = new DialogBox(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 6), new Vector2(12, 13), StringKey.NULL);
        db.AddBorder();

        TextButton tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 7), new Vector2(10, 2f), UNDO, delegate { Undo(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 10), new Vector2(10, 2f), SAVE, delegate { Save(screen); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 13), new Vector2(10, 2f), MAIN_MENU, delegate { Quit(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 16), new Vector2(10, 2f), CommonStringKeys.CANCEL, delegate { Destroyer.Dialog(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

    }

    public static void Undo()
    {
        Game game = Game.Get();
        game.quest.Undo();
    }

    public static void Save(Texture2D screen)
    {
        Destroyer.Dialog();
        new SaveSelectScreen(true, screen);
    }

    public static void Quit()
    {
        Destroyer.Dialog();
        SaveManager.Save(0);
        Destroyer.MainMenu();
    }
}
