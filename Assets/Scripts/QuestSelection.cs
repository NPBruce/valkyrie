using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestSelection {
    public Dictionary<string, QuestLoader.Quest> questList;

    public QuestSelection(Dictionary<string, QuestLoader.Quest> ql)
    {
        questList = ql;

        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        new DialogBox(new Vector2(300, 50), new Vector2(100, 40), "Quest:");

        int offset = 100;
        foreach (KeyValuePair<string, QuestLoader.Quest> q in questList)
        {
            new TextButton(new Vector2(300, offset), new Vector2(100, 40), q.Value.name, delegate { Selection(q.Key); });
            offset += 50;
        }
    }

    public void Selection(string key)
    {
        Game game = Game.Get();

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        game.StartQuest(questList[key]);
    }
}
