using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Paged list of items to select from
// Used by quest editor
public class EditorSelectionList
{
    // Selection made
    public string selection = "";
    // List of items to select
    public List<SelectionListEntry> items;
    public string title;
    public UnityEngine.Events.UnityAction returnCall;
    public UnityEngine.Events.UnityAction cancelCall;
    // Page offset
    public int indexOffset = 0;
    // Filters
    public HashSet<string> filter;
    public HashSet<string> traits;
    public int perPage = 20;

    // Create editor selection clist with title, list, colour and callback
    public EditorSelectionList(string t, List<SelectionListEntry> list, UnityEngine.Events.UnityAction call)
    {
        items = list;
        title = t;
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
        DialogBox db = new DialogBox(new Vector2(21, 0), new Vector2(20, 30), "");
        db.AddBorder();

        // Title
        db = new DialogBox(new Vector2(21, 0), new Vector2(20, 1), title);

        float offset = 2f;
        TextButton tb = null;

        float hOffset = 22;
        foreach (string s in traits)
        {
            db = new DialogBox(Vector2.zero, new Vector2(10, 1), s);
            float width = (db.textObj.GetComponent<UnityEngine.UI.Text>().preferredWidth / UIScaler.GetPixelsPerUnit()) + 0.5f;
            db.Destroy();
            if (hOffset + width > 40)
            {
                hOffset = 22;
                offset++;
            }
            string tmp = s;
            if (filter.Count == 0)
            {
                tb = new TextButton(new Vector2(hOffset, offset), new Vector2(width, 1), tmp, delegate { SetFilter(s); });
            }
            else if (filter.Contains(s))
            {
                tb = new TextButton(new Vector2(hOffset, offset), new Vector2(width, 1), tmp, delegate { ClearFilter(s); });
            }
            else
            {
                tb = new TextButton(new Vector2(hOffset, offset), new Vector2(width, 1), tmp, delegate { SetFilter(s); }, Color.gray);
            }
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            hOffset += width;
        }

        if (traits.Count > 0) offset += 2;

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
                    }
                }
                if (valid)
                {
                    filtered.Add(e);
                }
            }
        }

        perPage = 27 - Mathf.RoundToInt(offset);

        // All items on this page
        for (int i = indexOffset; i < (perPage + indexOffset); i++)
        {
            // limit to array length
            if (filtered.Count > i)
            {
                string key = filtered[i].name;
                tb = new TextButton(new Vector2(21, offset), new Vector2(20, 1), key, delegate { SelectComponent(key); }, filtered[i].color);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            }
            offset += 1;
        }
        // Paged
        if (filtered.Count > perPage)
        {
            // Prev button
            offset += 1;
            tb = new TextButton(new Vector2(22f, offset), new Vector2(1, 1), "<", delegate { PreviousPage(); });
            tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            // Next button
            tb = new TextButton(new Vector2(39f, offset), new Vector2(1, 1), ">", delegate { NextPage(); });
            tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        }
        // Cancel button
        tb = new TextButton(new Vector2(26.5f, offset), new Vector2(9, 1), "Cancel", cancelCall);
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

    public class SelectionListEntry
    {
        public Color color = Color.white;
        public string name = "";
        public List<string> filter;

        public SelectionListEntry(string nameIn)
        {
            name = nameIn;
            filter = new List<string>();
        }

        public SelectionListEntry(string nameIn, Color c)
        {
            name = nameIn;
            filter = new List<string>();
            color = c;
        }

        public SelectionListEntry(string nameIn, List<string> l)
        {
            name = nameIn;
            filter = l;
        }

        public SelectionListEntry(string nameIn, string l)
        {
            name = nameIn;
            filter = new List<string>();
            filter.Add(l);
        }

        public SelectionListEntry(string nameIn, List<string> l, Color c)
        {
            name = nameIn;
            filter = l;
            color = c;
        }

        public SelectionListEntry(string nameIn, string l, Color c)
        {
            name = nameIn;
            filter = new List<string>();
            filter.Add(l);
            color = c;
        }
    }
}