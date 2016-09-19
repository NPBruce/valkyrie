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
}
