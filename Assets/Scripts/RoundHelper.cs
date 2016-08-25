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

        // Find a random unactivated monster
        Game.Monster toActivate = game.monsters[notActivated[Random.Range(0, notActivated.Count)]];

        List<ActivationData> adList = new List<ActivationData>();
        
        // Find all possible activations
        foreach (KeyValuePair<string, ActivationData> kv in game.cd.activations)
        {
            // Is this activation for this monster type?
            // Fixme - bugged on some names
            if (kv.Key.IndexOf("MonsterActivation" + toActivate.monsterData.name) == 0)
            {
                adList.Add(kv.Value);
            }
        }

        // Check for no activations
        if(adList.Count == 0)
        {
            Debug.Log("Error: Unable to find any activation data for monster type: " + toActivate.monsterData.name);
            Application.Quit();
        }


        // Pick a random activation
        ActivationData activation = adList[Random.Range(0, adList.Count)];

        // Pick Minion of master
        bool master = Random.Range(0, 1) < 0.5;

        // Create activation window
        new ActivateDialog(toActivate, activation, master);

        // If there was one group left return true
        if (notActivated.Count == 1)
            return true;

        // More groups unactivated
        return false;
    }
}
