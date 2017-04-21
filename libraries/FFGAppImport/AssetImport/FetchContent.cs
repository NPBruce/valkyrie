using System.Collections;
using System.Collections.Generic;
using Unity_Studio;
using System.IO;
using ValkyrieTools;

namespace FFGAppImport
{

    // Used to import content from FFG apps
    public class FetchContent
    {
        FFGImport importData;
        List<AssetsFile> assetFiles; // All asset files
        string gameType;
        string contentPath;
        AppFinder finder = null;
        public bool importAvailable;
        string logFile;

        // Set up and check availability
        //public FetchContent(GameType type, string path)
        public FetchContent(FFGImport import)
        {
            importData = import;
            contentPath = import.path;
            if (import.type == GameType.D2E)
            {
                finder = new RtLFinder(import.platform);
                gameType = "D2E";
            }
            else if (import.type == GameType.MoM)
            {
                finder = new MoMFinder(import.platform);
                gameType = "MoM";
            }
            else
            {
                return;
            }

            string appVersion = finder.RequiredFFGVersion();
            // Open up the assets to get the actual version number
            string ffgVersion = fetchAppVersion();

            // Add version found to log
            if (ffgVersion.Length != 0)
            {
                ValkyrieDebug.Log("FFG " + gameType + " Version Found: " + ffgVersion + System.Environment.NewLine);
            }
            else
            {
                ValkyrieDebug.Log("FFG " + gameType + " not found." + System.Environment.NewLine);
            }

            // Check if version is acceptable for import
            importAvailable = VersionNewerOrEqual(appVersion, ffgVersion);
        }

        // Check if an import is required
        public bool NeedImport()
        {
            // Read the import log
            logFile = contentPath + gameType + "/ffg/import.ini";
            IniData log = IniRead.ReadFromIni(logFile);

            // If no import log, import is required
            if (log == null)
            {
                return true;
            }

            bool appVersionOK = false;
            bool valkVersionOK = false;

            // Check that the FFG app version in import is new enough
            string lastImport = log.Get("Import", "FFG");
            appVersionOK = VersionNewerOrEqual(finder.RequiredFFGVersion(), lastImport);

            // Check that the Valkyrie version in import is new enough
            int lastValkVersion = 0;
            int.TryParse(log.Get("Import", "Valkyrie"), out lastValkVersion);
            valkVersionOK = (FFGImport.version == lastValkVersion);
            return !appVersionOK || !valkVersionOK;
        }

        // Determine FFG app version
        public string fetchAppVersion()
        {
            string appVersion = "";
            if (!File.Exists(finder.location + "/resources.assets"))
            {
                ValkyrieDebug.Log("Could not find main assets file: " + finder.location + "/resources.assets");
            }
            try
            {
                // We assume that the version asset is in resources.assets
                AssetsFile assetsFile = new AssetsFile(finder.location + "/resources.assets", new EndianStream(File.OpenRead(finder.location + "/resources.assets"), EndianType.BigEndian));

                // Look through all listed assets
                foreach (var asset in assetsFile.preloadTable.Values)
                {
                    // Check all text assets
                    if (asset.Type2 == 49) //TextAsset
                    {
                        // Load as text
                        Unity_Studio.TextAsset m_TextAsset = new Unity_Studio.TextAsset(asset, false);
                        // Check if called _version
                        if (asset.Text.Equals("_version"))
                        {
                            // Read asset content
                            m_TextAsset = new Unity_Studio.TextAsset(asset, true);
                            // Extract version
                            appVersion = System.Text.Encoding.UTF8.GetString(m_TextAsset.m_Script);
                        }
                    }
                }

                // Trim content from #
                if (appVersion.IndexOf("#") != -1)
                {
                    appVersion = appVersion.Substring(0, appVersion.IndexOf("#"));
                }
            }
            catch (System.Exception) { }

            return appVersion;
        }

        // Import from app
        public void Import()
        {
            // List all assets files
            string[] assetFiles = Directory.GetFiles(finder.location, "*.assets");

            // Attempt to clean up old import
            if (!CleanImport()) return;
            // Import from all assets files
            foreach (string s in assetFiles)
            {
                Import(s);
            }
        }

        // Import one assets file
        public void Import(string assetFile)
        {
            List<string> unityFiles = new List<string>(); //files to load

            // read file
            AssetsFile assetsFile = new AssetsFile(assetFile, new EndianStream(File.OpenRead(assetFile), EndianType.BigEndian));
            // Legacy assets not supported, shouldn't be old
            if (assetsFile.fileGen < 15)
            {
                ValkyrieDebug.Log("Invalid asset file: " + assetFile);
                return;
            }

            // Loop through all assets listed in file
            // Fixme: I don't think we need to do this as we are importing from all files anyway
            foreach (var sharedFile in assetsFile.sharedAssetsList)
            {
                // Some listed assets are in other assets files
                string sharedFilePath = finder.location + "/" + sharedFile.fileName;
                string sharedFileName = Path.GetFileName(sharedFile.fileName);

                // find assets file
                var quedSharedFile = unityFiles.Find(uFile => string.Equals(Path.GetFileName(uFile), sharedFileName, System.StringComparison.OrdinalIgnoreCase));
                if (quedSharedFile == null)
                {
                    if (!File.Exists(sharedFilePath))
                    {
                        var findFiles = Directory.GetFiles(Path.GetDirectoryName(assetFile), sharedFileName, SearchOption.AllDirectories);
                        if (findFiles.Length > 0) { sharedFilePath = findFiles[0]; }
                    }

                    if (File.Exists(sharedFilePath))
                    {
                        //this would get screwed if the file somehow fails to load
                        sharedFile.Index = unityFiles.Count;
                        unityFiles.Add(sharedFilePath);
                    }
                }
                else { sharedFile.Index = unityFiles.IndexOf(quedSharedFile); }
            }

            // Make assets from string list
            assetFiles = new List<AssetsFile>();
            foreach (string s in unityFiles)
            {
                AssetsFile file = new AssetsFile(s, new EndianStream(File.OpenRead(s), EndianType.BigEndian));
                assetFiles.Add(file);
            }

            // Get all asset content
            BuildAssetStrucutres();
            // Write log
            WriteImportLog(logFile);
        }

        // Write log of import
        private void WriteImportLog(string logFile)
        {
            string[] log = new string[3];
            log[0] = "[Import]";
            log[1] = "Valkyrie=" + FFGImport.version;
            log[2] = "FFG=" + fetchAppVersion();
            // Write out data
            try
            {
                Directory.CreateDirectory(contentPath);
                Directory.CreateDirectory(contentPath + gameType);
                Directory.CreateDirectory(contentPath + gameType + "/ffg");

                logFile = contentPath + gameType + "/ffg/import.ini";

                if (File.Exists(logFile))
                {
                    File.Delete(logFile);
                }
                File.WriteAllLines(logFile, log);
            }
            catch (System.Exception)
            {
                ValkyrieDebug.Log("Warning: Unable to create import log");
            }
        }

        //Clean old fetched data
        private bool CleanImport()
        {
            if (!Directory.Exists(contentPath + gameType + "/ffg")) return true;
            try
            {
                Directory.Delete(contentPath + gameType + "/ffg", true);
            }
            catch (System.Exception)
            {
                ValkyrieDebug.Log("Warning: Unable to remove temporary files.");
                return false;
            }
            if (Directory.Exists(contentPath + gameType + "/ffg")) return false;
            return true;
        }

        // Import asset contents
        private void BuildAssetStrucutres()
        {
            // All asset files connected to the one openned
            foreach (AssetsFile file in assetFiles)
            {
                // All assets
                foreach (var asset in file.preloadTable.Values)
                {
                    switch (asset.Type2)
                    {
                        //case 1: //GameObject
                        //case 4: //Transform
                        //case 224: //RectTransform
                        //case 21: //Material

                        case 28: //Texture2D
                            {
                                ExportTexture(asset);
                                break;
                            }
                        //case 48: //Shader
                        case 49: //TextAsset
                            {
                                ExportText(asset);
                                break;
                            }
                        case 83: //AudioClip
                            {
                                ExportAudioClip(asset);
                                break;
                            }
                        //case 89: //CubeMap
                        case 128: //Font
                            {
                                ExportFont(asset);
                                break;
                            }
                        //case 129: //PlayerSettings
                        case 0:
                            break;

                    }
                }
            }
        }

        // Save texture to disk
        private void ExportTexture(Unity_Studio.AssetPreloadData asset)
        {
            Unity_Studio.Texture2D m_Texture2D = new Unity_Studio.Texture2D(asset, false);
            m_Texture2D = new Unity_Studio.Texture2D(asset, true);
            Directory.CreateDirectory(contentPath);
            Directory.CreateDirectory(contentPath + gameType);
            Directory.CreateDirectory(contentPath + gameType + "/ffg");
            Directory.CreateDirectory(contentPath + gameType + "/ffg/img");
            // Default file name
            string fileCandidate = contentPath + gameType + "/ffg/img/" + asset.Text;
            string fileName = fileCandidate + asset.extension;
            // This should apend a postfix to the name to avoid collisions, but as we import multiple times
            // This is broken
            while (File.Exists(fileName))
            {
                return;// Fixme;
            }

            switch (m_Texture2D.m_TextureFormat)
            {
                #region DDS
                case 1: //Alpha8
                case 2: //A4R4G4B4
                case 3: //B8G8R8 //confirmed on X360, iOS //PS3 unsure
                case 4: //G8R8A8B8 //confirmed on X360, iOS
                case 5: //B8G8R8A8 //confirmed on X360, PS3, Web, iOS
                case 7: //R5G6B5 //confirmed switched on X360; confirmed on iOS
                case 10: //DXT1
                case 12: //DXT5
                case 13: //R4G4B4A4, iOS (only?)
                    using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
                    {
                        // We have to manually add a header because unity doesn't have it
                        writer.Write(0x20534444);
                        writer.Write(0x7C);
                        writer.Write(m_Texture2D.dwFlags);
                        writer.Write(m_Texture2D.m_Height);
                        writer.Write(m_Texture2D.m_Width);
                        writer.Write(m_Texture2D.dwPitchOrLinearSize); //should be main tex size without mips);
                        writer.Write((int)0); //dwDepth not implemented
                        writer.Write(m_Texture2D.dwMipMapCount);
                        writer.Write(new byte[44]); //dwReserved1[11]
                        writer.Write(m_Texture2D.dwSize);
                        writer.Write(m_Texture2D.dwFlags2);
                        writer.Write(m_Texture2D.dwFourCC);
                        writer.Write(m_Texture2D.dwRGBBitCount);
                        writer.Write(m_Texture2D.dwRBitMask);
                        writer.Write(m_Texture2D.dwGBitMask);
                        writer.Write(m_Texture2D.dwBBitMask);
                        writer.Write(m_Texture2D.dwABitMask);
                        writer.Write(m_Texture2D.dwCaps);
                        writer.Write(m_Texture2D.dwCaps2);
                        writer.Write(new byte[12]); //dwCaps3&4 & dwReserved2

                        // Write image data
                        writer.Write(m_Texture2D.image_data);
                        writer.Close();
                    }
                    break;
                #endregion
                #region PVR
                case 30: //PVRTC_RGB2
                case 31: //PVRTC_RGBA2
                case 32: //PVRTC_RGB4
                case 33: //PVRTC_RGBA4
                case 34: //ETC_RGB4
                    using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
                    {
                        // We have to manually add a header because unity doesn't have it
                        writer.Write(m_Texture2D.pvrVersion);
                        writer.Write(m_Texture2D.pvrFlags);
                        writer.Write(m_Texture2D.pvrPixelFormat);
                        writer.Write(m_Texture2D.pvrColourSpace);
                        writer.Write(m_Texture2D.pvrChannelType);
                        writer.Write(m_Texture2D.m_Height);
                        writer.Write(m_Texture2D.m_Width);
                        writer.Write(m_Texture2D.pvrDepth);
                        writer.Write(m_Texture2D.pvrNumSurfaces);
                        writer.Write(m_Texture2D.pvrNumFaces);
                        writer.Write(m_Texture2D.dwMipMapCount);
                        writer.Write(m_Texture2D.pvrMetaDataSize);

                        // Write image data
                        writer.Write(m_Texture2D.image_data);
                        writer.Close();
                    }
                    break;
                #endregion
                case 28: //DXT1 Crunched
                case 29: //DXT1 Crunched
                default:
                    using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
                    {
                        writer.Write(m_Texture2D.image_data);
                        writer.Close();
                    }
                    break;
            }
        }

        // Save audio to file
        private void ExportAudioClip(Unity_Studio.AssetPreloadData asset)
        {
            Unity_Studio.AudioClip m_AudioClip = new Unity_Studio.AudioClip(asset, false);
            Directory.CreateDirectory(contentPath);
            Directory.CreateDirectory(contentPath + gameType);
            Directory.CreateDirectory(contentPath + gameType + "/ffg");
            Directory.CreateDirectory(contentPath + gameType + "/ffg/audio");

            string fileCandidate = contentPath + gameType + "/ffg/audio/" + asset.Text;
            string fileName = fileCandidate + ".ogg";
            // This should apend a postfix to the name to avoid collisions, but as we import multiple times
            // This is broken
            while (File.Exists(fileName))
            {
                return;// Fixme;
            }

            // Pass to FSB Export
            m_AudioClip = new Unity_Studio.AudioClip(asset, true);
            FSBExport.Write(m_AudioClip.m_AudioData, fileName);
        }

        // Save text to file
        private void ExportText(Unity_Studio.AssetPreloadData asset)
        {
            Unity_Studio.TextAsset m_TextAsset = new Unity_Studio.TextAsset(asset, false);
            Directory.CreateDirectory(contentPath);
            Directory.CreateDirectory(contentPath + gameType);
            Directory.CreateDirectory(contentPath + gameType + "/ffg");
            Directory.CreateDirectory(contentPath + gameType + "/ffg/text");
            string fileCandidate = contentPath + gameType + "/ffg/text/" + asset.Text;
            string fileName = fileCandidate + asset.extension;

            m_TextAsset = new Unity_Studio.TextAsset(asset, true);
            // This should apend a postfix to the name to avoid collisions, but as we import multiple times
            // This is broken
            while (File.Exists(fileName))
            {
                return;// Fixme;
            }

            // Write to disk
            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                // Pass the Deobfuscate key to decrypt
                writer.Write(m_TextAsset.Deobfuscate(finder.ObfuscateKey()));
                writer.Close();
            }

            // Run monster data extration tool if in dev
            if (importData.editor && asset.Text.Equals("Localization"))
            {
                if (finder is MoMFinder)
                {
                    ExtractDataTool.MoM(m_TextAsset.Deobfuscate(finder.ObfuscateKey()), contentPath);
                }
            }
        }

        // Save TTF font to dist
        private void ExportFont(Unity_Studio.AssetPreloadData asset)
        {
            Unity_Studio.unityFont m_Font = new Unity_Studio.unityFont(asset, false);
            Directory.CreateDirectory(contentPath);
            Directory.CreateDirectory(contentPath + gameType);
            Directory.CreateDirectory(contentPath + gameType + "/ffg");
            Directory.CreateDirectory(contentPath + gameType + "/ffg/fonts");
            string fileCandidate = contentPath + gameType + "/ffg/fonts/" + asset.Text;
            string fileName = fileCandidate + ".ttf";

            m_Font = new Unity_Studio.unityFont(asset, true);

            if (m_Font.m_FontData == null)
            {
                return;
            }

            // This should apend a postfix to the name to avoid collisions, but as we import multiple times
            // This is broken
            while (File.Exists(fileName))
            {
                return;// Fixme;
            }

            // Write to disk
            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                writer.Write(m_Font.m_FontData);
                writer.Close();
            }
        }

        // Test version of the form a.b.c is newer or equal
        public static bool VersionNewerOrEqual(string oldVersion, string newVersion)
        {
            string oldS = System.Text.RegularExpressions.Regex.Replace(oldVersion, "[^0-9]", "");
            string newS = System.Text.RegularExpressions.Regex.Replace(newVersion, "[^0-9]", "");
            // If numbers are the same they are equal
            if (oldS.Equals(newS)) return true;
            return VersionNewer(oldVersion, newVersion);
        }

        // Test version of the form a.b.c is newer
        public static bool VersionNewer(string oldVersion, string newVersion)
        {
            // Split into components
            string[] oldV = oldVersion.Split('.');
            string[] newV = newVersion.Split('.');

            if (newVersion.Equals("")) return false;

            if (oldVersion.Equals("")) return true;

            // Different number of components
            if (oldV.Length != newV.Length)
            {
                return true;
            }
            // Check each component
            for (int i = 0; i < oldV.Length; i++)
            {
                // Strip for only numbers
                string oldS = System.Text.RegularExpressions.Regex.Replace(oldV[i], "[^0-9]", "");
                string newS = System.Text.RegularExpressions.Regex.Replace(newV[i], "[^0-9]", "");
                try
                {
                    if (int.Parse(oldS) < int.Parse(newS))
                    {
                        return true;
                    }
                    if (int.Parse(oldS) > int.Parse(newS))
                    {
                        return false;
                    }
                }
                catch (System.Exception)
                {
                    return true;
                }
            }
            return false;
        }
    }
}