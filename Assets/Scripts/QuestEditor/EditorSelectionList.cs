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

    // Create editor selection clist with title, list, colour and callback
    public EditorSelectionList(string t, List<SelectionListEntry> list, UnityEngine.Events.UnityAction call)
    {
        items = list;
        title = t;
        returnCall = call;
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
        DialogBox db = new DialogBox(new Vector2(21, 0), new Vector2(20, 26), "");
        db.AddBorder();

        // Title
        db = new DialogBox(new Vector2(21, 0), new Vector2(20, 1), title);

        float offset = 2;
        TextButton tb = null;

        // All items on this page
        for (int i = indexOffset; i < (20 + indexOffset); i++)
        {
            // limit to array length
            if (items.Count > i)
            {
                string key = items[i].name;
                tb = new TextButton(new Vector2(21, offset), new Vector2(20, 1), key, delegate { SelectComponent(key); }, items[i].color);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            }
            offset += 1;
        }
        // Paged
        if (items.Count > 20)
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
        offset += 1;
        tb = new TextButton(new Vector2(26.5f, offset), new Vector2(9, 1), "Cancel", cancelCall);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
    }
    
    // Move to next page and redraw
    public void NextPage()
    {
        indexOffset += 20;
        if (indexOffset > items.Count)
        {
            indexOffset -= 20;
        }
        SelectItem(cancelCall);
    }

    // Move to previous page and redraw
    public void PreviousPage()
    {
        indexOffset -= 20;
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

        public SelectionListEntry(string nameIn, List<string> l, Color c)
        {
            name = nameIn;
            filter = l;
            color = c;
        }
    }
}