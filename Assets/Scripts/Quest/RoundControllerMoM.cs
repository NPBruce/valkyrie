using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This round controller extends the standard controller for MoM specific round order
public class RoundControllerMoM : RoundController
{
    // Investigators have finished
    override public void HeroActivated()
    {
        Game game = Game.Get();

        // Mark all Investigators as finished
        for (int i = 0; i < game.quest.heroes.Count; i++)
        {
            game.quest.heroes[i].activated = true;
        }
        // Activate monsters
        if (ActivateMonster())
        {
            // No monsters, end the round
            EndRound();
        }
    }

    // Mark a monster as activated
    override public void MonsterActivated()
    {
        Game game = Game.Get();

        // Check for any partial monster activations
        foreach (Quest.Monster m in game.quest.monsters)
        {
            // If both started then it is complete
            if (m.minionStarted || m.masterStarted)
            {
                m.activated = true;
            }
        }

        // Full activation, update display
        game.monsterCanvas.UpdateStatus();

        // Activate a monster
        if (ActivateMonster())
        {
            // No monsters left, end the round
            EndRound();
        }
    }

    // Activate a monster
    override public bool ActivateMonster()
    {
        Game game = Game.Get();

        bool allActivated = false;
        // Search for unactivated monsters
        // FIXME: this while shouldn't be here?
        while (!allActivated)
        {
            List<int> notActivated = new List<int>();
            // Get the index of all monsters that haven't activated
            for (int i = 0; i < game.quest.monsters.Count; i++)
            {
                if (!game.quest.monsters[i].activated)
                    notActivated.Add(i);
            }

            // If no monsters are found return true
            if (notActivated.Count == 0)
            {
                allActivated = true;
            }
            else
            {
                // Find a random unactivated monster
                Quest.Monster toActivate = game.quest.monsters[notActivated[Random.Range(0, notActivated.Count)]];

                ActivateMonster(toActivate);
                // Return false as activations remain
                return false;
            }
        }
        return true;
    }

    // Check if there are events that are required at the end of the round
    public override void CheckNewRound()
    {

        Game game = Game.Get();

        // Return if there is an event open
        if (game.quest.eManager.currentEvent != null)
            return;

        // Return if there is an event queued
        if (game.quest.eManager.eventStack.Count > 0)
            return;

        // Check all delayed events
        foreach (QuestData.Event.DelayedEvent de in game.quest.delayedEvents)
        {
            // Is the delay up?
            if (de.delay == game.quest.round)
            {
                // Trigger the event, then return
                game.quest.delayedEvents.Remove(de);
                game.quest.eManager.QueueEvent(de.eventName);
                return;
            }
        }

        // Finishing the round
        // Clear all investigator activated
        foreach (Quest.Hero h in game.quest.heroes)
        {
            h.activated = false;
        }

        //  Clear monster activations
        foreach (Quest.Monster m in game.quest.monsters)
        {
            m.activated = false;
            m.minionStarted = false;
            m.masterStarted = false;
            m.currentActivation = null;
        }

        // Advance to next round
        game.quest.round++;
        game.quest.horrorPhase = false;
        game.quest.threat += 1;

        // Update monster display
        game.monsterCanvas.UpdateStatus();
    }
}
