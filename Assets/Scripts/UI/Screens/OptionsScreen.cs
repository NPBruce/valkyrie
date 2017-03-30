using Assets.Scripts.Content;
using UnityEngine;

namespace Assets.Scripts.UI.Screens
{
    // Class for options menu
    public class OptionsScreen
    {
        private StringKey OPTIONS = new StringKey("val", "OPTIONS");
        private StringKey CHOOSE_LANG = new StringKey("val", "CHOOSE_LANG");

        Game game = Game.Get();

        // array of text buttons with all languages
        TextButton[] languageTextButtons;

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
                new Vector2(2, 1), 
                new Vector2(UIScaler.GetWidthUnits() - 4, 3), 
                OPTIONS
                );
            dbTittle.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
            dbTittle.SetFont(game.gameType.GetHeaderFont());

            // Select langauge text
            DialogBox dbLanguage = new DialogBox(
                new Vector2(2, 4), 
                new Vector2(UIScaler.GetWidthUnits() - 4, 2), 
                CHOOSE_LANG
                );
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
                    new StringKey(currentLanguage,false),
                    delegate { SelectLang(position); },
                    currentColor
                    );
            }

            // Button for back to main menu
            TextButton tb = new TextButton(
                new Vector2(1, UIScaler.GetBottom(-3)), 
                new Vector2(8, 2),
                CommonStringKeys.BACK,
                delegate { Destroyer.MainMenu(); }, 
                Color.red);
            tb.SetFont(game.gameType.GetHeaderFont());
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
