using UnityEngine;
using System.Collections;

public class MainMenu {
    // Create a menu which will take up the whole screen and have options.  All items are dialog for destruction.
    public MainMenu()
    {
        // This will destroy all, because we shouldn't have anything left at the main menu
        Destroyer.Destroy();

        // Name.  We should replace this with a banner
        DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 3), "Valkyrie");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();

        TextButton tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 10), new Vector2(12, 2f), "Start Quest", delegate { Start(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 13), new Vector2(12, 2f), "Select Content", delegate { Content(); }, Color.red);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 16), new Vector2(12, 2f), "Quest Editor", delegate { Editor(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 19), new Vector2(12, 2f), "Exit", delegate { Exit(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);
    }

    // Start quest
    public void Start()
    {
        Game game = Game.Get();

        // Remove the main menu
        Destroyer.Dialog();

        game.SelectQuest();
    }

    // FXIME!
    public void Content()
    {
    }


    public void Editor()
    {
        new QuestEditSelection();
    }

    public void Exit()
    {
        Application.Quit();
    }
}
