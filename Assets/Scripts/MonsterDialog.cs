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
        new TextButton(new Vector2(800, 100), new Vector2(160, 40), "Infomation", delegate { info(); });
        new TextButton(new Vector2(800, 130), new Vector2(160, 40), "Force Activate", delegate { activate(); });
        new TextButton(new Vector2(800, 160), new Vector2(160, 40), "Defeated", delegate { defeated(); });
        new TextButton(new Vector2(800, 190), new Vector2(160, 40), "Cancel", delegate { onCancel(); });
    }

    // Todo
    public void info()
    {

    }

    // Todo
    public void activate()
    {
        RoundHelper.ActivateMonster(monster);
    }

    // Defeated monsters
    public void defeated()
    {
        destroy();
        Game game = Game.Get();
        game.monsters.Remove(monster);
        updateDisplay();
        EventHelper.EventTriggerType("Defeated" + monster.monsterData.sectionName);
    }

    // Cancel cleans up
    public void onCancel()
    {
        destroy();
    }

    public void updateDisplay()
    {
        Game game = Game.Get();
        game.monsterCanvas.UpdateList();
    }

    public void destroy()
    {
        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
    }
}
