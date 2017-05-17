using UnityEngine;
using Assets.Scripts.Content;

public class ReorderComponents
{
    EditorSelectionList sourceESL;
    string source = "";

    public ReorderComponents()
    {
        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;
        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();

        foreach (string s in Directory.GetFiles(relativePath, "*.ini", SearchOption.AllDirectories))
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s.Substring(relativePath.Length + 1)));
        }

        sourceESL = new EditorSelectionList(new StringKey("val", "SELECT", new StringKey("val", "FILE")), list, delegate { ReorderSource(); });
        sourceESL.SelectItem();
    }

    public void ReorderSource()
    {
        source = Path.Combine(Path.GetDirectoryName(game.quest.qd.questPath), sourceESL.selection);
        Update();
    }

    public void Update()
    {
        Game game = Game.Get();
        Destroyer.Dialog();

        // Border
        DialogBox db = new DialogBox(new Vector2(UIScaler.GetHCenter(-12.5f), 0), new Vector2(25, 30), StringKey.NULL);
        db.AddBorder();

        // Title
        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-12), 0), new Vector2(24, 1), new StringKey(null, source, false));

        float offset = 2f;
        TextButton tb = null;

        // Scroll BG
        float scrollStart = offset;
        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-11.5f), offset), new Vector2(23, 27 - offset), StringKey.NULL);
        db.AddBorder();
        db.background.AddComponent<UnityEngine.UI.Mask>();
        UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

        GameObject scrollArea = new GameObject("scroll");
        RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
        scrollArea.transform.parent = db.background.transform;
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 22 * UIScaler.GetPixelsPerUnit());
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 1);

        GameObject scrollBarObj = new GameObject("scrollbar");
        scrollBarObj.transform.parent = db.background.transform;
        RectTransform scrollBarRect = scrollBarObj.AddComponent<RectTransform>();
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (28 - offset) * UIScaler.GetPixelsPerUnit());
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 22 * UIScaler.GetPixelsPerUnit(), 1 * UIScaler.GetPixelsPerUnit());
        UnityEngine.UI.Scrollbar scrollBar = scrollBarObj.AddComponent<UnityEngine.UI.Scrollbar>();
        scrollBar.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
        scrollRect.verticalScrollbar = scrollBar;

        GameObject scrollBarHandle = new GameObject("scrollbarhandle");
        scrollBarHandle.transform.parent = scrollBarObj.transform;
        scrollBarHandle.AddComponent<UnityEngine.UI.Image>();
        scrollBarHandle.GetComponent<UnityEngine.UI.Image>().color = new Color(0.7f, 0.7f, 0.7f);
        scrollBar.handleRect = scrollBarHandle.GetComponent<RectTransform>();
        scrollBar.handleRect.offsetMin = Vector2.zero;
        scrollBar.handleRect.offsetMax = Vector2.zero;

        scrollRect.content = scrollInnerRect;
        scrollRect.horizontal = false;
        scrollRect.scrollSensitivity = 27f;

        bool first = true;
        foreach (QuestData.QuestComponent c in game.quest.qd.components)
        {
            if (!c.source.Equals(source)) continue;

            string name = c.sectionName;

            if (!first)
            {
                tb = new TextButton(new Vector2(UIScaler.GetHCenter(9.5f), offset++), new Vector2(1, 1), 
                    new StringKey(null, "▽", false), delegate { DecComponent(name); }, Color.black);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.green;
                tb.background.transform.parent = scrollArea.transform;

                tb = new TextButton(new Vector2(UIScaler.GetHCenter(-11.5f), offset), new Vector2(1, 1), 
                    new StringKey(null, "△", false), delegate { DecComponent(name); }, Color.black);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.green;
                tb.background.transform.parent = scrollArea.transform;
            }
            db = new DialogBox(new Vector2(UIScaler.GetHCenter(-10.5f), 0), new Vector2(20, 1), new StringKey(null, source, false));
            tb = new TextButton(new Vector2(UIScaler.GetHCenter(-10.5f), offset), new Vector2(20, 1), 
                new StringKey(null, filtered[i].name, false), delegate { SelectComponent(key); }, Color.black);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
            tb.background.GetComponent<UnityEngine.UI.Image>().color = filtered[i].color;
            tb.background.transform.parent = scrollArea.transform;
            first = false;
        }
        offset++;
        if (offset < 28) offset = 28;

        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (offset - scrollStart) * UIScaler.GetPixelsPerUnit());
        // Cancel button
        tb = new TextButton(new Vector2(UIScaler.GetHCenter(-4.5f), 28f), new Vector2(9, 1), CommonStringKeys.CANCEL, Destroyer.Dialog());
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
    }

    public void DecComponent(string name)
    {
        bool held = false;
        Dictionary<string, QuestData.QuestComponent> newDict = new Dictionary<string, QuestData.QuestComponent>();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (!kv.Key.Equals(name))
            {
                newDict.Add(kv.Key, kv.Value);
                if (held && kv.Value.source.Equals(source))
                {
                    held = false;
                    newDict.Add(name, game.quest.qd.components[name]);
                }
            }
            else
            {
                held = true;
            }
        }
        if (held)
        {
            newDict.Add(name, game.quest.qd.components[name]);
        }
        Update();
    }
}