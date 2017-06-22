using Assets.Scripts.Content;
using Assets.Scripts.UI;
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

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-8), 11, 16, 2);
        if (game.quest.vars.GetValue("$fire") > 0)
        {
            ui.SetText(CLEAR_FIRE);
            ui.SetButton(ClearFire);
        }
        else
        {
            ui.SetText(SET_FIRE);
            ui.SetButton(SetFire);
        }
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-8), 14, 16, 2);
        if (game.quest.vars.GetValue("#eliminated") > 0)
        {
            ui.SetText(INVESTIGATOR_ELIMINATED, Color.gray);
            new UIElementBorder(ui, Color.gray);
        }
        else
        {
            ui.SetText(INVESTIGATOR_ELIMINATED);
            ui.SetButton(Eliminate);
            new UIElementBorder(ui);
        }
        ui.SetFontSize(UIScaler.GetMediumFont());

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-3), 17, 6, 2);
        ui.SetText(CommonStringKeys.CLOSE);
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Destroyer.Dialog);
        new UIElementBorder(ui);
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
