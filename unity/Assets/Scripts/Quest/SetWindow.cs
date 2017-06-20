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
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            Object.Destroy(go);

        UIElement ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-10), 10, 20, 10);
        new UIElementBorder(ui);

        if (game.quest.vars.GetValue("$fire") > 0)
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-8f), 11f), new Vector2(16, 2), CLEAR_FIRE, delegate { ClearFire(); });
        }
        else
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-8f), 11f), new Vector2(16, 2), SET_FIRE, delegate { SetFire(); });
        }
        if (game.quest.vars.GetValue("#eliminated") > 0)
        {
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-8), 14, 16, 2);
            ui.SetText(INVESTIGATOR_ELIMINATED, Color.gray);
            ui.SetFontSize(UIScaler.GetMediumFont());
            new UIElementBorder(ui, Color.gray);
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
        game.quest.vars.SetValue("$fire", 1);
        new SetWindow();
    }

    public void ClearFire()
    {
        Game game = Game.Get();
        game.quest.vars.SetValue("$fire", 0);
        new SetWindow();
    }

    public void Eliminate()
    {
        Game game = Game.Get();
        game.quest.vars.SetValue("#eliminated", 1);
        new SetWindow();
    }
}
