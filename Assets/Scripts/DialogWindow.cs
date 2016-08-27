using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

// Class for creation of a dialog window with buttons and handling button press
public class DialogWindow {
    // The even that raises this dialog
    public QuestData.Event eventData;
    public List<Game.Hero> heroList;

    public DialogWindow(QuestData.Event e)
    {
        eventData = e;
        heroList = new List<Game.Hero>();
        Game game = GameObject.FindObjectOfType<Game>();
        game.eventList.Push(eventData);

        if (!eventData.heroListName.Equals(""))
        {
            if (!game.qd.heroSelection.ContainsKey(eventData.heroListName))
            {
                Debug.Log("Warning: Hero selection in event: " + eventData.name + " from event " + eventData.heroListName + " with no data.");
            }
            else
            {
                foreach (Game.Hero h in game.qd.heroSelection[eventData.heroListName])
                {
                    h.selected = true;
                }
                HeroCanvas hc = GameObject.FindObjectOfType<HeroCanvas>();
                hc.UpdateStatus();
            }
        }

        CreateWindow();
    }

    public void CreateWindow()
    {
        new DialogBox(new Vector2(300, 30), new Vector2(500, 120), eventData.text.Replace("\\n", "\n"));

        // Do we have a cancel button?
        if (eventData.cancelable)
        {
            new TextButton(new Vector2(400, 170), new Vector2(80, 20), "Cancel", delegate { onCancel(); });
        }
        // If there isn't a fail event we have a confirm button
        if(eventData.failEvent.Length == 0)
        {
            new TextButton(new Vector2(600, 170), new Vector2(80, 20), "Confirm", delegate { onConfirm(); });
        }
        // Otherwise we have pass and fail buttons
        else
        {
            new TextButton(new Vector2(500, 170), new Vector2(80, 20), "Fail", delegate { onFail(); }, Color.red);
            new TextButton(new Vector2(600, 170), new Vector2(80, 20), "Pass", delegate { onPass(); }, Color.green);
        }
    }

    // Pass and confirm are the same
    public void onPass()
    {
        onConfirm();
    }

    // Cancel cleans up
    public void onCancel()
    {
        destroy();
        EventHelper.TriggerEvent();
    }

    public void onFail()
    {
        // Destroy this dialog to close
        destroy();
        // Trigger failure event
        // Trigger next event
        foreach (string e in eventData.failEvent)
        {
            if (EventHelper.IsEnabled(e))
            {
                EventHelper.QueueEvent(e);
                return;
            }
        }
        EventHelper.TriggerEvent();
    }

    public void onConfirm()
    {
        Game game = GameObject.FindObjectOfType<Game>();

        heroList = new List<Game.Hero>();

        foreach (Game.Hero h in game.heros)
        {
            if (h.selected)
            {
                heroList.Add(h);
            }
        }

        if (eventData.maxHeroes < heroList.Count && eventData.maxHeroes != 0) return;
        if (eventData.minHeroes > heroList.Count) return;

        foreach (Game.Hero h in game.heros)
        {
            h.selected = false;
        }

        if (game.qd.heroSelection.ContainsKey(eventData.name))
        {
            game.qd.heroSelection.Remove(eventData.name);
        }
        game.qd.heroSelection.Add(eventData.name, heroList);

        HeroCanvas hc= GameObject.FindObjectOfType<HeroCanvas>();
        hc.UpdateStatus();

        // Destroy this dialog to close
        destroy();
        // Trigger next event
        foreach (string e in eventData.nextEvent)
        {
            if (EventHelper.IsEnabled(e))
            {
                EventHelper.QueueEvent(e);
                return;
            }
        }
        EventHelper.TriggerEvent();
    }

    public void destroy()
    {
        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        Game game = GameObject.FindObjectOfType<Game>();
        game.eventList.Pop();
    }
}
