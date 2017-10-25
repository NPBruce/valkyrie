using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class QuestGenerator
{
    List<GeneratorTile> tiles = new List<GeneratorTile>();

    public QuestGenerator(QuestData.Generator data, Dictionary<string, QuestComponent> components)
    {
        foreach (TileSideData t in Game.Get().cd.tiles)
        {
            tiles.Add(new GeneratorTile(t));
        }
    }

    private RemoveTile(string name)
    {
        foreach (GeneratorTile t in tiles)
        {
            if (t.cd.sectionName.Equals(name)
            {
                t.available--;
            }
            if (t.cd.reverse.Equals(name)
            {
                t.available--;
            }
        }
    }

    private Dictionary<int, Dictionary<int, GeneratorMapSpace> BuildCluster()
    {

    }
}

