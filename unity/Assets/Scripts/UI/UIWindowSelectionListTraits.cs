using UnityEngine;
using Assets.Scripts.Content;
using System.Collections.Generic;

namespace Assets.Scripts.UI
{
    public class UIWindowSelectionListTraits : UIWindowSelectionList
    {
        protected List<TraitGroup> traitData = new List<TraitGroup>();

        protected SortedList<int, SelectionItemTraits> traitItems = new SortedList<int, SelectionItemTraits>();
        protected SortedList<string, SelectionItemTraits> alphaTraitItems = new SortedList<string, SelectionItemTraits>();

        protected Dictionary<string, List<string>> initialExclusions = new Dictionary<string, List<string>>();

        protected float scrollPos = 0;

        protected UIElementScrollVertical traitScrollArea;

        public UIWindowSelectionListTraits(UnityEngine.Events.UnityAction<string> call, string title = "") : base(call, title)
        {
        }

        public UIWindowSelectionListTraits(UnityEngine.Events.UnityAction<string> call, StringKey title) : base(call, title)
        {
        }


        override public void Draw()
        {
            foreach (SelectionItemTraits item in traitItems.Values)
            {
                foreach (string category in item.GetTraits().Keys)
                {
                    bool found = false;
                    foreach (TraitGroup tg in traitData)
                    {
                        if (tg.GetName().Equals(category))
                        {
                            found = true;
                            tg.AddTraits(item);
                        }
                    }

                    if (!found)
                    {
                        TraitGroup tg = new TraitGroup(category);
                        tg.AddTraits(item);
                        traitData.Add(tg);
                    }
                }
            }

            foreach (SelectionItemTraits item in traitItems.Values)
            {
                foreach (TraitGroup tg in traitData)
                {
                    tg.AddItem(item);
                }
            }

            foreach (var exclusion in initialExclusions)
            {
                foreach (string item in exclusion.Value)
                {
                    ExcludeTrait(exclusion.Key, item);
                }
            }

            Update();
        }

        override public void Update()
        {
            bool resetScroll = false;
            if (traitScrollArea == null)
            {
                resetScroll = true;
            }
            else
            {
                scrollPos = traitScrollArea.GetScrollPosition();
            }


            // Border
            UIElement ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-18), 0, 36, 30);
            new UIElementBorder(ui);

            // Title
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-10), 0, 20, 1);
            ui.SetText(_title);

            // Sort Buttons
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(15.5f), 1, 1, 1);
            ui.SetTextPadding(0);
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
            ui.SetLocation(UIScaler.GetHCenter(16.5f), 1, 1, 1);
            ui.SetTextPadding(0);
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

            traitScrollArea = new UIElementScrollVertical();
            traitScrollArea.SetLocation(UIScaler.GetHCenter(-17.5f), 2, 13, 25);
            new UIElementBorder(traitScrollArea);

            float offset = 0;
            foreach (TraitGroup tg in traitData)
            {
                ui = new UIElement(traitScrollArea.GetScrollTransform());
                ui.SetLocation(0, offset, 12, 1);
                ui.SetText(tg.GetName(), Color.black);
                ui.SetTextAlignment(TextAnchor.MiddleLeft);
                ui.SetBGColor(new Color(0.5f, 1, 0.5f));
                offset += 1.05f;

                bool noneSelected = tg.NoneSelected();

                foreach (string s in tg.traits.Keys)
                {
                    TraitGroup tmpGroup = tg;
                    string tmpTrait = s;
                    ui = new UIElement(traitScrollArea.GetScrollTransform());
                    ui.SetLocation(0, offset, 11, 1);
                    if (tg.traits[s].selected)
                    {
                        ui.SetBGColor(Color.white);
                        ui.SetButton(delegate { SelectTrait(tmpGroup, tmpTrait); });
                    }
                    else
                    {
                        int itemCount = 0;
                        foreach (SelectionItemTraits item in tg.traits[s].items)
                        {
                            bool display = true;
                            foreach (TraitGroup g in traitData)
                            {
                                display &= g.ActiveItem(item);
                            }
                            if (display) itemCount++;
                        }
                        if (itemCount > 0)
                        {
                            if (noneSelected)
                            {
                                ui.SetBGColor(Color.white);
                            }
                            else
                            {
                                ui.SetBGColor(Color.grey);
                            }
                            ui.SetButton(delegate { SelectTrait(tmpGroup, tmpTrait); });
                        }
                        else
                        {
                            ui.SetBGColor(new Color(0.5f, 0, 0));
                        }
                    }
                    ui.SetText(s, Color.black);

                    // Strikethrough
                    if (tg.traits[s].excluded)
                    {
                        ui = new UIElement(traitScrollArea.GetScrollTransform());
                        ui.SetLocation(0.2f, offset + 0.5f, 10.6f, 0.06f);
                        ui.SetBGColor(Color.black);
                        ui.SetButton(delegate { SelectTrait(tmpGroup, tmpTrait); });
                    }

                    // Exclude
                    ui = new UIElement(traitScrollArea.GetScrollTransform());
                    ui.SetLocation(11, offset, 1, 1);
                    ui.SetBGColor(Color.red);
                    ui.SetTextPadding(0);
                    ui.SetText("X", Color.black);
                    ui.SetButton(delegate { ExcludeTrait(tmpGroup, tmpTrait); });

                    offset += 1.05f;
                }
                offset += 1.05f;
            }
            traitScrollArea.SetScrollSize(offset);
            if (!resetScroll)
            {
                traitScrollArea.SetScrollPosition(scrollPos);
            }

            DrawItemList();

            // Cancel button
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-4.5f), 28, 9, 1);
            ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
            ui.SetText(CommonStringKeys.CANCEL);
            ui.SetButton(delegate { Destroyer.Dialog(); });
            new UIElementBorder(ui);
        }

        protected virtual void DrawItemList()
        {
            UIElementScrollVertical itemScrollArea = new UIElementScrollVertical();
            itemScrollArea.SetLocation(UIScaler.GetHCenter(-3.5f), 2, 21, 25);
            new UIElementBorder(itemScrollArea);

            List<SelectionItemTraits> toDisplay = new List<SelectionItemTraits>(traitItems.Values);
            if (alphaSort)
            {
                toDisplay = new List<SelectionItemTraits>(alphaTraitItems.Values);
            }
            if (reverseSort)
            {
                toDisplay.Reverse();
            }

            float offset = 0;
            foreach (SelectionItemTraits item in toDisplay)
            {
                bool display = true;
                foreach (TraitGroup tg in traitData)
                {
                    display &= tg.ActiveItem(item);
                }

                if (!display) continue;

                offset = DrawItem(item, itemScrollArea.GetScrollTransform(), offset);
            }
            itemScrollArea.SetScrollSize(offset);
        }

        protected virtual float DrawItem(SelectionItemTraits item, Transform transform, float offset)
        {
            string key = item.GetKey();
            UIElement ui = new UIElement(transform);
            ui.SetLocation(0, offset, 20, 1);
            if (key != null)
            {
                ui.SetButton(delegate { SelectItem(key); });
            }
            ui.SetBGColor(item.GetColor());
            ui.SetText(item.GetDisplay(), Color.black);
            return offset + 1.05f;
        }

        protected void SelectTrait(TraitGroup group, string trait)
        {
            if (!group.traits.ContainsKey(trait)) return;

            group.traits[trait].selected = !group.traits[trait].selected;
            group.traits[trait].excluded = false;
            Update();
        }

        protected void ExcludeTrait(TraitGroup group, string trait)
        {
            if (!group.traits.ContainsKey(trait)) return;

            group.traits[trait].excluded = !group.traits[trait].excluded;
            group.traits[trait].selected = false;
            Update();
        }

        public void AddItem(StringKey stringKey, Dictionary<string, IEnumerable<string>> traits)
        {
            AddItem(new SelectionItemTraits(stringKey.Translate(), stringKey.key, traits));
        }

        public void AddItem(StringKey stringKey, Dictionary<string, IEnumerable<string>> traits, Color color)
        {
            AddItem(new SelectionItemTraits(stringKey.Translate(), stringKey.key, traits), color);
        }

        public void AddItem(string item, Dictionary<string, IEnumerable<string>> traits)
        {
            AddItem(new SelectionItemTraits(item, item, traits));
        }

        public void AddItem(string item, Dictionary<string, IEnumerable<string>> traits, Color color)
        {
            AddItem(new SelectionItemTraits(item, item, traits), color);
        }

        public void AddItem(string display, string key, Dictionary<string, IEnumerable<string>> traits)
        {
            AddItem(new SelectionItemTraits(display, key, traits));
        }

        public void AddItem(string display, string key, Dictionary<string, IEnumerable<string>> traits, Color color)
        {
            AddItem(new SelectionItemTraits(display, key, traits), color);
        }

        public void AddItem(QuestData.QuestComponent qc)
        {
            Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();

            traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { new StringKey("val", qc.typeDynamic.ToUpper()).Translate() });
            traits.Add(new StringKey("val", "SOURCE").Translate(), new string[] { qc.source });

            AddItem(new SelectionItemTraits(qc.sectionName, qc.sectionName, traits));
        }

        public void AddItem(GenericData component)
        {
            AddItem(CreateItem(component));
        }

        public void AddItem(GenericData component, Color color)
        {
            AddItem(CreateItem(component), color);
        }

        override public void AddItem(SelectionItem item)
        {
            string key = item.GetDisplay();
            int duplicateIndex = 0;
            while (alphaTraitItems.ContainsKey(key))
            {
                key = item.GetDisplay() + "_" + duplicateIndex++;
            }

            if (item is SelectionItemTraits)
            {
                traitItems.Add(traitItems.Count, item as SelectionItemTraits);
                alphaTraitItems.Add(key, item as SelectionItemTraits);
            }
            else
            {
                traitItems.Add(traitItems.Count, new SelectionItemTraits(item));
                alphaTraitItems.Add(key, new SelectionItemTraits(item));
            }
        }

        protected void AddItem(SelectionItem item, Color color)
        {
            item.SetColor(color);
            AddItem(item);
        }

        protected SelectionItemTraits CreateItem(GenericData component)
        {
            Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();

            List<string> sets = new List<string>();
            foreach (string s in component.sets)
            {
                if (s.Length == 0)
                {
                    sets.Add(new StringKey("val", "base").Translate());
                }
                else
                {
                    sets.Add(new StringKey("val", s).Translate());
                }
            }
            traits.Add(new StringKey("val", "SOURCE").Translate(), sets);

            List<string> traitlocal = new List<string>();
            foreach (string s in component.traits)
            {
                traitlocal.Add(new StringKey("val", s).Translate());
            }
            traits.Add(new StringKey("val", "TRAITS").Translate(), traitlocal);

            return new SelectionItemTraits(component.name.Translate(), component.sectionName, traits);
        }

        public void AddNewComponentItem(string type)
        {
            Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();

            traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { new StringKey("val", type.ToUpper()).Translate() });
            traits.Add(new StringKey("val", "SOURCE").Translate(), new string[] { new StringKey("val", "NEW").Translate() });

            AddItem(new SelectionItemTraits(new StringKey("val", "NEW_X", new StringKey("val", type.ToUpper())).Translate(), "{NEW:" + type + "}", traits));
        }

        public void SelectTrait(string type, string trait)
        {
            foreach (TraitGroup tg in traitData)
            {
                if (tg.GetName().Equals(type))
                {
                    SelectTrait(tg, trait);
                    return;
                }
            }
        }

        public void ExcludeTrait(string type, string trait)
        {
            foreach (TraitGroup tg in traitData)
            {
                if (tg.GetName().Equals(type))
                {
                    ExcludeTrait(tg, trait);
                    return;
                }
            }
        }

        public void InitExcludeTrait(string type, string exclusion)
        {
            if (!initialExclusions.ContainsKey(type))
            {
                initialExclusions.Add(type, new List<string>());
            }
            initialExclusions[type].Add(exclusion);
        }

        public void ExcludeExpansions()
        {
            List<string> enabled = new List<string>();
            enabled.Add(new StringKey("val", "base").Translate());
            foreach (string anyPack in Game.Get().cd.GetLoadedPackIDs())
            {
                bool packRequired = false;
                if (anyPack.Equals("") || anyPack.Equals("base")) packRequired = true;
                foreach (string s in Game.Get().quest.qd.quest.packs)
                {
                    if (packRequired) break;
                    if (anyPack.Equals(s))
                    {
                        packRequired = true;
                    }
                }
                if (!packRequired)
                {
                    InitExcludeTrait(new StringKey("val", "SOURCE").Translate(), new StringKey("val", anyPack).Translate());
                }
            }
        }

        public class SelectionItemTraits : SelectionItem
        {
            Dictionary<string, IEnumerable<string>> _traits = new Dictionary<string, IEnumerable<string>>();

            public SelectionItemTraits(string display, string key) : base(display, key)
            {
            }

            public SelectionItemTraits(string display, string key, Dictionary<string, IEnumerable<string>> traits) : base(display, key)
            {
                _traits = traits;
            }

            public SelectionItemTraits(SelectionItem item) : base(item.GetDisplay(), item.GetKey())
            {
            }

            public Dictionary<string, IEnumerable<string>> GetTraits()
            {
                return _traits;
            }
        }

        protected class TraitGroup
        {
            public Dictionary<string, Trait> traits = new Dictionary<string, Trait>();
            public List<SelectionItem> ungrouped = new List<SelectionItem>();
            public string _name = "";

            public TraitGroup(string name)
            {
                _name = name;
            }

            public string GetName()
            {
                return _name;
            }

            public bool NoneSelected()
            {
                bool anySelected = false;
                foreach (Trait t in traits.Values)
                {
                    anySelected |= t.selected;
                }
                return !anySelected;
            }

            public bool ActiveItem(SelectionItemTraits item)
            {
                foreach (Trait t in traits.Values)
                {
                    if (t.items.Contains(item))
                    {
                        if (t.excluded)
                        {
                            // item contains excluded trait
                            return false;
                        }
                    }
                    else
                    {
                        if (t.selected && !NoneSelected())
                        {
                            // item does not contain selected trait
                            return false;
                        }
                    }
                }
                return true;
            }

            public void AddTraits(SelectionItemTraits item)
            {
                foreach (string trait in item.GetTraits()[_name])
                {
                    if (!traits.ContainsKey(trait))
                    {
                        traits.Add(trait, new Trait());
                    }
                }
            }

            public void AddItem(SelectionItemTraits item)
            {
                if (!item.GetTraits().ContainsKey(_name))
                {
                    ungrouped.Add(item);
                }
                else
                {
                    foreach (string s in item.GetTraits()[_name])
                    {
                        traits[s].items.Add(item);
                    }
                }
            }

            public class Trait
            {
                public bool selected = false;
                public bool excluded = false;
                public List<SelectionItem> items = new List<SelectionItem>();
            }
        }
    }
}
