using UnityEngine;
using System.Collections;

// Class for creation of hero control button menu
public class MonsterDialog
{
    public Game.Monster monster;

    public MonsterDialog(Game.Monster m)
    {
        monster = m;
        CreateWindow();
    }

    public void CreateWindow()
    {
        // Not done yet
        new TextButton(new Vector2(800, 100), new Vector2(80, 20), "Infomation", delegate { info(); });
        new TextButton(new Vector2(800, 130), new Vector2(80, 20), "Force Activate", delegate { activate(); });
        new TextButton(new Vector2(800, 160), new Vector2(80, 20), "Defeated", delegate { defeated(); });
        new TextButton(new Vector2(800, 190), new Vector2(80, 20), "Cancel", delegate { onCancel(); });
    }

    // Todo
    public void info()
    {
    }

    // Todo
    public void activate()
    {
    }

    // Null function for activated hero
    public void defeated()
    {
        Game game = GameObject.FindObjectOfType<Game>();
        game.monsters.Remove(monster);
        updateDisplay();
        destroy();
    }

    // Cancel cleans up
    public void onCancel()
    {
        destroy();
    }

    public void updateDisplay()
    {
        MonsterCanvas mc = GameObject.FindObjectOfType<MonsterCanvas>();
        mc.UpdateList();
    }

    public void destroy()
    {
        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
    }
}
