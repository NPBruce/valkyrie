using Assets.Scripts.Content;
using Assets.Scripts.UI;
using UnityEngine;

// Special class for the Menu button present while in a quest
public class MenuButton {

    private StringKey MENU = new StringKey("val", "MENU");

    public MenuButton()
    {
        Game game = Game.Get();

        UIElement ui = new UIElement(Game.QUESTUI);
        // For the editor button is moved to the right
        if (Game.Get().editMode)
        {
            ui.SetLocation(UIScaler.GetRight(-5.5f), UIScaler.GetBottom(-2.5f),5, 2);
        }
        else
        {
            ui.SetLocation(0.5f, UIScaler.GetBottom(-2.5f),5, 2);
        }
        ui.SetText(MENU);
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Menu);
        new UIElementBorder(ui);
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
