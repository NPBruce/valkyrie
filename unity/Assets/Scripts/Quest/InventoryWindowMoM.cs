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
        scrollArea.SetLocation(UIScaler.GetHCenter(-17), 5, 34, 13);
        new UIElementBorder(scrollArea);

        float xOffset = UIScaler.GetHCenter(-16);

        TextButton tb = null;
        foreach (string s in game.quest.itemInspect.Keys)
        {
            string tmp = s;
            tb = new TextButton(new Vector2(xOffset, 14),
                new Vector2(8, 2),
                game.cd.items[s].name,
                delegate { Inspect(tmp); },
                Color.black);
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            tb.background.transform.SetParent(scrollArea.GetScrollTransform());
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");

            Texture2D itemTex = ContentData.FileToTexture(game.cd.items[s].image);
            Sprite itemSprite = Sprite.Create(itemTex, new Rect(0, 0, itemTex.width, itemTex.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect);

            tb = new TextButton(new Vector2(xOffset, 6),
                new Vector2(8, 8),
                StringKey.NULL,
                delegate { Inspect(tmp); },
                Color.clear);
            tb.background.GetComponent<UnityEngine.UI.Image>().sprite = itemSprite;
            tb.background.transform.SetParent(scrollArea.GetScrollTransform());
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;

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
        game.quest.Save();
        Game.Get().quest.eManager.QueueEvent(Game.Get().quest.itemInspect[item]);
        Game.Get().quest.eManager.QueueEvent(Game.Get().quest.itemInspect[item]);
    }
}
