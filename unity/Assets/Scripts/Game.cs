using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI.Screens;
using Assets.Scripts.UI;
using ValkyrieTools;
using Ionic.Zip;

// General controller for the game
// There is one object of this class and it is used to find most game components
public class Game : MonoBehaviour {

    public static readonly string MONSTERS = "monsters";
    public static readonly string HEROSELECT = "heroselect";
    public static readonly string BOARD = "board";
    public static readonly string QUESTUI = "questui";
    public static readonly string EDITOR = "editor";
    public static readonly string UIPHASE = "uiphase";
    public static readonly string DIALOG = "dialog";
    public static readonly string ACTIVATION = "activation";
    public static readonly string SHOP = "shop";

    // This is populated at run time from the text asset
    public string version = "";

    // This is a reference to the Game object
    public static Game game;

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
    // Quest started as test from editor
    public bool testMode = false;

    // List of things that want to know if the mouse is clicked
    protected List<IUpdateListener> updateList;

    // Import thread
    public GameSelectionScreen gameSelect;

    // Current language
    public string currentLang;

    // Set when in quest editor
    public bool editMode = false;

    // This is used all over the place to find the game object.  Game then provides acces to common objects
    public static Game Get()
    {
        if (game == null)
        {
            game = FindObjectOfType<Game>();
        }
        return game;
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
        updateList = new List<IUpdateListener>();

        if (config.data.Get("UserConfig") == null)
        {
            // English is the default current language
            config.data.Add("UserConfig", "currentLang", "English");
            config.Save();
        }
        currentLang = config.data.Get("UserConfig", "currentLang");

        // On android extract streaming assets for use
        if (Application.platform == RuntimePlatform.Android)
        {
            System.IO.Directory.CreateDirectory(ContentData.ContentPath());
            using (ZipFile jar = ZipFile.Read(Application.dataPath))
            {
                foreach (ZipEntry e in jar)
                {
                    if (!e.FileName.StartsWith("assets")) continue;
                    if (e.FileName.StartsWith("assets/bin")) continue;

                    e.Extract(ContentData.ContentPath() + "../..", ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }

        DictionaryI18n valDict = new DictionaryI18n();
        foreach (string file in System.IO.Directory.GetFiles(ContentData.ContentPath() + "../text", "Localization*.txt"))
        {
            valDict.AddDataFromFile(file);
        }
        LocalizationRead.AddDictionary("val", valDict);

        roundControl = new RoundController();

        // Read the version and add it to the log
        TextAsset versionFile = Resources.Load("version") as TextAsset;
        version = versionFile.text.Trim();
        // The newline at the end stops the stack trace appearing in the log
        ValkyrieDebug.Log("Valkyrie Version: " + version + System.Environment.NewLine);

        // Bring up the Game selector
        gameSelect = new GameSelectionScreen();
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
        UIElement ui = new UIElement(Game.HEROSELECT);
        ui.SetLocation(UIScaler.GetRight(-8.5f), UIScaler.GetBottom(-2.5f), 8, 2);
        ui.SetText(CommonStringKeys.FINISHED, Color.green);
        ui.SetFont(gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(EndSelection);
        new UIElementBorder(ui, Color.green);

        // Add a title to the page
        ui = new UIElement(Game.HEROSELECT);
        ui.SetLocation(8, 1, UIScaler.GetWidthUnits() - 16, 3);
        ui.SetText(new StringKey("val","SELECT",gameType.HeroesName()));
        ui.SetFont(gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetLargeFont());

        heroCanvas.heroSelection = new HeroSelection();

        ui = new UIElement(Game.HEROSELECT);
        ui.SetLocation(0.5f, UIScaler.GetBottom(-2.5f), 8, 2);
        ui.SetText(CommonStringKeys.BACK, Color.red);
        ui.SetFont(gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Destroyer.QuestSelect);
        new UIElementBorder(ui, Color.red);
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
        quest.vars.SetValue("$%morale", count);
        // This validates the selection then if OK starts first quest event
        heroCanvas.EndSection();
    }

    public void QuestStartEvent()
    {
        // Start quest music
        List<string> music = new List<string>();
        foreach (AudioData ad in cd.audio.Values)
        {
            if (ad.ContainsTrait("quest")) music.Add(ad.file);
        }
        audioControl.Music(music);

        Destroyer.Dialog();
        // Create the menu button
        new MenuButton();
        new LogButton();
        new SkillButton();
        new InventoryButton();
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
        updateList.RemoveAll(delegate (IUpdateListener o) { return o == null; });
        for(int i = 0; i < updateList.Count; i++)
        {
            if (!updateList[i].Update())
            {
                updateList[i] = null;
            }
        }
        updateList.RemoveAll(delegate (IUpdateListener o) { return o == null; });

        if (Input.GetMouseButtonDown(0))
        {
            foreach(IUpdateListener iul in updateList)
            {
                iul.Click();
            }
        }
        // 0 is the left mouse button
        if (qed != null && Input.GetMouseButtonDown(0))
        {
            qed.MouseDown();
        }

        // 0 is the left mouse button
        if (qed != null && Input.GetMouseButtonDown(1))
        {
            qed.RightClick();
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

        if (gameSelect != null)
        {
            gameSelect.Update();
        }
    }

    public static string AppData()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            string appData = Android.GetStorage() + "/Valkyrie";
            if (appData != null)
            {
                ValkyrieDebug.Log("AppData: " + appData);
                return appData;
            }
        }
        return System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie";
    }

    public void AddUpdateListener(IUpdateListener obj)
    {
        updateList.Add(obj);
    }
}

public interface IUpdateListener
{
    /// <summary>
    /// This method is called on click
    /// </summary>
    void Click();

    /// <summary>
    /// This method is called on Unity Update.  Must return false to allow garbage collection.
    /// </summary>
    /// <returns>True to keep this in the update list, false to remove.</returns>
    bool Update();
}
