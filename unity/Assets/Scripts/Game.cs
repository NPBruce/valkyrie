using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
using Assets.Scripts.UI.Screens;
using Ionic.Zip;
using UnityEngine;
using ValkyrieTools;

// General controller for the game
// There is one object of this class and it is used to find most game components
public class Game : MonoBehaviour
{

    public static readonly string MONSTERS = "monsters";
    public static readonly string HEROSELECT = "heroselect";
    public static readonly string BOARD = "board";
    public static readonly string QUESTUI = "questui";
    public static readonly string QUESTLIST = "questlist";
    public static readonly string EDITOR = "editor";
    public static readonly string UIPHASE = "uiphase";
    public static readonly string TRANSITION = "transition";
    public static readonly string DIALOG = "dialog";
    public static readonly string SETWINDOW = "setwindow";
    public static readonly string ACTIVATION = "activation";
    public static readonly string SHOP = "shop";
    public static readonly string ENDGAME = "endgame";
    public static readonly string BG_TASKS = "bg_tasks";
    public static readonly string LOGS = "logs";

    // This is populated at run time from the text asset
    public string version = "";

    // This is a reference to the Game object
    public static Game game;

    // These components are referenced here for easy of use
    // Data included in content packs
    public ContentData cd
    {
        get => _cd;
        internal set { _cd = value; _contentLoader = new ContentLoader(value);}
    }

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
    // Transparecny value for non selected component in the editor
    public float editorTransparency;
    // Quest started as test from editor
    public bool testMode = false;
    // Stats manager for quest rating
    public StatsManager stats;
    // Quests manager
    public QuestsManager questsList;

    public bool googleTtsEnabled = false;
    public string googleTtsApiKey = "";
    public string googleTtsVoice = "en-US-Wavenet-J";

    // List of things that want to know if the mouse is clicked
    protected List<IUpdateListener> updateList;

    // Import thread
    public GameSelectionScreen gameSelect;

    // List of quests window
    public GameObject go_questSelectionScreen = null;
    public QuestSelectionScreen questSelectionScreen = null;

    private ContentLoader _contentLoader;
    public ContentLoader ContentLoader => _contentLoader;

    // Current language
    public string currentLang;

    // Set when in quest editor
    public bool editMode = false;

    // Debug option
    public bool debugTests = false;

    // main thread Id
    public Thread mainThread = null;
    private ContentData _cd;


    // This is used all over the place to find the game object.  Game then provides acces to common objects
    public static Game Get()
    {
        if (game == null)
        {
            game = FindObjectOfType<Game>();
        }
        
        return game;
    }

    internal string CurrentQuestPath()
    {
        return quest?.originalPath;
    }

    // Unity fires off this function
    void Awake()
    {
        // save main thread Id
        mainThread = Thread.CurrentThread;

        // used for float.TryParse
        mainThread.CurrentCulture = CultureInfo.InvariantCulture;
        mainThread.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        // Set specific configuration for Android 
        if (Application.platform == RuntimePlatform.Android)
        {
            // activate crashlytics
            DebugManager.Enable();

            // deactivate screen timeount while in Valkyrie
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        // Find the common objects we use.  These are created by unity.
        cc = FindObjectOfType<CameraController>();
        uICanvas = GameObject.Find("UICanvas").GetComponent<Canvas>();
        boardCanvas = GameObject.Find("BoardCanvas").GetComponent<Canvas>();
        tokenCanvas = GameObject.Find("TokenCanvas").GetComponent<Canvas>();
        tokenBoard = FindObjectOfType<TokenBoard>();
        heroCanvas = FindObjectOfType<HeroCanvas>();
        monsterCanvas = FindObjectOfType<MonsterCanvas>();

        // Create some things
        uiScaler = new UIScaler(uICanvas);
        config = new ConfigFile();
        GameObject go = new GameObject("audio");
        audioControl = go.AddComponent<Audio>();
        updateList = new List<IUpdateListener>();
        stats = new StatsManager();
        stats.DownloadStats();

        if (config.data.Get("UserConfig") == null)
        {
            // English is the default current language
            config.data.Add("UserConfig", "currentLang", "English");
            config.Save();
        }
        currentLang = config.data.Get("UserConfig", "currentLang");

        string vSet = config.data.Get("UserConfig", "editorTransparency");
        if (vSet == "")
            editorTransparency = 0.3f;
        else
            float.TryParse(vSet, out editorTransparency);
        
        string googleTtsEnabledFromConfig = config.data.Get("UserConfig", "googleTtsEnabled");
        if (googleTtsEnabledFromConfig == "")
            googleTtsEnabled = false;
        else
            bool.TryParse(googleTtsEnabledFromConfig, out googleTtsEnabled);
        
        googleTtsApiKey = config.data.Get("UserConfig", "googleTtsApiKey");
        
        string googleTtsVoiceFromConfig = config.data.Get("UserConfig", "googleTtsVoice");
        if (googleTtsVoiceFromConfig != "")
            googleTtsVoice = googleTtsVoiceFromConfig;

        string s_debug_tests = config.data.Get("Debug", "tests");
        if (s_debug_tests != "")
        {
            s_debug_tests = s_debug_tests.ToLower();
            if (s_debug_tests == "true" || s_debug_tests == "1")
                debugTests = true;
        }

        // On android extract streaming assets for use
        if (Application.platform == RuntimePlatform.Android)
        {
            Directory.CreateDirectory(ContentData.ContentPath());
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
        var localizationFiles = Directory.GetFiles(ContentData.ContentPath() + "../text", "Localization*.txt");
        foreach (string file in localizationFiles)
        {
            valDict.AddDataFromFile(file);
        }
        LocalizationRead.AddDictionary("val", valDict);

        roundControl = new RoundController();

        // Read the version and add it to the log
        TextAsset versionFile = Resources.Load("version") as TextAsset;
        version = versionFile.text.Trim();
        // The newline at the end stops the stack trace appearing in the log
        ValkyrieDebug.Log("Valkyrie Version: " + version + Environment.NewLine);

#if UNITY_STANDALONE_WIN
        SetScreenOrientationToLandscape();
#endif

        // Bring up the Game selector
        gameSelect = new GameSelectionScreen();
    }

    // This is called by 'start quest' on the main menu
    public void SelectQuest()
    {
        Dictionary<string, string> packs = config.data.Get(gameType.TypeName() + "Packs");
        if (packs != null)
        {
            foreach (KeyValuePair<string, string> kv in packs)
            {
                _contentLoader.LoadContentID(kv.Key);
            }
        }
            
        // Pull up the quest selection page
        if (questSelectionScreen == null)
        {
            go_questSelectionScreen = new GameObject("QuestSelectionScreen");
            questSelectionScreen = go_questSelectionScreen.AddComponent<QuestSelectionScreen>();
        }
        else
        {
            questSelectionScreen.Show();
        }
    }

    // This is called by editor on the main menu
    public void SelectEditQuest()
    {
        // Pull up the quest selection page
        new QuestEditSelection();
    }

    // This is called when a quest is selected
    internal void StartQuest(QuestData.Quest q)
    {
        if (Path.GetExtension(Path.GetFileName(q.path)) == ".valkyrie")
        {
            // extract the full package
            QuestLoader.ExtractSinglePackageFull(ContentData.DownloadPath() + Path.DirectorySeparatorChar + Path.GetFileName(q.path));
        }

        // Fetch all of the quest data and initialise the quest
        quest = new Quest(q);

        // Draw the hero icons, which are buttons for selection
        heroCanvas.SetupUI();

        // Add a finished button to start the quest
        UIElement ui = new UIElement(HEROSELECT);
        ui.SetLocation(UIScaler.GetRight(-8.5f), UIScaler.GetBottom(-2.5f), 8, 2);
        ui.SetText(CommonStringKeys.FINISHED, Color.green);
        ui.SetFont(gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(EndSelection);
        new UIElementBorder(ui, Color.green);

        // Add a title to the page
        ui = new UIElement(HEROSELECT);
        ui.SetLocation(8, 1, UIScaler.GetWidthUnits() - 16, 3);
        ui.SetText(new StringKey("val", "SELECT", gameType.HeroesName()));
        ui.SetFont(gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetLargeFont());

        heroCanvas.heroSelection = new HeroSelection();

        ui = new UIElement(HEROSELECT);
        ui.SetLocation(0.5f, UIScaler.GetBottom(-2.5f), 8, 2);
        ui.SetText(CommonStringKeys.BACK, Color.red);
        ui.SetFont(gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(GameStateManager.Quest.Restart);
        new UIElementBorder(ui, Color.red);
    }

    // HeroCanvas validates selection and starts quest if everything is good
    public void EndSelection()
    {
        // Count up how many heros have been selected
        int count = 0;
        foreach (Quest.Hero h in Get().quest.heroes)
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
        PlayDefaultQuestMusic();

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

    public void PlayDefaultQuestMusic()
    {
        List<string> music = GetDefaultQuestMusic();
        audioControl.PlayDefaultQuestMusic(music);
    }

    private List<string> GetDefaultQuestMusic()
    {
        List<string> music = new List<string>();
        //If default quest music  has been turned off do not add any audio files to the list
        if (quest.defaultMusicOn)
        {
            // Start quest music
            foreach (AudioData ad in cd.Values<AudioData>())
            {
                if (ad.ContainsTrait("quest")) music.Add(ad.file);
            }
        }

        return music;
    }

    // On quitting
    void OnApplicationQuit()
    {
        // This exists for the editor, because quitting doesn't actually work.
        Destroyer.Destroy();
        // Clean up temporary files
        QuestLoader.CleanTemp();
    }

    //  This is here because the editor doesn't get an update, so we are passing through mouse clicks to the editor
    void Update()
    {
        if (updateList == null)
        {
            return;
        }
        updateList.RemoveAll(delegate (IUpdateListener o) { return o == null; });
        for (int i = 0; i < updateList.Count; i++)
        {
            if (!updateList[i].Update())
            {
                updateList[i] = null;
            }
        }
        updateList.RemoveAll(delegate (IUpdateListener o) { return o == null; });

        if (Input.GetMouseButtonDown(0))
        {
            foreach (IUpdateListener iul in updateList)
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
            string appData = Path.Combine(Android.GetStorage(), "Valkyrie");
            if (appData != null)
            {
                return appData;
            }
        }
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Valkyrie");
    }

    public void AddUpdateListener(IUpdateListener obj)
    {
        updateList.Add(obj);
    }

#if UNITY_STANDALONE_WIN
    [DllImport("User32.dll")]
    private static extern bool SetDisplayAutoRotationPreferences(int value);

    private static void SetScreenOrientationToLandscape()
    {
        try
        {
        SetDisplayAutoRotationPreferences((int)ORIENTATION_PREFERENCE.ORIENTATION_PREFERENCE_LANDSCAPE |
            (int)ORIENTATION_PREFERENCE.ORIENTATION_PREFERENCE_LANDSCAPE_FLIPPED);
        }

        catch (EntryPointNotFoundException e)
        {
            Debug.Log("Exception triggered and caught :" + e.GetType().Name);
            Debug.Log("message :" + e.Message);
        }
    }

    private enum ORIENTATION_PREFERENCE
    {
        ORIENTATION_PREFERENCE_NONE = 0x0,
        ORIENTATION_PREFERENCE_LANDSCAPE = 0x1,
        ORIENTATION_PREFERENCE_PORTRAIT = 0x2,
        ORIENTATION_PREFERENCE_LANDSCAPE_FLIPPED = 0x4,
        ORIENTATION_PREFERENCE_PORTRAIT_FLIPPED = 0x8
    }
#endif

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
