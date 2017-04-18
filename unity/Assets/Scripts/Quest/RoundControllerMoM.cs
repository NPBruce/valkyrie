﻿using UnityEngine;
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
        game.quest.phase = Quest.MoMPhase.mythos;
        game.stageUI.Update();

        game.quest.eManager.EventTriggerType("Mythos", false);
        // This will cause the next phase if nothing was added
        game.quest.eManager.TriggerEvent();

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
                notActivated.Add(i);
        }

        if (notActivated.Count > 0)
        {
            // Find a random unactivated monster
            Quest.Monster toActivate = game.quest.monsters[notActivated[Random.Range(0, notActivated.Count)]];

            ActivateMonster(toActivate);
            // Return false as activations remain
            return false;
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

        if (game.quest.phase == Quest.MoMPhase.investigator)
        {
            return;
        }

        if (game.quest.phase == Quest.MoMPhase.mythos)
        {
            if (game.quest.monsters.Count > 0)
            {
                game.quest.phase = Quest.MoMPhase.monsters;
                game.stageUI.Update();
                ActivateMonster();
                return;
            }
            else
            {
                game.quest.phase = Quest.MoMPhase.horror;
                game.stageUI.Update();
                EndRound();
            }
        }

        if (game.quest.phase == Quest.MoMPhase.monsters)
        {
            game.quest.phase = Quest.MoMPhase.horror;
            game.stageUI.Update();
            return;
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
        int round = Mathf.RoundToInt(game.quest.vars.GetValue("#round")) + 1;
        game.quest.vars.SetValue("#round", round);

        game.quest.phase = Quest.MoMPhase.investigator;
        game.stageUI.Update();

        // Update monster display
        game.monsterCanvas.UpdateStatus();

        game.audioControl.PlayTrait("newround");

        // Start of round events
        game.quest.eManager.EventTriggerType("StartRound");
    }
}
