using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
Dump list of things to do:

system menu (editor, end quest, quit, about)
CleanUp quest at end

import from RtL
> activations
> symbols in text
> Threat
> Add expansions
> > conent selection
> > Stacked tileselection
*/

// General controller for the game
public class Game : MonoBehaviour {

    public static string version = "0.1.0";
    public ContentData cd;
    public QuestData qd;
    public List<Hero> heros;
    public List<Monster> monsters;
    public int round = 0;
    public bool heroesSelected = false;
    public Stack<QuestData.Event> eventList;
    public Canvas uICanvas;
    public Canvas boardCanvas;
    public Canvas tokenCanvas;
    public TokenBoard tokenBoard;
    public HeroCanvas heroCanvas;
    public MonsterCanvas monsterCanvas;
    public UIScaler uiScaler;
    public int morale;
    public MoraleDisplay moraleDisplay;
    public bool editMode = false;

    // This is used all over the place to find the game object.  Game then provides acces to common objects
    public static Game Get()
    {
        return FindObjectOfType<Game>();
    }

    // Unity fires off this function
    void Start()
    {

        // Find the common objects we use.  These are created by unity.
        uICanvas = GameObject.Find("UICanvas").GetComponent<Canvas>();
        boardCanvas = GameObject.Find("BoardCanvas").GetComponent<Canvas>();
        tokenCanvas = GameObject.Find("TokenCanvas").GetComponent<Canvas>();
        tokenBoard = GameObject.FindObjectOfType<TokenBoard>();
        heroCanvas = GameObject.FindObjectOfType<HeroCanvas>();
        monsterCanvas = GameObject.FindObjectOfType<MonsterCanvas>();

        // Create some things
        eventList = new Stack<QuestData.Event>();
        uiScaler = new UIScaler(uICanvas);

        // Bring up the main menu
        new MainMenu();
    }
    public void SelectQuest()
    {
        // In the build the content packs need to go into the build data dir, this is currently manual
        string contentLocation = Application.dataPath + "/valkyrie-contentpacks/";
        if (Application.isEditor)
        {
            // If running through unity then we assume you are using the git content, with the project at the same level
            contentLocation = Application.dataPath + "/../../valkyrie-contentpacks/";
        }

        // Find any content packs at the location
        cd = new ContentData(contentLocation);
        // Check if we found anything
        if (cd.GetPacks().Count == 0)
        {
            Debug.Log("Error: Failed to find any content packs, please check that you have them present in: " + contentLocation);
        }

        // In the future this is where you select which packs to load, for now we load everything.
        foreach(string pack in cd.GetPacks())
        {
            cd.LoadContent(pack);
        }

        // Get a list of available quests
        Dictionary<string, QuestLoader.Quest> ql = QuestLoader.GetQuests();

        // Pull up the quest selection page
        new QuestSelection(ql);
    }

    // This is called when a quest is selected
    public void StartQuest(QuestLoader.Quest q)
    {
        // Fetch all of the quest data
        qd = new QuestData(q);

        if (qd == null)
        {
            Debug.Log("Error: Invalid Quest.");
            Application.Quit();
        }

        // Populate null hero list, these can then be selected as hero types
        heros = new List<Hero>();
        for (int i = 1; i < 5; i++)
        {
            heros.Add(new Hero(null, i));
        }
        // Draw the hero icons, which are buttons for selection
        heroCanvas.SetupUI();

        // Add a finished button to start the quest
        TextButton endSelection = new TextButton(new Vector2(UIScaler.GetRight(-9), UIScaler.GetBottom(-3)), new Vector2(8, 2), "Finished", delegate { EndSelection(); }, Color.green);
        // Untag as dialog so this isn't cleared away during hero selection
        endSelection.ApplyTag("heroselect");

        TextButton cancelSelection = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Back", delegate { Destroyer.QuestSelect(); }, Color.red);
        // Untag as dialog so this isn't cleared away during hero selection
        cancelSelection.ApplyTag("heroselect");

        // Create the monster list so we are ready to start
        monsters = new List<Monster>();
    }

    public void AdjustMorale(int m)
    {
        morale += m;
        if(morale < 0)
        {
            morale = 0;
            moraleDisplay.Update();
            EventHelper.EventTriggerType("NoMorale");
        }
        moraleDisplay.Update();
    }

    // HeroCanvas validates selection and starts quest if everything is good
    public void EndSelection()
    {
        int count = 0;
        foreach (Hero h in heros)
        {
            if (h.heroData != null) count++;
        }
        morale = count;
        round = 0;
        heroCanvas.EndSection();
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
        // This can be null if not selected
        public HeroData heroData;
        public bool activated = false;
        public bool defeated = false;
        public int id = 0;
        // Used for events that can select or highlight heros
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
        public bool unique = false;
        public string uniqueText = "";
        public string uniqueTitle = "";
        // Activation is reset each round so that master/minion are the same and forcing doesn't re roll
        public ActivationData currentActivation;

        public Monster(MonsterData m)
        {
            monsterData = m;
        }

        public Monster(QuestData.Monster m)
        {
            monsterData = m.mData;
            unique = m.unique;
            uniqueTitle = m.uniqueTitle;
            uniqueText = m.uniqueText;
        }
    }
}

