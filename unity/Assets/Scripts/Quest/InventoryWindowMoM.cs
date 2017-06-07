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

        UIElementScrollHorizontal scrollArea = new UIElementScrollHorizontal();
        scrollArea.SetLocation(UIScaler.GetHCenter(-17), 5, 34, 14);
        new UIElementBorder(scrollArea);

        float xOffset = 1;

        TextButton tb = null;
        foreach (string s in game.quest.itemInspect.Keys)
        {
            string tmp = s;

            UIElement ui = new UIElement(scrollArea.GetScrollTransform());
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
        scrollArea.SetScrollSize(xOffset - UIScaler.GetHCenter(-16));

        tb = new TextButton(
            new Vector2(UIScaler.GetHCenter(-4f), 24.5f),
            new Vector2(8, 2),
            CommonStringKeys.CLOSE,
            delegate { Destroyer.Dialog(); });
    }

    public void Inspect(string item)
    {
        Destroyer.Dialog();
        Game.Get().quest.Save();
        Game.Get().quest.eManager.QueueEvent(Game.Get().quest.itemInspect[item]);
    }
}
