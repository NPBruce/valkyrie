using Assets.Scripts.Content;
using UnityEngine;

// Special class for the Menu button present while in a quest
public class MenuButton {

    private StringKey MENU = new StringKey("val", "MENU");

    public MenuButton()
    {
        Game game = Game.Get();
        TextButton qb;
        // For the editor button is moved to the right
        if (Game.Get().editMode)
        {
<<<<<<< HEAD
            qb = new TextButton(new Vector2(UIScaler.GetRight(-8.5f), UIScaler.GetBottom(-2.5f)), new Vector2(8, 2), MENU, delegate { Menu(); }, Color.red);
=======
            qb = new TextButton(new Vector2(UIScaler.GetRight(-5.5f), UIScaler.GetBottom(-2.5f)), new Vector2(5, 2), "Menu", delegate { Menu(); });
>>>>>>> 60b8b01694a79c1fb12fe0d9bcfe32ca36991bb4
            qb.SetFont(game.gameType.GetHeaderFont());
        }
        else
        {
<<<<<<< HEAD
            qb = new TextButton(new Vector2(0.5f, UIScaler.GetBottom(-2.5f)), new Vector2(8, 2), MENU, delegate { Menu(); }, Color.red);
=======
            qb = new TextButton(new Vector2(0.5f, UIScaler.GetBottom(-2.5f)), new Vector2(5, 2), "Menu", delegate { Menu(); });
>>>>>>> 60b8b01694a79c1fb12fe0d9bcfe32ca36991bb4
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
