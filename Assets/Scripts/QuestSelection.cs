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

        DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 3), "Select Quest");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();

        int offset = 5;
        foreach (KeyValuePair<string, QuestLoader.Quest> q in questList)
        {
            string key = q.Key;
            TextButton tb = new TextButton(new Vector2(2, offset), new Vector2(UIScaler.GetWidthUnits() - 4, 1.2f), "  " + q.Value.name, delegate { Selection(key); }, Color.white, offset);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
            tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0.1f);
            offset += 2;
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
