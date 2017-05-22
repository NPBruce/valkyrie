using Assets.Scripts.Content;
using UnityEngine;
using System.Collections.Generic;

// Next stage button is used by MoM to move between investigators and monsters
public class InventoryWindow
{
    public Dictionary<string, DialogBoxEditable> valueDBE;

    // Construct and display
    public InventoryWindow()
    {
        Update();
    }

    public void Update()
    {
        Destroyer.Dialog();
        Game game = Game.Get();

        DialogBox db = new DialogBox(
            new Vector2(UIScaler.GetHCenter(-18), 1),
            new Vector2(36, 23),
            StringKey.NULL);
        db.AddBorder();

        // Add a title to the page
        db = new DialogBox(
            new Vector2(UIScaler.GetHCenter(-6), 1),
            new Vector2(12, 3),
            new StringKey("val", "ITEMS"));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
        db.SetFont(game.gameType.GetHeaderFont());

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-17), 5), new Vector2(34, 13), StringKey.NULL);
        db.AddBorder();
        db.background.AddComponent<UnityEngine.UI.Mask>();
        UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

        GameObject scrollArea = new GameObject("scroll");
        RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
        scrollArea.transform.SetParent(db.background.transform);
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (12f) * UIScaler.GetPixelsPerUnit());
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 1);

        GameObject scrollBarObj = new GameObject("scrollbar");
        scrollBarObj.transform.SetParent(db.background.transform);
        RectTransform scrollBarRect = scrollBarObj.AddComponent<RectTransform>();
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (12f) * UIScaler.GetPixelsPerUnit(), 1 * UIScaler.GetPixelsPerUnit());
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 34 * UIScaler.GetPixelsPerUnit());
        UnityEngine.UI.Scrollbar scrollBar = scrollBarObj.AddComponent<UnityEngine.UI.Scrollbar>();
        scrollBar.direction = UnityEngine.UI.Scrollbar.Direction.LeftToRight;
        scrollRect.horizontalScrollbar = scrollBar;

        GameObject scrollBarHandle = new GameObject("scrollbarhandle");
        scrollBarHandle.transform.SetParent(scrollBarObj.transform);
        scrollBarHandle.AddComponent<UnityEngine.UI.Image>();
        scrollBarHandle.GetComponent<UnityEngine.UI.Image>().color = new Color(0.7f, 0.7f, 0.7f);
        scrollBar.handleRect = scrollBarHandle.GetComponent<RectTransform>();
        scrollBar.handleRect.offsetMin = Vector2.zero;
        scrollBar.handleRect.offsetMax = Vector2.zero;

        scrollRect.content = scrollInnerRect;
        scrollRect.vertical = false;
        scrollRect.scrollSensitivity = 27f;

        float xOffset = UIScaler.GetHCenter(-16);

        foreach (string s in game.quest.items)
        {
            db = new DialogBox(new Vector2(xOffset, 14),
                new Vector2(8, 2),
                game.cd.items[s].name,
                Color.black);
            db.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            db.background.transform.SetParent(scrollArea.transform);
            db.textObj.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");

            Texture2D itemTex = ContentData.FileToTexture(game.cd.items[s].image);
            Sprite itemSprite = Sprite.Create(itemTex, new Rect(0, 0, itemTex.width, itemTex.height), Vector2.zero, 1);

            db = new DialogBox(new Vector2(xOffset, 6),
                new Vector2(8, 8),
                StringKey.NULL,
                Color.clear,
                Color.white);
            db.background.GetComponent<UnityEngine.UI.Image>().sprite = itemSprite;
            db.background.transform.SetParent(scrollArea.transform);

            xOffset += 9;
        }

        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, (xOffset - UIScaler.GetHCenter(-16)) * UIScaler.GetPixelsPerUnit());

        db = new DialogBox(
            new Vector2(UIScaler.GetHCenter(-5), 19),
            new Vector2(10, 4),
            StringKey.NULL);
        db.AddBorder();

        db = new DialogBox(
            new Vector2(UIScaler.GetHCenter(-4), 20),
            new Vector2(5, 2),
            new StringKey("val", "GOLD"));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();

        db = new DialogBox(
            new Vector2(UIScaler.GetHCenter(1), 20),
            new Vector2(3, 2),
            Mathf.RoundToInt(game.quest.vars.GetValue("$%gold")));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();

        TextButton tb = new TextButton(
            new Vector2(UIScaler.GetHCenter(-4f), 24.5f),
            new Vector2(8, 2),
            CommonStringKeys.CLOSE,
            delegate { Destroyer.Dialog(); });
    }
}
