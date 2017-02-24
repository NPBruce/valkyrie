using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

// Class for creation of a dialog window with buttons and handling button press
// This is used for display of event information
public class DialogWindow {
    // The even that raises this dialog
    public EventManager.Event eventData;
    // An event can have a list of selected heroes
    public List<Quest.Hero> heroList;

    // Create from event
    public DialogWindow(EventManager.Event e)
    {
        eventData = e;
        heroList = new List<Quest.Hero>();
        Game game = Game.Get();

        // hero list can be populated from another event
        if (!eventData.qEvent.heroListName.Equals(""))
        {
            // Try to find the event
            if (!game.quest.heroSelection.ContainsKey(eventData.qEvent.heroListName))
            {
                Debug.Log("Warning: Hero selection in event: " + eventData.qEvent.name + " from event " + eventData.qEvent.heroListName + " with no data.");
            }
            else
            {
                // Get selection data from other event
                foreach (Quest.Hero h in game.quest.heroSelection[eventData.qEvent.heroListName])
                {
                    h.selected = true;
                }
                // Update selection status
                game.heroCanvas.UpdateStatus();
            }
        }

        CreateWindow();
    }

    public void CreateWindow()
    {
        // Draw text
        DialogBox db = new DialogBox(new Vector2(10, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 8), eventData.GetText());
        db.AddBorder();

        // Do we have a cancel button?
        if (eventData.qEvent.cancelable)
        {
            new TextButton(new Vector2(11, 9f), new Vector2(8f, 2), "Cancel", delegate { onCancel(); });
        }

        float offset = 9f;
        int num = 1;
        float length = 8f;
        float hOffset = UIScaler.GetWidthUnits() - 19f;
        if (evenData.GetButtons().Count > 2)
        {
            length = 16f;
            hOffset = UIScaler.GetHCenter(-8f);
        }
        foreach (EventButton eb in eventData.GetButtons())
        {
            int numTmp = num++;
            new TextButton(new Vector2(hOffset, offset), new Vector2(length, 2), eb.label, delegate { onButton(numTmp); }, eb.colour);
            offset += 2.5f;
        }

        // Do we have a cancel button?
        if (eventData.qEvent.cancelable)
        {
            if (evenData.GetButtons().Count > 2)
            {
                new TextButton(new Vector2(11, offset), new Vector2(8f, 2), "Cancel", delegate { onCancel(); });
            }
            else
            {
                new TextButton(new Vector2(hOffset, 9f), new Vector2(8f, 2), "Cancel", delegate { onCancel(); });
            }
        }

    }

    // Cancel cleans up
    public void onCancel()
    {
        destroy();
        Game.Get().quest.eManager.currentEvent = null;
        // There may be a waiting event
        Game.Get().quest.eManager.TriggerEvent();
    }

    public void onButton(int num)
    {
        // Do we have correct hero selection?
        if (!checkHeroes()) return;

        Game game = Game.Get();
        // Destroy this dialog to close
        destroy();

        // If the user started this event (cancelable) failing is undoable
        if (eventData.qEvent.cancelable)
        {
            game.quest.Save();
        }
        // Event manager handles the failure
        game.quest.eManager.EndEvent(num-1);
    }

    // Check that the correct number of heroes are selected
    public bool checkHeroes()
    {
        Game game = Game.Get();

        heroList = new List<Quest.Hero>();

        // List all selected heroes
        foreach (Quest.Hero h in game.quest.heroes)
        {
            if (h.selected)
            {
                heroList.Add(h);
            }
        }

        // Check that count matches
        if (eventData.qEvent.maxHeroes < heroList.Count && eventData.qEvent.maxHeroes != 0) return false;
        if (eventData.qEvent.minHeroes > heroList.Count) return false;

        // Clear selection
        foreach (Quest.Hero h in game.quest.heroes)
        {
            h.selected = false;
        }

        // If this event has previous selected heroes clear the data
        if (game.quest.heroSelection.ContainsKey(eventData.qEvent.name))
        {
            game.quest.heroSelection.Remove(eventData.qEvent.name);
        }
        // Add this selection to the quest
        game.quest.heroSelection.Add(eventData.qEvent.name, heroList);

        // Update hero image state
        game.heroCanvas.UpdateStatus();

        // Selection OK
        return true;
    }

    public void destroy()
    {
        // Clean up everything marked as 'dialog'
        Destroyer.Dialog();
    }

    public class EventButton
    {
        public string label = "";
        public Color colour = Color.white;

        public EventButton(string l)
        {
            label = l;
        }
    }
}
