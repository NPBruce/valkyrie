using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using System.IO;

namespace Assets.Scripts.UI.Screens
{
    // Class for quest selection window
    public class QuestSelectionScreen
    {
        List<string> questList = null;
        UIElementScrollVertical scrollArea = null;

        public QuestSelectionScreen()
        {
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

            // back to menu
            ui = new UIElement();
            ui.SetLocation(1, 0.5f, 8, 1.5f);
            ui.SetText(CommonStringKeys.BACK, Color.red);
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(delegate { Cancel(); });
            new UIElementBorder(ui, Color.red);

            // Sort options
            //sorted_by_rating = nu
            //sorted_by_name = null
            //sorted_by_difficulty
            //sorted_by_duration = nul
            //sorted_by_date = null

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetWidthUnits()-7, 1f, 7, 1f);
            if (game.questsList.download_done)
            {
                ui.SetText("rating");
                ui.SetButton(delegate { SetSort("rating"); });
            }
            else
            {
                ui.SetText("rating", Color.red);
            }
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetSmallFont());
            new UIElementBorder(ui, Color.red);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetWidthUnits() - 7, 2f, 7, 1f);
            ui.SetText("name");
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetSmallFont());
            ui.SetButton(delegate { SetSort("name"); });
            new UIElementBorder(ui, Color.red);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetWidthUnits() - 7, 3f, 7, 1f);
            ui.SetText("difficulty");
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetSmallFont());
            ui.SetButton(delegate { SetSort("difficulty"); });
            new UIElementBorder(ui, Color.red);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetWidthUnits() - 7, 4f, 7, 1f);
            ui.SetText("duration");
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetSmallFont());
            ui.SetButton(delegate { SetSort("duration"); });
            new UIElementBorder(ui, Color.red);

            if (game.questsList.download_done)
            {
                questList = game.questsList.GetList("rating");
            }
            else
            {
                // Display offline message
                ui = new UIElement();
                ui.SetLocation(UIScaler.GetHCenter(), 0.1f, 8, 1.2f);
                ui.SetText("OFFLINE", Color.red);
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetTextAlignment(TextAnchor.MiddleCenter);

                // Get and load a list of all locally available quests
                game.questsList.loadAllLocalQuests();
                questList = game.questsList.GetLocalList();
            }

            // scroll area
            scrollArea = new UIElementScrollVertical(Game.QUESTLIST);
            scrollArea.SetLocation(1, 5, UIScaler.GetWidthUnits() - 2f, UIScaler.GetHeightUnits() - 6f);
            new UIElementBorder(scrollArea);

            RefreshQuestList();
        }

        public void DestroyQuestList()
        {
            // Clean up everything marked as 'questlist'
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.QUESTLIST))
                Object.Destroy(go);

            // scroll area
            scrollArea = new UIElementScrollVertical(Game.QUESTLIST);
            scrollArea.SetLocation(1, 5, UIScaler.GetWidthUnits() - 2f, UIScaler.GetHeightUnits() - 6f);
            new UIElementBorder(scrollArea);
        }

        public void RefreshQuestList()
        {
            Game game = Game.Get();
            UIElement ui = null;

            // Start here
            float offset = 0;
            // Loop through all available quests
            foreach (string key in questList)
            {
                QuestData.Quest q = game.questsList.getQuestData(key);

                string translation = "";
                if (game.questsList.download_done)
                {
                    // quest name is local language, or default language
                    if (q.languages_name != null && !q.languages_name.TryGetValue(game.currentLang, out translation))
                    {
                        translation = q.languages_name[q.defaultLanguage];
                    }
                }
                else
                {
                    LocalizationRead.AddDictionary("qst", q.localizationDict);
                    translation = q.name.Translate();
                }

                // Frame
                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(0.95f, offset, UIScaler.GetWidthUnits() - 4.9f, 3.6f);
                ui.SetBGColor(Color.white);
                ui.SetButton(delegate { Selection(key); });
                offset += 0.05f;
                new UIElementBorder(ui, Color.grey);

                // Draw Image
                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(1, offset, 3.5f, 3.5f);
                ui.SetBGColor(Color.white);
                ui.SetButton(delegate { Selection(key); });
                if (q.image.Length > 0)
                {
                    //ui.SetImage(ContentData.FileToTexture(Path.Combine(q.Value.path, q.Value.image)));
                }

                // Quest name
                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetBGColor(Color.clear);
                ui.SetLocation(5f, offset - 0.2f, UIScaler.GetWidthUnits() - 8, 2.5f);
                ui.SetTextPadding(0.5f);
                ui.SetText(translation, Color.black);
                ui.SetButton(delegate { Selection(key); });
                ui.SetTextAlignment(TextAnchor.MiddleLeft);
                ui.SetFontSize(Mathf.RoundToInt(UIScaler.GetSmallFont() * 1.4f));
                ui.SetFont(game.gameType.GetHeaderFont());

                // TODO support missing packs
                //if (q.Value.GetMissingPacks(game.cd.GetLoadedPackIDs()).Count > 0)

                // Duration
                if (q.lengthMax != 0)
                {
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(6.5f, offset + 2.3f, 4, 1);
                    ui.SetText(new StringKey("val", "DURATION"), Color.black);
                    ui.SetButton(delegate { Selection(key); });
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetBGColor(Color.clear);

                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(10.5f, offset + 2.3f, 5, 1);
                    ui.SetText(q.lengthMin + "  -  " + q.lengthMax, Color.black);
                    ui.SetButton(delegate { Selection(key); });
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetBGColor(Color.clear);
                }

                // Difficulty
                if (q.difficulty != 0)
                {
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(UIScaler.GetHCenter() - 5.5f, offset + 2.3f, 6, 1);
                    ui.SetText(new StringKey("val", "DIFFICULTY"), Color.black);
                    ui.SetButton(delegate { Selection(key); });
                    ui.SetTextAlignment(TextAnchor.MiddleRight);
                    ui.SetBGColor(Color.clear);

                    string symbol = "π"; // will
                    if (game.gameType is MoMGameType)
                    {
                        symbol = new StringKey("val", "ICON_SUCCESS_RESULT").Translate();
                    }
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(UIScaler.GetHCenter(), offset + 1.8f, 9, 2);
                    ui.SetText(symbol + symbol + symbol + symbol + symbol, Color.black);
                    ui.SetBGColor(Color.clear);
                    ui.SetFontSize(UIScaler.GetMediumFont());
                    ui.SetButton(delegate { Selection(key); });

                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(UIScaler.GetHCenter() + 1.05f + (q.difficulty * 6.9f), offset + 1.8f, (1 - q.difficulty) * 6.9f, 1.6f);
                    ui.SetBGColor(new Color(1, 1, 1, 0.7f));
                    ui.SetButton(delegate { Selection(key); });
                }

                // Statistics
                string filename = key.ToLower() + ".valkyrie";
                if (game.stats != null && game.stats.scenarios_stats != null && game.stats.scenarios_stats.ContainsKey(filename))
                {
                    ScenarioStats q_stats = game.stats.scenarios_stats[filename];
                    int win_ratio = (int)(q_stats.scenario_avg_win_ratio * 100);

                    StringKey STATS_AVERAGE_WIN_RATIO = new StringKey("val", "STATS_AVERAGE_WIN_RATIO", win_ratio);
                    StringKey STATS_NO_AVERAGE_WIN_RATIO = new StringKey("val", "STATS_NO_AVERAGE_WIN_RATIO", win_ratio);
                    StringKey STATS_NB_USER_REVIEWS = new StringKey("val", "STATS_NB_USER_REVIEWS", q_stats.scenario_play_count);
                    StringKey STATS_AVERAGE_DURATION = new StringKey("val", "STATS_AVERAGE_DURATION", (int)(q_stats.scenario_avg_duration));
                    StringKey STATS_NO_AVERAGE_DURATION = new StringKey("val", "STATS_NO_AVERAGE_DURATION");

                    //  rating
                    string symbol = "★";
                    if (game.gameType is MoMGameType)
                    {
                        symbol = new StringKey("val", "ICON_TENTACLE").Translate();
                    }
                    float rating = q_stats.scenario_avg_rating / 10;
                    float score_text_width = 0;

                    ui = new UIElement(scrollArea.GetScrollTransform());

                    ui.SetText(symbol + symbol + symbol + symbol + symbol, Color.black);
                    score_text_width = ui.GetStringWidth(symbol + symbol + symbol + symbol + symbol, (int)System.Math.Round(UIScaler.GetMediumFont() * 1.4f)) + 1;
                    ui.SetLocation(UIScaler.GetRight(-12f), offset + 0.6f, score_text_width, 2);
                    ui.SetBGColor(Color.clear);
                    ui.SetFontSize((int)System.Math.Round(UIScaler.GetMediumFont() * 1.4f));
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetButton(delegate { Selection(key); });

                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(UIScaler.GetRight(-12) + (rating * (score_text_width - 1)), offset + 0.6f, (1 - rating) * score_text_width, 2);
                    ui.SetBGColor(new Color(1, 1, 1, 0.7f));
                    ui.SetButton(delegate { Selection(key); });

                    //  Number of user reviews
                    float user_review_text_width = 0;
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    user_review_text_width = ui.GetStringWidth(STATS_NB_USER_REVIEWS, UIScaler.GetSmallFont()) + 1;
                    ui.SetText(STATS_NB_USER_REVIEWS, Color.black);
                    ui.SetLocation(UIScaler.GetRight(-12) + (score_text_width / 2) - (user_review_text_width / 2), offset + 2.3f, user_review_text_width, 1);
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetBGColor(Color.clear);
                    ui.SetFontSize(UIScaler.GetSmallFont());
                    ui.SetButton(delegate { Selection(key); });

                    if (q_stats.scenario_avg_duration > 0 || win_ratio >= 0)
                    {
                        // Additional information in Grey frame
                        ui = new UIElement(scrollArea.GetScrollTransform());
                        ui.SetLocation(3.5f + 1f, offset + 3.6f, UIScaler.GetWidthUnits() - 4.9f - 3.5f - 0.05f, 1.2f);
                        ui.SetBGColor(Color.grey);
                        ui.SetButton(delegate { Selection(key); });

                        //  average duration
                        ui = new UIElement(scrollArea.GetScrollTransform());
                        ui.SetLocation(5f, offset + 3.8f, 14, 1);
                        if (q_stats.scenario_avg_duration > 0)
                            ui.SetText(STATS_AVERAGE_DURATION, Color.white);
                        else
                            ui.SetText(STATS_NO_AVERAGE_DURATION, Color.white);
                        ui.SetTextAlignment(TextAnchor.MiddleLeft);
                        ui.SetBGColor(Color.clear);
                        ui.SetButton(delegate { Selection(key); });

                        //  average win ratio
                        ui = new UIElement(scrollArea.GetScrollTransform());
                        ui.SetLocation(UIScaler.GetHCenter() - 5.5f, offset + 3.8f, 15, 1);
                        if (win_ratio >= 0)
                            ui.SetText(STATS_AVERAGE_WIN_RATIO, Color.white);
                        else
                            ui.SetText(STATS_NO_AVERAGE_WIN_RATIO, Color.white);
                        ui.SetBGColor(Color.clear);
                        ui.SetTextAlignment(TextAnchor.MiddleCenter);
                        ui.SetButton(delegate { Selection(key); });

                        offset += 1.2f;
                    }
                }

                foreach (string s in q.GetMissingPacks(game.cd.GetLoadedPackIDs()))
                {
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(3, offset, UIScaler.GetWidthUnits() - 9, 1.2f);
                    ui.SetText(new StringKey("val", "REQUIRES_EXPANSION", game.cd.GetContentName(s)), Color.black);
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetBGColor(new Color(0.4f, 0.4f, 0.4f));
                    offset += 1.2f;
                }

                offset += 4.5f;

            }

            scrollArea.SetScrollSize(offset);

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

            new QuestDetailsScreen(QuestLoader.GetSingleQuest(key));
        }

        public void SetSort(string sort_option)
        {
            questList = Game.Get().questsList.GetList(sort_option);
            DestroyQuestList();
            RefreshQuestList();
        }

    }
}