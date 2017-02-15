using UnityEngine;
using System.Collections;

// Next stage button is used by MoM to move between investigators and monsters
public class NextStageButton
{
    // Construct and display
    public NextStageButton()
    {
        if (Game.Get().gameType.DisplayHeroes()) return;
        TextButton qb = new TextButton(new Vector2(UIScaler.GetRight(-5), UIScaler.GetBottom(-3)), new Vector2(4, 2), "->", delegate { Next(); });
        // Untag as dialog so this isn't cleared away
        qb.ApplyTag("questui");
    }

    // Button pressed
    public void Next()
    {
        Game game = Game.Get();

        // Add to undo stack
        game.quest.Save();

        if (game.quest.horrorPhase)
        {
            game.roundControl.EndRound();
        }
        else
        {
            game.quest.horrorPhase = true;
        }
    }
}
