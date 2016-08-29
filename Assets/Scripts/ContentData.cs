using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// This class reads and stores all of the content for a base game and expansions
public class ContentData {

    List<ContentPack> allPacks;
    public Dictionary<string, TileSideData> tileSides;
    public Dictionary<string, HeroData> heros;
    public Dictionary<string, MonsterData> monsters;
    public Dictionary<string, ActivationData> activations;

    // Constructor takes a path in which to look for content
    public ContentData(string path)
    {
        Debug.Log("Searching for content in: \"" + path + "\"");

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
                AddContent(section.Key, section.Value, Path.GetDirectoryName(ini));
            }
        }
    }

    // Add a section of an ini file to game content
    // name is from the ini file and must start with the type
    // path is relative and is used for images or other paths in the content
    void AddContent(string name, Dictionary<string, string> content, string path)
    {
        // Is this a "TileSide" entry?
        if(name.IndexOf(TileSideData.type) == 0)
        {
            TileSideData d = new TileSideData(name, content, path);
            // Ignore invalid entry
            if (d.name.Equals(""))
                return;
            // If we don't already have one then add this
            if(!tileSides.ContainsKey(d.name))
            {
                tileSides.Add(name, d);
            }
            // If we do replace if this has higher priority
            else if(tileSides[d.name].priority < d.priority)
            {
                tileSides.Remove(name);
                tileSides.Add(name, d);
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
            if (!heros.ContainsKey(d.name))
            {
                heros.Add(name, d);
            }
            // If we do replace if this has higher priority
            else if (heros[d.name].priority < d.priority)
            {
                heros.Remove(name);
                heros.Add(name, d);
            }
        }

        // Is this a "Monster" entry?
        if (name.IndexOf(MonsterData.type) == 0)
        {
            MonsterData d = new MonsterData(name, content, path);
            // Ignore invalid entry
            if (d.name.Equals(""))
                return;
            // If we don't already have one then add this
            if (!monsters.ContainsKey(d.name))
            {
                monsters.Add(name, d);
            }
            // If we do replace if this has higher priority
            else if (monsters[d.name].priority < d.priority)
            {
                monsters.Remove(name);
                monsters.Add(name, d);
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
            if (!activations.ContainsKey(d.name))
            {
                activations.Add(name, d);
            }
            // If we do replace if this has higher priority
            else if (activations[d.name].priority < d.priority)
            {
                activations.Remove(name);
                activations.Add(name, d);
            }
        }
    }

    // Holding class for contentpack data
    class ContentPack
    {
        public string name;
        public string image;
        public string description;
        public List<string> iniFiles;
    }
}

// Class for tile specific data
public class TileSideData : GenericData
{
    public float top;
    public float left;
    public static new string type = "TileSide";

    public TileSideData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
    {
        // Get location of top left square in tile image, default 0
        if (content.ContainsKey("top"))
        {
            top = float.Parse(content["top"]);
        }
        else
        {
            top = 0;
        }

        if (content.ContainsKey("left"))
        {
            left = float.Parse(content["left"]);
        }
        else
        {
            left = 0;
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
            masterFirst = bool.Parse(content["masterfirst"]);
        }
        if (content.ContainsKey("minionfirst"))
        {
            minionFirst = bool.Parse(content["minionfirst"]);
        }
    }
}

// Super class for all content loaded from content packs
public class GenericData
{
    // name from section title or data
    public string name;
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

    // generic constructor gets common things
    public GenericData(string name_ini, Dictionary<string, string> content, string path, string type)
    {
        sectionName = name_ini;

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
        if (content.ContainsKey("priority"))
        {
            priority = int.Parse(content["priority"]);
        }
        else // Default piority is 0
        {
            priority = 0;
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