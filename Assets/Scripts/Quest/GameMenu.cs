using UnityEngine;
using System.Collections;

public class GameMenu {

	public static void Create()
    {
        if (GameObject.FindGameObjectWithTag("dialog") != null)
        {
            return;
        }

        DialogBox db = new DialogBox(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 6), new Vector2(12, 13), "");
        db.AddBorder();

        TextButton tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 7), new Vector2(10, 2f), "Undo", delegate { Undo(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 10), new Vector2(10, 2f), "Save", delegate { SaveManager.Save(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 13), new Vector2(10, 2f), "Main Menu", delegate { Destroyer.MainMenu(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 16), new Vector2(10, 2f), "Cancel", delegate { Destroyer.Dialog(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);

    }

    public static void Undo()
    {
        Game game = Game.Get();
        game.quest.Undo();
    }
}
