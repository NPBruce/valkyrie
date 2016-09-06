using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EventHelper {


    public static void EventTriggerType(string type)
    {
        Game game = Game.Get();
        foreach (KeyValuePair<string, QuestData.QuestComponent> k in game.qd.components)
        {
            QuestData.QuestComponent c = k.Value;

            // Check if it is an event
            if (c is QuestData.Event)
            {
                QuestData.Event e = (QuestData.Event)c;
                if (e.trigger.Equals(type))
                    QueueEvent(e.name);
            }
        }
    }

    public static void QueueEvent(string name)
    {
        Game game = Game.Get();
        // Check if the event doesn't exists - quest fault
        if (!game.qd.components.ContainsKey(name))
        {
            Debug.Log("Warning: Missing event called: " + name);
            return;
        }

        QuestData.Event e = (QuestData.Event)game.qd.components[name];

        if (game.eventList.Count == 0)
        {
            game.eventList.Push(e);
            TriggerEvent();
        }
        else
        {
            QuestData.Event ce = game.eventList.Pop();
            game.eventList.Push(e);
            game.eventList.Push(ce);
        }
    }

    public static void TriggerEvent()
    {

        Game game = Game.Get();

        RoundHelper.CheckNewRound();

        if (game.eventList.Count == 0) return;

        QuestData.Event e = game.eventList.Pop();

        // If the flags are not set do not trigger event
        foreach (string s in e.flags)
        {
            if (!game.qd.flags.Contains(s))
                return;
        }

        // Add set flags
        foreach (string s in e.setFlags)
        {
            if (!game.qd.flags.Contains(s))
            {
                Debug.Log("Notice: Setting quest flag: " + s);
                game.qd.flags.Add(s);
            }
        }

        // Remove clear flags
        foreach (string s in e.clearFlags)
        {
            if (game.qd.flags.Contains(s))
            {
                Debug.Log("Notice: Clearing quest flag: " + s);
                game.qd.flags.Remove(s);
            }
        }

        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);


        // If this is a monster event then add the monster group
        if (e is QuestData.Monster)
        {
            QuestData.Monster qm = (QuestData.Monster)e;

            // Is this type new?
            Round.Monster oldMonster = null;
            foreach (Round.Monster m in game.round.monsters)
            {
                if (m.monsterData.name.Equals(qm.mData.name))
                {
                    oldMonster = m;
                }
            }
            // Add the new type
            if (oldMonster == null)
            {
                game.round.monsters.Add(new Round.Monster(qm));
                game.monsterCanvas.UpdateList();
            }
            else if(qm.unique)
            {
                oldMonster.unique = true;
                oldMonster.uniqueText = qm.uniqueText;
                oldMonster.uniqueTitle = qm.uniqueTitle;
            }

            // Display the location
            game.tokenBoard.AddMonster(qm);
        }

        if (e.highlight)
        {
            game.tokenBoard.AddHighlight(e);
        }

        new DialogWindow(e);
        foreach (string s in e.addComponents)
        {
            if (game.qd.components.ContainsKey(s))
            {
                game.qd.components[s].SetVisible(true);
            }
            else
            {
                Debug.Log("Warning: Attempting to show missing item: " + s);
            }
        }
        foreach (string s in e.removeComponents)
        {
            if (game.qd.components.ContainsKey(s))
            {
                game.qd.components[s].SetVisible(false);
            }
            else
            {
                Debug.Log("Warning: Attempting to hide missing item: " + s);
            }
        }

        if (e.locationSpecified)
        {
            CameraController.SetCamera(e.location);
        }
    }


    public static bool IsEnabled(string name)
    {
        Game game = Game.Get();
        // Check if the event doesn't exists - quest fault
        if (!game.qd.components.ContainsKey(name))
        {
            Debug.Log("Warning: Missing event called: " + name);
            return false;
        }

        QuestData.Event e = (QuestData.Event)game.qd.components[name];

        // If the flags are not set do not trigger event
        foreach (string s in e.flags)
        {
            if (!game.qd.flags.Contains(s))
                return false;
        }

        return true;
    }
}
