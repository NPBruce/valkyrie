using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ValkyrieTools;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

namespace Assets.Scripts.UI.Screens
{
    // Class for options menu
    public class OptionsScreen
    {
        public static readonly HashSet<string> ENABLED_LANGS = new HashSet<string>("English,Spanish,French,Italian,German,Portuguese,Polish,Russian,Chinese,Korean,Czech,Japanese,Ukrainian".Split(','));


        private readonly StringKey OPTIONS = new StringKey("val", "OPTIONS");
        private readonly StringKey LANGUAGE = new StringKey("val", "LANGUAGE");
        private readonly StringKey CHOOSE_LANG = new StringKey("val", "CHOOSE_LANG");
        private readonly StringKey FALLBACK_LANG = new StringKey("val", "FALLBACK_LANG");
        private readonly StringKey NONE = new StringKey("val", "NONE");
        private readonly StringKey EFFECTS = new StringKey("val", "EFFECTS");
        private readonly StringKey MUSIC = new StringKey("val", "MUSIC");
        private readonly StringKey RESTART_TO_APPLY = new StringKey("val", "RESTART_TO_APPLY");
        private readonly StringKey RESOLUTION = new StringKey("val", "RESOLUTION");
        private readonly StringKey FULLSCREEN = new StringKey("val", "FULLSCREEN");
        private readonly StringKey OptionON = new StringKey("val", "ON");
        private readonly StringKey OptionOff = new StringKey("val", "OFF");
        private readonly StringKey ADVANCED_OPTIONS = new StringKey("val", "ADVANCED_OPTIONS");

        // Grid constants
        private const float LEFT_X = 2f;
        private const float LEFT_W = 14f;
        private const float RIGHT_X_OFFSET = 2f; // added to UIScaler.GetHCenter()
        private const float RIGHT_W = 14f;
        private const float ROW_LABEL_H = 2f;
        private const float ROW_BTN_H = 2f;
        private const float ROW_GAP = 1.5f;
        private const float ROW_STRIDE = ROW_LABEL_H + ROW_BTN_H + ROW_GAP;  // 5.5f per row
        private const float ROW0_Y = 2f;  // Language
        private const float ROW1_Y = ROW0_Y + ROW_STRIDE;  // Fallback = 7.5f
        private const float ROW2_Y = ROW1_Y + ROW_STRIDE;  // Resolution = 13f
        private const float ROW3_Y = ROW2_Y + ROW_STRIDE;  // Fullscreen = 18.5f

        Game game = Game.Get();

        public Slider musicSlide;
        public Slider musicSlideRev;
        public Slider effectSlide;
        public Slider effectSlideRev;

        // Create a menu which will take up the whole screen and have options.  All items are dialog for destruction.
        public OptionsScreen()
        {
            // This will destroy all, because we shouldn't have anything left at the main menu
            Destroyer.Destroy();

            game = Game.Get();

            // Create elements for the screen
            CreateElements();
        }

        /// <summary>
        /// Method to create UI elements in the screen
        /// </summary>
        /// <param name="game">current game</param>
        private void CreateElements()
        {

            CreateLanguageElements();

            CreateFallbackLanguageElements();

            CreateAudioElements();

            CreateResolutionAndFullScreenOptions();

            CreateAdvancedOptionsElements();

            // Button for back to main menu
            UIElement ui = new UIElement();
            ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
            ui.SetText(CommonStringKeys.BACK, Color.red);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(GameStateManager.MainMenu);
            new UIElementBorder(ui, Color.red);
        }

        private void CreateResolutionAndFullScreenOptions()
        {
            // Only render on Windows, Mac or Linux (player or editor)
            var p = Application.platform;
            bool isSupportedPlatform =
                p == RuntimePlatform.WindowsPlayer || p == RuntimePlatform.OSXPlayer || p == RuntimePlatform.LinuxPlayer
                || p == RuntimePlatform.WindowsEditor || p == RuntimePlatform.OSXEditor || p == RuntimePlatform.LinuxEditor;

            if (!isSupportedPlatform)
                return;

            Game game = Game.Get();

            // === Resolution row (ROW2) ===
            UIElement ui = new UIElement();
            ui.SetLocation(LEFT_X, ROW2_Y, LEFT_W, ROW_LABEL_H);
            ui.SetText(RESOLUTION);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());

            var resolutions = ResolutionManager.GetAvailableResolutions();
            int currentIndex = -1;

            string configRes = game.config.data.Get("UserConfig", "resolution");
            if (!string.IsNullOrEmpty(configRes))
            {
                string[] parts = configRes.Split('x');
                if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
                    currentIndex = resolutions.FindIndex(r => r.width == w && r.height == h);
            }
            if (currentIndex < 0)
                currentIndex = resolutions.FindIndex(r => r.width == Screen.width && r.height == Screen.height);
            if (currentIndex < 0) currentIndex = 0;

            // Current resolution button — click to open selection modal
            var cur = resolutions[currentIndex];
            ui = new UIElement();
            ui.SetLocation(LEFT_X, ROW2_Y + ROW_LABEL_H, LEFT_W, ROW_BTN_H);
            ui.SetText($"{cur.width} x {cur.height}");
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(delegate { OpenResolutionSelection(game, resolutions, currentIndex); });
            new UIElementBorder(ui);

            // Restart warning
            ui = new UIElement();
            ui.SetLocation(LEFT_X, ROW2_Y + ROW_LABEL_H + ROW_BTN_H, LEFT_W, 1.2f);
            ui.SetText(RESTART_TO_APPLY, Color.red);
            ui.SetFont(game.gameType.GetFont());
            ui.SetFontSize(UIScaler.GetSmallFont());

            // === Fullscreen row (ROW3) ===
            ui = new UIElement();
            ui.SetLocation(LEFT_X, ROW3_Y, LEFT_W, ROW_LABEL_H);
            ui.SetText(FULLSCREEN);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());

            string configFs = game.config.data.Get("UserConfig", "fullscreen");
            bool isFs = !string.IsNullOrEmpty(configFs) ? configFs == "1" : ResolutionManager.IsFullscreen();

            ui = new UIElement();
            ui.SetLocation(LEFT_X, ROW3_Y + ROW_LABEL_H, LEFT_W, ROW_BTN_H);
            ui.SetText(isFs ? OptionON : OptionOff);
            ui.SetButton(delegate
            {
                bool newFs = !isFs;
                ResolutionManager.SetFullscreen(newFs);
                game.config.data.Add("UserConfig", "fullscreen", newFs ? "1" : "0");
                game.config.Save();
                new OptionsScreen();
            });
            new UIElementBorder(ui, isFs ? Color.white : Color.grey);
        }

        private static void OpenResolutionSelection(Game game, List<Resolution> resolutions, int currentIndex)
        {
            UIWindowSelectionList selection = new UIWindowSelectionList(
                delegate(string result)
                {
                    if (result != null)
                    {
                        game.config.data.Add("UserConfig", "resolution", result);
                        game.config.Save();
                        Game.Get().StartCoroutine(ReloadOptionsScreenNextFrame());
                    }
                    else
                    {
                        new OptionsScreen();
                    }
                },
                "",
                callAfterCancel: true,
                showSortButtons: false
            );

            string configRes = game.config.data.Get("UserConfig", "resolution");
            foreach (var r in resolutions)
            {
                string key = $"{r.width}x{r.height}";
                string display = $"{r.width} x {r.height}";
                bool isSelected = key == configRes
                    || (string.IsNullOrEmpty(configRes) && r.width == Screen.width && r.height == Screen.height);
                selection.AddItem(display, key, isSelected ? Color.green : Color.white);
            }

            selection.Draw();
        }


        private void CreateAdvancedOptionsElements()
        {
            Game game = Game.Get();

            UIElement ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter() - 5, UIScaler.GetBottom(-3), 18, 2);
            ui.SetText(ADVANCED_OPTIONS);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(delegate { new AdvancedOptionsScreen(); });
            new UIElementBorder(ui);
        }

        private void CreateAudioElements()
        {
            float rightX = UIScaler.GetHCenter() + RIGHT_X_OFFSET;

            // === Music row (ROW0 on right) ===
            UIElement ui = new UIElement();
            ui.SetLocation(rightX, ROW0_Y, RIGHT_W, ROW_LABEL_H);
            ui.SetText(MUSIC);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());

            float mVolume;
            string vSet = game.config.data.Get("UserConfig", "music");
            float.TryParse(vSet, out mVolume);
            if (vSet.Length == 0) mVolume = 1;

            float musicBtnY = ROW0_Y + ROW_LABEL_H;
            ui = new UIElement();
            ui.SetLocation(rightX, musicBtnY, RIGHT_W, ROW_BTN_H);
            ui.SetBGColor(Color.clear);
            new UIElementBorder(ui);

            GameObject musicSlideObj = new GameObject("musicSlide");
            musicSlideObj.tag = Game.DIALOG;
            musicSlideObj.transform.SetParent(game.uICanvas.transform);
            musicSlide = musicSlideObj.AddComponent<Slider>();
            RectTransform musicSlideRect = musicSlideObj.GetComponent<RectTransform>();
            musicSlideRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, musicBtnY * UIScaler.GetPixelsPerUnit(), ROW_BTN_H * UIScaler.GetPixelsPerUnit());
            musicSlideRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, rightX * UIScaler.GetPixelsPerUnit(), RIGHT_W * UIScaler.GetPixelsPerUnit());
            musicSlide.onValueChanged.AddListener(delegate { UpdateMusic(); });

            GameObject musicFill = new GameObject("musicfill");
            musicFill.tag = Game.DIALOG;
            musicFill.transform.SetParent(musicSlideObj.transform);
            musicFill.AddComponent<Image>();
            musicFill.GetComponent<Image>().color = Color.white;
            musicSlide.fillRect = musicFill.GetComponent<RectTransform>();
            musicSlide.fillRect.offsetMin = Vector2.zero;
            musicSlide.fillRect.offsetMax = Vector2.zero;

            GameObject musicSlideObjRev = new GameObject("musicSlideRev");
            musicSlideObjRev.tag = Game.DIALOG;
            musicSlideObjRev.transform.SetParent(game.uICanvas.transform);
            musicSlideRev = musicSlideObjRev.AddComponent<Slider>();
            RectTransform musicSlideRectRev = musicSlideObjRev.GetComponent<RectTransform>();
            musicSlideRectRev.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, musicBtnY * UIScaler.GetPixelsPerUnit(), ROW_BTN_H * UIScaler.GetPixelsPerUnit());
            musicSlideRectRev.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, rightX * UIScaler.GetPixelsPerUnit(), RIGHT_W * UIScaler.GetPixelsPerUnit());
            musicSlideRev.onValueChanged.AddListener(delegate { UpdateMusicRev(); });
            musicSlideRev.direction = Slider.Direction.RightToLeft;

            GameObject musicFillRev = new GameObject("musicfillrev");
            musicFillRev.tag = Game.DIALOG;
            musicFillRev.transform.SetParent(musicSlideObjRev.transform);
            musicFillRev.AddComponent<Image>();
            musicFillRev.GetComponent<Image>().color = Color.clear;
            musicSlideRev.fillRect = musicFillRev.GetComponent<RectTransform>();
            musicSlideRev.fillRect.offsetMin = Vector2.zero;
            musicSlideRev.fillRect.offsetMax = Vector2.zero;

            musicSlide.value = mVolume;
            musicSlideRev.value = 1 - mVolume;

            // === Effects row (ROW1 on right) ===
            ui = new UIElement();
            ui.SetLocation(rightX, ROW1_Y, RIGHT_W, ROW_LABEL_H);
            ui.SetText(EFFECTS);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());

            float eVolume;
            vSet = game.config.data.Get("UserConfig", "effects");
            float.TryParse(vSet, out eVolume);
            if (vSet.Length == 0) eVolume = 1;

            float effectBtnY = ROW1_Y + ROW_LABEL_H;
            ui = new UIElement();
            ui.SetLocation(rightX, effectBtnY, RIGHT_W, ROW_BTN_H);
            ui.SetBGColor(Color.clear);
            new UIElementBorder(ui);

            GameObject effectSlideObj = new GameObject("effectSlide");
            effectSlideObj.tag = Game.DIALOG;
            effectSlideObj.transform.SetParent(game.uICanvas.transform);
            effectSlide = effectSlideObj.AddComponent<Slider>();
            RectTransform effectSlideRect = effectSlideObj.GetComponent<RectTransform>();
            effectSlideRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, effectBtnY * UIScaler.GetPixelsPerUnit(), ROW_BTN_H * UIScaler.GetPixelsPerUnit());
            effectSlideRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, rightX * UIScaler.GetPixelsPerUnit(), RIGHT_W * UIScaler.GetPixelsPerUnit());
            effectSlide.onValueChanged.AddListener(delegate { UpdateEffects(); });
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerUp;
            entry.callback.AddListener(delegate { PlayTestSound(); });
            effectSlideObj.AddComponent<EventTrigger>().triggers.Add(entry);

            GameObject effectFill = new GameObject("effectFill");
            effectFill.tag = Game.DIALOG;
            effectFill.transform.SetParent(effectSlideObj.transform);
            effectFill.AddComponent<Image>();
            effectFill.GetComponent<Image>().color = Color.white;
            effectSlide.fillRect = effectFill.GetComponent<RectTransform>();
            effectSlide.fillRect.offsetMin = Vector2.zero;
            effectSlide.fillRect.offsetMax = Vector2.zero;

            GameObject effectSlideObjRev = new GameObject("effectSlideRev");
            effectSlideObjRev.tag = Game.DIALOG;
            effectSlideObjRev.transform.SetParent(game.uICanvas.transform);
            effectSlideRev = effectSlideObjRev.AddComponent<Slider>();
            RectTransform effectSlideRectRev = effectSlideObjRev.GetComponent<RectTransform>();
            effectSlideRectRev.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, effectBtnY * UIScaler.GetPixelsPerUnit(), ROW_BTN_H * UIScaler.GetPixelsPerUnit());
            effectSlideRectRev.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, rightX * UIScaler.GetPixelsPerUnit(), RIGHT_W * UIScaler.GetPixelsPerUnit());
            effectSlideRev.onValueChanged.AddListener(delegate { UpdateEffectsRev(); });
            effectSlideRev.direction = Slider.Direction.RightToLeft;
            effectSlideObjRev.AddComponent<EventTrigger>().triggers.Add(entry);

            GameObject effectFillRev = new GameObject("effectFillRev");
            effectFillRev.tag = Game.DIALOG;
            effectFillRev.transform.SetParent(effectSlideObjRev.transform);
            effectFillRev.AddComponent<Image>();
            effectFillRev.GetComponent<Image>().color = Color.clear;
            effectSlideRev.fillRect = effectFillRev.GetComponent<RectTransform>();
            effectSlideRev.fillRect.offsetMin = Vector2.zero;
            effectSlideRev.fillRect.offsetMax = Vector2.zero;

            effectSlide.value = eVolume;
            effectSlideRev.value = 1 - eVolume;
        }

        /// <summary>
        /// Method to create language UI elements in the screen
        /// </summary>
        private void CreateLanguageElements()
        {
            // === Language row (ROW0 on left) ===
            UIElement ui = new UIElement();
            ui.SetLocation(LEFT_X, ROW0_Y, LEFT_W, ROW_LABEL_H);
            ui.SetText(LANGUAGE);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());

            ui = new UIElement();
            ui.SetLocation(LEFT_X, ROW0_Y + ROW_LABEL_H, LEFT_W, ROW_BTN_H);
            ui.SetText(game.currentLang);
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(OpenLanguageSelection);
            new UIElementBorder(ui);
        }

        private void OpenLanguageSelection()
        {
            string[] langs = "English,Spanish,French,German,Italian,Portuguese,Polish,Russian,Chinese,Korean,Czech,Japanese,Ukrainian".Split(',');
            Array.Sort(langs);

            UIWindowSelectionList selection = new UIWindowSelectionList(
                delegate(string result)
                {
                    if (result != null)
                        SelectLang(result);
                    else
                        new OptionsScreen();
                },
                CHOOSE_LANG,
                callAfterCancel: true,
                showSortButtons: false
            );

            foreach (string lang in langs)
            {
                selection.AddItem(lang, lang, lang == game.currentLang ? Color.green : Color.white);
            }

            selection.Draw();
        }

        /// <summary>
        /// Method to create fallback language UI elements in the screen
        /// </summary>
        private void CreateFallbackLanguageElements()
        {
            // === Fallback Language row (ROW1 on left) ===
            UIElement ui = new UIElement();
            ui.SetLocation(LEFT_X, ROW1_Y, LEFT_W, ROW_LABEL_H);
            ui.SetText(FALLBACK_LANG);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());

            string currentFallback = game.fallbackLang ?? string.Empty;
            string displayLabel = string.IsNullOrEmpty(currentFallback) ? NONE.Translate() : currentFallback;
            Color displayColor = string.IsNullOrEmpty(currentFallback) ? Color.grey : Color.white;

            ui = new UIElement();
            ui.SetLocation(LEFT_X, ROW1_Y + ROW_LABEL_H, LEFT_W, ROW_BTN_H);
            ui.SetText(displayLabel, displayColor);
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(OpenFallbackLangSelection);
            new UIElementBorder(ui, displayColor);
        }

        private void OpenFallbackLangSelection()
        {
            string[] allLangs = "English,Spanish,French,German,Italian,Portuguese,Polish,Russian,Chinese,Korean,Czech,Japanese,Ukrainian".Split(',');
            Array.Sort(allLangs);

            UIWindowSelectionList selection = new UIWindowSelectionList(
                delegate(string result)
                {
                    if (result != null)
                        SelectFallbackLang(result == "\x00none" ? string.Empty : result);
                    else
                        new OptionsScreen();
                },
                FALLBACK_LANG,
                callAfterCancel: true,
                showSortButtons: false
            );

            // "None" option always at top
            selection.AddItem(NONE.Translate(), "\x00none", alwaysOnTop: true);

            // All languages — no filtering
            foreach (string lang in allLangs)
            {
                Color c = (lang == (game.fallbackLang ?? string.Empty)) ? Color.green : Color.white;
                selection.AddItem(lang, lang, c);
            }

            selection.Draw();
        }

        private void SelectFallbackLang(string lang)
        {
            game.fallbackLang = lang;
            game.config.data.Add("UserConfig", "fallbackLang", lang);
            game.config.Save();
            new OptionsScreen();
        }


        private void UpdateMusic()
        {
            musicSlideRev.value = 1 - musicSlide.value;
            game.config.data.Add("UserConfig", "music", musicSlide.value.ToString());
            game.config.Save();
            game.audioControl.audioSource.volume = musicSlide.value;
            game.audioControl.musicVolume = musicSlide.value;
        }

        private void UpdateMusicRev()
        {
            musicSlide.value = 1 - musicSlideRev.value;
            game.config.data.Add("UserConfig", "music", musicSlide.value.ToString());
            game.config.Save();
            game.audioControl.audioSource.volume = musicSlide.value;
            game.audioControl.musicVolume = musicSlide.value;
        }

        private void UpdateEffects()
        {
            effectSlideRev.value = 1 - effectSlide.value;
            game.config.data.Add("UserConfig", "effects", effectSlide.value.ToString());
            game.config.Save();
            game.audioControl.effectVolume = effectSlide.value;
        }

        private void UpdateEffectsRev()
        {
            effectSlide.value = 1 - effectSlideRev.value;
            game.config.data.Add("UserConfig", "effects", effectSlide.value.ToString());
            game.config.Save();
            game.audioControl.effectVolume = effectSlide.value;
        }

        private void PlayTestSound()
        {
            game.audioControl.PlayTest();
        }

        /// <summary>
        /// Select current language to specified
        /// </summary>
        /// <param name="langName"></param>
        private void SelectLang(string lang)
        {
            string newLang = lang;

            // If fallback is the same as the new main language, reset it to None (it would be redundant)
            if (game.fallbackLang == newLang)
            {
                game.fallbackLang = string.Empty;
                game.config.data.Add("UserConfig", "fallbackLang", string.Empty);
            }

            game.config.data.Add("UserConfig", "currentLang", newLang);
            game.config.Save();
            game.currentLang = newLang;
            LocalizationRead.ChangeCurrentLangTo(newLang);
            ValkyrieDebug.Log("new current language stablished:" + newLang + Environment.NewLine);

            new OptionsScreen();

            // clear list of local quests to make sure we take the latest changes
            Game.Get().questsList.UnloadLocalQuests();
        }



        private static void ScheduleOptionsScreenReload()
        {
            Game.Get().StartCoroutine(ReloadOptionsScreenNextFrame());
        }

        private static IEnumerator ReloadOptionsScreenNextFrame()
        {
            yield return null; // wait one frame
            new OptionsScreen();
        }
    }
}
