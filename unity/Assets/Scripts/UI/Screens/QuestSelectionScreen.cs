using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using System.IO;

namespace Assets.Scripts.UI.Screens
{
    // Class for quest selection window
    public class QuestSelectionScreen
    {
        private StringKey DOWNLOAD = new StringKey("val", "DOWNLOAD");

        public Dictionary<string, QuestData.Quest> questList;

        public QuestSelectionScreen(Dictionary<string, QuestData.Quest> ql)
        {
            questList = ql;
            Game game = Game.Get();
            // If a dialog window is open we force it closed (this shouldn't happen)
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
                Object.Destroy(go);

            // Clean up downloader if present
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.QUESTUI))
                Object.Destroy(go);

            // Heading
            UIElement ui = new UIElement();
            ui.SetLocation(2, 1, UIScaler.GetWidthUnits() - 4, 3);
            ui.SetText(new StringKey("val", "SELECT", game.gameType.QuestName()));
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetLargeFont());

            UIElementScrollVertical scrollArea = new UIElementScrollVertical();
            scrollArea.SetLocation(1, 5, UIScaler.GetWidthUnits() - 2f, 21f);
            new UIElementBorder(scrollArea);

            // Start here
            float offset = 0;
            // Loop through all available quests
            foreach (KeyValuePair<string, QuestData.Quest> q in questList)
            {
                if (q.Value.GetMissingPacks(game.cd.GetLoadedPackIDs()).Count == 0)
                {
                    string key = q.Key;
                    LocalizationRead.AddDictionary("qst", q.Value.localizationDict);
                    string translation = q.Value.name.Translate();

                    // Frame
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(0.95f, offset, UIScaler.GetWidthUnits() - 4.9f, 3.1f);
                    ui.SetBGColor(Color.white);
                    ui.SetButton(delegate { Selection(key); });
                    offset += 0.05f;

                    // Draw Image
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(1, offset, 3, 3);
                    ui.SetBGColor(Color.white);
                    ui.SetButton(delegate { Selection(key); });
                    if (q.Value.image.Length > 0)
                    {
                        ui.SetImage(ContentData.FileToTexture(Path.Combine(q.Value.path, q.Value.image)));
                    }

                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetBGColor(Color.clear);
                    ui.SetLocation(4, offset, UIScaler.GetWidthUnits() - 8, 3f);
                    ui.SetTextPadding(1.2f);
                    ui.SetText(translation, Color.black);
                    ui.SetButton(delegate { Selection(key); });
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetFontSize(Mathf.RoundToInt(UIScaler.GetSmallFont() * 1.3f));

                    // Duration
                    if (q.Value.lengthMax != 0)
                    {
                        ui = new UIElement(scrollArea.GetScrollTransform());
                        ui.SetLocation(UIScaler.GetRight(-11), offset, 2, 1);
                        ui.SetText(q.Value.lengthMin.ToString(), Color.black);
                        ui.SetButton(delegate { Selection(key); });
                        ui.SetBGColor(Color.clear);

                        ui = new UIElement(scrollArea.GetScrollTransform());
                        ui.SetLocation(UIScaler.GetRight(-9), offset, 1, 1);
                        ui.SetButton(delegate { Selection(key); });
                        ui.SetText("-", Color.black);
                        ui.SetBGColor(Color.clear);

                        ui = new UIElement(scrollArea.GetScrollTransform());
                        ui.SetLocation(UIScaler.GetRight(-8), offset, 2, 1);
                        ui.SetText(q.Value.lengthMax.ToString(), Color.black);
                        ui.SetButton(delegate { Selection(key); });
                        ui.SetBGColor(Color.clear);
                    }

                    // Difficulty
                    if (q.Value.difficulty != 0)
                    {
                        string symbol = "π"; // will
                        if (game.gameType is MoMGameType)
                        {
                            symbol = new StringKey("val", "ICON_SUCCESS_RESULT").Translate();
                        }
                        ui = new UIElement(scrollArea.GetScrollTransform());
                        ui.SetLocation(UIScaler.GetRight(-13), offset + 1, 9, 2);
                        ui.SetText(symbol + symbol + symbol + symbol + symbol, Color.black);
                        ui.SetBGColor(Color.clear);
                        ui.SetFontSize(UIScaler.GetMediumFont());
                        ui.SetButton(delegate { Selection(key); });

                        ui = new UIElement(scrollArea.GetScrollTransform());
                        ui.SetLocation(UIScaler.GetRight(-11.95f) + (q.Value.difficulty * 6.9f), offset + 1, (1 - q.Value.difficulty) * 6.9f, 2);
                        ui.SetBGColor(new Color(1, 1, 1, 0.7f));
                        ui.SetButton(delegate { Selection(key); });
                    }

                    offset += 4;
                }
            }

            // Loop through all unavailable quests
            foreach (KeyValuePair<string, QuestData.Quest> q in questList)
            {
                if (q.Value.GetMissingPacks(game.cd.GetLoadedPackIDs()).Count > 0)
                {
                    string key = q.Key;
                    LocalizationRead.AddDictionary("qst", q.Value.localizationDict);
                    string translation = q.Value.name.Translate();

                    // Size is 1.2 to be clear of characters with tails
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(1, offset, UIScaler.GetWidthUnits() - 5, 1.2f);
                    ui.SetText(new StringKey("val", "INDENT", translation), Color.black);
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetBGColor(new Color(0.4f, 0.4f, 0.4f));
                    offset += 1.2f;

                    foreach (string s in q.Value.GetMissingPacks(game.cd.GetLoadedPackIDs()))
                    {
                        ui = new UIElement(scrollArea.GetScrollTransform());
                        ui.SetLocation(3, offset, UIScaler.GetWidthUnits() - 9, 1.2f);
                        ui.SetText(new StringKey("val", "REQUIRES_EXPANSION", game.cd.GetContentName(s)), Color.black);
                        ui.SetTextAlignment(TextAnchor.MiddleLeft);
                        ui.SetBGColor(new Color(0.4f, 0.4f, 0.4f));
                        offset += 1.2f;
                    }
                }
                offset += 0.8f;
            }

            scrollArea.SetScrollSize(offset);

            ui = new UIElement();
            ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
            ui.SetText(CommonStringKeys.BACK, Color.red);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(delegate { Cancel(); });
            new UIElementBorder(ui, Color.red);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetRight(-9), UIScaler.GetBottom(-3), 8, 2);
            ui.SetText(DOWNLOAD, Color.green);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(delegate { Download(); });
            new UIElementBorder(ui, Color.green);
        }

        // Return to main menu
        public void Cancel()
        {
            Destroyer.MainMenu();
        }

        // Return to main menu
        public void Download()
        {
            Destroyer.Dialog();
            GameObject download = new GameObject("downloadPage");
            download.tag = Game.QUESTUI;
            download.AddComponent<QuestDownload>();
        }

        // Select a quest
        public void Selection(string key)
        {
            Destroyer.Dialog();
            new QuestDetailsScreen(questList[key]);
        }
    }
}