using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class GeneratorCluster
{
    public Dictionary<string, int> usage;

    List<GeneratorTile> tiles;

    public Dictionary<int, Dictionary<int, GeneratorMapSpace>> map = new Dictionary<int, Dictionary<int, GeneratorMapSpace>>();

    public List<GeneratorTileJoin> joints = new List<GeneratorTileJoin>();

    public GeneratorCluster(List<GeneratorTile> tilesIn, GeneratorTileJoin joinIn = null)
    {
        tiles = tilesIn;
    }

    public bool AddRandomTile()
    {

    }

    public List<GeneratorTileJoin> CheckTile(GeneratorTile tile, GeneratorTileJoin joint)
    {
        List<GeneratorTileJoin> toReturn = new List<GeneratorTileJoin>();
        if (!CheckAvailable(tile)) return toReturn;

        foreach (GeneratorTileJoin t in tile.joints)
        {
            // Must be matching joint type
            if (t.type != joint.type) continue;

            if (CheckJoin(tile, joint, t))
            {
                toReturn.Add(t);
            }
        }

        return toReturn;
    }

    private bool CheckJoin(GeneratorTile tile, GeneratorTileJoin clusterJoint, GeneratorTileJoin tileJoint)
    {
        int tileRotation = clusterJoint.rotation - tileJoint.rotation;

        if (tileRotation < 0)
        {
            tileRotation += 360;
        }

        return CheckCollision(map, tile.map, GetXInsert(clusterJoint, tileJoint), GetYInsert(clusterJoint, tileJoint), tileRotation);
    }

    private int GetYInsert(GeneratorTileJoin baseJoint, GeneratorTileJoin AddJoint)
    {
        int tileRotation = baseJoint.rotation - AddJoint.rotation;
        if (tileRotation < 0)
        {
            tileRotation += 360;
        }
        int yInsert = baseJoint.y;

        if (tileRotation == 0)
        {
            yInsert -= AddJoint.y;
        }
        if (tileRotation == 90)
        {
            yInsert -= AddJoint.x;
        }
        if (tileRotation == 180)
        {
            yInsert += AddJoint.y;
        }
        if (tileRotation == 270)
        {
            yInsert += AddJoint.x;
        }

        if (baseJoint.rotation == 0)
        {
            yInsert++;
        }
        if (baseJoint.rotation == 90)
        {
            yInsert -= baseJoint.TypeAAsSign();
        }
        if (baseJoint.rotation == 180)
        {
            yInsert--;
        }
        if (baseJoint.rotation == 270)
        {
            yInsert += baseJoint.TypeAAsSign();
        }
        return yInsert;
    }

    private int GetXInsert(GeneratorTileJoin baseJoint, GeneratorTileJoin AddJoint)
    {
        int tileRotation = baseJoint.rotation - AddJoint.rotation;
        if (tileRotation < 0)
        {
            tileRotation += 360;
        }
        int xInsert = baseJoint.x;

        if (tileRotation == 0)
        {
            xInsert -= AddJoint.x;
        }
        if (tileRotation == 90)
        {
            xInsert += AddJoint.y;
        }
        if (tileRotation == 180)
        {
            xInsert += AddJoint.x;
        }
        if (tileRotation == 270)
        {
            xInsert -= AddJoint.y;
        }

        if (baseJoint.rotation == 0)
        {
            xInsert -= baseJoint.TypeAAsSign();
        }
        if (baseJoint.rotation == 90)
        {
            xInsert--;
        }
        if (baseJoint.rotation == 180)
        {
            xInsert += baseJoint.TypeAAsSign();
        }
        if (baseJoint.rotation == 270)
        {
            xInsert++;
        }
        return xInsert;
    }

    public bool CheckCollision(Dictionary<int, Dictionary<int, GeneratorMapSpace>> mapA, Dictionary<int, Dictionary<int, GeneratorMapSpace>> mapB, int x, int y, int rotation)
    {
        foreach (KeyValuePair<int, Dictionary<int, GeneratorMapSpace> row in mapB)
        {
            foreach (KeyValuePair<int, GeneratorMapSpace> space in row.Value)
            {
                if (space.Value == GeneratorMapSpace.Void) continue;

                int yTranslated = y;
                int xTranslated = x;

                if (rotation = 0)
                {
                    xTranslated += space.Key;
                    yTranslated += row.Key;
                }
                if (rotation = 90)
                {
                    xTranslated -= row.Key;
                    yTranslated += space.Key;
                }
                if (rotation = 180)
                {
                    xTranslated -= space.Key;
                    yTranslated -= row.Key;
                }
                if (rotation = 270)
                {
                    xTranslated += row.Key;
                    yTranslated -= space.Key;
                }

                if (GetSpace(mapA, xTranslated, yTranslated) != GeneratorMapSpace.Void) return false;
            }
        }
        return true;
    }

    public bool AddTile(GeneratorTile tile, GeneratorTileJoin clusterJoint, GeneratorTileJoin tileJoint)
    {
        // Mark usage
        foreach (GeneratorTile t in tiles)
        {
            if (t.cd.reverse.Equals(tile.cd.sectionName))
            {
                if (usage.ContainsKey(t.cd.reverse))
                {
                    usage[t.cd.reverse]++;
                }
                else
                {
                    usage.Add(t.cd.reverse, 1);
                }
            }
        }
        if (usage.ContainsKey(tile.cd.sectionName))
        {
            usage[tile.cd.sectionName]++;
        }
        else
        {
            usage.Add(tile.cd.sectionName, 1);
        }

        int tileRotation = clusterJoint.rotation - tileJoint.rotation;
        if (tileRotation < 0)
        {
            tileRotation += 360;
        }
        CheckCollision(map, tile.map, GetXInsert(clusterJoint, tileJoint), GetYInsert(clusterJoint, tileJoint), tileRotation);
        foreach (KeyValuePair<int, Dictionary<int, GeneratorMapSpace> row in tile.map)
        {
            foreach (KeyValuePair<int, GeneratorMapSpace> space in row.Value)
            {
                if (space.Value == GeneratorMapSpace.Void) continue;

                int yTranslated = y;
                int xTranslated = x;

                if (tileRotation = 0)
                {
                    xTranslated += space.Key;
                    yTranslated += row.Key;
                }
                if (tileRotation = 90)
                {
                    xTranslated -= row.Key;
                    yTranslated += space.Key;
                }
                if (tileRotation = 180)
                {
                    xTranslated -= space.Key;
                    yTranslated -= row.Key;
                }
                if (tileRotation = 270)
                {
                    xTranslated += row.Key;
                    yTranslated -= space.Key;
                }

                SetSpace(map, xTranslated, yTranslated, space.Value);
            }
        }
    }

    public GeneratorMapSpace GetSpace(Dictionary<int, Dictionary<int, GeneratorMapSpace>> map, int x, int y)
    {
        if (!map.ContainsKey(x)) return GeneratorMapSpace.Void;
        if (!map[x].ContainsKey(y)) return GeneratorMapSpace.Void;
        return map[x][y];
    }

    public bool CheckAvailable(GeneratorTile tile);
    {
        int clusterUse = 0;
        if (usage.ContainsKey(tile.cd.sectionName))
        {
            clusterUse = usage[sectionName];
        }
        return (tile.available - clusterUse) > 0;
    }
}
