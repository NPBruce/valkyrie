using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// General controller for the game
public class Game : MonoBehaviour {

    public string version = "";
    public ContentData cd;
    public Quest quest;
    public Canvas uICanvas;
    public Canvas boardCanvas;
    public Canvas tokenCanvas;
    public TokenBoard tokenBoard;
    public HeroCanvas heroCanvas;
    public MonsterCanvas monsterCanvas;
    public UIScaler uiScaler;
    public MoraleDisplay moraleDisplay;
    public bool editMode = false;
    public QuestEditorData qed;
    public string[] ffgText = null;
    public GameType gameType;
    public CameraController cc;

    // This is used all over the place to find the game object.  Game then provides acces to common objects
    public static Game Get()
    {
        return FindObjectOfType<Game>();
    }

    // Unity fires off this function
    void Start()
    {

        // Find the common objects we use.  These are created by unity.
        cc = GameObject.FindObjectOfType<CameraController>();
        uICanvas = GameObject.Find("UICanvas").GetComponent<Canvas>();
        boardCanvas = GameObject.Find("BoardCanvas").GetComponent<Canvas>();
        tokenCanvas = GameObject.Find("TokenCanvas").GetComponent<Canvas>();
        tokenBoard = GameObject.FindObjectOfType<TokenBoard>();
        heroCanvas = GameObject.FindObjectOfType<HeroCanvas>();
        monsterCanvas = GameObject.FindObjectOfType<MonsterCanvas>();

        // Create some things
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
        cd = new ContentData(gameType.DataDirectory());
        // Check if we found anything
        if (cd.GetPacks().Count == 0)
        {
            Debug.Log("Error: Failed to find any content packs, please check that you have them present in: " + gameType.DataDirectory() + System.Environment.NewLine);
            Application.Quit();
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
        // Fetch all of the quest data and initialise the quest
        quest = new Quest(q);

        // Draw the hero icons, which are buttons for selection
        heroCanvas.SetupUI();

        // Add a finished button to start the quest
        TextButton endSelection = new TextButton(new Vector2(UIScaler.GetRight(-9), UIScaler.GetBottom(-3)), new Vector2(8, 2), "Finished", delegate { EndSelection(); }, Color.green);
        // Untag as dialog so this isn't cleared away during hero selection
        endSelection.ApplyTag("heroselect");

        // Add a title to the page
        DialogBox db = new DialogBox(new Vector2(8, 1), new Vector2(UIScaler.GetWidthUnits() - 16, 3), "Select Heroes");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
        db.ApplyTag("heroselect");

        TextButton cancelSelection = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Back", delegate { Destroyer.QuestSelect(); }, Color.red);
        // Untag as dialog so this isn't cleared away during hero selection
        cancelSelection.ApplyTag("heroselect");
    }
    
    // HeroCanvas validates selection and starts quest if everything is good
    public void EndSelection()
    {
        // Count up how many heros have been selected
        int count = 0;
        foreach (Quest.Hero h in Game.Get().quest.heroes)
        {
            if (h.heroData != null) count++;
        }
        // Starting morale is number of heros
        quest.morale = count;
        // This validates the selection then if OK starts first quest event
        heroCanvas.EndSection();
    }

    // On quitting
    void OnApplicationQuit ()
    {
        // This exists for the editor, because quitting doesn't actually work.
        Destroyer.Destroy();
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
}
