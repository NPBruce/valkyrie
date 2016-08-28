using UnityEngine;
using System.Collections;

// Window with Monster activation
public class ActivateDialog {
    // The even that raises this dialog
    public Game.Monster monster;
    public bool master;

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

        // ability box
        DialogBox db = new DialogBox(new Vector2(15, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 30, 2), monster.monsterData.name);
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();
        string abiltiyText = monster.currentActivation.ability.Replace("\\n", "\n");
        db = new DialogBox(new Vector2(10, 2.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 4), abiltiyText);
        db.AddBorder();

        // Activation box
        string activationText = "";

        if (master)
        {
            db = new DialogBox(new Vector2(15, 7f), new Vector2(UIScaler.GetWidthUnits() - 30, 2), "Master");
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            db.textObj.GetComponent<UnityEngine.UI.Text>().color = Color.red;
            db.AddBorder();
            activationText = monster.currentActivation.masterActions.Replace("\\n", "\n");
            db = new DialogBox(new Vector2(10, 9), new Vector2(UIScaler.GetWidthUnits() - 20, 7), activationText);
        }
        else
        {
            db = new DialogBox(new Vector2(15, 7f), new Vector2(UIScaler.GetWidthUnits() - 30, 2), "Minion");
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            db.AddBorder();
            activationText = monster.currentActivation.minionActions.Replace("\\n", "\n");
            db = new DialogBox(new Vector2(10, 9), new Vector2(UIScaler.GetWidthUnits() - 20, 7), activationText);
        }
        db.AddBorder();

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
