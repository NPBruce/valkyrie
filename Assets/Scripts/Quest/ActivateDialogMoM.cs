using UnityEngine;
using System.Collections;

// Window with Monster activation
public class ActivateDialogMoM : ActivateDialog
{
    // Create an activation window, if master is false then it is for minions
    public ActivateDialogMoM(Quest.Monster m) : base(m, true)
    {
    }

    override public void CreateWindow()
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

        new TextButton(new Vector2(15, offset), new Vector2(UIScaler.GetWidthUnits() - 30, 2), "Monster Attacks", delegate { CreateAttackWindow(); });

        offset += 2.5f;

        new TextButton(new Vector2(15, offset), new Vector2(UIScaler.GetWidthUnits() - 30, 2), "Unable to Atack", delegate { activated(); });
    }

    public void CreateAttackWindow()
    {
        Destroyer.Dialog();

        // ability box - name header
        DialogBox db = new DialogBox(new Vector2(15, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 30, 2), monster.monsterData.name);
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        float offset = 2.5f;
        db = new DialogBox(new Vector2(10, offset), new Vector2(UIScaler.GetWidthUnits() - 20, 4), monster.currentActivation.ad.masterActions.Replace("\\n", "\n"));
        db.AddBorder();

        offset += 4.5f;

        new TextButton(new Vector2(15, offset), new Vector2(UIScaler.GetWidthUnits() - 30, 2), "Finished", delegate { activated(); });
    }
}
