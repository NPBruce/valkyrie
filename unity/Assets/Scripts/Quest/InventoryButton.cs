using UnityEngine;
using System.Collections;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

// Special class for the Menu button present while in a quest
public class InventoryButton
{
    private StringKey ITEMS = new StringKey("val", "ITEMS");

    public InventoryButton()
    {
        Game game = Game.Get();
        // For the editor button is moved to the right
        if (game.editMode) return;

        if (game.gameType is MoMGameType) return;

        UIElement ui = new UIElement(Game.QUESTUI);
        ui.SetLocation(15.5f, UIScaler.GetBottom(-2.5f), 5, 2);
        ui.SetText(ITEMS);
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Items);
        new UIElementBorder(ui);
    }

    // When pressed bring up the approriate menu
    public void Items()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null) return;
        if (GameObject.FindGameObjectWithTag(Game.ACTIVATION) != null) return;
        new InventoryWindow();
    }
}
