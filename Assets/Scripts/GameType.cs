using System;
using UnityEngine;

public abstract class GameType
{
    public abstract string DataDirectory();
    public abstract string HeroName();
    public abstract string HeroesName();
    public abstract string QuestName();
    public abstract int MaxHeroes();
    public abstract bool DisplayHeroes();
    public abstract float TilePixelPerSquare();
    public abstract Font GetFont();
    public abstract string TypeName();
    public abstract bool TileOnGrid();
    public abstract bool DisplayMorale();
    public abstract float SelectionRound();
    public abstract float TileRound();
}

public class NoGameType : GameType
{
    public override string DataDirectory()
    {
        return ContentData.ContentPath();
    }

    public override string HeroName()
    {
        return "Hero";
    }

    public override string HeroesName()
    {
        return "Heroes";
    }

    public override string QuestName()
    {
        return "Quest";
    }

    public override Font GetFont()
    {
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    public override int MaxHeroes()
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

    public override float SelectionRound()
    {
        return 1f;
    }

    public override float TileRound()
    {
        return 1f;
    }
}

public class D2EGameType : GameType
{
    public override string DataDirectory()
    {
        return ContentData.ContentPath() + "D2E/";
    }

    public override string HeroName()
    {
        return "Hero";
    }

    public override string HeroesName()
    {
        return "Heroes";
    }

    public override string QuestName()
    {
        return "Quest";
    }

    public override Font GetFont()
    {
        return (Font)Resources.Load("fonts/gara_scenario_desc");
    }

    public override int MaxHeroes()
    {
        return 4;
    }

    public override bool DisplayHeroes()
    {
        return true;
    }


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
}

class MoMGameType : GameType
{
    public override string DataDirectory()
    {
        return ContentData.ContentPath() + "MoM/";
    }

    public override string HeroName()
    {
        return "Investigator";
    }

    public override string HeroesName()
    {
        return "Investigators";
    }

    public override string QuestName()
    {
        return "Scenario";
    }

    public override Font GetFont()
    {
        return (Font)Resources.Load("fonts/OldNewspaperTypes");
    }

    public override int MaxHeroes()
    {
        return 5;
    }

    public override bool DisplayHeroes()
    {
        return false;
    }

    public override float TilePixelPerSquare()
    {
        // the base side of the tile is 1024 pixels, we are having 3 'squares' in this
        // These squares are slightly larger than D2E squares
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

    public override float SelectionRound()
    {
        return 1.75f;
    }

    public override float TileRound()
    {
        return 3.5f;
    }
}