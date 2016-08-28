using UnityEngine;
using System.Collections;

public class InfoDialog {

    public InfoDialog(MonsterData md)
    {
        if (md == null)
        {
            Debug.Log("Warning: Invalid monster type requested.");
            return;
        }

        new DialogBox(new Vector2(10, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 12), md.info.Replace("\\n", "\n"));

        new TextButton(new Vector2(UIScaler.GetWidthUnits() - 21, 13f), new Vector2(10, 2), "Close", delegate { onClose(); });
    }

    // Close cleans up
    public void onClose()
    {
        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
    }
}
