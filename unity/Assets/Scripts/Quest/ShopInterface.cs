using Assets.Scripts.Content;
using UnityEngine;
using System.Collections.Generic;


// Tokens are events that are tied to a token placed on the board
public class ShopInterface : Quest.BoardComponent
{
    GameObject panel;
    Game game;
    List<string> items;

    // Construct with quest info and reference to Game
    public ShopInterface(List<string> i, Game gameObject) : base(gameObject)
    {
        items = i;
        // Find quest UI panel
        panel = GameObject.Find("QuestUIPanel");
        if (panel == null)
        {
            // Create UI Panel
            panel = new GameObject("QuestUIPanel");
            panel.tag = "board";
            panel.transform.parent = game.uICanvas.transform;
            panel.transform.SetAsFirstSibling();
            panel.AddComponent<RectTransform>();
            panel.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, Screen.height);
            panel.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, Screen.width);
        }
        game = Game.Get();
        Update();
    }

    public override QuestData.Event GetEvent()
    {
        return null;
    }

    // Clean up
    public override void Remove()
    {
        // Remove custom tag
    }

    public void Update()
    {
        Remove();
        DrawShopItems();
        DrawPartyItems();
        DrawGold();
    }

    public void DrawShopItems()
    {
        DialogBox db = new DialogBox(new Vector2(UIScaler.GetHCenter(-17), 5), new Vector2(34, 13), StringKey.NULL);
        db.AddBorder();
        db.background.AddComponent<UnityEngine.UI.Mask>();
        db.ApplyTag("heroselect");
        UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

        GameObject scrollArea = new GameObject("scroll");
        RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
        scrollArea.transform.parent = db.background.transform;
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (12f) * UIScaler.GetPixelsPerUnit());
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 1);

        GameObject scrollBarObj = new GameObject("scrollbar");
        scrollBarObj.transform.parent = db.background.transform;
        RectTransform scrollBarRect = scrollBarObj.AddComponent<RectTransform>();
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (12f) * UIScaler.GetPixelsPerUnit(), 1 * UIScaler.GetPixelsPerUnit());
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 34 * UIScaler.GetPixelsPerUnit());
        UnityEngine.UI.Scrollbar scrollBar = scrollBarObj.AddComponent<UnityEngine.UI.Scrollbar>();
        scrollBar.direction = UnityEngine.UI.Scrollbar.Direction.LeftToRight;
        scrollRect.horizontalScrollbar = scrollBar;

        GameObject scrollBarHandle = new GameObject("scrollbarhandle");
        scrollBarHandle.transform.parent = scrollBarObj.transform;
        scrollBarHandle.AddComponent<UnityEngine.UI.Image>();
        scrollBarHandle.GetComponent<UnityEngine.UI.Image>().color = new Color(0.7f, 0.7f, 0.7f);
        scrollBar.handleRect = scrollBarHandle.GetComponent<RectTransform>();
        scrollBar.handleRect.offsetMin = Vector2.zero;
        scrollBar.handleRect.offsetMax = Vector2.zero;

        scrollRect.content = scrollInnerRect;
        scrollRect.vertical = false;
        scrollRect.scrollSensitivity = 27f;

        float xOffset = UIScaler.GetHCenter(-16);

        TextButton tb = null;
        foreach (string s in items)
        {
            tb = new TextButton(new Vector2(8, 2),
                new Vector2(xOffset, 14),
                game.cd.items[s].name,
                delegate {; },
                Color.clear);
            tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
            tb.background.transform.parent = scrollArea.transform;
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");

            Texture2D itemTex = ContentData.FileToTexture(game.cd.items[s].image);
            Sprite itemSprite = Sprite.Create(itemTex, new Rect(0, 0, itemTex.width, itemTex.height), Vector2.zero, 1);

            tb = new TextButton(new Vector2(8, 8),
                new Vector2(xOffset, 6),
                StringKey.NULL,
                delegate {; },
                Color.clear);
            tb.background.GetComponent<UnityEngine.UI.Image>().sprite = itemSprite;
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            tb.background.transform.parent = scrollArea.transform;

            xOffset += 9;
        }

        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, (xOffset - UIScaler.GetHCenter(-16)) * UIScaler.GetPixelsPerUnit());
    }


    public void DrawPartyItems()
    {
        DialogBox db = new DialogBox(new Vector2(UIScaler.GetHCenter(-17), 5), new Vector2(34, 13), StringKey.NULL);
        db.AddBorder();
        db.background.AddComponent<UnityEngine.UI.Mask>();
        db.ApplyTag("heroselect");
        UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

        GameObject scrollArea = new GameObject("scroll");
        RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
        scrollArea.transform.parent = db.background.transform;
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (12f) * UIScaler.GetPixelsPerUnit());
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 1);

        GameObject scrollBarObj = new GameObject("scrollbar");
        scrollBarObj.transform.parent = db.background.transform;
        RectTransform scrollBarRect = scrollBarObj.AddComponent<RectTransform>();
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (12f) * UIScaler.GetPixelsPerUnit(), 1 * UIScaler.GetPixelsPerUnit());
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 34 * UIScaler.GetPixelsPerUnit());
        UnityEngine.UI.Scrollbar scrollBar = scrollBarObj.AddComponent<UnityEngine.UI.Scrollbar>();
        scrollBar.direction = UnityEngine.UI.Scrollbar.Direction.LeftToRight;
        scrollRect.horizontalScrollbar = scrollBar;

        GameObject scrollBarHandle = new GameObject("scrollbarhandle");
        scrollBarHandle.transform.parent = scrollBarObj.transform;
        scrollBarHandle.AddComponent<UnityEngine.UI.Image>();
        scrollBarHandle.GetComponent<UnityEngine.UI.Image>().color = new Color(0.7f, 0.7f, 0.7f);
        scrollBar.handleRect = scrollBarHandle.GetComponent<RectTransform>();
        scrollBar.handleRect.offsetMin = Vector2.zero;
        scrollBar.handleRect.offsetMax = Vector2.zero;

        scrollRect.content = scrollInnerRect;
        scrollRect.vertical = false;
        scrollRect.scrollSensitivity = 27f;

        float xOffset = UIScaler.GetHCenter(-16);

        TextButton tb = null;
        foreach (string s in game.quest.items)
        {
            tb = new TextButton(new Vector2(8, 2),
                new Vector2(xOffset, 14),
                game.cd.items[s].name,
                delegate {; },
                Color.clear);
            tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
            tb.background.transform.parent = scrollArea.transform;
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");

            Texture2D itemTex = ContentData.FileToTexture(game.cd.items[s].image);
            Sprite itemSprite = Sprite.Create(itemTex, new Rect(0, 0, itemTex.width, itemTex.height), Vector2.zero, 1);

            tb = new TextButton(new Vector2(8, 8),
                new Vector2(xOffset, 6),
                StringKey.NULL,
                delegate {; },
                Color.clear);
            tb.background.GetComponent<UnityEngine.UI.Image>().sprite = itemSprite;
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            tb.background.transform.parent = scrollArea.transform;

            xOffset += 9;
        }

        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, (xOffset - UIScaler.GetHCenter(-16)) * UIScaler.GetPixelsPerUnit());
    }

    public void DrawGold()
    {
        DialogBox db = new DialogBox(
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
    }
}
