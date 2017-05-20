using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

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
    // Filters
    public HashSet<string> filter;
    public HashSet<string> traits;

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

        float windowSize= 22;
        if (traits.Count > 10)
        {
            windowSize = 36;
        }

        float windowEdge = UIScaler.GetHCenter(windowSize / -2);

        // Border
        DialogBox db = new DialogBox(new Vector2(windowEdge, 0), new Vector2(windowSize, 30), StringKey.NULL);
        db.AddBorder();

        // Title
        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-10), 0), new Vector2(20, 1), title);

        // Create a list of traits of all items in the list
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

        float hOffset = windowEdge + 1;
        // Create filter traits buttons
        foreach (string trait in traits)
        {
            // Traits are in val dictionary
            db = new DialogBox(Vector2.zero, new Vector2(10, 1), new StringKey(VAL, trait));
            float width = (db.textObj.GetComponent<UnityEngine.UI.Text>().preferredWidth / UIScaler.GetPixelsPerUnit()) + 0.5f;
            db.Destroy();
            if (hOffset + width > windowEdge + windowSize - 1)
            {
                hOffset = windowEdge + 1;
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

        // Scroll BG
        float scrollStart = offset;
        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-10.5f), offset), new Vector2(21, 27 - offset), StringKey.NULL);
        db.AddBorder();
        db.background.AddComponent<UnityEngine.UI.Mask>();
        UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

        GameObject scrollArea = new GameObject("scroll");
        RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
        scrollArea.transform.SetParent(db.background.transform);
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 20 * UIScaler.GetPixelsPerUnit());
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 1);

        GameObject scrollBarObj = new GameObject("scrollbar");
        scrollBarObj.transform.SetParent(db.background.transform);
        RectTransform scrollBarRect = scrollBarObj.AddComponent<RectTransform>();
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (27 - offset) * UIScaler.GetPixelsPerUnit());
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 20 * UIScaler.GetPixelsPerUnit(), 1 * UIScaler.GetPixelsPerUnit());
        UnityEngine.UI.Scrollbar scrollBar = scrollBarObj.AddComponent<UnityEngine.UI.Scrollbar>();
        scrollBar.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
        scrollRect.verticalScrollbar = scrollBar;

        GameObject scrollBarHandle = new GameObject("scrollbarhandle");
        scrollBarHandle.transform.SetParent(scrollBarObj.transform);
        scrollBarHandle.AddComponent<UnityEngine.UI.Image>();
        scrollBarHandle.GetComponent<UnityEngine.UI.Image>().color = new Color(0.7f, 0.7f, 0.7f);
        scrollBar.handleRect = scrollBarHandle.GetComponent<RectTransform>();
        scrollBar.handleRect.offsetMin = Vector2.zero;
        scrollBar.handleRect.offsetMax = Vector2.zero;

        scrollRect.content = scrollInnerRect;
        scrollRect.horizontal = false;
        scrollRect.scrollSensitivity = 27f;

        for (int i = 0; i < filtered.Count; i++)
        {
            // Print the name but select the key
            string key = filtered[i].key;
            UIElement ui = new UIElement(scrollArea.transform);
            ui.SetLocation(0, (i * 1.05f), 20, 1);
            if (key == null)
            {
                ui.SetButton(delegate { SelectComponent(key); });
            }
            ui.SetBGColor(Color.white);
            ui.SetText(filtered[i].name, Color.black);
        }

        float scrollsize = filtered.Count * 1.05f;
        if (scrollsize < 28)
        {
            scrollsize = 28;
        }
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, scrollsize * UIScaler.GetPixelsPerUnit());
        // Cancel button
        tb = new TextButton(new Vector2(UIScaler.GetHCenter(-4.5f), 28f), new Vector2(9, 1), CommonStringKeys.CANCEL, cancelCall);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
    }
    
    public void SetFilter(string f)
    {
        filter.Add(f);
        SelectItem(cancelCall);
    }

    public void ClearFilter(string f)
    {
        filter.Remove(f);
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

        public static SelectionListEntry BuildNameKeyTraitsColorItem(string name, string key, List<string> traits, Color newColor)
        {
            SelectionListEntry entry = BuildNameKeyItem(name, key);
            entry.filter = traits;
            entry.color = newColor;
            return entry;
        }

        public static SelectionListEntry BuildNewComponent(string type)
        {
            SelectionListEntry entry = BuildNameKeyItem(new StringKey("val","NEW_X",type.ToUpper()).Translate(),"{NEW:" + type + "}");
            entry.filter.Add(type);
            entry.filter.Add(new StringKey(VAL,"NEW").Translate());
            return entry;
        }

        public SelectionListEntry()
        {
            filter = new List<string>();
        }

        public SelectionListEntry(QuestData.QuestComponent component, List<string> traits = null)
        {
            name = component.sectionName;
            key = name;
            filter = traits;
            if (filter == null)
            {
                filter = new List<string>();
            }
            Game game = Game.Get();
            filter.Add(component.source);
            filter.Add(new StringKey(VAL,component.typeDynamic.ToUpper()).Translate());
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