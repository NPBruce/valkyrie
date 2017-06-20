using Assets.Scripts.Content;
using Assets.Scripts.UI;
using UnityEngine;
using System.Collections.Generic;


// Tokens are events that are tied to a token placed on the board
public class ShopInterface : Quest.BoardComponent
{
    GameObject panel;
    QuestData.Event eventData;

    // Construct with quest info and reference to Game
    public ShopInterface(List<string> items, Game gameObject, string eventName) : base(gameObject)
    {
        game = gameObject;
        if (!game.quest.shops.ContainsKey(eventName))
        {
            List<string> contentItems = new List<string>();
            foreach (string s in items)
            {
                if (game.quest.itemSelect.ContainsKey(s))
                {
                    contentItems.Add(game.quest.itemSelect[s]);
                }
            }
            game.quest.shops.Add(eventName, contentItems);
        }
        eventData = game.quest.qd.components[eventName] as QuestData.Event;
        // Find quest UI panel
        panel = GameObject.Find("QuestUIPanel");
        if (panel == null)
        {
            // Create UI Panel
            panel = new GameObject("QuestUIPanel");
            panel.tag = Game.BOARD;
            panel.transform.SetParent(game.uICanvas.transform);
            panel.transform.SetAsFirstSibling();
            panel.AddComponent<RectTransform>();
            panel.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, Screen.height);
            panel.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, Screen.width);
        }
        Update();
    }

    public override QuestData.Event GetEvent()
    {
        return null;
    }

    // Clean up
    public override void Remove()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.SHOP))
            Object.Destroy(go);
        game.quest.activeShop = "";
    }

    public void Update()
    {
        Remove();
        game.quest.activeShop = eventData.sectionName;
        DrawButtons();
        DrawShopItems();
        DrawPartyItems();
        DrawGold();
    }

    public void DrawButtons()
    {
        float offset = 3;

        for (int i = 0; i < eventData.buttons.Count; i++)
        {
            StringKey label = new StringKey(null, EventManager.OutputSymbolReplace(eventData.buttons[i].Translate()), false);
            Color colour = Color.white;
            string colorRGB = ColorUtil.FromName(eventData.buttonColors[i]);
            // Check format is valid
            if ((colorRGB.Length != 7) || (colorRGB[0] != '#'))
            {
                Game.Get().quest.log.Add(new Quest.LogEntry("Warning: Button color must be in #RRGGBB format or a known name", true));
            }

            // Hexadecimal to float convert (0x00-0xFF -> 0.0-1.0)
            colour[0] = (float)System.Convert.ToInt32(colorRGB.Substring(1, 2), 16) / 255f;
            colour[1] = (float)System.Convert.ToInt32(colorRGB.Substring(3, 2), 16) / 255f;
            colour[2] = (float)System.Convert.ToInt32(colorRGB.Substring(5, 2), 16) / 255f;

            int tmp = i;
            TextButton tb = new TextButton(new Vector2(UIScaler.GetHCenter(-17), offset), new Vector2(10f, 2), 
                label, delegate { OnButton(tmp); }, colour);
            tb.ApplyTag(Game.SHOP);

            offset += 3;
        }
    }

    public void OnButton(int i)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null) return;
        game.quest.Save();
        game.quest.eManager.EndEvent(eventData, i);
    }

    public void DrawShopItems()
    {
        UIElement ui = new UIElement(Game.SHOP);
        ui.SetLocation(UIScaler.GetHCenter(-5), 0.5f, 10, 2);
        ui.SetText(new StringKey("val", "BUY"));
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetFont(game.gameType.GetHeaderFont());
        new UIElementBorder(ui);

        UIElementScrollVertical scrollArea = new UIElementScrollVertical(Game.SHOP);
        scrollArea.SetLocation(UIScaler.GetHCenter(-5), 2.5f, 10, 24.5f);
        new UIElementBorder(scrollArea);

        float vOffset = 3;

        TextButton tb = null;
        foreach (string s in game.quest.shops[eventData.sectionName])
        {
            string itemName = s;
            tb = new TextButton(new Vector2(UIScaler.GetHCenter(-4.5f), vOffset + 4.5f),
                new Vector2(8, 2),
                game.cd.items[s].name,
                delegate { Buy(itemName); },
                Color.clear);
            tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            tb.background.transform.SetParent(scrollArea.GetScrollTransform());
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
            tb.ApplyTag(Game.SHOP);

            Texture2D itemTex = ContentData.FileToTexture(game.cd.items[s].image);
            Sprite itemSprite = Sprite.Create(itemTex, new Rect(0, 0, itemTex.width, itemTex.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect);

            tb = new TextButton(new Vector2(UIScaler.GetHCenter(-2.5f), vOffset + 0.5f),
                new Vector2(4, 4),
                StringKey.NULL,
                delegate { Buy(itemName); },
                Color.clear);
            tb.background.GetComponent<UnityEngine.UI.Image>().sprite = itemSprite;
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            tb.background.transform.SetParent(scrollArea.GetScrollTransform());
            tb.ApplyTag(Game.SHOP);

            StringKey act = new StringKey(null, "-", false);
            if (game.cd.items[s].ContainsTrait("class"))
            {
                act = new StringKey("val", "CLASS");
            }
            if (game.cd.items[s].ContainsTrait("act1"))
            {
                act = new StringKey("val", "ACT_1");
            }
            if (game.cd.items[s].ContainsTrait("act2"))
            {
                act = new StringKey("val", "ACT_2");
            }
            tb = new TextButton(new Vector2(UIScaler.GetHCenter(-2f), vOffset + 4f),
                new Vector2(3, 1),
                act,
                delegate { Buy(itemName); },
                Color.black);
            tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.yellow;
            tb.background.transform.SetParent(scrollArea.GetScrollTransform());
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
            tb.ApplyTag(Game.SHOP);

            tb = new TextButton(new Vector2(UIScaler.GetHCenter(-2f), vOffset),
                new Vector2(3, 1),
                GetPurchasePrice(game.cd.items[s]),
                delegate { Buy(itemName); },
                Color.black);
            tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.yellow;
            tb.background.transform.SetParent(scrollArea.GetScrollTransform());
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
            tb.ApplyTag(Game.SHOP);

            vOffset += 7;
        }
        scrollArea.SetScrollSize(vOffset - 3);
    }


    public void DrawPartyItems()
    {
        UIElement ui = new UIElement(Game.SHOP);
        ui.SetLocation(UIScaler.GetHCenter(7), 0.5f, 10, 2);
        ui.SetText(new StringKey("val", "SELL"));
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetFont(game.gameType.GetHeaderFont());
        new UIElementBorder(ui);

        UIElementScrollVertical scrollArea = new UIElementScrollVertical(Game.SHOP);
        scrollArea.SetLocation(UIScaler.GetHCenter(7), 2.5f, 10, 24.5f);
        new UIElementBorder(scrollArea);

        float vOffset = 3f;

        TextButton tb = null;
        foreach (string s in game.quest.items)
        {
            string itemName = s;
            if (game.cd.items[itemName].ContainsTrait("relic")) continue;

            tb = new TextButton(new Vector2(UIScaler.GetHCenter(7.5f), vOffset + 4.5f),
                new Vector2(8, 2),
                game.cd.items[s].name,
                delegate { Sell(itemName); },
                Color.clear);
            tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            tb.background.transform.SetParent(scrollArea.GetScrollTransform());
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
            tb.ApplyTag(Game.SHOP);

            Texture2D itemTex = ContentData.FileToTexture(game.cd.items[s].image);
            Sprite itemSprite = Sprite.Create(itemTex, new Rect(0, 0, itemTex.width, itemTex.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect);

            tb = new TextButton(new Vector2(UIScaler.GetHCenter(9.5f), vOffset + 0.5f),
                new Vector2(4, 4),
                StringKey.NULL,
                delegate { Sell(itemName); },
                Color.clear);
            tb.background.GetComponent<UnityEngine.UI.Image>().sprite = itemSprite;
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            tb.background.transform.SetParent(scrollArea.GetScrollTransform());
            tb.ApplyTag(Game.SHOP);

            StringKey act = new StringKey(null, "-", false);
            if (game.cd.items[s].ContainsTrait("class"))
            {
                act = new StringKey("val", "CLASS");
            }
            if (game.cd.items[s].ContainsTrait("act1"))
            {
                act = new StringKey("val", "ACT_1");
            }
            if (game.cd.items[s].ContainsTrait("act2"))
            {
                act = new StringKey("val", "ACT_2");
            }
            tb = new TextButton(new Vector2(UIScaler.GetHCenter(10f), vOffset + 4f),
                new Vector2(3, 1),
                act,
                delegate { Sell(itemName); },
                Color.black);
            tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.yellow;
            tb.background.transform.SetParent(scrollArea.GetScrollTransform());
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
            tb.ApplyTag(Game.SHOP);

            tb = new TextButton(new Vector2(UIScaler.GetHCenter(10f), vOffset),
                new Vector2(3, 1),
                GetSellPrice(game.cd.items[itemName]),
                delegate { Sell(itemName); },
                Color.black);
            tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.yellow;
            tb.background.transform.SetParent(scrollArea.GetScrollTransform());
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
            tb.ApplyTag(Game.SHOP);

            vOffset += 7;
        }
        scrollArea.SetScrollSize(vOffset - 3);
    }

    public int GetPurchasePrice(ItemData item)
    {
        if (item.ContainsTrait("class"))
        {
            return 25;
        }
        return item.price;
    }

    public int GetSellPrice(ItemData item)
    {
        if (item.ContainsTrait("class"))
        {
            return 25;
        }

        if (game.quest.vars.GetValue("$%sellratio") == 0)
        {
            return GetPurchasePrice(item);
        }
        return Mathf.RoundToInt(GetPurchasePrice(item) * game.quest.vars.GetValue("$%sellratio"));
    }

    public void DrawGold()
    {
        UIElement ui = new UIElement(Game.SHOP);
        ui.SetLocation(UIScaler.GetHCenter(-16), 24, 5, 2);
        ui.SetText(new StringKey("val", "GOLD"));
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetFont(game.gameType.GetHeaderFont());
        new UIElementBorder(ui);

        ui = new UIElement(Game.SHOP);
        ui.SetLocation(UIScaler.GetHCenter(-11), 24, 3, 2);
        ui.SetText(Mathf.RoundToInt(game.quest.vars.GetValue("$%gold")).ToString());
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui);
    }

    public void Buy(string item)
    {
        ItemData itemData = game.cd.items[item];
        if (game.quest.vars.GetValue("$%gold") < GetPurchasePrice(itemData)) return;

        game.quest.vars.SetValue("$%gold", game.quest.vars.GetValue("$%gold") - GetPurchasePrice(itemData));
        game.quest.shops[eventData.sectionName].Remove(item);
        game.quest.items.Add(item);
        Update();
    }

    public void Sell(string item)
    {
        game.quest.vars.SetValue("$%gold", game.quest.vars.GetValue("$%gold") + GetSellPrice(game.cd.items[item]));
        game.quest.shops[eventData.sectionName].Add(item);
        game.quest.items.Remove(item);
        Update();
    }
}
