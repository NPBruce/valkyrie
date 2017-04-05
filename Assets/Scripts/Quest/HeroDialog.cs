using UnityEngine;
using System.Collections;

// Class for creation of hero control button menu
public class HeroDialog{
    public Quest.Hero hero;

    public HeroDialog(Quest.Hero h)
    {
        hero = h;
        CreateWindow();
    }

    public void CreateWindow()
    {
        float offset = ((hero.id - 0.9f) * (HeroCanvas.heroSize + 0.5f)) + HeroCanvas.offsetStart;
        // Has this hero been activated?
        if (hero.activated || (GameObject.FindGameObjectWithTag("activation") != null))
        {
            // Grey button with no action
            new TextButton(new Vector2(HeroCanvas.heroSize + 0.5f, offset), new Vector2(10, 2), "End Turn", delegate { ; }, Color.gray);
        }
        else
        {
            new TextButton(new Vector2(HeroCanvas.heroSize + 0.5f, offset), new Vector2(10, 2), "End Turn", delegate { activated(); });
        }

        // Is this hero defeated?
        if (hero.defeated)
        {
            new TextButton(new Vector2(HeroCanvas.heroSize + 0.5f, offset + 2.5f), new Vector2(10, 2), "Recover", delegate { restored(); });
        }
        else
        {
            new TextButton(new Vector2(HeroCanvas.heroSize + 0.5f, offset + 2.5f), new Vector2(10, 2), "KO", delegate { defeated(); });
        }

        new TextButton(new Vector2(HeroCanvas.heroSize + 0.5f, offset + 5f), new Vector2(10, 2), "Cancel", delegate { onCancel(); });
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
        // This can trigger events
        game.quest.AdjustMorale(-1);
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
