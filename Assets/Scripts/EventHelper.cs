using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EventHelper {


    public static void eventTriggerType(string type)
    {
        Game game = GameObject.FindObjectOfType<Game>();
        foreach (KeyValuePair<string, QuestData.QuestComponent> k in game.qd.components)
        {
            QuestData.QuestComponent c = k.Value;

            // Check if it is an event
            if (c is QuestData.Event)
            {
                QuestData.Event e = (QuestData.Event)c;
                if (e.trigger.Equals(type))
                    triggerEvent(e.name);
            }
        }
    }

    public static void triggerEvent(string name)
    {
        Game game = GameObject.FindObjectOfType<Game>();
        // Check if the event doesn't exists - quest fault
        if (!game.qd.components.ContainsKey(name))
        {
            Debug.Log("Warning: Missing event called: " + name);
            return;
        }

        QuestData.Event e = (QuestData.Event)game.qd.components[name];

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
                game.qd.flags.Add(s);
        }

        // Remove clear flags
        foreach (string s in e.clearFlags)
        {
            if (game.qd.flags.Contains(s))
                game.qd.flags.Remove(s);
        }

        // If this is a monster event then add the monster group
        if (e is QuestData.Monster)
        {
            QuestData.Monster qm = (QuestData.Monster)e;

            // Is this type new?
            bool newMonster = true;
            foreach (Game.Monster m in game.monsters)
            {
                if (m.monsterData.name.Equals(qm.mData.name))
                    newMonster = false;
            }

            // Add the new type
            if (newMonster)
            {
                game.monsters.Add(new Game.Monster(qm.mData));
                MonsterCanvas mc = GameObject.FindObjectOfType<MonsterCanvas>();
                mc.UpdateList();
            }
        }

        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        new DialogWindow(e);
        foreach (string s in e.addComponents)
        {
            game.qd.components[s].setVisible(true);
        }
        foreach (string s in e.removeComponents)
        {
            game.qd.components[s].setVisible(false);
        }

        if (e.locationSpecified)
        {
            Camera cam = GameObject.FindObjectOfType<Camera>();
            cam.transform.position = new Vector3(e.location.x * 105, e.location.y * 105, -800);
        }
    }


    public static bool IsEnabled(string name)
    {
        Game game = GameObject.FindObjectOfType<Game>();
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
