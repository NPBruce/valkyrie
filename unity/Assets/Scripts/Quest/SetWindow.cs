using Assets.Scripts.Content;
using UnityEngine;

// Next stage button is used by MoM to move between investigators and monsters
public class SetWindow
{
    private StringKey SET_FIRE = new StringKey("val", "SET_FIRE");
    private StringKey CLEAR_FIRE = new StringKey("val", "CLEAR_FIRE");
    private StringKey INVESTIGATOR_ELIMINATED = new StringKey("val", "INVESTIGATOR_ELIMINATED");

    // Construct and display
    public SetWindow()
    {
        Game game = Game.Get();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        DialogBox db = new DialogBox(new Vector2(UIScaler.GetHCenter(-10f), 10f), new Vector2(20, 10f), StringKey.NULL);
        db.AddBorder();

        if (game.quest.vars.GetValue("#fire") > 0)
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-8f), 11f), new Vector2(16, 2), CLEAR_FIRE, delegate { ClearFire(); });
        }
        else
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-8f), 11f), new Vector2(16, 2), SET_FIRE, delegate { SetFire(); });
        }
        if (game.quest.vars.GetValue("#eliminated") > 0)
        {
            db = new DialogBox(new Vector2(UIScaler.GetHCenter(-8f), 14f), new Vector2(16, 2), INVESTIGATOR_ELIMINATED, Color.gray);
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            db.AddBorder();
        }
        else
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-8f), 14f), new Vector2(16, 2), INVESTIGATOR_ELIMINATED, delegate { Eliminate(); });
        }

        new TextButton(new Vector2(UIScaler.GetHCenter(-3f), 17f), new Vector2(6, 2), CommonStringKeys.CLOSE, delegate { Destroyer.Dialog(); });
    }

    public void SetFire()
    {
        Game game = Game.Get();
        game.quest.vars.SetValue("#fire", 1);
        new SetWindow();
    }

    public void ClearFire()
    {
        Game game = Game.Get();
        game.quest.vars.SetValue("#fire", 0);
        new SetWindow();
    }

    public void Eliminate()
    {
        Game game = Game.Get();
        game.quest.vars.SetValue("#eliminated", 1);
        new SetWindow();
    }
}
