using UnityEngine;
using System.Collections;

public class GameSelection
{
    FetchContent fc;

    // Create a menu which will take up the whole screen and have options.  All items are dialog for destruction.
    public GameSelection()
    {
        // This will destroy all
        Destroyer.Destroy();

        fc = new FetchContent("D2E");

        // Name.  We should replace this with a banner
        DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 3), "Valkyrie");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();

        Color startColor = Color.white;
        if (fc.needImport)
        {
            startColor = Color.gray;
        }
        TextButton tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 30) / 2, 10), new Vector2(30, 4f), "Descent: Journeys in the Dark Second Edition", delegate { D2E(); }, startColor);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

        Color importColor = Color.white;
        if (!fc.needImport)
        {
            importColor = Color.gray;
        }
        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 8) / 2, 14.2f), new Vector2(8, 2f), "Import Content", delegate { D2EImport(); }, importColor);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

        new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Exit", delegate { Exit(); }, Color.red);
    }

    // Start quest
    public void D2E()
    {
        if (!fc.needImport)
        {
            Destroyer.MainMenu();
        }
    }

    public void D2EImport()
    {
        fc.Import();
        Destroyer.Dialog();
        new GameSelection();
    }

    public void Exit()
    {
        Application.Quit();
    }
}
