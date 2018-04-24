using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Content;
using ValkyrieTools;

/// <summary>
/// This class reads and stores all of the content for a base game and expansions.</summary>
public class ContentData {

    public HashSet<string> loadedPacks;
    public List<ContentPack> allPacks;
    public Dictionary<string, PackTypeData> packTypes;
    public Dictionary<string, TileSideData> tileSides;
    public Dictionary<string, HeroData> heroes;
    public Dictionary<string, ClassData> classes;
    public Dictionary<string, SkillData> skills;
    public Dictionary<string, ItemData> items;
    public Dictionary<string, MonsterData> monsters;
    public Dictionary<string, ActivationData> activations;
    public Dictionary<string, AttackData> investigatorAttacks;
    public Dictionary<string, EvadeData> investigatorEvades;
    public Dictionary<string, HorrorData> horrorChecks;
    public Dictionary<string, TokenData> tokens;
    public Dictionary<string, PerilData> perils;
    public Dictionary<string, PuzzleData> puzzles;
    public Dictionary<string, ImageData> images;
    public Dictionary<string, AudioData> audio;

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
        return Game.AppData() + "/" + Game.Get().gameType.TypeName() + "/import";
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

    /// <summary>
    /// Seach the provided path for all content packs and read meta data.</summary>
    /// <param name="path">Path to search for content packs.</param>
    public ContentData(string path)
    {
        // This is pack type for sorting packs
        loadedPacks = new HashSet<string>();

        // This is pack type for sorting packs
        packTypes = new Dictionary<string, PackTypeData>();

        // This is all of the available sides of tiles (not currently tracking physical tiles)
        tileSides = new Dictionary<string, TileSideData>();

        // Available heros
        heroes = new Dictionary<string, HeroData>();

        // Available classes
        classes = new Dictionary<string, ClassData>();

        // Available skills
        skills = new Dictionary<string, SkillData>();

        // Available items
        items = new Dictionary<string, ItemData>();

        // Available monsters
        monsters = new Dictionary<string, MonsterData>();

        //This has the game game and all expansions, general info
        allPacks = new List<ContentPack>();

        // This has all monster activations
        activations = new Dictionary<string, ActivationData>();

        // This has all available attacks
        investigatorAttacks = new Dictionary<string, AttackData>();

        // This has all available evades
        investigatorEvades = new Dictionary<string, EvadeData>();

        // This has all available evades
        horrorChecks = new Dictionary<string, HorrorData>();

        // This has all available tokens
        tokens = new Dictionary<string, TokenData>();

        // This has all avilable perils
        perils = new Dictionary<string, PerilData>();

        // This has all avilable puzzle images
        puzzles = new Dictionary<string, PuzzleData>();

        // This has all avilable general images
        images = new Dictionary<string, ImageData>();

        // This has all avilable puzzle images
        audio = new Dictionary<string, AudioData>();

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
        if (File.Exists(path + "/content_pack.ini"))
        {
            ContentPack pack = new ContentPack();

            // Get all data from the file
            IniData d = IniRead.ReadFromIni(path + "/content_pack.ini");
            // Todo: better error handling
            if (d == null)
            {
                ValkyrieDebug.Log("Failed to get any data out of " + path + "/content_pack.ini!");
                Application.Quit();
            }

            pack.name = d.Get("ContentPack", "name");
            if (pack.name.Equals(""))
            {
                ValkyrieDebug.Log("Failed to get name data out of " + path + "/content_pack.ini!");
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
                pack.image = path + "/" + d.Get("ContentPack", "image");
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
            files.Add(path + "/content_pack.ini");

            // No extra files is valid
            if (d.Get("ContentPackData") != null)
            {
                foreach (string file in d.Get("ContentPackData").Keys)
                {
                    files.Add(path + "/" + file);
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
                    dictFiles[id].Add(path + "/" + file);
                }
            }
            // Save list of files
            pack.localizationFiles = dictFiles;

            // Add content pack
            allPacks.Add(pack);

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

        foreach(string ini in cp.iniFiles)
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

        foreach(KeyValuePair<string, List<string>> kv in cp.localizationFiles)
        {
            DictionaryI18n packageDict = new DictionaryI18n();
            foreach(string file in kv.Value)
            {
                packageDict.AddDataFromFile(file);
            }

            LocalizationRead.AddDictionary(kv.Key, packageDict);
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
        // Is this a "PackType" entry?
        if(name.IndexOf(PackTypeData.type) == 0)
        {
            PackTypeData d = new PackTypeData(name, content, path);
            // Ignore invalid entry
            if (d.name.Equals(""))
                return;
            // If we don't already have one then add this
            if(!packTypes.ContainsKey(name))
            {
                packTypes.Add(name, d);
                d.sets.Add(packID);
            }
            // If we do replace if this has higher priority
            else if(packTypes[name].priority < d.priority)
            {
                packTypes.Remove(name);
                packTypes.Add(name, d);
            }
            // items of the same priority belong to multiple packs
            else if (packTypes[name].priority == d.priority)
            {
                packTypes[name].sets.Add(packID);
            }
        }

        // Is this a "TileSide" entry?
        if(name.IndexOf(TileSideData.type) == 0)
        {
            TileSideData d = new TileSideData(name, content, path);
            // Ignore invalid entry
            if (d.name.Equals(""))
                return;
            // If we don't already have one then add this
            if(!tileSides.ContainsKey(name))
            {
                tileSides.Add(name, d);
                d.sets.Add(packID);
            }
            // If we do replace if this has higher priority
            else if(tileSides[name].priority < d.priority)
            {
                tileSides.Remove(name);
                tileSides.Add(name, d);
            }
            // items of the same priority belong to multiple packs
            else if (tileSides[name].priority == d.priority)
            {
                tileSides[name].sets.Add(packID);
            }
        }

        // Is this a "Hero" entry?
        if (name.IndexOf(HeroData.type) == 0)
        {
            HeroData d = new HeroData(name, content, path);
            // Ignore invalid entry
            if (d.name.Equals(""))
                return;
            // If we don't already have one then add this
            if (!heroes.ContainsKey(name))
            {
                heroes.Add(name, d);
                d.sets.Add(packID);
            }
            // If we do replace if this has higher priority
            else if (heroes[name].priority < d.priority)
            {
                heroes.Remove(name);
                heroes.Add(name, d);
            }
            // items of the same priority belong to multiple packs
            else if (heroes[name].priority == d.priority)
            {
                heroes[name].sets.Add(packID);
            }
        }

        // Is this a "Class" entry?
        if (name.IndexOf(ClassData.type) == 0)
        {
            ClassData d = new ClassData(name, content, path);
            // Ignore invalid entry
            if (d.name.Equals(""))
                return;
            // If we don't already have one then add this
            if (!classes.ContainsKey(name))
            {
                classes.Add(name, d);
                d.sets.Add(packID);
            }
            // If we do replace if this has higher priority
            else if (classes[name].priority < d.priority)
            {
                classes.Remove(name);
                classes.Add(name, d);
            }
            // items of the same priority belong to multiple packs
            else if (classes[name].priority == d.priority)
            {
                classes[name].sets.Add(packID);
            }
        }

        // Is this a "Skill" entry?
        if (name.IndexOf(SkillData.type) == 0)
        {
            SkillData d = new SkillData(name, content, path);
            // Ignore invalid entry
            if (d.name.Equals(""))
                return;
            // If we don't already have one then add this
            if (!skills.ContainsKey(name))
            {
                skills.Add(name, d);
                d.sets.Add(packID);
            }
            // If we do replace if this has higher priority
            else if (skills[name].priority < d.priority)
            {
                skills.Remove(name);
                skills.Add(name, d);
            }
            // items of the same priority belong to multiple packs
            else if (skills[name].priority == d.priority)
            {
                skills[name].sets.Add(packID);
            }
        }

        // Is this a "Item" entry?
        if (name.IndexOf(ItemData.type) == 0)
        {
            ItemData d = new ItemData(name, content, path);
            // Ignore invalid entry
            if (d.name.Equals(""))
                return;
            // If we don't already have one then add this
            if (!items.ContainsKey(name))
            {
                items.Add(name, d);
                d.sets.Add(packID);
            }
            // If we do replace if this has higher priority
            else if (items[name].priority < d.priority)
            {
                items.Remove(name);
                items.Add(name, d);
            }
            // items of the same priority belong to multiple packs
            else if (items[name].priority == d.priority)
            {
                items[name].sets.Add(packID);
            }
        }

        // Is this a "Monster" entry?
        if (name.IndexOf(MonsterData.type) == 0)
        {
            MonsterData d = new MonsterData(name, content, path);
            // Ignore invalid entry
            if (d.name.Equals(""))
                return;
            // Ignore monster activations
            if (name.IndexOf(ActivationData.type) != 0)
            {
                // If we don't already have one then add this
                if (!monsters.ContainsKey(name))
                {
                    monsters.Add(name, d);
                    d.sets.Add(packID);
                }
                // If we do replace if this has higher priority
                else if (monsters[name].priority < d.priority)
                {
                    monsters.Remove(name);
                    monsters.Add(name, d);
                }
                // items of the same priority belong to multiple packs
                else if (monsters[name].priority == d.priority)
                {
                    monsters[name].sets.Add(packID);
                }
            }
        }

        // Is this a "Activation" entry?
        if (name.IndexOf(ActivationData.type) == 0)
        {
            ActivationData d = new ActivationData(name, content, path);
            // Ignore invalid entry
            if (d.name.Equals(""))
                return;
            // If we don't already have one then add this
            if (!activations.ContainsKey(name))
            {
                activations.Add(name, d);
                d.sets.Add(packID);
            }
            // If we do replace if this has higher priority
            else if (activations[name].priority < d.priority)
            {
                activations.Remove(name);
                activations.Add(name, d);
            }
            // items of the same priority belong to multiple packs
            else if (activations[name].priority == d.priority)
            {
                activations[name].sets.Add(packID);
            }
        }
        
        // Is this a "Attack" entry?
        if (name.IndexOf(AttackData.type) == 0)
        {
            AttackData d = new AttackData(name, content, path);
            // Ignore invalid entry
            if (d.name.Equals(""))
                return;
            // If we don't already have one then add this
            if (!investigatorAttacks.ContainsKey(name))
            {
                investigatorAttacks.Add(name, d);
                d.sets.Add(packID);
            }
            // If we do replace if this has higher priority
            else if (investigatorAttacks[name].priority < d.priority)
            {
                investigatorAttacks.Remove(name);
                investigatorAttacks.Add(name, d);
            }
            // items of the same priority belong to multiple packs
            else if (investigatorAttacks[name].priority == d.priority)
            {
                investigatorAttacks[name].sets.Add(packID);
            }
        }

        // Is this a "Evade" entry?
        if (name.IndexOf(EvadeData.type) == 0)
        {
            EvadeData d = new EvadeData(name, content, path);
            // Ignore invalid entry
            if (d.name.Equals(""))
                return;
            // If we don't already have one then add this
            if (!investigatorEvades.ContainsKey(name))
            {
                investigatorEvades.Add(name, d);
                d.sets.Add(packID);
            }
            // If we do replace if this has higher priority
            else if (investigatorEvades[name].priority < d.priority)
            {
                investigatorEvades.Remove(name);
                investigatorEvades.Add(name, d);
            }
            // items of the same priority belong to multiple packs
            else if (investigatorEvades[name].priority == d.priority)
            {
                investigatorEvades[name].sets.Add(packID);
            }
        }

        // Is this a "Horror" entry?
        if (name.IndexOf(HorrorData.type) == 0)
        {
            HorrorData d = new HorrorData(name, content, path);
            // Ignore invalid entry
            if (d.name.Equals(""))
                return;
            // If we don't already have one then add this
            if (!horrorChecks.ContainsKey(name))
            {
                horrorChecks.Add(name, d);
                d.sets.Add(packID);
            }
            // If we do replace if this has higher priority
            else if (horrorChecks[name].priority < d.priority)
            {
                horrorChecks.Remove(name);
                horrorChecks.Add(name, d);
            }
            // items of the same priority belong to multiple packs
            else if (horrorChecks[name].priority == d.priority)
            {
                horrorChecks[name].sets.Add(packID);
            }
        }

        // Is this a "Token" entry?
        if (name.IndexOf(TokenData.type) == 0)
        {
            TokenData d = new TokenData(name, content, path);
            // Ignore invalid entry
            if (d.name.Equals(""))
                return;
            // If we don't already have one then add this
            if (!tokens.ContainsKey(name))
            {
                tokens.Add(name, d);
                d.sets.Add(packID);
            }
            // If we do replace if this has higher priority
            else if (tokens[name].priority < d.priority)
            {
                tokens.Remove(name);
                tokens.Add(name, d);
            }
            // items of the same priority belong to multiple packs
            else if (tokens[name].priority == d.priority)
            {
                tokens[name].sets.Add(packID);
            }
        }

        // Is this a "Peril" entry?
        if (name.IndexOf(PerilData.type) == 0)
        {
            PerilData d = new PerilData(name, content);
            // Ignore invalid entry
            if (d.sectionName.Equals(""))
                return;
            // If we don't already have one then add this
            if (!perils.ContainsKey(name))
            {
                perils.Add(name, d);
            }
            // If we do replace if this has higher priority
            else if (perils[name].priority < d.priority)
            {
                perils.Remove(name);
                perils.Add(name, d);
            }
        }

        // Is this a "Puzzle" entry?
        if (name.IndexOf(PuzzleData.type) == 0)
        {
            PuzzleData d = new PuzzleData(name, content, path);
            // Ignore invalid entry
            if (d.name.Equals(""))
                return;
            // If we don't already have one then add this
            if (!puzzles.ContainsKey(name))
            {
                puzzles.Add(name, d);
                d.sets.Add(packID);
            }
            // If we do replace if this has higher priority
            else if (puzzles[name].priority < d.priority)
            {
                puzzles.Remove(name);
                puzzles.Add(name, d);
            }
            // items of the same priority belong to multiple packs
            else if (puzzles[name].priority == d.priority)
            {
                puzzles[name].sets.Add(packID);
            }
        }

        // Is this a "Image" entry?
        if (name.IndexOf(ImageData.type) == 0)
        {
            ImageData d = new ImageData(name, content, path);
            // Ignore invalid entry
            if (d.name.Equals(""))
                return;
            // If we don't already have one then add this
            if (!images.ContainsKey(name))
            {
                images.Add(name, d);
                d.sets.Add(packID);
            }
            // If we do replace if this has higher priority
            else if (images[name].priority < d.priority)
            {
                images.Remove(name);
                images.Add(name, d);
            }
            // items of the same priority belong to multiple packs
            else if (images[name].priority == d.priority)
            {
                images[name].sets.Add(packID);
            }
        }

        // Is this a "Audio" entry?
        if (name.IndexOf(AudioData.type) == 0)
        {
            AudioData d = new AudioData(name, content, path);
            // Ignore invalid entry
            if (d.name.Equals(""))
                return;
            // If we don't already have one then add this
            if (!audio.ContainsKey(name))
            {
                audio.Add(name, d);
            }
            // If we do replace if this has higher priority
            else if (audio[name].priority < d.priority)
            {
                audio.Remove(name);
                audio.Add(name, d);
            }
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

    public PackTypeData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
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
    public static new string type = "TileSide";

    public TileSideData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
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

    public HeroData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
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

    public ClassData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
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

    public SkillData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
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

    public ItemData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
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
            minFame = Fame(content["maxfame"]);
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

    public MonsterData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
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
                imagePlace = path + "/" + content["imageplace"];
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
        foreach (KeyValuePair<string, AttackData> kv in Game.Get().cd.investigatorAttacks)
        {
            if (ContainsTrait(kv.Value.target))
            {
                toReturn.Add(kv.Value.attackType);
            }
        }
        return toReturn;
    }

    virtual public StringKey GetRandomAttack(string type)
    {
        List<AttackData> validAttacks = new List<AttackData>();
        foreach (AttackData ad in Game.Get().cd.investigatorAttacks.Values)
        {
            if (ad.attackType.Equals(type))
            {
                validAttacks.Add(ad);
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

    public ActivationData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
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

    public TokenData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
    {
        init(content);
    }

    public TokenData(string name, Dictionary<string, string> content, string path, string typeIn) : base(name, content, path, typeIn)
    {
        init(content);
    }

    public void init(Dictionary<string, string> content)
    {
        if (content.ContainsKey("x"))
        {
            int.TryParse(content["x"], out x);
        }
        if (content.ContainsKey("y"))
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

    public AttackData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
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

    public EvadeData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
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

    public HorrorData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
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

    public PuzzleData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
    {
    }
}

// Class for images
public class ImageData : TokenData
{
    public static new string type = "Image";

    public ImageData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
    {
    }
}

// Class for Audio
public class AudioData : GenericData
{
    public static new string type = "Audio";
    public string file = "";

    public AudioData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
    {
        if (content.ContainsKey("file"))
        {
            if (content["file"].IndexOf("{import}") == 0)
            {
                file = ContentData.ImportPath() + content["file"].Substring(8);
            }
            else
            {
                file = path + "/" + content["file"];
            }
        }
    }
}

// Super class for all content loaded from content packs
public class GenericData
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
    // priority for duplicates
    public int priority;
    // for sub classes to set type
    public static string type = "";

    public GenericData()
    {
    }

    // generic constructor gets common things
    public GenericData(string name_ini, Dictionary<string, string> content, string path, string type)
    {
        sectionName = name_ini;
        sets = new List<string>();

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
public class PerilData : QuestData.Event
{
    new public static string type = "Peril";
    public int priority = 0;

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

        for (int i = 0; i < buttons.Count; i++)
        {
            if (data.ContainsKey("button" + (i + 1)))
            {
                buttons[i] = new StringKey(data["button" + (i + 1)]);
            }
            else
            {
                buttons[i] = StringKey.NULL;
            }
        }
    }
}