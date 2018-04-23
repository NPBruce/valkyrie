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
        List<Tuple<string, GeneratorMapVector>> mapComponents = map.Build();

        List<sting> questTileIDs = new List<string>();

        foreach (Tuple<string, GeneratorMapVector> tile in mapComponents)
        {
            string tileComponentName = GetUniqueName("Tile");
            QuestData.Tile generatedTile = new QuestData.Tile(tileComponentName, tile.Item1, tile.Item2.x, tile.Item2.y, tile.Item2.rotation);
            components.Add(generatedTile.sectionName, generatedTile);
            questTileIDs.Add(generatedTile.sectionName);
        }

        string eventComponentName = GetUniqueName("Event");
        QuestData.Event startingEvent = new QuestData.Event(eventComponentName);
        
        startingEvent.display = true;
        startingEvent.buttons.Add(new StringKey("qst", startingEvent.sectionName + '.' + "button1"));
        startingEvent.buttonColors.Add("white");
        startingEvent.trigger = "EventStart";
        startingEvent.addComponents = questTileIDs.ToArray();
        startingEvent.locationSpecified = true;

        components.Add(startingEvent.sectionName, startingEvent);
    }

    private string GetUniqueName(string baseName)
    {
        int uniqueId = 0;
        while (components.ContainsKey(baseName + uniqueId))
        {
            uniqueId++;
        }
        return baseName + uniqueId;
    }
}
