using Assets.Scripts.Content;
using Assets.Scripts.UI;
using UnityEngine;
using System.Collections.Generic;


// Tokens are events that are tied to a token placed on the board
public class ShopInterface : Quest.BoardComponent
{
    private const int LARGE_FONT_LIMIT = 36;

    GameObject panel;
    QuestData.Event eventData;

    // Construct with quest info and reference to Game
    public ShopInterface(List<string> items, Game gameObject, string eventName) : base(gameObject)
    {
        game = gameObject;
        if (!game.CurrentQuest.shops.ContainsKey(eventName))
        {
            List<string> contentItems = new List<string>();
            foreach (string s in items)
            {
                if (game.CurrentQuest.itemSelect.ContainsKey(s))
                {
                    contentItems.Add(game.CurrentQuest.itemSelect[s]);
                }
            }
            game.CurrentQuest.shops.Add(eventName, contentItems);
        }
        eventData = game.CurrentQuest.qd.components[eventName] as QuestData.Event;

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
        game.CurrentQuest.activeShop = "";
    }

    public void Update()
    {
        Remove();
        game.CurrentQuest.activeShop = eventData.sectionName;
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
            StringKey label = new StringKey(null, EventManager.OutputSymbolReplace(eventData.buttons[i].Label.Translate()), false);
            Color colour = Color.white;
            string colorRGB = ColorUtil.FromName(eventData.buttons[i].Color);
            // Check format is valid
            if ((colorRGB.Length != 7 && colorRGB.Length != 9) || (colorRGB[0] != '#'))
            {
                Game.Get().CurrentQuest.log.Add(new Quest.LogEntry("Warning: Button color must be in #RRGGBB format or a known name", true));
            }

            // Hexadecimal to float convert (0x00-0xFF -> 0.0-1.0)
            colour.r = (byte)System.Convert.ToByte(colorRGB.Substring(1, 2), 16);
            colour.g = (byte)System.Convert.ToByte(colorRGB.Substring(3, 2), 16);
            colour.b = (byte)System.Convert.ToByte(colorRGB.Substring(5, 2), 16);

            if (colorRGB.Length == 9)
                colour.a = (byte)System.Convert.ToByte(colorRGB.Substring(7, 2), 16);
            else
                colour.a = 255; // opaque by default

            int tmp = i;
            UIElement ui = new UIElement(Game.SHOP);
            ui.SetLocation(UIScaler.GetHCenter(-17), offset, 10, 2);
            ui.SetText(label, colour);
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(delegate { OnButton(tmp); });
            new UIElementBorder(ui, colour);

            offset += 3;
        }
    }

    public void OnButton(int i)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null) return;
        game.CurrentQuest.Save();
        game.CurrentQuest.eManager.EndEvent(eventData, i);
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

        float vOffset = 0.5f;

        foreach (string itemName in game.CurrentQuest.shops[eventData.sectionName])
        {
            var itemData = game.cd.Get<ItemData>(itemName);
            ui = new UIElement(Game.SHOP, scrollArea.GetScrollTransform());
            ui.SetLocation(0.5f, vOffset + 4.5f, 8, 2);
            ui.SetText(itemData.name, Color.black);
            ui.SetFontSize(ui.GetText().Length > LARGE_FONT_LIMIT ? UIScaler.GetSmallerFont() : UIScaler.GetSmallFont());
            ui.SetButton(delegate { Buy(itemName); });
            ui.SetBGColor(Color.white);

            ui = new UIElement(Game.SHOP, scrollArea.GetScrollTransform());
            ui.SetLocation(2.5f, vOffset + 0.5f, 4, 4);
            ui.SetButton(delegate { Buy(itemName); });
            Texture2D itemTex = ContentData.FileToTexture(itemData.image);
            Sprite itemSprite = Sprite.Create(itemTex, new Rect(0, 0, itemTex.width, itemTex.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect);
            ui.SetImage(itemSprite);

            StringKey act = new StringKey(null, "-", false);
            if (itemData.ContainsTrait("class"))
            {
                act = new StringKey("val", "CLASS");
            }
            if (itemData.ContainsTrait("act1"))
            {
                act = new StringKey("val", "ACT_1");
            }
            if (itemData.ContainsTrait("act2"))
            {
                act = new StringKey("val", "ACT_2");
            }
            ui = new UIElement(Game.SHOP, scrollArea.GetScrollTransform());
            ui.SetLocation(3, vOffset + 3.9f, 3, 0.7f);
            ui.SetText(act);
            ui.SetFontSize(UIScaler.GetSmallerFont());
            ui.SetButton(delegate { Buy(itemName); });
            ui.SetBGColor(Color.grey);
            new UIElementBorder(ui, Color.black);

            ui = new UIElement(Game.SHOP, scrollArea.GetScrollTransform());
            ui.SetLocation(3, vOffset, 3, 1);
            ui.SetText(GetPurchasePrice(itemData).ToString());
            ui.SetButton(delegate { Buy(itemName); });
            ui.SetBGColor(new Color32(178, 154, 0, 255)); // dark gold
            new UIElementBorder(ui, Color.black);

            vOffset += 7;
        }
        scrollArea.SetScrollSize(vOffset - 0.5f);
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

        float vOffset = 0.5f;

        foreach (string itemName in game.CurrentQuest.items)
        {
            var itemData = game.cd.Get<ItemData>(itemName);
            if (itemData.ContainsTrait("relic")) continue;

            ui = new UIElement(Game.SHOP, scrollArea.GetScrollTransform());
            ui.SetLocation(0.5f, vOffset + 4.5f, 8, 2);
            ui.SetText(itemData.name, Color.black);
            ui.SetFontSize(ui.GetText().Length > LARGE_FONT_LIMIT ? UIScaler.GetSmallerFont() : UIScaler.GetSmallFont());
            ui.SetButton(delegate { Sell(itemName); });
            ui.SetBGColor(Color.white);

            ui = new UIElement(Game.SHOP, scrollArea.GetScrollTransform());
            ui.SetLocation(2.5f, vOffset + 0.5f, 4, 4);
            ui.SetButton(delegate { Sell(itemName); });
            Texture2D itemTex = ContentData.FileToTexture(itemData.image);
            Sprite itemSprite = Sprite.Create(itemTex, new Rect(0, 0, itemTex.width, itemTex.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect);
            ui.SetImage(itemSprite);

            StringKey act = new StringKey(null, "-", false);
            if (itemData.ContainsTrait("class"))
            {
                act = new StringKey("val", "CLASS");
            }
            if (itemData.ContainsTrait("act1"))
            {
                act = new StringKey("val", "ACT_1");
            }
            if (itemData.ContainsTrait("act2"))
            {
                act = new StringKey("val", "ACT_2");
            }
            ui = new UIElement(Game.SHOP, scrollArea.GetScrollTransform());
            ui.SetLocation(3, vOffset + 3.9f, 3, 0.7f);
            ui.SetText(act);
            ui.SetFontSize(UIScaler.GetSmallerFont());
            ui.SetButton(delegate { Sell(itemName); });
            ui.SetBGColor(Color.grey);
            new UIElementBorder(ui, Color.black);

            ui = new UIElement(Game.SHOP, scrollArea.GetScrollTransform());
            ui.SetLocation(3, vOffset, 3, 1);
            ui.SetText(GetSellPrice(itemData).ToString());
            ui.SetButton(delegate { Sell(itemName); });
            ui.SetBGColor(new Color32(178, 154, 0, 255)); // dark gold
            new UIElementBorder(ui, Color.black);

            vOffset += 7;
        }
        scrollArea.SetScrollSize(vOffset - 0.5f);
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

        if (game.CurrentQuest.vars.GetValue("$%sellratio") == 0)
        {
            return GetPurchasePrice(item);
        }
        return Mathf.RoundToInt(GetPurchasePrice(item) * game.CurrentQuest.vars.GetValue("$%sellratio"));
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
        ui.SetText(Mathf.RoundToInt(game.CurrentQuest.vars.GetValue("$%gold")).ToString());
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui);
    }

    public void Buy(string item)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null) return;

        ItemData itemData = game.cd.Get<ItemData>(item);
        if (game.CurrentQuest.vars.GetValue("$%gold") < GetPurchasePrice(itemData)) return;

        game.CurrentQuest.vars.SetValue("$%gold", game.CurrentQuest.vars.GetValue("$%gold") - GetPurchasePrice(itemData));
        game.CurrentQuest.shops[eventData.sectionName].Remove(item);
        game.CurrentQuest.items.Add(item);
        Update();
    }

    public void Sell(string item)
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null) return;

        ItemData itemData = game.cd.Get<ItemData>(item);
        game.CurrentQuest.vars.SetValue("$%gold", game.CurrentQuest.vars.GetValue("$%gold") + GetSellPrice(itemData));
        game.CurrentQuest.shops[eventData.sectionName].Add(item);
        game.CurrentQuest.items.Remove(item);
        Update();
    }
}
