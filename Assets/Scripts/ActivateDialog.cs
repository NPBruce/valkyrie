using UnityEngine;
using System.Collections;

// Window with Monster activation
public class ActivateDialog {
    // The even that raises this dialog
    public Game.Monster monster;
    public bool master;

    // Create an activation window, if master is false then it is for minions
    public ActivateDialog(Game.Monster m, bool masterIn)
    {
        monster = m;
        master = masterIn;
        CreateWindow();
    }

    public void CreateWindow()
    {
        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        // ability box - name header
        DialogBox db = new DialogBox(new Vector2(15, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 30, 2), monster.monsterData.name);
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();
        
        // ability text
        string abiltiyText = monster.currentActivation.ability.Replace("\\n", "\n");
        db = new DialogBox(new Vector2(10, 2.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 4), abiltiyText);
        db.AddBorder();

        // Activation box
        string activationText = "";
        // Create header
        if (master)
        {
            db = new DialogBox(new Vector2(15, 7f), new Vector2(UIScaler.GetWidthUnits() - 30, 2), "Master", Color.red);
            activationText = monster.currentActivation.masterActions.Replace("\\n", "\n");
        }
        else
        {
            db = new DialogBox(new Vector2(15, 7f), new Vector2(UIScaler.GetWidthUnits() - 30, 2), "Minion");
            activationText = monster.currentActivation.minionActions.Replace("\\n", "\n");
        }
        db.AddBorder();
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();

        // Create ability text box
        db = new DialogBox(new Vector2(10, 9), new Vector2(UIScaler.GetWidthUnits() - 20, 7), activationText);
        if (master)
        {
            db.AddBorder(Color.red);
        }
        else
        {
            db.AddBorder();
        }

        // Create finished button
        if (master)
        {
            new TextButton(new Vector2(15, 16.5f), new Vector2(UIScaler.GetWidthUnits() - 30, 2), "Masters Activated", delegate { activated(); }, Color.red);
        }
        else
        {
            new TextButton(new Vector2(15, 16.5f), new Vector2(UIScaler.GetWidthUnits() - 30, 2), "Minions Activated", delegate { activated(); });
        }
    }

    public void activated()
    {
        // Destroy this dialog to close
        destroy();

        RoundHelper.MonsterActivated();
    }

    public void destroy()
    {
        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
    }

}
