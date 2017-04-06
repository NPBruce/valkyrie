using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Content;

// This class reads and stores all of the content for a base game and expansions
public class ContentData {

    public HashSet<string> loadedPacks;
    public List<ContentPack> allPacks;
    public Dictionary<string, PackTypeData> packTypes;
    public Dictionary<string, TileSideData> tileSides;
    public Dictionary<string, HeroData> heros;
    public Dictionary<string, ItemData> items;
    public Dictionary<string, MonsterData> monsters;
    public Dictionary<string, ActivationData> activations;
    public Dictionary<string, AttackData> investigatorAttacks;
    public Dictionary<string, EvadeData> investigatorEvades;
    public Dictionary<string, HorrorData> horrorChecks;
    public Dictionary<string, TokenData> tokens;
    public Dictionary<string, PerilData> perils;
    public Dictionary<string, PuzzleData> puzzles;
    public Dictionary<string, AudioData> audio;

    public static string ContentPath()
    {
        if (Application.isEditor)
        {
            // If running through unity then we assume you are using the git content, with the project at the same level
            return Application.dataPath + "/../content/";
        }
        return Application.dataPath + "/content/";
    }

    // Constructor takes a path in which to look for content
    public ContentData(string path)
    {
        // This is pack type for sorting packs
        loadedPacks = new HashSet<string>();

        // This is pack type for sorting packs
        packTypes = new Dictionary<string, PackTypeData>();

        // This is all of the available sides of tiles (not currently tracking physical tiles)
        tileSides = new Dictionary<string, TileSideData>();

        // Available heros
        heros = new Dictionary<string, HeroData>();

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
            pack.image = path + "/" + d.Get("ContentPack", "image");

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

            // Add content pack
            allPacks.Add(pack);

            // We finish without actually loading the content, this is done later (content optional)
        }
    }

    // Return a list of names for all found content packs
    public List<string> GetPacks()
    {
        List<string> names = new List<string>();
        foreach(ContentPack cp in allPacks)
        {
            names.Add(cp.name);
        }
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
                loadedPacks.Add(cp.id);
            }
        }

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
            if (!heros.ContainsKey(name))
            {
                heros.Add(name, d);
                d.sets.Add(packID);
            }
            // If we do replace if this has higher priority
            else if (heros[name].priority < d.priority)
            {
                heros.Remove(name);
                heros.Add(name, d);
            }
            // items of the same priority belong to multiple packs
            else if (heros[name].priority == d.priority)
            {
                heros[name].sets.Add(packID);
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
            }
            // If we do replace if this has higher priority
            else if (puzzles[name].priority < d.priority)
            {
                puzzles.Remove(name);
                puzzles.Add(name, d);
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
        public List<string> clone;
    }

    // Get a unity texture from a file (dds or other unity supported format)
    public static Texture2D FileToTexture(string file)
    {
        return FileToTexture(file, Vector2.zero, Vector2.zero);
    }

    // Get a unity texture from a file (dds or other unity supported format)
    // Crop to pos and size in pixels
    public static Texture2D FileToTexture(string file, Vector2 pos, Vector2 size)
    {
        string imagePath = @"file://" + file;
        WWW www = null;
        Texture2D texture = null;

        // Unity doesn't support dds directly, have to do hackery
        if (Path.GetExtension(file).Equals(".dds"))
        {
            // Read the data
            byte[] ddsBytes = null;
            try
            {
                ddsBytes = File.ReadAllBytes(file);
            }
            catch (System.Exception)
            {
                ValkyrieDebug.Log("Warning: DDS Image missing: " + file);
                return null;
            }
            // Check for valid header
            byte ddsSizeCheck = ddsBytes[4];
            if (ddsSizeCheck != 124)
            {
                ValkyrieDebug.Log("Warning: Image invalid: " + file);
                return null;
            }

            // Extract dimensions
            int height = ddsBytes[13] * 256 + ddsBytes[12];
            int width = ddsBytes[17] * 256 + ddsBytes[16];

            // Copy image data (skip header)
            int DDS_HEADER_SIZE = 128;
            byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
            System.Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

            // Create empty texture
            texture = new Texture2D(width, height, TextureFormat.DXT5, false);
            // Load data into texture
            try
            {
                texture.LoadRawTextureData(dxtBytes);
            }
            catch (System.Exception)
            {
                texture = new Texture2D(width, height, TextureFormat.DXT1, false);
                texture.LoadRawTextureData(dxtBytes);
            }
            texture.Apply();
        }
        else
        // If the image isn't DDS just use unity file load
        {
            try
            {
                www = new WWW(@"file://" + imagePath);
                texture = new Texture2D(256, 256, TextureFormat.DXT5, false);
                www.LoadImageIntoTexture(texture);
            }
            catch (System.Exception)
            {
                ValkyrieDebug.Log("Warning: Image missing: " + file);
                return null;
            }
        }
        // Get whole image
        if (size.x == 0) return texture;

        // Get part of the image
        // Array of pixels from image
        Color[] pix = texture.GetPixels(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y));
        // Empty texture
        Texture2D subTexture = new Texture2D(Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y));
        // Set pixels
        subTexture.SetPixels(pix);
        subTexture.Apply();
        return subTexture;
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
            float.TryParse(content["pps"], out pxPerSquare);
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

// Class for Item specific data
public class ItemData : GenericData
{
    public static new string type = "Item";
    public bool unique = false;

    public ItemData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
    {
        if (name.IndexOf("ItemUnique") == 0)
        {
            unique = true;
        }
    }
}

// Class for Hero specific data
public class MonsterData : GenericData
{
    public StringKey info = new StringKey("-", false);
    public string imagePlace;
    public static new string type = "Monster";
    public string[] activations;
    public float health = 0;
    
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
            imagePlace = path + "/" + content["imageplace"];
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
            float.TryParse(content["health"], out health);
        }
    }
}

// Class for Activation specific data
public class ActivationData : GenericData
{
    public StringKey ability = new StringKey("-", false);
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

// Class for Audio
public class AudioData : GenericData
{
    public static new string type = "Audio";
    public string file = "";

    public AudioData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
    {
        if (content.ContainsKey("file"))
        {
            file = path + "/" + content["file"];
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
            name = new StringKey(name_ini.Substring(type.Length));
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
        if (content.ContainsKey("image"))
        {
            image = path + "/" + content["image"];
        }
        else // No image is a valid condition
        {
            image = "";
        }
    }

    // Does the component contain a trait?
    public bool ContainsTrait(string trait)
    {
        bool t = false;
        foreach (string s in traits)
        {
            if (trait.Equals(s))
            {
                t = true;
            }
        }
        return t;
    }
}

// Perils are content data that inherits from QuestData for reasons.
public class PerilData : QuestData.Event
{
    new public static string type = "Peril";
    public string monster = "";
    public int priority = 0;

    public PerilData(string name, Dictionary<string, string> data) : base(name, data)
    {
        typeDynamic = type;
        if (data.ContainsKey("monster"))
        {
            monster = data["monster"];
        }
        if (data.ContainsKey("priority"))
        {
            int.TryParse(data["priority"], out priority);
        }
    }
}