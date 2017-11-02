using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class QuestGenerator
{
    List<GeneratorMapSegment> tiles = new List<GeneratorMapSegment>();

    GeneratorMapSegment output = new GeneratorMapSegment();

    public QuestGenerator(QuestData.Generator data, Dictionary<string, QuestComponent> components)
    {
        foreach (TileSideData t in Game.Get().cd.tiles)
        {
            tiles.Add(new GeneratorMapSegmentTile(t));
        }
    }

    public void GenerateComponents()
    {

    }


    public bool ComponentAvailable(string name, int count = 1)
    {
        if (name.IndexOf("TileSide") == 0)
        {
            return (Game.Get().cd.tiles[name].count >= (output.GetComponentCount(name) + count));
        }
        if (name.IndexOf("Token") == 0)
        {
            return (Game.Get().cd.tokens[name].count >= (output.GetComponentCount(name) + count));
        }
        return false;
    }

    public GeneratorMapSegment GenerateCluster(GeneratorMapJoint in = null)
    {
        if (in == null)
        {

        }
    }

    public GeneratorMapSegment RandomTile(GeneratorMapSegment checkJoinTo = null)
    {
        List<GeneratorMapSegment> availableTiles = new List<GeneratorMapSegment>();
        foreach (GeneratorMapSegment candidate in tiles)
        {
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

            availableTiles.Add(candidate, kv.);
        }
        return availableTiles[Rand.Range(0, availableTiles.Length)];
    }
}

