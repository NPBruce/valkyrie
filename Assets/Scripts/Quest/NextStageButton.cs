using UnityEngine;
using System.Collections;

public class NextStageButton
{
    public NextStageButton()
    {
        if (Game.Get().gameType.DisplayHeroes()) return;
        TextButton qb = new TextButton(new Vector2(UIScaler.GetRight(-5), UIScaler.GetBottom(-3)), new Vector2(4, 2), "->", delegate { Next(); });
        // Untag as dialog so this isn't cleared away
        qb.ApplyTag("questui");
    }

    public void Next()
    {
        Game game = Game.Get();

        game.quest.Save();

        game.quest.horrorPhase = !game.quest.horrorPhase;
    }
}
