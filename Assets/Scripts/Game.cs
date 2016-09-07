using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
Dump list of things to do:

Done:
    Better panning
    end round event in editor
    about page
    #monsters flag
    prevent events failing on incorrect selection
    no confirm on door/token (if no available post events)
    custom pass/fail
    build now 32bit
    camera jump smoothing

Now:
    Threat/peril
    {0}/{1} in activations

Later:
    catch all parse exceptions
    text editing sucks (text wrap?)
    frame color for portraits Frame_Monster_1x1.dds (color rotation?)
    random tags
    tags for hero count
    tags for round number
    Expansion name on elements in editor, tags
    ffg app version unity
    quest set
    lt.
    Add expansions, conent selection
    Stacked tileselection? (tags?)
    save games
    undo
*/

// General controller for the game
public class Game : MonoBehaviour {

    public string version = "";
    public ContentData cd;
    public QuestData qd;
    public Round round;
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

        round = new Round();

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
        foreach (Round.Hero h in round.heroes)
        {
            if (h.heroData != null) count++;
        }
        // Starting morale is number of heros
        round.morale = count;
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
}

