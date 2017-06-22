using UnityEngine;
using System.Collections;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

// Special class for the Menu button present while in a quest
public class LogButton
{
    private StringKey LOG = new StringKey("val", "LOG");

    public LogButton()
    {
        Game game = Game.Get();
        // For the editor button is moved to the right
        if (game.editMode) return;

        if (game.gameType is MoMGameType) return;

        UIElement ui = new UIElement(Game.QUESTUI);
        ui.SetLocation(5.5f, UIScaler.GetBottom(-2.5f),5, 2);
        ui.SetText(LOG);
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Log);
        new UIElementBorder(ui);
    }

    // When pressed bring up the approriate menu
    public void Log()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null) return;
        if (GameObject.FindGameObjectWithTag(Game.ACTIVATION) != null) return;
        new LogWindow();
    }
}
