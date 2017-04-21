using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI.Screens;
using ValkyrieTools;

// General controller for the game
// There is one object of this class and it is used to find most game components
public class Game : MonoBehaviour {

    // This is populated at run time from the text asset
    public string version = "";

    // These components are referenced here for easy of use
    // Data included in content packs
    public ContentData cd;
    // Data for the current quest
    public Quest quest;
    // Canvas for UI components (fixed on screen)
    public Canvas uICanvas;
    // Canvas for board tiles (tilted, in game space)
    public Canvas boardCanvas;
    // Canvas for board tokens (just above board tiles)
    public Canvas tokenCanvas;
    // Class for management of tokens on the board
    public TokenBoard tokenBoard;
    // Class for management of hero selection panel
    public HeroCanvas heroCanvas;
    // Class for management of monster selection panel
    public MonsterCanvas monsterCanvas;
    // Utility Class for UI scale and position
    public UIScaler uiScaler;
    // Class for Morale counter
    public MoraleDisplay moraleDisplay;
    // Class for quest editor management
    public QuestEditorData qed;
    // Class for gameType information (controls specific to a game type)
    public GameType gameType;
    // Class for camera management (zoom, scroll)
    public CameraController cc;
    // Class for managing user configuration
    public ConfigFile config;
    // Class for progress of activations, rounds
    public RoundController roundControl;
    // Class for stage control UI
    public NextStageButton stageUI;
    // Class log window
    public LogWindow logWindow;
    // Class for stage control UI

    public Audio audioControl;

    // Current language
    public string currentLang;

    // Set when in quest editor
    public bool editMode = false;

    // This is used all over the place to find the game object.  Game then provides acces to common objects
    // Note that this is not fast, so shouldn't be used in frame
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
        config = new ConfigFile();
        GameObject go = new GameObject("audio");
        audioControl = go.AddComponent<Audio>();

        if (config.data.Get("UserConfig") == null)
        {
            // English is the default current language
            config.data.Add("UserConfig", "currentLang", DictionaryI18n.DEFAULT_LANG);
            config.Save();
        }
        currentLang = config.data.Get("UserConfig", "currentLang");

        try
        {
            TextAsset localizationFile = Resources.Load("Text/Localization") as TextAsset;
            LocalizationRead.valkyrieDict = LocalizationRead.ReadFromTextAsset(localizationFile, currentLang);
            LocalizationRead.changeCurrentLangTo(currentLang);
        }
        catch (System.Exception e)
        {
            ValkyrieDebug.Log("Error loading valkyrie localization file:" + e.Message);
        }

        roundControl = new RoundController();

        // Read the version and add it to the log
        TextAsset versionFile = Resources.Load("version") as TextAsset;
        version = versionFile.text.Trim();
        // The newline at the end stops the stack trace appearing in the log
        ValkyrieDebug.Log("Valkyrie Version: " + version + System.Environment.NewLine);

        // Bring up the Game selector
        new GameSelectionScreen();
    }

    // This is called by 'start quest' on the main menu
    public void SelectQuest()
    {
        // Find any content packs at the location
        cd = new ContentData(gameType.DataDirectory());
        // Check if we found anything
        if (cd.GetPacks().Count == 0)
        {
            ValkyrieDebug.Log("Error: Failed to find any content packs, please check that you have them present in: " + gameType.DataDirectory() + System.Environment.NewLine);
            Application.Quit();
        }

        // Load configured packs
        cd.LoadContentID("");
        Dictionary<string, string> packs = config.data.Get(gameType.TypeName() + "Packs");
        if (packs != null)
        {
            foreach (KeyValuePair<string, string> kv in packs)
            {
                cd.LoadContentID(kv.Key);
            }
        }

        // Get a list of available quests
        Dictionary<string, QuestData.Quest> ql = QuestLoader.GetQuests();

        // Pull up the quest selection page
        new QuestSelectionScreen(ql);
    }

    // This is called by editor on the main menu
    public void SelectEditQuest()
    {
        // Find any content packs at the location
        cd = new ContentData(gameType.DataDirectory());
        // Check if we found anything
        if (cd.GetPacks().Count == 0)
        {
            ValkyrieDebug.Log("Error: Failed to find any content packs, please check that you have them present in: " + gameType.DataDirectory() + System.Environment.NewLine);
            Application.Quit();
        }

        // We load all packs for the editor, not just those selected
        foreach (string pack in cd.GetPacks())
        {
            cd.LoadContent(pack);
        }

        // Pull up the quest selection page
        new QuestEditSelection();
    }

    // This is called when a quest is selected
    public void StartQuest(QuestData.Quest q)
    {
        // Fetch all of the quest data and initialise the quest
        quest = new Quest(q);

        // Draw the hero icons, which are buttons for selection
        heroCanvas.SetupUI();

        // Add a finished button to start the quest
        TextButton endSelection = new TextButton(
            new Vector2(UIScaler.GetRight(-9), 
            UIScaler.GetBottom(-3)), 
            new Vector2(8, 2), 
            CommonStringKeys.FINISHED, 
            delegate { EndSelection(); }, 
            Color.green);

        endSelection.SetFont(gameType.GetHeaderFont());
        // Untag as dialog so this isn't cleared away during hero selection
        endSelection.ApplyTag("heroselect");

        // Add a title to the page
        DialogBox db = new DialogBox(
            new Vector2(8, 1), 
            new Vector2(UIScaler.GetWidthUnits() - 16, 3), 
            new StringKey("val","SELECT",gameType.HeroesName())
            );
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
        db.SetFont(gameType.GetHeaderFont());
        db.ApplyTag("heroselect");

        heroCanvas.heroSelection = new HeroSelection();

        TextButton cancelSelection = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), CommonStringKeys.BACK, delegate { Destroyer.QuestSelect(); }, Color.red);
        cancelSelection.SetFont(gameType.GetHeaderFont());
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
        quest.vars.SetValue("$morale", count);
        // This validates the selection then if OK starts first quest event
        heroCanvas.EndSection();
    }

    public void QuestStartEvent()
    {
        Destroyer.Dialog();
        // Create the menu button
        new MenuButton();
        new LogButton();
        // Draw next stage button if required
        stageUI = new NextStageButton();

        // Start round events
        quest.eManager.EventTriggerType("StartRound", false);
        // Start the quest (top of stack)
        quest.eManager.EventTriggerType("EventStart", false);
        quest.eManager.TriggerEvent();
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

        if (quest != null)
        {
            quest.Update();
        }

        if (Input.GetKey("right alt") || Input.GetKey("left alt"))
        {
            if (Input.GetKeyDown("d") && logWindow != null)
            {
                logWindow.Update(true);
            }
        }
    }

    // This is here to call a function after the frame has been rendered
    // We use this on import because the import function blocks rendering
    // and we want to update the display before it starts
    public void CallAfterFrame(UnityEngine.Events.UnityAction call)
    {
        StartCoroutine(CallAfterFrameDelay(call));
    }

    private IEnumerator CallAfterFrameDelay(UnityEngine.Events.UnityAction call)
    {
        yield return new WaitForEndOfFrame();
        // Fixme this is hacky, the single frame solution doesn't work, we add 1 second
        yield return new WaitForSeconds(1);
        call();
    }
}
