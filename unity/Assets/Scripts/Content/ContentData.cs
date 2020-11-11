using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Content;
using ValkyrieTools;
using Random = UnityEngine.Random;

/// <summary>
/// This class reads and stores all of the content for a base game and expansions.</summary>
public class ContentData {

    public HashSet<string> loadedPacks;
    public List<ContentPack> allPacks;
    public Dictionary<string, StringKey> packSymbol;

    private Dictionary<Type, Dictionary<string, IContent>> Content = new Dictionary<Type, Dictionary<string, IContent>>
    {
        // This is pack type for sorting packs
        { typeof(PackTypeData),  new Dictionary<string, IContent> () },
        // This is all of the available sides of tiles (not currently tracking physical tiles)
        { typeof(TileSideData),  new Dictionary<string, IContent> () },
        // Available heroes
        { typeof(HeroData),  new Dictionary<string, IContent> () },
        // Available classes
        { typeof(ClassData),  new Dictionary<string, IContent> () },
        // Available skills
        { typeof(SkillData),  new Dictionary<string, IContent> () },
        // Available items
        { typeof(ItemData),  new Dictionary<string, IContent> () },
        // Available monsters
        { typeof(MonsterData),  new Dictionary<string, IContent> () },
        // This has all monster activations
        { typeof(ActivationData),  new Dictionary<string, IContent> () },
        // This has all available attacks
        { typeof(AttackData),  new Dictionary<string, IContent> () },
        // This has all available evades
        { typeof(EvadeData),  new Dictionary<string, IContent> () },
        // This has all available evades
        { typeof(HorrorData),  new Dictionary<string, IContent> () },
        // This has all available tokens
        { typeof(TokenData),  new Dictionary<string, IContent> () },
        // This has all available perils
        { typeof(PerilData),  new Dictionary<string, IContent> () },
        // This has all available puzzle images
        { typeof(PuzzleData),  new Dictionary<string, IContent> () },
        // This has all available general images
        { typeof(ImageData),  new Dictionary<string, IContent> () },
        // This has all available puzzle images
        { typeof(AudioData),  new Dictionary<string, IContent> () }
    };

    public void AddOrReplace<T>(string name, T content) where T : IContent
    {
        Content[typeof(T)][name] = content;
    }

    public bool TryGet<T>(string name, out T value) where T : IContent
    {
        var contains = Content[typeof(T)].TryGetValue(name, out IContent obj);
        value = (T) obj;
        return contains;
    }
    
    public T Get<T>(string name) where T : IContent
    {
        return (T) Content[typeof(T)][name];
    }
    public IEnumerable<string> Keys<T>() where T : IContent
    {
        return Content[typeof(T)].Keys.AsEnumerable();
    }
    
    public IEnumerable<T> Values<T>() where T : IContent
    {
        return Content[typeof(T)].Values.Cast<T>().AsEnumerable();
    }

    public bool ContainsKey<T>(string key) where T : IContent
    {
        return Content[typeof(T)].ContainsKey(key);
    }

    public IEnumerable<KeyValuePair<string, T>> GetAll<T>() where T : IContent
    {
        return Content[typeof(T)].AsEnumerable()
            .Select(t => new KeyValuePair<string, T>(t.Key, (T)t.Value));
    }

    public int Count<T>() where T : IContent
    {
        return Content[typeof(T)].Count;
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
                pack.image = ContentData.ImportPath() + d.Get("ContentPack", "image").Substring(8);
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
            foreach (string s in cloneString.Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries))
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

    // Return a list of id for all enbaled content packs
    public List<string> GetLoadedPackIDs()
    {
        return new List<string>(loadedPacks);
    }

    // This loads content from a pack by name
    // Duplicate content will be replaced by the higher priority value
    public void LoadContent(string name)
    {
        foreach (ContentPack cp in allPacks)
        {
            if(cp.name.Equals(name))
            {
                LoadContent(cp);
            }
        }
    }

    // This loads content from a pack by ID
    // Duplicate content will be replaced by the higher priority value
    public void LoadContentID(string id)
    {
        foreach (ContentPack cp in allPacks)
        {
            if (cp.id.Equals(id))
            {
                LoadContent(cp);
            }
        }
    }

    // This loads content from a pack by object
    // Duplicate content will be replaced by the higher priority value
    void LoadContent(ContentPack cp)
    {
        // Don't reload content
        if (loadedPacks.Contains(cp.id)) return;

        foreach (KeyValuePair<string, List<string>> kv in cp.localizationFiles)
        {
            DictionaryI18n packageDict = new DictionaryI18n();
            foreach (string file in kv.Value)
            {
                packageDict.AddDataFromFile(file);
            }

            LocalizationRead.AddDictionary(kv.Key, packageDict);
        }

        foreach (string ini in cp.iniFiles)
        {
            IniData d = IniRead.ReadFromIni(ini);
            // Bad ini file not a fatal error, just ignore (will be in log)
            if (d == null)
                return;

            // Add each section
            foreach(KeyValuePair<string, Dictionary<string, string>> section in d.data)
            {
                AddContent(section.Key, section.Value, Path.GetDirectoryName(ini), cp.id);
            }
        }


        loadedPacks.Add(cp.id);

        foreach (string s in cp.clone)
        {
            LoadContentID(s);
        }
    }
    
    // Add a section of an ini file to game content
    // name is from the ini file and must start with the type
    // path is relative and is used for images or other paths in the content
    void AddContent(string name, Dictionary<string, string> content, string path, string packID)
    {
        var sets = string.IsNullOrWhiteSpace(packID) ? new List<string>() : new List<string> { packID };
        
        // Is this a "PackType" entry?
        if(name.StartsWith(PackTypeData.type))
        {
            PackTypeData d = new PackTypeData(name, content, path, sets);
            AddContentInternal(name, d);
        }

        // Is this a "TileSide" entry?
        if(name.StartsWith(TileSideData.type))
        {
            TileSideData d = new TileSideData(name, content, path, sets);
            AddContentInternal(name, d);
        }

        // Is this a "Hero" entry?
        if (name.StartsWith(HeroData.type))
        {
            HeroData d = new HeroData(name, content, path, sets);
            AddContentInternal(name, d);
        }

        // Is this a "Class" entry?
        if (name.StartsWith(ClassData.type))
        {
            ClassData d = new ClassData(name, content, path, sets);
            AddContentInternal(name, d);
        }

        // Is this a "Skill" entry?
        if (name.StartsWith(SkillData.type))
        {
            SkillData d = new SkillData(name, content, path, sets);
            AddContentInternal(name, d);
        }

        // Is this a "Item" entry?
        if (name.StartsWith(ItemData.type))
        {
            ItemData d = new ItemData(name, content, path, sets);
            AddContentInternal(name, d);
        }

        // Is this a "Monster" entry?
        if (name.StartsWith(MonsterData.type))
        {
            MonsterData d = new MonsterData(name, content, path, sets);
            AddContentInternal(name, d);
        }

        // Is this a "Activation" entry?
        if (name.StartsWith(ActivationData.type))
        {
            ActivationData d = new ActivationData(name, content, path, sets);
            AddContentInternal(name, d);
        }
        
        // Is this a "Attack" entry?
        if (name.StartsWith(AttackData.type))
        {
            AttackData d = new AttackData(name, content, path, sets);
            AddContentInternal(name, d);
        }

        // Is this a "Evade" entry?
        if (name.StartsWith(EvadeData.type))
        {
            EvadeData d = new EvadeData(name, content, path, sets);
            AddContentInternal(name, d);
        }

        // Is this a "Horror" entry?
        if (name.StartsWith(HorrorData.type))
        {
            HorrorData d = new HorrorData(name, content, path, sets);
            AddContentInternal(name, d);
        }

        // Is this a "Token" entry?
        if (name.StartsWith(TokenData.type))
        {
            TokenData d = new TokenData(name, content, path, sets);
            if (d.image.Equals(""))
            {
                ValkyrieDebug.Log("Token " + d.name + "did not have an image. Skipping");
                return;
            }
            AddContentInternal(name, d);
        }

        // Is this a "Peril" entry?
        if (name.StartsWith(PerilData.type))
        {
            PerilData d = new PerilData(name, content);
            // Ignore invalid entry
            if (d.sectionName.Equals(""))
                return;
            AddContentInternal(name, d);
        }

        // Is this a "Puzzle" entry?
        if (name.StartsWith(PuzzleData.type))
        {
            PuzzleData d = new PuzzleData(name, content, path, sets);
            // Ignore invalid entry
            AddContentInternal(name, d);
        }

        // Is this a "Image" entry?
        if (name.StartsWith(ImageData.type))
        {
            ImageData d = new ImageData(name, content, path, sets);
            // Ignore invalid entry
            AddContentInternal(name, d);
        }

        // Is this a "Audio" entry?
        if (name.StartsWith(AudioData.type))
        {
            AudioData d = new AudioData(name, content, path, sets);
            // Ignore invalid entry
            AddContentInternal(name, d);
        }
    }

    private void AddContentInternal<T>(string name, T d) where T : IContent
    {
        // If we don't already have one or it's lower priority then add this
        if (!TryGet(name, out T existingPackData)
            || existingPackData.Priority < d.Priority)
        {
            AddOrReplace(name, d);
        }
        // items of the same priority belong to multiple packs
        else if (existingPackData.Priority == d.Priority)
        {
            existingPackData.Sets.AddRange(d.Sets);
        }
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
        if (name == null) throw new System.ArgumentNullException("name");
        if (File.Exists(name))
        {
            return name;
        }

        // Check all possible extensions
        foreach (string extension in new[] { ".dds", ".pvr", ".png", ".jpg", ".jpeg" })
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
        if (file == null) throw new System.ArgumentNullException("file");
        return FileToTexture(file, Vector2.zero, Vector2.zero);
    }

    // Get a unity texture from a file (dds, svr or other unity supported format)
    // Crop to pos and size in pixels
    public static Texture2D FileToTexture(string file, Vector2 pos, Vector2 size)
    {
        if (file == null) throw new System.ArgumentNullException("file");
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
            if (ext.Equals(".dds", System.StringComparison.InvariantCultureIgnoreCase))
            {
                texture = DdsToTexture(file);
            }
            else if (ext.Equals(".pvr", System.StringComparison.InvariantCultureIgnoreCase))
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
        if (file == null) throw new System.ArgumentNullException("file");
        // Unity doesn't support dds directly, have to do hackery
        // Read the data
        byte[] ddsBytes = null;
        try
        {
            ddsBytes = File.ReadAllBytes(file);
        }
        catch (System.Exception ex)
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
        System.Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

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
        if (file == null) throw new System.ArgumentNullException("file");

        long pixelformat = -1;
        try
        {
            string imagePath = new System.Uri(file).AbsoluteUri;
            var www = new WWW(imagePath);

            // *** HEADER ****
            // int verison
            // int flags
            pixelformat = System.BitConverter.ToInt64(www.bytes, 8);
            // int color space
            // int channel type
            int height = System.BitConverter.ToInt32(www.bytes, 24);
            int width = System.BitConverter.ToInt32(www.bytes, 28);
            // int depth
            // int num surfaces
            // int num faces
            // int mip map count
            // int meta data size

            // *** IMAGE DATA ****
            const int PVR_HEADER_SIZE = 52;
            var image = new byte[www.bytesDownloaded - PVR_HEADER_SIZE];
            System.Buffer.BlockCopy(www.bytes, PVR_HEADER_SIZE, image, 0, www.bytesDownloaded - PVR_HEADER_SIZE);
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
        catch (System.Exception ex)
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
        if (file == null) throw new System.ArgumentNullException("file");
        try
        {
            string imagePath = new System.Uri(file).AbsoluteUri;
            var www = new WWW(imagePath);
            return www.texture;
        }
        catch (System.Exception ex)
        {
            if (!File.Exists(file))
                ValkyrieDebug.Log("Warning: Image missing: '" + file + "'");
            else
                ValkyrieDebug.Log("Warning: Image loading of file '" + file + "' failed with " + ex.GetType().Name + " message: " + ex.Message);
            return null;
        }
    }

}

// Class for tile specific data
public class PackTypeData : GenericData
{
    public static new string type = "PackType";

    public PackTypeData(string name, Dictionary<string, string> content, string path, List<string> sets = null) : base(name, content, path, type, sets)
    {
    }
}

// Class for tile specific data
public class TileSideData : GenericData
{
    public float top = 0;
    public float left = 0;
    public float pxPerSquare;
    public float aspect = 0;
    public string reverse = "";
    public new static string type = "TileSide";

    public TileSideData(string name, Dictionary<string, string> content, string path, List<string> sets = null) : base(name, content, path, type, sets)
    {
        // Get location of top left square in tile image, default 0
        if (content.ContainsKey("top"))
        {
            float.TryParse(content["top"], out top);
        }
        if (content.ContainsKey("left"))
        {
            float.TryParse(content["left"], out left);
        }

        // pixel per D2E square (inch) of image
        if (content.ContainsKey("pps"))
        {
            if (content["pps"].StartsWith("*"))
            {
                float.TryParse(content["pps"].Remove(0, 1), out pxPerSquare);
                pxPerSquare *= Game.Get().gameType.TilePixelPerSquare();
            }
            else
            {
                float.TryParse(content["pps"], out pxPerSquare);
            }
        }
        else
        {
            pxPerSquare = Game.Get().gameType.TilePixelPerSquare();
        }

        // Some MoM tiles have crazy aspect
        if (content.ContainsKey("aspect"))
        {
            float.TryParse(content["aspect"], out aspect);
        }

        // Other side used for validating duplicate use
        if (content.ContainsKey("reverse"))
        {
            reverse = content["reverse"];
        }
    }
}

// Class for Hero specific data
public class HeroData : GenericData
{
    public string archetype = "warrior";
    public static new string type = "Hero";
    public string item = "";

    public HeroData(string name, Dictionary<string, string> content, string path, List<string> sets = null) : base(name, content, path, type, sets)
    {
        // Get archetype
        if (content.ContainsKey("archetype"))
        {
            archetype = content["archetype"];
        }
        // Get starting item
        if (content.ContainsKey("item"))
        {
            item = content["item"];
        }
    }
}

// Class for Class specific data
public class ClassData : GenericData
{
    public string archetype = "warrior";
    public string hybridArchetype = "";
    public static new string type = "Class";
    public List<string> items;

    public ClassData(string name, Dictionary<string, string> content, string path, List<string> sets = null) : base(name, content, path, type, sets)
    {
        // Get archetype
        if (content.ContainsKey("archetype"))
        {
            archetype = content["archetype"];
        }
        // Get hybridArchetype
        if (content.ContainsKey("hybridarchetype"))
        {
            hybridArchetype = content["hybridarchetype"];
        }
        // Get starting item
        items = new List<string>();
        if (content.ContainsKey("items"))
        {
            items.AddRange(content["items"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries));
        }
    }
}

// Class for Class specific data
public class SkillData : GenericData
{
    public static new string type = "Skill";
    public int xp = 0;

    public SkillData(string name, Dictionary<string, string> content, string path, List<string> sets = null) : base(name, content, path, type, sets)
    {
        // Get archetype
        if (content.ContainsKey("xp"))
        {
            int.TryParse(content["xp"], out xp);
        }
    }
}

// Class for Item specific data
public class ItemData : GenericData
{
    public static new string type = "Item";
    public bool unique = false;
    public int price = 0;
    public int minFame = -1;
    public int maxFame = -1;

    public ItemData(string name, Dictionary<string, string> content, string path, List<string> sets = null) : base(name, content, path, type, sets)
    {
        if (name.IndexOf("ItemUnique") == 0)
        {
            unique = true;
        }
        if (content.ContainsKey("price"))
        {
            int.TryParse(content["price"], out price);
        }
        if (content.ContainsKey("minfame"))
        {
            minFame = Fame(content["minfame"]);
        }
        if (content.ContainsKey("maxfame"))
        {
            maxFame = Fame(content["maxfame"]);
        }
    }

    public static int Fame(string name)
    {
        if (name.Equals("insignificant")) return 1;
        if (name.Equals("noteworthy")) return 2;
        if (name.Equals("impressive")) return 3;
        if (name.Equals("celebrated")) return 4;
        if (name.Equals("heroic")) return 5;
        if (name.Equals("legendary")) return 6;
        return 0;
    }
}

// Class for Hero specific data
public class MonsterData : GenericData
{
    public StringKey info = new StringKey(null,"-", false);
    public string imagePlace;
    public static new string type = "Monster";
    public string[] activations;
    public float healthBase = 0;
    public float healthPerHero = 0;
    public int horror = 0;
    public int awareness = 0;
    
    // This constuctor only exists for the quest version of this class to use to do nothing
    public MonsterData()
    {
    }

    public MonsterData(string name, Dictionary<string, string> content, string path, List<string> sets) : base(name, content, path, type, sets)
    {
        // Get usage info
        if (content.ContainsKey("info"))
        {
            info = new StringKey(content["info"]);
        }
        if (content.ContainsKey("imageplace"))
        {
            if (content["imageplace"].IndexOf("{import}") == 0)
            {
                imagePlace = ContentData.ImportPath() + content["imageplace"].Substring(8);
            }
            else
            {
                imagePlace = path + Path.DirectorySeparatorChar + content["imageplace"];
            }
        }
        else // No image is a valid condition
        {
            imagePlace = image;
        }
        activations = new string[0];
        if (content.ContainsKey("activation"))
        {
            activations = content["activation"].Split(' ');
        }
        if (content.ContainsKey("health"))
        {
            float.TryParse(content["health"], out healthBase);
        }
        if (content.ContainsKey("healthperhero"))
        {
            float.TryParse(content["healthperhero"], out healthPerHero);
        }
        if (content.ContainsKey("horror"))
        {
            int.TryParse(content["horror"], out horror);
        }
        if (content.ContainsKey("awareness"))
        {
            int.TryParse(content["awareness"], out awareness);
        }
    }

    virtual public IEnumerable<string> GetAttackTypes()
    {
        HashSet<string> toReturn = new HashSet<string>();
        foreach (AttackData kv in Game.Get().cd.Values<AttackData>())
        {
            if (ContainsTrait(kv.target))
            {
                toReturn.Add(kv.attackType);
            }
        }
        return toReturn;
    }

    virtual public StringKey GetRandomAttack(string type)
    {
        List<AttackData> validAttacks = new List<AttackData>();
        foreach (AttackData ad in Game.Get().cd.Values<AttackData>())
        {
            if (ad.attackType.Equals(type))
            {
                if(traits.Length == 0)
                {
                    ValkyrieDebug.Log("Monster with no traits, this should not happen");
                    validAttacks.Add(ad);
                }
                else if (traits.Contains(ad.target))
                {
                    validAttacks.Add(ad);
                }
            }
        }
        return validAttacks[Random.Range(0, validAttacks.Count)].text;
    }
}

// Class for Activation specific data
public class ActivationData : GenericData
{
    public StringKey ability = new StringKey(null,"-", false);
    public StringKey minionActions = StringKey.NULL;
    public StringKey masterActions = StringKey.NULL;
    public StringKey moveButton = StringKey.NULL;
    public StringKey move = StringKey.NULL;
    public static new string type = "MonsterActivation";
    public bool masterFirst = false;
    public bool minionFirst = false;

    public ActivationData()
    {
    }

    public ActivationData(string name, Dictionary<string, string> content, string path, List<string> sets = null) : base(name, content, path, type, sets)
    {
        // Get ability
        if (content.ContainsKey("ability"))
        {
            ability = new StringKey(content["ability"]);
        }
        // Get minion activation info
        if (content.ContainsKey("minion"))
        {
            minionActions = new StringKey(content["minion"]);
        }
        // Get master activation info
        if (content.ContainsKey("master"))
        {
            masterActions = new StringKey(content["master"]);
        }
        if (content.ContainsKey("movebutton"))
        {
            moveButton = new StringKey(content["movebutton"]);
        }
        if (content.ContainsKey("move"))
        {
            move = new StringKey(content["move"]);
        }
        if (content.ContainsKey("masterfirst"))
        {
            bool.TryParse(content["masterfirst"], out masterFirst);
        }
        if (content.ContainsKey("minionfirst"))
        {
            bool.TryParse(content["minionfirst"], out minionFirst);
        }
    }
}

// Class for Token data
public class TokenData : GenericData
{
    public int x = 0;
    public int y = 0;
    public int height = 0;
    public int width = 0;
    // 0 means token is 1 square big
    public float pxPerSquare = 0;
    public static new string type = "Token";

    public TokenData(string name, Dictionary<string, string> content, string path, List<String> sets = null) : base(name, content, path, type, sets)
    {
        init(content);
    }

    public TokenData(string name, Dictionary<string, string> content, string path, string typeIn, List<String> sets = null) : base(name, content, path, typeIn, sets)
    {
        init(content);
    }

    public void init(Dictionary<string, string> content)
    {

        if (Application.platform == RuntimePlatform.Android && content.ContainsKey("x_android"))
        {
            int.TryParse(content["x_android"], out x);
        }
        else if (content.ContainsKey("x"))
        {
            int.TryParse(content["x"], out x);
        }

        if (Application.platform == RuntimePlatform.Android && content.ContainsKey("y_android"))
        {
            int.TryParse(content["y_android"], out y);
        }
        else if (content.ContainsKey("y"))
        {
            int.TryParse(content["y"], out y);
        }

        // These are used to extract part of an image (atlas) for the token
        if (content.ContainsKey("height"))
        {
            int.TryParse(content["height"], out height);
        }
        if (content.ContainsKey("height"))
        {
            int.TryParse(content["width"], out width);
        }
        // pixel per D2E square (inch) of image
        if (content.ContainsKey("pps"))
        {
            float.TryParse(content["pps"], out pxPerSquare);
        }
    }

    public bool FullImage()
    {
        if (height == 0) return true;
        if (width == 0) return true;
        return false;
    }
}

// Class for Investigator Attacks
public class AttackData : GenericData
{
    public static new string type = "Attack";

    // Attack text
    public StringKey text = StringKey.NULL;
    // Target type (human, spirit...)
    public string target = "";
    // Attack type (heavy, unarmed)
    public string attackType = "";

    public AttackData(string name, Dictionary<string, string> content, string path, List<string> sets = null) : base(name, content, path, type, sets)
    {
        // Get attack text
        if (content.ContainsKey("text"))
        {
            text = new StringKey(content["text"]);
        }

        // Get attack target
        if (content.ContainsKey("target"))
        {
            target = content["target"];
        }

        // Get attack type
        if (content.ContainsKey("attacktype"))
        {
            attackType = content["attacktype"];
        }
    }
}

// Class for Investigator Evades
public class EvadeData : GenericData
{
    public static new string type = "Evade";

    // Evade text
    public StringKey text = StringKey.NULL;
    public string monster = "";

    public EvadeData(string name, Dictionary<string, string> content, string path, List<string> sets = null) : base(name, content, path, type, sets)
    {
        // Get attack text
        if (content.ContainsKey("text"))
        {
            text = new StringKey(content["text"]);
        }

        // Get attack target
        if (content.ContainsKey("monster"))
        {
            monster = content["monster"];
        }
    }
}

// Class for Horror Checks
public class HorrorData : GenericData
{
    public static new string type = "Horror";

    // Evade text
    public StringKey text = StringKey.NULL;
    public string monster = "";

    public HorrorData(string name, Dictionary<string, string> content, string path, List<string> sets = null) : base(name, content, path, type, sets)
    {
        // Get attack text
        if (content.ContainsKey("text"))
        {
            text = new StringKey(content["text"]);
        }

        // Get attack target
        if (content.ContainsKey("monster"))
        {
            monster = content["monster"];
        }
    }
}

// Class for Puzzle images
public class PuzzleData : GenericData
{
    public static new string type = "Puzzle";

    public PuzzleData(string name, Dictionary<string, string> content, string path, List<string> sets) : base(name, content, path, type, sets)
    {
    }
}

// Class for images
public class ImageData : TokenData
{
    public static new string type = "Image";

    public ImageData(string name, Dictionary<string, string> content, string path, List<string> sets) : base(name, content, path, type, sets)
    {
    }
}

// Class for Audio
public class AudioData : GenericData
{
    public static new string type = "Audio";
    public string file = "";

    public AudioData(string name, Dictionary<string, string> content, string path, List<string> sets) : base(name, content, path, type, sets)
    {
        if (content.ContainsKey("file"))
        {
            if (content["file"].IndexOf("{import}") == 0)
            {
                file = ContentData.ImportPath() + content["file"].Substring(8);
            }
            else
            {
                file = path + Path.DirectorySeparatorChar + content["file"];
            }
        }
    }
}

// Super class for all content loaded from content packs
public class GenericData : IContent
{
    // name from section title or data
    public StringKey name = StringKey.NULL;
    // sets from which this belogs (expansions)
    public List<string> sets;
    // section name
    public string sectionName;
    // List of traits
    public string[] traits;
    // Path to image
    public string image;
    // for sub classes to set type
    public static string type = "";
    // priority for duplicates
    protected int priority;

    public List<string> Sets => sets;
    public int Priority => priority;

    public GenericData()
    {
    }

    // generic constructor gets common things
    public GenericData(string name_ini, Dictionary<string, string> content, string path, string type, List<string> sets)
    {
        sectionName = name_ini;
        this.sets = sets ?? new List<string>();

        // Has the name been specified?
        if (content.ContainsKey("name"))
        {
            name = new StringKey(content["name"]);
        } else
        {
            name = new StringKey(null,name_ini.Substring(type.Length));
        }

        priority = 0;
        if (content.ContainsKey("priority"))
        {
            int.TryParse(content["priority"], out priority);
        }

        if (content.ContainsKey("traits"))
        {
            traits = content["traits"].Split(" ".ToCharArray()) ;
        }
        else // No traits is a valid condition
        {
            traits = new string[0];
        }

        // If image specified it is relative to the path of the ini file
        // absolute paths are not supported
        // resolve optional images like image, image2, image3 and so on
        int count = 0;
        while (true)
        {
            string key = "image" + (count > 0 ? (count + 1).ToString() : "");
            if (!content.ContainsKey(key))
            {
                image = ""; // No image is a valid condition
                break;
            }
            if (content[key].StartsWith("{import}"))
            {
                image = Path.Combine(ContentData.ImportPath(), content[key].Substring(9));
            }
            else
            {
                image = Path.Combine(path, content[key]);
            }
            if (ContentData.ResolveTextureFile(image) != null)
            {
                break;
            }
            count++;
        }
    }

    // Does the component contain a trait?
    public bool ContainsTrait(string trait)
    {
        foreach (string s in traits)
        {
            if (trait.Equals(s))
            {
                return true;
            }
        }
        return false;
    }
}

// Perils are content data that inherits from QuestData for reasons.
public class PerilData : QuestData.Event, IContent
{
    new public static string type = "Peril";
    private int priority = 0;

    public int Priority => priority;
    public List<string> Sets { get; } = new List<string>();

    public StringKey perilText;
    override public StringKey text { get { return perilText; } }

    public PerilData(string name, Dictionary<string, string> data) : base(name, data, "", QuestData.Quest.currentFormat)
    {
        typeDynamic = type;
        if (data.ContainsKey("priority"))
        {
            int.TryParse(data["priority"], out priority);
        }

        if (data.ContainsKey("text"))
        {
            perilText = new StringKey(data["text"]);
        }
    }
}