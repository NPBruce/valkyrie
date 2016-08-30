using UnityEngine;
using System.Collections;

public class MenuButton {
    public MenuButton()
    {
        TextButton qb = new TextButton(new Vector2(UIScaler.GetRight(-9), UIScaler.GetBottom(-3)), new Vector2(8, 2), "Menu", delegate { Menu(); }, Color.red);

        // Untag as dialog so this isn't cleared away

        qb.ApplyTag("Untagged");
    }

    public void Menu()
    {
        GameMenu.Create();
    }

}
