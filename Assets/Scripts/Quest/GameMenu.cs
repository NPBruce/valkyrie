using UnityEngine;
using System.Collections;

// In quest game menu
public class GameMenu {

    // Open the menu
	public static void Create()
    {
        Game game = Game.Get();
        if (GameObject.FindGameObjectWithTag("dialog") != null)
        {
            return;
        }

        // Border around menu items
        DialogBox db = new DialogBox(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 6), new Vector2(12, 13), "");
        db.AddBorder();

        TextButton tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 7), new Vector2(10, 2f), "Undo", delegate { Undo(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 10), new Vector2(10, 2f), "Save", delegate { Save(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 13), new Vector2(10, 2f), "Main Menu", delegate { Destroyer.MainMenu(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 16), new Vector2(10, 2f), "Cancel", delegate { Destroyer.Dialog(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

    }

    public static void Undo()
    {
        Game game = Game.Get();
        game.quest.Undo();
    }

    public static void Save()
    {
        SaveManager.Save();
        Destroyer.Dialog();
    }
}
