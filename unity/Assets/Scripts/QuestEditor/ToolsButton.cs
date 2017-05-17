using UnityEngine;
using System.Collections;
using Assets.Scripts.Content;

// Special class for the Menu button present while in a quest
public class ToolsButton
{
    private StringKey TOOLS = new StringKey("val", "TOOLS");

    public ToolsButton()
    {
        Game game = Game.Get();
        if (!game.editMode) return;

        TextButton tb = new TextButton(new Vector2(UIScalers.GetRight(-10.5f), UIScaler.GetBottom(-2.5f)), new Vector2(5, 2), TOOLS, delegate { EditorTools.Create(); });
        tb.SetFont(game.gameType.GetHeaderFont());
        // Untag as dialog so this isn't cleared away
        tb.ApplyTag(Game.QUESTUI);
    }
}
