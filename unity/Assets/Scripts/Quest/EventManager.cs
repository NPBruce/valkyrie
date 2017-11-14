using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Content;
using ValkyrieTools;
using System.IO;
using System.Text.RegularExpressions;
using System;

// Class for managing quest events
public class EventManager
{
    // A dictionary of available events
    public Dictionary<string, Event> events;

    // events should display monster image when not null
    public Quest.Monster monsterImage;
    // events should display monster health if true
    public bool monsterHealth = false;

    // Stack of events to be triggered
    public Stack<Event> eventStack;

    public Game game;

    // Event currently open
    public Event currentEvent;

    public EventManager()
    {
        Init(null);
    }

    public EventManager(Dictionary<string, string> data)
    {
        Init(data);
    }

    public void Init(Dictionary<string, string> data)
    {
        game = Game.Get();

        // This is filled out later but is required for loading saves
        game.quest.eManager = this;

        events = new Dictionary<string, Event>();
        eventStack = new Stack<Event>();

        // Find quest events
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                // If the event is a monster type cast it
                if (kv.Value is QuestData.Spawn)
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
        foreach (KeyValuePair<string, PerilData> kv in game.cd.perils)
        {
            events.Add(kv.Key, new Peril(kv.Key));
        }

        if (data != null)
        {
            if (data.ContainsKey("queue"))
            {
                foreach (string s in data["queue"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries))
                {
                    eventStack.Push(events[s]);
                }
            }
            if (data.ContainsKey("monsterimage"))
            {
                monsterImage = Quest.Monster.GetMonster(data["monsterimage"]);
            }
            if (data.ContainsKey("monsterhealth"))
            {
                bool.TryParse(data["monsterhealth"], out monsterHealth);
            }
            if (data.ContainsKey("currentevent") && game.quest.activeShop != data["currentevent"])
            {
                currentEvent = events[data["currentevent"]];
                ResumeEvent();
            }
        }
    }

    // Queue all events by trigger, optionally start
    public void EventTriggerType(string type, bool trigger=true)
    {
        foreach (KeyValuePair<string, Event> kv in events)
        {
            if (kv.Value.qEvent != null && kv.Value.qEvent.trigger.Equals(type))
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
            if (File.Exists(Path.GetDirectoryName(game.quest.qd.questPath) + "/" + name))
            {
                events.Add(name, new StartQuestEvent(name));
            }
            else
            {
                game.quest.log.Add(new Quest.LogEntry("Warning: Missing event called: " + name, true));
                return;
            }
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
        Game game = Game.Get();
        // First check if things need to be added to the queue at end round
        if (game.roundControl.CheckNewRound()) return;

        // No events to trigger
        if (eventStack.Count == 0) return;

        // Get the next event
        Event e = eventStack.Pop();
        currentEvent = e;

        // Move to another quest
        if (e is StartQuestEvent)
        {
            // This loads the game
            game.quest.ChangeQuest((e as StartQuestEvent).name);
            return;
        }

        // Event may have been disabled since added
        if (e.Disabled())
        {
            currentEvent = null;
            TriggerEvent();
            return;
        }

        // Play audio
        if (game.cd.audio.ContainsKey(e.qEvent.audio))
        {
            game.audioControl.Play(game.cd.audio[e.qEvent.audio].file);
        }
        else if (e.qEvent.audio.Length > 0)
        {
            game.audioControl.Play(Path.GetDirectoryName(game.quest.qd.questPath) + "/" + e.qEvent.audio);
        }

        // Set Music
        if (e.qEvent.music.Count > 0)
        {
            List<string> music = new List<string>();
            foreach (string s in e.qEvent.music)
            {
                if (game.cd.audio.ContainsKey(s))
                {
                    music.Add(game.cd.audio[s].file);
                }
                else
                {
                    music.Add(Path.GetDirectoryName(game.quest.qd.questPath) + "/" + s);
                }
            }
            game.audioControl.Music(music, false);
            if (music.Count > 1)
            {
                game.quest.music = new List<string>(e.qEvent.music);
            }
        }

        // Perform var operations
        game.quest.vars.Perform(e.qEvent.operations);
        // Update morale change
        if (game.gameType is D2EGameType)
        {
            game.quest.AdjustMorale(0);
        }
        if (game.quest.vars.GetValue("$restock") == 1)
        {
            game.quest.GenerateItemSelection();
        }

        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            UnityEngine.Object.Destroy(go);

        // If this is a monster event then add the monster group
        if (e is MonsterEvent)
        {
            MonsterEvent qe = (MonsterEvent)e;

            // Is this type new?
            Quest.Monster oldMonster = null;
            foreach (Quest.Monster m in game.quest.monsters)
            {
                if (m.monsterData.sectionName.Equals(qe.cMonster.sectionName))
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
                // Update monster var
                game.quest.vars.SetValue("#monsters", game.quest.monsters.Count);
            }
            // There is an existing group, but now it is unique
            else if (qe.qMonster.unique)
            {
                oldMonster.unique = true;
                oldMonster.uniqueText = qe.qMonster.uniqueText;
                oldMonster.uniqueTitle = qe.GetUniqueTitle();
                oldMonster.healthMod = Mathf.RoundToInt(qe.qMonster.uniqueHealthBase + (Game.Get().quest.GetHeroCount() * qe.qMonster.uniqueHealthHero));
            }

            // Display the location(s)
            if (qe.qEvent.locationSpecified && e.qEvent.display)
            {
                game.tokenBoard.AddMonster(qe);
            }
        }

        // Highlight a space on the board
        if (e.qEvent.highlight)
        {
            game.tokenBoard.AddHighlight(e.qEvent);
        }

        // Is this a shop?
        List<string> itemList = new List<string>();
        foreach (string s in e.qEvent.addComponents)
        {
            if (s.IndexOf("QItem") == 0)
            {
                itemList.Add(s);
            }
        }
        // Add board components
        game.quest.Add(e.qEvent.addComponents, itemList.Count > 1);
        // Remove board components
        game.quest.Remove(e.qEvent.removeComponents);

        // Move camera
        if (e.qEvent.locationSpecified && !(e.qEvent is QuestData.UI))
        {
            CameraController.SetCamera(e.qEvent.location);
        }

        if (e.qEvent is QuestData.Puzzle)
        {
            QuestData.Puzzle p = e.qEvent as QuestData.Puzzle;
            if (p.puzzleClass.Equals("slide"))
            {
                new PuzzleSlideWindow(e);
            }
            if (p.puzzleClass.Equals("code"))
            {
                new PuzzleCodeWindow(e);
            }
            if (p.puzzleClass.Equals("image"))
            {
                new PuzzleImageWindow(e);
            }
            if (p.puzzleClass.Equals("tower"))
            {
                new PuzzleTowerWindow(e);
            }
            return;
        }

        // Set camera limits
        if (e.qEvent.minCam)
        {
            CameraController.SetCameraMin(e.qEvent.location);
        }
        if (e.qEvent.maxCam)
        {
            CameraController.SetCameraMax(e.qEvent.location);
        }

        // Is this a shop?
        if (itemList.Count > 1 && !game.quest.boardItems.ContainsKey("#shop"))
        {
            game.quest.boardItems.Add("#shop", new ShopInterface(itemList, Game.Get(), e.qEvent.sectionName));
        }
        else if (!e.qEvent.display)
        {
            // Only raise dialog if there is text, otherwise auto confirm
            EndEvent();
        }
        else
        {
            if (monsterImage != null)
            {
                MonsterDialogMoM.DrawMonster(monsterImage);
                if (monsterHealth)
                {
                }
            }
            new DialogWindow(e);
        }
    }

    public void ResumeEvent()
    {
        Event e = currentEvent;
        if (e is MonsterEvent)
        {
            // Display the location(s)
            if (e.qEvent.locationSpecified && e.qEvent.display)
            {
                game.tokenBoard.AddMonster(e as MonsterEvent);
            }
        }

        // Highlight a space on the board
        if (e.qEvent.highlight)
        {
            game.tokenBoard.AddHighlight(e.qEvent);
        }

        if (e.qEvent is QuestData.Puzzle)
        {
            QuestData.Puzzle p = e.qEvent as QuestData.Puzzle;
            if (p.puzzleClass.Equals("slide"))
            {
                new PuzzleSlideWindow(e);
            }
            if (p.puzzleClass.Equals("code"))
            {
                new PuzzleCodeWindow(e);
            }
            if (p.puzzleClass.Equals("image"))
            {
                new PuzzleImageWindow(e);
            }
            return;
        }
        new DialogWindow(e);
    }

    // Event ended
    public void EndEvent(int state = 0)
    {
        EndEvent(currentEvent.qEvent, state);
    }

    // Event ended
    public void EndEvent(QuestData.Event eventData, int state=0)
    {
        // Get list of next events
        List<string> eventList = new List<string>();
        if (eventData.nextEvent.Count > state)
        {
            eventList = eventData.nextEvent[state];
        }

        // Only take enabled events from list
        List<string> enabledEvents = new List<string>();
        foreach (string s in eventList)
        {
            // Check if the event doesn't exists - quest fault
            if (!events.ContainsKey(s))
            {
                if (File.Exists(Path.GetDirectoryName(game.quest.qd.questPath) + "/" + s))
                {
                    events.Add(s, new StartQuestEvent(s));
                    enabledEvents.Add(s);
                }
                else
                {
                    game.quest.log.Add(new Quest.LogEntry("Warning: Missing event called: " + s, true));
                }
            }
            else if (!game.quest.eManager.events[s].Disabled())
            {
                enabledEvents.Add(s);
            }
        }

        // Does this event end the quest?
        if (eventData.sectionName.IndexOf("EventEnd") == 0)
        {
            Destroyer.MainMenu();
            return;
        }

        currentEvent = null;
        // Are there any events?
        if (enabledEvents.Count > 0)
        {
            // Are we picking at random?
            if (eventData.randomEvents)
            {
                // Add a random event
                game.quest.eManager.QueueEvent(enabledEvents[UnityEngine.Random.Range(0, enabledEvents.Count)], false);
            }
            else
            {
                // Add the first valid event
                game.quest.eManager.QueueEvent(enabledEvents[0], false);
            }
        }

        // Add any custom triggered events
        AddCustomTriggers();

        if (eventStack.Count == 0)
        {
            monsterImage = null;
            monsterHealth = false;
            if (game.quest.phase == Quest.MoMPhase.monsters)
            {
                game.roundControl.MonsterActivated();
                return;
            }
        }

        // Trigger a stacked event
        TriggerEvent();
    }

    public void AddCustomTriggers()
    {
        foreach (KeyValuePair<string, float> kv in game.quest.vars.GetPrefixVars("@"))
        {
            if (kv.Value > 0)
            {
                game.quest.vars.SetValue(kv.Key, 0);
                EventTriggerType("Var" + kv.Key.Substring(1), false);
            }
        }
        foreach (KeyValuePair<string, float> kv in game.quest.vars.GetPrefixVars("$@"))
        {
            if (kv.Value > 0)
            {
                game.quest.vars.SetValue(kv.Key, 0);
                EventTriggerType("Var" + "$" + kv.Key.Substring(2), false);
            }
        }
    }

    // Event control class
    public class Event
    {
        public Game game;
        public QuestData.Event qEvent;
        public bool cancelable;

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
            string text = qEvent.text.Translate(true);

            // Find and replace {q:element with the name of the
            // element

            if (text.Contains("{c:"))
            {
                Regex questItemRegex = new Regex("{c:(((?!{).)*?)}");
                string replaceFrom;
                string componentName;
                string componentText;
                foreach (Match oneVar in questItemRegex.Matches(text))
                {
                    replaceFrom = oneVar.Value;                    
                    componentName = oneVar.Groups[1].Value;
                    QuestData.QuestComponent component;
                    if (game.quest.qd.components.TryGetValue(componentName, out component))
                    {
                        componentText = getComponentText(component);
                        text = text.Replace(replaceFrom, componentText);
                    }
                }
            }

            // Find and replace rnd:hero with a hero
            // replaces all occurances with the one hero

            Quest.Hero h = game.quest.GetRandomHero();
            if (text.Contains("{rnd:hero"))
            {
                h.selected = true;
            }
            text = text.Replace("{rnd:hero}", h.heroData.name.Translate());

            // Random heroes can have custom lookups
            if (text.StartsWith("{rnd:hero:"))
            {
                HeroData hero = h.heroData;
                int start = "{rnd:hero:".Length;
                if (!hero.ContainsTrait("male"))
                {
                    if (text[start] == '{')
                    {
                        start = text.IndexOf("}", start);
                    }
                    start = text.IndexOf(":", start) + 1;
                    if (text[start] == '{')
                    {
                        start = text.IndexOf("}", start);
                    }
                    start = text.IndexOf(":", start) + 1;
                }
                int next = start;
                if (text[next] == '{')
                {
                    next = text.IndexOf("}", next);
                }
                next = text.IndexOf(":", next) + 1;
                int end = next;
                if (text[end] == '{')
                {
                    end = text.IndexOf("}", end);
                }
                end = text.IndexOf(":", end);
                if (end < 0) end = text.Length - 1;
                string toReplace = text.Substring(next, end - next);
                text = new StringKey(text.Substring(start, (next - start) - 1)).Translate();
                text = text.Replace(toReplace, hero.name.Translate());
            }

            // Fix new lines and replace symbol text with special characters
            return OutputSymbolReplace(text).Replace("\\n", "\n");
        }

        /// <summary>
        /// Get text related with selected component
        /// </summary>
        /// <param name="component">component to get text</param>
        /// <returns>extracted text</returns>
        private string getComponentText(QuestData.QuestComponent component)
        {
            switch (component.GetType().Name)
            {
                case "Event":
                    if(!game.quest.heroSelection.ContainsKey(component.sectionName) || game.quest.heroSelection[component.sectionName].Count == 0)
                    {
                        return component.sectionName;
                    }
                    return game.quest.heroSelection[component.sectionName][0].heroData.name.Translate();
                case "Tile":
                    // Replaced with the name of the Tile
                    return game.cd.tileSides[((QuestData.Tile)component).tileSideName].name.Translate();
                case "CustomMonster":
                    // Replaced with the custom nonster name
                    return ((QuestData.CustomMonster)component).monsterName.Translate();
                case "Spawn":
                    if (!game.quest.monsterSelect.ContainsKey(component.sectionName))
                    {
                        return component.sectionName;
                    }
                    // Replaced with the text shown in the spawn
                    string monsterName = game.quest.monsterSelect[component.sectionName];
                    if (monsterName.StartsWith("Custom")) {
                        return ((QuestData.CustomMonster)game.quest.qd.components[monsterName]).monsterName.Translate();
                    } else {
                        return game.cd.monsters[game.quest.monsterSelect[component.sectionName]].name.Translate();
                    }
                case "QItem":
                    if (!game.quest.itemSelect.ContainsKey(component.sectionName))
                    {
                        return component.sectionName;
                    }
                    // Replaced with the first element in the list
                    return game.cd.items[game.quest.itemSelect[component.sectionName]].name.Translate();
                default:
                    return component.sectionName;
            }
        }

        public static explicit operator Event(QuestData.QuestComponent v)
        {
            throw new NotImplementedException();
        }

        public List<DialogWindow.EventButton> GetButtons()
        {
            List<DialogWindow.EventButton> buttons = new List<DialogWindow.EventButton>();

            // Determine if no buttons should be displayed
            if (!ButtonsPresent())
            {
                return buttons;
            }

            for (int i = 0; i < qEvent.buttons.Count; i++)
            {
                buttons.Add(new DialogWindow.EventButton(qEvent.buttons[i], qEvent.buttonColors[i]));
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
                    // Check if the event doesn't exists - quest fault
                    if (!game.quest.eManager.events.ContainsKey(s))
                    {
                        if (File.Exists(game.quest.questPath + "/" + s))
                        {
                            game.quest.eManager.events.Add(s, new StartQuestEvent(s));
                            return true;
                        }
                        else
                        {
                            game.quest.log.Add(new Quest.LogEntry("Warning: Missing event called: " + s, true));
                            return false;
                        }
                    }
                    if (!game.quest.eManager.events[s].Disabled()) return true;
                }
            }
            // Nothing valid, no buttons
            return false;
        }

        // Is this event disabled?
        virtual public bool Disabled()
        {
            return !game.quest.vars.Test(qEvent.conditions);
        }
    }

    public class StartQuestEvent : Event
    {
        public string name;

        public StartQuestEvent(string n) : base(n)
        {
            name = n;
        }

        override public bool Disabled()
        {
            return false;
        }
    }

    // Monster event extends event for adding monsters
    public class MonsterEvent : Event
    {
        public QuestData.Spawn qMonster;
        public MonsterData cMonster;

        public MonsterEvent(string name) : base(name)
        {
            // cast the monster event
            qMonster = qEvent as QuestData.Spawn;

            if (!game.quest.monsterSelect.ContainsKey(qMonster.sectionName))
            {
                ValkyrieDebug.Log("Warning: Monster type unknown in event: " + qMonster.sectionName);
                return;
            }
            string t = game.quest.monsterSelect[qMonster.sectionName];
            if (game.quest.qd.components.ContainsKey(t))
            {
                cMonster = new QuestMonster(game.quest.qd.components[t] as QuestData.CustomMonster);
            }
            else
            {
                cMonster = game.cd.monsters[t];
            }
        }

        // Event text
        override public string GetText()
        {
            // Monster events have {type} replaced with the selected type
            return base.GetText().Replace("{type}", cMonster.name.Translate());
        }

        // Unique monsters can have a special name
        public StringKey GetUniqueTitle()
        {
            // Default to Master {type}
            if (qMonster.uniqueTitle.KeyExists())
            {
                return new StringKey("val", "MONSTER_MASTER_X", cMonster.name);
            }
            return new StringKey(qMonster.uniqueTitle,"{type}",cMonster.name.fullKey);
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


    public override string ToString()
    {
        //Game game = Game.Get();
        string nl = System.Environment.NewLine;
        // General quest state block
        string r = "[EventManager]" + nl;
        r += "queue=";
        foreach (Event e in eventStack.ToArray())
        {
            r += e.qEvent.sectionName + " ";
        }
        r += nl;
        if (currentEvent != null)
        {
            r += "currentevent=" + currentEvent.qEvent.sectionName + nl;
        }
        if (monsterImage != null)
        {
            r += "monsterimage=" + monsterImage.GetIdentifier() + nl;
        }
        if (monsterHealth)
        {
            r += "monsterhealth=" + monsterHealth.ToString() + nl;
        }
        return r;
    }

    /// <summary>
    /// Replace symbol markers with special characters to be shown in Quest
    /// </summary>
    /// <param name="input">text to show</param>
    /// <returns></returns>
    public static string OutputSymbolReplace(string input)
    {
        string output = input;
        Game game = Game.Get();

        // Fill in variable data
        try
        {
            // Find first random number tag
            int index = output.IndexOf("{var:");
            // loop through event text
            while (index != -1)
            {
                // find end of tag
                string statement = output.Substring(index, output.IndexOf("}", index) + 1 - index);
                // Replace with variable data
                output = output.Replace(statement, game.quest.vars.GetValue(statement.Substring(5, statement.Length - 6)).ToString());
                //find next random tag
                index = output.IndexOf("{var:");
            }
        }
        catch (System.Exception)
        {
            game.quest.log.Add(new Quest.LogEntry("Warning: Invalid var clause in text: " + input, true));
        }

        output = output.Replace("{heart}", "≥");
        output = output.Replace("{fatigue}", "∏");
        output = output.Replace("{might}", "∂");
        if (game.gameType is MoMGameType)
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
        output = output.Replace("{strength}", "");
        output = output.Replace("{agility}", "");
        output = output.Replace("{lore}", "");
        output = output.Replace("{influence}", "");
        output = output.Replace("{observation}", "");
        output = output.Replace("{success}", "");
        output = output.Replace("{clue}", "");
        output = output.Replace("{MAD01}", "");
        output = output.Replace("{MAD06}", "");
        output = output.Replace("{MAD09}", "");
        output = output.Replace("{MAD20}", "");
        output = output.Replace("{MAD21}", "");
        output = output.Replace("{MAD22}", "");
        output = output.Replace("{MAD23}", "");
        output = output.Replace("{MAD25}", "");

        return output;
    }

    /// <summary>
    /// Replace symbol markers with special characters to be stored in editor
    /// </summary>
    /// <param name="input">text to store</param>
    /// <returns></returns>
    public static string InputSymbolReplace(string input)
    {
        string output = input;
        Game game = Game.Get();        

        output = output.Replace("≥", "{heart}");
        output = output.Replace("∏", "{fatigue}");
        output = output.Replace("∂", "{might}");
        if (game.gameType is MoMGameType)
        {
            output = output.Replace("","{will}");
            output = output.Replace("","{action}");
        }
        else
        {
            output = output.Replace("π","{will}");
            output = output.Replace("∞","{action}");
        }
        output = output.Replace("∑", "{knowledge}");
        output = output.Replace("μ", "{awareness}");
        output = output.Replace("≤","{shield}" );
        output = output.Replace( "±","{surge}");
        output = output.Replace( "","{strength}");
        output = output.Replace( "","{agility}");
        output = output.Replace("","{lore}" );
        output = output.Replace("","{influence}" );
        output = output.Replace("","{observation}" );
        output = output.Replace( "","{success}");
        output = output.Replace("","{clue}" );
        output = output.Replace("","{MAD01}");
        output = output.Replace("","{MAD06}");
        output = output.Replace("","{MAD09}");
        output = output.Replace("","{MAD20}");
        output = output.Replace("","{MAD21}");
        output = output.Replace("","{MAD22}");
        output = output.Replace("", "{MAD23}");
        output = output.Replace("", "{MAD25}");

        return output;
    }

    public static readonly Dictionary<string, Dictionary<string, string>> CHARS_MAP =
        new Dictionary<string, Dictionary<string, string>>()
                {
                    { "D2E", new Dictionary<string,string>()
                    {
                        {"≥","{heart}"},
                        {"∏","{fatigue}"},
                        {"∂","{might}"},
                        {"π","{will}"},
                        {"∞","{action}"},
                        {"∑","{knowledge}"},
                        {"μ","{awareness}"},
                        {"≤","{shield}" },
                        {"±","{surge}"},
                        {"Rnd","{rnd:hero}"}
                    } },
                    { "MoM", new Dictionary<string,string>()
                    {
                        {"","{will}"},
                        {"","{action}" },
                        {"","{strength}"},
                        {"","{agility}"},
                        {"","{lore}"},
                        {"","{influence}"},
                        {"","{observation}"},
                        {"","{success}"},
                        {"","{clue}"},
                        {"Rnd","{rnd:hero}"}
                    } }
                };
}

