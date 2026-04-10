// unity/Assets/Scripts/UI/Screens/ImportManager.cs
using System;
using System.IO;
using System.Threading;
using Assets.Scripts.UI;
using FFGAppImport;
using UnityEngine;
using ValkyrieTools;

namespace Assets.Scripts.UI.Screens
{
    public static class ImportManager
    {
        private static FFGImport fcD2E;
        private static FFGImport fcMoM;
        private static Thread importThread;
        private static string importType = "";
        private static Action onComplete;

        private static readonly StringKey CONTENT_IMPORTING = new StringKey("val", "CONTENT_IMPORTING");

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

        public static void Import(string type, string path, Action callback)
        {
            onComplete = callback;
            Destroyer.Destroy();
            new LoadingScreen(CONTENT_IMPORTING.Translate());
            importType = type;

            importThread = null;
            if (type.Equals(ValkyrieConstants.typeDescent))
                importThread = new Thread(() => fcD2E.Import(path));
            else if (type.Equals(ValkyrieConstants.typeMom))
                importThread = new Thread(() => fcMoM.Import(path));
            importThread?.Start();
        }

        public static void ImportZip(string type, Action callback)
        {
            FileSelector.PickZipFile(zipPath => ImportZipFromPath(zipPath, type, callback));
        }

        private static void ImportZipFromPath(string zipPath, string type, Action callback)
        {
            onComplete = callback;
            Destroyer.Destroy();
            new LoadingScreen(CONTENT_IMPORTING.Translate());
            importType = type;

            string tempPath = Path.Combine(Application.temporaryCachePath, "import_extract");
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
            Directory.CreateDirectory(tempPath);

            importThread = new Thread(() =>
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
            });
            importThread?.Start();
        }

        public static void Update()
        {
            if (importThread == null) return;
            if (importThread.IsAlive) return;
            importThread = null;
            ExtractBundles();
            Action action = onComplete;
            onComplete = null;
            action?.Invoke();
        }

        private static void ExtractBundles()
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
    }
}
