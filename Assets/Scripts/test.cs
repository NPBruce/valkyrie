using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class test : MonoBehaviour {
    // Use this for initialization
    void Start () {

        Game game = FindObjectOfType<Game>();

        QuestData.Event CurrentEvent = game.qd.start;

        Debug.Log(CurrentEvent.text);
        while (!CurrentEvent.nextEvent.Equals(""))
        {
            CurrentEvent = (QuestData.Event)game.qd.components[CurrentEvent.nextEvent];
            Debug.Log(CurrentEvent.text);
            foreach(string s in CurrentEvent.addComponents)
            {
                game.qd.components[s].setVisible(true);
            }
        }

        foreach(KeyValuePair<string, HeroData> h in game.cd.heros)
        {
            Debug.Log(h.Value.name);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

