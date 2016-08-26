using UnityEngine;
using System.Collections;

public class QuitButton {
    public QuitButton()
    {
        TextButton qb = new TextButton(new Vector2(950, 550), new Vector2(100, 40), "Quit", delegate { Quit(); });

        // Untag as dialog so this isn't cleared away
        qb.background.tag = "Untagged";
        qb.button.tag = "Untagged";
    }

    public void Quit()
    {
        Application.Quit();
    }

}
