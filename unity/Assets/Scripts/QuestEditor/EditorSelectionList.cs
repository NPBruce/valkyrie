﻿using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

// Paged list of items to select from
// Used by quest editor
public class EditorSelectionList
{
    private const string VAL = "val";

    // Selection made
    public string selection = null;
    // List of items to select
    public List<SelectionListEntry> items;
    public StringKey title;
    public UnityEngine.Events.UnityAction returnCall;
    public UnityEngine.Events.UnityAction cancelCall;
    // Page offset
    public int indexOffset = 0;
    // Filters
    public HashSet<string> filter;
    public HashSet<string> traits;
    public int perPage = 20;

    /// <summary>
    /// Create editor selection clist with title, list, colour and callback.
    /// Creates a list of possible traits extracted from items
    /// </summary>
    /// <param name="newTitle">title of the dialog</param>
    /// <param name="list">list of selectable items</param>
    /// <param name="call">callback delegate</param>
    public EditorSelectionList(StringKey newTitle, List<SelectionListEntry> list, UnityEngine.Events.UnityAction call)
    {
        items = list;
        title = newTitle;
        returnCall = call;
        filter = new HashSet<string>();
        traits = new HashSet<string>();
        foreach (SelectionListEntry e in items)
        {
            foreach (string s in e.filter)
            {
                traits.Add(s);
            }
        }
    }

    // Draw list, destroy on cancel
    public void SelectItem()
    {
        SelectItem(delegate { Destroyer.Dialog(); });
    }

    // Destroy list, call on cancel
    public void SelectItem(UnityEngine.Events.UnityAction call)
    {
        Destroyer.Dialog();
        cancelCall = call;

        // Border
        DialogBox db = new DialogBox(new Vector2(21, 0), new Vector2(20, 30), StringKey.NULL);
        db.AddBorder();

        // Title
        db = new DialogBox(new Vector2(21, 0), new Vector2(20, 1), title);

        List<SelectionListEntry> filtered = items;
        if (filter.Count > 0)
        {
            filtered = new List<SelectionListEntry>();
            foreach (SelectionListEntry e in items)
            {
                bool valid = true;
                foreach (string s in filter)
                {
                    if (!e.filter.Contains(s))
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid)
                {
                    filtered.Add(e);
                }
            }
        }

        float offset = 2f;
        TextButton tb = null;

        float hOffset = 22;
        foreach (string trait in traits)
        {
            // Traits are in val dictionary
            db = new DialogBox(Vector2.zero, new Vector2(10, 1), new StringKey(VAL, trait));
            float width = (db.textObj.GetComponent<UnityEngine.UI.Text>().preferredWidth / UIScaler.GetPixelsPerUnit()) + 0.5f;
            db.Destroy();
            if (hOffset + width > 40)
            {
                hOffset = 22;
                offset++;
            }
            string tmp = trait;
            if (filter.Count == 0)
            {
                tb = new TextButton(new Vector2(hOffset, offset), new Vector2(width, 1), 
                    new StringKey(VAL, tmp), delegate { SetFilter(trait); });
            }
            else if (filter.Contains(trait))
            {
                tb = new TextButton(new Vector2(hOffset, offset), new Vector2(width, 1), 
                    new StringKey(VAL, tmp), delegate { ClearFilter(trait); });
            }
            else
            {
                bool valid = false;
                foreach (SelectionListEntry e in filtered)
                {
                    if (e.filter.Contains(tmp))
                    {
                        valid = true;
                        break;
                    }
                }
                if (valid)
                {
                    tb = new TextButton(new Vector2(hOffset, offset), new Vector2(width, 1), 
                        new StringKey(VAL, tmp), delegate { SetFilter(trait); }, Color.gray);
                }
                else
                {
                    tb = new TextButton(new Vector2(hOffset, offset), new Vector2(width, 1), 
                        new StringKey(VAL, tmp), delegate { ; }, new Color(0.5f, 0, 0));
                }
            }
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            hOffset += width;
        }

        if (traits.Count > 0) offset += 2;
        perPage = 27 - Mathf.RoundToInt(offset);

        // All items on this page
        for (int i = indexOffset; i < (perPage + indexOffset); i++)
        {
            // limit to array length
            if (filtered.Count > i)
            {
                // Print the name but select the key
                string key = filtered[i].key;
                tb = new TextButton(new Vector2(21, offset), new Vector2(20, 1), 
                    new StringKey(null, filtered[i].name, false), delegate { SelectComponent(key); }, filtered[i].color);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            }
            offset += 1;
        }
        // Paged
        if (filtered.Count > perPage)
        {
            // Prev button
            offset += 1;
            tb = new TextButton(new Vector2(22f, offset), new Vector2(1, 1), new StringKey(null, "<", false), delegate { PreviousPage(); });
            tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            // Next button
            tb = new TextButton(new Vector2(39f, offset), new Vector2(1, 1), new StringKey(null, ">", false), delegate { NextPage(); });
            tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        }
        // Cancel button
        tb = new TextButton(new Vector2(26.5f, offset), new Vector2(9, 1), CommonStringKeys.CANCEL, cancelCall);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
    }
    
    public void SetFilter(string f)
    {
        filter.Add(f);
        indexOffset = 0;
        SelectItem(cancelCall);
    }

    public void ClearFilter(string f)
    {
        filter.Remove(f);
        indexOffset = 0;
        SelectItem(cancelCall);
    }

    // Move to next page and redraw
    public void NextPage()
    {
        indexOffset += perPage;
        if (indexOffset > items.Count)
        {
            indexOffset -= perPage;
        }
        SelectItem(cancelCall);
    }

    // Move to previous page and redraw
    public void PreviousPage()
    {
        indexOffset -= perPage;
        if (indexOffset < 0)
        {
            indexOffset = 0;
        }
        SelectItem(cancelCall);
    }

    // Component selected
    public void SelectComponent(string s)
    {
        selection = s;
        returnCall();
    }

    /// <summary>
    /// Selection list entry
    /// </summary>
    public class SelectionListEntry
    {
        public Color color = Color.white;
        public string name = "";
        public string key = "";
        public List<string> filter;

        public static SelectionListEntry BuildNameKeyItem(string namekey)
        {
            SelectionListEntry entry = new SelectionListEntry();
            entry.name = new StringKey(VAL,namekey).Translate();
            entry.key = namekey;
            return entry;
        }

        public static SelectionListEntry BuildNameKeyItem(string name, string key)
        {
            SelectionListEntry entry = new SelectionListEntry();
            entry.name = name;
            entry.key = key;
            return entry;
        }

        public static SelectionListEntry BuildNameKeyTraitItem(string name, string key, string trait)
        {
            SelectionListEntry entry = BuildNameKeyItem(name, key);
            entry.filter = new List<string>();
            entry.filter.Add(trait);
            return entry;
        }

        public static SelectionListEntry BuildNameKeyTraitsItem(string name, string key, List<string> traits)
        {
            SelectionListEntry entry = BuildNameKeyItem(name, key);
            entry.filter = traits;
            return entry;
        }

        private SelectionListEntry()
        {
            filter = new List<string>();
        }
        public SelectionListEntry(string nameKeyIn)
        {
            name = nameKeyIn;
            key = name;
            filter = new List<string>();
        }

        public SelectionListEntry(string nameKeyIn, Color c)
        {
            name = nameKeyIn;
            key = name;
            filter = new List<string>();
            color = c;
        }

        public SelectionListEntry(string nameKeyIn, List<string> l)
        {
            name = nameKeyIn;
            key = name;
            filter = l;
        }

        public SelectionListEntry(string nameKeyIn, string l)
        {
            name = nameKeyIn;
            key = name;
            filter = new List<string>();
            filter.Add(l);
        }

        public SelectionListEntry(string nameKeyIn, List<string> l, Color c)
        {
            name = nameKeyIn;
            key = name;
            filter = l;
            color = c;
        }

        public SelectionListEntry(string nameKeyIn, string l, Color c)
        {
            name = nameKeyIn;
            key = name;
            filter = new List<string>();
            filter.Add(l);
            color = c;
        }
    }
}