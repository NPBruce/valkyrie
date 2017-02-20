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
        if (hero.activated)
        {
            // Grey button with no action
            new TextButton(new Vector2(HeroCanvas.heroSize + 0.5f, offset), new Vector2(10, 2), "Activated", delegate { noAction(); }, Color.gray);
        }
        else
        {
            new TextButton(new Vector2(HeroCanvas.heroSize + 0.5f, offset), new Vector2(10, 2), "Activated", delegate { activated(); });
        }

        // Is this hero defeated?
        if (hero.defeated)
        {
            new TextButton(new Vector2(HeroCanvas.heroSize + 0.5f, offset + 2.5f), new Vector2(10, 2), "Restored", delegate { restored(); });
        }
        else
        {
            new TextButton(new Vector2(HeroCanvas.heroSize + 0.5f, offset + 2.5f), new Vector2(10, 2), "Defeated", delegate { defeated(); });
        }

        new TextButton(new Vector2(HeroCanvas.heroSize + 0.5f, offset + 5f), new Vector2(10, 2), "Cancel", delegate { onCancel(); });
    }

    // Null function for activated hero
    public void noAction()
    {
    }

    // Hero defeated
    public void defeated()
    {
        destroy();
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
        destroy();
        Game game = Game.Get();
        // Save to undo stack
        game.quest.Save();
        hero.defeated = false;
        updateDisplay();
    }

    // Activated hero
    public void activated()
    {
        destroy();
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
        destroy();
    }

    public void updateDisplay()
    {
        Game game = Game.Get();
        game.heroCanvas.UpdateStatus();
    }

    public void destroy()
    {
        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
    }
}
