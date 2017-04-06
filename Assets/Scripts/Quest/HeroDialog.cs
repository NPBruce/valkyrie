using UnityEngine;
using System.Collections;
using Assets.Scripts.Content;

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
        if (hero.activated || (GameObject.FindGameObjectWithTag("activation") != null))
        {
            // Grey button with no action
            new TextButton(new Vector2(HeroCanvas.heroSize + 0.5f, offset), new Vector2(10, 2), END_TURN, delegate { ; }, Color.gray);
        }
        else
        {
            new TextButton(new Vector2(HeroCanvas.heroSize + 0.5f, offset), new Vector2(10, 2), END_TURN, delegate { activated(); });
        }

        // Is this hero defeated?
        if (hero.defeated)
        {
            new TextButton(new Vector2(HeroCanvas.heroSize + 0.5f, offset + 2.5f), new Vector2(10, 2), RECOVER, delegate { restored(); });
        }
        else
        {
            new TextButton(new Vector2(HeroCanvas.heroSize + 0.5f, offset + 2.5f), new Vector2(10, 2), KO, delegate { defeated(); });
        }

        new TextButton(new Vector2(HeroCanvas.heroSize + 0.5f, offset + 5f), new Vector2(10, 2), CommonStringKeys.CANCEL, delegate { onCancel(); });
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
        if (GameObject.FindGameObjectWithTag("activation") != null)
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
