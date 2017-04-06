using UnityEngine;
using System.Collections;
using Assets.Scripts.Content;

// Used to display remaining morale
public class MoraleDisplay {
    DialogBox md;

    // Construct and display
    public MoraleDisplay()
    {
        Game game = Game.Get();
        md = new DialogBox(new Vector2(0.75f, 0.5f), new Vector2(3, 3), new StringKey(game.quest.morale.ToString(),false), Color.red);
        md.textObj.tag = "questui";
        md.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
        md.background.tag = "questui";
        md.AddBorder();
    }

    // Update must be called if the morale is changed
    public void Update()
    {
        Game game = Game.Get();
        md.textObj.GetComponent<UnityEngine.UI.Text>().text = game.quest.morale.ToString();
    }
}

