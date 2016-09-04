using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoundHelper {

    // A hero has finished their turn
    public static void HeroActivated()
    {
        Game game = Game.Get();
        // Check if all heros have finished
        bool herosActivated = true;
        foreach (Game.Hero h in game.heros)
        {
            if (!h.activated && h.heroData != null)
                herosActivated = false;
        }

        // activate a monster group (returns if all activated, does nothing if none left)
        bool monstersActivated = ActivateMonster();

        // If everyone has finished move to next round
        if (monstersActivated && herosActivated)
        {
            EndRound();
        }
    }

    // Finish the other half of monster activation
    public static void ParticalActivationComplete(Game.Monster m)
    {
        // Start the other half of the activation
        new ActivateDialog(m, m.minionStarted);
        m.minionStarted = true;
        m.masterStarted= true;
    }


    public static void MonsterActivated()
    {
        Game game = Game.Get();

        // Check for any partial monster activations
        foreach (Game.Monster m in game.monsters)
        {
            if (m.minionStarted ^ m.masterStarted)
            {
                // Half activated group, complete then return;
                ParticalActivationComplete(m);
                return;
            }

            // If both started then it is complete
            if (m.minionStarted && m.masterStarted)
            {
                m.activated = true;
            }
        }

        // Full activation, update display
        game.monsterCanvas.UpdateStatus();

        // Check if all heros have finished
        bool herosActivated = true;
        foreach (Game.Hero h in game.heros)
        {
            if (!h.activated && h.heroData != null)
                herosActivated = false;
        }

        // If there no heros left activate another monster
        if(herosActivated)
        {
            if (ActivateMonster())
            {
                // Evenyone has finished, move to next round
                EndRound();
            }
        }
    }

    // Activate a monster (if any left) and return true if all monsters activated
    public static bool ActivateMonster()
    {
        Game game = Game.Get();

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

        return ActivateMonster(toActivate);
    }

    public static bool ActivateMonster(Game.Monster m)
    {
        List<ActivationData> adList = new List<ActivationData>();
        Game game = Game.Get();

        // Find all possible activations
        foreach (KeyValuePair<string, ActivationData> kv in game.cd.activations)
        {
            // Is this activation for this monster type? (replace "Monster" with "MonsterActivation", ignore specific variety)
            if (kv.Key.IndexOf("MonsterActivation" + m.monsterData.sectionName.Substring("Monster".Length)) == 0)
            {
                adList.Add(kv.Value);
            }
        }
        // Search for additional common activations
        foreach (string s in m.monsterData.activations)
        {
            Debug.Log(s);
            if (s.IndexOf("Monster") == 0)

            if (game.cd.activations.ContainsKey("MonsterActivation" + s))
            {
                adList.Add(game.cd.activations["MonsterActivation" + s]);
            }
            else
            {
                Debug.Log("Warning: Unable to find activation: " + s + " for monster type: " + m.monsterData.sectionName);
            }
        }

        // Check for no activations
        if (adList.Count == 0)
        {
            Debug.Log("Error: Unable to find any activation data for monster type: " + m.monsterData.name);
            Application.Quit();
        }

        if (m.currentActivation == null)
        {
            // Pick a random activation
            ActivationData activation = adList[Random.Range(0, adList.Count)];
            m.currentActivation = activation;
        }

        // Pick Minion or master
        m.minionStarted = Random.Range(0, 2) == 0;
        if(m.currentActivation.masterFirst)
        {
            m.minionStarted = false;
        }
        if (m.currentActivation.minionFirst)
        {
            m.minionStarted = true;
        }

        m.masterStarted = !m.minionStarted;

        // Create activation window
        new ActivateDialog(m, m.masterStarted);

        // More groups unactivated
        return false;
    }

    public static void EndRound()
    {
        EventHelper.EventTriggerType("EndRound");
        CheckNewRound();
    }

    public static void CheckNewRound()
    {

        Game game = Game.Get();

        if (game.eventList.Count != 0)
            return;

        // Check if all heros have finished
        foreach (Game.Hero h in game.heros)
        {
            if (!h.activated && h.heroData != null) return;
        }

        // Check if all heros have finished
        foreach (Game.Monster m in game.monsters)
        {
            if (!m.activated) return;
        }

        foreach (Game.Hero h in game.heros)
        {
            h.activated = false;
        }
        foreach (Game.Monster m in game.monsters)
        {
            m.activated = false;
            m.minionStarted = false;
            m.masterStarted = false;
            m.currentActivation = null;
        }
        game.round++;

        // Update monster and hero display
        game.monsterCanvas.UpdateStatus();
        game.heroCanvas.UpdateStatus();
    }
}
