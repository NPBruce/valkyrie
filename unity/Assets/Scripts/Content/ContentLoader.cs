using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Content;
using UnityEngine;
using ValkyrieTools;

public class ContentLoader
{
    private static readonly List<IContentLoader> CONTENT_TYPE_LOADERS = new List<IContentLoader>()
    {
        new PackTypeDataLoader(),
        new TileSideDataLoader(),
        new HeroDataLoader(),
        new ClassDataLoader(),
        new SkillDataLoader(),
        new ItemDataLoader(),
        new ActivationDataLoader(),
        new MonsterDataLoader(),
        new AttackDataLoader(),
        new EvadeDataLoader(),
        new HorrorDataLoader(),
        new TokenDataLoader(),
        new PerilDataLoader(),
        new PuzzleDataLoader(),
        new ImageDataLoader(),
        new AudioDataLoader()
    };


    private readonly ContentData cd;

    public ContentLoader(ContentData cd)
    {
        this.cd = cd;
    }

    internal static void AddNewContentPack(Game game, string path)
    {
        game.cd.AddPackByGameType(true, true, path);
    }

    internal static void RemoveContentPack(Game game, string key)
    {
        var pack = game.cd.allPacks.FirstOrDefault(p => p.id.Equals(key));
        game.cd.allPacks.Remove(pack);
        game.cd.packSymbolDict.Remove(key);
    }

    internal static void GetContentData(Game game)
    {
        game.cd = new ContentData(game.gameType.DataDirectory());
        // Check if we found anything
        if (game.cd.GetPacks().Count == 0)
        {
            ValkyrieDebug.Log("Error: Failed to find any content packs, please check that you have them present in: " + game.gameType.DataDirectory() + Environment.NewLine);
            Application.Quit();
        }
    }

    // This loads content from a pack by name
    // Duplicate content will be replaced by the higher priority value
    public void LoadContent(string name)
    {
        foreach (ContentPack cp in cd.allPacks)
        {
            if (cp.name.Equals(name))
            {
                LoadContent(cp);
            }
        }
    }

    // This loads content from a pack by ID
    // Duplicate content will be replaced by the higher priority value
    public void LoadContentID(string id)
    {
        foreach (ContentPack cp in cd.allPacks)
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
        if (cd.loadedPacks.Contains(cp.id)) return;

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
            foreach (KeyValuePair<string, Dictionary<string, string>> section in d.data)
            {
                LoadContent(section.Key, section.Value, Path.GetDirectoryName(ini), cp.id);
            }
        }


        cd.loadedPacks.Add(cp.id);

        foreach (string s in cp.clone)
        {
            LoadContentID(s);
        }
    }

    // Add a section of an ini file to game content
    // name is from the ini file and must start with the type
    // path is relative and is used for images or other paths in the content
    void LoadContent(string name, Dictionary<string, string> content, string path, string packID)
    {
        foreach (var loader in CONTENT_TYPE_LOADERS.Where(l => l.Supports(name)))
        {
            var handled = loader.LoadContent(cd, name, content, path, new List<string> { packID });
            if (handled) break;
        }
    }
}


public interface IContentLoader
{
    bool Supports(string name);

    bool LoadContent(ContentData cd, string name, Dictionary<string, string> content, string path, List<string> sets);
}

public abstract class ContentLoader<T> : IContentLoader where T : IContent
{
    public virtual bool Supports(string name)
    {
        return name.StartsWith(TypePrefix);
    }

    public bool LoadContent(ContentData cd, string name, Dictionary<string, string> content, string path,
        List<string> sets)
    {
        T t = Create(name, content, path, sets);

        if (string.IsNullOrWhiteSpace(t?.SectionName))
        {
            ValkyrieDebug.Log($"Ignored invalid entry {name}");
            return false;
        }

        var isNew = cd.AddContent(name, t);
        if (isNew && AdditionalTranslation)
        {
            t.Sets.ForEach(id => LocalizationRead.RegisterKeyInGroup(t.TranslationKey, id));
        }

        return true;
    }

    protected virtual bool AdditionalTranslation { get; } = false;

    protected abstract string TypePrefix { get; }

    protected abstract T Create(string name, Dictionary<string, string> content, string path,
        List<string> sets);
}

public class PackTypeDataLoader : ContentLoader<PackTypeData>
{
    protected override string TypePrefix => PackTypeData.type;

    protected override PackTypeData Create(string name, Dictionary<string, string> content, string path,
        List<string> sets)
        => new PackTypeData(name, content, path, sets);
}

public class TileSideDataLoader : ContentLoader<TileSideData>
{
    protected override string TypePrefix => TileSideData.type;

    protected override TileSideData Create(string name, Dictionary<string, string> content, string path,
        List<string> sets)
        => new TileSideData(name, content, path, sets);

    protected override bool AdditionalTranslation { get; } = true;
}

public class HeroDataLoader : ContentLoader<HeroData>
{
    protected override string TypePrefix => HeroData.type;

    protected override HeroData Create(string name, Dictionary<string, string> content, string path, List<string> sets)
        => new HeroData(name, content, path, sets);

    protected override bool AdditionalTranslation { get; } = true;
}

public class ClassDataLoader : ContentLoader<ClassData>
{
    protected override string TypePrefix => ClassData.type;

    protected override ClassData Create(string name, Dictionary<string, string> content, string path, List<string> sets)
        => new ClassData(name, content, path, sets);

    protected override bool AdditionalTranslation { get; } = true;
}

public class SkillDataLoader : ContentLoader<SkillData>
{
    protected override string TypePrefix => SkillData.type;

    protected override SkillData Create(string name, Dictionary<string, string> content, string path, List<string> sets)
        => new SkillData(name, content, path, sets);

    protected override bool AdditionalTranslation { get; } = true;
}

public class ItemDataLoader : ContentLoader<ItemData>
{
    protected override string TypePrefix => ItemData.type;

    protected override ItemData Create(string name, Dictionary<string, string> content, string path, List<string> sets)
        => new ItemData(name, content, path, sets);

    protected override bool AdditionalTranslation { get; } = true;
}

public class MonsterDataLoader : ContentLoader<MonsterData>
{
    protected override string TypePrefix => MonsterData.type;

    protected override MonsterData Create(string name, Dictionary<string, string> content, string path,
        List<string> sets)
        => new MonsterData(name, content, path, sets);

    protected override bool AdditionalTranslation { get; } = true;
}

public class ActivationDataLoader : ContentLoader<ActivationData>
{
    protected override string TypePrefix => ActivationData.type;

    protected override ActivationData Create(string name, Dictionary<string, string> content, string path,
        List<string> sets)
        => new ActivationData(name, content, path, sets);
}

public class AttackDataLoader : ContentLoader<AttackData>
{
    protected override string TypePrefix => AttackData.type;

    protected override AttackData Create(string name, Dictionary<string, string> content, string path,
        List<string> sets)
        => new AttackData(name, content, path, sets);
}

public class EvadeDataLoader : ContentLoader<EvadeData>
{
    protected override string TypePrefix => EvadeData.type;

    protected override EvadeData Create(string name, Dictionary<string, string> content, string path, List<string> sets)
        => new EvadeData(name, content, path, sets);
}

public class HorrorDataLoader : ContentLoader<HorrorData>
{
    protected override string TypePrefix => HorrorData.type;

    protected override HorrorData Create(string name, Dictionary<string, string> content, string path,
        List<string> sets)
        => new HorrorData(name, content, path, sets);
}

public class TokenDataLoader : ContentLoader<TokenData>
{
    protected override string TypePrefix => TokenData.type;

    protected override TokenData Create(string name, Dictionary<string, string> content, string path,
        List<string> sets)
    {
        TokenData d = new TokenData(name, content, path, sets);

        if (d.image.Equals(""))
        {
            ValkyrieDebug.Log("Token " + d.name + "did not have an image. Skipping");
            return null;
        }

        return d;
    }
}

public class PerilDataLoader : ContentLoader<PerilData>
{
    protected override string TypePrefix => PerilData.type;

    protected override PerilData Create(string name, Dictionary<string, string> content, string path, List<string> sets)
        => new PerilData(name, content);
}

public class PuzzleDataLoader : ContentLoader<PuzzleData>
{
    protected override string TypePrefix => PuzzleData.type;

    protected override PuzzleData Create(string name, Dictionary<string, string> content, string path,
        List<string> sets)
        => new PuzzleData(name, content, path, sets);
}

public class ImageDataLoader : ContentLoader<ImageData>
{
    protected override string TypePrefix => ImageData.type;

    protected override ImageData Create(string name, Dictionary<string, string> content, string path,
        List<string> sets)
        => new ImageData(name, content, path, sets);
}

public class AudioDataLoader : ContentLoader<AudioData>
{
    protected override string TypePrefix => AudioData.type;

    protected override AudioData Create(string name, Dictionary<string, string> content, string path,
        List<string> sets)
        => new AudioData(name, content, path, sets);
}