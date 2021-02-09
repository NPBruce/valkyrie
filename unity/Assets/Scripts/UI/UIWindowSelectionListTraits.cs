using System;
using UnityEngine;
using Assets.Scripts.Content;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.UI
{
    public class UIWindowSelectionListTraits : UIWindowSelectionList
    {
        protected List<TraitGroup> traitGroups = new List<TraitGroup>();

        protected List<SelectionItemTraits> allItems = new List<SelectionItemTraits>();
        protected SortedList<string, SelectionItemTraits> alwaysOnTopTraitItems = new SortedList<string, SelectionItemTraits>();
        protected SortedList<int, SelectionItemTraits> traitItems = new SortedList<int, SelectionItemTraits>();
        protected SortedList<string, SelectionItemTraits> alphaTraitItems = new SortedList<string, SelectionItemTraits>();

        protected Dictionary<string, List<string>> initialExclusions = new Dictionary<string, List<string>>();

        protected float scrollPos = 0;

        protected UIElementScrollVertical traitScrollArea;

        string val_base_translated = null;
        string val_source_translated = null;
        string val_traits_translated = null;
        string val_type_translated = null;

        private static readonly Color DISABLED_TRAIT_COLOR = new Color(0.5f, 0, 0);
        private static readonly Color SELECTED_TRAIT_COLOR = Color.white;
        private static readonly Color NOT_SELECTED_TRAIT_COLOR = Color.grey;

        public UIWindowSelectionListTraits(UnityEngine.Events.UnityAction<string> call, string title = "", bool callAfterCancel = false) : base(call, title, callAfterCancel)
        {
            val_base_translated = CommonStringKeys.BASE.Translate();
            val_source_translated = CommonStringKeys.SOURCE.Translate();
            val_traits_translated = CommonStringKeys.TRAITS.Translate();
            val_type_translated = CommonStringKeys.TYPE.Translate();
        }

        public UIWindowSelectionListTraits(UnityEngine.Events.UnityAction<string> call, StringKey title, bool callAfterCancel = false) : base(call, title, callAfterCancel)
        {
            val_base_translated = CommonStringKeys.BASE.Translate();
            val_source_translated = CommonStringKeys.SOURCE.Translate();
            val_traits_translated = CommonStringKeys.TRAITS.Translate();
            val_type_translated = CommonStringKeys.TYPE.Translate();
        }


        override public void Draw()
        {
            foreach (SelectionItemTraits item in allItems)
            {
                foreach (string category in item.GetTraits().Keys)
                {
                    bool found = false;
                    foreach (TraitGroup tg in traitGroups)
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
                        traitGroups.Add(tg);
                    }
                }
            }

            foreach (SelectionItemTraits item in allItems)
            {
                foreach (TraitGroup tg in traitGroups)
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
            foreach (TraitGroup tg in traitGroups)
            {
                ui = new UIElement(traitScrollArea.GetScrollTransform());
                ui.SetLocation(0, offset, 12, 1);
                ui.SetText(tg.GetName(), Color.black);
                ui.SetTextAlignment(TextAnchor.MiddleLeft);
                ui.SetBGColor(new Color(0.5f, 1, 0.5f));
                offset += 1.05f;

                foreach (string s in tg.traits.Keys)
                {
                    TraitGroup tmpGroup = tg;
                    string tmpTrait = s;
                    ui = new UIElement(traitScrollArea.GetScrollTransform());
                    ui.SetLocation(0, offset, 11, 1);
                    
                    var isExcluded = tg.traits[s].excluded;
                    var isSelected = tg.traits[s].selected;
                    if (isSelected)
                    {
                        ui.SetBGColor(SELECTED_TRAIT_COLOR);
                        ui.SetButton(delegate { ToggleTraitSelectState(tmpGroup, tmpTrait); });
                    }
                    else if (isExcluded)
                    {
                        ui.SetBGColor(DISABLED_TRAIT_COLOR);
                    }
                    else
                    {
                        int itemCount = 0;
                        foreach (SelectionItemTraits item in tg.traits[s].items)
                        {
                            if (traitGroups.All(g => g.ActiveItem(item)))
                            {
                                itemCount++;
                            }
                        }

                        if (itemCount <= 0)
                        {
                            ui.SetBGColor(DISABLED_TRAIT_COLOR);
                        }
                        else
                        {
                            ui.SetButton(delegate { ToggleTraitSelectState(tmpGroup, tmpTrait); });
                            if (tg.NoneSelected())
                            {
                                ui.SetBGColor(SELECTED_TRAIT_COLOR);
                            }
                            else
                            {
                                ui.SetBGColor(NOT_SELECTED_TRAIT_COLOR);
                            }
                        }
                    }
                    ui.SetText(s, Color.black);

                    // Strikethrough
                    if (isExcluded)
                    {
                        ui = new UIElement(traitScrollArea.GetScrollTransform());
                        ui.SetLocation(0.2f, offset + 0.5f, 10.6f, 0.06f);
                        ui.SetBGColor(Color.black);
                        ui.SetButton(delegate { ToggleTraitSelectState(tmpGroup, tmpTrait); });
                    }

                    // Exclude
                    ui = new UIElement(traitScrollArea.GetScrollTransform());
                    ui.SetLocation(11, offset, 1, 1);
                    ui.SetBGColor(Color.red);
                    ui.SetTextPadding(0);
                    ui.SetText("X", Color.black);
                    ui.SetButton(delegate { ExcludeTrait(tmpGroup, tmpTrait, true); });

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
            ui.SetButton(delegate
            {
                Destroyer.Dialog();
                if (callAfterCancel)
                {
                    _call(null);
                }
            });
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
            
            toDisplay.InsertRange(0, alwaysOnTopTraitItems.Values);

            float offset = 0;
            foreach (SelectionItemTraits item in toDisplay)
            {
                bool display = true;
                foreach (TraitGroup tg in traitGroups)
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
            if (alphaSort) ui.SetTextAlignment(TextAnchor.MiddleLeft);
            return offset + 1.05f;
        }

        protected void ToggleTraitSelectState(TraitGroup group, string trait)
        {
            if (!group.traits.ContainsKey(trait)) return;

            group.traits[trait].selected = !group.traits[trait].selected;
            group.traits[trait].excluded = false;
            Update();
        }

        protected void ExcludeTrait(TraitGroup group, string trait, bool update)
        {
            if (!group.traits.ContainsKey(trait)) return;

            group.traits[trait].excluded = !group.traits[trait].excluded;
            group.traits[trait].selected = false;
            if(update)
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

        public void AddItem(string display, string key, Dictionary<string, IEnumerable<string>> traits, bool alwaysOnTop = false)
        {
            AddItem(new SelectionItemTraits(display, key, traits, alwaysOnTop));
        }

        public void AddItem(string display, string key, Dictionary<string, IEnumerable<string>> traits, Color color)
        {
            AddItem(new SelectionItemTraits(display, key, traits), color);
        }

        public void AddItem(QuestData.QuestComponent qc)
        {
            Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();

            traits.Add(val_type_translated, new string[] { new StringKey("val", qc.typeDynamic.ToUpper()).Translate() });
            traits.Add(val_source_translated, new string[] { qc.source });

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
            if (item is SelectionItemTraits traitItem)
            {
                AddTraitItem(traitItem);
            }
            else
            {
                AddTraitItem(new SelectionItemTraits(item));
            }
        }

        private void AddTraitItem(SelectionItemTraits traitItem)
        {
            allItems.Add(traitItem);
            if (traitItem.AlwaysOnTop)
            {
                alwaysOnTopTraitItems.Add(traitItem.GetDisplay(), traitItem);
                return;
            }
            
            string key = traitItem.GetDisplay();
            int duplicateIndex = 0;
            while (alphaTraitItems.ContainsKey(key))
            {
                key = traitItem.GetDisplay() + "_" + duplicateIndex++;
            }

            traitItems.Add(traitItems.Count, traitItem);
            alphaTraitItems.Add(key, traitItem);
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
                    sets.Add(val_base_translated);
                }
                else
                {
                    sets.Add(new StringKey("val", s).Translate());
                }
            }
            traits.Add(val_source_translated, sets);

            List<string> traitlocal = new List<string>();
            foreach (string s in component.traits)
            {
                traitlocal.Add(new StringKey("val", s).Translate());
            }
            traits.Add(val_traits_translated, traitlocal);

            return new SelectionItemTraits(component.name.Translate(), component.sectionName, traits);
        }

        public void AddNewComponentItem(string type)
        {
            Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();

            traits.Add(val_type_translated, new string[] { new StringKey("val", type.ToUpper()).Translate() });
            traits.Add(val_source_translated, new string[] { new StringKey("val", "NEW").Translate() });

            AddItem(new SelectionItemTraits(new StringKey("val", "NEW_X", new StringKey("val", type.ToUpper())).Translate(), "{NEW:" + type + "}", traits, true));
        }

        public void SelectTrait(string type, string trait)
        {
            foreach (TraitGroup tg in traitGroups)
            {
                if (tg.GetName().Equals(type))
                {
                    ToggleTraitSelectState(tg, trait);
                    return;
                }
            }
        }

        public void ExcludeTrait(string type, string trait)
        {
            foreach (TraitGroup tg in traitGroups)
            {
                if (tg.GetName().Equals(type))
                {
                    ExcludeTrait(tg, trait, false);
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
            enabled.Add(val_base_translated);
            foreach (string anyPack in Game.Get().cd.GetLoadedPackIDs())
            {
                bool packRequired = string.Empty == anyPack 
                    || anyPack.Equals("base", StringComparison.InvariantCultureIgnoreCase)
                    || Game.Get().quest.qd.quest.packs.Any(s => anyPack.Equals(s));
                
                if (!packRequired)
                {
                    InitExcludeTrait(val_source_translated, new StringKey("val", anyPack).Translate());
                }
            }
        }

        public class SelectionItemTraits : SelectionItem
        {
            Dictionary<string, IEnumerable<string>> _traits = new Dictionary<string, IEnumerable<string>>();

            public SelectionItemTraits(string display, string key) : base(display, key)
            {
            }

            public SelectionItemTraits(string display, string key, Dictionary<string, IEnumerable<string>> traits, bool alwaysOnTop = false) : base(display, key, alwaysOnTop)
            {
                _traits = traits;
            }

            public SelectionItemTraits(SelectionItem item) : base(item.GetDisplay(), item.GetKey(), item.AlwaysOnTop)
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
            protected TraitGroupFilterMode _filterMode = TraitGroupFilterMode.Strict;

            public TraitGroup(string name)
            {
                _name = name;
                if ("Source".Trim().Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    _filterMode = TraitGroupFilterMode.AtLeastOneSelected;
                }
            }

            public string GetName()
            {
                return _name;
            }

            public bool NoneSelected()
            {
                return traits.Values.All(t => !t.selected);
            }

            public bool ActiveItem(SelectionItemTraits item)
            {
                var itemTraits = traits
                    .Where(t => t.Value.items.Contains(item)).ToList();
                var itemTraitKeys = itemTraits.Select(t => t.Key).ToSet();
                
                bool hasAllSelectedTraits = traits
                    .Where(st => st.Value.selected)
                    .Select(st => st.Key)
                    .All(selectedTrait => itemTraitKeys.Contains(selectedTrait));

                if (!hasAllSelectedTraits)
                {
                    return false;
                }

                switch (_filterMode)
                {
                    case TraitGroupFilterMode.AtLeastOneSelected:
                    {
                        var noneSelected = NoneSelected();
                        bool atLeastOneNotExcludedSelected = itemTraits
                            .Where(t => !t.Value.excluded)
                            .Any(t => noneSelected || t.Value.selected);
                        return atLeastOneNotExcludedSelected;
                    }
                    case TraitGroupFilterMode.Strict:
                    default:
                    {
                        bool noneExcluded = !itemTraits.Any(t => t.Value.excluded);
                        return noneExcluded;
                    }
                }
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
            
            protected enum TraitGroupFilterMode
            {
                // Old default. All traits must be selected and none of the traits need to be excluded.
                Strict,
                // All traits must be selected. Exclusion only works if no traits are selected.
                AtLeastOneSelected,
            }
        }
    }
}
