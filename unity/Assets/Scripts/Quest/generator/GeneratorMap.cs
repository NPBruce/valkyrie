using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class GeneratorMap
{
    List<GeneratorMapSegment> tiles = new List<GeneratorMapSegment>();

    GeneratorMapSegment output = new GeneratorMapSegment();

    public GeneratorMap(QuestData.Generator data, Dictionary<string, QuestComponent> components)
    {
        foreach (TileSideData t in Game.Get().cd.tiles)
        {
            tiles.Add(new GeneratorMapSegmentTile(t));
        }
    }

    public void GenerateComponents()
    {
        output = tiles[Random.Range(0, tiles.Count)].Copy();

        while (output.Size() < 30)
        {
            if (output.joints.Count > 2)
            {
                output.Merge(RandomTile(true, output));
            }
        }
    }


    public bool ComponentAvailable(string name, int count = 0)
    {
        if (name.IndexOf("TileSide") == 0)
        {
            return (Game.Get().cd.tiles[name].count > (output.GetComponentCount(name) + count));
        }
        if (name.IndexOf("Token") == 0)
        {
            return (Game.Get().cd.tokens[name].count > (output.GetComponentCount(name) + count));
        }
        return false;
    }

    public GeneratorMapSegment RandomTile(bool singleJoint, GeneratorMapSegment checkJoinTo = null)
    {
        List<GeneratorMapSegment> availableTiles = new List<GeneratorMapSegment>();
        foreach (GeneratorMapSegment candidate in tiles)
        {
            if (tiles.joints.Count == 0) continue;
            if ((tiles.joints.Count == 1) && !singleJoint) continue;
            if ((tiles.joints.Count > 1) && singleJoint) continue;

            bool componentsAvailable = true;
            foreach (KeyValuePair<Dictionary<string, int>> kv in candidate.GetComponentCounts())
            {
                if (!ComponentAvailable(kv.Key, kv.Value))
                {
                    componentsAvailable = false;
                }
            }
            if (!componentsAvailable) continue;

            if (checkJoinTo != null && !checkJoinTo.CheckMerge(candidate).Count == 0) continue;

            availableTiles.Add(candidate);
        }
        return availableTiles[Random.Range(0, availableTiles.Count)];
    }
}

