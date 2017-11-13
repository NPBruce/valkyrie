using Assets.Scripts.Content;
using Assets.Scripts.UI;
using UnityEngine;
using System.Collections.Generic;

// Next stage button is used by MoM to move between investigators and monsters
public class InventoryWindowMoM
{
    // Construct and display
    public InventoryWindowMoM()
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
        scrollArea.SetLocation(UIScaler.GetHCenter(-17), 5, 34, 14);
        new UIElementBorder(scrollArea);

        float xOffset = 1;

        foreach (string s in game.quest.itemInspect.Keys)
        {
            string tmp = s;

            ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetLocation(xOffset, 9, 8, 3);
            ui.SetButton(delegate { Inspect(tmp); });
            ui.SetText(game.cd.items[s].name, Color.black);
            ui.SetBGColor(Color.white);

            Texture2D itemTex = ContentData.FileToTexture(game.cd.items[s].image);
            Sprite itemSprite = Sprite.Create(itemTex, new Rect(0, 0, itemTex.width, itemTex.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect);
            ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetLocation(xOffset, 1, 8, 8);
            ui.SetButton(delegate { Inspect(tmp); });
            ui.SetImage(itemSprite);

            xOffset += 9;
        }
        scrollArea.SetScrollSize(xOffset);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-4f), 24.5f, 8, 2);
        ui.SetText(CommonStringKeys.CLOSE);
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Destroyer.Dialog);
        new UIElementBorder(ui);
    }

    public void Inspect(string item)
    {
        Destroyer.Dialog();
        Game.Get().quest.Save();
        Game.Get().quest.eManager.QueueEvent(Game.Get().quest.itemInspect[item]);
    }
}
