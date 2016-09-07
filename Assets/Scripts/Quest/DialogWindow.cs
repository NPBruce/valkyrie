using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

// Class for creation of a dialog window with buttons and handling button press
public class DialogWindow {
    // The even that raises this dialog
    public QuestData.Event eventData;
    public List<Round.Hero> heroList;

    public DialogWindow(QuestData.Event e)
    {
        eventData = e;
        heroList = new List<Round.Hero>();
        Game game = Game.Get();
        game.round.eventList.Push(eventData);

        if (!eventData.heroListName.Equals(""))
        {
            if (!game.qd.heroSelection.ContainsKey(eventData.heroListName))
            {
                Debug.Log("Warning: Hero selection in event: " + eventData.name + " from event " + eventData.heroListName + " with no data.");
            }
            else
            {
                foreach (Round.Hero h in game.qd.heroSelection[eventData.heroListName])
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
        Game game = Game.Get();
        DialogBox db = new DialogBox(new Vector2(10, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 8), eventData.text.Replace("\\n", "\n"));
        db.AddBorder();

        // Do we have a cancel button?
        if (eventData.cancelable)
        {
            new TextButton(new Vector2(11, 9f), new Vector2(8f, 2), "Cancel", delegate { onCancel(); });
        }

        // If there isn't a fail event we have a confirm button
        if (eventData.failEvent.Length == 0)
        {
            bool showConfirm = true;
            if (eventData is QuestData.Token || eventData is QuestData.Door)
            {
                showConfirm = false;
                foreach (string s in eventData.nextEvent)
                {
                    showConfirm |= EventHelper.EventEnabled(game.qd.components[s] as QuestData.Event);
                }
            }
            if (showConfirm)
            {
                string confirmLabel = "Confirm";
                if (!eventData.confirmText.Equals(""))
                {
                    confirmLabel = eventData.confirmText;
                }
                new TextButton(new Vector2(UIScaler.GetWidthUnits() - 19, 9f), new Vector2(8f, 2), confirmLabel, delegate { onConfirm(); });
            }
        }
        // Otherwise we have pass and fail buttons
        else
        {
            Color passColor = Color.green;
            Color failColor = Color.red;
            string confirmLabel = "Pass";
            if (!eventData.confirmText.Equals(""))
            {
                confirmLabel = eventData.confirmText;
                passColor = Color.white;
            }
            string failLabel = "Fail";
            if (!eventData.failText.Equals(""))
            {
                failLabel = eventData.failText;
                failColor = Color.white;
            }
            new TextButton(new Vector2(UIScaler.GetWidthUnits() - 19, 11.5f), new Vector2(8f, 2), failLabel, delegate { onFail(); }, failColor);
            new TextButton(new Vector2(UIScaler.GetWidthUnits() - 19, 9f), new Vector2(8f, 2), confirmLabel, delegate { onPass(); }, passColor);
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
        if (!checkHeroes()) return;
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

    public bool checkHeroes()
    {
        Game game = Game.Get();

        heroList = new List<Round.Hero>();

        foreach (Round.Hero h in game.round.heroes)
        {
            if (h.selected)
            {
                heroList.Add(h);
            }
        }

        if (eventData.maxHeroes < heroList.Count && eventData.maxHeroes != 0) return false;
        if (eventData.minHeroes > heroList.Count) return false;

        foreach (Round.Hero h in game.round.heroes)
        {
            h.selected = false;
        }

        if (game.qd.heroSelection.ContainsKey(eventData.name))
        {
            game.qd.heroSelection.Remove(eventData.name);
        }
        game.qd.heroSelection.Add(eventData.name, heroList);

        game.heroCanvas.UpdateStatus();

        return true;
    }


    public void onConfirm()
    {
        if (!checkHeroes()) return;
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
        if (eventData.name.IndexOf("EventEnd") == 0)
        {
            Destroyer.MainMenu();
        }

        EventHelper.TriggerEvent();
    }

    public void destroy()
    {
        // Clean up everything marked as 'dialog'
        Destroyer.Dialog();

        Game.Get().round.eventList.Pop();
    }
}
