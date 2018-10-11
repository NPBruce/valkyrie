﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

// This round controller extends the standard controller for MoM specific round order
public class RoundControllerMoM : RoundController
{
    bool endRoundRequested = false;

    // Investigators have finished
    override public void HeroActivated()
    {
        Game game = Game.Get();

        // Mark all Investigators as finished
        for (int i = 0; i < game.quest.heroes.Count; i++)
        {
            game.quest.heroes[i].activated = true;
        }
        game.quest.phase = Quest.MoMPhase.mythos;
        game.stageUI.Update();
        game.monsterCanvas.UpdateList();

        game.quest.eManager.EventTriggerType("Mythos", false);
        // This will cause the next phase if nothing was added
        game.quest.eManager.TriggerEvent();

        // Display the transition dialog for investigator phase
        ChangePhaseWindow.DisplayTransitionWindow(Quest.MoMPhase.mythos);

        return;
    }

    // Mark a monster as activated
    override public void MonsterActivated()
    {
        Game game = Game.Get();

        // Check for any partial monster activations
        foreach (Quest.Monster m in game.quest.monsters)
        {
            if (m.minionStarted || m.masterStarted)
            {
                m.activated = true;
            }
        }

        // Activate a monster
        if (ActivateMonster())
        {
            CheckNewRound();
        }
    }

    // Activate a monster
    override public bool ActivateMonster()
    {
        Game game = Game.Get();

        // Search for unactivated monsters
        List<int> notActivated = new List<int>();
        // Get the index of all monsters that haven't activated
        for (int i = 0; i < game.quest.monsters.Count; i++)
        {
            if (!game.quest.monsters[i].activated)
            {
                QuestMonster qm = game.quest.monsters[i].monsterData as QuestMonster;
                if (qm != null && qm.activations != null && qm.activations.Length == 1 && qm.activations[0].IndexOf("Event") == 0 
                    && game.quest.eManager.events[qm.activations[0]].Disabled())
                {
                    // monster cannot be activated, mark as activated
                    game.quest.monsters[i].activated = true;
                }
                else
                {
                    notActivated.Add(i);
                }
            }
        }

        if (notActivated.Count > 0)
        {
            // Find a random unactivated monster
            Quest.Monster toActivate = game.quest.monsters[notActivated[Random.Range(0, notActivated.Count)]];

            // Find out of this monster is quest specific
            QuestMonster qm = toActivate.monsterData as QuestMonster;
            if (qm != null && qm.activations != null && qm.activations.Length == 1 && qm.activations[0].IndexOf("Event") == 0)
            {
                toActivate.masterStarted = true;
                game.quest.eManager.monsterImage = toActivate;
                game.quest.eManager.QueueEvent(qm.activations[0]);
            }
            else
            {
                ActivateMonster(toActivate);
            }
            // Return false as activations remain
            return false;
        }
        return true;
    }

    public override void EndRound()
    {
        endRoundRequested = true;
        base.EndRound();
    }


    // Check if there are events that are required at the end of the round
    public override bool CheckNewRound()
    {

        Game game = Game.Get();

        // Return if there is an event open
        if (game.quest.eManager.currentEvent != null)
            return false;

        // Return if there is an event queued
        if (game.quest.eManager.eventStack.Count > 0)
            return false;

        if (game.quest.phase == Quest.MoMPhase.investigator)
        {
            return false;
        }

        if (game.quest.phase == Quest.MoMPhase.mythos)
        {
            if (game.quest.monsters.Count > 0)
            {
                if (ActivateMonster())
                {
                    // no monster can be activated (activation conditions may prevent existing monster from doing anything), switch to horror phase
                    game.quest.phase = Quest.MoMPhase.horror;
                    game.stageUI.Update();
                    return false;
                }
                else
                {
                    // going through monster activation: switch to phase monsters
                    game.quest.phase = Quest.MoMPhase.monsters;
                    game.stageUI.Update();
                    return game.quest.eManager.currentEvent != null;
                }
            }
            else
            {
                game.quest.phase = Quest.MoMPhase.horror;
                game.stageUI.Update();
                EndRound();
                return game.quest.eManager.currentEvent != null;
            }
        }

        if (game.quest.phase == Quest.MoMPhase.monsters)
        {
            game.quest.phase = Quest.MoMPhase.horror;
            game.stageUI.Update();
            return false;
        }

        // we need this test to make sure user can do the horro test, as a random event would switch the game to investigator phase 
        if (!endRoundRequested && game.quest.phase == Quest.MoMPhase.horror && game.quest.monsters.Count > 0)
        {
            return false;
        }

        // Finishing the round

        // reset the endRound Request
        endRoundRequested = false;

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
        int round = Mathf.RoundToInt(game.quest.vars.GetValue("#round")) + 1;
        game.quest.vars.SetValue("#round", round);

        game.quest.log.Add(new Quest.LogEntry(new StringKey("val", "PHASE_INVESTIGATOR").Translate()));

        game.quest.phase = Quest.MoMPhase.investigator;
        game.stageUI.Update();
        game.monsterCanvas.UpdateList();

        game.audioControl.PlayTrait("newround");

        // Start of round events
        game.quest.eManager.EventTriggerType("StartRound");
        SaveManager.Save(0);
        
        // Display the transition dialog for investigator phase
        ChangePhaseWindow.DisplayTransitionWindow(Quest.MoMPhase.investigator);

        return true;
    }
}
