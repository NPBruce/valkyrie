using UnityEngine;
using System.Collections;

// Class for creation of hero control button menu
public class HeroDialog{
    public Game.Hero hero;

    public HeroDialog(Game.Hero h)
    {
        hero = h;
        CreateWindow();
    }

    public void CreateWindow()
    {
        // Has this hero been activated?
        if (hero.activated)
        {
            // Grey button with no action
            new TextButton(new Vector2(4, 2), new Vector2(10, 2), "Activated", delegate { noAction(); }, Color.gray);
        }
        else
        {
            new TextButton(new Vector2(4, 2), new Vector2(10, 2), "Activated", delegate { activated(); });
        }

        // Is this hero defeated?
        if (hero.defeated)
        {
            new TextButton(new Vector2(4, 4.5f), new Vector2(10, 2), "Restored", delegate { restored(); });
        }
        else
        {
            new TextButton(new Vector2(4, 4.5f), new Vector2(10, 2), "Defeated", delegate { defeated(); });
        }

        new TextButton(new Vector2(4, 7), new Vector2(10, 2), "Cancel", delegate { onCancel(); });
    }

    // Null function for activated hero
    public void noAction()
    {
    }

    // Null function for activated hero
    public void defeated()
    {
        destroy();
        hero.defeated = true;
        updateDisplay();
    }

    // Null function for activated hero
    public void restored()
    {
        destroy();
        hero.defeated = false;
        updateDisplay();
    }

    // Null function for activated hero
    public void activated()
    {
        destroy();
        hero.activated = true;

        // Let the game know that a hero has activated
        RoundHelper.HeroActivated();

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
