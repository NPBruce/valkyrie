using Assets.Scripts.Content;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ValkyrieTools;

namespace Assets.Scripts.UI.Screens
{
    // Class for options menu
    public class OptionsScreen
    {
        private readonly StringKey OPTIONS = new StringKey("val", "OPTIONS");
        private readonly StringKey CHOOSE_LANG = new StringKey("val", "CHOOSE_LANG");
        private readonly StringKey EFFECTS = new StringKey("val", "EFFECTS");
        private readonly StringKey MUSIC = new StringKey("val", "MUSIC");

        Game game = Game.Get();

        // array of text buttons with all languages
        TextButton[] languageTextButtons;

        public UnityEngine.UI.Slider musicSlide;
        public UnityEngine.UI.Slider musicSlideRev;
        public UnityEngine.UI.Slider effectSlide;
        public UnityEngine.UI.Slider effectSlideRev;

        int selectedIndex = -1;

        // Create a menu which will take up the whole screen and have options.  All items are dialog for destruction.
        public OptionsScreen()
        {
            // This will destroy all, because we shouldn't have anything left at the main menu
            Destroyer.Destroy();

            // Create elements for the screen
            CreateElements();
        }

        /// <summary>
        /// Method to create UI elements in the screen
        /// </summary>
        /// <param name="game">current game</param>
        private void CreateElements()
        {
            // Options screen text
            DialogBox dbTittle = new DialogBox(
                new Vector2(0, 1), 
                new Vector2(UIScaler.GetWidthUnits() - 4, 3), 
                OPTIONS
                );
            dbTittle.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
            dbTittle.SetFont(game.gameType.GetHeaderFont());

            CreateLanguageElements();

            CreateAudioElements();

            // Button for back to main menu
            TextButton tb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), 
                CommonStringKeys.BACK, delegate { Destroyer.MainMenu(); }, Color.red);
            tb.SetFont(game.gameType.GetHeaderFont());
        }

        private void CreateAudioElements()
        {
            DialogBox db = new DialogBox(new Vector2(((0.75f * UIScaler.GetWidthUnits()) - 5), 8), new Vector2(10, 2), MUSIC);
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            db.SetFont(game.gameType.GetHeaderFont());

            float mVolume;
            string vSet = game.config.data.Get("UserConfig", "music");
            float.TryParse(vSet, out mVolume);
            if (vSet.Length == 0) mVolume = 1;

            GameObject musicSlideObj = new GameObject("musicSlide");
            musicSlideObj.tag = "dialog";
            musicSlideObj.transform.parent = game.uICanvas.transform;
            musicSlide = musicSlideObj.AddComponent<UnityEngine.UI.Slider>();
            RectTransform musicSlideRect = musicSlideObj.GetComponent<RectTransform>();
            musicSlideRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 11 * UIScaler.GetPixelsPerUnit(), 2 * UIScaler.GetPixelsPerUnit());
            musicSlideRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, ((0.75f * UIScaler.GetWidthUnits()) - 7) * UIScaler.GetPixelsPerUnit(), 14 * UIScaler.GetPixelsPerUnit());
            musicSlide.onValueChanged.AddListener(delegate { UpdateMusic(); });
            new RectangleBorder(musicSlideObj.transform, Color.white, new Vector2(musicSlideRect.rect.width / UIScaler.GetPixelsPerUnit(), musicSlideRect.rect.height / UIScaler.GetPixelsPerUnit()));

            GameObject musicFill = new GameObject("musicfill");
            musicFill.tag = "dialog";
            musicFill.transform.parent = musicSlideObj.transform;
            musicFill.AddComponent<UnityEngine.UI.Image>();
            musicFill.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            musicSlide.fillRect = musicFill.GetComponent<RectTransform>();
            musicSlide.fillRect.offsetMin = Vector2.zero;
            musicSlide.fillRect.offsetMax = Vector2.zero;

            // Double slide is a hack because I can't get a click in the space to work otherwise
            GameObject musicSlideObjRev = new GameObject("musicSlideRev");
            musicSlideObjRev.tag = "dialog";
            musicSlideObjRev.transform.parent = game.uICanvas.transform;
            musicSlideRev = musicSlideObjRev.AddComponent<UnityEngine.UI.Slider>();
            RectTransform musicSlideRectRev = musicSlideObjRev.GetComponent<RectTransform>();
            musicSlideRectRev.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 11 * UIScaler.GetPixelsPerUnit(), 2 * UIScaler.GetPixelsPerUnit());
            musicSlideRectRev.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, ((0.75f * UIScaler.GetWidthUnits()) - 7) * UIScaler.GetPixelsPerUnit(), 14 * UIScaler.GetPixelsPerUnit());
            musicSlideRev.onValueChanged.AddListener(delegate { UpdateMusicRev(); });
            musicSlideRev.direction = UnityEngine.UI.Slider.Direction.RightToLeft;

            GameObject musicFillRev = new GameObject("musicfillrev");
            musicFillRev.tag = "dialog";
            musicFillRev.transform.parent = musicSlideObjRev.transform;
            musicFillRev.AddComponent<UnityEngine.UI.Image>();
            musicFillRev.GetComponent<UnityEngine.UI.Image>().color = Color.clear;
            musicSlideRev.fillRect = musicFillRev.GetComponent<RectTransform>();
            musicSlideRev.fillRect.offsetMin = Vector2.zero;
            musicSlideRev.fillRect.offsetMax = Vector2.zero;


            musicSlide.value = mVolume;
            musicSlideRev.value = 1 - mVolume;

            db = new DialogBox(new Vector2(((0.75f * UIScaler.GetWidthUnits()) - 5), 14), new Vector2(10, 2), EFFECTS);
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            db.SetFont(game.gameType.GetHeaderFont());

            float eVolume;
            vSet = game.config.data.Get("UserConfig", "effects");
            float.TryParse(vSet, out eVolume);
            if (vSet.Length == 0) eVolume = 1;

            GameObject effectSlideObj = new GameObject("effectSlide");
            effectSlideObj.tag = "dialog";
            effectSlideObj.transform.parent = game.uICanvas.transform;
            effectSlide = effectSlideObj.AddComponent<UnityEngine.UI.Slider>();
            RectTransform effectSlideRect = effectSlideObj.GetComponent<RectTransform>();
            effectSlideRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 17 * UIScaler.GetPixelsPerUnit(), 2 * UIScaler.GetPixelsPerUnit());
            effectSlideRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, ((0.75f * UIScaler.GetWidthUnits()) - 7) * UIScaler.GetPixelsPerUnit(), 14 * UIScaler.GetPixelsPerUnit());
            effectSlide.onValueChanged.AddListener(delegate { UpdateEffects(); });
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerUp;
            entry.callback.AddListener(delegate { PlayTestSound(); });
            effectSlideObj.AddComponent<EventTrigger>().triggers.Add(entry);
            new RectangleBorder(effectSlideObj.transform, Color.white, new Vector2(effectSlideRect.rect.width / UIScaler.GetPixelsPerUnit(), effectSlideRect.rect.height / UIScaler.GetPixelsPerUnit()));

            GameObject effectFill = new GameObject("effectFill");
            effectFill.tag = "dialog";
            effectFill.transform.parent = effectSlideObj.transform;
            effectFill.AddComponent<UnityEngine.UI.Image>();
            effectFill.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            effectSlide.fillRect = effectFill.GetComponent<RectTransform>();
            effectSlide.fillRect.offsetMin = Vector2.zero;
            effectSlide.fillRect.offsetMax = Vector2.zero;

            // Double slide is a hack because I can't get a click in the space to work otherwise
            GameObject effectSlideObjRev = new GameObject("effectSlideRev");
            effectSlideObjRev.tag = "dialog";
            effectSlideObjRev.transform.parent = game.uICanvas.transform;
            effectSlideRev = effectSlideObjRev.AddComponent<UnityEngine.UI.Slider>();
            RectTransform effectSlideRectRev = effectSlideObjRev.GetComponent<RectTransform>();
            effectSlideRectRev.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 17 * UIScaler.GetPixelsPerUnit(), 2 * UIScaler.GetPixelsPerUnit());
            effectSlideRectRev.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, ((0.75f * UIScaler.GetWidthUnits()) - 7) * UIScaler.GetPixelsPerUnit(), 14 * UIScaler.GetPixelsPerUnit());
            effectSlideRev.onValueChanged.AddListener(delegate { UpdateEffectsRev(); });
            effectSlideRev.direction = UnityEngine.UI.Slider.Direction.RightToLeft;
            effectSlideObjRev.AddComponent<EventTrigger>().triggers.Add(entry);

            GameObject effectFillRev = new GameObject("effectFillRev");
            effectFillRev.tag = "dialog";
            effectFillRev.transform.parent = effectSlideObjRev.transform;
            effectFillRev.AddComponent<UnityEngine.UI.Image>();
            effectFillRev.GetComponent<UnityEngine.UI.Image>().color = Color.clear;
            effectSlideRev.fillRect = effectFillRev.GetComponent<RectTransform>();
            effectSlideRev.fillRect.offsetMin = Vector2.zero;
            effectSlideRev.fillRect.offsetMax = Vector2.zero;

            effectSlide.value = eVolume;
            effectSlideRev.value = 1 - eVolume;
        }


        /// <summary>
        /// Method to create language UI elements in the screen
        /// </summary>
        /// <param name="game">current game</param>
        private void CreateLanguageElements()
        {
            // Select langauge text
            DialogBox dbLanguage = new DialogBox(
                new Vector2(((0.25f * UIScaler.GetWidthUnits()) - 9), 4), 
                new Vector2(18, 2), 
                CHOOSE_LANG
                );
            dbLanguage.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            dbLanguage.SetFont(game.gameType.GetHeaderFont());

            // The list of languages is determined by FFG languages for MoM
            // In D2E there is an additional language
            // It can change in future

            string[] langs = DictionaryI18n.FFG_LANGS.Split(',');
            // For now, English and Spanish languages available.
            HashSet<string> enabled_langs = new HashSet<string> ("English,Spanish,French,Italian,German".Split(','));

            //The first button in the list of buttons should start in this vertical coordinate
            float verticalStart = UIScaler.GetVCenter(-1f) - ((langs.Length - 1) * 1f);

            languageTextButtons = new TextButton[langs.Length];
            for (int i = 1; i < langs.Length; i++)
            {
                int position = i;
                // Need current index in order to delegate not point to loop for variable
                string currentLanguage = langs[position];
                Color currentColor = Color.gray;

                if (currentLanguage == game.currentLang)
                {
                    selectedIndex = position;
                    currentColor = Color.white;
                }
                languageTextButtons[position] = new TextButton(
                    new Vector2((0.25f * UIScaler.GetWidthUnits()) - 4, verticalStart + (2f * position)),
                    new Vector2(8, 1.8f),
                    new StringKey(null, currentLanguage, false),
                    delegate { SelectLang(position); },
                    currentColor
                    );

                if (!enabled_langs.Contains(currentLanguage))
                {
                    languageTextButtons[position].setColor(Color.red);
                    languageTextButtons[position].setActive(false);
                }


            }
        }

        private void UpdateMusic()
        {
            musicSlideRev.value = 1 - musicSlide.value;
            game.config.data.Add("UserConfig", "music", musicSlide.value.ToString());
            game.config.Save();
            game.audioControl.audioSource.volume = musicSlide.value;
        }

        private void UpdateMusicRev()
        {
            musicSlide.value = 1 - musicSlideRev.value;
            game.config.data.Add("UserConfig", "music", musicSlide.value.ToString());
            game.config.Save();
            game.audioControl.audioSource.volume = musicSlide.value;
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
        private void SelectLang(int langPosition)
        {
            // Disable previous button
            if (selectedIndex > 0)
            {
                languageTextButtons[selectedIndex].setColor(Color.gray);
            }
            selectedIndex = langPosition;

            // Set newn lang in UI...
            string newLang = languageTextButtons[langPosition].uiText.text;        
            languageTextButtons[langPosition].setColor(Color.white);

            // ... and in configuration
            game.config.data.Add("UserConfig", "currentLang", newLang);
            game.config.Save();
            game.currentLang = newLang;
            LocalizationRead.changeCurrentLangTo(newLang);
            refreshScreen();
            ValkyrieDebug.Log("new current language stablished:" + newLang + System.Environment.NewLine);
        }

        /// <summary>
        /// Refreshes the screen texts
        /// </summary>
        private void refreshScreen()
        {
            // TODO
            // Reprint all texts in the screen.
        }
    }

}
