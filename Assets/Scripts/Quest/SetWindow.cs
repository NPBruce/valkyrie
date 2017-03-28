using UnityEngine;
using System.Collections;

// Next stage button is used by MoM to move between investigators and monsters
public class SetWindow
{
    // Construct and display
    public SetWindow()
    {
        Game game = Game.Get();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        DialogBox db = new DialogBox(new Vector2(UIScaler.GetHCenter(-10f), 10f), new Vector2(20, 10f), "");
        db.AddBorder();

        if (game.quest.flags.Contains("#fire"))
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-8f), 11f), new Vector2(16, 2), "Clear Fire", delegate { ClearFire(); });
        }
        else
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-8f), 11f), new Vector2(16, 2), "Set Fire", delegate { SetFire(); });
        }
        if (game.quest.flags.Contains("#eliminated"))
        {
            db = new DialogBox(new Vector2(UIScaler.GetHCenter(-8f), 14f), new Vector2(16, 2), "Investigator Eliminated", Color.gray);
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            db.AddBorder();
        }
        else
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-8f), 14f), new Vector2(16, 2), "Investigator Eliminated", delegate { Eliminate(); });
        }

        new TextButton(new Vector2(UIScaler.GetHCenter(-3f), 17f), new Vector2(6, 2), "Close", delegate { Destroyer.Dialog(); });
    }

    public void SetFire()
    {
        Game game = Game.Get();
        game.quest.flags.Add("#fire");
        game.vars.SetValue("#fire", 1);
        new SetWindow();
    }

    public void ClearFire()
    {
        Game game = Game.Get();
        game.quest.flags.Remove("#fire");
        game.vars.SetValue("#fire", 0);
        new SetWindow();
    }

    public void Eliminate()
    {
        Game game = Game.Get();
        game.quest.flags.Add("#eliminated");
        new SetWindow();
    }
}
