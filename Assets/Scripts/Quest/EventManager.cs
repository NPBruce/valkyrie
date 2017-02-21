using System.Collections.Generic;
using UnityEngine;

// Class for managing quest events
public class EventManager
{
    // A dictionary of available events
    public Dictionary<string, Event> events;

    // Stack of events to be triggered
    public Stack<Event> eventStack;

    public Game game;

    // Event currently open
    public Event currentEvent;

    public EventManager()
    {
        game = Game.Get();

        events = new Dictionary<string, Event>();
        eventStack = new Stack<Event>();

        // Find quest events
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                // If the event is a monster type cast it
                if (kv.Value is QuestData.Monster)
                {
                    events.Add(kv.Key, new MonsterEvent(kv.Key));
                }
                else
                {
                    events.Add(kv.Key, new Event(kv.Key));
                }
            }
        }

        // Add game content perils as available events
        // This is needed as perils may raise other peril events
        foreach (KeyValuePair<string, PerilData> kv in game.cd.perils)
        {
            events.Add(kv.Key, new Peril(kv.Key));
        }
    }

    // Queue a peril event of a type
    public void RaisePeril(PerilData.PerilType type)
    {
        List<string> list = new List<string>();
        // Find all matching perils
        foreach (KeyValuePair<string, PerilData> kv in game.cd.perils)
        {
            if (kv.Value.pType == type)
            {
                Peril p = new Peril(kv.Key);
                // If peril is valid add to list
                if (!p.Disabled())
                {
                    list.Add(kv.Key);
                }
            }
        }
        // Queue a random valid peril
        QueueEvent(list[Random.Range(0, list.Count)]);
    }

    // Queue all events by trigger, optionally start
    public void EventTriggerType(string type, bool trigger=true)
    {
        foreach (KeyValuePair<string, Event> kv in events)
        {
            if (kv.Value.qEvent.trigger.Equals(type))
            {
                QueueEvent(kv.Key, trigger);
            }
        }
    }

    // Queue event, optionally trigger next event
    public void QueueEvent(string name, bool trigger=true)
    {
        // Check if the event doesn't exists - quest fault
        if (!events.ContainsKey(name))
        {
            Debug.Log("Warning: Missing event called: " + name);
            return;
        }

        // Don't queue disabled events
        if (events[name].Disabled()) return;

        // Place this on top of the stack
        eventStack.Push(events[name]);

        // IF there is a current event trigger if specified
        if (currentEvent == null && trigger)
        {
            TriggerEvent();
        }
    }

    // Trigger next event in stack
    public void TriggerEvent()
    {
        // First check if things need to be added to the queue at end round
        Game.Get().roundControl.CheckNewRound();

        // No events to trigger
        if (eventStack.Count == 0) return;

        // Get the next event
        Event e = eventStack.Pop();
        currentEvent = e;

        // Event may have been disabled since added
        if (e.Disabled()) return;

        // Add set flags
        foreach (string s in e.qEvent.setFlags)
        {
            Debug.Log("Notice: Setting quest flag: " + s + System.Environment.NewLine);
            game.quest.flags.Add(s);
        }

        // Remove clear flags
        foreach (string s in e.qEvent.clearFlags)
        {
            Debug.Log("Notice: Clearing quest flag: " + s + System.Environment.NewLine);
            game.quest.flags.Remove(s);
        }

        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        // If this is a monster event then add the monster group
        if (e is MonsterEvent)
        {
            // Set monster tag if not already
            game.quest.flags.Add("#monsters");

            MonsterEvent qe = (MonsterEvent)e;

            // Is this type new?
            Quest.Monster oldMonster = null;
            foreach (Quest.Monster m in game.quest.monsters)
            {
                if (m.monsterData.name.Equals(qe.cMonster.name))
                {
                    // Matched existing monster
                    oldMonster = m;
                }
            }

            // Add the new type
            if (!game.gameType.MonstersGrouped() || oldMonster == null)
            {
                game.quest.monsters.Add(new Quest.Monster(qe));
                game.monsterCanvas.UpdateList();
            }
            // There is an existing group, but now it is unique
            else if (qe.qMonster.unique)
            {
                oldMonster.unique = true;
                oldMonster.uniqueText = qe.qMonster.uniqueText;
                oldMonster.uniqueTitle = qe.GetUniqueTitle();
            }

            // Display the location(s)
            game.tokenBoard.AddMonster(qe);
        }

        // Highlight a space on the board
        if (e.qEvent.highlight)
        {
            game.tokenBoard.AddHighlight(e.qEvent);
        }

        // Add board components
        game.quest.Add(e.qEvent.addComponents);
        // Remove board components
        game.quest.Remove(e.qEvent.removeComponents);
        // Adjust threat
        game.quest.threat += e.qEvent.threat;

        // Set absolute threat
        if (e.qEvent.absoluteThreat)
        {
            if (e.qEvent.threat != 0)
            {
                Debug.Log("Setting threat to: " + e.qEvent.threat + System.Environment.NewLine);
            }
            game.quest.threat = e.qEvent.threat;
        }
        else if (e.qEvent.threat != 0)
        {
            Debug.Log("Changing threat by: " + e.qEvent.threat + System.Environment.NewLine);
        }

        // Add delayed events
        foreach (QuestData.Event.DelayedEvent de in e.qEvent.delayedEvents)
        {
            game.quest.delayedEvents.Add(new QuestData.Event.DelayedEvent(de.delay + game.quest.round, de.eventName));
        }

        // Move camera
        if (e.qEvent.locationSpecified)
        {
            CameraController.SetCamera(e.qEvent.location);
        }

        // Only raise dialog if there is text, otherwise auto confirm
        if (e.GetText().Length == 0)
        {
            EndEvent();
        }
        else
        {
            new DialogWindow(e);
        }
    }

    // Event ended (pass or set as fail)
    public void EndEvent(int state=0)
    {
        // Get list of next events
        List<string> eventList = currentEvent.qEvent.nextEvent[state];

        // Only take enabled events from list
        List<string> enabledEvents = new List<string>();
        foreach (string s in eventList)
        {
            if (!game.quest.eManager.events[s].Disabled())
            {
                enabledEvents.Add(s);
            }
        }

        // Are there any events?
        if (enabledEvents.Count > 0)
        {
            // Are we picking at random?
            if (currentEvent.qEvent.randomEvents)
            {
                currentEvent = null;
                // Start a random event
                game.quest.eManager.QueueEvent(enabledEvents[Random.Range(0, enabledEvents.Count)]);
            }
            else
            {
                currentEvent = null;
                // Start the first valid event
                game.quest.eManager.QueueEvent(enabledEvents[0]);

            }
            // Chained event ongoing
            return;
        }

        // Does this event end the quest?
        if (currentEvent.qEvent.name.IndexOf("EventEnd") == 0)
        {
            Destroyer.MainMenu();
            return;
        }
        // Trigger a stacked event
        currentEvent = null;
        TriggerEvent();
    }

    // Event control class
    public class Event
    {
        public Game game;
        public QuestData.Event qEvent;

        // Create event from quest data
        public Event(string name)
        {
            game = Game.Get();
            if (game.quest.qd.components.ContainsKey(name))
            {
                qEvent = game.quest.qd.components[name] as QuestData.Event;
            }
        }

        // Get the text to display for the event
        virtual public string GetText()
        {
            string text = qEvent.text;

            // Default door text
            if (qEvent is QuestData.Door && text.Length == 0)
            {
                text = "You can open this door with an \"Open Door\" action.";
            }

            // Find and replace rnd:hero with a hero
            // replaces all occurances with the one hero
            text = text.Replace("{rnd:hero}", game.quest.GetRandomHero().heroData.name);

            // Random numbers in events
            try
            {
                // Find first random number tag
                int index = text.IndexOf("{rnd:");
                // loop through event text
                while (index != -1)
                {
                    // find end of tag
                    string rand = text.Substring(index, text.IndexOf("}", index) + 1 - index);
                    // find separator
                    int separator = rand.IndexOf(":", 5);
                    // Parse min and max
                    int min, max;
                    int.TryParse(rand.Substring(5, separator - 5), out min);
                    int.TryParse(rand.Substring(separator + 1, rand.Length - separator - 2), out max);
                    // Replace with random number
                    text = text.Replace(rand, Random.Range(min, max + 1).ToString());
                    //find next random tag
                    index = text.IndexOf("{rnd:");
                }
            }
            catch (System.Exception)
            {
                Debug.Log("Warning: Invalid random clause in event dialog: " + text + System.Environment.NewLine);
            }

            // Fix new lines and replace symbol text with special characters
            return SymbolReplace(text).Replace("\\n", "\n");
        }

        public List<DialogWindow.EventButton> GetButtons()
        {
            List<DialogWindow.EventButton> buttons = new List<DialogWindow.EventButton>();

            // Determine if no buttons should be displayed
            if (!ButtonsPresent())
            {
                return buttons;
            }

            foreach (string s in qEvent.buttons)
            {
                DialogWindow.EventButton eb = new DialogWindow.EventButton(s);
                // Hack for pass/fail color buttons
                if (qEvent.nextEvent.Count == 2)
                {
                    if (buttons.Count == 0 && eb.label.Equals("Pass"))
                    {
                        eb.colour = Color.green;
                    }
                    if (buttons.Count == 1 && eb.label.Equals("Fail"))
                    {
                        eb.colour = Color.red;
                    }
                }
                buttons.Add(eb);
            }

            return buttons;
        }

        // Is the confirm button present?
        public bool ButtonsPresent()
        {
            // If the event can't be canceled it must have buttons
            if (!qEvent.cancelable) return true;
            // Check if any of the next events are enabled
            foreach (List<string> l in qEvent.nextEvent)
            {
                foreach (string s in l)
                {
                    if (!game.quest.eManager.events[s].Disabled()) return true;
                }
            }
            // Nothing valid, no buttons
            return false;
        }

        // Is this event disabled?
        public bool Disabled()
        {
            // Check all flags
            foreach (string s in qEvent.flags)
            {
                // Is the flag missing from the quest?
                if (!game.quest.flags.Contains(s))
                    return true;
            }
            // No missing flags
            return false;
        }
    }

    // Monster event extends event for adding monsters
    public class MonsterEvent : Event
    {
        public QuestData.Monster qMonster;
        public MonsterData cMonster;

        public MonsterEvent(string name) : base(name)
        {
            // cast the monster event
            qMonster = qEvent as QuestData.Monster;
            // Try to find a type that is valid
            // Searches is specified order
            // FIXME: is this reverse order?
            foreach (string t in qMonster.mTypes)
            {
                // Monster type might be a unique for this quest
                if (game.quest.qd.components.ContainsKey(t) && game.quest.qd.components[t] is QuestData.UniqueMonster)
                {
                    cMonster = new QuestMonster(game.quest.qd.components[t] as QuestData.UniqueMonster);
                }
                // Monster type might exist in content packs, 'Monster' is optional
                else if (game.cd.monsters.ContainsKey(t))
                {
                    cMonster = game.cd.monsters[t];
                }
                else if (game.cd.monsters.ContainsKey("Monster" + t))
                {
                    cMonster = game.cd.monsters["Monster" + t];
                }
            }

            // If we didn't find anything try by trait
            if (cMonster == null)
            {
                // No matches or trait match
                if (qMonster.mTraits.Length == 0)
                {
                    Debug.Log("Error: Cannot find monster and no traits provided in event: " + qMonster.name);
                    Application.Quit();
                }

                // Start a list of matches
                List<MonsterData> list = new List<MonsterData>();
                foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
                {
                    bool allFound = true;
                    foreach (string t in qMonster.mTraits)
                    {
                        // Does the monster have this trait?
                        if (!kv.Value.ContainsTrait(t))
                        {
                            // Trait missing, exclude monster
                            allFound = false;
                        }
                    }
                    // Monster has all traits
                    if (allFound)
                    {
                        list.Add(kv.Value);
                    }
                }

                // Not found, throw error
                if (list.Count == 0)
                {
                    Debug.Log("Error: Unable to find monster of traits specified in event: " + qMonster.name);
                    Application.Quit();
                }

                // Pick monster at random from candidates
                cMonster = list[Random.Range(0, list.Count)];
            }
        }

        // Event text
        override public string GetText()
        {
            // Monster events have {type} replaced with the selected type
            return base.GetText().Replace("{type}", cMonster.name);
        }

        // Unique monsters can have a special name
        public string GetUniqueTitle()
        {
            // Default to Master {type}
            if (qMonster.uniqueTitle.Equals(""))
            {
                return "Master " + cMonster.name;
            }
            return qMonster.uniqueTitle.Replace("{type}", cMonster.name);
        }
    }

    // Peril extends event
    public class Peril : Event
    {
        public PerilData cPeril;

        public Peril(string name) : base(name)
        {
            // Event is pulled from content data not quest data
            qEvent = game.cd.perils[name] as QuestData.Event;
            cPeril = qEvent as PerilData;
        }
    }

    // Replace symbol markers with special characters
    public static string SymbolReplace(string input)
    {
        string output = input;
        output = output.Replace("{heart}", "≥");
        output = output.Replace("{fatigue}", "∏");
        output = output.Replace("{might}", "∂");
        if (Game.Get().gameType is MoMGameType)
        {
            output = output.Replace("{will}", "");
            output = output.Replace("{action}", "");
        }
        else
        {
            output = output.Replace("{will}", "π");
            output = output.Replace("{action}", "∞");
        }
        output = output.Replace("{knowledge}", "∑");
        output = output.Replace("{awareness}", "μ");
        output = output.Replace("{shield}", "≤");
        output = output.Replace("{surge}", "±");
        output = output.Replace("{strenth}", "");
        output = output.Replace("{agility}", "");
        output = output.Replace("{lore}", "");
        output = output.Replace("{influence}", "");
        output = output.Replace("{success}", "");
        output = output.Replace("{clue}", "");
        output = output.Replace("{MAD01}", "");
        output = output.Replace("{MAD06}", "");
        output = output.Replace("{MAD09}", "");
        output = output.Replace("{MAD20}", "");
        output = output.Replace("{MAD21}", "");
        output = output.Replace("{MAD22}", "");
        output = output.Replace("{MAD23}", "");

        return output;
    }
}

