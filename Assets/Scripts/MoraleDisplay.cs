using UnityEngine;
using System.Collections;

public class MoraleDisplay {
    DialogBox md;

    public MoraleDisplay()
    {
        Game game = Game.Get();
        md = new DialogBox(new Vector2(0.75f, 0.5f), new Vector2(3, 3), game.morale.ToString(), Color.red);
        md.textObj.tag = "Untagged";
        md.textObj.GetComponent<UnityEngine.UI.Text>().fontSize =UIScaler.GetLargeFont();
        md.background.tag = "Untagged";
        md.AddBorder();
    }

    public void Update()
    {
        Game game = Game.Get();
        md.textObj.GetComponent<UnityEngine.UI.Text>().text = game.morale.ToString();
    }
}

