using UnityEngine;
using System.Collections;

public class MainMenu {
    // Create a menu which will take up the whole screen and have options.  All items are dialog for destruction.
    public MainMenu()
    {
        // This will destroy all, because we shouldn't have anything left at the main menu
        Destroyer.Destroy();
        Game game = Game.Get();

        // Name.  We should replace this with a banner
        DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 3), "Valkyrie");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();

        TextButton tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 10), new Vector2(12, 2f), "Start " + game.gameType.QuestName(), delegate { Start(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 13), new Vector2(12, 2f), "Select Content", delegate { Content(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 16), new Vector2(12, 2f), game.gameType.QuestName() + " Editor", delegate { Editor(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 19), new Vector2(12, 2f), "About", delegate { About(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 22), new Vector2(12, 2f), "Exit", delegate { Exit(); });
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

    // FIXME!
    public void Content()
    {
        new ContentSelect();
    }


    public void Editor()
    {
        Game game = Game.Get();
        game.SelectEditQuest();
    }

    public void About()
    {
        // This will destroy all, because we shouldn't have anything left at the main menu
        Destroyer.Destroy();

        // Name.  We should replace this with a banner
        DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 3), "Valkyrie");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();

        db = new DialogBox(new Vector2((UIScaler.GetWidthUnits() - 30f) / 2, 8f), new Vector2(30, 6), "Valkyrie is a game master helper tool inspired by Fantasy Flight Games' Descent: Road to Legend.  Most images used are imported from FFG applications are are copyright FFG and other rights holders.");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();

        db = new DialogBox(new Vector2((UIScaler.GetWidthUnits() - 30f) / 2, 16f), new Vector2(30, 5), "Valkyrie uses DotNetZip-For-Unity and has code derived from Unity Studio.");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();

        TextButton tb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Back", delegate { Destroyer.MainMenu(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
