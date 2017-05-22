using UnityEngine;
using Assets.Scripts.Content;
using System.Collections.Generic;

namespace Assets.Scripts.UI
{
    public class UIWindowSelectionListTraits : UIWindowSelectionList
    {
        public UIWindowSelectionListTraits(UnityEngine.Events.UnityAction<string> call, string title = "") : base(call, title)
        {
        }

        public UIWindowSelectionListTraits(UnityEngine.Events.UnityAction<string> call, StringKey title) : base(call, title)
        {
        }

        public void Draw()
        {
            // Border
            UIElement ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-18), 0, 36, 30);
            new UIElementBorder(ui);

            // Title
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-10), 0, 20, 1);
            ui.SetText(_title);

            UIElementScrollVertical traitScrollArea = new UIElementScrollVertical();
            traitScrollArea.SetLocation(UIScaler.GetHCenter(-17.5f), 2, 13, 25);

            for (int i = 0; i < items.Count; i++)
            {
                // Print the name but select the key
                string key = items[i].GetKey();
                ui = new UIElement(traitScrollArea.GetScrollTransform());
                ui.SetLocation(0, (i * 1.05f), 20, 1);
                if (key != null)
                {
                    ui.SetButton(delegate { SelectItem(key); });
                }
                ui.SetBGColor(Color.white);
                ui.SetText(items[i].GetDisplay(), Color.black);
            }

            traitScrollArea.SetScrollSize(items.Count * 1.05f);


            UIElementScrollVertical itemScrollArea = new UIElementScrollVertical();
            itemScrollArea.SetLocation(UIScaler.GetHCenter(-3.5f), 2, 21, 25);

            for (int i = 0; i < items.Count; i++)
            {
                // Print the name but select the key
                string key = items[i].GetKey();
                ui = new UIElement(itemScrollArea.GetScrollTransform());
                ui.SetLocation(0, (i * 1.05f), 20, 1);
                if (key != null)
                {
                    ui.SetButton(delegate { SelectItem(key); });
                }
                ui.SetBGColor(Color.white);
                ui.SetText(items[i].GetDisplay(), Color.black);
            }

            itemScrollArea.SetScrollSize(items.Count * 1.05f);

            // Cancel button
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-4.5f), 28, 9, 1);
            ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
            ui.SetText(CommonStringKeys.CANCEL);
            ui.SetButton(delegate { Destroyer.Dialog(); });
            new UIElementBorder(ui);
        }

        public void AddItem(StringKey stringKey, Dictionary<string, IEnumerable<string>> traits)
        {
            items.Add(new SelectionItemTraits(stringKey.Translate(), stringKey.key, traits));
        }

        public void AddItem(string item, Dictionary<string, IEnumerable<string>> traits)
        {
            items.Add(new SelectionItemTraits(item, item, traits));
        }

        public void AddItem(string display, string key, Dictionary<string, IEnumerable<string>> traits)
        {
            items.Add(new SelectionItemTraits(display, key, traits));
        }

        public void AddItem(QuestData.QuestComponent qc)
        {

        }

        public void AddNewComponentItem(string type)
        {
            Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();

            traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { type });
            traits.Add(new StringKey("val", "SOURCE").Translate(), new string[] { new StringKey("val", "NEW").Translate() });

            items.Add(new SelectionItemTraits(new StringKey("val", "NEW_X", type.ToUpper()).Translate(), "{NEW:" + type + "}", traits));
        }

        protected class SelectionItemTraits : SelectionItem
        {
            Dictionary<string, IEnumerable<string>> _traits = new Dictionary<string, IEnumerable<string>>();

            public SelectionItemTraits(string display, string key) : base(display, key)
            {
            }

            public SelectionItemTraits(string display, string key, Dictionary<string, IEnumerable<string>> traits) : base(display, key)
            {
                _traits = traits;
            }

            public Dictionary<string, IEnumerable<string>> GetTraits()
            {
                return _traits;
            }
        }
    }
}
