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
        scrollArea.SetLocation(UIScaler.GetHCenter(-17), 5, 34, 13);
        new UIElementBorder(scrollArea);

        float xOffset = UIScaler.GetHCenter(-16);

        foreach (string s in game.quest.items)
        {
            db = new DialogBox(new Vector2(xOffset, 14),
                new Vector2(8, 2),
                game.cd.items[s].name,
                Color.black);
            db.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            db.background.transform.SetParent(scrollArea.GetScrollTransform());
            db.textObj.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");

            Texture2D itemTex = ContentData.FileToTexture(game.cd.items[s].image);
            Sprite itemSprite = Sprite.Create(itemTex, new Rect(0, 0, itemTex.width, itemTex.height), Vector2.zero, 1);

            db = new DialogBox(new Vector2(xOffset, 6),
                new Vector2(8, 8),
                StringKey.NULL,
                Color.clear,
                Color.white);
            db.background.GetComponent<UnityEngine.UI.Image>().sprite = itemSprite;
            db.background.transform.SetParent(scrollArea.GetScrollTransform());

            xOffset += 9;
        }
        scrollArea.SetScrollSize(xOffset - UIScaler.GetHCenter(-16));

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
