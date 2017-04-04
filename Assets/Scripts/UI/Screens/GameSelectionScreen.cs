using Assets.Scripts.Content;
using UnityEngine;

namespace Assets.Scripts.UI.Screens
{

    // First screen which contains game type selection
    // and import controls
    public class GameSelectionScreen
    {
        FetchContent fcD2E;
        FetchContent fcMoM;

        private StringKey D2E_NAME = new StringKey("{val:D2E_NAME}");
        private StringKey CONTENT_IMPORT = new StringKey("{val:CONTENT_IMPORT}");
        private StringKey CONTENT_REIMPORT = new StringKey("{val:CONTENT_REIMPORT}");
        private StringKey D2E_APP_NOT_FOUND = new StringKey("{val:D2E_APP_NOT_FOUND}");
        private StringKey MOM_NAME = new StringKey("{val:MOM_NAME}");
        private StringKey MOM_APP_NOT_FOUND = new StringKey("{val:MOM_APP_NOT_FOUND}");
        private StringKey CONTENT_IMPORTING = new StringKey("{val:CONTENT_IMPORTING}");

        // Create a menu which will take up the whole screen and have options.  All items are dialog for destruction.
        public GameSelectionScreen()
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
            TextButton tb = new TextButton(
                new Vector2((UIScaler.GetWidthUnits() - 30) / 2, 10), 
                new Vector2(30, 4f), 
                D2E_NAME, 
                delegate { D2E(); }, 
                startColor);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

            // Draw D2E import button
            if (fcD2E.importAvailable)
            {
                StringKey keyText = fcD2E.NeedImport() ? CONTENT_IMPORT : CONTENT_REIMPORT;
                tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 14.2f), new Vector2(10, 2f), keyText, delegate { Import("D2E"); });
                tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

            }
            else // Import unavailable
            {
                db = new DialogBox(new Vector2((UIScaler.GetWidthUnits() - 24) / 2, 14.2f), new Vector2(24, 1f), D2E_APP_NOT_FOUND, Color.red);
                db.AddBorder();
            }

            // Draw MoM button
            startColor = Color.white;
            if (fcMoM.NeedImport())
            {
                startColor = Color.gray;
            }
            tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 30) / 2, 19), new Vector2(30, 4f), MOM_NAME, delegate { MoM(); }, startColor);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

            // Draw MoM import button
            if (fcMoM.importAvailable)
            {
                StringKey keyText = fcMoM.NeedImport() ? CONTENT_IMPORT : CONTENT_REIMPORT;
                tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 23.2f), new Vector2(10, 2f), keyText, delegate { Import("MoM"); });
                tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);
            }
            else // Import unavailable
            {
                db = new DialogBox(new Vector2((UIScaler.GetWidthUnits() - 24) / 2, 23.2f), new Vector2(24, 1f), MOM_APP_NOT_FOUND, Color.red);
                db.AddBorder();
            }

            new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), CommonStringKeys.EXIT, delegate { Exit(); }, Color.red);
        }

        // Start game as D2E
        public void D2E()
        {
            // Check if import neeeded
            if (!fcD2E.NeedImport())
            {
                Game.Get().gameType = new D2EGameType();
                loadLocalization();
                Destroyer.MainMenu();
            }
        }

        // Import content
        public void Import(string type)
        {
            Destroyer.Destroy();
            // Display message
            DialogBox db = new DialogBox(new Vector2(2, 10), new Vector2(UIScaler.GetWidthUnits() - 4, 2), CONTENT_IMPORTING);
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
                loadLocalization();
                Destroyer.MainMenu();
            }
        }

        /// <summary>
        /// After selecting game, we load the localization file.
        /// Deppends on the gameType selected.
        /// There are two Localization.txt, one for D2E and one for MoM
        /// </summary>
        private void loadLocalization()
        {
            // After content import, we load the localization file
            if (LocalizationRead.ffgDict == null)
            {
                // FFG default language is allways English
                LocalizationRead.ffgDict = new DictionaryI18n(
                    System.IO.File.ReadAllLines(Game.Get().gameType.DataDirectory() + "ffg/text/Localization.txt"),
                    DictionaryI18n.DEFAULT_LANG,
                    Game.Get().currentLang);
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
            new GameSelectionScreen();
        }

        // Exit Valkyrie
        public void Exit()
        {
            Application.Quit();
        }
    }
}