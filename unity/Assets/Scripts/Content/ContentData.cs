using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Content;
using UnityEngine;
using ValkyrieTools;

/// <summary>
/// This class reads and stores all of the content for a base game and expansions.</summary>
public class ContentData {

    public HashSet<string> loadedPacks;
    public List<ContentPack> allPacks;
    public Dictionary<string, StringKey> packSymbol;

    private Dictionary<Type, Dictionary<string, IContent>> Content = new Dictionary<Type, Dictionary<string, IContent>>();

    protected Dictionary<string, IContent> ContentOfType<T>() where T : IContent
    {
        Content.TryGetValue(typeof(T), out Dictionary<string, IContent> content);
        if (content == null)
        {
            content = new Dictionary<string, IContent>();
            Content[typeof(T)] = content;
        }
        return content;
    }

    public void AddOrReplace<T>(string name, T content) where T : IContent
    {
        ContentOfType<T>()[name] = content;
    }

    public bool TryGet<T>(string name, out T value) where T : IContent
    {
        var contains = ContentOfType<T>().TryGetValue(name, out IContent obj);
        value = (T) obj;
        return contains;
    }
    
    public T Get<T>(string name) where T : IContent
    {
        return (T) ContentOfType<T>()[name];
    }
    public IEnumerable<string> Keys<T>() where T : IContent
    {
        return ContentOfType<T>().Keys.AsEnumerable();
    }
    
    public IEnumerable<T> Values<T>() where T : IContent
    {
        return ContentOfType<T>().Values.Cast<T>().AsEnumerable();
    }

    public bool ContainsKey<T>(string key) where T : IContent
    {
        return ContentOfType<T>().ContainsKey(key);
    }

    public IEnumerable<KeyValuePair<string, T>> GetAll<T>() where T : IContent
    {
        return ContentOfType<T>().AsEnumerable()
            .Select(t => new KeyValuePair<string, T>(t.Key, (T)t.Value));
    }

    public int Count<T>() where T : IContent
    {
        return ContentOfType<T>().Count;
    }

    // textureCache is used to store previously loaded textures so they are faster next time
    // For the editor all defined images are loaded, requires ~1GB RAM
    // For quests only used tiles/tokens will be loaded
    public static Dictionary<string, Texture2D> textureCache;

    /// <summary>
    /// Get the path where game content is defined.</summary>
    /// <returns>
    /// The path as a string with a trailing '/'.</returns>
    public static string ContentPath()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            return Application.persistentDataPath + "/assets/content/";
        }
        return Application.streamingAssetsPath + "/content/";
    }

    // Return a list of id for all enbaled content packs
    public List<string> GetLoadedPackIDs()
    {
        return new List<string>(loadedPacks);
    }


    /// <summary>
    /// Get the path where ffg app content is imported.</summary>
    /// <returns>
    /// The path as a string without a trailing '/'.</returns>
    public static string ImportPath()
    {
        return GameTypePath + Path.DirectorySeparatorChar + "import";
    }

    /// <summary>
    /// Get download directory without trailing '/'
    /// </summary>
    /// <returns>location to save / load packages</returns>
    public static string DownloadPath()
    {
        return Game.AppData() + Path.DirectorySeparatorChar + "Download";
    }

    public static string TempPath
    {
        get
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return Path.Combine(Game.AppData(), "temp");
            }
            return Path.GetTempPath();
        }
    }

    public static string TempValyriePath
    {
        get
        {
            return Path.Combine(TempPath, "Valkyrie");
        }
    }

    public static string GameTypePath
    {
        get
        {
            return Path.Combine(Game.AppData() , Game.Get().gameType.TypeName());
        }
    }

    public static string ValkyrieLoadPath
    {
        get
        {
            return Path.Combine(TempValyriePath, "Load");
        }
    }

    public static string ValkyriePreloadPath
    {
        get
        {
            return Path.Combine(TempValyriePath, "Preload");
        }
    }

    public static string ValkyrieLoadQuestPath
    {
        get
        {
            return Path.Combine(ValkyrieLoadPath, "quest");
        }
    }

    /// <summary>
    /// Seach the provided path for all content packs and read meta data.</summary>
    /// <param name="path">Path to search for content packs.</param>
    public ContentData(string path)
    {
        // This is pack type for sorting packs
        loadedPacks = new HashSet<string>();

        // This is pack symbol list
        packSymbol = new Dictionary<string, StringKey>();
    
        //This has the game game and all expansions, general info
        allPacks = new List<ContentPack>();


        // Search each directory in the path (one should be base game, others expansion.  Names don't matter
        string[] contentFiles = Directory.GetFiles(path, "content_pack.ini", SearchOption.AllDirectories);
        foreach (string p in contentFiles)
        {
            PopulatePackList(Path.GetDirectoryName(p));
        }
    }

    // Read a content pack for list of files and meta data
    public void PopulatePackList(string path)
    {
        // All packs must have a content_pack.ini, otherwise ignore
        if (File.Exists(path + Path.DirectorySeparatorChar + "content_pack.ini"))
        {
            ContentPack pack = new ContentPack();

            // Get all data from the file
            IniData d = IniRead.ReadFromIni(path + Path.DirectorySeparatorChar + "content_pack.ini");
            // Todo: better error handling
            if (d == null)
            {
                ValkyrieDebug.Log("Failed to get any data out of " + path + Path.DirectorySeparatorChar + "content_pack.ini!");
                Application.Quit();
            }

            pack.name = d.Get("ContentPack", "name");
            if (pack.name.Equals(""))
            {
                ValkyrieDebug.Log("Failed to get name data out of " + path + Path.DirectorySeparatorChar + "content_pack.ini!");
                Application.Quit();
            }

            // id can be empty/missing
            pack.id = d.Get("ContentPack", "id");

            // If this is invalid we will just handle it later, not fatal
            if (d.Get("ContentPack", "image").IndexOf("{import}") == 0)
            {
                pack.image = ImportPath() + d.Get("ContentPack", "image").Substring(8);
            }
            else
            {
                pack.image = path + Path.DirectorySeparatorChar + d.Get("ContentPack", "image");
            }

            // Black description isn't fatal
            pack.description = d.Get("ContentPack", "description");

            // Some packs have a type
            pack.type = d.Get("ContentPack", "type");

            // Get cloned packs
            string cloneString = d.Get("ContentPack", "clone");
            pack.clone = new List<string>();
            foreach (string s in cloneString.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                pack.clone.Add(s);
            }

            // Get all the other ini files in the pack
            List<string> files = new List<string>();
            // content_pack file is included
            files.Add(path + Path.DirectorySeparatorChar + "content_pack.ini");

            // No extra files is valid
            if (d.Get("ContentPackData") != null)
            {
                foreach (string file in d.Get("ContentPackData").Keys)
                {
                    files.Add(path + Path.DirectorySeparatorChar + file);
                }
            }
            // Save list of files
            pack.iniFiles = files;

            // Get all the other ini files in the pack
            Dictionary<string, List<string>> dictFiles = new Dictionary<string, List<string>>();
            // No extra files is valid
            if (d.Get("LanguageData") != null)
            {
                foreach (string s in d.Get("LanguageData").Keys)
                {
                    int firstSpace = s.IndexOf(' ');
                    string id = s.Substring(0, firstSpace);
                    string file = s.Substring(firstSpace + 1);
                    if (!dictFiles.ContainsKey(id))
                    {
                        dictFiles.Add(id, new List<string>());
                    }
                    dictFiles[id].Add(path + Path.DirectorySeparatorChar + file);
                }
            }
            // Save list of files
            pack.localizationFiles = dictFiles;

            // Add content pack
            allPacks.Add(pack);

            // Add symbol
            packSymbol.Add(pack.id, new StringKey("val", pack.id + "_SYMBOL"));

            // We finish without actually loading the content, this is done later (content optional)
        }
    }

    // Return a list of names for all found content packs
    public List<string> GetPacks()
    {
        List<string> names = new List<string>();
        allPacks.ForEach(cp => names.Add(cp.name));
        return names;
    }

    public string GetContentName(string id)
    {
        foreach (ContentPack cp in allPacks)
        {
            if (cp.id.Equals(id))
            {
                return new StringKey(cp.name).Translate();
            }
        }
        return "";
    }

    public string GetContentAcronym(string id)
    {
        foreach (ContentPack cp in allPacks)
        {
            if (cp.id.Equals(id))
            {
                return new StringKey("val", cp.id).Translate();
            }
        }
        return "";
    }

    public string GetContentSymbol(string id)
    {
        foreach (ContentPack cp in allPacks)
        {
            if (cp.id.Equals(id))
            {
                return new StringKey("val", cp.id+"_SYMBOL").Translate();
            }
        }
        return "";
    }
    
    internal bool AddContent<T>(string name, T d) where T : IContent
    {
        bool added = false;
        // If we don't already have one or it's lower priority then add this
        if (!TryGet(name, out T existingPackData)
            || existingPackData.Priority < d.Priority)
        {
            AddOrReplace(name, d);
            return true;
        }
        // items of the same priority belong to multiple packs
        else if (existingPackData.Priority == d.Priority)
        {
            existingPackData.Sets.AddRange(d.Sets);
            return false;
        }

        return false;
    }

    // Holding class for contentpack data
    public class ContentPack
    {
        public string name;
        public string image;
        public string description;
        public string id;
        public string type;
        public List<string> iniFiles;
        public Dictionary<string, List<string>> localizationFiles;
        public List<string> clone;
    }

    /// <summary>
    /// This will resove the asset name to a matching file and return the full file path including the file extension or null, if the file can't be resolved.
    /// </summary>
    public static string ResolveTextureFile(string name)
    {
        if (name == null) throw new ArgumentNullException("name");
        if (File.Exists(name))
        {
            return name;
        }

        // Check all possible extensions
        foreach (string extension in new[] { ".dds", ".pvr", ".png", ".jpg", ".jpeg",".tex" })
        {
            string file = name + extension;
            if (File.Exists(file))
            {
                return file;
            }
        }
        return null;
    }

    // Get a unity texture from a file (dds or other unity supported format)
    public static Texture2D FileToTexture(string file)
    {
        if (file == null) throw new ArgumentNullException("file");
        return FileToTexture(file, Vector2.zero, Vector2.zero);
    }

    // Get a unity texture from a file (dds, svr or other unity supported format)
    // Crop to pos and size in pixels
    public static Texture2D FileToTexture(string file, Vector2 pos, Vector2 size)
    {
        if (file == null) throw new ArgumentNullException("file");
        var resolvedFile = ResolveTextureFile(file);
        // return if file could not be resolved
        if (resolvedFile == null)
        {
            ValkyrieDebug.Log("Could not resolve file: '" + file + "'");
            return null;
        }
        file = resolvedFile;

        Texture2D texture = null;

        if (textureCache == null)
        {
            textureCache = new Dictionary<string, Texture2D>();
        }

        if (textureCache.ContainsKey(file))
        {
            texture = textureCache[file];
        }
        else
        {
            string ext = Path.GetExtension(file);
            if (ext.Equals(".dds", StringComparison.InvariantCultureIgnoreCase))
            {
                texture = DdsToTexture(file);
            }
            else if (ext.Equals(".pvr", StringComparison.InvariantCultureIgnoreCase))
            {
                texture = PvrToTexture(file);
            }
            else // If the image isn't one of the formats above, just use unity file load
            {
                texture = ImageToTexture(file);
            }
            if (!file.Contains(TempPath))
            {
                textureCache.Add(file, texture);
            }
        }
        // Get whole image
        if (size.x == 0) return texture;

        // Get part of the image
        // Array of pixels from image
        Color[] pix = texture.GetPixels(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y));
        // Empty texture
        var subTexture = new Texture2D(Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y));
        // Set pixels
        subTexture.SetPixels(pix);
        subTexture.Apply();
        return subTexture;
    }

    private static Texture2D DdsToTexture(string file)
    {
        if (file == null) throw new ArgumentNullException("file");
        // Unity doesn't support dds directly, have to do hackery
        // Read the data
        byte[] ddsBytes = null;
        try
        {
            ddsBytes = File.ReadAllBytes(file);
        }
        catch (Exception ex)
        {
            if (!File.Exists(file))
                ValkyrieDebug.Log("Warning: DDS Image missing: '" + file + "'");
            else
                ValkyrieDebug.Log("Warning: DDS Image loading of file '" + file + "' failed with " + ex.GetType().Name + " message: " + ex.Message);
            return null;
        }
        // Check for valid header
        byte ddsSizeCheck = ddsBytes[4];
        if (ddsSizeCheck != 124)
        {
            ValkyrieDebug.Log("Warning: Image invalid: '" + file + "'");
            return null;
        }

        // Extract dimensions
        int height = ddsBytes[13] * 256 + ddsBytes[12];
        int width = ddsBytes[17] * 256 + ddsBytes[16];

        /*20-23 pit
         * 24-27 dep
         * 28-31 mm
         * 32-35 al
         * 36-39 res
         * 40-43 sur
         * 44-51 over
         * 52-59 des
         * 60-67 sov
         * 68-75 sblt
         * 76-79 size
         * 80-83 flags
         * 84-87 fourCC
         * 88-91 bpp
         * 92-95 red
         */

        char[] type = new char[4];
        type[0] = (char)ddsBytes[84];
        type[1] = (char)ddsBytes[85];
        type[2] = (char)ddsBytes[86];
        type[3] = (char)ddsBytes[87];

        // Copy image data (skip header)
        const int DDS_HEADER_SIZE = 128;
        byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
        Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

        Texture2D texture = null;

        if (ddsBytes[87] == '5')
        {
            texture = new Texture2D(width, height, TextureFormat.DXT5, false);
        }
        else if (ddsBytes[87] == '1')
        {
            texture = new Texture2D(width, height, TextureFormat.DXT1, false);
        }
        else if (ddsBytes[88] == 32)
        {
            if (ddsBytes[92] == 0)
            {
                texture = new Texture2D(width, height, TextureFormat.BGRA32, false);
            }
            else
            {
                texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            }
        }
        else if (ddsBytes[88] == 24)
        {
            texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        }
        else
        {
            ValkyrieDebug.Log("Warning: Image invalid: '" + file + "'");
        }
        texture.LoadRawTextureData(dxtBytes);
        texture.Apply();
        return texture;
    }

    private static Texture2D PvrToTexture(string file)
    {
        if (file == null) throw new ArgumentNullException("file");

        long pixelformat = -1;
        try
        {
            string imagePath = new Uri(file).AbsoluteUri;
            var www = new WWW(imagePath);

            // *** HEADER ****
            // int verison
            // int flags
            pixelformat = BitConverter.ToInt64(www.bytes, 8);
            // int color space
            // int channel type
            int height = BitConverter.ToInt32(www.bytes, 24);
            int width = BitConverter.ToInt32(www.bytes, 28);
            // int depth
            // int num surfaces
            // int num faces
            // int mip map count
            // int meta data size

            // *** IMAGE DATA ****
            const int PVR_HEADER_SIZE = 52;
            var image = new byte[www.bytesDownloaded - PVR_HEADER_SIZE];
            Buffer.BlockCopy(www.bytes, PVR_HEADER_SIZE, image, 0, www.bytesDownloaded - PVR_HEADER_SIZE);
            Texture2D texture = null;
            switch (pixelformat)
            {
                case 22: // ETC2_RGB4
                    texture = new Texture2D(width, height, TextureFormat.ETC2_RGB, false, true);
                    texture.LoadRawTextureData(image);
                    texture.Apply();
                    break;
                case 23:  // ETC2_RGB8
                    texture = new Texture2D(width, height, TextureFormat.ETC2_RGBA8, false, true);
                    texture.LoadRawTextureData(image);
                    texture.Apply();
                    break;
                default:
                    ValkyrieDebug.Log("Warning: PVR unknown pixelformat: '" + pixelformat + "' in file: '" + file + "'");
                    break;
            } 
            return texture;
        }
        catch (Exception ex)
        {
            if (!File.Exists(file))
                ValkyrieDebug.Log("Warning: PVR Image missing: '" + file + "'");
            else
                ValkyrieDebug.Log("Warning: PVR Image loading of file '" + file + "' failed with " + ex.GetType().Name + " message: " + ex.Message + " pixelformat: '" + pixelformat + "'");
            return null;
        }
    }

    private static Texture2D ImageToTexture(string file)
    {
        if (file == null) throw new ArgumentNullException("file");
        try
        {
            string imagePath = new Uri(file).AbsoluteUri;
            var www = new WWW(imagePath);
            return www.texture;
        }
        catch (Exception ex)
        {
            if (!File.Exists(file))
                ValkyrieDebug.Log("Warning: Image missing: '" + file + "'");
            else
                ValkyrieDebug.Log("Warning: Image loading of file '" + file + "' failed with " + ex.GetType().Name + " message: " + ex.Message);
            return null;
        }
    }

}