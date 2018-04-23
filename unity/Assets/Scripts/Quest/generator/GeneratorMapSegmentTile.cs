using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class GeneratorMapSegmentTile : GeneratorMapSegment
{
    public GeneratorMapSegmentTile(TileSideData t)
    {
        components.Add(t.sectionName, new GeneratorMapVector());
        BuildMap(t);
    }

    protected void BuildMap(TileSideData tileSideData)
    {
        for (int y = 0; y < tileSideData.map.Length; y++)
        {
            char[] mapRow = tileSideData.map[y].ToCharArray();
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
                    SetSpace(x - tileSideData.mapZero.x, y - tileSideData.mapZero.y, space);
                }
                if (System.Convert.ToByte(map[y][x]) >= System.Convert.ToByte('A') && System.Convert.ToByte(map[y][x]) <= System.Convert.ToByte('Z'))
                {
                    joints.Add(new GeneratorMapJoint(new GeneratorMapVectory(x - tileSideData.mapZero.x, y - tileSideData.mapZero.y), map[y][x]))
                }
            }
        }
        for (int i = 0; i < joints.Length; i++)
        {
            GeneratorMapVector jointLocation = joints[i].location;
            // Possible join directions
            bool Top = GetSpace(jointLocation.x, jointLocation.y + 1) != GeneratorMapSpace.Void;
            bool Bottom = GetSpace(joints[i].x, jointLocation.y - 1) != GeneratorMapSpace.Void;
            bool Left = GetSpace(jointLocation.x + 1, jointLocation.y) != GeneratorMapSpace.Void;
            bool Right = GetSpace(jointLocation.x - 1, jointLocation.y) != GeneratorMapSpace.Void;
            
            // For each type find valid join direction
            if (joints[i].IsTypeADerivative())
            {
                if (Top && !Right)
                {
                    jointLocation.rotation = 0;
                    jointLocation.x += 0.5f;
                    jointLocation.y -= 0.5f;
                }
                if (Right && !Bottom)
                {
                    jointLocation.rotation = 90;
                    jointLocation.x -= 0.5f;
                    jointLocation.y -= 0.5f;
                }
                if (Bottom && !Left)
                {
                    jointLocation.rotation = 180;
                    jointLocation.x -= 0.5f;
                    jointLocation.y += 0.5f;
                }
                if (Left && !Top)
                {
                    jointLocation.rotation = 270;
                    jointLocation.x += 0.5f;
                    jointLocation.y += 0.5f;
                }
            }
            else
            {
                if (Top && !Left)
                {
                    jointLocation.rotation = 0;
                    jointLocation.x -= 0.5f;
                    jointLocation.y -= 0.5f;
                }
                if (Right && !Top)
                {
                    jointLocation.rotation = 90;
                    jointLocation.x -= 0.5f;
                    jointLocation.y += 0.5f;
                }
                if (Bottom && !Right)
                {
                    jointLocation.rotation = 180;
                    jointLocation.x += 0.5f;
                    jointLocation.y += 0.5f;
                }
                if (Left && !Bottom)
                {
                    jointLocation.rotation = 270;
                    jointLocation.x += 0.5f;
                    jointLocation.y -= 0.5f;
                }
            }
        }
    }
}
