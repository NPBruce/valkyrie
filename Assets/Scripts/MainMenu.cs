using UnityEngine;
using System.Collections;

// Class for creation and management of the main menu
public class MainMenu {
    // Create a menu which will take up the whole screen and have options.  All items are dialog for destruction.
    public MainMenu()
    {
        // This will destroy all, because we shouldn't have anything left at the main menu
        Destroyer.Destroy();
        Game game = Game.Get();

        // Name.  Should this be the banner, or better to print Valkyrie with the game font?
        DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 3), "Valkyrie");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
        db.SetFont(game.gameType.GetHeaderFont());

        // Button for start quest/scenario
        TextButton tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 8), new Vector2(12, 2f), "Start " + game.gameType.QuestName(), delegate { Start(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        // Load save game (enabled if exists)
        if (SaveManager.SaveExists())
        {
            tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 11), new Vector2(12, 2f), "Load " + game.gameType.QuestName(), delegate { SaveManager.Load(); });
            tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);
            tb.SetFont(game.gameType.GetHeaderFont());
        }
        else
        {
            db = new DialogBox(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 11), new Vector2(12, 2f), "Load " + game.gameType.QuestName(), Color.red);
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            db.SetFont(game.gameType.GetHeaderFont());
            db.AddBorder();
        }

        // Content selection page
        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 14), new Vector2(12, 2f), "Select Content", delegate { Content(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        // Quest/Scenario editor
        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 17), new Vector2(12, 2f), game.gameType.QuestName() + " Editor", delegate { Editor(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        // About page (managed in this class)
        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 20), new Vector2(12, 2f), "About", delegate { About(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        // Exit Valkyrie
        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 23), new Vector2(12, 2f), "Exit", delegate { Exit(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());
    }

    // Start quest
    public void Start()
    {
        Game game = Game.Get();

        // Remove the main menu
        Destroyer.Dialog();

        game.SelectQuest();
    }

    public void Content()
    {
        new ContentSelect();
    }

    public void Editor()
    {
        Game game = Game.Get();
        game.SelectEditQuest();
    }

    // Create the about dialog
    public void About()
    {
        // This will destroy all, because we shouldn't have anything left at the main menu
        Destroyer.Destroy();

        Sprite bannerSprite;
        Texture2D newTex = Resources.Load("sprites/banner") as Texture2D;

        GameObject banner = new GameObject("banner");
        banner.tag = "dialog";

        banner.transform.parent = Game.Get().uICanvas.transform;

        RectTransform trans = banner.AddComponent<RectTransform>();
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 1 * UIScaler.GetPixelsPerUnit(), 7f * UIScaler.GetPixelsPerUnit());
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (UIScaler.GetWidthUnits() - 18f) * UIScaler.GetPixelsPerUnit() / 2f, 18f * UIScaler.GetPixelsPerUnit());
        banner.AddComponent<CanvasRenderer>();


        UnityEngine.UI.Image image = banner.AddComponent<UnityEngine.UI.Image>();
        bannerSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
        image.sprite = bannerSprite;
        image.rectTransform.sizeDelta = new Vector2(18f * UIScaler.GetPixelsPerUnit(), 7f * UIScaler.GetPixelsPerUnit());

        DialogBox db = new DialogBox(new Vector2((UIScaler.GetWidthUnits() - 30f) / 2, 10f), new Vector2(30, 6), "Valkyrie is a game master helper tool inspired by Fantasy Flight Games' Descent: Road to Legend.  Most images used are imported from FFG applications are are copyright FFG and other rights holders.");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();

        db = new DialogBox(new Vector2((UIScaler.GetWidthUnits() - 30f) / 2, 18f), new Vector2(30, 5), "Valkyrie uses DotNetZip-For-Unity and has code derived from Unity Studio.");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();

        TextButton tb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Back", delegate { Destroyer.MainMenu(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);
        tb.SetFont(Game.Get().gameType.GetHeaderFont());
    }

    public void Exit()
    {
        Application.Quit();
    }
}
