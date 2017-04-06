using UnityEngine;
using System.Collections;

// Window with Monster activation
public class ActivateDialogMoM : ActivateDialog
{
    // Create an activation window, if master is false then it is for minions
    public ActivateDialogMoM(Quest.Monster m) : base(m, true)
    {
    }

    override public void CreateWindow(bool singleStep = false)
    {
        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        // ability box - name header
        DialogBox db = new DialogBox(new Vector2(UIScaler.GetHCenter(-9f), 0.5f), new Vector2(18, 2), monster.monsterData.name.Translate());
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

        new TextButton(new Vector2(UIScaler.GetHCenter(-9f), offset), new Vector2(18, 2), "The monster attacks.", delegate { CreateAttackWindow(); });

        offset += 2.5f;

        new TextButton(
            new Vector2(UIScaler.GetHCenter(-9f), offset), 
            new Vector2(18, 2), 
            monster.currentActivation.ad.moveButton.Translate(), 
            delegate { CreateMoveWindow(); });

        MonsterDialogMoM.DrawMonster(monster);
    }

    public void CreateAttackWindow()
    {
        Destroyer.Dialog();

        // ability box - name header
        DialogBox db = new DialogBox(new Vector2(15, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 30, 2), monster.monsterData.name.Translate());
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        float offset = 2.5f;
        db = new DialogBox(
            new Vector2(10, offset), 
            new Vector2(UIScaler.GetWidthUnits() - 20, 4), 
            monster.currentActivation.ad.masterActions.Translate().Replace("\\n", "\n"));
        db.AddBorder();

        offset += 4.5f;

        new TextButton(new Vector2(UIScaler.GetHCenter(-6f), offset), new Vector2(12, 2), "Finished", delegate { activated(); });

        MonsterDialogMoM.DrawMonster(monster);
    }

    public void CreateMoveWindow()
    {
        if (monster.currentActivation.ad.move.key.Length == 0)
        {
            activated();
            return;
        }

        Destroyer.Dialog();
        DialogBox db = new DialogBox(new Vector2(15, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 30, 2), monster.monsterData.name.Translate());
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        float offset = 2.5f;
        db = new DialogBox(new Vector2(10, offset), new Vector2(UIScaler.GetWidthUnits() - 20, 4), monster.currentActivation.move.Replace("\\n", "\n"));
        db.AddBorder();

        offset += 4.5f;

        new TextButton(new Vector2(UIScaler.GetHCenter(-6f), offset), new Vector2(12, 2), "Finished", delegate { activated(); });

        MonsterDialogMoM.DrawMonster(monster);
    }
}
