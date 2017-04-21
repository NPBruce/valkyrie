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
        int morale = Mathf.RoundToInt(game.quest.vars.GetValue("$morale"));
        if (morale < 0)
        {
            morale = 0;
        }
        md = new DialogBox(new Vector2(0.75f, 0.5f), new Vector2(3, 3), morale, Color.red);
        md.textObj.tag = "questui";
        md.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
        md.background.tag = "questui";
        md.AddBorder();
    }

    // Update must be called if the morale is changed
    public void Update()
    {
        Game game = Game.Get();
        int morale = Mathf.RoundToInt(game.quest.vars.GetValue("$morale"));
        if (morale < 0)
        {
            morale = 0;
        }
        md.textObj.GetComponent<UnityEngine.UI.Text>().text = morale.ToString();
    }
}

