using UnityEngine;
using System.Collections;

public class GameMenu {

	public static void Create()
    {
        if (GameObject.FindGameObjectWithTag("dialog") != null)
        {
            return;
        }

        DialogBox db = new DialogBox(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 9), new Vector2(12, 7), "");
        db.AddBorder();

        TextButton tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 10), new Vector2(10, 2f), "Main Menu", delegate { Destroyer.MainMenu(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 13), new Vector2(10, 2f), "Cancel", delegate { Cancel(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);

    }

    public static void Cancel()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
    }
}
