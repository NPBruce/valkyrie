using Assets.Scripts.Content;
using UnityEngine;

// First screen which contains game type selection
// and import controls
public class GameSelection 
{
    FetchContent fcD2E;
    FetchContent fcMoM;

    //TextButton[] languageTextButtons;

    // Create a menu which will take up the whole screen and have options.  All items are dialog for destruction.
    public GameSelection()
    {
        // This will destroy all
        Destroyer.Destroy();

        Game game = Game.Get();

        game.gameType = new NoGameType();

        // Get the current content for games
        fcD2E = new FetchContent("D2E");
        fcMoM = new FetchContent("MoM");

        // Banner Image
        Sprite bannerSprite;
        Texture2D newTex = Resources.Load("sprites/banner") as Texture2D;

        GameObject banner = new GameObject("banner");
        banner.tag = "dialog";

        banner.transform.parent = game.uICanvas.transform;

        RectTransform trans = banner.AddComponent<RectTransform>();
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 1 * UIScaler.GetPixelsPerUnit(), 7f * UIScaler.GetPixelsPerUnit());
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (UIScaler.GetWidthUnits() - 18f) * UIScaler.GetPixelsPerUnit() / 2f, 18f * UIScaler.GetPixelsPerUnit());
        banner.AddComponent<CanvasRenderer>();


        UnityEngine.UI.Image image = banner.AddComponent<UnityEngine.UI.Image>();
        bannerSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
        image.sprite = bannerSprite;
        image.rectTransform.sizeDelta = new Vector2(18f * UIScaler.GetPixelsPerUnit(), 7f * UIScaler.GetPixelsPerUnit());

        DialogBox db;

        Color startColor = Color.white;
        // If we need to import we can't play this type
        if (fcD2E.NeedImport())
        {
            startColor = Color.gray;
        }
        // Draw D2E button
        TextButton tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 32) / 2, 10), new Vector2(30, 4f), "Descent: Journeys in the Dark Second Edition", delegate { D2E(); }, startColor);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

        // Draw D2E import button
        if (fcD2E.importAvailable)
        {
            string text = fcD2E.NeedImport() ? "Import Content" : "Reimport Content";
            tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 14.2f), new Vector2(10, 2f), text, delegate { Import("D2E"); });
            tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

        }
        else // Import unavailable
        {
            db = new DialogBox(new Vector2((UIScaler.GetWidthUnits() - 26) / 2, 14.2f), new Vector2(24, 1f), "Unable to locate Road to Legend, install via Steam", Color.red);
            db.AddBorder();
        }

        // Draw MoM button
        startColor = Color.white;
        if (fcMoM.NeedImport())
        {
            startColor = Color.gray;
        }
        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 32) / 2, 19), new Vector2(30, 4f), "Mansions of Madness Second Edition", delegate { MoM(); }, startColor);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

        // Draw MoM import button
        if (fcMoM.importAvailable)
        {
            string text = fcMoM.NeedImport() ? "Import Content" : "Reimport Content";
            tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 23.2f), new Vector2(10, 2f), text, delegate { Import("MoM"); });
            tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);
        }
        else // Import unavailable
        {
            db = new DialogBox(new Vector2((UIScaler.GetWidthUnits() - 26) / 2, 23.2f), new Vector2(24, 1f), "Unable to locate Mansions of Madness, install via Steam", Color.red);
            db.AddBorder();
        }

        /*
        // If localization data were imported.
        if (!fcMoM.NeedImport())
        {
            if (game.config.data.Get("MoMConfig") == null)
            {
                // TODO: FFGlookup to get the first line (languages list)
                string langsString = DictionaryI18n.FFG_LANGS;

                // List of languages
                game.config.data.Add("MoMConfig","Langs", langsString);
                // English is the default current language
                game.config.data.Add("MoMConfig", "currentLang","1");
                game.config.Save();
            }

            string[] langs = game.config.data.Get("MoMConfig", "Langs").Split(',');
            int currentLang = int.Parse(game.config.data.Get("MoMConfig", "currentLang"));

            game.currentLang = currentLang;

            //The first button in the list of buttons should start in this vertical coordinate
            float verticalStart = UIScaler.GetVCenter(-1.25f) - ((langs.Length - 1) * 1.25f);

            languageTextButtons = new TextButton[langs.Length];
            for (int i = 1; i < langs.Length; i++)
            {
                // Need current index in order to delegate not point to loop for variable
                int currentIndex = i;

                languageTextButtons[currentIndex] = new TextButton(
                    new Vector2(UIScaler.GetHCenter() + 15, verticalStart + (2.5f * currentIndex)),
                    new Vector2(8, 2f), 
                    langs[currentIndex], 
                    delegate { SelectLang(currentIndex); },
                    currentIndex == currentLang ? Color.white : Color.gray
                    );
            }
        }
        */

        new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Exit", delegate { Exit(); }, Color.red);
    }
    /*
    /// <summary>
    /// Select current language to specified
    /// </summary>
    /// <param name="i"></param>
    private void SelectLang(int i)
    {
        Game currentGame = Game.Get();
        if (currentGame.currentLang < languageTextButtons.Length)
        {
            languageTextButtons[currentGame.currentLang].setColor(Color.gray);
        }
        languageTextButtons[i].setColor(Color.white);
        // English is the default current language
        currentGame.config.data.Add("MoMConfig", "currentLang", i.ToString());
        currentGame.config.Save();
        currentGame.currentLang = i;
        refreshScreen();
        Debug.Log("new current language stablished:" + i.ToString());
    }

    /// <summary>
    /// Refreshes the screen texts
    /// </summary>
    private void refreshScreen()
    {
        // TODO
        // Reprint all texts in the screen.
    }
    */

    // Start game as D2E
    public void D2E()
    {
        // Check if import neeeded
        if (!fcD2E.NeedImport())
        {
            Game.Get().gameType = new D2EGameType();
            Destroyer.MainMenu();
        }
    }

    // Import content
    public void Import(string type)
    {
        Destroyer.Destroy();
        // Display message
        DialogBox db = new DialogBox(new Vector2(2, 10), new Vector2(UIScaler.GetWidthUnits() - 4, 2), "Importing...");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        // Perform importing later, to ensure message is displayed first
        Game.Get().CallAfterFrame(delegate { PerformImport(type); });
    }

    // Start game as MoM
    public void MoM()
    {
        // Check if import neeeded
        if (!fcMoM.NeedImport())
        {
            Game.Get().gameType = new MoMGameType();
            // MoM also has a special reound controller
            Game.Get().roundControl = new RoundControllerMoM();
            Destroyer.MainMenu();
        }
    }

    // Import (called once message displayed)
    private void PerformImport(string type)
    {
        if (type.Equals("D2E"))
        {
            fcD2E.Import();
        }
        if (type.Equals("MoM"))
        {
            fcMoM.Import();
        }
        Destroyer.Dialog();
        new GameSelection();
    }

    // Exit Valkyrie
    public void Exit()
    {
        Application.Quit();
    }
}
