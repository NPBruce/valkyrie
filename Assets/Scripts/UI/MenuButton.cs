using UnityEngine;
using System.Collections;

// Special class for the Menu button present while in a quest
public class MenuButton {
    public MenuButton()
    {
        Game game = Game.Get();
        TextButton qb;
        // For the editor button is moved to the right
        if (Game.Get().editMode)
        {
            qb = new TextButton(new Vector2(UIScaler.GetRight(-9), UIScaler.GetBottom(-3)), new Vector2(8, 2), "Menu", delegate { Menu(); }, Color.red);
            qb.SetFont(game.gameType.GetHeaderFont());
        }
        else
        {
            qb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Menu", delegate { Menu(); }, Color.red);
            qb.SetFont(game.gameType.GetHeaderFont());
        }

        // Untag as dialog so this isn't cleared away
        qb.ApplyTag("questui");
    }

    // When pressed bring up the approriate menu
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
