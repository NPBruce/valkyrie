using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
Dump list of things to to:

Monster activation name lookup bugged
Always minion first why?

UI positons/scale
hero section
event triggers
event conditions
event flags
monster Info
moster activations
mouse pan
packaging
text background
big tokens

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
    public int round = 0;

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

        monsters = new List<Monster>();
    }

	// Update is called once per frame
	void Update () {
        // Escape will quit because we don't have a proper UI yet
        if (Input.GetKey("escape"))
           Application.Quit();
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
        public MonsterData monsterData;
        public bool activated = false;
        public bool minionStarted = false;
        public bool masterStarted = false;
        public ActivationData currentActivation;

        public Monster(MonsterData m)
        {
            monsterData = m;
        }
    }
}
