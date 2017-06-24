using Assets.Scripts.Content;
using Assets.Scripts.UI;
using UnityEngine;
using System.Collections.Generic;

// Next stage button is used by MoM to move between investigators and monsters
public class InventoryWindow
{
    // Construct and display
    public InventoryWindow()
    {
        Update();
    }

    public void Update()
    {
        Destroyer.Dialog();
        Game game = Game.Get();

        UIElement ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-18), 1, 36, 23);
        new UIElementBorder(ui);

        // Add a title to the page
        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-6), 1, 12, 3);
        ui.SetText(new StringKey("val", "ITEMS"));
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetLargeFont());

        UIElementScrollHorizontal scrollArea = new UIElementScrollHorizontal();
        scrollArea.SetLocation(UIScaler.GetHCenter(-17), 5, 34, 13);
        new UIElementBorder(scrollArea);

        float xOffset = 1;

        foreach (string s in game.quest.items)
        {
            ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetLocation(xOffset, 9, 8, 2);
            ui.SetText(game.cd.items[s].name, Color.black);
            ui.SetBGColor(Color.white);

            Texture2D itemTex = ContentData.FileToTexture(game.cd.items[s].image);
            Sprite itemSprite = Sprite.Create(itemTex, new Rect(0, 0, itemTex.width, itemTex.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect);

            ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetLocation(xOffset, 1, 8, 8);
            ui.SetImage(itemSprite);

            xOffset += 9;
        }
        scrollArea.SetScrollSize(xOffset);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-5), 19, 10, 4);
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-4), 20, 5, 2);
        ui.SetText(new StringKey("val", "GOLD"));
        ui.SetFontSize(UIScaler.GetMediumFont());

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(1), 20, 3, 2);
        ui.SetText(Mathf.RoundToInt(game.quest.vars.GetValue("$%gold")).ToString());
        ui.SetFontSize(UIScaler.GetMediumFont());

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-4f), 24.5f, 8, 2);
        ui.SetText(CommonStringKeys.CLOSE);
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Destroyer.Dialog);
        new UIElementBorder(ui);
    }
}
