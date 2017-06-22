using UnityEngine;
using System.Collections;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

// Class for creation of hero control button menu
public class HeroDialog{
    public Quest.Hero hero;
    private readonly StringKey RECOVER = new StringKey("val","RECOVER");
    private readonly StringKey END_TURN = new StringKey("val", "END_TURN");
    private readonly StringKey KO = new StringKey("val", "KO");

    public HeroDialog(Quest.Hero h)
    {
        hero = h;
        CreateWindow();
    }

    public void CreateWindow()
    {
        float offset = ((hero.id - 0.9f) * (HeroCanvas.heroSize + 0.5f)) + HeroCanvas.offsetStart;
        // Has this hero been activated?
        UIElement ui = new UIElement();
        ui.SetLocation(HeroCanvas.heroSize + 0.5f, offset, 10, 2);
        if (hero.activated || (GameObject.FindGameObjectWithTag(Game.ACTIVATION) != null))
        {
            ui.SetText(END_TURN, Color.gray);
            new UIElementBorder(ui, Color.gray);
        }
        else
        {
            ui.SetText(END_TURN);
            ui.SetButton(activated);
            new UIElementBorder(ui);
        }
        ui.SetFontSize(UIScaler.GetMediumFont());

        // Is this hero defeated?
        ui = new UIElement();
        ui.SetLocation(HeroCanvas.heroSize + 0.5f, offset + 2.5f, 10, 2);
        new UIElementBorder(ui);
        if (hero.defeated)
        {
            ui.SetText(RECOVER);
            ui.SetButton(restored);
        }
        else
        {
            ui.SetText(KO);
            ui.SetButton(defeated);
        }
        ui.SetFontSize(UIScaler.GetMediumFont());

        ui = new UIElement();
        ui.SetLocation(HeroCanvas.heroSize + 0.5f, offset + 5, 10, 2);
        ui.SetText(CommonStringKeys.CANCEL);
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(onCancel);
        new UIElementBorder(ui);
    }

    // Hero defeated
    public void defeated()
    {
        Destroyer.Dialog();
        Game game = Game.Get();
        // Save to undo stack
        game.quest.Save();
        hero.defeated = true;
        updateDisplay();
        // This can trigger events, delay events if activation present
        if (GameObject.FindGameObjectWithTag(Game.ACTIVATION) != null)
        {
            game.quest.AdjustMorale(-1, true);
        }
        else
        {
            game.quest.AdjustMorale(-1);
        }
    }

    // Hero restored
    public void restored()
    {
        Destroyer.Dialog();
        Game game = Game.Get();
        // Save to undo stack
        game.quest.Save();
        hero.defeated = false;
        updateDisplay();
    }

    // Activated hero
    public void activated()
    {
        Destroyer.Dialog();
        Game game = Game.Get();
        // Save state to undo stack
        game.quest.Save();
        hero.activated = true;

        // Let the game know that a hero has activated
        Game.Get().roundControl.HeroActivated();

        // Should this be before the roundControl?
        updateDisplay();
    }

    // Cancel cleans up
    public void onCancel()
    {
        Destroyer.Dialog();
    }

    public void updateDisplay()
    {
        Game game = Game.Get();
        game.heroCanvas.UpdateStatus();
    }
}
