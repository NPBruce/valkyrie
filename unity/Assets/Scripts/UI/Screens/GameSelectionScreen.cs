using System;
using System.IO;
using System.Linq;
using Assets.Scripts.Content;
using UnityEngine;
using UnityEngine.UI;
using ValkyrieTools;
using Assets.Scripts.UI;

namespace Assets.Scripts.UI.Screens
{

    // First screen which contains game type selection
    // and import controls
    public class GameSelectionScreen
    {
        private static readonly StringKey D2E_NAME = new StringKey("val","D2E_NAME");
        private static readonly StringKey D2E_APP_NOT_FOUND = new StringKey("val", "D2E_APP_NOT_FOUND");

        private static readonly StringKey MOM_NAME = new StringKey("val", "MOM_NAME");
        private static readonly StringKey MOM_APP_NOT_FOUND = new StringKey("val", "MOM_APP_NOT_FOUND");

        private static readonly StringKey IMPORT_GAME_DATA   = new StringKey("val", "IMPORT_GAME_DATA");
        private static readonly StringKey REIMPORT_GAME_DATA = new StringKey("val", "REIMPORT_GAME_DATA");

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

            ImportManager.Inspect();

            DrawBanner();

            // first button y offset
            float offset = 12f;

            DrawD2ESection(offset);

            offset += 7f;

            DrawMoMSection(offset);



            DrawExitButton();

            // will display a button if a new version is available
            VersionManager.GetLatestVersionAsync(CheckForNewValkyrieVersion);
        }

        private void DrawBanner()
        {
            Game game = Game.Get();
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


            Image image = banner.AddComponent<Image>();
            bannerSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            image.sprite = bannerSprite;
        }

        private void DrawD2ESection(float offset)
        {
            bool needImport = ImportManager.NeedImport(ValkyrieConstants.typeDescent);
            Color startColor = needImport ? Color.grey : Color.white;
            int fontSize = UIScaler.GetMediumFont();

            UIElement ui = new UIElement();
            ui.SetLocation((UIScaler.GetWidthUnits() - 30) / 2, offset, 30, 3);
            if (!needImport)
            {
                ui.SetText(D2E_NAME, startColor);
                ui.SetButton(delegate { D2E(); });
            }
            else
            {
                string message = ImportManager.ImportAvailable(ValkyrieConstants.typeDescent)
                    ? D2E_NAME.Translate()
                    : D2E_NAME.Translate() + System.Environment.NewLine + D2E_APP_NOT_FOUND.Translate();
                if (!ImportManager.ImportAvailable(ValkyrieConstants.typeDescent))
                    fontSize = (int)(UIScaler.GetMediumFont() / 1.05f);
                ui.SetText(message, startColor);
            }
            ui.SetFontSize(fontSize);
            ui.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(ui, startColor);

            UIElement importBtn = new UIElement();
            importBtn.SetLocation((UIScaler.GetWidthUnits() - 14) / 2, offset + 3.2f, 14, 2);
            importBtn.SetText(needImport ? IMPORT_GAME_DATA : REIMPORT_GAME_DATA);
            importBtn.SetFontSize(UIScaler.GetSmallFont());
            importBtn.SetButton(delegate { new ContentImportScreen(ValkyrieConstants.typeDescent, needImport); });
            importBtn.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(importBtn);
        }

        private void DrawMoMSection(float offset)
        {
            bool needImport = ImportManager.NeedImport(ValkyrieConstants.typeMom);
            Color startColor = needImport ? Color.grey : Color.white;
            int fontSize = UIScaler.GetMediumFont();

            UIElement ui = new UIElement();
            ui.SetLocation((UIScaler.GetWidthUnits() - 30) / 2, offset, 30, 3);
            if (!needImport)
            {
                ui.SetText(MOM_NAME, startColor);
                ui.SetButton(delegate { MoM(); });
            }
            else
            {
                string message = ImportManager.ImportAvailable(ValkyrieConstants.typeMom)
                    ? MOM_NAME.Translate()
                    : MOM_NAME.Translate() + System.Environment.NewLine + MOM_APP_NOT_FOUND.Translate();
                if (!ImportManager.ImportAvailable(ValkyrieConstants.typeMom))
                    fontSize = (int)(UIScaler.GetMediumFont() / 1.05f);
                ui.SetText(message, startColor);
            }
            ui.SetFontSize(fontSize);
            ui.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(ui, startColor);

            UIElement importBtn = new UIElement();
            importBtn.SetLocation((UIScaler.GetWidthUnits() - 14) / 2, offset + 3.2f, 14, 2);
            importBtn.SetText(needImport ? IMPORT_GAME_DATA : REIMPORT_GAME_DATA);
            importBtn.SetFontSize(UIScaler.GetSmallFont());
            importBtn.SetButton(delegate { new ContentImportScreen(ValkyrieConstants.typeMom, needImport); });
            importBtn.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(importBtn);
        }

        private void DrawExitButton()
        {
            UIElement ui = new UIElement();
            ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
            ui.SetText(CommonStringKeys.EXIT, Color.red);
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(Exit);
            ui.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(ui, Color.red);
        }

        public void D2E()
        {
            ValkyrieDebug.Log("INFO: Start game as D2E");

            if (!ImportManager.NeedImport(ValkyrieConstants.typeDescent))
            {
                Game game = Game.Get();

                game.gameType = new D2EGameType();


                // Clean up any old localization
                LocalizationRead.RemoveDictionary("ffg");
                LocalizationRead.RemoveDictionary("csh");
                // Clean up texture cache
                if (ContentData.textureCache != null) ContentData.textureCache.Clear();

                // Load localization before content
                loadLocalization();

                // Loading list of content - doing this later is not required
                ContentLoader.GetContentData(game);

                // Download quests list
                game.questsList = new QuestsManager();
                game.remoteContentPackManager = new RemoteContentPackManager();
                game.roundControl = new RoundController();
                Texture2D cursor = Resources.Load("sprites/CursorD2E") as Texture2D;
                Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);

                GameStateManager.MainMenu();
            }
        }

        // Start game as MoM
        public void MoM()
        {
            ValkyrieDebug.Log("INFO: Start game as MoM");

            if (!ImportManager.NeedImport(ValkyrieConstants.typeMom))
            {

                Game game = Game.Get();
                game.gameType = new MoMGameType();

                // Clean up any old localization
                LocalizationRead.RemoveDictionary("ffg");
                LocalizationRead.RemoveDictionary("csh");
                // Clean up texture cache
                if (ContentData.textureCache != null) ContentData.textureCache.Clear();

                // Load localization before content
                loadLocalization();

                // Loading list of content - doing this later is not required
                ContentLoader.GetContentData(game);
                // Check if we found anything
                if (game.cd.GetPacks().Count == 0)
                {
                    ValkyrieDebug.Log("Error: Failed to find any content packs, please check that you have them present in: " + game.gameType.DataDirectory() + Environment.NewLine);
                    Application.Quit();
                }

                // Download quests list
                game.questsList = new QuestsManager();
                game.remoteContentPackManager = new RemoteContentPackManager();
                // MoM also has a special reound controller
                game.roundControl = new RoundControllerMoM();
                Texture2D cursor = Resources.Load("sprites/CursorMoM") as Texture2D;
                Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);

                GameStateManager.MainMenu();
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
            if (LocalizationRead.selectDictionary("ffg") == null)
            {
                DictionaryI18n ffgDict = new DictionaryI18n();
                var localizationFilesRaw = Directory.GetFiles(ContentData.ImportPath() + "/text", "Localization_*.txt");
                var localizationFiles = localizationFilesRaw.Where(x => !Path.GetFileName(x).Any(char.IsDigit));

                foreach (string file in localizationFiles)
                {
                    ffgDict.AddDataFromFile(file);
                }
                LocalizationRead.AddDictionary("ffg", ffgDict);

                // CoSH used for Dunwich Horror data
                DictionaryI18n cshDict = new DictionaryI18n();
                foreach (string file in Directory.GetFiles(ContentData.ImportPath() + "/text", "SCENARIO_CULT_OF_SENTINEL_HILL_MAD22_*.txt"))
                {
                    cshDict.AddDataFromFile(file);
                }
                LocalizationRead.AddDictionary("csh", cshDict);

                var game = Game.Get();
                foreach (var packInfo in game.config.GetPackLanguages(game.gameType.TypeName())
                    .Where(kv => !string.IsNullOrWhiteSpace(kv.Value)))
                {
                    LocalizationRead.SetGroupTranslationLanguage(packInfo.Key, packInfo.Value);
                }
            }
        }

        // Exit Valkyrie
        public void Exit()
        {
            ValkyrieDebug.Log("INFO: Leaving Valkyrie");

            Application.Quit();
        }

        public void CheckForNewValkyrieVersion()
        {
            StringKey NEW_VERSION_AVAILABLE = new StringKey("val", "NEW_VERSION_AVAILABLE");

            if ( VersionManager.VersionNewer(Game.Get().version, VersionManager.online_version) )
            {
                float string_width = 0f;
                UIElement ui = new UIElement();
                ui.SetText(NEW_VERSION_AVAILABLE, Color.green);
                string_width = ui.GetStringWidth(NEW_VERSION_AVAILABLE, UIScaler.GetMediumFont());
                ui.SetLocation(UIScaler.GetRight() - 3 - string_width, UIScaler.GetBottom(-3), string_width + 2, 2);
                ui.SetText(NEW_VERSION_AVAILABLE, Color.green);
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetButton(delegate { GotoWebBrowser(VersionManager.GetlatestReleaseURL()); } );
                ui.SetBGColor(new Color(0, 0.03f, 0f));
                new UIElementBorder(ui, Color.green);
            }
        }

        // Open link and quit Valkyrie
        private void GotoWebBrowser(string url)
        {
            ValkyrieDebug.Log("INFO: Accessing new version");

            Application.OpenURL(url);

            Application.Quit();
        }

    }

}
