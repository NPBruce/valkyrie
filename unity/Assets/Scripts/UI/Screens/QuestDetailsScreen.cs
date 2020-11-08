using System.IO;
using Assets.Scripts.Content;
using UnityEngine;
using ValkyrieTools;

namespace Assets.Scripts.UI.Screens
{
    public class QuestDetailsScreen
    {
        public QuestDetailsScreen(QuestData.Quest q)
        {
            Game game = Game.Get();
            LocalizationRead.AddDictionary("qst", q.localizationDict);
            // If a dialog window is open we force it closed (this shouldn't happen)
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
                Object.Destroy(go);

            // Heading
            UIElement ui = new UIElement();
            ui.SetLocation(2, 0.5f, UIScaler.GetWidthUnits() - 4, 3);
            ui.SetText(q.name);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetLargeFont());

            // Draw Image
            if (q.image.Length > 0)
            {
                ui = new UIElement();
                ui.SetLocation(UIScaler.GetHCenter(-20), 4, 8, 8);
                ui.SetImage(ContentData.FileToTexture(Path.Combine(q.path, q.image)));
                new UIElementBorder(ui);
            }

            // Draw Description
            ui = new UIElement();
            float height = ui.GetStringHeight(q.description, 30);
            if (height > 25) height = 25;
            ui.SetLocation(UIScaler.GetHCenter(-7), 15 - (height / 2), 30, height);
            ui.SetText(q.description);
            new UIElementBorder(ui);

            // Draw authors
            ui = new UIElement();
            height = ui.GetStringHeight(q.authors, 14);
            if (height > 25) height = 25;
            ui.SetLocation(UIScaler.GetHCenter(-23), 18.5f - (height / 2), 14, height);
            ui.SetText(q.authors);
            new UIElementBorder(ui);

            // Difficulty
            if (q.difficulty != 0)
            {
                ui = new UIElement();
                ui.SetLocation(UIScaler.GetHCenter(-13), 27, 11, 1);
                ui.SetText(new StringKey("val","DIFFICULTY"));
                string symbol = "*";
                if (game.gameType is MoMGameType)
                {
                    symbol = new StringKey("val", "ICON_SUCCESS_RESULT").Translate();
                }
                ui = new UIElement();
                ui.SetLocation(UIScaler.GetHCenter(-13), 28, 11, 2);
                ui.SetText(symbol + symbol + symbol + symbol + symbol);
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui = new UIElement();
                ui.SetLocation(UIScaler.GetHCenter(-10.95f) + (q.difficulty * 6.9f), 28, (1 - q.difficulty) * 6.9f, 2);
                ui.SetBGColor(new Color(0, 0, 0, 0.7f));
            }

            // Duration
            if (q.lengthMax != 0)
            {
                ui = new UIElement();
                ui.SetLocation(UIScaler.GetHCenter(2), 27, 11, 1);
                ui.SetText(new StringKey("val","DURATION"));

                ui = new UIElement();
                ui.SetLocation(UIScaler.GetHCenter(2), 28, 4, 2);
                ui.SetText(q.lengthMin.ToString());
                ui.SetFontSize(UIScaler.GetMediumFont());

                ui = new UIElement();
                ui.SetLocation(UIScaler.GetHCenter(6.5f), 28, 2, 2);
                ui.SetText("-");
                ui.SetFontSize(UIScaler.GetMediumFont());

                ui = new UIElement();
                ui.SetLocation(UIScaler.GetHCenter(9), 28, 4, 2);
                ui.SetText(q.lengthMax.ToString());
                ui.SetFontSize(UIScaler.GetMediumFont());
            }

            // DELETE button (only for archive, directory might be edited by user)
            if (Path.GetExtension(Path.GetFileName(q.path)) == ".valkyrie")
            {
                ui = new UIElement();
                ui.SetLocation(UIScaler.GetRight(-8.5f), 0.5f, 8, 2);
                ui.SetText(CommonStringKeys.DELETE, Color.grey);
                ui.SetFont(game.gameType.GetHeaderFont());
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetButton(delegate { Delete(q); });
                new UIElementBorder(ui, Color.grey);
            }

            ui = new UIElement();
            ui.SetLocation(0.5f, UIScaler.GetBottom(-2.5f), 8, 2);
            ui.SetText(CommonStringKeys.BACK, Color.red);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(Cancel);
            new UIElementBorder(ui, Color.red);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetRight(-8.5f), UIScaler.GetBottom(-2.5f), 8, 2);
            ui.SetText(new StringKey("val", "START"), Color.green);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(delegate { Start(q); });
            new UIElementBorder(ui, Color.green);


        }

        /// <summary>
        /// Select to delete
        /// </summary>
        /// <param file="file">File name to delete</param>
        public void Delete(QuestData.Quest q)
        {
            ValkyrieDebug.Log("INFO: Delete quest");

            string toDelete = "";

            if (Path.GetExtension(Path.GetFileName(q.path)) == ".valkyrie")
            {
                toDelete = ContentData.DownloadPath() + Path.DirectorySeparatorChar + Path.GetFileName(q.path);
                File.Delete(toDelete);

                // update quest status : downloaded/updated
                Game.Get().questsList.SetQuestAvailability(Path.GetFileNameWithoutExtension(q.path), false);
            }
            else
            {
                // this is not an archive, it is a local quest within a directory
                Directory.Delete(q.path, true);

                Game.Get().questsList.UnloadLocalQuests();
            }

            Destroyer.Dialog();

            // Pull up the quest selection page
            Game.Get().questSelectionScreen.Show();
        }

        // Return to quest selection
        public void Cancel()
        {
            ValkyrieDebug.Log("INFO: Return to quest list from details screen");

            // Pull up the quest selection page
            GameStateManager.Quest.List();
        }

        // Select a quest
        public void Start(QuestData.Quest q)
        {
            ValkyrieDebug.Log("INFO: Start quest from details screen");
            GameStateManager.Quest.Start(q);
        }
    }
}
