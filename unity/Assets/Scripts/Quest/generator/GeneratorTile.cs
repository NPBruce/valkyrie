using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class GeneratorTile
{
    public TileSideData cd;

    public Dictionary<int, Dictionary<int, GeneratorMapSpace>> map = new Dictionary<int, Dictionary<int, GeneratorMapSpace>>();

    public List<GeneratorTileJoin> joints = new List<GeneratorTileJoin>();

    public int available = 1;

    public int size = 0;

    public GeneratorTile(TileSideData t)
    {
        cd = t;
        available = cd.count;
        BuildMap();
    }

    protected void BuildMap()
    {
        for (int y = 0; y < cd.map.Length; y++)
        {
            char[] mapRow = cd.map[y].ToCharArray();
            for (int x = 0; x < mapRow.Length; x++)
            {
                GeneratorMapSpace space = GeneratorMapSpace.Void;
                if (map[y][x] == '#')
                {
                    space = GeneratorMapSpace.Terrain;
                }
                if (map[y][x] == '*')
                {
                    space = GeneratorMapSpace.Obstacle;
                }
                if (map[y][x] == '_')
                {
                    space = GeneratorMapSpace.Pit;
                }
                if (map[y][x] == '=')
                {
                    space = GeneratorMapSpace.Sludge;
                }
                if (map[y][x] == '!')
                {
                    space = GeneratorMapSpace.Hazard;
                }
                if (map[y][x] == '~')
                {
                    space = GeneratorMapSpace.Water;
                }

                if (space != GeneratorMapSpace.Void)
                {
                    SetSpace(x - cd.mapZero.x, y - cd.mapZero.y, space);
                    size++;
                }
                if (System.Convert.ToByte((map[y][x]) >= System.Convert.ToByte('A') && System.Convert.ToByte((map[y][x]) <= System.Convert.ToByte('Z'))
                {
                    joints.Add(new GeneratorTileJoin(x - cd.mapZero.x, y - cd.mapZero.y, map[y][x]))
                }
            }
        }
        for (int i = 0; i < joints.Length; i++)
        {
            // Possible join directions
            bool Top = GetSpace(joints[i].x, joints[i].y + 1) != GeneratorMapSpace.Void;
            bool Bottom = GetSpace(joints[i].x, joints[i].y - 1) != GeneratorMapSpace.Void;
            bool Left = GetSpace(joints[i].x + 1, joints[i].y) != GeneratorMapSpace.Void;
            bool Right = GetSpace(joints[i].x - 1, joints[i].y) != GeneratorMapSpace.Void;
            
            // For each type find valid join direction
            if (joints[i].IsTypeADerivative())
            {
                if (Top && !Right)
                {
                    joints[i].rotation = 0;
                }
                if (Right && !Bottom)
                {
                    joints[i].rotation = 90;
                }
                if (Bottom && !Left)
                {
                    joints[i].rotation = 180;
                }
                if (Left && !Top)
                {
                    joints[i].rotation = 270;
                }
            }
            else
            {
                if (Top && !Left)
                {
                    joints[i].rotation = 0;
                }
                if (Right && !Top)
                {
                    joints[i].rotation = 90;
                }
                if (Bottom && !Right)
                {
                    joints[i].rotation = 180;
                }
                if (Left && !Bottom)
                {
                    joints[i].rotation = 270;
                }
            }
        }
    }

    public GeneratorMapSpace GetSpace(int x, int y)
    {
        if (!map.ContainsKey(x)) return GeneratorMapSpace.Void;
        if (!map[x].ContainsKey(y)) return GeneratorMapSpace.Void;
        return map[x][y];
    }

    public void SetSpace(int x, int y, GeneratorMapSpace space)
    {
        if (!map.ContainsKey(x))
        {
            map.Add(x, new Dictionary<int, GeneratorMapSpace>());
        }
        if (!map[x].ContainsKey(y))
        {
            map.Add(y, space)
        }
        else
        {
            map[x][y] = space;
        }
    }
}
