using System.Collections.Generic;
using AssetStudio;
using System;
using System.Linq;
using System.IO;
using ValkyrieTools;
using System.Text.RegularExpressions;
using System.Globalization;

namespace FFGAppImport
{

    // Used to import content from FFG apps
    public class FetchContent
    {
        FFGImport importData;
        string gameType;
        string contentPath;
        AppFinder finder = null;
        public bool importAvailable;
        string logFile;
        AssetsManager assetsManager;

        // Set up and check availability
        //public FetchContent(GameType type, string path)
        public FetchContent(FFGImport import)
        {
            importData = import;
            contentPath = import.path;
            assetsManager = new AssetsManager(this);
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
            else if (import.type == GameType.IA)
            {
                finder = new IAFinder(import.platform);
                gameType = "IA";
            }
            else
            {
                return;
            }

            string appVersion = finder.RequiredFFGVersion();
            // Open up the assets to get the actual version number
            string ffgVersion = FetchAppVersion();

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

            if (importData.platform == Platform.Android)
            {
                try
                {
                    importAvailable = File.Exists(finder.ObbPath());
                }
                catch (Exception ex)
                {
                    ValkyrieDebug.Log("FetchContent caused " + ex.GetType().Name + ": " + ex.Message + " " + ex.StackTrace);
                }
                if (!importAvailable && import.apkPath.Length != 0)
                {
                    importAvailable = true;
                }
            }
        }

        // Check if an import is required
        public bool NeedImport()
        {
            // Read the import log
            logFile = Path.Combine(contentPath, "import.ini");
            ValkyrieDebug.Log("Import log file: " + logFile);
            // If no import log, import is required
            if (!File.Exists(logFile))
            {
                return true;
            }
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
        public string FetchAppVersion()
        {
            string appVersion = "";
            var versionAssetsManager = new AssetsManager();
            try
            {
                if (importData.platform == Platform.Android)
                {
                    appVersion = finder.ExtractObbVersion();
                    if (appVersion != "")
                    {
                        return appVersion;
                    }
                    else if(importData.packageVersion != "")
                    {
                        return importData.packageVersion;
                    }

                }
                string resourcesAssets = Path.Combine(finder.location, "resources.assets");
                if (!File.Exists(resourcesAssets))
                {
                    ValkyrieDebug.Log("Could not find main assets file: " + resourcesAssets);
                    return appVersion;
                }

                ValkyrieDebug.Log("Going to look in " + resourcesAssets);
                // We assume that the version asset is in resources.assets
                //using (var fs = new AssetStudio.FileReader(resourcesAssets, File.OpenRead(resourcesAssets)))
                {
                    ValkyrieDebug.Log("Opening " + resourcesAssets);
                    //var endianStream = new SerializedFile(fs, new AssetsManager());
                    {
                        //var assetsFile = new AssetsFile(resourcesAssets, endianStream);
                        //var objectinfo = new ObjectInfo();
                        //var assetsFile = new ObjectReader(fs, endianStream, objectinfo);
                        ValkyrieDebug.Log("Opening assets file" + resourcesAssets);
                        ValkyrieDebug.Log("Opening assets file" + resourcesAssets);

                        //var assetsManager = new AssetsManager();
                        versionAssetsManager.LoadFiles(new[] { resourcesAssets });
                        // Look through all listed assets
                        foreach (var assetsFile in versionAssetsManager.assetsFileList)
                        {
                            foreach (var asset in assetsFile.Objects)
                            {
                                //ValkyrieDebug.Log("Looking at " + asset + " Asset");
                                // Check all text assets
                                if (!(asset is AssetStudio.TextAsset)) //TextAsset
                                {
                                    continue;
                                }

                                // Check if called _version
                                if (((NamedObject)asset).m_Name.Equals("_version"))
                                {
                                    // Read asset content
                                    var textAsset = (AssetStudio.TextAsset)asset;
                                    // Extract version

                                    appVersion = System.Text.Encoding.UTF8.GetString(textAsset.m_Script);
                                    break;
                                }
                            }
                        }
                    }
                }

                // Trim content from #
                if (appVersion.IndexOf("#") != -1)
                {
                    appVersion = appVersion.Substring(0, appVersion.IndexOf("#"));
                }
            }
            catch(Exception ex)
            {
                ValkyrieDebug.Log("fetchAppVersion caused " + ex.GetType().Name + ": " + ex.Message + " " + ex.StackTrace);
            }

            return appVersion;
        }

        // Import from app
        public void Import(string path)
        {
            ValkyrieDebug.Log("Importing for path: " + path);
            if (path != null)
            {
                finder.location = path;
            }
            try
            {
                finder.apkPath = importData.apkPath;
                // Does nothing, if we aren't on Android
                if (!finder.ExtractObb())
                {
                    finder.ExtractApk();
                }

                // Attempt to clean up old import
                if (!CleanImport()) return;

                // Get all assets 
                GetAssetFiles();

                // Export all asset content
                //BuildAssetStructures();

                // Write log
                WriteImportLog();

                // Find any streaming asset files
                ImportStreamAssets();
            }
            catch (Exception ex)
            {
                ValkyrieDebug.Log("Import caused " + ex.GetType().Name + ": " + ex.Message + " " + ex.StackTrace);
            }
        }

        /// <summary>
        /// Get all available assets in the right load order.
        /// </summary>
        /// <remarks>This is an expensive method, don't call it too often, use a reference.</remarks>
        /// <returns>All available assets in the right load order. Never <c>null</c>.</returns>
        private void GetAssetFiles()
        {
            assetsManager.LoadFolder(finder.location);

            if(importData.platform == Platform.Android)
            {
                try
                {
                                assetsManager.LoadFolder(finder.DataPath());
                }
                catch (Exception ex)
                {
                    ValkyrieDebug.Log("GetAssetFiles caused " + ex.GetType().Name + ": " + ex.Message + " " + ex.StackTrace);
                }
                if (Directory.Exists(finder.AuxDataPath()))
                {
                     assetsManager.LoadFolder(finder.AuxDataPath());
                }
            }

        }

        // Write log of import
        private void WriteImportLog()
        {
            // Write out data
            try
            {
                string version = FetchAppVersion();
                Directory.CreateDirectory(contentPath);
                logFile = Path.Combine(contentPath, "import.ini");
                if (File.Exists(logFile))
                {
                    File.Delete(logFile);
                }
                string[] log = 
                        {
                            "[Import]",
                            "Valkyrie=" + FFGImport.version,
                            "FFG=" + version
                        };
                File.WriteAllLines(logFile, log);
            }
            catch
            {
                ValkyrieDebug.Log("Warning: Unable to create import log");
            }
        }

        //Clean old fetched data
        private bool CleanImport()
        {
            if (!Directory.Exists(contentPath)) return true;
            try
            {
                Directory.Delete(contentPath, true);
            }
            catch
            {
                ValkyrieDebug.Log("Warning: Unable to remove temporary files.");
                return false;
            }
            return !Directory.Exists(contentPath);
        }

        public void ImportAssetPreloadData(AssetStudio.Object assetPreloadData)
        {
            if (assetPreloadData == null) throw new ArgumentNullException("assetPreloadData");

            switch (assetPreloadData)
            {
                //case 1: //GameObject
                //case 4: //Transform
                //case 224: //RectTransform
                //case 21: //Material
                case AssetStudio.Texture2D a1: //Texture2D
                    {
                        ExportTexture(assetPreloadData);
                        break;
                    }
                //case 48: //Shader
                case AssetStudio.TextAsset a2: //TextAsset
                    {
                        ExportText(assetPreloadData);
                        break;
                    }
                case AssetStudio.AudioClip a3: //AudioClip
                    {
                        ExportAudioClip(assetPreloadData);
                        break;
                    }
                //case 89: //CubeMap
                case AssetStudio.Font a4: //Font
                    {
                        // There have been issues with android font exports in the past, failing the font export is not fatal.
                        try
                        {
                            ExportFont(assetPreloadData);
                        }
                        catch (Exception e)
                        {
                            ValkyrieDebug.Log("Font export error: '" + e.Message);
                        }
                        break;
                    }
                //case 129: //PlayerSettings
                default:
                    break;
            }
        }

        // Save texture to disk
        private void ExportTexture(AssetStudio.Object asset)
        {
            if (asset == null)
                throw new ArgumentNullException("asset");
            AssetStudio.Texture2D texture2D = (AssetStudio.Texture2D) asset;
            //texture2D = new Unity_Studio.Texture2D(asset, true);
            NamedObject namedAsset = ((NamedObject)asset);
            string imgPath = Path.Combine(contentPath, "img");
            Directory.CreateDirectory(imgPath);
            // Default file name
            string fileCandidate = Path.Combine(imgPath, namedAsset.m_Name);

            switch (texture2D.m_TextureFormat)
            {
                #region DDS
                case TextureFormat.Alpha8: //Alpha8
                case TextureFormat.ARGB4444: //A4R4G4B4
                case TextureFormat.BGR24: //B8G8R8 //confirmed on X360, iOS //PS3 unsure
                case TextureFormat.RGB24:
                case TextureFormat.RGBA32: //G8R8A8B8 //confirmed on X360, iOS
                case TextureFormat.BGRA32: //B8G8R8A8 //confirmed on X360, PS3, Web, iOS
                case TextureFormat.RGB565: //R5G6B5 //confirmed switched on X360; confirmed on iOS
                case TextureFormat.DXT1: //DXT1
                case TextureFormat.DXT5: //DXT5
                case TextureFormat.RGBA4444: //R4G4B4A4, iOS (only?)
                    // This should append a postfix to the name to avoid collisions
                    //if (!IsAvailableFileName(fileCandidate, ".dds")) return;
                    string fileName = GetAvailableFileName(fileCandidate, ".dds");
                    ValkyrieDebug.Log("ExportTexture: '" + fileName + "' format: '" + texture2D.m_TextureFormat + "'");
                    using (var fs = File.Open(fileName, FileMode.Create))
                    {
                        using (var writer = new BinaryWriter(fs))
                        {
                            // We have to manually add a header because unity doesn't have it
                            writer.Write(0x20534444);
                            writer.Write(0x7C);
                            writer.Write(texture2D.dwFlags);
                            writer.Write(texture2D.m_Height);
                            writer.Write(texture2D.m_Width);
                            writer.Write(texture2D.dwPitchOrLinearSize); //should be main tex size without mips);
                            writer.Write((int)0); //dwDepth not implemented
                            writer.Write(texture2D.dwMipMapCount);
                            writer.Write(new byte[44]); //dwReserved1[11]
                            writer.Write(texture2D.dwSize);
                            writer.Write(texture2D.dwFlags2);
                            writer.Write(texture2D.dwFourCC);
                            writer.Write(texture2D.dwRGBBitCount);
                            writer.Write(texture2D.dwRBitMask);
                            writer.Write(texture2D.dwGBitMask);
                            writer.Write(texture2D.dwBBitMask);
                            writer.Write(texture2D.dwABitMask);
                            writer.Write(texture2D.dwCaps);
                            writer.Write(texture2D.dwCaps2);
                            writer.Write(new byte[12]); //dwCaps3&4 & dwReserved2

                            // Write image data
                            writer.Write(texture2D.image_data_bytes);
                        }
                    }
                    break;
                #endregion
                #region PVR
                case TextureFormat.PVRTC_RGB2: //PVRTC_RGB2
                case TextureFormat.PVRTC_RGBA2: //PVRTC_RGBA2
                case TextureFormat.PVRTC_RGB4: //PVRTC_RGB4
                case TextureFormat.PVRTC_RGBA4: //PVRTC_RGBA4
                case TextureFormat.ETC_RGB4: //ETC_RGB4
                case TextureFormat.ETC2_RGBA8: //ETC2_RGBA8
                case TextureFormat.ETC2_RGB:
                    // We put the image data in a PVR container. See the specification for details
                    // http://cdn.imgtec.com/sdk-documentation/PVR+File+Format.Specification.pdf
                    // This should append a postfix to the name to avoid collisions
                    //if (!IsAvailableFileName(fileCandidate, ".pvr")) return;
                    fileName = GetAvailableFileName(fileCandidate, ".pvr");
                    ValkyrieDebug.Log("ExportTexture: '" + fileName + "' format: '" + texture2D.m_TextureFormat + "'");

                    using (var fs = File.Open(fileName, FileMode.Create))
                    {
                        using (var writer = new BinaryWriter(fs))
                        {
                            // We have to manually add a header because unity doesn't have it
                            writer.Write(texture2D.pvrVersion);
                            writer.Write(texture2D.pvrFlags);
                            writer.Write(texture2D.pvrPixelFormat);
                            writer.Write(texture2D.pvrColourSpace);
                            writer.Write(texture2D.pvrChannelType);
                            writer.Write(texture2D.m_Height);
                            writer.Write(texture2D.m_Width);
                            writer.Write(texture2D.pvrDepth);
                            writer.Write(texture2D.pvrNumSurfaces);
                            writer.Write(texture2D.pvrNumFaces);
                            writer.Write(texture2D.dwMipMapCount);
                            writer.Write(texture2D.pvrMetaDataSize);

                            // Write image data
                            writer.Write(texture2D.image_data_bytes);
                        }
                    }
                    break;
                #endregion
                case TextureFormat.DXT1Crunched: //DXT1 Crunched
                case TextureFormat.DXT5Crunched: //DXT1 Crunched
                default:
                    // This should append a postfix to the name to avoid collisions
                    //if (!IsAvailableFileName(fileCandidate, ".tex")) return;
                    fileName = GetAvailableFileName(fileCandidate, ".tex");
                    ValkyrieDebug.Log("ExportTexture: '" + fileName + "' format: '" + texture2D.m_TextureFormat + "'");

                    File.WriteAllBytes(fileName, texture2D.image_data_bytes);

                    break;
            }
        }

        // Save audio to file
        private void ExportAudioClip(AssetStudio.Object asset)
        {
            if (asset == null) throw new ArgumentNullException("asset");
            NamedObject namedAsset = ((NamedObject)asset);
            //var audioClip = new Unity_Studio.AudioClip(asset, false);
            string audioPath = Path.Combine(contentPath, "audio");
            Directory.CreateDirectory(audioPath);

            string fileCandidate = Path.Combine(audioPath, namedAsset.m_Name);

            // This should apends a postfix to the name to avoid collisions
            //if (!IsAvailableFileName(fileCandidate, ".ogg")) return;
            string fileName = GetAvailableFileName(fileCandidate, ".ogg");

            // Pass to FSB Export
            AssetStudio.AudioClip audioClip = (AssetStudio.AudioClip)asset;
            FSBExport.Write(audioClip.m_AudioData.GetData(), fileName);
        }

        // Save text to file
        private void ExportText(AssetStudio.Object asset)
        {
            if (asset == null) throw new ArgumentNullException("asset");
            NamedObject namedAsset = ((NamedObject)asset);
            var textAsset = (AssetStudio.TextAsset)asset;
            string textPath = Path.Combine(contentPath, "text");
            Directory.CreateDirectory(textPath);
            string fileCandidate = Path.Combine(textPath, namedAsset.m_Name);

            //textAsset = new Unity_Studio.TextAsset(asset, true);

            // This should apends a postfix to the name to avoid collisions
            //if (!IsAvailableFileName(fileCandidate, ".txt")) return;
            string fileName = GetAvailableFileName(fileCandidate, ".txt");

            // Write to disk, pass the Deobfuscate key to decrypt
            File.WriteAllBytes(fileName, textAsset.Deobfuscate(finder.ObfuscateKey()));

            // Need to trim new lines from old D2E format
            if (namedAsset.m_Name.Equals("Localization"))
            {
                string[] locFix = File.ReadAllLines(fileName);
                var locOut = new List<string>();
                string currentLine = "";
                for (int i = 0; i < locFix.Length; i++)
                {
                    if (locFix[i].Split('\"').Length % 2 == 0)
                    {
                        if (currentLine.Length == 0)
                        {
                            currentLine = locFix[i];
                        }
                        else
                        {
                            locOut.Add(currentLine + "\\n" + locFix[i]);
                            currentLine = "";
                        }
                    }
                    else
                    {
                        if (currentLine.Length == 0)
                        {
                            locOut.Add(locFix[i]);
                        }
                        else
                        {
                            currentLine += "\\n" + locFix[i];
                        }
                    }
                }
                File.WriteAllLines(fileName, locOut.ToArray());
            }
        }

        // Save TTF font to dist
        private void ExportFont(AssetStudio.Object asset)
        {
            if (asset == null) throw new ArgumentNullException("asset");
            NamedObject namedAsset = ((NamedObject)asset);
            //var font = new Unity_Studio.unityFont(asset, false);
            AssetStudio.Font font = (AssetStudio.Font)asset;
            string fontsPath = Path.Combine(contentPath, "fonts");
            Directory.CreateDirectory(fontsPath);
            string fileCandidate = Path.Combine(fontsPath, namedAsset.m_Name);

            //font = new Unity_Studio.unityFont(asset, true);

            if (font.m_FontData == null)
            {
                ValkyrieDebug.Log("ERROR ExportFont: '" + namedAsset.m_Name);
                return;
            }

            // This should apends a postfix to the name to avoid collisions
            //if (!IsAvailableFileName(fileCandidate, ".ttf")) return;
            string fileName = GetAvailableFileName(fileCandidate, ".ttf");

            // Write to disk
            File.WriteAllBytes(fileName, font.m_FontData);
        }

        // Test version of the form a.b.c is newer or equal
        public static bool VersionNewerOrEqual(string oldVersion, string newVersion)
        {
            string oldS = Regex.Replace(oldVersion, "[^0-9]", "");
            string newS = Regex.Replace(newVersion, "[^0-9]", "");
            // If numbers are the same they are equal
            if (oldS.Equals(newS)) return true;
            return VersionNewer(oldVersion, newVersion);
        }

        // Test version of the form a.b.c is newer
        public static bool VersionNewer(string oldVersion, string newVersion)
        {
            if (oldVersion == null) throw new ArgumentNullException("oldVersion");
            if (newVersion == null) throw new ArgumentNullException("newVersion");
            if (newVersion.Equals(string.Empty, StringComparison.Ordinal)) return false;
            if (oldVersion.Equals(string.Empty, StringComparison.Ordinal)) return true;

            // Split into components
            string[] oldV = oldVersion.Split('.');
            string[] newV = newVersion.Split('.');

            // Different number of components
            if (oldV.Length != newV.Length) return true;
            
            // Check each component
            for (int i = 0; i < oldV.Length; i++)
            {
                // Strip for only numbers
                string oldS = Regex.Replace(oldV[i], "[^0-9]", "");
                string newS = Regex.Replace(newV[i], "[^0-9]", "");
                try
                {
                    if (int.Parse(oldS, CultureInfo.InvariantCulture) < int.Parse(newS, CultureInfo.InvariantCulture)) return true;
                    if (int.Parse(oldS, CultureInfo.InvariantCulture) > int.Parse(newS, CultureInfo.InvariantCulture)) return false;
                }
                catch
                {
                    return true;
                }
            }
            return false;
        }
        
        private void ImportStreamAssets()
        {
            string streamDir = Path.Combine(finder.location, "StreamingAssets");
            if (Directory.Exists(streamDir))
            {
                var streamFiles = Directory.GetFiles(streamDir, "*", SearchOption.AllDirectories).ToList();
                // sort it because Directory.GetFiles() is not guaranteed to return it sorted
                streamFiles.Sort();
                ImportStreamAssets(streamFiles);
            }
            else
            {
                ValkyrieDebug.Log("StreamingAssets dir '" + streamDir + "' not found");
            }
        }

        /// <summary>
        /// Find asset bundles and create a list of them in a file.  Invalid files ignored.
        /// </summary>
        /// <param name="streamFiles"></param>
        protected void ImportStreamAssets(IEnumerable<string> streamFiles)
        {
            if (streamFiles == null) throw new ArgumentNullException("streamFiles");
            var bundles = new List<string>();

            foreach (string file in streamFiles)
            {
                try
                {
                    string header = null;
                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[8];
                        fs.Read(buffer, 0, buffer.Length);
                        header = System.Text.Encoding.Default.GetString(buffer);
                    }
                    if (header.StartsWith("UnityFS"))
                    {
                        bundles.Add(file);
                    }
                }
                catch (Exception ex)
                {
                    ValkyrieDebug.Log("ImportStreamAssets on file '" + file + "' caused " + ex.GetType().Name + ": " + ex.Message + " " + ex.StackTrace);
                    continue;
                }
            }

            // We can't extract these here because this isn't the main thread and unity doesn't handle that
            string bundlesFile = Path.Combine(contentPath, "bundles.txt");
            ValkyrieDebug.Log("Writing '" + bundlesFile + "' containing " + bundles.Count + " items");
            File.WriteAllLines(bundlesFile, bundles.ToArray());
        }

        private static bool IsAvailableFileName(string fileCandidate, string extension)
        {
            if (fileCandidate == null) throw new ArgumentNullException("fileCandidate");
            if (extension == null) throw new ArgumentNullException("extension");
            string fileName = fileCandidate + extension;
            return !File.Exists(fileName);
        }

        /// <summary>
        /// Checks if the file is already present on the file system and returns a file path that is available.
        /// For example, if c:\temp\myfile.dds already exists, it will return the available file c:\temp\myfile_000002.dds and increment to c:\temp\myfile_000003.dds if that file would exist.
        /// </summary>
        /// <param name="fileCandidate">path prefix without extension</param>
        /// <param name="extension">extension of the resulting file including the dot like '.ogg' for audio files</param>
        /// <returns>File path, where no file exists.</returns>
        private static string GetAvailableFileName(string fileCandidate, string extension)
        {
            if (fileCandidate == null) throw new ArgumentNullException("fileCandidate");
            if (extension == null) throw new ArgumentNullException("extension");
            string fileName = fileCandidate + extension;
            int count = 1;
            while (File.Exists(fileName))
            {
                count++;
                fileName = fileCandidate + "_" + count.ToString().PadLeft(6, '0') + extension;
            }
            return fileName;
        }
    }
}
