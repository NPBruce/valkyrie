using UnityEngine;
using System.Collections;

// Class for creation of monster selection options
public class MonsterDialog
{
    public Quest.Monster monster;

    // Constuct the button list
    public MonsterDialog(Quest.Monster m)
    {
        monster = m;
        CreateWindow();
    }

    // Draw items
    public virtual void CreateWindow()
    {
        Game game = Game.Get();
        // Count the monster number
        int index = 0;
        for (int i = 0; i < game.quest.monsters.Count; i++)
        {
            if (game.quest.monsters[i] == monster)
            {
                index = i;
            }
        }

        // Work out where on the screen to display
        float offset = (index + 0.1f - game.monsterCanvas.offset) * (MonsterCanvas.monsterSize + 0.5f);

        new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset), new Vector2(10, 2), "Information", delegate { Info(); });
        new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset + 2.5f), new Vector2(10, 2), "Force Activate", delegate { Activate(); });
        new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset + 5f), new Vector2(10, 2), "Defeated", delegate { Defeated(); });
        if (monster.unique)
        {
            // If there is a unique option the offset needs to be increased
            offset += 3.5f;
            new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset + 4f), new Vector2(10, 3), "Unique\nDefeated", delegate { UniqueDefeated(); });
        }
        // FIXME: This doesn't fit if there is a unique monster in the last space
        new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset + 7.5f), new Vector2(10, 2), "Cancel", delegate { OnCancel(); });
    }

    // Monster Information
    public void Info()
    {
        Destroy();
        new InfoDialog(monster);
    }

    // Force Activation
    public void Activate()
    {
        Game game = Game.Get();
        // Save to undo stack
        game.quest.Save();
        game.roundControl.ActivateMonster(monster);
    }

    // Defeated monsters
    public void Defeated()
    {
        Destroyer.Dialog();
        Game game = Game.Get();
        // Save to undo stack
        game.quest.Save();
        // Remove this monster group
        game.quest.monsters.Remove(monster);
        updateDisplay();

        game.quest.vars.SetValue("#monsters", game.quest.monsters.Count);

        game.audio.PlayTrait("defeated");
        
        // Trigger defeated event
        game.quest.eManager.EventTriggerType("Defeated" + monster.monsterData.sectionName);
        // If unique trigger defeated unique event
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
        // Add to undo stack
        game.quest.Save();
        // Monster is no longer unique
        monster.unique = false;
        game.monsterCanvas.UpdateStatus();
        // Trigger unique defeated event
        game.quest.eManager.EventTriggerType("DefeatedUnique" + monster.monsterData.sectionName);
    }

    // Cancel cleans up
    public void OnCancel()
    {
        Destroy();
    }

    // Update the list of monsters
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
