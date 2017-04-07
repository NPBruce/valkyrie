using UnityEngine;
using System.Collections;
using Assets.Scripts.Content;

// Monster information dialog (additional rules)
public class InfoDialog {

    public InfoDialog(Quest.Monster m)
    {
        if (m == null)
        {
            ValkyrieDebug.Log("Warning: Invalid monster type requested.");
            return;
        }

        // box with monster info
        DialogBox db = new DialogBox(
            new Vector2(10, 0.5f), 
            new Vector2(UIScaler.GetWidthUnits() - 20, 12), 
            m.monsterData.info);
        db.AddBorder();

        // Unique monsters have additional info
        if (m.unique)
        {
            db = new DialogBox(new Vector2(12, 13f), new Vector2(UIScaler.GetWidthUnits() - 24, 2), 
                new StringKey(m.uniqueTitle,false), Color.red);
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            db.AddBorder();
            string uniqueText = EventManager.SymbolReplace(m.uniqueText.Replace("\\n", "\n"));
            db = new DialogBox(new Vector2(10, 15f), new Vector2(UIScaler.GetWidthUnits() - 20, 8),
                new StringKey(uniqueText,false));
            db.AddBorder(Color.red);
            new TextButton(new Vector2(UIScaler.GetWidthUnits() - 21, 23.5f), new Vector2(10, 2), CommonStringKeys.CLOSE, delegate { onClose(); });
        }
        else
        {
            new TextButton(new Vector2(UIScaler.GetWidthUnits() - 21, 13f), new Vector2(10, 2), CommonStringKeys.CLOSE, delegate { onClose(); });
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
