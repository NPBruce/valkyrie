using UnityEngine;
using System.Collections;

public class MenuButton {
    public MenuButton()
    {
        TextButton qb;
        if (Game.Get().editMode)
        {
            qb = new TextButton(new Vector2(UIScaler.GetRight(-9), UIScaler.GetBottom(-3)), new Vector2(8, 2), "Menu", delegate { Menu(); }, Color.red);
        }
        else
        {
            qb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Menu", delegate { Menu(); }, Color.red);
        }

        // Untag as dialog so this isn't cleared away

        qb.ApplyTag("questui");
    }

    public void Menu()
    {
        Game game = Game.Get();
        if (game.editMode)
        {
            EditorMenu.Create();
        }
        else
        {
            GameMenu.Create();
        }
    }

}
