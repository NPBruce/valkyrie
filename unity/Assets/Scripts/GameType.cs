using Assets.Scripts.Content;
using System;
using UnityEngine;

// GameType manages setting that are specific to the game type
public abstract class GameType
{
    public abstract string DataDirectory();
    public abstract StringKey HeroName();
    public abstract StringKey HeroesName();
    public abstract StringKey QuestName();
    public abstract int MaxHeroes();
    public abstract int DefaultHeroes();
    public abstract bool DisplayHeroes();
    public abstract float TilePixelPerSquare();
    // There are actually two fonts, should expand to include header/text
    public abstract Font GetFont();
    public abstract Font GetHeaderFont();
    public abstract string TypeName();
    public abstract bool TileOnGrid();
    public abstract bool DisplayMorale();
    public abstract float SelectionRound();
    public abstract float TileRound();
    public abstract bool MonstersGrouped();
}

// NoGameType exists for management reasons
// Perhaps this should be the base and others inherit from this to simplify this class?
public class NoGameType : GameType
{
    public override string DataDirectory()
    {
        return ContentData.ContentPath();
    }

    public override StringKey HeroName()
    {
        return new StringKey("val","D2E_HERO_NAME");
    }

    public override StringKey HeroesName()
    {
        return new StringKey("val","D2E_HEROES_NAME");
    }

    public override StringKey QuestName()
    {
        return new StringKey("val", "D2E_QUEST_NAME");
    }

    public override Font GetFont()
    {
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    public override Font GetHeaderFont()
    {
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    public override int MaxHeroes()
    {
        return 0;
    }

    public override int DefaultHeroes()
    {
        return 0;
    }

    public override bool DisplayHeroes()
    {
        return true;
    }

    public override float TilePixelPerSquare()
    {
        return 1f;
    }

    public override string TypeName()
    {
        return "";
    }

    public override bool TileOnGrid()
    {
        return true;
    }

    public override bool DisplayMorale()
    {
        return false;
    }

    // Number of squares for snap of objects in editor
    public override float SelectionRound()
    {
        return 1f;
    }

    // Number of squares for snap of tiles in editor
    public override float TileRound()
    {
        return 1f;
    }

    public override bool MonstersGrouped()
    {
        return true;
    }
}

// Things for D2E
public class D2EGameType : GameType
{
    public override string DataDirectory()
    {
        return ContentData.ContentPath() + "D2E/";
    }

    public override StringKey HeroName()
    {
        return new StringKey("val", "D2E_HERO_NAME");
    }

    public override StringKey HeroesName()
    {
        return new StringKey("val", "D2E_HEROES_NAME");
    }

    public override StringKey QuestName()
    {
        return new StringKey("val", "D2E_QUEST_NAME");
    }

    // There are actually two fonts, should expand to include header/text
    public override Font GetFont()
    {
        return (Font)Resources.Load("fonts/gara_scenario_desc");
    }

    public override Font GetHeaderFont()
    {
        return (Font)Resources.Load("fonts/windl");
    }

    public override int MaxHeroes()
    {
        return 4;
    }

    public override int DefaultHeroes()
    {
        return 4;
    }

    public override bool DisplayHeroes()
    {
        return true;
    }

    // Tiles imported from RtL have 105 pixels per square (each 1 inch)
    public override float TilePixelPerSquare()
    {
        return 105;
    }

    public override string TypeName()
    {
        return "D2E";
    }

    public override bool TileOnGrid()
    {
        return true;
    }
    public override bool DisplayMorale()
    {
        return true;
    }

    public override float SelectionRound()
    {
        return 1f;
    }

    public override float TileRound()
    {
        return 1f;
    }

    public override bool MonstersGrouped()
    {
        return true;
    }
}

class MoMGameType : GameType
{
    public override string DataDirectory()
    {
        return ContentData.ContentPath() + "MoM/";
    }

    public override StringKey HeroName()
    {
        return new StringKey("val", "MOM_HERO_NAME");
    }

    public override StringKey HeroesName()
    {
        return new StringKey("val", "MOM_HEROES_NAME");
    }

    public override StringKey QuestName()
    {
        return new StringKey("val", "MOM_QUEST_NAME");
    }

    public override Font GetFont()
    {
        return (Font)Resources.Load("fonts/MADGaramondPro");
    }

    public override Font GetHeaderFont()
    {
        return (Font)Resources.Load("fonts/oldnewspapertypes");
    }

    public override int MaxHeroes()
    {
        return 10;
    }

    public override int DefaultHeroes()
    {
        return 5;
    }

    public override bool DisplayHeroes()
    {
        return false;
    }

    public override float TilePixelPerSquare()
    {
        // the base side of the tile is 1024 pixels, we are having 3.5 'squares' (3.5 inches) in this
        // These squares are the same size as D2E squares
        return 1024f / 3.5f;
    }

    public override string TypeName()
    {
        return "MoM";
    }

    public override bool TileOnGrid()
    {
        return false;
    }
    public override bool DisplayMorale()
    {
        return false;
    }

    // Number of squares for snap of objects in editor
    public override float SelectionRound()
    {
        return 1.75f;
    }

    // Number of squares for snap of tiles in editor
    public override float TileRound()
    {
        return 3.5f;
    }

    public override bool MonstersGrouped()
    {
        return false;
    }
}