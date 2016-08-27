using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
Dump list of things to to:

UI positons/scale
mouse pan
packaging
Quest documentation
example quest
review: errors/comments
Bing
color from name
EventHeroPlaceFall text bug

tokens/doors in the same space
specific place monsters
unique monsters
extra event types
threat
content selection
symbols in text
    */


// General controller for the game
public class Game : MonoBehaviour {

    public ContentData cd;
    public QuestData qd;
    public List<Hero> heros;
    public List<Monster> monsters;
    public int round = 0;
    public bool heroesSelected = false;
    public DialogWindow currentDialog;

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

        for (int i = 1; i < 5; i++)
        {
            heros.Add(new Hero(null, i));
        }
        HeroCanvas hc = FindObjectOfType<HeroCanvas>();
        hc.SetupUI();

        TextButton endSelection = new TextButton(new Vector2(50, 550), new Vector2(100, 40), "Finished", delegate { EndSelection(); });
        // Untag as dialog so this isn't cleared away
        endSelection.background.tag = "heroselect";
        endSelection.button.tag = "heroselect";

        monsters = new List<Monster>();
    }

    public void EndSelection()
    {
        HeroCanvas hc = FindObjectOfType<HeroCanvas>();
        hc.EndSection();
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
        public int id = 0;
        public bool selected;

        public Hero(HeroData h, int i)
        {
            heroData = h;
            id = i;
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
