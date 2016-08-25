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

        // ability holding object
        GameObject ability = new GameObject("dialog");
        // All things are tagged dialog so we can clean up
        ability.tag = "dialog";

        // actions holding object
        GameObject actions = new GameObject("dialog");
        // All things are tagged dialog so we can clean up
        actions.tag = "dialog";

        // Find the UI canvas
        Canvas[] canvii = GameObject.FindObjectsOfType<Canvas>();
        Canvas canvas = canvii[0];
        foreach (Canvas c in canvii)
        {
            if (c.name.Equals("UICanvas"))
            {
                canvas = c;
            }
        }
        ability.transform.parent = canvas.transform;
        actions.transform.parent = canvas.transform;

        // Position the ability text
        RectTransform transAb = ability.AddComponent<RectTransform>();
        transAb.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 30, 100);
        transAb.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 400, 500);
        ability.AddComponent<CanvasRenderer>();

        // Position the actions text
        RectTransform transAc = actions.AddComponent<RectTransform>();
        transAc.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 130, 100);
        transAc.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 400, 500);
        actions.AddComponent<CanvasRenderer>();

        // Add the ability text to the component
        UnityEngine.UI.Text textAb = ability.AddComponent<UnityEngine.UI.Text>();
        textAb.color = Color.white;
        textAb.text = monster.monsterData.name + ":\n\n" + monster.currentActivation.ability.Replace("\\n", "\n");
        textAb.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Add the actions text to the component
        UnityEngine.UI.Text textAc = actions.AddComponent<UnityEngine.UI.Text>();
        textAc.color = Color.white;
        if(!master)
        {
            textAc.text = "Minion:\n\n" + monster.currentActivation.minionActions.Replace("\\n", "\n");
        }
        else
        {
            textAc.text = "Master:\n\n" + monster.currentActivation.masterActions.Replace("\\n", "\n");
        }
        textAc.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        if (master)
        {
            new TextButton(new Vector2(500, 250), new Vector2(120, 20), "Masters Activated", delegate { activated(); });
        }
        else
        {
            new TextButton(new Vector2(500, 250), new Vector2(120, 20), "Minions Activated", delegate { activated(); });
        }
    }

    public void activated()
    {
        // Destroy this dialog to close
        destroy();

        RoundHelper.monsterActivated();
    }

    public void destroy()
    {
        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
    }

}
