using UnityEngine;

// Window with Monster activation
public class ActivateDialog {
    // The monster that raises this dialog
    public Quest.Monster monster;
    public bool master;

    // Create an activation window, if master is false then it is for minions
    public ActivateDialog(Quest.Monster m, bool masterIn, bool singleStep = false)
    {
        monster = m;
        master = masterIn;
        CreateWindow(singleStep);
    }

    virtual public void CreateWindow(bool singleStep = false)
    {
        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        // ability box - name header
        DialogBox db = new DialogBox(
            new Vector2(15, 0.5f), 
            new Vector2(UIScaler.GetWidthUnits() - 30, 2), 
            monster.monsterData.name.Translate());
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
        if (singleStep)
        {
            db = new DialogBox(new Vector2(15, offset), new Vector2(UIScaler.GetWidthUnits() - 30, 2), "Actions");
        }
        else if (master)
        {
            db = new DialogBox(new Vector2(15, offset), new Vector2(UIScaler.GetWidthUnits() - 30, 2), "Master", Color.red);
        }
        else
        {
            db = new DialogBox(new Vector2(15, offset), new Vector2(UIScaler.GetWidthUnits() - 30, 2), "Minion");
        }

        if (master)
        {
            activationText = monster.currentActivation.ad.masterActions.Translate().Replace("\\n", "\n");
        }
        else
        {
            activationText = monster.currentActivation.ad.minionActions.Translate().Replace("\\n", "\n");
        }
        db.AddBorder();
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        offset += 2;

        // Create activation text box
        db = new DialogBox(new Vector2(10, offset), new Vector2(UIScaler.GetWidthUnits() - 20, 7), activationText);
        if (master && !singleStep)
        {
            db.AddBorder(Color.red);
        }
        else
        {
            db.AddBorder();
        }

        offset += 7.5f;

        // Create finished button
        if (singleStep)
        {
            new TextButton(new Vector2(15, offset), new Vector2(UIScaler.GetWidthUnits() - 30, 2), "Activated", delegate { activated(); });
        }
        else if (master)
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
