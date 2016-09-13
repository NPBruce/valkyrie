using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity_Studio;
using System.IO;

public class FetchContent {
    public List<AssetsFile> assetFiles; // All asset files
    public string gameType;
    AppFinder finder = null;
    public bool importAvailable;
    string logFile;
    public static string requiredValkyrieVersion = "0.4.1";

    public FetchContent(string type)
    {
        gameType = type;
        if (type.Equals("D2E"))
        {
            finder = new RtLFinder();
        }
        else if (type.Equals("MoM"))
        {
            finder = new MoMFinder();
        }
        else
        {
            return;
        }

        Debug.Log("FFG " + type + " Version Found: " + finder.AppVersion() + System.Environment.NewLine);

        importAvailable = VersionNewerOrEqual(finder.RequiredFFGVersion(), finder.AppVersion());
    }

    public bool NeedImport()
    {
        logFile = ContentData.ContentPath() + gameType + "/ffg/import.ini";
        IniData log = IniRead.ReadFromIni(logFile);

        if (log == null)
        {
            return true;
        }

        bool appVersionOK = false;
        bool valkVersionOK = false;

        string lastImport = log.Get("Import", "FFG");
        appVersionOK = VersionNewerOrEqual(finder.RequiredFFGVersion(), lastImport);

        lastImport = log.Get("Import", "Valkyrie");
        valkVersionOK = VersionNewerOrEqual(requiredValkyrieVersion, lastImport);

        return !appVersionOK || !valkVersionOK;
    }

    public void Import()
    {
        string[] assetFiles = Directory.GetFiles(finder.location, "*.assets");

        if (!CleanImport()) return;
        foreach (string s in assetFiles)
        {
            Import(s);
        }
    }

    public void Import(string assetFile)
    {
        List<string> unityFiles = new List<string>(); //files to load

        AssetsFile assetsFile = new AssetsFile(assetFile, new EndianStream(File.OpenRead(assetFile), EndianType.BigEndian));
        if (assetsFile.fileGen < 15)
        {
            Debug.Log("Invalid asset file: " + assetFile);
            return;
        }


        foreach (var sharedFile in assetsFile.sharedAssetsList)
        {
            string sharedFilePath = finder.location + "/" + sharedFile.fileName;
            string sharedFileName = Path.GetFileName(sharedFile.fileName);

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

        assetFiles = new List<AssetsFile>();
        foreach (string s in unityFiles)
        {
            AssetsFile file = new AssetsFile(s, new EndianStream(File.OpenRead(s), EndianType.BigEndian));
            assetFiles.Add(file);
        }

        BuildAssetStrucutres();
        WriteImportLog(logFile);
    }

    private void WriteImportLog(string logFile)
    {
        string[] log = new string[3];
        log[0] = "[Import]";
        log[1] = "Valkyrie=" + Game.Get().version;
        log[2] = "FFG=" + finder.AppVersion();
        try
        {
            Directory.CreateDirectory(ContentData.ContentPath());
            Directory.CreateDirectory(ContentData.ContentPath() + gameType);
            Directory.CreateDirectory(ContentData.ContentPath() + gameType + "/ffg");

            logFile = ContentData.ContentPath() + gameType + "/ffg/import.ini";

            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }
            File.WriteAllLines(logFile, log);
        }
        catch(System.Exception)
        {
            Debug.Log("Warning: Unable to create import log");
        }
    }

    //Clean old fetched data
    private bool CleanImport()
    {
        if (!Directory.Exists(ContentData.ContentPath() + gameType + "/ffg")) return true;
        try
        {
            Directory.Delete(ContentData.ContentPath() + gameType + "/ffg", true);
        }
        catch (System.Exception)
        {
            Debug.Log("Warning: Unable to remove temporary files.");
            return false;
        }
        if (Directory.Exists(ContentData.ContentPath() + gameType + "/ffg")) return false;
        return true;
    }

    private void BuildAssetStrucutres()
    {
        foreach (AssetsFile file in assetFiles)
        {
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
                    case 48: //Shader
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

    private void ExportTexture(Unity_Studio.AssetPreloadData asset)
    {
        Unity_Studio.Texture2D m_Texture2D = new Unity_Studio.Texture2D(asset, false);
        m_Texture2D = new Unity_Studio.Texture2D(asset, true);
        Directory.CreateDirectory(ContentData.ContentPath());
        Directory.CreateDirectory(ContentData.ContentPath() + gameType);
        Directory.CreateDirectory(ContentData.ContentPath() + gameType + "/ffg");
        Directory.CreateDirectory(ContentData.ContentPath() + gameType + "/ffg/img");
        string fileCandidate = ContentData.ContentPath() + gameType + "/ffg/img/" + asset.Text;
        string fileName = fileCandidate + asset.extension;
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
    private void ExportAudioClip(Unity_Studio.AssetPreloadData asset)
    {
        Unity_Studio.AudioClip m_AudioClip = new Unity_Studio.AudioClip(asset, false);
        Directory.CreateDirectory(ContentData.ContentPath());
        Directory.CreateDirectory(ContentData.ContentPath() + gameType);
        Directory.CreateDirectory(ContentData.ContentPath() + gameType + "/ffg");
        Directory.CreateDirectory(ContentData.ContentPath() + gameType + "/ffg/audio");
        string fileCandidate = ContentData.ContentPath() + gameType + "/ffg/audio/" + asset.Text;
        string fileName = fileCandidate + asset.extension;

        m_AudioClip = new Unity_Studio.AudioClip(asset, true);
        while (File.Exists(fileName))
        {
            return;// Fixme;
        }

        using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
        {
            writer.Write(m_AudioClip.m_AudioData);
            writer.Close();
        }
    }

    private void ExportText(Unity_Studio.AssetPreloadData asset)
    {
        Unity_Studio.TextAsset m_TextAsset = new Unity_Studio.TextAsset(asset, false);
        Directory.CreateDirectory(ContentData.ContentPath());
        Directory.CreateDirectory(ContentData.ContentPath() + gameType);
        Directory.CreateDirectory(ContentData.ContentPath() + gameType + "/ffg");
        Directory.CreateDirectory(ContentData.ContentPath() + gameType + "/ffg/text");
        string fileCandidate = ContentData.ContentPath() + gameType + "/ffg/text/" + asset.Text;
        string fileName = fileCandidate + asset.extension;

        m_TextAsset = new Unity_Studio.TextAsset(asset, true);
        while (File.Exists(fileName))
        {
            return;// Fixme;
        }

        using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
        {
            writer.Write(m_TextAsset.m_Script);
            writer.Close();
        }
    }

    private void ExportFont(Unity_Studio.AssetPreloadData asset)
    {
        Unity_Studio.unityFont m_Font = new Unity_Studio.unityFont(asset, false);
        Directory.CreateDirectory(ContentData.ContentPath());
        Directory.CreateDirectory(ContentData.ContentPath() + gameType);
        Directory.CreateDirectory(ContentData.ContentPath() + gameType + "/ffg");
        Directory.CreateDirectory(ContentData.ContentPath() + gameType + "/ffg/fonts");
        string fileCandidate = ContentData.ContentPath() + gameType + "/ffg/fonts/" + asset.Text;
        string fileName = fileCandidate + ".ttf";

        m_Font = new Unity_Studio.unityFont(asset, true);

        if (m_Font.m_FontData == null)
        {
            return;
        }

        while (File.Exists(fileName))
        {
            return;// Fixme;
        }

        using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
        {
            writer.Write(m_Font.m_FontData);
            writer.Close();
        }
    }

    public static bool VersionNewerOrEqual(string oldVersion, string newVersion)
    {
        string oldS = System.Text.RegularExpressions.Regex.Replace(oldVersion, "[^0-9]", "");
        string newS = System.Text.RegularExpressions.Regex.Replace(newVersion, "[^0-9]", "");
        if (oldS.Equals(newS)) return true;
        return VersionNewer(oldVersion, newVersion);
    }

    public static bool VersionNewer(string oldVersion, string newVersion)
    {
        string[] oldV = oldVersion.Split('.');
        string[] newV = newVersion.Split('.');

        if (newVersion.Equals("")) return false;

        if (oldVersion.Equals("")) return true;

        if (oldV.Length != newV.Length)
        {
            return true;
        }
        for (int i = 0; i < oldV.Length; i++)
        {
            string oldS = System.Text.RegularExpressions.Regex.Replace(oldV[i], "[^0-9]", "");
            string newS = System.Text.RegularExpressions.Regex.Replace(newV[i], "[^0-9]", "");
            try
            {
                if (int.Parse(oldS) < int.Parse(newS))
                {
                    return true;
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
