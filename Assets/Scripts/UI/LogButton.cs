using UnityEngine;
using System.Collections;
using Assets.Scripts.Content;

// Special class for the Menu button present while in a quest
public class LogButton
{
    private StringKey LOG = new StringKey("val", "log");

    public LogButton()
    {
        Game game = Game.Get();
        TextButton qb;
        // For the editor button is moved to the right
        if (game.editMode) return;

        if (game.gameType is MoMGameType) return;

<<<<<<< HEAD
        qb = new TextButton(new Vector2(UIScaler.GetRight(-6.5f), UIScaler.GetBottom(-2.5f)), new Vector2(6, 2), LOG, delegate { Log(); });
=======
        qb = new TextButton(new Vector2(5.5f, UIScaler.GetBottom(-2.5f)), new Vector2(5, 2), "Log", delegate { Log(); });
>>>>>>> 60b8b01694a79c1fb12fe0d9bcfe32ca36991bb4
        qb.SetFont(game.gameType.GetHeaderFont());
        // Untag as dialog so this isn't cleared away
        qb.ApplyTag("questui");
    }

    // When pressed bring up the approriate menu
    public void Log()
    {
        if (GameObject.FindGameObjectWithTag("dialog") == null)
        {
            new LogWindow();
        }
    }
}
