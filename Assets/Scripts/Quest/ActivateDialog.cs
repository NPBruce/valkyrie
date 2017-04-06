using Assets.Scripts.Content;
using UnityEngine;

// Window with Monster activation
public class ActivateDialog {
    // The monster that raises this dialog
    public Quest.Monster monster;
    public bool master;
    private readonly StringKey ACTIONS = new StringKey("val", "ACTIONS");
    private readonly StringKey MONSTER_MASTER = new StringKey("val", "MONSTER_MASTER");
    private readonly StringKey MONSTER_MINION = new StringKey("val", "MONSTER_MINION");
    private readonly StringKey ACTIVATED = new StringKey("val", "ACTIVATED");

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
        Destroyer.Dialog();

        // ability box - name header
        TextButton tb = new TextButton(
            new Vector2(15, 0.5f), 
            new Vector2(UIScaler.GetWidthUnits() - 30, 2), 
            monster.monsterData.name,
            delegate { new InfoDialog(monster); });
        tb.ApplyTag("activation");

        DialogBox db = null;
        float offset = 2.5f;
        if (monster.currentActivation.effect.Length > 0)
        {
            string effect = monster.currentActivation.effect.Replace("\\n", "\n");
            // ability text
            db = new DialogBox(new Vector2(10, offset), new Vector2(UIScaler.GetWidthUnits() - 20, 4), 
                new StringKey(effect,false));
            db.AddBorder();
            db.ApplyTag("activation");
            offset += 4.5f;
        }

        // Activation box
        string activationText = "";
        // Create header
        if (singleStep)
        {
            db = new DialogBox(new Vector2(15, offset), new Vector2(UIScaler.GetWidthUnits() - 30, 2), ACTIONS);
        }
        else if (master)
        {
            db = new DialogBox(new Vector2(15, offset), new Vector2(UIScaler.GetWidthUnits() - 30, 2), MONSTER_MASTER, Color.red);
        }
        else
        {
            db = new DialogBox(new Vector2(15, offset), new Vector2(UIScaler.GetWidthUnits() - 30, 2), MONSTER_MINION);
        }

        if (master)
        {
            activationText = monster.currentActivation.ad.masterActions.Translate();
        }
        else
        {
            activationText = monster.currentActivation.ad.minionActions.Translate();
        }
        db.AddBorder();
        db.ApplyTag("activation");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        offset += 2;

        // Create activation text box
        db = new DialogBox(new Vector2(10, offset), new Vector2(UIScaler.GetWidthUnits() - 20, 7),
            new StringKey(activationText,false));
        if (master && !singleStep)
        {
            db.AddBorder(Color.red);
        }
        else
        {
            db.AddBorder();
        }
        db.ApplyTag("activation");

        offset += 7.5f;

        // Create finished button
        if (singleStep)
        {
            tb = new TextButton(new Vector2(15, offset), new Vector2(UIScaler.GetWidthUnits() - 30, 2), ACTIVATED, delegate { activated(); });
        }
        else if (master)
        {
            tb = new TextButton(new Vector2(15, offset), new Vector2(UIScaler.GetWidthUnits() - 30, 2), new StringKey("val","X_ACTIVATED",MONSTER_MASTER ), delegate { activated(); }, Color.red);
        }
        else
        {
            tb = new TextButton(new Vector2(15, offset), new Vector2(UIScaler.GetWidthUnits() - 30, 2), new StringKey("val", "X_ACTIVATED", MONSTER_MINION), delegate { activated(); });
        }
        tb.ApplyTag("activation");
    }

    virtual public void activated()
    {
        // Disable if there is a menu open
        if (GameObject.FindGameObjectWithTag("dialog") != null) return;

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("activation"))
            Object.Destroy(go);
        Game.Get().roundControl.MonsterActivated();
    }
}
