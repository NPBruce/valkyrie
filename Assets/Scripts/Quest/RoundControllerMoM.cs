using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoundControllerMoM : RoundController
{
    // Investigators have finished
    override public void HeroActivated()
    {
        Game game = Game.Get();

        for (int i = 0; i < game.quest.heroes.Count; i++)
        {
            game.quest.heroes[i].activated = true;
        }
        if (ActivateMonster())
        {
            EndRound();
        }
    }

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

        if (ActivateMonster())
        {
            EndRound();
        }
    }

    // Activate All monsters
    override public bool ActivateMonster()
    {
        Game game = Game.Get();

        bool allActivated = false;
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
                return false;
            }
        }
        return true;
    }


    public override void CheckNewRound()
    {

        Game game = Game.Get();

        if (game.quest.eManager.currentEvent != null)
            return;

        if (game.quest.eManager.eventStack.Count > 0)
            return;

        foreach (QuestData.Event.DelayedEvent de in game.quest.delayedEvents)
        {
            if (de.delay == game.quest.round)
            {
                game.quest.delayedEvents.Remove(de);
                game.quest.eManager.QueueEvent(de.eventName);
                return;
            }
        }

        foreach (Quest.Hero h in game.quest.heroes)
        {
            h.activated = false;
        }
        foreach (Quest.Monster m in game.quest.monsters)
        {
            m.activated = false;
            m.minionStarted = false;
            m.masterStarted = false;
            m.currentActivation = null;
        }

        game.quest.round++;
        game.quest.horrorPhase = false;
        game.quest.threat += 1;

        // Update monster display
        game.monsterCanvas.UpdateStatus();
    }
}
