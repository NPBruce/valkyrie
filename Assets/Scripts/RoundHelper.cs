using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoundHelper {

    // A hero has finished their turn
    public static void heroActivated()
    {
        Game game = GameObject.FindObjectOfType<Game>();
        // Check if all heros have finished
        bool herosActivated = true;
        foreach (Game.Hero h in game.heros)
        {
            if (!h.activated)
                herosActivated = false;
        }

        // activate a monster group
        bool monstersActivated = activateMonster();

        // If all heros have finished activate all other monster groups
        if (herosActivated)
        {
            while (!monstersActivated)
                monstersActivated = activateMonster();
        }

        // If everyone has finished move to next round
        if (monstersActivated && herosActivated)
        {
            EventHelper.eventTriggerType("EndRound");

            foreach (Game.Hero h in game.heros)
            {
                h.activated = false;
            }
            foreach (Game.Monster m in game.monsters)
            {
                m.activated = false;
            }
            game.round++;
            MonsterCanvas mc = GameObject.FindObjectOfType<MonsterCanvas>();
            mc.UpdateStatus();
        }
    }

    // Activate a monster (if any left) and return true if all monsters activated
    public static bool activateMonster()
    {
        Game game = GameObject.FindObjectOfType<Game>();

        List<int> notActivated = new List<int>();
        // Get the index of all monsters that haven't activated
        for (int i = 0; i < game.monsters.Count; i++)
        {
            if (!game.monsters[i].activated)
                notActivated.Add(i);
        }

        // If no monsters are found return true
        if (notActivated.Count == 0)
            return true;

        // Todo: proper activation
        game.monsters[notActivated[Random.Range(0, notActivated.Count)]].activated = true;
        MonsterCanvas mc = GameObject.FindObjectOfType<MonsterCanvas>();
        mc.UpdateStatus();

        // If there was one group left return true
        if (notActivated.Count == 1)
            return true;

        // More groups unactivated
        return false;
    }
}
