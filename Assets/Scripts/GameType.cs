using System;
using UnityEngine;

public abstract class GameType
{
    public abstract string DataDirectory();
    public abstract string HeroName();
    public abstract string HeroesName();
    public abstract string QuestName();
    public abstract int MaxHeroes();
    public abstract int TilePixelPerSquare();
    public abstract Font GetFont();
    public abstract string TypeName();
    public abstract bool TileOnGrid();
    public abstract bool DisplayMorale();
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

    public override int TilePixelPerSquare()
    {
        return 1;
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

    public override int TilePixelPerSquare()
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

    public override int TilePixelPerSquare()
    {
        return 341;
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

}