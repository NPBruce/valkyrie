using UnityEngine;
using System.Collections;

public class QuitButton {
    public QuitButton()
    {
        TextButton qb = new TextButton(new Vector2(UIScaler.GetRight(-9), UIScaler.GetBottom(-3)), new Vector2(8, 2), "Quit", delegate { Quit(); }, Color.red);

        // Untag as dialog so this isn't cleared away
        qb.background.tag = "Untagged";
        qb.button.tag = "Untagged";
    }

    public void Quit()
    {
        Application.Quit();
    }

}
