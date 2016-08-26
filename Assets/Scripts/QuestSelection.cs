using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestSelection {

    public Dictionary<string, QuestLoader.Quest> questList;

    public QuestSelection(Dictionary<string, QuestLoader.Quest> ql)
    {
        Game game = GameObject.FindObjectOfType<Game>();

        questList = ql;

        foreach (KeyValuePair<string, QuestLoader.Quest> q in questList)
        {
            game.StartQuest(q.Value);
        }
    }
}
