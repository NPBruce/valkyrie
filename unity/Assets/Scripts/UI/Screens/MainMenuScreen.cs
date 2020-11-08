using System.Collections.Generic;
using Assets.Scripts.Content;
using UnityEngine;
using UnityEngine.UI;
using ValkyrieTools;

namespace Assets.Scripts.UI.Screens
{

    // Class for creation and management of the main menu
    public class MainMenuScreen
    {
        private static readonly StringKey SELECT_CONTENT = new StringKey("val", "SELECT_CONTENT");
        private static readonly StringKey ABOUT = new StringKey("val", "ABOUT");
        private static readonly StringKey OPTIONS = new StringKey("val", "OPTIONS");
        private static readonly StringKey ABOUT_FFG = new StringKey("val", "ABOUT_FFG");
        private static readonly StringKey ABOUT_LIBS = new StringKey("val", "ABOUT_LIBS");
        private static readonly StringKey START_QUEST = new StringKey("val", "START_QUEST");
        private static readonly StringKey LOAD_QUEST = new StringKey("val", "LOAD_QUEST");

        private float ButtonWidth = 14;

        // Create a menu which will take up the whole screen and have options.  All items are dialog for destruction.
        public MainMenuScreen()
        {
            // This will destroy all, because we shouldn't have anything left at the main menu
            Destroyer.Destroy();
            Game game = Game.Get();

            List<string> music = new List<string>();
            foreach (AudioData ad in game.cd.audio.Values)
            {
                if (ad.ContainsTrait("menu")) music.Add(ad.file);
            }
            game.audioControl.PlayDefaultQuestMusic(music);

            // Name.  Should this be the banner, or better to print Valkyrie with the game font?
            UIElement ui = new UIElement();
            ui.SetLocation(2, 1, UIScaler.GetWidthUnits() - 4, 3);
            ui.SetText("Valkyrie");
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetLargeFont());
			
           // Version type : alpha / beta should be displayed
            if( Game.Get().version.EndsWith("a") )
            {
                ui = new UIElement();
                ui.SetLocation(UIScaler.GetRight(-6), 1, 6, 3);
                ui.SetText("alpha version");
                ui.SetTextAlignment(TextAnchor.MiddleLeft);
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetButton(delegate { TestCrash(); });
            }
            if (Game.Get().version.EndsWith("b"))
            {
                ui = new UIElement();
                ui.SetLocation(UIScaler.GetRight(-6), 1, 6, 3);
                ui.SetText("beta version");
                ui.SetTextAlignment(TextAnchor.MiddleLeft);
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetButton(delegate { TestCrash(); });
            }

            // Button for start quest/scenario
            ui = new UIElement();
            ui.SetLocation((UIScaler.GetWidthUnits() - ButtonWidth) / 2, 5, ButtonWidth, 2);
            ui.SetText(START_QUEST);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(Start);
            ui.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(ui);

            ui = new UIElement();
            ui.SetLocation((UIScaler.GetWidthUnits() - ButtonWidth) / 2, 8, ButtonWidth, 2);
            if (SaveManager.SaveExists())
            {
                ui.SetText(LOAD_QUEST);
                ui.SetButton(delegate { new SaveSelectScreen(); });
                ui.SetBGColor(new Color(0, 0.03f, 0f));
                new UIElementBorder(ui);
            }
            else
            {
                ui.SetText(LOAD_QUEST, Color.grey);
                new UIElementBorder(ui, Color.grey);
            }
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());

            // Content selection page
            ui = new UIElement();
            ui.SetLocation((UIScaler.GetWidthUnits() - ButtonWidth) / 2, 11, ButtonWidth, 2);
            ui.SetText(SELECT_CONTENT);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(Content);
            ui.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(ui);

            // Quest/Scenario editor
            ui = new UIElement();
            ui.SetLocation((UIScaler.GetWidthUnits() - ButtonWidth) / 2, 14, ButtonWidth, 2);
            ui.SetText(new StringKey("val","QUEST_NAME_EDITOR",game.gameType.QuestName()));
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(Editor);
            ui.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(ui);

            // About page (managed in this class)
            ui = new UIElement();
            ui.SetLocation((UIScaler.GetWidthUnits() - ButtonWidth) / 2, 17, ButtonWidth, 2);
            ui.SetText(ABOUT);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(About);
            ui.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(ui);
            
            // Configuration menu
            ui = new UIElement();
            ui.SetLocation((UIScaler.GetWidthUnits() - ButtonWidth) / 2, 20, ButtonWidth, 2);
            ui.SetText(OPTIONS);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(Config);
            ui.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(ui);

            // Exit Valkyrie
            ui = new UIElement();
            ui.SetLocation((UIScaler.GetWidthUnits() - ButtonWidth) / 2, 23, ButtonWidth, 2);
            ui.SetText(CommonStringKeys.EXIT);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(Exit);
            ui.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(ui);
        }

        // Start quest
        internal void Start()
        {
            ValkyrieDebug.Log("INFO: Accessing quests");

            Game game = Game.Get();

            // Remove the main menu
            Destroyer.Dialog();

            game.SelectQuest();
        }

        public void Content()
        {
            ValkyrieDebug.Log("INFO: Accessing content selection screen");

            new ContentSelectScreen();
        }

        public void Editor()
        {
            ValkyrieDebug.Log("INFO: Accessing editor selection screen");

            Game game = Game.Get();
            game.SelectEditQuest();
        }

        private void Config()
        {
            ValkyrieDebug.Log("INFO: Accessing config");
            new OptionsScreen();
        }

        static int click_counter = 0;
        static public void TestCrash()
        {
            click_counter++;

            if(click_counter >= 5)
            {
                DebugManager.Crash();
            }
        }

        // Create the about dialog
        public void About()
        {
            ValkyrieDebug.Log("INFO: Accessing about");

            // This will destroy all, because we shouldn't have anything left at the main menu
            Destroyer.Destroy();

            Sprite bannerSprite;
            Texture2D newTex = Resources.Load("sprites/banner") as Texture2D;

            GameObject banner = new GameObject("banner");
            banner.tag = Game.DIALOG;

            banner.transform.SetParent(Game.Get().uICanvas.transform);

            RectTransform trans = banner.AddComponent<RectTransform>();
            trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 1 * UIScaler.GetPixelsPerUnit(), 7f * UIScaler.GetPixelsPerUnit());
            trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (UIScaler.GetWidthUnits() - 18f) * UIScaler.GetPixelsPerUnit() / 2f, 18f * UIScaler.GetPixelsPerUnit());
            banner.AddComponent<CanvasRenderer>();


            Image image = banner.AddComponent<Image>();
            bannerSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            image.sprite = bannerSprite;
            image.rectTransform.sizeDelta = new Vector2(18f * UIScaler.GetPixelsPerUnit(), 7f * UIScaler.GetPixelsPerUnit());

            UIElement ui = new UIElement();
            ui.SetLocation((UIScaler.GetWidthUnits() - 30f) / 2, 10, 30, 6);
            ui.SetText(ABOUT_FFG);
            ui.SetFontSize(UIScaler.GetMediumFont());

            ui = new UIElement();
            ui.SetLocation((UIScaler.GetWidthUnits() - 30f) / 2, 18, 30, 5);
            ui.SetText(ABOUT_LIBS);
            ui.SetFontSize(UIScaler.GetMediumFont());

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetWidthUnits() - 5, UIScaler.GetBottom(-3), 5, 2);
            ui.SetText(Game.Get().version);
            ui.SetFontSize(UIScaler.GetMediumFont());

            ui = new UIElement();
            ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
            ui.SetText(CommonStringKeys.BACK);
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(GameStateManager.MainMenu);
            ui.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(ui);
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}