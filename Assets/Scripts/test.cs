using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {
    // Use this for initialization
    void Start () {

        Game game = FindObjectOfType<Game>();

        QuestData qd = new QuestData(Application.dataPath + "/../../valkyrie-quests/roag-intro/quest.ini", game);

        foreach(QuestData.Tile t in qd.tiles)
        {
            t.image.color = Color.white;
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}

