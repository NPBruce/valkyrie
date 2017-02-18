using UnityEngine;
using System.Collections;

// Window with Monster activation
public class ActivateDialog {
    // The monster that raises this dialog
    public Quest.Monster monster;
    public bool master;

    // Create an activation window, if master is false then it is for minions
    public ActivateDialog(Quest.Monster m, bool masterIn)
    {
        monster = m;
        master = masterIn;
        CreateWindow();
    }

    virtual public void CreateWindow()
    {
        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        // ability box - name header
        DialogBox db = new DialogBox(new Vector2(15, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 30, 2), monster.monsterData.name);
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        float offset = 2.5f;
        if (monster.currentActivation.effect.Length > 0)
        {
            // ability text
            db = new DialogBox(new Vector2(10, offset), new Vector2(UIScaler.GetWidthUnits() - 20, 4), monster.currentActivation.effect.Replace("\\n", "\n"));
            db.AddBorder();
            offset += 4.5f;
        }

        // Activation box
        string activationText = "";
        // Create header
        if (master)
        {
            db = new DialogBox(new Vector2(15, offset), new Vector2(UIScaler.GetWidthUnits() - 30, 2), "Master", Color.red);
            activationText = monster.currentActivation.ad.masterActions.Replace("\\n", "\n");
        }
        else
        {
            db = new DialogBox(new Vector2(15, offset), new Vector2(UIScaler.GetWidthUnits() - 30, 2), "Minion");
            activationText = monster.currentActivation.ad.minionActions.Replace("\\n", "\n");
        }
        db.AddBorder();
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        offset += 2;

        // Create activation text box
        db = new DialogBox(new Vector2(10, offset), new Vector2(UIScaler.GetWidthUnits() - 20, 7), activationText);
        if (master)
        {
            db.AddBorder(Color.red);
        }
        else
        {
            db.AddBorder();
        }

        offset += 7.5f;

        // Create finished button
        if (master)
        {
            new TextButton(new Vector2(15, offset), new Vector2(UIScaler.GetWidthUnits() - 30, 2), "Masters Activated", delegate { activated(); }, Color.red);
        }
        else
        {
            new TextButton(new Vector2(15, offset), new Vector2(UIScaler.GetWidthUnits() - 30, 2), "Minions Activated", delegate { activated(); });
        }
    }

    public void activated()
    {
        // Destroy this dialog to close
        destroy();

        Game.Get().roundControl.MonsterActivated();
    }

    public void destroy()
    {
        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
    }

}
