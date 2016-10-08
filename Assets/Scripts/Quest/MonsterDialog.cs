using UnityEngine;
using System.Collections;

// Class for creation of monster seleciton options
public class MonsterDialog
{
    public Quest.Monster monster;

    public MonsterDialog(Quest.Monster m)
    {
        monster = m;
        CreateWindow();
    }

    public virtual void CreateWindow()
    {
        Game game = Game.Get();
        int index = 0;
        for (int i = 0; i < game.quest.monsters.Count; i++)
        {
            if (game.quest.monsters[i] == monster)
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
        Game game = Game.Get();
        game.quest.Save();
        game.roundControl.ActivateMonster(monster);
    }

    // Defeated monsters
    public void Defeated()
    {
        Destroy();
        Game game = Game.Get();
        game.quest.Save();
        game.quest.monsters.Remove(monster);
        updateDisplay();

        // Check if all monsters gone
        if (game.quest.monsters.Count == 0)
        {
            // clear monster tag
            game.quest.flags.Remove("#monsters");
        }

        game.quest.eManager.EventTriggerType("Defeated" + monster.monsterData.sectionName);
        if (monster.unique)
        {
            game.quest.eManager.EventTriggerType("DefeatedUnique" + monster.monsterData.sectionName);
        }
    }

    // Unique Defeated (others still around)
    public void UniqueDefeated()
    {
        Game game = Game.Get();
        Destroy();
        game.quest.Save();
        monster.unique = false;
        game.monsterCanvas.UpdateStatus();
        game.quest.eManager.EventTriggerType("DefeatedUnique" + monster.monsterData.sectionName);
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
