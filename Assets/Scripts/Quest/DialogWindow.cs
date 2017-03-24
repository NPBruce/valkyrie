using UnityEngine;
using System.Collections.Generic;

// Class for creation of a dialog window with buttons and handling button press
// This is used for display of event information
public class DialogWindow {
    // The even that raises this dialog
    public EventManager.Event eventData;
    // An event can have a list of selected heroes
    public List<Quest.Hero> heroList;

    public int quota = 0;

    public string text= "";

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
                ValkyrieDebug.Log("Warning: Hero selection in event: " + eventData.qEvent.sectionName + " from event " + eventData.qEvent.heroListName + " with no data.");
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

        if (eventData.qEvent.quota > 0)
        {
            CreateQuotaWindow();
        }
        else
        {
            CreateWindow();
        }
    }

    public void CreateWindow()
    {
        // Draw text
        text = eventData.GetText();
        DialogBox db = new DialogBox(new Vector2(UIScaler.GetHCenter(-14f), 0.5f), new Vector2(28, 8), text);
        float offset = (db.textObj.GetComponent<UnityEngine.UI.Text>().preferredHeight / UIScaler.GetPixelsPerUnit()) + 1;
        db.Destroy();
        
        if (offset < 8)
        {
            offset = 8;
        }

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-14f), 0.5f), new Vector2(28, offset), text);
        db.AddBorder();

        offset += 1f;
        int num = 1;
        float length = 8f;
        float hOffset = UIScaler.GetWidthUnits() - 19f;
        if (eventData.GetButtons().Count > 2)
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
            if (eventData.GetButtons().Count > 2)
            {
                new TextButton(new Vector2(hOffset, offset), new Vector2(8f, 2), "Cancel", delegate { onCancel(); });
            }
            else
            {
                new TextButton(new Vector2(11, 9f), new Vector2(8f, 2), "Cancel", delegate { onCancel(); });
            }
        }

    }

    public void CreateQuotaWindow()
    {
        // Draw text
        DialogBox db = new DialogBox(new Vector2(10, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 8), eventData.GetText());
        db.AddBorder();

        if (quota == 0)
        {
            new TextButton(new Vector2(11, 9f), new Vector2(2f, 2f), "-", delegate { ; }, Color.grey);
        }
        else
        {
            new TextButton(new Vector2(11, 9f), new Vector2(2f, 2f), "-", delegate { quotaDec(); }, Color.white);
        }

        db = new DialogBox(new Vector2(14, 9f), new Vector2(2f, 2f), quota.ToString());
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        if (quota >= 10)
        {
            new TextButton(new Vector2(17, 9f), new Vector2(2f, 2f), "+", delegate { ; }, Color.grey);
        }
        else
        {
            new TextButton(new Vector2(17, 9f), new Vector2(2f, 2f), "+", delegate { quotaInc(); }, Color.white);
        }

        // Only one button, action depends on quota
        new TextButton(new Vector2(UIScaler.GetWidthUnits() - 19, 9f), new Vector2(8f, 2), eventData.GetButtons()[0].label, delegate { onQuota(); }, Color.white);

        // Do we have a cancel button?
        if (eventData.qEvent.cancelable)
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-4f), 11.5f), new Vector2(8f, 2), "Cancel", delegate { onCancel(); });
        }

    }

    public void quotaDec()
    {
        quota--;
        Destroyer.Dialog();
        CreateQuotaWindow();
    }

    public void quotaInc()
    {
        quota++;
        Destroyer.Dialog();
        CreateQuotaWindow();
    }

    public void onQuota()
    {
        Game game = Game.Get();
        if (game.quest.eventQuota.ContainsKey(eventData.qEvent.sectionName))
        {
            game.quest.eventQuota[eventData.qEvent.sectionName] += quota;
        }
        else
        {
            game.quest.eventQuota.Add(eventData.qEvent.sectionName, quota);
        }
        if (game.quest.eventQuota[eventData.qEvent.sectionName] >= eventData.qEvent.quota)
        {
            game.quest.eventQuota.Remove(eventData.qEvent.sectionName);
            onButton(1);
        }
        else
        {
            onButton(2);
        }
    }

    // Cancel cleans up
    public void onCancel()
    {
        Destroyer.Dialog();
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
        Destroyer.Dialog();

        // If the user started this event button is undoable
        if (eventData.qEvent.cancelable)
        {
            game.quest.Save();
        }

        // Add this to the log
        game.quest.log.Add(text.Replace("\n", "\\n"));

        // Event manager handles the aftermath
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
        if (game.quest.heroSelection.ContainsKey(eventData.qEvent.sectionName))
        {
            game.quest.heroSelection.Remove(eventData.qEvent.sectionName);
        }
        // Add this selection to the quest
        game.quest.heroSelection.Add(eventData.qEvent.sectionName, heroList);

        // Update hero image state
        game.heroCanvas.UpdateStatus();

        // Selection OK
        return true;
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
