using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoundController {

    // A hero has finished their turn
    virtual public void HeroActivated()
    {
        Game game = Game.Get();
        // Check if all heros have finished
        bool herosActivated = true;
        foreach (Quest.Hero h in game.quest.heroes)
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
    public void ParticalActivationComplete(Quest.Monster m)
    {
        // Start the other half of the activation
        new ActivateDialog(m, m.minionStarted);
        m.minionStarted = true;
        m.masterStarted= true;
    }


    virtual public void MonsterActivated()
    {
        Game game = Game.Get();

        // Check for any partial monster activations
        foreach (Quest.Monster m in game.quest.monsters)
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
        foreach (Quest.Hero h in game.quest.heroes)
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
    virtual public bool ActivateMonster()
    {
        Game game = Game.Get();

        List<int> notActivated = new List<int>();
        // Get the index of all monsters that haven't activated
        for (int i = 0; i < game.quest.monsters.Count; i++)
        {
            if (!game.quest.monsters[i].activated)
                notActivated.Add(i);
        }

        // If no monsters are found return true
        if (notActivated.Count == 0)
            return true;

        // Find a random unactivated monster
        Quest.Monster toActivate = game.quest.monsters[notActivated[Random.Range(0, notActivated.Count)]];

        return ActivateMonster(toActivate);
    }

    virtual public bool ActivateMonster(Quest.Monster m)
    {
        List<ActivationData> adList = new List<ActivationData>();
        Game game = Game.Get();

        bool customActivations = false;
        MonsterData md = m.monsterData;

        QuestMonster qm = md as QuestMonster;
        if (qm != null)
        {
            if (game.cd.monsters.ContainsKey(qm.derivedType))
            {
                md = game.cd.monsters[qm.derivedType];
            }
            customActivations = !qm.useMonsterTypeActivations;
        }

        if (customActivations)
        {
            if (!qm.useMonsterTypeActivations)
            {
                adList = new List<ActivationData>();
                foreach (string s in qm.activations)
                {
                    // This should check for quest activations!
                    if (game.quest.qd.components.ContainsKey("Activation" + s))
                    {
                        adList.Add(new QuestActivation(game.quest.qd.components["Activation" + s] as QuestData.Activation));
                    }
                    else if (game.cd.activations.ContainsKey("MonsterActivation" + s))
                    {
                        adList.Add(game.cd.activations["MonsterActivation" + s]);
                    }
                    else
                    {
                        Debug.Log("Warning: Unable to find activation: " + s + " for monster type: " + m.monsterData.sectionName);
                    }
                }
            }
        }
        else
        {
            // Find all possible activations
            foreach (KeyValuePair<string, ActivationData> kv in game.cd.activations)
            {
                // Is this activation for this monster type? (replace "Monster" with "MonsterActivation", ignore specific variety)
                if (kv.Key.IndexOf("MonsterActivation" + md.sectionName.Substring("Monster".Length)) == 0)
                {
                    adList.Add(kv.Value);
                }
            }
            // Search for additional common activations
            foreach (string s in md.activations)
            {
                if (game.cd.activations.ContainsKey("MonsterActivation" + s))
                {
                    adList.Add(game.cd.activations["MonsterActivation" + s]);
                }
                else
                {
                    Debug.Log("Warning: Unable to find activation: " + s + " for monster type: " + md.sectionName);
                }
            }
        }

        // Check for no activations
        if (adList.Count == 0)
        {
            Debug.Log("Error: Unable to find any activation data for monster type: " + md.name);
            Application.Quit();
        }

        if (m.currentActivation == null)
        {
            // Pick a random activation
            ActivationData activation = adList[Random.Range(0, adList.Count)];
            m.NewActivation(activation);
        }

        // If no minion activation just do master
        if (m.currentActivation.ad.minionActions.Length == 0)
        {
            m.minionStarted = true;
            m.masterStarted = true;
            new ActivateDialog(m, true);
            return false;
        }

        // If no master activation just do minion
        if (m.currentActivation.ad.masterActions.Length == 0)
        {
            m.minionStarted = true;
            m.masterStarted = true;
            new ActivateDialog(m, false);
            return false;
        }

        // Pick Minion or master
        m.minionStarted = Random.Range(0, 2) == 0;
        if(m.currentActivation.ad.masterFirst)
        {
            m.minionStarted = false;
        }
        if (m.currentActivation.ad.minionFirst)
        {
            m.minionStarted = true;
        }

        m.masterStarted = !m.minionStarted;

        // Create activation window
        new ActivateDialog(m, m.masterStarted);

        // More groups unactivated
        return false;
    }

    public void EndRound()
    {
        Game game = Game.Get();
        game.quest.eManager.EventTriggerType("EndRound", false);
        game.quest.eManager.EventTriggerType("EndRound" + game.quest.round, false);
        // This will cause the end of the round if nothing was added
        game.quest.eManager.TriggerEvent();
    }

    public void CheckNewRound()
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

        if (!game.quest.minorPeril && game.quest.qd.quest.minorPeril <= game.quest.round)
        {
            game.quest.eManager.RaisePeril(PerilData.PerilType.minor);
            game.quest.minorPeril = true;
            return;
        }

        if (!game.quest.majorPeril && game.quest.qd.quest.majorPeril <= game.quest.round)
        {
            game.quest.eManager.RaisePeril(PerilData.PerilType.major);
            game.quest.majorPeril = true;
            return;
        }

        if (!game.quest.deadlyPeril && game.quest.qd.quest.deadlyPeril <= game.quest.round)
        {
            game.quest.eManager.RaisePeril(PerilData.PerilType.deadly);
            game.quest.deadlyPeril = true;
            return;
        }

        // Check if all heros have finished
        foreach (Quest.Hero h in game.quest.heroes)
        {
            if (!h.activated && h.heroData != null) return;
        }

        // Check if all heros have finished
        foreach (Quest.Monster m in game.quest.monsters)
        {
            if (!m.activated) return;
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
        game.quest.threat += 1;

        // Update monster and hero display
        game.monsterCanvas.UpdateStatus();
        game.heroCanvas.UpdateStatus();
    }
}
