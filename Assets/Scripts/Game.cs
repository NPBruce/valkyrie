using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
Dump list of things to do:

import from RtL
64 bit
symbols in text
more activations
end of each round tag

    tag ideas:
    random
    expansions

duplicate build in zip
ffg app version unity
32bit
better import diagnosis
remove warnings
quest set
Threat
lt.
Add expansions, conent selection
Stacked tileselection? (tags?)
*/

// General controller for the game
public class Game : MonoBehaviour {

    public string version = "";
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
    public QuestEditorData qed;
    public string[] ffgText = null;

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

        // Read the version and add it to the log
        TextAsset versionFile = Resources.Load("version") as TextAsset;
        version = versionFile.text.Trim();
        // The newline at the end stops the stack trace appearing in the log
        Debug.Log("Valkyrie Version: " + version + System.Environment.NewLine);

        // Bring up the Game selector
        new GameSelection();
    }

    // This is called by 'start quest' on the main menu
    public void SelectQuest()
    {
        // Find any content packs at the location
        cd = new ContentData(ContentData.ContentPath() + "D2E/");
        // Check if we found anything
        if (cd.GetPacks().Count == 0)
        {
            Debug.Log("Error: Failed to find any content packs, please check that you have them present in: " + ContentData.ContentPath() + "D2E/" + System.Environment.NewLine);
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

        // This shouldn't happen!
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

    // This function adjusts morale.  We don't write directly so that NoMorale can be triggered
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
        // Count up how many heros have been selected
        int count = 0;
        foreach (Hero h in heros)
        {
            if (h.heroData != null) count++;
        }
        // Starting morale is number of heros
        morale = count;
        // Starting round is 1
        round = 1;
        // This validates the selection then if OK starts first quest event
        heroCanvas.EndSection();
    }

    // On quitting
    void OnApplicationQuit ()
    {
        // This exists for the editor, because quitting doesn't actually work.
        Destroyer.MainMenu();
        // Clean up temporary files
        QuestLoader.CleanTemp();
    }

    //  This is here because the editor doesn't get an update, so we are passing through mouse clicks to the editor
    void Update()
    {
        // 0 is the left mouse button
        if (qed != null && Input.GetMouseButtonDown(0))
        {
            qed.MouseDown();
        }
    }

    // Class for holding current hero status
    public class Hero
    {
        // This can be null if not selected
        public HeroData heroData;
        public bool activated = false;
        public bool defeated = false;
        //  Heros are in a list so they need ID... maybe at some point this can move to an array
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

        // Initialise from monster event
        public Monster(QuestData.Monster m)
        {
            monsterData = m.mData;
            unique = m.unique;
            uniqueTitle = m.uniqueTitle;
            uniqueText = m.uniqueText;
        }
    }
}

