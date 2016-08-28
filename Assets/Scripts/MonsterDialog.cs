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
        Game game = Game.Get();
        int index = 0;
        for (int i = 0; i < game.monsters.Count; i++)
        {
            if (game.monsters[i] == monster)
            {
                index = i;
            }
        }

        float offset = (index + 0.1f) * (MonsterCanvas.monsterSize + 0.5f);

        new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset), new Vector2(10, 2), "Infomation", delegate { info(); });
        new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset + 2.5f), new Vector2(10, 2), "Force Activate", delegate { activate(); });
        new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset + 5f), new Vector2(10, 2), "Defeated", delegate { defeated(); });
        new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset + 7.5f), new Vector2(10, 2), "Cancel", delegate { onCancel(); });
    }

    // Todo
    public void info()
    {
        destroy();
        new InfoDialog(monster.monsterData);
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
