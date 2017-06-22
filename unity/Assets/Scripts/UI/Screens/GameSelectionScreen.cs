using Assets.Scripts.Content;
using UnityEngine;
using FFGAppImport;
using System.Threading;

namespace Assets.Scripts.UI.Screens
{

    // First screen which contains game type selection
    // and import controls
    public class GameSelectionScreen
    {
        FFGImport fcD2E;
        FFGImport fcMoM;
        Thread importThread;

        private StringKey D2E_NAME = new StringKey("val","D2E_NAME");
        private StringKey CONTENT_IMPORT = new StringKey("val", "CONTENT_IMPORT");
        private StringKey CONTENT_REIMPORT = new StringKey("val", "CONTENT_REIMPORT");
        private StringKey D2E_APP_NOT_FOUND = new StringKey("val", "D2E_APP_NOT_FOUND");
        private StringKey MOM_NAME = new StringKey("val", "MOM_NAME");
        private StringKey MOM_APP_NOT_FOUND = new StringKey("val", "MOM_APP_NOT_FOUND");
        private StringKey CONTENT_IMPORTING = new StringKey("val", "CONTENT_IMPORTING");

        // Create a menu which will take up the whole screen and have options.  All items are dialog for destruction.
        public GameSelectionScreen()
        {
            Draw();
        }

        public void Draw()
        {
            // This will destroy all
            Destroyer.Destroy();

            Game game = Game.Get();

            game.gameType = new NoGameType();

            // Get the current content for games
            if (Application.platform == RuntimePlatform.OSXPlayer)
            {
                fcD2E = new FFGImport(FFGAppImport.GameType.D2E, Platform.MacOS, Game.AppData() + "/", Application.isEditor);
                fcMoM = new FFGImport(FFGAppImport.GameType.MoM, Platform.MacOS, Game.AppData() + "/", Application.isEditor);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                fcD2E = new FFGImport(FFGAppImport.GameType.D2E, Platform.Android, Game.AppData() + "/", Application.isEditor);
                fcMoM = new FFGImport(FFGAppImport.GameType.MoM, Platform.Android, Game.AppData() + "/", Application.isEditor);
            }
            else
            {
                fcD2E = new FFGImport(FFGAppImport.GameType.D2E, Platform.Windows, Game.AppData() + "/", Application.isEditor);
                fcMoM = new FFGImport(FFGAppImport.GameType.MoM, Platform.Windows, Game.AppData() + "/", Application.isEditor);
            }

            fcD2E.Inspect();
            fcMoM.Inspect();

            // Banner Image
            Sprite bannerSprite;
            Texture2D newTex = Resources.Load("sprites/banner") as Texture2D;

            GameObject banner = new GameObject("banner");
            banner.tag = Game.DIALOG;

            banner.transform.SetParent(game.uICanvas.transform);

            RectTransform trans = banner.AddComponent<RectTransform>();
            trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 1 * UIScaler.GetPixelsPerUnit(), 7f * UIScaler.GetPixelsPerUnit());
            trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (UIScaler.GetWidthUnits() - 18f) * UIScaler.GetPixelsPerUnit() / 2f, 18f * UIScaler.GetPixelsPerUnit());
            banner.AddComponent<CanvasRenderer>();


            UnityEngine.UI.Image image = banner.AddComponent<UnityEngine.UI.Image>();
            bannerSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            image.sprite = bannerSprite;
            image.rectTransform.sizeDelta = new Vector2(18f * UIScaler.GetPixelsPerUnit(), 7f * UIScaler.GetPixelsPerUnit());

            Color startColor = Color.white;
            // If we need to import we can't play this type
            if (fcD2E.NeedImport())
            {
                startColor = Color.gray;
            }
            // Draw D2E button
            UIElement ui = new UIElement();
            ui.SetLocation((UIScaler.GetWidthUnits() - 30) / 2, 10, 30, 4);
            ui.SetText(D2E_NAME, startColor);
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(delegate { D2E(); });
            ui.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(ui, startColor);

            // Draw D2E import button
            ui = new UIElement();
            if (fcD2E.ImportAvailable())
            {
                ui.SetLocation((UIScaler.GetWidthUnits() - 10) / 2, 14.2f, 10, 2);
                StringKey keyText = fcD2E.NeedImport() ? CONTENT_IMPORT : CONTENT_REIMPORT;
                ui.SetText(keyText);
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetButton(delegate { Import("D2E"); });
                ui.SetBGColor(new Color(0, 0.03f, 0f));
                new UIElementBorder(ui);
            }
            else // Import unavailable
            {
                ui.SetLocation((UIScaler.GetWidthUnits() - 24) / 2, 14.2f, 24, 1);
                ui.SetText(D2E_APP_NOT_FOUND, Color.red);
                new UIElementBorder(ui, Color.red);
            }

            // Draw MoM button
            startColor = Color.white;
            if (fcMoM.NeedImport())
            {
                startColor = Color.gray;
            }
            ui = new UIElement();
            ui.SetLocation((UIScaler.GetWidthUnits() - 30) / 2, 19, 30, 4);
            ui.SetText(MOM_NAME, startColor);
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(delegate { MoM(); });
            ui.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(ui, startColor);

            // Draw MoM import button
            ui = new UIElement();
            if (fcMoM.ImportAvailable())
            {
                ui.SetLocation((UIScaler.GetWidthUnits() - 10) / 2, 23.2f, 10, 2);
                StringKey keyText = fcMoM.NeedImport() ? CONTENT_IMPORT : CONTENT_REIMPORT;
                ui.SetText(keyText);
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetButton(delegate { Import("MoM"); });
                ui.SetBGColor(new Color(0, 0.03f, 0f));
                new UIElementBorder(ui);
            }
            else // Import unavailable
            {
                ui.SetLocation((UIScaler.GetWidthUnits() - 24) / 2, 23.2f, 24, 1);
                ui.SetText(MOM_APP_NOT_FOUND, Color.red);
                new UIElementBorder(ui, Color.red);
            }

            ui = new UIElement();
            ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
            ui.SetText(CommonStringKeys.EXIT, Color.red);
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(Exit);
            ui.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(ui, Color.red);
        }

        // Start game as D2E
        public void D2E()
        {
            // Check if import neeeded
            if (!fcD2E.NeedImport())
            {
                Game.Get().gameType = new D2EGameType();
                Texture2D cursor = Resources.Load("sprites/CursorD2E") as Texture2D;
                Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
                loadLocalization();
                Destroyer.MainMenu();
            }
        }

        // Import content
        public void Import(string type)
        {
            Destroyer.Destroy();

            // Create an object
            GameObject logo = new GameObject("logo");
            // Mark it as dialog
            logo.tag = Game.DIALOG;
            logo.transform.SetParent(Game.Get().uICanvas.transform);

            RectTransform transBg = logo.AddComponent<RectTransform>();
            transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, UIScaler.GetHCenter(-3) * UIScaler.GetPixelsPerUnit(), 6 * UIScaler.GetPixelsPerUnit());
            transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 8 * UIScaler.GetPixelsPerUnit(), 6 * UIScaler.GetPixelsPerUnit());

            // Create the image
            Texture2D tex = Resources.Load("sprites/logo") as Texture2D;
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1);
            UnityEngine.UI.Image uiImage = logo.AddComponent<UnityEngine.UI.Image>();
            uiImage.sprite = sprite;
            logo.AddComponent<SpritePulser>();

            // Display message
            UIElement ui = new UIElement();
            ui.SetLocation(2, 20, UIScaler.GetWidthUnits() - 4, 2);
            ui.SetText(CONTENT_IMPORTING);
            ui.SetFontSize(UIScaler.GetMediumFont());
            if (type.Equals("D2E"))
            {
                importThread = new Thread(new ThreadStart(delegate { fcD2E.Import(); }));
            }
            if (type.Equals("MoM"))
            {
                importThread = new Thread(new ThreadStart(delegate { fcMoM.Import(); }));
            }
            importThread.Start();
            //while (!importThread.IsAlive) ;
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
                Texture2D cursor = Resources.Load("sprites/CursorMoM") as Texture2D;
                Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
                loadLocalization();
                Destroyer.MainMenu();
            }
        }

        /// <summary>
        /// After selecting game, we load the localization file.
        /// Deppends on the gameType selected.
        /// There are two Localization files from ffg, one for D2E and one for MoM
        /// </summary>
        private void loadLocalization()
        {
            // After content import, we load the localization file
            if (LocalizationRead.ffgDict == null)
            {
                // FFG default language is always English
                LocalizationRead.ffgDict = new DictionaryI18n(
                    System.IO.File.ReadAllLines(ContentData.ImportPath() + "/text/Localization.txt"),
                    DictionaryI18n.DEFAULT_LANG,
                    Game.Get().currentLang);

                // Hack for Dunwich Horror
                if (System.IO.File.Exists(ContentData.ImportPath() + "/text/SCENARIO_CULT_OF_SENTINEL_HILL_MAD22.txt"))
                {
                    LocalizationRead.ffgDict.Add(new DictionaryI18n(System.IO.File.ReadAllLines(ContentData.ImportPath() + "/text/SCENARIO_CULT_OF_SENTINEL_HILL_MAD22.txt"),
                        DictionaryI18n.DEFAULT_LANG, Game.Get().currentLang));
                }
            }
        }

        public void Update()
        {
            if (importThread == null) return;
            if (importThread.IsAlive) return;
            importThread = null;
            Draw();
        }

        // Exit Valkyrie
        public void Exit()
        {
            Application.Quit();
        }
    }
}