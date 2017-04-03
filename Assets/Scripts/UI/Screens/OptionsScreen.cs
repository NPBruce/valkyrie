using Assets.Scripts.Content;
using UnityEngine;

namespace Assets.Scripts.UI.Screens
{
    // Class for options menu
    public class OptionsScreen
    {
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
            DialogBox dbTittle = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 3), "Options");
            dbTittle.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
            dbTittle.SetFont(game.gameType.GetHeaderFont());

            //CreateLanguageElements();

            CreateAudioElements();

            // Button for back to main menu
            TextButton tb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Back", delegate { Destroyer.MainMenu(); }, Color.red);
            tb.SetFont(game.gameType.GetHeaderFont());
        }

        private void CreateAudioElements()
        {
            DialogBox db = new DialogBox(new Vector2(2, 8), new Vector2(UIScaler.GetWidthUnits() - 4, 2), "Music");
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            db.SetFont(game.gameType.GetHeaderFont());

            float mVolume;
            string vSet = game.config.data.Get("UserConfig", "music");
            float.TryParse(vSet, out mVolume);
            if (vSet.Length == 0) mVolume = 1;

            GameObject musicSlideObj = new GameObject("musicSlide");
            musicSlideObj.transform.parent = game.uICanvas.transform;
            musicSlide = musicSlideObj.AddComponent<UnityEngine.UI.Slider>();
            RectTransform musicSlideRect = musicSlideObj.GetComponent<RectTransform>();
            musicSlideRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 11 * UIScaler.GetPixelsPerUnit(), 2 * UIScaler.GetPixelsPerUnit());
            musicSlideRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, UIScaler.GetHCenter(-7) * UIScaler.GetPixelsPerUnit(), 14 * UIScaler.GetPixelsPerUnit());
            musicSlide.onValueChanged.AddListener(delegate { UpdateMusic(); });
            new RectangleBorder(musicSlideObj.transform, Color.white, new Vector2(musicSlideRect.rect.width / UIScaler.GetPixelsPerUnit(), musicSlideRect.rect.height / UIScaler.GetPixelsPerUnit()));

            GameObject musicFill = new GameObject("musicfill");
            musicFill.transform.parent = musicSlideObj.transform;
            musicFill.AddComponent<UnityEngine.UI.Image>();
            musicFill.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            musicSlide.fillRect = musicFill.GetComponent<RectTransform>();
            musicSlide.fillRect.offsetMin = Vector2.zero;
            musicSlide.fillRect.offsetMax = Vector2.zero;

            // Double slide is a hack because I can't get a click in the space to work otherwise
            GameObject musicSlideObjRev = new GameObject("musicSlideRev");
            musicSlideObjRev.transform.parent = game.uICanvas.transform;
            musicSlideRev = musicSlideObjRev.AddComponent<UnityEngine.UI.Slider>();
            RectTransform musicSlideRectRev = musicSlideObjRev.GetComponent<RectTransform>();
            musicSlideRectRev.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 11 * UIScaler.GetPixelsPerUnit(), 2 * UIScaler.GetPixelsPerUnit());
            musicSlideRectRev.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, UIScaler.GetHCenter(-7) * UIScaler.GetPixelsPerUnit(), 14 * UIScaler.GetPixelsPerUnit());
            musicSlideRev.onValueChanged.AddListener(delegate { UpdateMusicRev(); });
            musicSlideRev.direction = UnityEngine.UI.Slider.Direction.RightToLeft;

            GameObject musicFillRev = new GameObject("musicfillrev");
            musicFillRev.transform.parent = musicSlideObjRev.transform;
            musicFillRev.AddComponent<UnityEngine.UI.Image>();
            musicFillRev.GetComponent<UnityEngine.UI.Image>().color = Color.clear;
            musicSlideRev.fillRect = musicFillRev.GetComponent<RectTransform>();
            musicSlideRev.fillRect.offsetMin = Vector2.zero;
            musicSlideRev.fillRect.offsetMax = Vector2.zero;

            db = new DialogBox(new Vector2(2, 14), new Vector2(UIScaler.GetWidthUnits() - 4, 2), "Effects");
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            db.SetFont(game.gameType.GetHeaderFont());

            musicSlide.value = mVolume;
            musicSlideRev.value = 1 - mVolume;
        }


        /// <summary>
        /// Method to create language UI elements in the screen
        /// </summary>
        /// <param name="game">current game</param>
        private void CreateLanguageElements()
        {
            // Select langauge text
            DialogBox dbLanguage = new DialogBox(new Vector2(2, 4), new Vector2(UIScaler.GetWidthUnits() - 4, 2), "Choose Language");
            dbLanguage.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            dbLanguage.SetFont(game.gameType.GetHeaderFont());

            // The list of languages is determined by FFG languages for MoM
            // In D2E there is an additional language
            // It can change in future

            string[] langs = DictionaryI18n.FFG_LANGS.Split(',');

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
                    new Vector2(UIScaler.GetHCenter() - 4, verticalStart + (2f * position)),
                    new Vector2(8, 1.8f),
                    currentLanguage,
                    delegate { SelectLang(position); },
                    currentColor
                    );
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
