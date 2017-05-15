using UnityEngine;
using System.Collections;
using Assets.Scripts.Content;

// Special class for the Menu button present while in a quest
public class InventoryButton
{
    private StringKey ITEMS = new StringKey("val", "ITEMS");

    public InventoryButton()
    {
        Game game = Game.Get();
        TextButton qb;
        // For the editor button is moved to the right
        if (game.editMode) return;

        if (game.gameType is MoMGameType) return;

        qb = new TextButton(new Vector2(15.5f, UIScaler.GetBottom(-2.5f)), new Vector2(5, 2), ITEMS, delegate { Items(); });
        qb.SetFont(game.gameType.GetHeaderFont());
        // Untag as dialog so this isn't cleared away
        qb.ApplyTag(Game.QUESTUI);
    }

    // When pressed bring up the approriate menu
    public void Items()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null) return;
        if (GameObject.FindGameObjectWithTag(Game.ACTIVATION) != null) return;
        new InventoryWindow();
    }
}
