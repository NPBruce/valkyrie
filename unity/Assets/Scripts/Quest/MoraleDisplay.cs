using UnityEngine;
using System.Collections;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

// Used to display remaining morale
public class MoraleDisplay {
    UIElement md;

    // Construct and display
    public MoraleDisplay()
    {
        Game game = Game.Get();
        int morale = Mathf.RoundToInt(game.quest.vars.GetValue("$%morale"));
        if (morale < 0)
        {
            morale = 0;
        }
        md = new UIElement(Game.QUESTUI);
        md.SetLocation(0.75f, 0.5f, 3, 3);
        md.SetText(morale.ToString(), Color.red);
        md.SetFontSize(UIScaler.GetLargeFont());
        new UIElementBorder(md, Color.red);
    }

    // Update must be called if the morale is changed
    public void Update()
    {
        Game game = Game.Get();
        int morale = Mathf.RoundToInt(game.quest.vars.GetValue("$%morale"));
        if (morale < 0)
        {
            morale = 0;
        }
        md.SetText(morale.ToString(), Color.red);
    }
}

