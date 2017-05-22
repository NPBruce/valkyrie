using UnityEngine;
using Assets.Scripts.Content;
using System.Collections.Generic;

namespace Assets.Scripts.UI
{
    public class UIWindowSelectionList
    {
        protected string _title = "";
        protected UnityEngine.Events.UnityAction<string> _call;

        protected List<SelectionItem> items = new List<SelectionItem>();

        public UIWindowSelectionList(UnityEngine.Events.UnityAction<string> call, string title = "")
        {
            _title = title;
            _call = call;
        }

        public UIWindowSelectionList(UnityEngine.Events.UnityAction<string> call, StringKey title)
        {
            _title = title.Translate();
            _call = call;
        }

        public void AddItem(StringKey stringKey)
        {
            items.Add(new SelectionItem(stringKey.Translate(), stringKey.key));
        }

        public void AddItem(StringKey stringKey, Color color)
        {
            items.Add(new SelectionItem(stringKey.Translate(), stringKey.key, color));
        }

        public void AddItem(string item)
        {
            items.Add(new SelectionItem(item, item));
        }

        public void AddItem(string item, Color color)
        {
            items.Add(new SelectionItem(item, item, color));
        }

        public void AddItem(string display, string key)
        {
            items.Add(new SelectionItem(display, key));
        }

        public void AddItem(string display, string key, Color color)
        {
            items.Add(new SelectionItem(display, key, color));
        }

        virtual public void Draw()
        {
            // Border
            UIElement ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-11), 0, 22, 30);
            new UIElementBorder(ui);

            // Title
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-10), 0, 20, 1);
            ui.SetText(_title);

            UIElementScrollVertical scrollArea = new UIElementScrollVertical();
            scrollArea.SetLocation(UIScaler.GetHCenter(-10.5f), 2, 21, 25);
            new UIElementBorder(scrollArea);

            for (int i = 0; i < items.Count; i++)
            {
                // Print the name but select the key
                string key = items[i].GetKey();
                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(0, (i * 1.05f), 20, 1);
                if (key != null)
                {
                    ui.SetButton(delegate { SelectItem(key); });
                }
                ui.SetBGColor(items[i].GetColor());
                ui.SetText(items[i].GetDisplay(), Color.black);
            }

            scrollArea.SetScrollSize(items.Count * 1.05f);

            // Cancel button
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-4.5f), 28, 9, 1);
            ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
            ui.SetText(CommonStringKeys.CANCEL);
            ui.SetButton(delegate { Destroyer.Dialog(); });
            new UIElementBorder(ui);
        }

        protected void SelectItem(string key)
        {
            Destroyer.Dialog();
            _call(key);
        }

        protected class SelectionItem
        {
            string _display = "";
            string _key = "";
            Color _color = Color.white;

            public SelectionItem(string display, string key)
            {
                _key = key;
                _display = display;
            }

            public SelectionItem(string display, string key, Color color)
            {
                _key = key;
                _display = display;
                _color = color;
            }

            public string GetKey()
            {
                return _key;
            }

            public string GetDisplay()
            {
                return _display;
            }

            public void SetColor(Color color)
            {
                _color = color;
            }

            public Color GetColor()
            {
                return _color;
            }

        }
    }
}
