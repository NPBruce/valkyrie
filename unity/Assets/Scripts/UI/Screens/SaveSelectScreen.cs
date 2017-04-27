using Assets.Scripts.Content;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Screens
{
    class SaveSelectScreen
    {
        public bool save;
        Game game = Game.Get();
        private readonly StringKey SELECT_SAVE = new StringKey("val", "SELECT_SAVE");

        public SaveSelectScreen(bool performSave = false)
        {
            save = performSave;
            // This will destroy all, because we shouldn't have anything left at the main menu
            Destroyer.Destroy();

            // Create elements for the screen
            CreateElements();
        }

        private void CreateElements()
        {
            // Options screen text
            DialogBox dbTittle = new DialogBox(
                new Vector2(2, 1),
                new Vector2(UIScaler.GetWidthUnits() - 4, 3),
                SELECT_SAVE
                );
            dbTittle.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
            dbTittle.SetFont(game.gameType.GetHeaderFont());

            TextButton tb = new TextButton(
                new Vector2(UIScaler.GetHCenter(-20), 5f),
                new Vector2(40, 4f),
                new StringKey(null, "AutoSave", false),
                delegate { SaveManager.Load(0); });

            tb = new TextButton(
                new Vector2(UIScaler.GetHCenter(-20), 10f),
                new Vector2(40, 4f),
                new StringKey(null, "Save1", false),
                delegate { SaveManager.Load(1); });

            tb = new TextButton(
                new Vector2(UIScaler.GetHCenter(-20), 15f),
                new Vector2(40, 4f),
                new StringKey(null, "Save2", false),
                delegate { SaveManager.Load(2); });

            tb = new TextButton(
                new Vector2(UIScaler.GetHCenter(-20), 20f),
                new Vector2(40, 4f),
                new StringKey(null, "Save3", false),
                delegate { SaveManager.Load(3); });

            // Button for back to main menu
            tb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2),
                CommonStringKeys.BACK, delegate { Destroyer.MainMenu(); }, Color.red);
            tb.SetFont(game.gameType.GetHeaderFont());
        }
    }
}
