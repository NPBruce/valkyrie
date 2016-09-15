using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    // A dictionary of events
    public Dictionary<string, Event> events;

    public Stack<Event> eventStack;

    public Game game;

    public Event currentEvent;

    public EventManager()
    {
        game = Game.Get();

        events = new Dictionary<string, Event>();
        eventStack = new Stack<Event>();

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
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

        foreach (KeyValuePair<string, PerilData> kv in game.cd.perils)
        {
            events.Add(kv.Key, new Peril(kv.Key));
        }
    }

    public void RaisePeril(PerilData.PerilType type)
    {
        List<string> list = new List<string>();
        foreach (KeyValuePair<string, PerilData> kv in game.cd.perils)
        {
            if (kv.Value.pType == type)
            {
                Peril p = new Peril(kv.Key);
                if (!p.Disabled())
                {
                    list.Add(kv.Key);
                }
            }
        }
        QueueEvent(list[Random.Range(0, list.Count)]);
    }

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

        if (currentEvent == null && trigger)
        {
            TriggerEvent();
        }
    }

    public void TriggerEvent()
    {
        RoundHelper.CheckNewRound();

        if (eventStack.Count == 0) return;

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
                    oldMonster = m;
                }
            }
            // Add the new type
            if (oldMonster == null)
            {
                game.quest.monsters.Add(new Quest.Monster(qe));
                game.monsterCanvas.UpdateList();
            }
            // There is an existing tpye, but now it is unique
            else if (qe.qMonster.unique)
            {
                oldMonster.unique = true;
                oldMonster.uniqueText = qe.qMonster.uniqueText;
                oldMonster.uniqueTitle = qe.GetUniqueTitle();
            }

            // Display the location
            game.tokenBoard.AddMonster(qe);
        }

        if (e.qEvent.highlight)
        {
            game.tokenBoard.AddHighlight(e.qEvent);
        }

        game.quest.Add(e.qEvent.addComponents);
        game.quest.Remove(e.qEvent.removeComponents);
        game.quest.threat += e.qEvent.threat;
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

        foreach (QuestData.Event.DelayedEvent de in e.qEvent.delayedEvents)
        {
            game.quest.delayedEvents.Add(new QuestData.Event.DelayedEvent(de.delay + game.quest.round, de.eventName));
        }

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

    public void EndEvent(bool fail=false)
    {
        string[] eventList = currentEvent.qEvent.nextEvent;
        if (fail)
        {
            eventList = currentEvent.qEvent.failEvent;
        }

        List<string> enabledEvents = new List<string>();
        foreach (string s in eventList)
        {
            if (!game.quest.eManager.events[s].Disabled())
            {
                enabledEvents.Add(s);
            }
        }
        if (enabledEvents.Count > 0)
        {
            if (currentEvent.qEvent.randomEvents)
            {
                currentEvent = null;
                game.quest.eManager.QueueEvent(enabledEvents[Random.Range(0, enabledEvents.Count)]);
            }
            else
            {
                currentEvent = null;
                game.quest.eManager.QueueEvent(enabledEvents[0]);
            }
            return;
        }

        if (currentEvent.qEvent.name.IndexOf("EventEnd") == 0)
        {
            Destroyer.MainMenu();
            return;
        }
        currentEvent = null;
        TriggerEvent();
    }

    public class Event
    {
        public Game game;
        public QuestData.Event qEvent;

        public Event(string name)
        {
            game = Game.Get();
            if (game.quest.qd.components.ContainsKey(name))
            {
                qEvent = game.quest.qd.components[name] as QuestData.Event;
            }
        }
        virtual public string GetText()
        {
            string text = qEvent.text;

            if (qEvent is QuestData.Door && text.Length == 0)
            {
                text = "You can open this door with an \"Open Door\" action.";
            }

            text = text.Replace("{rnd:hero}", game.quest.GetRandomHero().heroData.name);

            try
            {
                int index = text.IndexOf("{rnd:");
                while (index != -1)
                {
                    string rand = text.Substring(index, text.IndexOf("}", index) + 1 - index);
                    int separator = rand.IndexOf(":", 5);
                    int min = int.Parse(rand.Substring(5, separator - 5));
                    int max = int.Parse(rand.Substring(separator + 1, rand.Length - separator - 2));
                    text = text.Replace(rand, Random.Range(min, max + 1).ToString());
                    index = text.IndexOf("{rnd:");
                }
            }
            catch (System.Exception)
            {
                Debug.Log("Warning: Invalid random clause in event dialog: " + text + System.Environment.NewLine);
            }

            return SymbolReplace(text).Replace("\\n", "\n");
        }

        public bool ConfirmPresent()
        {
            if (!qEvent.cancelable) return true;
            foreach (string s in qEvent.nextEvent)
            {
                if (!game.quest.eManager.events[s].Disabled()) return true;
            }
            return false;
        }

        public bool FailPresent()
        {
            return (qEvent.failEvent.Length != 0);
        }

        public string GetPass()
        {
            if (!qEvent.confirmText.Equals("")) return qEvent.confirmText;
            if (qEvent.failEvent.Length == 0) return "Confirm";
            return "Pass";
        }
        public string GetFail()
        {
            if (!qEvent.failText.Equals("")) return qEvent.failText;
            return "Fail";
        }

        public Color GetPassColor()
        {
            if (GetPass().Equals("Pass")) return Color.green;
            return Color.white;
        }

        public Color GetFailColor()
        {
            if (GetFail().Equals("Fail")) return Color.red;
            return Color.white;
        }

        public bool Disabled()
        {
            foreach (string s in qEvent.flags)
            {
                if (!game.quest.flags.Contains(s))
                    return true;
            }
            return false;
        }

        public Event Next()
        {
            return null;
        }

        public Event NextFail()
        {
            return null;
        }
    }

    public class MonsterEvent : Event
    {
        public QuestData.Monster qMonster;
        public MonsterData cMonster;

        public MonsterEvent(string name) : base(name)
        {
            qMonster = qEvent as QuestData.Monster;
            // Next try to find a type that is valid
            foreach (string t in qMonster.mTypes)
            {
                // Monster type must exist in content packs, 'Monster' is optional
                if (game.cd.monsters.ContainsKey(t))
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
                if (qMonster.mTraits.Length == 0)
                {
                    Debug.Log("Error: Cannot find monster and no traits provided in event: " + qMonster.name);
                    Application.Quit();
                }

                List<MonsterData> list = new List<MonsterData>();
                foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
                {
                    bool allFound = true;
                    foreach (string t in qMonster.mTraits)
                    {
                        if (!kv.Value.ContainsTrait(t))
                        {
                            allFound = false;
                        }
                    }
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

                cMonster = list[Random.Range(0, list.Count)];
            }
        }

        override public string GetText()
        {
            return base.GetText().Replace("{type}", cMonster.name);
        }

        public string GetUniqueTitle()
        {
            if (qMonster.uniqueTitle.Equals(""))
            {
                return "Master " + cMonster.name;
            }
            return qMonster.uniqueTitle.Replace("{type}", cMonster.name);
        }
    }

    public class Peril : Event
    {
        public PerilData cPeril;

        public Peril(string name) : base(name)
        {
            qEvent = game.cd.perils[name] as QuestData.Event;
            cPeril = qEvent as PerilData;
        }
    }

    public static string SymbolReplace(string input)
    {
        string output = input;
        output = output.Replace("{heart}", "≥");
        output = output.Replace("{fatigue}", "∏");
        output = output.Replace("{might}", "∂");
        output = output.Replace("{will}", "π");
        output = output.Replace("{knowledge}", "∑");
        output = output.Replace("{awareness}", "μ");
        output = output.Replace("{action}", "∞");
        output = output.Replace("{shield}", "≤");
        output = output.Replace("{surge}", "±");
        return output;
    }

}
