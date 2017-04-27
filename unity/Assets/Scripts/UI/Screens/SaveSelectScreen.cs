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

            float offset = 5f;
            TextButton tb;
            List<SaveManager.SaveData> saves = SaveManager.GetSaves();
            for (int i = 0; i < saves.Count; i++)
            {
                int tmp = i;
                if (saves[i].valid)
                {
                    tb = new TextButton(
                        new Vector2(UIScaler.GetHCenter(-20), offset),
                        new Vector2(40, 4f),
                        new StringKey(null, saves[i].quest, false),
                        delegate { SaveManager.Load(tmp); });
                }
                else
                {
                    tb = new TextButton(
                        new Vector2(UIScaler.GetHCenter(-20), offset),
                        new Vector2(40, 4f),
                        new StringKey(null, "", false),
                        delegate { ; }, Color.gray);
                }
                offset += 5;
            }

            // Button for back to main menu
            tb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2),
                CommonStringKeys.BACK, delegate { Destroyer.MainMenu(); }, Color.red);
            tb.SetFont(game.gameType.GetHeaderFont());
        }
    }
}
