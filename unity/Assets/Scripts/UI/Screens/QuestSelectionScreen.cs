using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using System.IO;

namespace Assets.Scripts.UI.Screens
{
    // Class for quest selection window
    public class QuestSelectionScreen
    {
        private StringKey DOWNLOAD   = new StringKey("val", "DOWNLOAD");

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
                    if (q.Value.image.Length > 0)
                    {
                        ui.SetImage(ContentData.FileToTexture(Path.Combine(q.Value.path, q.Value.image)));
                    }

                    // Quest name
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetBGColor(Color.clear);
                    ui.SetLocation(5f, offset-0.2f, UIScaler.GetWidthUnits() - 8, 2.5f);
                    ui.SetTextPadding(0.5f);
                    ui.SetText(translation, Color.black);
                    ui.SetButton(delegate { Selection(key); });
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetFontSize(Mathf.RoundToInt(UIScaler.GetSmallFont() * 1.4f));
                    ui.SetFont(game.gameType.GetHeaderFont());

                    // Duration
                    if (q.Value.lengthMax != 0)
                    {
                        ui = new UIElement(scrollArea.GetScrollTransform());
                        ui.SetLocation(6.5f, offset + 2.3f, 4, 1);
                        ui.SetText(new StringKey("val","DURATION"), Color.black);
                        ui.SetButton(delegate { Selection(key); });
                        ui.SetTextAlignment(TextAnchor.MiddleLeft);
                        ui.SetBGColor(Color.clear);

                        ui = new UIElement(scrollArea.GetScrollTransform());
                        //ui.SetLocation(UIScaler.GetRight(-11), offset, 2, 1);
                        ui.SetLocation(10.5f, offset + 2.3f, 5, 1);
                        ui.SetText(q.Value.lengthMin+ "  -  " + q.Value.lengthMax, Color.black);
                        ui.SetButton(delegate { Selection(key); });
                        ui.SetTextAlignment(TextAnchor.MiddleLeft);
                        ui.SetBGColor(Color.clear);
                    }

                    // Difficulty
                    if (q.Value.difficulty != 0)
                    {
                        ui = new UIElement(scrollArea.GetScrollTransform());
                        ui.SetLocation(UIScaler.GetHCenter()-4.5f, offset + 2.3f, 4, 1);
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
                        // ui.SetLocation(UIScaler.GetRight(-13), offset + 1, 9, 2);
                        ui.SetLocation(UIScaler.GetHCenter()-1, offset + 1.8f, 9, 2);
                        ui.SetText(symbol + symbol + symbol + symbol + symbol, Color.black);
                        ui.SetBGColor(Color.clear);
                        ui.SetFontSize(UIScaler.GetMediumFont());
                        ui.SetButton(delegate { Selection(key); });

                        ui = new UIElement(scrollArea.GetScrollTransform());
                        ui.SetLocation(UIScaler.GetHCenter() + 1.05f + (q.Value.difficulty * 6.9f), offset + 1.8f, (1 - q.Value.difficulty) * 6.9f, 1.6f);
                        ui.SetBGColor(new Color(1, 1, 1, 0.7f));
                        ui.SetButton(delegate { Selection(key); });

                   }

                    // Statistics
                    string filename = Path.GetFileName(key).ToLower();
                    Debug.Log("Quest filename: " + filename + "\n");
                    if (game.stats!=null && game.stats.scenarios_stats.ContainsKey(filename))
                    {
                        ScenarioStats q_stats = game.stats.scenarios_stats[filename];
                        int win_ratio = (int)(q_stats.scenario_avg_win_ratio * 100);

                        StringKey STATS_AVERAGE_WIN_RATIO = new StringKey("val", "STATS_AVERAGE_WIN_RATIO", win_ratio);
                        StringKey STATS_NO_AVERAGE_WIN_RATIO = new StringKey("val", "STATS_NO_AVERAGE_WIN_RATIO", win_ratio);
                        StringKey STATS_NB_USER_REVIEWS   = new StringKey("val", "STATS_NB_USER_REVIEWS", q_stats.scenario_play_count);
                        StringKey STATS_AVERAGE_DURATION  = new StringKey("val", "STATS_AVERAGE_DURATION", (int)(q_stats.scenario_avg_duration));
                        StringKey STATS_NO_AVERAGE_DURATION = new StringKey("val", "STATS_NO_AVERAGE_DURATION");

                        //  rating
                        string symbol = "★";
                        float rating = q_stats.scenario_avg_rating / 10;
                        float text_width = 0;

                        ui = new UIElement(scrollArea.GetScrollTransform());
                        ui.SetLocation(UIScaler.GetRight(-11), offset + 0.7f, 7, 2);
                        ui.SetText(symbol + symbol + symbol + symbol + symbol, Color.black);
                        ui.SetBGColor(Color.clear);
                        ui.SetFontSize((int)System.Math.Round(UIScaler.GetMediumFont()*1.2f));
                        ui.SetTextAlignment(TextAnchor.MiddleLeft);
                        ui.SetButton(delegate { Selection(key); });

                        text_width = UIElement.GetStringWidth(symbol + symbol + symbol + symbol + symbol, (int)System.Math.Round(UIScaler.GetMediumFont() * 1.2f));
                        Debug.Log("text_width: " + text_width + "\n");

                        ui = new UIElement(scrollArea.GetScrollTransform());
                        ui.SetLocation(UIScaler.GetRight(-11) + (rating * text_width), offset + 0.7f, (1 - rating) * text_width, 2);
                        ui.SetBGColor(new Color(1, 1, 1, 0.7f));
                        ui.SetButton(delegate { Selection(key); });

                        //  Number of user reviews
                        ui = new UIElement(scrollArea.GetScrollTransform());
                        ui.SetLocation(UIScaler.GetRight(-12), offset + 2.1f, text_width+2, 1);
                        ui.SetText(STATS_NB_USER_REVIEWS, Color.black);
                        ui.SetTextAlignment(TextAnchor.MiddleCenter);
                        ui.SetBGColor(Color.clear);
                        ui.SetFontSize(UIScaler.GetSmallFont());
                        ui.SetButton(delegate { Selection(key); });

                        if (q_stats.scenario_avg_duration > 0 || win_ratio >= 0)
                        {
                            // Additional information in Grey frame
                            ui = new UIElement(scrollArea.GetScrollTransform());
                            ui.SetLocation(3.5f + 1f, offset+3.6f, UIScaler.GetWidthUnits() - 4.9f - 3.5f - 0.05f, 1.2f);
                            ui.SetBGColor(Color.grey);
                            ui.SetButton(delegate { Selection(key); });

                            //  average duration
                            ui = new UIElement(scrollArea.GetScrollTransform());
                            ui.SetLocation(5f, offset + 3.8f, 11, 1);
                            if (q_stats.scenario_avg_duration > 0)
                                ui.SetText(STATS_AVERAGE_DURATION, Color.white);
                            else
                                ui.SetText(STATS_NO_AVERAGE_DURATION, Color.white);
                            ui.SetTextAlignment(TextAnchor.MiddleCenter);
                            ui.SetBGColor(Color.clear);
                            ui.SetButton(delegate { Selection(key); });
 
                            //  average win ratio
                            ui = new UIElement(scrollArea.GetScrollTransform());
                            ui.SetLocation(UIScaler.GetHCenter() - 4.5f, offset + 3.8f, 13, 1);
                            if(win_ratio>=0)
                                ui.SetText(STATS_AVERAGE_WIN_RATIO, Color.white);
                            else
                                ui.SetText(STATS_NO_AVERAGE_WIN_RATIO, Color.white);
                            ui.SetBGColor(Color.clear);
                            ui.SetTextAlignment(TextAnchor.MiddleCenter);
                            ui.SetButton(delegate { Selection(key); });

                            offset += 1.4f;
                        }
                    }

                    offset += 4.5f;
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