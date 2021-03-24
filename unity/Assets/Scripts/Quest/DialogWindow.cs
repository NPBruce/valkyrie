using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
using ValkyrieTools;

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
        text = eventData.GetText();

        // hero list can be populated from another event
        if (!eventData.qEvent.heroListName.Equals(""))
        {
            // Try to find the event
            if (!game.quest.heroSelection.ContainsKey(eventData.qEvent.heroListName))
            {
                ValkyrieDebug.Log("Warning: Hero selection in event: " + eventData.qEvent.sectionName + " from event " + eventData.qEvent.heroListName + " with no data.");
                game.quest.log.Add(new Quest.LogEntry("Warning: Hero selection in event: " + eventData.qEvent.sectionName + " from event " + eventData.qEvent.heroListName + " with no data.", true));
            }
            else
            {
                // Get selection data from other event
                foreach (Quest.Hero h in game.quest.heroSelection[eventData.qEvent.heroListName])
                {
                    h.selected = true;
                }
            }
        }
        // Update selection status
        game.heroCanvas.UpdateStatus();

        if (eventData.qEvent.quota > 0 || eventData.qEvent.quotaVar.Length > 0)
        {
            if (eventData.qEvent.quotaVar.Length > 0)
            {
                quota = Mathf.RoundToInt(game.quest.vars.GetValue(eventData.qEvent.quotaVar));
            }
            CreateQuotaWindow();
        }
        else
        {
            CreateWindow();
        }

        DrawItem();
    }

    public void CreateWindow()
    {
        // Draw text
        UIElement ui = new UIElement();
        float offset = ui.GetStringHeight(text, 28);
        if (offset < 4)
        {
            offset = 4;
        }
        ui.SetLocation(UIScaler.GetHCenter(-14f), 0.5f, 28, offset);
        ui.SetText(text);
        new UIElementBorder(ui);
        offset += 1f;
        
        if (Game.Get().googleTtsEnabled)
            new UITtsSpeakButton(ui);

        // Determine button size
        float buttonWidth = 8;
        float hOffset = UIScaler.GetWidthUnits() - 19f;
        float hOffsetCancel = 11;
        float offsetCancel = offset;
        
        List<DialogWindow.EventButton> buttons = eventData.GetButtons();
        foreach (EventButton eb in buttons)
        {
            float length = ui.GetStringWidth(eb.GetLabel().Translate(), UIScaler.GetMediumFont());
            if (length > buttonWidth)
            {
                buttonWidth = length;
                hOffset = UIScaler.GetHCenter(-length / 2);
                hOffsetCancel = UIScaler.GetHCenter(-4);
                offsetCancel = offset + (2.5f * buttons.Count);
            }
        }

        
        var varManager = Game.Get().quest.vars;
        
        int num = 0;
        foreach (EventButton eb in buttons)
        {
            var numTmp = num++;
            // Handle condition failure
            var buttonConditionFailed = eb.condition != null && !varManager.Test(eb.condition);
            if (buttonConditionFailed && eb.action == QuestButtonAction.HIDE)
            {
                continue;
            }
            ui = new UIElement();
            ui.SetLocation(hOffset, offset, buttonWidth, 2);
            if (buttonConditionFailed && eb.action == QuestButtonAction.DISABLE)
            {
                ui.SetText(eb.GetLabel(), Color.gray);
                ui.SetFontSize(UIScaler.GetMediumFont());
                new UIElementBorder(ui, Color.gray);
            }
            else
            {
                ui.SetText(eb.GetLabel(), eb.colour);
                ui.SetFontSize(UIScaler.GetMediumFont());
                new UIElementBorder(ui, eb.colour);
                ui.SetButton(delegate { onButton(numTmp); });
            }

            offset += 2.5f;
        }

        // Do we have a cancel button?
        if (eventData.qEvent.cancelable)
        {
            ui = new UIElement();
            ui.SetLocation(hOffsetCancel, offsetCancel, 8, 2);
            ui.SetText(CommonStringKeys.CANCEL);
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(onCancel);
            new UIElementBorder(ui);
        }
    }

    public void CreateQuotaWindow()
    {
        // Draw text
        UIElement ui = new UIElement();
        float offset = ui.GetStringHeight(text, 28);
        if (offset < 4)
        {
            offset = 4;
        }
        ui.SetLocation(UIScaler.GetHCenter(-14f), 0.5f, 28, offset);
        ui.SetText(text);
        new UIElementBorder(ui);
        offset += 1;

        ui = new UIElement();
        ui.SetLocation(11, offset, 2, 2);
        new UIElementBorder(ui);
        if (quota == 0)
        {
            ui.SetText(CommonStringKeys.MINUS, Color.grey);
        }
        else
        {
            ui.SetText(CommonStringKeys.MINUS);
            ui.SetButton(quotaDec);
        }
        ui.SetFontSize(UIScaler.GetMediumFont());

        ui = new UIElement();
        ui.SetLocation(14, offset, 2, 2);
        ui.SetText(quota.ToString());
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation(17, offset, 2, 2);
        new UIElementBorder(ui);
        if (quota >= 10)
        {
            ui.SetText(CommonStringKeys.PLUS, Color.grey);
        }
        else
        {
            ui.SetText(CommonStringKeys.PLUS);
            ui.SetButton(quotaInc);
        }
        ui.SetFontSize(UIScaler.GetMediumFont());
        
        // Only one button, action depends on quota
        ui = new UIElement();
        ui.SetLocation(UIScaler.GetWidthUnits() - 19, offset, 8, 2);
        ui.SetText(eventData.GetButtons()[0].GetLabel());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(onQuota);
        new UIElementBorder(ui);

        // Do we have a cancel button?
        if (eventData.qEvent.cancelable)
        {
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-4f), offset + 2.5f, 8, 2);
            ui.SetText(CommonStringKeys.CANCEL);
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(onCancel);
            new UIElementBorder(ui);
        }
    }

    public void DrawItem()
    {
        if (eventData.qEvent.highlight) return;

        string item = "";
        int items = 0;
        foreach (string s in eventData.qEvent.addComponents)
        {
            if (s.IndexOf("QItem") == 0)
            {
                item = s;
                items++;
            }
        }
        if (items != 1) return;

        Game game = Game.Get();

        if (!game.quest.itemSelect.ContainsKey(item)) return;

        var selectedItemData = game.cd.Get<ItemData>(game.quest.itemSelect[item]);
        Texture2D tex = ContentData.FileToTexture(selectedItemData.image);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect);

        UIElement ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-21), 0.5f, 6, 6);
        ui.SetImage(sprite);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-22.5f), 6.5f, 9, 1);
        ui.SetText(selectedItemData.name);
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
        if (eventData.qEvent.quotaVar.Length > 0)
        {
            game.quest.vars.SetValue(eventData.qEvent.quotaVar, quota);
            onButton(0);
            return;
        }
        
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
            onButton(0);
        }
        else
        {
            onButton(1);
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
        game.quest.log.Add(new Quest.LogEntry(text.Replace("\n", "\\n")));

        // Add this to the eventList
        game.quest.eventList.Add(eventData.qEvent.sectionName);

        // Event manager handles the aftermath
        game.quest.eManager.EndEvent(num);
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
        StringKey label = StringKey.NULL;
        public Color32 colour = Color.white;
        public VarTests condition = null;
        public QuestButtonAction action = QuestButtonAction.NONE;

        public EventButton(QuestButtonData buttonData)
        {
            label = buttonData.Label;
            string colorRGB = ColorUtil.FromName(buttonData.Color);      

            // Check format is valid
            if ((colorRGB.Length != 7 && colorRGB.Length != 9) || (colorRGB[0] != '#'))
            {
                Game.Get().quest.log.Add(new Quest.LogEntry("Warning: Button color must be in #RRGGBB format or a known name", true));
            }

            // Hexadecimal to float convert (0x00-0xFF -> 0.0-1.0)
            colour.r = (byte)System.Convert.ToByte(colorRGB.Substring(1, 2), 16);
            colour.g = (byte)System.Convert.ToByte(colorRGB.Substring(3, 2), 16);
            colour.b = (byte)System.Convert.ToByte(colorRGB.Substring(5, 2), 16);

            if (colorRGB.Length == 9)
                colour.a = (byte)System.Convert.ToByte(colorRGB.Substring(7, 2), 16);
            else
                colour.a = 255; // opaque by default

            this.condition = buttonData.Condition;
            this.action = buttonData.ConditionFailedAction;
        }

        public StringKey GetLabel()
        {
            return new StringKey(null, EventManager.OutputSymbolReplace(EventManager.Event.ReplaceComponentText(label.Translate())), false);
        }
    }
}
