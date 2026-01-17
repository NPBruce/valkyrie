
// Class for tile specific data

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Content;
using UnityEngine;
using ValkyrieTools;
using System.Globalization;
using Random = UnityEngine.Random;
using Assets.Scripts;

public class PackTypeData : GenericData
{
    public static new string type = ValkyrieConstants.PackType;

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
            float.TryParse(content["top"], NumberStyles.Float, CultureInfo.InvariantCulture, out top);
        }
        if (content.ContainsKey("left"))
        {
            float.TryParse(content["left"], NumberStyles.Float, CultureInfo.InvariantCulture, out left);
        }

        // pixel per D2E square (inch) of image
        if (content.ContainsKey("pps"))
        {
            if (content["pps"].StartsWith("*"))
            {
                float.TryParse(content["pps"].Remove(0, 1), NumberStyles.Float, CultureInfo.InvariantCulture, out pxPerSquare);
                pxPerSquare *= Game.Get().gameType.TilePixelPerSquare();
            }
            else
            {
                float.TryParse(content["pps"], NumberStyles.Float, CultureInfo.InvariantCulture, out pxPerSquare);
            }
        }
        else
        {
            pxPerSquare = Game.Get().gameType.TilePixelPerSquare();
        }

        // Some MoM tiles have crazy aspect
        if (content.ContainsKey("aspect"))
        {
            float.TryParse(content["aspect"], NumberStyles.Float, CultureInfo.InvariantCulture, out aspect);
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
            items.AddRange(content["items"].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
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
            float.TryParse(content["health"], NumberStyles.Float, CultureInfo.InvariantCulture, out healthBase);
        }
        if (content.ContainsKey("healthperhero"))
        {
            float.TryParse(content["healthperhero"], NumberStyles.Float, CultureInfo.InvariantCulture, out healthPerHero);
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
            float.TryParse(content["pps"], NumberStyles.Float, CultureInfo.InvariantCulture, out pxPerSquare);
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
    public string SectionName => sectionName;
    
    public StringKey TranslationKey => name;

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

    public string SectionName => type;

    public StringKey TranslationKey => text; 

    override public StringKey text => perilText;

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
