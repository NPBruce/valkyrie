using UnityEngine;
using System.Collections.Generic;

public class Round
{
    public int round = 1;
    public bool heroesSelected = false;
    public int morale;

    // This function adjusts morale.  We don't write directly so that NoMorale can be triggered
    public void AdjustMorale(int m)
    {
        Game game = Game.Get();
        morale += m;
        if (morale < 0)
        {
            morale = 0;
            game.moraleDisplay.Update();
            game.quest.eManager.EventTriggerType("NoMorale");
        }
        game.moraleDisplay.Update();
    }
}
