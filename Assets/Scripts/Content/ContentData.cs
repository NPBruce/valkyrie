using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// This class reads and stores all of the content for a base game and expansions
public class ContentData {

    public List<ContentPack> allPacks;
    public Dictionary<string, TileSideData> tileSides;
    public Dictionary<string, HeroData> heros;
    public Dictionary<string, MonsterData> monsters;
    public Dictionary<string, ActivationData> activations;
    public Dictionary<string, TokenData> tokens;
    public Dictionary<string, PerilData> perils;

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
        // This is all of the available sides of tiles (not currently tracking physical tiles)
        tileSides = new Dictionary<string, TileSideData>();

        // Available heros
        heros = new Dictionary<string, HeroData>();

        // Available monsters
        monsters = new Dictionary<string, MonsterData>();

        //This has the game game and all expansions, general info
        allPacks = new List<ContentPack>();

        // This has all monster activations
        activations = new Dictionary<string, ActivationData>();

        // This has all available tokens
        tokens = new Dictionary<string, TokenData>();

        // This has all avilable perils
        perils = new Dictionary<string, PerilData>();

        // Search each directory in the path (one should be base game, others expansion.  Names don't matter
        string[] contentDirectories = Directory.GetDirectories(path);
        foreach (string p in contentDirectories)
        {
            // All packs must have a content_pack.ini, otherwise ignore
            if (File.Exists(p + "/content_pack.ini"))
            {
                ContentPack pack = new ContentPack();
                
                // Get all data from the file
                IniData d = IniRead.ReadFromIni(p + "/content_pack.ini");
                // Todo: better error handling
                if (d == null)
                {
                    Debug.Log("Failed to get any data out of " + p + "/content_pack.ini!");
                    Application.Quit();
                }

                pack.name = d.Get("ContentPack", "name");
                if (pack.name.Equals(""))
                {
                    Debug.Log("Failed to get name data out of " + p + "/content_pack.ini!");
                    Application.Quit();
                }

                // id can be empty/missing
                pack.id = d.Get("ContentPack", "id");

                // If this is invalid we will just handle it later, not fatal
                pack.image = p + "/" + d.Get("ContentPack", "image");

                // Black description isn't fatal
                pack.description = d.Get("ContentPack", "description");

                // Get all the other ini files in the pack
                List<string> files = new List<string>();
                // content_pack file is included
                files.Add(p + "/content_pack.ini");

                // No extra files is valid
                if (d.Get("ContentPackData") != null)
                {
                    foreach (string file in d.Get("ContentPackData").Keys)
                    {
                        files.Add(p + "/" + file);
                    }
                }
                // Save list of files
                pack.iniFiles = files;
                
                // Add content pack
                allPacks.Add(pack);

                // We finish without actually loading the content, this is done later (content optional)
            }
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

    // Return a list of names for all enbaled content packs
    public List<string> GetEnabledPacks()
    {
        Game game = Game.Get();
        List<string> names = new List<string>();
        Dictionary<string, string> setPacks = game.config.data.Get(game.gameType.TypeName() + "Packs");
        foreach (ContentPack cp in allPacks)
        {
            if (cp.id.Length == 0)
            {
                names.Add(cp.name);
            }
            if (setPacks.ContainsKey(cp.id))
            {
                names.Add(cp.name);
            }
        }
        return names;
    }

    // Return a list of id for all enbaled content packs
    public List<string> GetEnabledPackIDs()
    {
        Game game = Game.Get();
        List<string> ids = new List<string>();
        Dictionary<string, string> setPacks = game.config.data.Get(game.gameType.TypeName() + "Packs");
        foreach (ContentPack cp in allPacks)
        {
            if (cp.id.Length == 0)
            {
                ids.Add(cp.id);
            }
            if (setPacks.ContainsKey(cp.id))
            {
                ids.Add(cp.id);
            }
        }
        return ids;
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
            }
        }
    }

    // Add a section of an ini file to game content
    // name is from the ini file and must start with the type
    // path is relative and is used for images or other paths in the content
    void AddContent(string name, Dictionary<string, string> content, string path, string packID)
    {
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
            if (d.name.Equals(""))
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
    }

    // Holding class for contentpack data
    public class ContentPack
    {
        public string name;
        public string image;
        public string description;
        public string id;
        public List<string> iniFiles;
    }

    public static Texture2D FileToTexture(string file)
    {
        return FileToTexture(file, Vector2.zero, Vector2.zero);
    }

    public static Texture2D FileToTexture(string file, Vector2 pos, Vector2 size)
    {
        string imagePath = @"file://" + file;
        WWW www = null;
        Texture2D texture = null;

        if (Path.GetExtension(file).Equals(".dds"))
        {
            byte[] ddsBytes = null;
            try
            {
                ddsBytes = File.ReadAllBytes(file);
            }
            catch (System.Exception)
            {
                return null;
            }
            byte ddsSizeCheck = ddsBytes[4];
            if (ddsSizeCheck != 124)
            {
                return null;
            }

            int height = ddsBytes[13] * 256 + ddsBytes[12];
            int width = ddsBytes[17] * 256 + ddsBytes[16];

            int DDS_HEADER_SIZE = 128;
            byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
            System.Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

            texture = new Texture2D(width, height, TextureFormat.DXT5, false);
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
        {
            try
            {
                www = new WWW(@"file://" + imagePath);
                texture = new Texture2D(256, 256, TextureFormat.DXT5, false);
                www.LoadImageIntoTexture(texture);
            }
            catch (System.Exception)
            {
                return null;
            }
        }
        // Get whole image
        if (size.x == 0) return texture;

        // Get part of the image
        Color[] pix = texture.GetPixels(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y));
        Texture2D subTexture = new Texture2D(Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y));
        subTexture.SetPixels(pix);
        subTexture.Apply();
        return subTexture;
    }
}

// Class for tile specific data
public class TileSideData : GenericData
{
    public float top = 0;
    public float left = 0;
    public float pxPerSquare;
    public float aspect = 0;
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

        if (content.ContainsKey("pps"))
        {
            float.TryParse(content["pps"], out pxPerSquare);
        }
        else
        {
            pxPerSquare = Game.Get().gameType.TilePixelPerSquare();
        }

        if (content.ContainsKey("aspect"))
        {
            float.TryParse(content["aspect"], out aspect);
        }
    }
}

// Class for Hero specific data
public class HeroData : GenericData
{
    public string archetype = "warrior";
    public static new string type = "Hero";

    public HeroData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
    {
        // Get archetype
        if (content.ContainsKey("archetype"))
        {
            archetype = content["archetype"];
        }
    }
}

// Class for Hero specific data
public class MonsterData : GenericData
{
    public string info = "-";
    public string imagePlace;
    public static new string type = "Monster";
    public string[] activations;
    
    // This constuctor only exists for the quest version of this class to use to do nothing
    public MonsterData()
    {
    }

    public MonsterData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
    {
        // Get usage info
        if (content.ContainsKey("info"))
        {
            info = content["info"];
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
    }
}

// Class for Activation specific data
public class ActivationData : GenericData
{
    public string ability = "-";
    public string minionActions = "-";
    public string masterActions = "-";
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
            ability = content["ability"];
        }
        // Get minion activation info
        if (content.ContainsKey("minion"))
        {
            minionActions = content["minion"];
        }
        // Get master activation info
        if (content.ContainsKey("master"))
        {
            masterActions = content["master"];
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
        if (content.ContainsKey("height"))
        {
            int.TryParse(content["height"], out height);
        }
        if (content.ContainsKey("height"))
        {
            int.TryParse(content["width"], out width);
        }
    }

    public bool FullImage()
    {
        if (height == 0) return true;
        if (width == 0) return true;
        return false;
    }
}

// Super class for all content loaded from content packs
public class GenericData
{
    // name from section title or data
    public string name;
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
            name = content["name"];
        }
        else
        // If not use section name without type as name
        {
            name = name_ini.Substring(type.Length);
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

public class PerilData : QuestData.Event
{
    new public static string type = "Peril";
    public string monster = "";
    public int priority = 0;
    public PerilType pType = PerilType.na;

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
        if (name.IndexOf("PerilMinor") == 0)
        {
            pType = PerilType.minor;
        }
        if (name.IndexOf("PerilMajor") == 0)
        {
            pType = PerilType.major;
        }
        if (name.IndexOf("PerilDeadly") == 0)
        {
            pType = PerilType.deadly;
        }
    }

    public enum PerilType
    {
        na,
        minor,
        major,
        deadly
    }
}