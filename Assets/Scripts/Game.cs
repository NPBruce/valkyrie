using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
Dump list of things to to:

UI positons/scale
hero section
more event triggers
mouse pan
packaging
text background
quit button (remove esc)

Hero selection on events
Hero names / highlights in events
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

    // Use this for initialization (before Start)
    void Awake () {
        // This will load content, need to work out where this should be stored, and how it should be packed
        if (Application.isEditor)
        {
            cd = new ContentData(Application.dataPath + "/../../valkyrie-contentpacks/");
        }
        else
        {
            cd = new ContentData(Application.dataPath + "/valkyrie-contentpacks/");
        }

        // In the future this is where you select which packs to load
        foreach(string pack in cd.GetPacks())
        {
            cd.LoadContent(pack);
        }
    }

    // Use this for initialization (things are set up, ready to start)
    void Start()
    {
        Dictionary<string, QuestLoader.Quest> ql = QuestLoader.GetQuests();

        new QuitButton();

        new QuestSelection(ql);
    }

    public void StartQuest(QuestLoader.Quest q)
    {
        qd = new QuestData(q);

        if (qd == null)
        {
            Debug.Log("Error: Invalid Quest.");
            Application.Quit();
        }

        // Working on Hero selection here (currently hard coded)
        heros = new List<Hero>();
        heros.Add(new Hero(cd.heros["HeroSyndrael"]));
        heros.Add(new Hero(cd.heros["HeroJainFairwood"]));
        HeroCanvas hc = FindObjectOfType<HeroCanvas>();
        hc.SetupUI();

        monsters = new List<Monster>();

        EventHelper.triggerEvent("EventStart");
    }

    // Update is called once per frame
    void Update () {
        // Escape will quit because we don't have a proper UI yet
        if (Input.GetKey("escape"))
           Application.Quit();
    }

    // On quitting
    void onApplicationQuit ()
    {
        // Clean up temporary files
        QuestLoader.CleanTemp();
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
