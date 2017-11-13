using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class QuestGenerator
{
    public QuestGenerator(QuestData.Generator data, Dictionary<string, QuestComponent> components)
    {
        ValkyrieDebug.Log("Generating Components.");
        GeneratorMap map = new GeneratorMap(data);

        ValkyrieDebug.Log("Building Map.");
        map.Build();
    }
}

