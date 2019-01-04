using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using System.Collections;

namespace Assets.Scripts.UI.Screens
{
    // Class for quest selection window
    public class QuestSelectionScreen
    {
        List<string> questList = null;
        UIElement text_number_of_filtered_scenario = null;
        UIElementScrollVertical scrollArea = null;
        UIElement sortOptionsPopup = null;
        UIElement filtersPopup = null;
        UIElement filter_missing_expansions_text = null;

        // Class to handle async images to display
        ImgAsyncLoader images_list = null;


        Game game = null;

        private readonly StringKey SORT_TITLE = new StringKey("val", "SORT_TITLE");
        private readonly StringKey SORT_SELECT_CRITERIA = new StringKey("val", "SORT_SELECT_CRITERIA");
        private readonly StringKey SORT_SELECT_ORDER = new StringKey("val", "SORT_SELECT_ORDER");
        private readonly StringKey SORT_ASCENDING = new StringKey("val", "SORT_ASCENDING");
        private readonly StringKey SORT_DESCENDING = new StringKey("val", "SORT_DESCENDING");

        private readonly StringKey SORT_BY_AUTHOR = new StringKey("val", "SORT_BY_AUTHOR");
        private readonly StringKey SORT_BY_NAME = new StringKey("val", "SORT_BY_NAME");
        private readonly StringKey SORT_BY_DIFFICULTY = new StringKey("val", "SORT_BY_DIFFICULTY");
        private readonly StringKey SORT_BY_DURATION = new StringKey("val", "SORT_BY_DURATION");
        private readonly StringKey SORT_BY_RATING = new StringKey("val", "SORT_BY_RATING");
        private readonly StringKey SORT_BY_AVERAGE_DURATION = new StringKey("val", "SORT_BY_AVERAGE_DURATION");
        private readonly StringKey SORT_BY_WIN_RATIO = new StringKey("val", "SORT_BY_WIN_RATIO");
        private readonly StringKey SORT_BY_DATE = new StringKey("val", "SORT_BY_DATE");

        private readonly StringKey FILTER_TITLE = new StringKey("val", "FILTER_TITLE");
        private readonly StringKey FILTER_SELECT_LANG = new StringKey("val", "FILTER_SELECT_LANG");
        private readonly StringKey FILTER_MISSING_EXPANSIONS_ON = new StringKey("val", "FILTER_MISSING_EXPANSIONS_ON");
        private readonly StringKey FILTER_MISSING_EXPANSIONS_OFF = new StringKey("val", "FILTER_MISSING_EXPANSIONS_OFF");

        private readonly StringKey STATS_NO_AVERAGE_WIN_RATIO = new StringKey("val", "STATS_NO_AVERAGE_WIN_RATIO");
        private readonly StringKey STATS_NO_AVERAGE_DURATION = new StringKey("val", "STATS_NO_AVERAGE_DURATION");

        // filters
        string[] langs = "English,Spanish,French,German,Italian,Portuguese,Polish,Japanese,Chinese,Czech".Split(',');
        Dictionary<string, bool> langs_selected = null;
        bool filter_missing_expansions = false;

        // sort options
        string sort_criteria = "rating";
        string sort_order = "descending";

        public QuestSelectionScreen()
        {
            game = Game.Get();

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

            // Initialize filters
            langs_selected = new Dictionary<string, bool>();
            foreach (string lang in langs)
            {
                // initialize dict
                langs_selected.Add(lang, true);
            }
            langs_selected["Japanese"] = false;
            langs_selected["Czech"] = false;

            // initialize text indicator for filtered scenario
            text_number_of_filtered_scenario = new UIElement();
            text_number_of_filtered_scenario.SetLocation(1, 3.6f, 20, 1.2f);
            text_number_of_filtered_scenario.SetText(" ");
            text_number_of_filtered_scenario.SetTextAlignment(TextAnchor.MiddleLeft);
            text_number_of_filtered_scenario.SetFont(Game.Get().gameType.GetHeaderFont());
            text_number_of_filtered_scenario.SetFontSize(UIScaler.GetSmallFont());

            // Show filter button
            ui = new UIElement();
            Texture2D filterTex = null;
            filterTex = Resources.Load("sprites/filter") as Texture2D;
            ui.SetLocation(UIScaler.GetWidthUnits() - 1f - 1.5f - 1.5f, 3.5f, 1.5f, 1.5f);
            ui.SetImage(filterTex);
            ui.SetButton(delegate { FilterPopup(); });
            new UIElementBorder(ui);

            // Show sort button
            ui = new UIElement();
            Texture2D sortTex = null;
            sortTex = Resources.Load("sprites/sort") as Texture2D;
            ui.SetLocation(UIScaler.GetWidthUnits() - 1f - 1.5f, 3.5f, 1.5f, 1.5f);
            ui.SetImage(sortTex);
            ui.SetButton(delegate { SortByPopup(); });
            new UIElementBorder(ui);

            // check if connected on internet, and display scenario list accordingly (local or online)
            if (game.questsList.download_done)
            {
                sort_criteria = "rating";
                sort_order = "descending";
            }
            else
            {
                // Display offline message
                ui = new UIElement();
                ui.SetLocation(UIScaler.GetWidthUnits() - 10, 1f, 8, 1.2f);
                ui.SetText("OFFLINE", Color.red);
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetTextAlignment(TextAnchor.MiddleRight);

                // Get and load a list of all locally available quests
                game.questsList.loadAllLocalQuests();
                sort_criteria = "name";
                sort_order = "ascending";
            }

            // Get sorted list
            questList = game.questsList.GetList(sort_criteria);
            if (sort_order == "descending")
                questList.Reverse();

            // scroll area
            scrollArea = new UIElementScrollVertical(Game.QUESTLIST);
            scrollArea.SetLocation(1, 5, UIScaler.GetWidthUnits() - 2f, UIScaler.GetHeightUnits() - 6f);
            new UIElementBorder(scrollArea);

            // List of images for asynchronous loading
            images_list = new ImgAsyncLoader();

            DrawQuestList();
        }


        // Show button and initialize the popup
        private void FilterPopup()
        {
            if (sortOptionsPopup != null || filtersPopup != null)
                return;

            // popup background
            filtersPopup = new UIElement();
            filtersPopup.SetLocation(UIScaler.GetHCenter(-21), 3, 42, 24);
            filtersPopup.SetBGColor(Color.black);
            new UIElementBorder(filtersPopup);

            // Title
            UIElement ui = new UIElement(filtersPopup.GetTransform());
            ui.SetLocation(11, 2, 20, 3);
            ui.SetText(FILTER_TITLE);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetTextAlignment(TextAnchor.MiddleCenter);
            ui.SetFontSize(UIScaler.GetLargeFont());

            // List of flags
            ui = new UIElement(filtersPopup.GetTransform());
            ui.SetLocation(6, 9, 30, 2);
            ui.SetText(FILTER_SELECT_LANG);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());

            DrawFlags();

            // Missing expansions checkbox
            filter_missing_expansions_text = new UIElement(filtersPopup.GetTransform());
            filter_missing_expansions_text.SetLocation(2, 15, 38, 2);
            if (filter_missing_expansions)
                filter_missing_expansions_text.SetText(FILTER_MISSING_EXPANSIONS_ON);
            else
                filter_missing_expansions_text.SetText(FILTER_MISSING_EXPANSIONS_OFF);
            filter_missing_expansions_text.SetFont(game.gameType.GetHeaderFont());
            filter_missing_expansions_text.SetFontSize(UIScaler.GetMediumFont());
            filter_missing_expansions_text.SetButton(delegate { FilterMissingExpansions(); });

            // OK button closes popup and refresh quest list
            ui = new UIElement(filtersPopup.GetTransform());
            ui.SetLocation(18, 20, 6, 2);
            ui.SetText(CommonStringKeys.OK);
            ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(delegate { filtersPopup.Destroy(); filtersPopup = null; ReloadQuestList(); });
            new UIElementBorder(ui);
        }

        private void FilterMissingExpansions()
        {
            if (filter_missing_expansions)
            {
                filter_missing_expansions = false;
                filter_missing_expansions_text.SetText(FILTER_MISSING_EXPANSIONS_OFF);
            }
            else
            {
                filter_missing_expansions = true;
                filter_missing_expansions_text.SetText(FILTER_MISSING_EXPANSIONS_ON);
            }
        }


        private void DrawFlags()
        {
            float x_offset = 11f;
            float y_offset = 11f;
            const float flag_size = 1.7f;

            foreach (KeyValuePair<string, bool> k in langs_selected)
            {
                string lang = k.Key;
                bool activated = k.Value;

                UIElement ui = null;
                ui = new UIElement(filtersPopup.GetTransform());
                Texture2D flagTex = null;
                flagTex = Resources.Load("sprites/flags/" + lang) as Texture2D;
                ui.SetLocation(x_offset, y_offset, flag_size, flag_size);
                ui.SetImage(flagTex);
                ui.SetButton(delegate { FilterLang(lang); });

                if (!activated)
                {
                    ui = new UIElement(filtersPopup.GetTransform());
                    ui.SetLocation(x_offset, y_offset, flag_size, flag_size);
                    ui.SetBGColor(new Color(0, 0, 0, 0.8f));
                    ui.SetButton(delegate { FilterLang(lang); });
                }

                x_offset += flag_size + 0.3f;
            }
        }

        private void FilterLang(string lang)
        {
            langs_selected[lang] = !langs_selected[lang];
            // lazy : display on top
            DrawFlags();
        }

        // Initialize the popup
        private void SortByPopup()
        {
            if (sortOptionsPopup != null || filtersPopup != null)
                return;

            // popup background
            sortOptionsPopup = new UIElement();
            sortOptionsPopup.SetLocation(UIScaler.GetHCenter(-21), 3, 42, 24);
            sortOptionsPopup.SetBGColor(Color.black);
            new UIElementBorder(sortOptionsPopup);

            // Title
            UIElement ui = new UIElement(sortOptionsPopup.GetTransform());
            ui.SetLocation(11, 1.5f, 20, 3);
            ui.SetText(SORT_TITLE);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetTextAlignment(TextAnchor.MiddleCenter);
            ui.SetFontSize(UIScaler.GetLargeFont());

            // Show sort options
            ui = new UIElement(sortOptionsPopup.GetTransform());
            ui.SetLocation(6, 6, 30, 2);
            ui.SetText(SORT_SELECT_CRITERIA);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetTextAlignment(TextAnchor.MiddleCenter);
            ui.SetFontSize(UIScaler.GetMediumFont());

            DrawSortCriteriaButtons();

            // Show sort options
            ui = new UIElement(sortOptionsPopup.GetTransform());
            ui.SetLocation(6, 14, 30, 2);
            ui.SetText(SORT_SELECT_ORDER);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetTextAlignment(TextAnchor.MiddleCenter);
            ui.SetFontSize(UIScaler.GetMediumFont());

            DrawSortOrderButtons();

            // OK button closes popup and refresh quest list
            ui = new UIElement(sortOptionsPopup.GetTransform());
            ui.SetLocation(18, 21, 6, 2);
            ui.SetText(CommonStringKeys.OK);
            ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(delegate { sortOptionsPopup.Destroy(); sortOptionsPopup = null; ReloadQuestList(); });
            new UIElementBorder(ui);
        }

        //sorted_by_rating
        //sorted_by_name
        //sorted_by_difficulty
        //sorted_by_duration
        //sorted_by_date
        public class SortOption
        {
            public string name;
            public StringKey button_text;

            public SortOption(string p_name, StringKey p_button_text)
            {
                name = p_name;
                button_text = p_button_text;
            }
        }


        private void DrawSortCriteriaButtons()
        {
            const float button_size = 9f;
            const float space_between_buttons = 0.8f;
            float x_offset = 1.5f;
            float y_offset = 8.2f;

            List<SortOption> sort_options_offline = new List<SortOption>();
            sort_options_offline.Add(new SortOption("author", SORT_BY_AUTHOR));
            sort_options_offline.Add(new SortOption("name", SORT_BY_NAME));
            sort_options_offline.Add(new SortOption("difficulty", SORT_BY_DIFFICULTY));
            sort_options_offline.Add(new SortOption("duration", SORT_BY_DURATION));

            List<SortOption> sort_options_online = new List<SortOption>();
            sort_options_online.Add(new SortOption("rating", SORT_BY_RATING));
            sort_options_online.Add(new SortOption("date", SORT_BY_DATE));
            sort_options_online.Add(new SortOption("average_win_ratio", SORT_BY_WIN_RATIO));
            sort_options_online.Add(new SortOption("average_duration", SORT_BY_AVERAGE_DURATION));

            // sort type
            UIElement ui = null;

            Color button_color = Color.grey;

            foreach (SortOption s in sort_options_offline)
            {
                if (s.name == sort_criteria)
                    button_color = Color.white;

                // local var required as button is called later with this value
                string local_name = s.name;

                ui = new UIElement(sortOptionsPopup.GetTransform());
                ui.SetLocation(x_offset, y_offset, button_size, 2f);
                ui.SetText(s.button_text, button_color);
                ui.SetFont(Game.Get().gameType.GetHeaderFont());
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetButton(delegate { SetSort(local_name); });
                new UIElementBorder(ui, button_color);

                x_offset += button_size + space_between_buttons;
                button_color = Color.grey;
            }

            y_offset += 2.5f;
            x_offset = 1.5f;

            foreach (SortOption s in sort_options_online)
            {
                if (s.name == sort_criteria)
                    button_color = Color.white;
                if (!game.questsList.download_done)
                    button_color = Color.red;

                // local var required as button is called later with this value
                string local_name = s.name;

                ui = new UIElement(sortOptionsPopup.GetTransform());
                ui.SetLocation(x_offset, y_offset, button_size, 2f);
                ui.SetText(s.button_text, button_color);
                ui.SetFont(Game.Get().gameType.GetHeaderFont());
                ui.SetFontSize(UIScaler.GetMediumFont());
                if (game.questsList.download_done)
                    ui.SetButton(delegate { SetSort(local_name); });
                new UIElementBorder(ui, button_color);

                x_offset += button_size + space_between_buttons;
                button_color = Color.grey;
            }
        }


        private void DrawSortOrderButtons()
        {
            const float button_size = 9f;
            const float space_between_buttons = 1f;
            float x_offset = 12f;
            float y_offset = 16f;
            Color ascending_color = Color.white;
            Color descending_color = Color.white;

            if (sort_order == "ascending")
                descending_color = Color.grey;
            else
                ascending_color = Color.grey;

            // sort order
            UIElement ui = new UIElement(sortOptionsPopup.GetTransform());
            ui.SetLocation(x_offset, y_offset, button_size, 2f);
            ui.SetText(SORT_ASCENDING, ascending_color);
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(delegate { sort_order = "ascending"; DrawSortOrderButtons(); SetSort(sort_criteria); });
            new UIElementBorder(ui, ascending_color);

            x_offset += button_size + space_between_buttons;

            ui = new UIElement(sortOptionsPopup.GetTransform());
            ui.SetLocation(x_offset, y_offset, button_size, 2f);
            ui.SetText(SORT_DESCENDING, descending_color);
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(delegate { sort_order = "descending"; DrawSortOrderButtons(); SetSort(sort_criteria); });
            new UIElementBorder(ui, descending_color);
        }

        public void SetSort(string sort_selected_option)
        {
            sort_criteria = sort_selected_option;

            questList = Game.Get().questsList.GetList(sort_selected_option);
            if (sort_order == "descending")
                questList.Reverse();

            DrawSortCriteriaButtons();
        }

        public void ReloadQuestList()
        {
            CleanQuestList();
            DrawQuestList();
        }

        public void CleanQuestList()
        {
            // Clean up everything marked as 'questlist'
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.QUESTLIST))
                Object.Destroy(go);

            // scroll area
            scrollArea = new UIElementScrollVertical(Game.QUESTLIST);
            scrollArea.SetLocation(1, 5, UIScaler.GetWidthUnits() - 2f, UIScaler.GetHeightUnits() - 6f);
            new UIElementBorder(scrollArea);

            // quest images
            images_list.Clear();
        }

        // check if the quest proposes at least one selected language
        public bool HasSelectedLanguage(QuestData.Quest q)
        {
            foreach (KeyValuePair<string, bool> lang in langs_selected)
            {
                // check if lang is selected in filters
                if (!lang.Value)
                    continue;

                if (!game.questsList.download_done)
                {
                    // check list of languages when offline
                    if (q.localizationDict == null)
                    {
                        Debug.Log("Scenario " + q.package_url + " does not have dictionary, this should not happen");
                        return false;
                    }
                    if (q.localizationDict.SerializeMultiple() == null)
                    {
                        Debug.Log("Scenario " + q.package_url + " does not have any languages, this should not happen");
                        return false;
                    }

                    if (q.localizationDict.SerializeMultiple().ContainsKey(lang.Key))
                    {
                        return true;
                    }

                }
                else
                {
                    // check list of languages when online
                    if (q == null || q.languages_name == null)
                    {
                        Debug.Log("Scenario " + q.package_url + " does not have a name, this should not happen");
                        return false;
                    }

                    if (q.languages_name.ContainsKey(lang.Key))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void DrawQuestList()
        {
            UIElement ui = null;

            // Start here
            float offset = 0;
            int nb_filtered_out_quest = 0;

            // Loop through all available quests
            foreach (string key in questList)
            {
                QuestData.Quest q = game.questsList.getQuestData(key);
                UIElement frame = null;

                // Filter langs
                if (!HasSelectedLanguage(q))
                {
                    nb_filtered_out_quest++;
                    continue;
                }

                // Filter packages
                if (filter_missing_expansions && q.GetMissingPacks(game.cd.GetLoadedPackIDs()).Count != 0)
                {
                    nb_filtered_out_quest++;
                    continue;
                }

                // Statistics data preparation for this quest
                string filename = key.ToLower() + ".valkyrie";
                bool has_stats = (game.stats != null && game.stats.scenarios_stats != null && game.stats.scenarios_stats.ContainsKey(filename));
                int stats_play_count = 0;
                int stats_win_ratio = 0;
                int stats_avg_duration = 0;
                float stats_rating = 0f;
                if (has_stats)
                {
                    ScenarioStats q_stats = game.stats.scenarios_stats[filename];
                    stats_play_count = q_stats.scenario_play_count;
                    stats_win_ratio = (int)(q_stats.scenario_avg_win_ratio * 100);
                    stats_avg_duration = (int)(q_stats.scenario_avg_duration);
                    stats_rating = q_stats.scenario_avg_rating / 10;
                }

                // Frame
                frame = new UIElement(scrollArea.GetScrollTransform());
                frame.SetLocation(0.3f, offset, UIScaler.GetWidthUnits() - 3.7f, 6.2f);
                frame.SetBGColor(Color.white);
                frame.SetButton(delegate { Selection(key); });
                offset += 0.05f;
                new UIElementBorder(frame, Color.grey);

                // prepare list of Images
                if (q.image.Length > 0)
                {
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(0.6f, offset + 0.3f, 4.5f, 4.5f);
                    ui.SetBGColor(Color.white);
                    ui.SetButton(delegate { Selection(key); });
                    images_list.Add(q.package_url + q.image, ui);
                }

                // languages flags
                Texture2D flagTex = null;
                const float flag_size = 0.9f;
                float flag_x_offset = (UIScaler.GetWidthUnits() - 4f) - q.languages_name.Count*(flag_size+0.2f); // align right
                float flag_y_offset = offset + 0.15f;
                foreach (KeyValuePair<string, string> lang_name in q.languages_name)
                {
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    flagTex = Resources.Load("sprites/flags/" + lang_name.Key) as Texture2D;
                    ui.SetLocation(flag_x_offset, flag_y_offset, flag_size, flag_size);
                    ui.SetImage(flagTex);
                    flag_x_offset += flag_size + 0.2f;
                }

                // Required expansions
                List<string> missing_packs = q.GetMissingPacks(game.cd.GetLoadedPackIDs());
                Color expansion_text_color = Color.black;
                float expansion_x_offset = 0.4f;
                float expansion_y_offset = offset + 5.1f;
                List<string> expansion_symbols = new List<string>();
                foreach (string pack in q.packs)
                {
                    string pack_symbol = game.cd.packSymbol[pack].Translate();
                    if (pack_symbol != "" && !expansion_symbols.Contains(pack_symbol))
                    {
                        expansion_symbols.Add(pack_symbol);
                        if (missing_packs.Contains(pack))
                            expansion_text_color = Color.red;
                        else
                            expansion_text_color = Color.black;
                        ui = new UIElement(scrollArea.GetScrollTransform());
                        float symbol_width = ui.GetStringWidth(pack_symbol);
                        ui.SetLocation(expansion_x_offset, expansion_y_offset, symbol_width, 1);
                        ui.SetText(pack_symbol, expansion_text_color);
                        ui.SetBGColor(Color.clear);
                        expansion_x_offset += symbol_width - 0.25f;
                    }
                }

                string name_translation = "";
                string synopsys_translation = "";
                if (game.questsList.download_done)
                {
                    // quest name is local language, or default language
                    if (q.languages_name != null && !q.languages_name.TryGetValue(game.currentLang, out name_translation))
                    {
                        name_translation = q.languages_name[q.defaultLanguage];
                    }
                    // same thing for synopsys: local language, or default language
                    if (q.languages_synopsys != null && !q.languages_synopsys.TryGetValue(game.currentLang, out synopsys_translation))
                    {
                        synopsys_translation = q.languages_synopsys[q.defaultLanguage];
                    }
                }
                else
                {
                    LocalizationRead.AddDictionary("qst", q.localizationDict);
                    name_translation = q.name.Translate();
                    LocalizationRead.AddDictionary("qst", q.localizationDict);
                    synopsys_translation = q.synopsys.Translate();
                }

                // Quest name
                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetBGColor(Color.clear);
                ui.SetLocation(5f, offset + 0.1f, UIScaler.GetWidthUnits() - 8, 1.5f);
                ui.SetTextPadding(0.5f);
                ui.SetText(name_translation, Color.black);
                ui.SetButton(delegate { Selection(key); });
                ui.SetTextAlignment(TextAnchor.MiddleLeft);
                ui.SetFontSize(Mathf.RoundToInt(UIScaler.GetSmallFont() * 1.4f));
                ui.SetFont(game.gameType.GetHeaderFont());

                // Synopsys TODO remove
                synopsys_translation = "The Gordon family has contacted you to find their daughter, Felicia. " +
                    "The last clue is an appointment in her personal diary: 22.30h at the bird shop";

                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetBGColor(Color.clear);
                ui.SetLocation(5f, offset + 1.6f, UIScaler.GetRight(-11f) - 5, 2f);
                ui.SetTextPadding(0.5f);
                ui.SetText(synopsys_translation, Color.black);
                ui.SetButton(delegate { Selection(key); });
                ui.SetTextAlignment(TextAnchor.MiddleLeft);
                ui.SetFontSize(Mathf.RoundToInt(UIScaler.GetSmallFont() * 0.85f));
                ui.SetFontStyle(FontStyle.Italic);
                ui.SetFont(game.gameType.GetHeaderFont());

                float top_text_y = offset + 4.1f;

                // Duration
                if (q.lengthMax != 0)
                {
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(7f, top_text_y, 4, 1);
                    ui.SetText(new StringKey("val", "DURATION"), Color.black);
                    ui.SetButton(delegate { Selection(key); });
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetBGColor(Color.clear);

                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(11f, top_text_y, 5, 1);
                    ui.SetText(q.lengthMin + "  -  " + q.lengthMax, Color.black);
                    ui.SetButton(delegate { Selection(key); });
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetBGColor(Color.clear);
                }

                //  average duration
                if (has_stats)
                {
                    StringKey STATS_AVERAGE_DURATION = new StringKey("val", "STATS_AVERAGE_DURATION", stats_avg_duration);
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(7, top_text_y + 1, 15, 1);
                    if (stats_avg_duration > 0)
                        ui.SetText(STATS_AVERAGE_DURATION, Color.black);
                    else
                        ui.SetText(STATS_NO_AVERAGE_DURATION, Color.black);
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetBGColor(Color.clear);
                    ui.SetButton(delegate { Selection(key); });
                }

                // Difficulty
                if (q.difficulty != 0)
                {
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(UIScaler.GetHCenter() - 5.5f, top_text_y, 6, 1);
                    ui.SetText(new StringKey("val", "DIFFICULTY"), Color.black);
                    ui.SetButton(delegate { Selection(key); });
                    ui.SetTextAlignment(TextAnchor.LowerLeft);
                    ui.SetFontSize(UIScaler.GetSmallFont());
                    ui.SetBGColor(Color.clear);

                    string difficulty_symbol = "π"; // will
                    if (game.gameType is MoMGameType)
                    {
                        difficulty_symbol = new StringKey("val", "ICON_SUCCESS_RESULT").Translate();
                    }
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(UIScaler.GetHCenter(), top_text_y, 9, 1);
                    ui.SetText(difficulty_symbol + difficulty_symbol + difficulty_symbol + difficulty_symbol + difficulty_symbol, Color.black);
                    ui.SetTextAlignment(TextAnchor.LowerLeft);
                    ui.SetBGColor(Color.clear);
                    ui.SetFontSize(UIScaler.GetSmallFont());
                    ui.SetButton(delegate { Selection(key); });

                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(UIScaler.GetHCenter() + 1.05f + (q.difficulty * 6.9f), top_text_y-0.1f, (1 - q.difficulty) * 6.9f, 1.2f);
                    ui.SetBGColor(new Color(1, 1, 1, 0.7f));
                    ui.SetButton(delegate { Selection(key); });
                }

                //  average win ratio
                if (has_stats)
                {
                    StringKey STATS_AVERAGE_WIN_RATIO = new StringKey("val", "STATS_AVERAGE_WIN_RATIO", stats_win_ratio);
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(UIScaler.GetHCenter() - 5.5f, top_text_y + 1f, 15, 1);
                    if (stats_win_ratio >= 0)
                        ui.SetText(STATS_AVERAGE_WIN_RATIO, Color.black);
                    else
                        ui.SetText(STATS_NO_AVERAGE_WIN_RATIO, Color.black);
                    ui.SetBGColor(Color.clear);
                    ui.SetTextAlignment(TextAnchor.LowerLeft);
                    ui.SetButton(delegate { Selection(key); });
                }

                //  rating
                if (has_stats)
                {
                    StringKey STATS_NB_USER_REVIEWS = new StringKey("val", "STATS_NB_USER_REVIEWS", stats_play_count);
                    string rating_symbol = "★";
                    if (game.gameType is MoMGameType)
                    {
                        rating_symbol = new StringKey("val", "ICON_TENTACLE").Translate();
                    }
                    float score_text_width = 0;

                    ui = new UIElement(scrollArea.GetScrollTransform());

                    ui.SetText(rating_symbol + rating_symbol + rating_symbol + rating_symbol + rating_symbol, Color.black);
                    score_text_width = ui.GetStringWidth(rating_symbol + rating_symbol + rating_symbol + rating_symbol + rating_symbol, (int)System.Math.Round(UIScaler.GetMediumFont() * 1.2f)) + 1;
                    ui.SetLocation(UIScaler.GetRight(-9f), top_text_y + 0.5f, score_text_width, 1.5f);
                    ui.SetBGColor(Color.clear);
                    ui.SetFontSize((int)System.Math.Round(UIScaler.GetMediumFont() * 1.2f));
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetButton(delegate { Selection(key); });

                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(UIScaler.GetRight(-9) + (stats_rating * (score_text_width - 1)), top_text_y + 0.5f, (1 - stats_rating) * score_text_width, 1.5f);
                    ui.SetBGColor(new Color(1, 1, 1, 0.7f));
                    ui.SetButton(delegate { Selection(key); });

                    //  Number of user reviews
                    float user_review_text_width = 0;
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    user_review_text_width = ui.GetStringWidth(STATS_NB_USER_REVIEWS, UIScaler.GetSmallFont()) + 1;
                    ui.SetText(STATS_NB_USER_REVIEWS, Color.black);
                    ui.SetLocation(UIScaler.GetRight(-9) - (user_review_text_width), top_text_y + 0.7f, user_review_text_width, 1);
                    ui.SetTextAlignment(TextAnchor.MiddleRight);
                    ui.SetBGColor(Color.clear);
                    ui.SetFontSize(UIScaler.GetSmallFont());
                    ui.SetButton(delegate { Selection(key); });
                }

                offset += 7.1f;
            }

            scrollArea.SetScrollSize(offset);

            if (nb_filtered_out_quest > 0)
            {
                StringKey FILTER_TEXT_NUMBER_OF_FILTERED_SCENARIO = new StringKey("val", "FILTER_TEXT_NUMBER_OF_FILTERED_SCENARIO", nb_filtered_out_quest);
                text_number_of_filtered_scenario.SetText(FILTER_TEXT_NUMBER_OF_FILTERED_SCENARIO);
            }

            images_list.StartDownloadASync();

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

    }


    class ImgAsyncLoader
    {
        private Dictionary<string, UIElement> images_list = null;

        public ImgAsyncLoader()
        {
            images_list = new Dictionary<string, UIElement>();
        }

        public void Add(string url, UIElement uie)
        {
            images_list.Add(url, uie);
        }

        public void Clear()
        {
            images_list.Clear();
        }

        public void StartDownloadASync()
        {
            if(images_list.Count>0)
            { 
                foreach (KeyValuePair<string, UIElement> kv in images_list)
                {
                    HTTPManager.GetImage(kv.Key, ImageDownloaded_callback);
                }
            }
        }

        /// <summary>
        /// Parse the downloaded remote manifest and start download of individual quest files
        /// </summary>
        public void ImageDownloaded_callback(Texture2D texture, bool error, System.Uri uri)
        {
            if (error)
            {
                Debug.Log("Error downloading picture : " + uri.ToString());
            }
            else
            {
                images_list[uri.ToString()].SetImage(texture);
            }
        }
    }
}