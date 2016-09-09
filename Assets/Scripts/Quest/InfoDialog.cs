using UnityEngine;
using System.Collections;

public class InfoDialog {

    public InfoDialog(Quest.Monster m)
    {
        if (m == null)
        {
            Debug.Log("Warning: Invalid monster type requested.");
            return;
        }

        DialogBox db = new DialogBox(new Vector2(10, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 12), m.monsterData.info.Replace("\\n", "\n"));
        db.AddBorder();

        if (m.unique)
        {
            db = new DialogBox(new Vector2(12, 13f), new Vector2(UIScaler.GetWidthUnits() - 24, 2), m.uniqueTitle, Color.red);
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            db.AddBorder();
            db = new DialogBox(new Vector2(10, 15f), new Vector2(UIScaler.GetWidthUnits() - 20, 8), m.uniqueText.Replace("\\n", "\n"));
            db.AddBorder(Color.red);
            new TextButton(new Vector2(UIScaler.GetWidthUnits() - 21, 23.5f), new Vector2(10, 2), "Close", delegate { onClose(); });
        }
        else
        {
            new TextButton(new Vector2(UIScaler.GetWidthUnits() - 21, 13f), new Vector2(10, 2), "Close", delegate { onClose(); });
        }
    }

    // Close cleans up
    public void onClose()
    {
        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
    }
}
