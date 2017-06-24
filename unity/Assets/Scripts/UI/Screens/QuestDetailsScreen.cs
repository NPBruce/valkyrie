using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using System.IO;

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
            float height = UIElement.GetStringHeight(q.description, 30);
            if (height > 25) height = 25;
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-7), 15 - (height / 2), 30, height);
            ui.SetText(q.description);
            new UIElementBorder(ui);

            // Draw authors
            height = UIElement.GetStringHeight(q.authors, 14);
            if (height > 25) height = 25;
            ui = new UIElement();
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

        // Return to quest selection
        public void Cancel()
        {
            Destroyer.Dialog();
            // Get a list of available quests
            Dictionary<string, QuestData.Quest> ql = QuestLoader.GetQuests();

            // Pull up the quest selection page
            new QuestSelectionScreen(ql);
        }

        // Select a quest
        public void Start(QuestData.Quest q)
        {
            Destroyer.Dialog();
            Game.Get().StartQuest(q);
        }
    }
}
