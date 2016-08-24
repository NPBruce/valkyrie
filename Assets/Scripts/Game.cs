using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
Dump list of things to to:

hero section
monster events (point)
event triggers
event conditions
event flags
monster UI
moster activations
hero activations
mouse scroll
comments
error handling
packaging
text background
bigtokens
dialog interlock

tokens/doors in the same space
specific place monsters
unique monsters
extra event types
threat
content selection

    */


// General controller for the game
public class Game : MonoBehaviour {

    public ContentData cd;
    public QuestData qd;
    public List<Hero> heros;
    public List<Monster> monsters;

    // Use this for initialization
    void Awake () {
        // This will load content, need to work out where this should be stored, and how it should be packed
        cd = new ContentData(Application.dataPath + "/../../valkyrie-contentpacks/");

        // In the future this is where you select which packs to load
        foreach(string pack in cd.GetPacks())
        {
            cd.LoadContent(pack);
        }

        // In the future this is where you select a quest, need to work out where this should be stored, and how it should be delivered
        qd = new QuestData(Application.dataPath + "/../../valkyrie-quests/roag-intro/quest.ini");

        // Working on Hero selection here (currently hard coded)
        heros = new List<Hero>();
        heros.Add(new Hero(cd.heros["HeroSyndrael"]));
        heros.Add(new Hero(cd.heros["HeroJainFairwood"]));
    }

	// Update is called once per frame
	void Update () {
        // Escape will quit because we don't have a proper UI yet
        if (Input.GetKey("escape"))
           Application.Quit();
    }

    public void triggerEvent(string name)
    {
        QuestData.Event e = (QuestData.Event)qd.components[name];

        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        new DialogWindow(e);
        foreach (string s in e.addComponents)
        {
            qd.components[s].setVisible(true);
        }
        foreach (string s in e.removeComponents)
        {
            qd.components[s].setVisible(false);
        }

        if (e.locationSpecified)
        {
            Camera cam = FindObjectOfType<Camera>();
            cam.transform.position = new Vector3(e.location.x * 105, e.location.y * 105, -800);
        }
    }

    // Class for holding current hero status
    public class Hero
    {
        public HeroData heroData;
        public bool activated = false;
        public bool defeated = false;

        public Hero(HeroData h)
        {
            heroData = h;
        }
    }

    // Class for holding current monster status
    public class Monster
    {

    }
}
