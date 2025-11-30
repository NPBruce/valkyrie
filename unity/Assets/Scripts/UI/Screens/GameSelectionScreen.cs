using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Assets.Scripts.Content;
using FFGAppImport;
using UnityEngine;
using UnityEngine.UI;
using ValkyrieTools;

namespace Assets.Scripts.UI.Screens
{

    // First screen which contains game type selection
    // and import controls
    public class GameSelectionScreen
    {
        FFGImport fcD2E;
        FFGImport fcMoM;
#if IA
        FFGImport fcIA;
#endif
        protected string importType = "";
        Thread importThread;

        private static readonly StringKey D2E_NAME = new StringKey("val","D2E_NAME");
        private static readonly StringKey D2E_APP_NOT_FOUND = new StringKey("val", "D2E_APP_NOT_FOUND");

        private static readonly StringKey MOM_NAME = new StringKey("val", "MOM_NAME");
        private static readonly StringKey MOM_APP_NOT_FOUND = new StringKey("val", "MOM_APP_NOT_FOUND");


        private static readonly StringKey CONTENT_IMPORTING = new StringKey("val", "CONTENT_IMPORTING");
        private static readonly StringKey CONTENT_LOCATE = new StringKey("val", "CONTENT_LOCATE");
        private static readonly StringKey CONTENT_INSTALL_VIA_STEAM = new StringKey("val", "CONTENT_INSTALL_VIA_STEAM");
        private static readonly StringKey CONTENT_INSTALL_VIA_GOOGLEPLAY = new StringKey("val", "CONTENT_INSTALL_VIA_GOOGLEPLAY");
        private static readonly StringKey CONTENT_IMPORT_OFFICIAL = new StringKey("val", "CONTENT_IMPORT_OFFICIAL");
        private static readonly StringKey CONTENT_IMPORT_ZIP = new StringKey("val", "CONTENT_IMPORT_ZIP");
        private static readonly StringKey CONTENT_REIMPORT_OFFICIAL = new StringKey("val", "CONTENT_REIMPORT_OFFICIAL");
        private static readonly StringKey CONTENT_REIMPORT_ZIP = new StringKey("val", "CONTENT_REIMPORT_ZIP");

        private static readonly string MOM_APP_URL_ANDROID = "https://play.google.com/store/apps/details?id=com.fantasyflightgames.mom";
        private static readonly string MOM_APP_URL_STEAM = "https://store.steampowered.com/app/478980/Mansions_of_Madness/";

        private static readonly string D2E_APP_URL_ANDROID = "https://play.google.com/store/apps/details?id=com.fantasyflightgames.rtl";
        private static readonly string D2E_APP_URL_STEAM = "https://store.steampowered.com/app/477200/Descent_Road_to_Legend/";

#if IA
        private StringKey IA_NAME = new StringKey("val", "IA_NAME");
        private StringKey IA_APP_NOT_FOUND = new StringKey("val", "IA_APP_NOT_FOUND");
        private StringKey IA_APP_NOT_FOUND_ANDROID = new StringKey("val", "IA_APP_NOT_FOUND_ANDROID");
#endif

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

            InitializeImporters();

            DrawBanner();

            // first button y offset
            float offset = 12f;

            DrawD2ESection(offset);

            offset += 7f;

            DrawMoMSection(offset);
            
#if IA
            DrawIASection();
#endif

            DrawExitButton();

            // will display a button if a new version is available
            VersionManager.GetLatestVersionAsync(CheckForNewValkyrieVersion);
        }

        private void InitializeImporters()
        {
            // Get the current content for games
            if (Application.platform == RuntimePlatform.OSXPlayer)
            {
                fcD2E = new FFGImport(FFGAppImport.GameType.D2E, Platform.MacOS, Game.AppData() + Path.DirectorySeparatorChar, Application.isEditor);
                fcMoM = new FFGImport(FFGAppImport.GameType.MoM, Platform.MacOS, Game.AppData() + Path.DirectorySeparatorChar, Application.isEditor);
#if IA
                fcIA = new FFGImport(FFGAppImport.GameType.IA, Platform.MacOS, Game.AppData() + Path.DirectorySeparatorChar, Application.isEditor);
#endif
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                fcD2E = new FFGImport(FFGAppImport.GameType.D2E, Platform.Android, Game.AppData() + Path.DirectorySeparatorChar, Application.isEditor);
                try
                {
                    fcD2E.apkPath = Android.GetAndroidAPKPath("com.fantasyflightgames.rtl");
                    fcD2E.packageVersion = Android.GetAndroidPackageVersion("com.fantasyflightgames.rtl");
                }
                catch (System.Exception e)
                {
                    ValkyrieDebug.Log("Didn't find D2E app");
                    ValkyrieDebug.Log(e.ToString());
                }


                fcMoM = new FFGImport(FFGAppImport.GameType.MoM, Platform.Android, Game.AppData() + Path.DirectorySeparatorChar, Application.isEditor);

                try
                {
                    fcMoM.apkPath = Android.GetAndroidAPKPath("com.fantasyflightgames.mom");
                    fcMoM.packageVersion = Android.GetAndroidPackageVersion("com.fantasyflightgames.mom");
                }
                catch (System.Exception e)
                {
                    ValkyrieDebug.Log("Didn't find MoM app");
                    ValkyrieDebug.Log(e.ToString());
                }

#if IA
                fcIA = new FFGImport(FFGAppImport.GameType.IA, Platform.Android, Game.AppData() + Path.DirectorySeparatorChar, Application.isEditor);
                try
                {
                    fcIA.apkPath = Android.GetAndroidAPKPath("com.fantasyflightgames.iaca");
                    fcIA.packageVersion = Android.GetAndroidPackageVersion("com.fantasyflightgames.iaca");
                }
                catch (System.Exception e)
                {
                    ValkyrieDebug.Log("Didn't find IA app");
                    ValkyrieDebug.Log(e.ToString());
                }

#endif
            }
            else
            {
                fcD2E = new FFGImport(FFGAppImport.GameType.D2E, Platform.Windows, Game.AppData() + Path.DirectorySeparatorChar, Application.isEditor);
                fcMoM = new FFGImport(FFGAppImport.GameType.MoM, Platform.Windows, Game.AppData() + Path.DirectorySeparatorChar, Application.isEditor);
#if IA
                fcIA = new FFGImport(FFGAppImport.GameType.IA, Platform.Windows, Game.AppData() + Path.DirectorySeparatorChar, Application.isEditor);
#endif
            }

            fcD2E.Inspect();
            fcMoM.Inspect();
#if IA
            fcIA.Inspect();
#endif
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
            // Draw D2E button
            bool D2E_need_import = fcD2E.NeedImport();
            bool D2E_import_available = fcD2E.ImportAvailable();
            Color startColor = D2E_need_import ? Color.grey : Color.white;
            int fontSize = UIScaler.GetMediumFont();

            UIElement ui = new UIElement();
            ui.SetLocation((UIScaler.GetWidthUnits() - 30) / 2, offset, 30, 3);
            // If we need to import we can't play this type
            if (!D2E_need_import)
            {
                ui.SetText(D2E_NAME, startColor);
                ui.SetButton(delegate { D2E(); });
            }
            else
            {
                string message = "";
                if (D2E_import_available)
                {
                    message = D2E_NAME.Translate();
                } else
                {
                    message = D2E_NAME.Translate() + Environment.NewLine + D2E_APP_NOT_FOUND.Translate();
                    fontSize = (int) (UIScaler.GetMediumFont() / 1.05f);
                }
                ui.SetText(message, startColor);
            }
            ui.SetFontSize(fontSize);
            ui.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(ui, startColor);

            // Draw D2E import button
            ui = new UIElement();
            if (D2E_import_available || !D2E_need_import)
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    ui.SetLocation((UIScaler.GetWidthUnits() - 14) / 2, offset + 3.2f, 14, 2);
                    ui.SetText(D2E_need_import ? CONTENT_IMPORT_ZIP : CONTENT_REIMPORT_ZIP);
                    ui.SetFontSize(UIScaler.GetSmallFont());
                    ui.SetButton(delegate { ImportZip(ValkyrieConstants.typeDescent); });
                    ui.SetBGColor(new Color(0, 0.03f, 0f));
                    new UIElementBorder(ui);
                }
                else
                {
                    // Desktop: Side-by-side buttons
                    ui.SetLocation((UIScaler.GetWidthUnits() / 2) - 13, offset + 3.2f, 12, 2);
                    ui.SetText(D2E_need_import ? CONTENT_IMPORT_OFFICIAL : CONTENT_REIMPORT_OFFICIAL);
                    ui.SetFontSize(UIScaler.GetSmallFont());
                    ui.SetButton(delegate { Import(ValkyrieConstants.typeDescent, !D2E_import_available); });
                    ui.SetBGColor(new Color(0, 0.03f, 0f));
                    new UIElementBorder(ui);

                    ui = new UIElement();
                    ui.SetLocation((UIScaler.GetWidthUnits() / 2) + 1, offset + 3.2f, 12, 2);
                    ui.SetText(D2E_need_import ? CONTENT_IMPORT_ZIP : CONTENT_REIMPORT_ZIP);
                    ui.SetFontSize(UIScaler.GetSmallFont());
                    ui.SetButton(delegate { ImportZip(ValkyrieConstants.typeDescent); });
                    ui.SetBGColor(new Color(0, 0.03f, 0f));
                    new UIElementBorder(ui);
                }
            }
            else // Import unavailable
            {
                // only install button for Android
                if (Application.platform == RuntimePlatform.Android)
                {
                    ui.SetLocation((UIScaler.GetWidthUnits() - 14) / 2, offset + 3.2f, 14, 2);
                    ui.SetText(CONTENT_IMPORT_ZIP);
                    ui.SetFontSize(UIScaler.GetSmallFont());
                    ui.SetButton(delegate { ImportZip(ValkyrieConstants.typeDescent); });
                    ui.SetBGColor(new Color(0, 0.03f, 0f));
                    new UIElementBorder(ui);
                }
                else
                {
                    // install and locate button for other systems
                    ui = new UIElement();
                    ui.SetLocation((UIScaler.GetWidthUnits()/ 2) - 13, offset + 3.2f, 12, 1.3f);
                    ui.SetText(CONTENT_INSTALL_VIA_STEAM, Color.red);
                    ui.SetButton(delegate { GotoWebBrowser(D2E_APP_URL_STEAM); });
                    new UIElementBorder(ui, Color.red);

                    ui = new UIElement();
                    ui.SetLocation((UIScaler.GetWidthUnits() /2) + 1, offset + 3.2f, 12, 1.3f);
                    ui.SetText(CONTENT_LOCATE, Color.red);
                    ui.SetButton(delegate { Import(ValkyrieConstants.typeDescent, true); });
                    new UIElementBorder(ui, Color.red);

                    ui = new UIElement();
                    ui.SetLocation((UIScaler.GetWidthUnits() / 2) - 6, offset + 4.8f, 12, 1.3f);
                    ui.SetText(CONTENT_IMPORT_ZIP);
                    ui.SetButton(delegate { ImportZip(ValkyrieConstants.typeDescent); });
                    new UIElementBorder(ui, Color.red);
                }
            }
        }

        private void DrawMoMSection(float offset)
        {
            // Draw MoM button
            bool MoM_need_import = fcMoM.NeedImport();
            bool MoM_import_available = fcMoM.ImportAvailable();
            Color startColor = MoM_need_import ? Color.grey : Color.white;
            int fontSize = UIScaler.GetMediumFont();

            UIElement ui = new UIElement();
            ui.SetLocation((UIScaler.GetWidthUnits() - 30) / 2, offset, 30, 3);
            // If we need to import we can't play this type
            if (!MoM_need_import)
            {
                ui.SetText(MOM_NAME, startColor);
                ui.SetButton(delegate { MoM(); });
            }
            else
            {
                string message = "";
                if (MoM_import_available)
                {
                    message = MOM_NAME.Translate();
                }
                else
                {
                    message = MOM_NAME.Translate() + Environment.NewLine + MOM_APP_NOT_FOUND.Translate();
                    fontSize = (int)(UIScaler.GetMediumFont() / 1.05f);
                }
                ui.SetText(message, startColor);
            }
            ui.SetFontSize(fontSize);
            ui.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(ui, startColor);

            // Draw MoM import button
            ui = new UIElement();
            if (MoM_import_available || !MoM_need_import)
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    ui.SetLocation((UIScaler.GetWidthUnits() - 14) / 2, offset + 3.2f, 14, 2);
                    ui.SetText(MoM_need_import ? CONTENT_IMPORT_ZIP : CONTENT_REIMPORT_ZIP);
                    ui.SetFontSize(UIScaler.GetSmallFont());
                    ui.SetButton(delegate { ImportZip(ValkyrieConstants.typeMom); });
                    ui.SetBGColor(new Color(0, 0.03f, 0f));
                    new UIElementBorder(ui);
                }
                else
                {
                    // Desktop: Side-by-side buttons
                    ui.SetLocation((UIScaler.GetWidthUnits() / 2) - 13, offset + 3.2f, 12, 2);
                    ui.SetText(MoM_need_import ? CONTENT_IMPORT_OFFICIAL : CONTENT_REIMPORT_OFFICIAL);
                    ui.SetFontSize(UIScaler.GetSmallFont());
                    ui.SetButton(delegate { Import(ValkyrieConstants.typeMom, !MoM_import_available); });
                    ui.SetBGColor(new Color(0, 0.03f, 0f));
                    new UIElementBorder(ui);

                    ui = new UIElement();
                    ui.SetLocation((UIScaler.GetWidthUnits() / 2) + 1, offset + 3.2f, 12, 2);
                    ui.SetText(MoM_need_import ? CONTENT_IMPORT_ZIP : CONTENT_REIMPORT_ZIP);
                    ui.SetFontSize(UIScaler.GetSmallFont());
                    ui.SetButton(delegate { ImportZip(ValkyrieConstants.typeMom); });
                    ui.SetBGColor(new Color(0, 0.03f, 0f));
                    new UIElementBorder(ui);
                }
            }
            else // Import unavailable
            {
                // only install button for Android
                if (Application.platform == RuntimePlatform.Android)
                {
                    ui.SetLocation((UIScaler.GetWidthUnits() - 14) / 2, offset + 3.2f, 14, 2);
                    ui.SetText(CONTENT_IMPORT_ZIP);
                    ui.SetFontSize(UIScaler.GetSmallFont());
                    ui.SetButton(delegate { ImportZip(ValkyrieConstants.typeMom); });
                    ui.SetBGColor(new Color(0, 0.03f, 0f));
                    new UIElementBorder(ui);
                }
                else
                {
                    // install and locate button for other systems
                    ui = new UIElement();
                    ui.SetLocation((UIScaler.GetWidthUnits() / 2) - 13, offset + 3.2f, 12, 1.3f);
                    ui.SetText(CONTENT_INSTALL_VIA_STEAM, Color.red);
                    ui.SetButton(delegate { GotoWebBrowser(MOM_APP_URL_STEAM); });
                    new UIElementBorder(ui, Color.red);

                    ui = new UIElement();
                    ui.SetLocation((UIScaler.GetWidthUnits() / 2) + 1, offset + 3.2f, 12, 1.3f);
                    ui.SetText(CONTENT_LOCATE, Color.red);
                    ui.SetButton(delegate { Import(ValkyrieConstants.typeMom, true); });
                    new UIElementBorder(ui, Color.red);

                    ui = new UIElement();
                    ui.SetLocation((UIScaler.GetWidthUnits() / 2) - 6, offset + 4.8f, 12, 1.3f);
                    ui.SetText(CONTENT_IMPORT_ZIP);
                    ui.SetButton(delegate { ImportZip(ValkyrieConstants.typeMom); });
                    new UIElementBorder(ui, Color.red);
                }
            }
        }

#if IA
        private void DrawIASection()
        {
            // Draw IA button
            Color startColor = Color.white;
            if (fcIA.NeedImport())
            {
                startColor = Color.gray;
            }
            // Always disabled
            startColor = Color.gray;
            UIElement ui = new UIElement();
            ui.SetLocation((UIScaler.GetWidthUnits() - 30) / 2, 21, 30, 3);
            ui.SetText(IA_NAME, startColor);
            ui.SetFontSize(UIScaler.GetMediumFont());
            //ui.SetButton(delegate { IA(); });
            ui.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(ui, startColor);

            // Draw IA import button
            ui = new UIElement();
            if (fcIA.ImportAvailable())
            {
                ui.SetLocation((UIScaler.GetWidthUnits() - 14) / 2, 24.2f, 14, 2);
                StringKey keyText = StringKey.NULL;
                if (Application.platform == RuntimePlatform.Android)
                {
                    keyText = fcIA.NeedImport() ? CONTENT_IMPORT_ZIP : CONTENT_REIMPORT_ZIP;
                }
                else
                {
                    keyText = fcIA.NeedImport() ? CONTENT_IMPORT_OFFICIAL : CONTENT_REIMPORT_OFFICIAL;
                }
                ui.SetText(keyText);
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetButton(delegate { Import("IA"); });
                ui.SetBGColor(new Color(0, 0.03f, 0f));
                new UIElementBorder(ui);
            }
            else // Import unavailable
            {
                ui.SetLocation((UIScaler.GetWidthUnits() - 24) / 2, 24.2f, 24, 1);
                if (Application.platform == RuntimePlatform.Android)
                {
                    ui.SetText(IA_APP_NOT_FOUND_ANDROID, Color.red);
                }
                else
                {
                    ui.SetText(IA_APP_NOT_FOUND, Color.red);
                }
                new UIElementBorder(ui, Color.red);
            }
        }
#endif

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

            // Check if import neeeded
            if (!fcD2E.NeedImport())
            {
                Game game = Game.Get();

                game.gameType = new D2EGameType();


                // Load localization before content
                loadLocalization();

                // Loading list of content - doing this later is not required
                ContentLoader.GetContentData(game);

                // Download quests list
                game.questsList = new QuestsManager();
                game.remoteContentPackManager = new RemoteContentPackManager();
                Texture2D cursor = Resources.Load("sprites/CursorD2E") as Texture2D;
                Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);

                GameStateManager.MainMenu();
            }
        }

        // Import content
        // Import content
        public void Import(string type, bool manual_path_selection=false)
        {
            ValkyrieDebug.Log("INFO: Import "+type);

            if (Application.platform == RuntimePlatform.Android)
            {
                ImportZip(type);
                return;
            }

            if (manual_path_selection)
            {
                string app_filename="";
                if (type.Equals(ValkyrieConstants.typeDescent)) app_filename = "Road to Legend";
                if (type.Equals(ValkyrieConstants.typeMom)) app_filename = "Mansions of Madness";

#if UNITY_STANDALONE_OSX
                string extension = "app";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                string extension = "exe";
#else
                string extension = null;    
#endif

                NativeFilePicker.PickFile(delegate(string pickedPath)
                {
                    if (string.IsNullOrEmpty(pickedPath)) return;

                    string path = null;
                    if (Application.platform == RuntimePlatform.OSXPlayer)
                    {
                        path = Path.Combine(pickedPath, "Contents/Resources/Data");
                    }
                    else 
                    {
                        path = Path.Combine(Path.GetDirectoryName(pickedPath), app_filename + "_Data");
                    }
                    ValkyrieDebug.Log("Using path: " + path);

                    if (!Directory.Exists(path)) return;

                    StartImport(type, path);
                }, extension);
            }
            else
            {
                 // Logic for automatic import (if any) or if manual selection is false but not Android
                 // The original code didn't seem to have an 'else' block for manual_path_selection=false that did anything significant 
                 // other than fall through to the import execution which would fail with path=null.
                 // However, looking at the original code, if manual_path_selection is false, it skips the file browser
                 // and proceeds to import with path=null? 
                 // Wait, looking at the original code:
                 // if (manual_path_selection) { ... path = ... }
                 // Destroyer.Destroy(); ... importThread = ... fcD2E.Import(path);
                 // So if manual_path_selection is false, it calls Import(null).
                 // Let's preserve this behavior.
                 StartImport(type, null);
            }
        }

        private void StartImport(string type, string path)
        {
            Destroyer.Destroy();

            new LoadingScreen(CONTENT_IMPORTING.Translate());
            importType = type;

            if (type.Equals(ValkyrieConstants.typeDescent))
            {
                importThread = new Thread(new ThreadStart(delegate { fcD2E.Import(path); }));
            }
            if (type.Equals(ValkyrieConstants.typeMom))
            {
                importThread = new Thread(new ThreadStart(delegate { fcMoM.Import(path); }));
            }
#if IA
            if (type.Equals("IA"))
            {
                importThread = new Thread(new ThreadStart(delegate { fcIA.Import(path); }));
            }
#endif
            importThread.Start();
        }

        public void ImportZip(string type)
        {
            NativeFilePicker.PickFile(delegate (string path)
            {
                if (string.IsNullOrEmpty(path)) return;
                string zipPath = path;

                Destroyer.Destroy();
                new LoadingScreen(CONTENT_IMPORTING.Translate());
                importType = type;

                string tempPath = Path.Combine(Application.temporaryCachePath, "import_extract");
                if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
                Directory.CreateDirectory(tempPath);

                importThread = new Thread(new ThreadStart(delegate
                {
                    ZipManager.ExtractZipAsync(tempPath, zipPath, ZipManager.Extract_mode.ZIPMANAGER_EXTRACT_FULL);
                    ZipManager.Wait4PreviousSave();

                    // Check for raw assets
                    string resourcesAssets = Path.Combine(tempPath, "resources.assets");
                    bool rawAssetsFound = File.Exists(resourcesAssets);
                    // Also check for OBB in subfolders if not found at root (simple check)
                    if (!rawAssetsFound)
                    {
                        rawAssetsFound = Directory.GetFiles(tempPath, "*.obb", SearchOption.AllDirectories).Length > 0;
                    }

                    if (type.Equals(ValkyrieConstants.typeDescent))
                    {
                        if (rawAssetsFound) fcD2E.Import(tempPath);
                        else
                        {
                            ValkyrieDebug.Log("ZIP Import: Raw assets not found, performing direct copy.");
                            string destPath = fcD2E.path;
                            if (Directory.Exists(destPath)) Directory.Delete(destPath, true);
                            ZipManager.CopyDirectory(tempPath, destPath);
                        }
                    }
                    if (type.Equals(ValkyrieConstants.typeMom))
                    {
                        if (rawAssetsFound) fcMoM.Import(tempPath);
                        else
                        {
                            ValkyrieDebug.Log("ZIP Import: Raw assets not found, performing direct copy.");
                            string destPath = fcMoM.path;
                            if (Directory.Exists(destPath)) Directory.Delete(destPath, true);
                            ZipManager.CopyDirectory(tempPath, destPath);
                        }
                    }
#if IA
                    if (type.Equals("IA"))
                    {
                        if (rawAssetsFound) fcIA.Import(tempPath);
                        else
                        {
                            ValkyrieDebug.Log("ZIP Import: Raw assets not found, performing direct copy.");
                            string destPath = fcIA.path;
                            if (Directory.Exists(destPath)) Directory.Delete(destPath, true);
                            ZipManager.CopyDirectory(tempPath, destPath);
                        }
                    }
#endif
                }));
                importThread.Start();
            }, NativeFilePicker.ConvertExtensionToFileType("zip"));
        }

        // Start game as MoM
        public void MoM()
        {
            ValkyrieDebug.Log("INFO: Start game as MoM");

            // Check if import neeeded
            if (!fcMoM.NeedImport())
            {

                Game game = Game.Get();
                game.gameType = new MoMGameType();

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
                // MoM also has a special reound controller
                game.roundControl = new RoundControllerMoM();
                Texture2D cursor = Resources.Load("sprites/CursorMoM") as Texture2D;
                Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);

                GameStateManager.MainMenu();
            }
        }

        // Start game as IA
        public void IA()
        {
            // Not working yet
#if false
            // Check if import neeeded
            if (!fcIA.NeedImport())
            {
                Game.Get().gameType = new IAGameType();
                loadLocalization();
                Destroyer.MainMenu();
            }
#endif
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

        public void Update()
        {
            if (importThread == null) return;
            if (importThread.IsAlive) return;
            importThread = null;
            ExtractBundles();
            // TODO: Delete Obb dir for Android build here
            Draw();
        }


        /// <summary>
        /// 
        /// </summary>
        public void ExtractBundles()
        {
            try
            {
                string importDir = Path.Combine(Game.AppData(), importType + Path.DirectorySeparatorChar + "import");
                string bundlesFile = Path.Combine(importDir, "bundles.txt");
                ValkyrieDebug.Log("Loading all bundles from '" + bundlesFile + "'");
                string[] bundles = File.ReadAllLines(bundlesFile);
                foreach (string file in bundles)
                {
                    AssetBundle bundle = AssetBundle.LoadFromFile(file);
                    if (bundle == null) continue;
                    ValkyrieDebug.Log("Loading assets from '" + file + "'");
                    foreach (TextAsset asset in bundle.LoadAllAssets<TextAsset>())
                    {
                        string textDir = Path.Combine(importDir, "text");
                        Directory.CreateDirectory(textDir);
                        string f = Path.Combine(importDir, Path.Combine(textDir, asset.name + ".txt"));
                        ValkyrieDebug.Log("Writing text asset to '" + f + "'");
                        File.WriteAllText(f, asset.ToString());
                    }
                    bundle.Unload(false);
                }
            }
            catch (Exception ex)
            {
                ValkyrieDebug.Log("ExtractBundles caused " + ex.GetType().Name + ": " + ex.Message + " " + ex.StackTrace);
            }
            importType = "";
        }

        // Exit Valkyrie
        public void Exit()
        {
            ValkyrieDebug.Log("INFO: Leaving Valkyrie");

            Application.Quit();
        }

        // Open link and quit Valkyrie
        public void GotoWebBrowser(string url)
        {
            ValkyrieDebug.Log("INFO: Accessing new version");

            Application.OpenURL(url);

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

    }

}
