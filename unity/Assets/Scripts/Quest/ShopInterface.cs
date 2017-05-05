using Assets.Scripts.Content;
using UnityEngine;
using System.Collections.Generic;


// Tokens are events that are tied to a token placed on the board
public class ShopInterface : Quest.BoardComponent
{
    GameObject panel;
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
            panel.tag = Game.BOARD;
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
        DialogBox db = new DialogBox(new Vector2(UIScaler.GetHCenter(-5f), 0.5f), new Vector2(10, 2), new StringKey("val", "BUY"));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.SetFont(game.gameType.GetHeaderFont());
        db.AddBorder();

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-5), 2.5f), new Vector2(10, 24.5f), StringKey.NULL);
        db.AddBorder();
        db.background.AddComponent<UnityEngine.UI.Mask>();
        UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

        GameObject scrollArea = new GameObject("scroll");
        RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
        scrollArea.transform.parent = db.background.transform;
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, (9) * UIScaler.GetPixelsPerUnit());
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 1);

        GameObject scrollBarObj = new GameObject("scrollbar");
        scrollBarObj.transform.parent = db.background.transform;
        RectTransform scrollBarRect = scrollBarObj.AddComponent<RectTransform>();
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 1 * UIScaler.GetPixelsPerUnit());
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 24.5f * UIScaler.GetPixelsPerUnit());
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

        float vOffset = 3;

        TextButton tb = null;
        foreach (string s in items)
        {
            tb = new TextButton(new Vector2(UIScaler.GetHCenter(-4.5f), vOffset + 4.5f),
                new Vector2(8, 2),
                game.cd.items[s].name,
                delegate {; },
                Color.clear);
            tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            tb.background.transform.parent = scrollArea.transform;
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");

            Texture2D itemTex = ContentData.FileToTexture(game.cd.items[s].image);
            Sprite itemSprite = Sprite.Create(itemTex, new Rect(0, 0, itemTex.width, itemTex.height), Vector2.zero, 1);

            tb = new TextButton(new Vector2(UIScaler.GetHCenter(-2.5f), vOffset + 0.5f),
                new Vector2(4, 4),
                StringKey.NULL,
                delegate {; },
                Color.clear);
            tb.background.GetComponent<UnityEngine.UI.Image>().sprite = itemSprite;
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            tb.background.transform.parent = scrollArea.transform;

            StringKey act = new StringKey("val", "CLASS");
            if (game.cd.items[s].act == 1)
            {
                act = new StringKey("val", "ACT_1");
            }
            if (game.cd.items[s].act == 2)
            {
                act = new StringKey("val", "ACT_2");
            }
            tb = new TextButton(new Vector2(UIScaler.GetHCenter(-2f), vOffset + 4f),
                new Vector2(3, 1),
                act,
                delegate {; },
                Color.black);
            tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.yellow;
            tb.background.transform.parent = scrollArea.transform;
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");

            tb = new TextButton(new Vector2(UIScaler.GetHCenter(-2f), vOffset),
                new Vector2(3, 1),
                GetPurchasePrice(game.cd.items[s]),
                delegate {; },
                Color.black);
            tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.yellow;
            tb.background.transform.parent = scrollArea.transform;
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");

            vOffset += 7;
        }

        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (vOffset - 3) * UIScaler.GetPixelsPerUnit());
    }


    public void DrawPartyItems()
    {
        DialogBox db = new DialogBox(new Vector2(UIScaler.GetHCenter(7), 0.5f), new Vector2(10, 2), new StringKey("val", "SELL"));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.SetFont(game.gameType.GetHeaderFont());
        db.AddBorder();

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(7), 2.5f), new Vector2(10, 24.5f), StringKey.NULL);
        db.AddBorder();
        db.background.AddComponent<UnityEngine.UI.Mask>();
        UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

        GameObject scrollArea = new GameObject("scroll");
        RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
        scrollArea.transform.parent = db.background.transform;
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 1);
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, (9) * UIScaler.GetPixelsPerUnit());

        GameObject scrollBarObj = new GameObject("scrollbar");
        scrollBarObj.transform.parent = db.background.transform;
        RectTransform scrollBarRect = scrollBarObj.AddComponent<RectTransform>();
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 1 * UIScaler.GetPixelsPerUnit());
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 24.5f * UIScaler.GetPixelsPerUnit());
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

        float vOffset = 3f;

        TextButton tb = null;
        foreach (string s in game.quest.items)
        {
            tb = new TextButton(new Vector2(UIScaler.GetHCenter(7.5f), vOffset + 4.5f),
                new Vector2(8, 2),
                game.cd.items[s].name,
                delegate {; },
                Color.clear);
            tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            tb.background.transform.parent = scrollArea.transform;
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");

            Texture2D itemTex = ContentData.FileToTexture(game.cd.items[s].image);
            Sprite itemSprite = Sprite.Create(itemTex, new Rect(0, 0, itemTex.width, itemTex.height), Vector2.zero, 1);

            tb = new TextButton(new Vector2(UIScaler.GetHCenter(9.5f), vOffset + 0.5f),
                new Vector2(4, 4),
                StringKey.NULL,
                delegate {; },
                Color.clear);
            tb.background.GetComponent<UnityEngine.UI.Image>().sprite = itemSprite;
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            tb.background.transform.parent = scrollArea.transform;

            StringKey act = new StringKey("val", "CLASS");
            if (game.cd.items[s].act == 1)
            {
                act = new StringKey("val", "ACT_1");
            }
            if (game.cd.items[s].act == 2)
            {
                act = new StringKey("val", "ACT_2");
            }
            tb = new TextButton(new Vector2(UIScaler.GetHCenter(10f), vOffset + 4f),
                new Vector2(3, 1),
                act,
                delegate {; },
                Color.black);
            tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.yellow;
            tb.background.transform.parent = scrollArea.transform;
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");

            int cost = GetPurchasePrice(game.cd.items[s]);
            if (game.quest.vars.GetValue("$%sellratio") != 0)
            {
                cost = Mathf.RoundToInt(GetPurchasePrice(game.cd.items[s]) * game.quest.vars.GetValue("$%sellratio"));
            }
            tb = new TextButton(new Vector2(UIScaler.GetHCenter(10f), vOffset),
                new Vector2(3, 1),
                cost,
                delegate {; },
                Color.black);
            tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.yellow;
            tb.background.transform.parent = scrollArea.transform;
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");

            vOffset += 7;
        }

        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (vOffset - 3) * UIScaler.GetPixelsPerUnit());
    }

    public int GetPurchasePrice(ItemData item)
    {
        if (item.act == 0) return 25;
        return item.price;
    }

    public void DrawGold()
    {
        DialogBox db = new DialogBox(
            new Vector2(UIScaler.GetHCenter(-4), 27.5f),
            new Vector2(5, 2),
            new StringKey("val", "GOLD"));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.SetFont(game.gameType.GetHeaderFont());
        db.AddBorder();

        db = new DialogBox(
            new Vector2(UIScaler.GetHCenter(1), 27.5f),
            new Vector2(3, 2),
            Mathf.RoundToInt(game.quest.vars.GetValue("$%gold")));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();
    }
}
