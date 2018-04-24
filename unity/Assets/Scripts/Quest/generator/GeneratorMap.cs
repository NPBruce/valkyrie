using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using ValkyrieTools;

public class GeneratorMap
{
    List<GeneratorMapSegment> tiles = new List<GeneratorMapSegment>();

    GeneratorMapSegment output = new GeneratorMapSegment();

    public GeneratorMap(QuestData.Generator data)
    {
        ValkyrieDebug.Log("Building map data for all tiles");
        foreach (TileSideData t in Game.Get().cd.tileSides.Values)
        {
            tiles.Add(new GeneratorMapSegmentTile(t));
        }
    }

    /// <summary>
    /// Build a random map</summary>
    /// <returns>List of component names and locations</returns>
    public List<GeneratorMapSegment.MapComponent> Build()
    {
        for (int i = 0; i < 1000; i++)
        {
            ValkyrieDebug.Log("Map build attempt " + (i+1));
            if (GenerateComponents()) break;
        }
        return output.components;
    }

    /// <summary>
    /// Generate map</summary>
    /// <returns>True if generation completed</returns>
    private bool GenerateComponents()
    {
        int startRotation = Random.Range(0, 4) * 90;
        output = new GeneratorMapSegment(tiles[Random.Range(0, tiles.Count)], startRotation);

        while (output.Size() < 30)
        {
            // We are closed off and too small
            if (output.joints.Count == 0)
            {
                ValkyrieDebug.Log("Map closed off before reaching target size");
                return false;
            }

            //No forks
            GeneratorMapSegment randomTile = RandomTile(output.joints.Count > 1, output);
            // Unable to find a matching tile
            if (randomTile == null)
            {
                ValkyrieDebug.Log("No tile can be added to map");
                return false;
            }
            output.Merge(RandomTile(true, output));
        }

        ValkyrieDebug.Log("Closing open joints");
        while (output.joints.Count > 0)
        {
            GeneratorMapSegment randomTile = RandomTile(true, output);
            // Unable to find a matching tile
            if (randomTile == null)
            {
                ValkyrieDebug.Log("No tile can be added to map");
                return false;
            }
            output.Merge(RandomTile(true, output));
        }

        return true;
    }

    /// <summary>
    /// Check if additional components are available</summary>
    /// <param name="name">Name of component to check</param>
    /// <param name="count">Number of components required</param>
    /// <returns>True if all components available</returns>
    public bool ComponentAvailable(string name, int count = 0)
    {
        if (name.IndexOf("TileSide") == 0)
        {
            return (Game.Get().cd.tileSides[name].count > (output.GetComponentCount(name) + count));
        }
        if (name.IndexOf("Token") == 0)
        {
            return (Game.Get().cd.tokens[name].count > (output.GetComponentCount(name) + count));
        }
        return false;
    }

    /// <summary>
    /// Check if additional components are available</summary>
    /// <param name="name">Name of component to check</param>
    /// <param name="count">Number of components required</param>
    /// <returns>True if all components available</returns>
    public GeneratorMapSegment RandomTile(bool singleJoint, GeneratorMapSegment checkJoinTo = null)
    {
        List<GeneratorMapSegment> availableTiles = new List<GeneratorMapSegment>();
        foreach (GeneratorMapSegment candidate in tiles)
        {
            if (candidate.joints.Count == 0) continue;
            if ((candidate.joints.Count == 1) && !singleJoint) continue;
            if ((candidate.joints.Count > 1) && singleJoint) continue;

            bool componentsAvailable = true;
            foreach (KeyValuePair<string, int> kv in candidate.GetComponentCounts())
            {
                if (!ComponentAvailable(kv.Key, kv.Value))
                {
                    componentsAvailable = false;
                }
            }
            if (!componentsAvailable) continue;

            if (checkJoinTo != null && checkJoinTo.ValidMergeJoints(candidate).Count == 0) continue;

            availableTiles.Add(candidate);
        }
        if (availableTiles.Count == 0) return null;

        return availableTiles[Random.Range(0, availableTiles.Count)];
    }
}

