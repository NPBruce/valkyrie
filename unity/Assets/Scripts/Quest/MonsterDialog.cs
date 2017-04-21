using UnityEngine;
using System.Collections;
using Assets.Scripts.Content;

// Class for creation of monster selection options
public class MonsterDialog
{
    public Quest.Monster monster;

    private readonly StringKey DEFEATED = new StringKey("val", "DEFEATED");
    private readonly StringKey FORCE_ACTIVATE = new StringKey("val", "FORCE_ACTIVATE");
    private readonly StringKey INFORMATION = new StringKey("val", "INFORMATION");
    private readonly StringKey UNIQUE_DEFEATED = new StringKey("val", "UNIQUE_DEFEATED");

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

        if (GameObject.FindGameObjectWithTag("activation") != null)
        {
            offset += 2.8f;
        }

        new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset), new Vector2(10, 2), INFORMATION, delegate { Info(); });
        offset += 2.5f;
        if (GameObject.FindGameObjectWithTag("activation") == null)
        {
            new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset), new Vector2(10, 2), FORCE_ACTIVATE, delegate { Activate(); });
            offset += 2.5f;
            new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset), new Vector2(10, 2), DEFEATED, delegate { Defeated(); });
            offset += 2.5f;
            if (monster.unique)
            {
                // If there is a unique option the offset needs to be increased
                new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset), new Vector2(10, 3), UNIQUE_DEFEATED, delegate { UniqueDefeated(); });
                offset += 3.5f;
            }
        }
        // FIXME: This doesn't fit if there is a unique monster in the last space
        new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset), new Vector2(10, 2), CommonStringKeys.CANCEL, delegate { OnCancel(); });
    }

    // Monster Information
    public void Info()
    {
        Destroyer.Dialog();
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

        game.audioControl.PlayTrait("defeated");
        
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
        Destroyer.Dialog();
        // Add to undo stack
        game.quest.Save();
        // Monster is no longer unique
        monster.unique = false;
        monster.healthMod = 0;
        game.monsterCanvas.UpdateList();
        // Trigger unique defeated event
        game.quest.eManager.EventTriggerType("DefeatedUnique" + monster.monsterData.sectionName);
    }

    // Cancel cleans up
    public void OnCancel()
    {
        Destroyer.Dialog();
    }

    // Update the list of monsters
    public void updateDisplay()
    {
        Game game = Game.Get();
        game.monsterCanvas.UpdateList();
    }
}
