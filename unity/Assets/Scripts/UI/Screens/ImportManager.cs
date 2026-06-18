// unity/Assets/Scripts/UI/Screens/ImportManager.cs
using System;
using System.IO;
using System.Threading;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
using FFGAppImport;
using UnityEngine;
using ValkyrieTools;

namespace Assets.Scripts.UI.Screens
{
    public static class ImportManager
    {
        public static string importError = null;
        private static volatile string lastLogMessage = null;
        private static LoadingScreen loadingScreen;
        private static FFGImport fcD2E;
        private static FFGImport fcMoM;
        private static Thread importThread;
        private static string importType = "";
        private static Action onComplete;

        private static readonly StringKey CONTENT_IMPORTING = new StringKey("val", "CONTENT_IMPORTING");

        private const string DESCENT_DESKTOP_ZIP = "ImportDesktop_Descent.zip";
        private const string DESCENT_ANDROID_ZIP = "ImportAndroid_Descent.zip";
        private const string MOM_DESKTOP_ZIP = "ImportDesktop_MansionsOfMadnessImport.zip";
        private const string MOM_ANDROID_ZIP = "ImportAndroid_MansionsOfMadness.zip";

        public static void Inspect()
        {
            if (fcD2E != null && fcMoM != null) return;

            if (Application.platform == RuntimePlatform.OSXPlayer)
            {
                fcD2E = new FFGImport(FFGAppImport.GameType.D2E, Platform.MacOS, Game.AppData() + Path.DirectorySeparatorChar, Application.isEditor);
                fcMoM = new FFGImport(FFGAppImport.GameType.MoM, Platform.MacOS, Game.AppData() + Path.DirectorySeparatorChar, Application.isEditor);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                fcD2E = new FFGImport(FFGAppImport.GameType.D2E, Platform.Android, Game.AppData() + Path.DirectorySeparatorChar, Application.isEditor);
                fcMoM = new FFGImport(FFGAppImport.GameType.MoM, Platform.Android, Game.AppData() + Path.DirectorySeparatorChar, Application.isEditor);
            }
            else
            {
                fcD2E = new FFGImport(FFGAppImport.GameType.D2E, Platform.Windows, Game.AppData() + Path.DirectorySeparatorChar, Application.isEditor);
                fcMoM = new FFGImport(FFGAppImport.GameType.MoM, Platform.Windows, Game.AppData() + Path.DirectorySeparatorChar, Application.isEditor);
            }

            fcD2E.Inspect();
            fcMoM.Inspect();
        }

        public static bool NeedImport(string type)
        {
            if (fcD2E == null || fcMoM == null) return false;
            if (type.Equals(ValkyrieConstants.typeDescent)) return fcD2E.NeedImport();
            if (type.Equals(ValkyrieConstants.typeMom)) return fcMoM.NeedImport();
            return false;
        }

        public static bool ImportAvailable(string type)
        {
            if (fcD2E == null || fcMoM == null) return false;
            if (type.Equals(ValkyrieConstants.typeDescent)) return fcD2E.ImportAvailable();
            if (type.Equals(ValkyrieConstants.typeMom)) return fcMoM.ImportAvailable();
            return false;
        }

        /// <summary>
        /// Returns a hint path to pre-populate the file picker when the user must
        /// locate the game manually. Returns "" if no hint is available.
        /// </summary>
        public static string GetInstallHintPath()
        {
#if UNITY_STANDALONE_WIN
            try
            {
                string steamPath = (string)Microsoft.Win32.Registry.GetValue(
                    @"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", "");
                if (!string.IsNullOrEmpty(steamPath))
                {
                    string commonPath = Path.Combine(steamPath, "steamapps", "common");
                    if (Directory.Exists(commonPath)) return commonPath;
                    return steamPath;
                }
            }
            catch { }
            return "";
#elif UNITY_STANDALONE_OSX
            try
            {
                string steamCommon = Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                    "Library/Application Support/Steam/steamapps/common");
                if (Directory.Exists(steamCommon)) return steamCommon;
            }
            catch { }
            return "/Applications";
#else
            return "";
#endif
        }

        public static void Import(string type, string path, Action callback)
        {
            importError = null;
            onComplete = callback;
            Destroyer.Destroy();
            loadingScreen = new LoadingScreen(CONTENT_IMPORTING.Translate());
            importType = type;

            lastLogMessage = null;
            ValkyrieTools.ValkyrieDebug.OnLog -= OnLogMessage;
            ValkyrieTools.ValkyrieDebug.OnLog += OnLogMessage;

            importThread = null;
            if (type.Equals(ValkyrieConstants.typeDescent))
                importThread = new Thread(() => { try { fcD2E.Import(path); } catch (Exception ex) { importError = ex.Message; UnityEngine.Debug.LogException(ex); } });
            else if (type.Equals(ValkyrieConstants.typeMom))
                importThread = new Thread(() => { try { fcMoM.Import(path); } catch (Exception ex) { importError = ex.Message; UnityEngine.Debug.LogException(ex); } });

            if (importThread != null) importThread.IsBackground = true;
            importThread?.Start();
        }

        public static void ImportZip(string type, Action callback)
        {
            FileSelector.PickZipFile(zipPath => ImportZipFromPath(zipPath, type, callback));
        }

        private static void ImportZipFromPath(string zipPath, string type, Action callback)
        {
            importError = null;
            onComplete = callback;
            Destroyer.Destroy();
            loadingScreen = new LoadingScreen(CONTENT_IMPORTING.Translate());
            importType = type;

            string fileName = Path.GetFileName(zipPath);
            bool validName = false;
            string expectedFileName = "";
            if (type.Equals(ValkyrieConstants.typeDescent))
            {
                if (fileName.Equals(DESCENT_DESKTOP_ZIP, StringComparison.OrdinalIgnoreCase) ||
                    fileName.Equals(DESCENT_ANDROID_ZIP, StringComparison.OrdinalIgnoreCase))
                    validName = true;
                else
                    expectedFileName = DESCENT_DESKTOP_ZIP + " / " + DESCENT_ANDROID_ZIP;
            }
            else if (type.Equals(ValkyrieConstants.typeMom))
            {
                if (fileName.Equals(MOM_DESKTOP_ZIP, StringComparison.OrdinalIgnoreCase) ||
                    fileName.Equals(MOM_ANDROID_ZIP, StringComparison.OrdinalIgnoreCase))
                    validName = true;
                else
                    expectedFileName = MOM_DESKTOP_ZIP + " / " + MOM_ANDROID_ZIP;
            }

            if (!validName && !string.IsNullOrEmpty(expectedFileName))
            {
                importError = string.Format(new StringKey("val", "ERROR_INVALID_ZIP_NAME").Translate(), expectedFileName);
                callback?.Invoke();
                return;
            }

            try
            {
                using (Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(zipPath))
                {
                    if (!zip.ContainsEntry("import.ini"))
                    {
                        importError = new StringKey("val", "ERROR_MISSING_IMPORT_INI").Translate();
                        callback?.Invoke();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                importError = ex.Message;
                callback?.Invoke();
                return;
            }

            lastLogMessage = null;
            ValkyrieTools.ValkyrieDebug.OnLog -= OnLogMessage;
            ValkyrieTools.ValkyrieDebug.OnLog += OnLogMessage;

            string tempPath = Path.Combine(Application.temporaryCachePath, "import_extract");
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
            Directory.CreateDirectory(tempPath);

            importThread = new Thread(() =>
            {
                try
                {
                    ZipManager.ExtractZipAsync(tempPath, zipPath, ZipManager.Extract_mode.ZIPMANAGER_EXTRACT_FULL);
                    ZipManager.Wait4PreviousSave();

                    string resourcesAssets = Path.Combine(tempPath, "resources.assets");
                    bool rawAssetsFound = File.Exists(resourcesAssets);
                    if (!rawAssetsFound)
                        rawAssetsFound = Directory.GetFiles(tempPath, "*.obb", SearchOption.AllDirectories).Length > 0;

                    if (type.Equals(ValkyrieConstants.typeDescent))
                    {
                        if (rawAssetsFound)
                            fcD2E.Import(tempPath);
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
                        if (rawAssetsFound)
                            fcMoM.Import(tempPath);
                        else
                        {
                            ValkyrieDebug.Log("ZIP Import: Raw assets not found, performing direct copy.");
                            string destPath = fcMoM.path;
                            if (Directory.Exists(destPath)) Directory.Delete(destPath, true);
                            ZipManager.CopyDirectory(tempPath, destPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    importError = ex.Message;
                    UnityEngine.Debug.LogException(ex);
                }
            });

            if (importThread != null) importThread.IsBackground = true;
            importThread?.Start();
        }



        private static void OnLogMessage(string message)
        {
            // Discard overly long messages to prevent UI overflow or exceptions
            if (message != null && message.Length < 200)
            {
                lastLogMessage = message;
            }
        }

        public static void Update()
        {
            if (importThread == null) return;
            if (importThread.IsAlive)
            {
                if (lastLogMessage != null && loadingScreen != null)
                {
                    loadingScreen.UpdateDisplay(lastLogMessage);
                    lastLogMessage = null;
                }
                loadingScreen?.UpdateIndicator(UnityEngine.Time.time);
                return;
            }
            importThread = null;
            ValkyrieTools.ValkyrieDebug.OnLog -= OnLogMessage;
            loadingScreen = null;

            Action action = onComplete;
            onComplete = null;
            action?.Invoke();
        }

    }
}
