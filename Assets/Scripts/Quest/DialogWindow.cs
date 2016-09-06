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
        Game game = Game.Get();
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
                game.heroCanvas.UpdateStatus();
            }
        }

        CreateWindow();
    }

    public void CreateWindow()
    {
        DialogBox db = new DialogBox(new Vector2(10, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 8), eventData.text.Replace("\\n", "\n"));
        db.AddBorder();

        // Do we have a cancel button?
        if (eventData.cancelable)
        {
            new TextButton(new Vector2(11, 9f), new Vector2(8f, 2), "Cancel", delegate { onCancel(); });
        }
        // If there isn't a fail event we have a confirm button
        if(eventData.failEvent.Length == 0)
        {
            new TextButton(new Vector2(UIScaler.GetWidthUnits() - 19, 9f), new Vector2(8f, 2), "Confirm", delegate { onConfirm(); });
        }
        // Otherwise we have pass and fail buttons
        else
        {
            new TextButton(new Vector2(UIScaler.GetWidthUnits() - 19, 11.5f), new Vector2(8f, 2), "Fail", delegate { onFail(); }, Color.red);
            new TextButton(new Vector2(UIScaler.GetWidthUnits() - 19, 9f), new Vector2(8f, 2), "Pass", delegate { onPass(); }, Color.green);
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
        Game game = Game.Get();

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

        game.heroCanvas.UpdateStatus();

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

        if (eventData.name.IndexOf("EventEnd") == 0)
        {
            Destroyer.MainMenu();
        }
    }

    public void destroy()
    {
        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        Game game = Game.Get();
        game.eventList.Pop();
    }
}
