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

        new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset), new Vector2(10, 2), "Information", delegate { Info(); });
        new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset + 2.5f), new Vector2(10, 2), "Force Activate", delegate { Activate(); });
        new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset + 5f), new Vector2(10, 2), "Defeated", delegate { Defeated(); });
        if (monster.unique)
        {
            offset += 3.5f;
            new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset + 4f), new Vector2(10, 3), "Unique\nDefeated", delegate { UniqueDefeated(); });
        }
        new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset + 7.5f), new Vector2(10, 2), "Cancel", delegate { OnCancel(); });
    }

    // Todo
    public void Info()
    {
        Destroy();
        new InfoDialog(monster);
    }

    // Todo
    public void Activate()
    {
        RoundHelper.ActivateMonster(monster);
    }

    // Defeated monsters
    public void Defeated()
    {
        Destroy();
        Game game = Game.Get();
        game.monsters.Remove(monster);
        updateDisplay();
        EventHelper.EventTriggerType("Defeated" + monster.monsterData.sectionName);
        if (monster.unique)
        {
            EventHelper.EventTriggerType("DefeatedUnique" + monster.monsterData.sectionName);
        }
    }

    // Unique Defeated (others still around)
    public void UniqueDefeated()
    {
        Destroy();
        monster.unique = false;
        Game.Get().monsterCanvas.UpdateStatus();
        EventHelper.EventTriggerType("DefeatedUnique" + monster.monsterData.sectionName);
    }

    // Cancel cleans up
    public void OnCancel()
    {
        Destroy();
    }

    public void updateDisplay()
    {
        Game game = Game.Get();
        game.monsterCanvas.UpdateList();
    }

    public void Destroy()
    {
        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
    }
}
