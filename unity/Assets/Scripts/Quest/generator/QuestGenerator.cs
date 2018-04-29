using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using ValkyrieTools;

public class QuestGenerator
{
    public QuestGenerator(QuestData.Generator data, Dictionary<string, QuestData.QuestComponent> components)
    {
        ValkyrieDebug.Log("Generating Components.");
        GeneratorMap map = new GeneratorMap(data);

        ValkyrieDebug.Log("Building Map.");
        List<GeneratorMapSegment.MapComponent> mapComponents = map.Build();

        List<string> questTileIDs = new List<string>();

        foreach (GeneratorMapSegment.MapComponent tile in mapComponents)
        {
            string tileComponentName = GetUniqueName("Tile");
            QuestData.Tile generatedTile = new QuestData.Tile(tileComponentName, tile.componentName, tile.position.x, tile.position.y, tile.position.rotation);
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
        while (Game.Get().quest.qd.components.ContainsKey(baseName + uniqueId))
        {
            uniqueId++;
        }
        return baseName + uniqueId;
    }
}
