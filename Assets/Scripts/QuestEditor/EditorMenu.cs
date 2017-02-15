using UnityEngine;
using System.Collections;

// Menu popup when in editor
public class EditorMenu {
    public static void Create()
    {
        if (GameObject.FindGameObjectWithTag("dialog") != null)
        {
            return;
        }

        // Menu border
        DialogBox db = new DialogBox(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 9), new Vector2(12, 13), "");
        db.AddBorder();

        TextButton tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 10), new Vector2(10, 2f), "Save", delegate { QuestEditor.Save(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 13), new Vector2(10, 2f), "Reload", delegate { QuestEditor.Reload(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 16), new Vector2(10, 2f), "Main Menu", delegate { Destroyer.MainMenu(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 19), new Vector2(10, 2f), "Cancel", delegate { Destroyer.Dialog(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);

    }
}
