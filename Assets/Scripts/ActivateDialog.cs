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
        string abiltiyText = monster.monsterData.name + ":\n\n" + monster.currentActivation.ability.Replace("\\n", "\n");
        new DialogBox(new Vector2(300, 30), new Vector2(500, 120), abiltiyText);

        // Activation box
        string activationText = "";

        if (master)
        {
            activationText = "Master:\n\n" + monster.currentActivation.masterActions.Replace("\\n", "\n");
        }
        else
        {
            activationText = "Minion:\n\n" + monster.currentActivation.minionActions.Replace("\\n", "\n");
        }
        new DialogBox(new Vector2(300, 180), new Vector2(500, 120), activationText);

        if (master)
        {
            new TextButton(new Vector2(500, 330), new Vector2(300, 40), "Masters Activated", delegate { activated(); });
        }
        else
        {
            new TextButton(new Vector2(500, 330), new Vector2(300, 40), "Minions Activated", delegate { activated(); });
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
