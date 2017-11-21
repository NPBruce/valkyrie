using UnityEngine;
using Assets.Scripts.Content;
using System.Collections.Generic;

namespace Assets.Scripts.UI
{
    public class UIWindowSelectionList
    {
        protected string _title = "";
        protected UnityEngine.Events.UnityAction<string> _call;

        protected SortedList<int, SelectionItem> items = new SortedList<int, SelectionItem>();
        protected SortedList<string, SelectionItem> alphaItems = new SortedList<string, SelectionItem>();

        protected bool alphaSort = false;
        protected bool reverseSort = false;

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
            AddItem(new SelectionItem(stringKey.Translate(), stringKey.key));
        }

        public void AddItem(StringKey stringKey, Color color)
        {
            AddItem(new SelectionItem(stringKey.Translate(), stringKey.key, color));
        }

        public void AddItem(string item)
        {
            AddItem(new SelectionItem(item, item));
        }

        public void AddItem(string item, Color color)
        {
            AddItem(new SelectionItem(item, item, color));
        }

        public void AddItem(string display, string key)
        {
            AddItem(new SelectionItem(display, key));
        }

        public void AddItem(string display, string key, Color color)
        {
            AddItem(new SelectionItem(display, key, color));
        }

        virtual public void AddItem(SelectionItem item)
        {
            items.Add(items.Count, item);
            string key = item.GetDisplay();
            int duplicateIndex = 0;
            while (alphaItems.ContainsKey(key))
            {
                key = item.GetDisplay() + "_" + duplicateIndex++;
            }
            alphaItems.Add(key, item);
        }

        virtual public void Draw()
        {
            Update();
        }

        virtual public void Update()
        {
            // Border
            UIElement ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-11), 0, 22, 30);
            new UIElementBorder(ui);

            // Title
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-10), 0, 20, 1);
            ui.SetText(_title);

            // Sort Buttons
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(8.5f), 1, 1, 1);
            if (alphaSort)
            {
                ui.SetText("1", Color.white);
                ui.SetBGColor(Color.black);
            }
            else
            {
                if (reverseSort)
                {
                    ui.SetText("9", Color.black);
                }
                else
                {
                    ui.SetText("1", Color.black);
                }
                ui.SetBGColor(Color.white);
            }
            ui.SetButton(SortNumerical);
            new UIElementBorder(ui);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(9.5f), 1, 1, 1);
            if (alphaSort)
            {
                if (reverseSort)
                {
                    ui.SetText("Z", Color.black);
                }
                else
                {
                    ui.SetText("A", Color.black);
                }
                ui.SetBGColor(Color.white);
            }
            else
            {
                ui.SetText("A", Color.white);
                ui.SetBGColor(Color.black);
            }
            ui.SetButton(SortAlpha);
            new UIElementBorder(ui);

            UIElementScrollVertical scrollArea = new UIElementScrollVertical();
            scrollArea.SetLocation(UIScaler.GetHCenter(-10.5f), 2, 21, 25);
            new UIElementBorder(scrollArea);

            List<SelectionItem> toDisplay = new List<SelectionItem>(items.Values);
            if (alphaSort)
            {
                toDisplay = new List<SelectionItem>(alphaItems.Values);
            }
            if (reverseSort)
            {
                toDisplay.Reverse();
            }

            int lineNum = 0;
            foreach(SelectionItem item in toDisplay)
            {
                // Print the name but select the key
                string key = item.GetKey();
                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(0, (lineNum * 1.05f), 20, 1);
                if (key != null)
                {
                    ui.SetButton(delegate { SelectItem(key); });
                }
                ui.SetBGColor(item.GetColor());
                ui.SetText(item.GetDisplay(), Color.black);
                lineNum++;
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

        protected void SortNumerical()
        {
            if (!alphaSort)
            {
                reverseSort = !reverseSort;
            }
            alphaSort = false;
            Update();
        }

        protected void SortAlpha()
        {
            if (alphaSort)
            {
                reverseSort = !reverseSort;
            }
            alphaSort = true;
            Update();
        }

        protected void SelectItem(string key)
        {
            Destroyer.Dialog();
            _call(key);
        }

        public class SelectionItem
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
