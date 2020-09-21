using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using System.IO;
using System.Collections;
using ValkyrieTools;

namespace Assets.Scripts.UI.Screens
{
    // Class for quest selection window
    public class QuestSelectionScreen: MonoBehaviour
    {
        // List of Quest.QuestData to display (either local or remote)
        List<string> questList = null;

        // Persistent UI Element
        UIElement text_connection_status = null;
        UIElement text_number_of_filtered_scenario = null;
        UIElementScrollVertical scrollArea = null;
        UIElement sortOptionsPopup = null;
        UIElement filtersPopup = null;
        UIElement filter_missing_expansions_text = null;

        // Class to handle async images to display
        ImgAsyncLoader images_list = null;

        // Display coroutine
        Coroutine co_display=null;

        Game game = null;

        // texts
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

        private readonly StringKey MISSING_EXPANSIONS = new StringKey("val", "MISSING_EXPANSIONS");

        private readonly StringKey STATS_NO_AVERAGE_WIN_RATIO = new StringKey("val", "STATS_NO_AVERAGE_WIN_RATIO");
        private readonly StringKey STATS_NO_AVERAGE_DURATION = new StringKey("val", "STATS_NO_AVERAGE_DURATION");

        private readonly StringKey GO_OFFLINE = new StringKey("val", "GO_OFFLINE");
        private readonly StringKey GO_ONLINE = new StringKey("val", "GO_ONLINE");
        private readonly StringKey DOWNLOAD_ONGOING = new StringKey("val", "DOWNLOAD_ONGOING");
        private readonly StringKey OFFLINE_DUE_TO_ERROR = new StringKey("val", "OFFLINE_DUE_TO_ERROR");

            
        // text colors
        private readonly Color grey_transparent_text_color = new Color(0.1f, 0.1f, 0.1f, 0.50f);
        private readonly Color dark_grey_text_color = new Color(0.1f, 0.1f, 0.1f);
//        private readonly Color dark_red_text_color = new Color(0.686f, 0.031f, 0.023f);
        private readonly Color dark_red_text_color = new Color(0.686f, 0.031f, 0.023f);

        // filters
        string[] langs = "English,Spanish,French,German,Italian,Portuguese,Polish,Russian,Chinese,Czech,Korean".Split(',');
        Dictionary<string, bool> langs_selected = null;
        bool filter_missing_expansions = false;

        // sort options

        List<SortOption> sort_options_offline = null;
        List<SortOption> sort_options_online = null;

        string sort_criteria = "rating";
        string sort_order = "descending";
        string last_author = "";
        StringKey last_update_info = null;
        SortedDictionary<float, StringKey> nbDays_durationText = null;

        // textures
        Texture2D scroll_paper = null;
        Texture2D picture_shadow = null;
        Texture2D picture_pin = null;
        Texture2D button_download = null;
        Texture2D button_update = null;
        Texture2D button_play = null;
        Texture2D button_no_entry = null;
        Texture2D button_hole = null;

        public void Start()
        {
            game = Game.Get();

            // initialize sort information
            nbDays_durationText = new SortedDictionary<float, StringKey>();
            nbDays_durationText.Add(7, new StringKey("val", "UPDATED_THIS_WEEK"));
            nbDays_durationText.Add(30, new StringKey("val", "UPDATED_THIS_MONTH"));
            nbDays_durationText.Add(90, new StringKey("val", "UPDATED_THIS_TRIMESTER"));
            nbDays_durationText.Add(180, new StringKey("val", "UPDATED_THIS_SEMESTER"));
            nbDays_durationText.Add(356, new StringKey("val", "UPDATED_THIS_YEAR"));
            nbDays_durationText.Add(700, new StringKey("val", "UPDATED_TWO_YEARS_AGO"));
            nbDays_durationText.Add(999999, new StringKey("val", "UPDATE_OLDER_THAN_TWO_YEAR"));

            sort_options_offline = new List<SortOption>();
            sort_options_offline.Add(new SortOption("author", SORT_BY_AUTHOR));
            sort_options_offline.Add(new SortOption("name", SORT_BY_NAME));
            sort_options_offline.Add(new SortOption("difficulty", SORT_BY_DIFFICULTY));
            sort_options_offline.Add(new SortOption("duration", SORT_BY_DURATION));
            sort_options_online = new List<SortOption>();
            sort_options_online.Add(new SortOption("rating", SORT_BY_RATING));
            sort_options_online.Add(new SortOption("date", SORT_BY_DATE));
            sort_options_online.Add(new SortOption("average_win_ratio", SORT_BY_WIN_RATIO));
            sort_options_online.Add(new SortOption("average_duration", SORT_BY_AVERAGE_DURATION));

            // Initialize list of images for asynchronous loading
            images_list = new ImgAsyncLoader(this);

            // preload textures
            scroll_paper = Resources.Load("sprites/scenario_list/scroll_paper") as Texture2D;
            picture_shadow = Resources.Load("sprites/scenario_list/picture_shadow") as Texture2D;
            picture_pin = Resources.Load("sprites/scenario_list/picture_pin") as Texture2D;
            button_hole = Resources.Load("sprites/scenario_list/button_hole") as Texture2D;
            button_download = Resources.Load("sprites/scenario_list/button_download") as Texture2D;
            button_update = Resources.Load("sprites/scenario_list/button_update") as Texture2D;
            button_play = Resources.Load("sprites/scenario_list/button_play") as Texture2D;
            button_no_entry = Resources.Load("sprites/scenario_list/button_no_entry") as Texture2D;

            Show();
        }

        public void Show()
        {
            // If a dialog window is open we force it closed (this shouldn't happen)
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
                Object.Destroy(go);

            // Clean up downloader if present
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.QUESTUI))
                Object.Destroy(go);

            // Get all user configuration
            Dictionary<string, string> config_values = game.config.data.Get("UserConfig");

            // initalize list of langs
            langs_selected = new Dictionary<string, bool>();
            foreach (string lang in langs)
            {
                // initialize dict
                langs_selected.Add(lang, false);
            }

            // apply lang configuration
            if (config_values.ContainsKey("filterLangs"))
            {
                // saved configuration
                foreach (string lang in config_values["filterLangs"].Split(';'))
                {
                    if (langs_selected.ContainsKey(lang))
                        langs_selected[lang] = true;
                }
            }
            else
            {
                // default configuration
                foreach (string lang in langs)
                {
                    // initialize dict
                    langs_selected[lang] = true;
                }
                langs_selected["Japanese"] = false;
            }

            if (config_values.ContainsKey("filterMissingExpansions"))
            {
                filter_missing_expansions = false;
                bool.TryParse(config_values["filterMissingExpansions"], out filter_missing_expansions);
            }

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

            // Show offline/online button (or info)
            DrawOnlineModeButton();

            if (co_display!=null)
                StopCoroutine(co_display);
            co_display = StartCoroutine(DrawQuestList());
        }

        private void RemoteQuestsListDownload_cb(bool is_available)
        {
            DrawOnlineModeButton();
        }

        private void SetOnlineMode(bool go_online)
        {
            if (go_online)
            {
                ValkyrieDebug.Log("INFO: Set online mode for quests");

                game.questsList.SetMode(QuestsManager.QuestListMode.ONLINE);
            }
            else
            {
                ValkyrieDebug.Log("INFO: Set offline mode for quests");
                game.questsList.SetMode(QuestsManager.QuestListMode.LOCAL);
            }

            DrawOnlineModeButton();
            ReloadQuestList();
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
            filter_missing_expansions_text.SetButton(delegate { SetFilterMissingExpansions(); });

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

        private void SetFilterMissingExpansions()
        {
            // save filter configuration
            game.config.data.Add("UserConfig", "filterMissingExpansions", (!filter_missing_expansions).ToString());
            game.config.Save();

            // update configuration
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

        private void DrawOnlineModeButton()
        {
            float text_width = 0f;
            bool border = false;

            if (text_connection_status != null)
                text_connection_status.Destroy();

            text_connection_status = new UIElement();

            // Display connection status message
            if (game.questsList.quest_list_mode == QuestsManager.QuestListMode.ERROR_DOWNLOAD)
            {
                // error download (no connection, timeout, of file not available)
                text_connection_status.SetText(OFFLINE_DUE_TO_ERROR, Color.red);
            }
            else if (game.questsList.quest_list_mode == QuestsManager.QuestListMode.DOWNLOADING)
            {
                // Download ongoing
                text_connection_status.SetText(DOWNLOAD_ONGOING, Color.cyan);
                game.questsList.Register_cb_download(RemoteQuestsListDownload_cb);
            }
            else if (game.questsList.quest_list_mode == QuestsManager.QuestListMode.ONLINE)
            {
                // Download done, we are online
                text_connection_status.SetText(GO_OFFLINE, Color.red);
                text_connection_status.SetButton(delegate { SetOnlineMode(false); });
                border = true;
            }
            else
            {
                // Download done, user has switched offline modline
                text_connection_status.SetText(GO_ONLINE, Color.green);
                text_connection_status.SetButton(delegate { SetOnlineMode(true); });
                border = true;
            }

            text_width = text_connection_status.GetStringWidth(text_connection_status.GetText(), UIScaler.GetSmallFont(), game.gameType.GetHeaderFont());
            text_connection_status.SetLocation(UIScaler.GetWidthUnits() - text_width - 1, 0.5f, text_width, 1.2f);
            text_connection_status.SetFont(game.gameType.GetHeaderFont());
            text_connection_status.SetFontSize(UIScaler.GetSmallFont());
            text_connection_status.SetTextAlignment(TextAnchor.MiddleCenter);
            if(border)
                new UIElementBorder(text_connection_status, text_connection_status.GetTextColor());

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
                ui.SetButton(delegate { SetFilterLang(lang); });

                if (!activated)
                {
                    ui = new UIElement(filtersPopup.GetTransform());
                    ui.SetLocation(x_offset, y_offset, flag_size, flag_size);
                    ui.SetBGColor(new Color(0, 0, 0, 0.8f));
                    ui.SetButton(delegate { SetFilterLang(lang); });
                }

                x_offset += flag_size + 0.3f;
            }
        }

        public void DrawScenarioPicture(Texture2D texture, UIElement ui_picture_shadow)
        {
            float width_heigth = ui_picture_shadow.GetRectTransform().rect.width / UIScaler.GetPixelsPerUnit();
            UnityEngine.Events.UnityAction buttonCall = ui_picture_shadow.GetAction();

            // draw picture shadow
            ui_picture_shadow.SetImage(picture_shadow);

            // draw image
            UIElement picture = new UIElement(ui_picture_shadow.GetTransform());
            picture.SetLocation(0.30f, 0.30f, width_heigth-0.6f, width_heigth-0.6f);
            picture.SetBGColor(Color.clear);
            picture.SetImage(texture);
            picture.SetButton(buttonCall);

            // draw pin
            const float pin_width = 1.4f;
            const float pin_height = 1.6f;
            UIElement pin = new UIElement(picture.GetTransform());
            pin.SetLocation((width_heigth/2f)-(pin_width/1.5f), (-pin_height /2f), pin_width, pin_height);
            pin.SetBGColor(Color.clear);
            pin.SetImage(picture_pin);
            pin.SetButton(buttonCall);
        }


        private void SetFilterLang(string lang)
        {
            // update configuration
            langs_selected[lang] = !langs_selected[lang];

            // count active langs
            string list_langs = "";
            foreach (KeyValuePair<string, bool> ls in langs_selected)
            {
                if (ls.Value) list_langs += ls.Key + ";";
            }

            // ignore last flag deactivation
            if (list_langs.Length == 0)
            {
                langs_selected[lang] = !langs_selected[lang];
                return;
            }

            // save filter configuration
            list_langs = list_langs.Substring(0, list_langs.Length - 1);
            game.config.data.Add("UserConfig", "filterLangs", list_langs);
            game.config.Save();

            // lazy : display new flag on top
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
            int font_size_sort_buttons = (int)(UIScaler.GetMediumFont() * 0.95f);

            // sort type
            UIElement ui = null;

            Color button_color = Color.grey;

            foreach (SortOption s in sort_options_offline)
            {
                button_color = Color.grey;
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
            }

            y_offset += 2.5f;
            x_offset = 1.5f;

            foreach (SortOption s in sort_options_online)
            {
                button_color = Color.grey;
                if (s.name == sort_criteria)
                    button_color = Color.white;
                if (game.questsList.quest_list_mode != QuestsManager.QuestListMode.ONLINE)
                    button_color = Color.red;

                // local var required as button is called later with this value
                string local_name = s.name;

                ui = new UIElement(sortOptionsPopup.GetTransform());
                ui.SetLocation(x_offset, y_offset, button_size, 2f);
                ui.SetText(s.button_text, button_color);
                ui.SetFont(Game.Get().gameType.GetHeaderFont());
                ui.SetFontSize(font_size_sort_buttons);
                if (game.questsList.quest_list_mode == QuestsManager.QuestListMode.ONLINE)
                    ui.SetButton(delegate { SetSort(local_name); });
                new UIElementBorder(ui, button_color);

                x_offset += button_size + space_between_buttons;
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
            int font_size_sort_order_buttons = (int)(UIScaler.GetMediumFont() * 0.95f);

            if (sort_order == "ascending")
                descending_color = Color.grey;
            else
                ascending_color = Color.grey;

            // sort order
            UIElement ui = new UIElement(sortOptionsPopup.GetTransform());
            ui.SetLocation(x_offset, y_offset, button_size, 2f);
            ui.SetText(SORT_ASCENDING, ascending_color);
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(font_size_sort_order_buttons);
            ui.SetButton(delegate { sort_order = "ascending"; DrawSortOrderButtons(); SetSort(sort_criteria); });
            new UIElementBorder(ui, ascending_color);

            x_offset += button_size + space_between_buttons;

            ui = new UIElement(sortOptionsPopup.GetTransform());
            ui.SetLocation(x_offset, y_offset, button_size, 2f);
            ui.SetText(SORT_DESCENDING, descending_color);
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(font_size_sort_order_buttons);
            ui.SetButton(delegate { sort_order = "descending"; DrawSortOrderButtons(); SetSort(sort_criteria); });
            new UIElementBorder(ui, descending_color);
        }

        private float DrawSortInfo(string key, float offset)
        {
            switch (sort_criteria)
            {
                 case "author":
                    string current_author = game.questsList.GetQuestData(key).GetShortAuthor();
                    if(current_author != last_author)
                    {
                        UIElement ui = new UIElement(scrollArea.GetScrollTransform());
                        ui.SetLocation(1f, offset+0.4f, UIScaler.GetWidthUnits() - 5f, 2f);
                        ui.SetText(current_author, Color.black);
                        ui.SetTextAlignment(TextAnchor.MiddleLeft);
                        ui.SetFont(game.gameType.GetHeaderFont());
                        if(game.gameType.TypeName()=="D2E")
                            ui.SetFontSize((int)(UIScaler.GetMediumFont()*0.9f));
                        if (game.gameType.TypeName() == "MoM")
                            ui.SetFontSize(UIScaler.GetMediumFont());
                        ui.SetBGColor(Color.grey);
                        last_author = current_author;
                        offset += 2.7f;
                    }
                    break;

                case "date":
                    System.DateTime currentQuestDate = game.questsList.GetQuestData(key).latest_update;
                    float nb_days_since_update = (System.DateTime.Today - currentQuestDate).Days;
                    StringKey current_update_information = null;

                    foreach (KeyValuePair<float,StringKey> duration_text in nbDays_durationText)
                    {
                        if(nb_days_since_update < duration_text.Key)
                        {
                            current_update_information = duration_text.Value;
                            break;
                        }
                    }

                    if (current_update_information != last_update_info)
                    {
                        UIElement ui = new UIElement(scrollArea.GetScrollTransform());
                        ui.SetLocation(1f, offset + 0.4f, UIScaler.GetWidthUnits() - 5f, 2f);
                        ui.SetText(current_update_information, Color.black);
                        ui.SetTextAlignment(TextAnchor.MiddleLeft);
                        ui.SetFont(Game.Get().gameType.GetHeaderFont());
                        ui.SetFontSize(UIScaler.GetMediumFont());
                        ui.SetBGColor(Color.grey);
                        last_update_info = current_update_information;
                        offset += 2.6f;
                    }
                    break;

                default:
                    break;
            }
            
            return offset;
        }

        public void SetSort(string sort_selected_option)
        {
            if (game.questsList.quest_list_mode == QuestsManager.QuestListMode.ONLINE)
            { 
                // save sort configuration
                game.config.data.Add("UserConfig", "sortCriteria", sort_selected_option);
                game.config.data.Add("UserConfig", "sortOrder", sort_order);
                game.config.Save();
            }
            else
            {
                // save sort configuration
                game.config.data.Add("UserConfig", "offlineSortCriteria", sort_selected_option);
                game.config.data.Add("UserConfig", "offlineSortOrder", sort_order);
                game.config.Save();
            }

            // apply sort configuration
            sort_criteria = sort_selected_option;

            questList = Game.Get().questsList.GetList(sort_selected_option);
            if (sort_order == "descending")
                questList.Reverse();

            // update sort popup
            DrawSortCriteriaButtons();
        }

        public void GetQuestList()
        {
            // Get all user configuration
            Dictionary<string, string> config_values = game.config.data.Get("UserConfig");

            // check if connected on internet, and display scenario list accordingly (local or online)
            if (game.questsList.quest_list_mode == QuestsManager.QuestListMode.ONLINE)
            {
                if (config_values.ContainsKey("sortCriteria"))
                {
                    sort_criteria = config_values["sortCriteria"];
                }
                else
                {
                    sort_criteria = "rating";
                }

                if (config_values.ContainsKey("sortOrder"))
                {
                    sort_order = config_values["sortOrder"];
                }
                else
                {
                    sort_order = "descending";
                }
            }
            else
            {
                // check offline sort option is available
                if (config_values.ContainsKey("offlineSortCriteria"))
                {
                    sort_criteria = config_values["offlineSortCriteria"];
                }
                else
                {
                    sort_criteria = "name";
                }

                if (config_values.ContainsKey("offlineSortOrder"))
                {
                    sort_order = config_values["offlineSortOrder"];
                }
                else
                {
                    sort_order = "descending";
                }
            }

            // Get sorted list
            game.questsList.SortQuests();
            questList = game.questsList.GetList(sort_criteria);
            if (sort_order == "descending")
                questList.Reverse();
        }

        public void ReloadQuestList()
        {
            CleanQuestList();

            co_display = StartCoroutine(DrawQuestList());
        }

        public void CleanQuestList()
        {
            // Clean up everything marked as 'questlist'
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.QUESTLIST))
                Object.Destroy(go);

            scrollArea = null;

            // quest images
            images_list.Clear();

            if (co_display != null)
                StopCoroutine(co_display);
        }

        // check if the quest proposes at least one selected language
        public bool HasSelectedLanguage(QuestData.Quest q)
        {
            foreach (KeyValuePair<string, bool> lang in langs_selected)
            {
                // check if lang is selected in filters
                if (!lang.Value)
                    continue;

                if (game.questsList.quest_list_mode != QuestsManager.QuestListMode.ONLINE)
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
                    if (q == null || (q != null && q.languages_name != null && q.languages_name.Count <= 0))
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


        public IEnumerator DrawQuestList()
        {
            UIElement ui = null;

            // Start here
            float offset = 0;
            int nb_filtered_out_quest = 0;
            bool is_expansion_missing = false;

            if(scrollArea==null)
            {
                // scroll area
                scrollArea = new UIElementScrollVertical(Game.QUESTLIST);
                scrollArea.SetLocation(1, 5, UIScaler.GetWidthUnits() - 2f, UIScaler.GetHeightUnits() - 6f);
                new UIElementBorder(scrollArea, Color.grey);
            }

            // reset sort information
            last_author = "";
            last_update_info = null;

            // wait for the quest list to be downloaded
            while(game.questsList.quest_list_mode==QuestsManager.QuestListMode.DOWNLOADING)
            {
                yield return null;
            }

            if(game.questsList.quest_list_mode != QuestsManager.QuestListMode.ONLINE)
            {
                // Get and load a list of all locally available quests
                game.questsList.LoadAllLocalQuests();
            }

            // get quest list dependant on mode (online/offline)
            GetQuestList();

            // Loop through all available quests
            foreach (string key in questList)
            {
                QuestData.Quest q = game.questsList.GetQuestData(key);
                UIElement frame = null;
                is_expansion_missing = false;

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

                // Get data translation
                string name_translation = "";
                string synopsys_translation = "";
                if (game.questsList.quest_list_mode == QuestsManager.QuestListMode.ONLINE)
                {
                    // quest name is local language, or default language
                    if (q.languages_name != null &&
                        !q.languages_name.TryGetValue(game.currentLang, out name_translation))
                    {
                        q.languages_name.TryGetValue(q.defaultLanguage, out name_translation);
                    }
                    // same thing for synopsys: local language, or default language
                    if (q.languages_synopsys != null &&
                        !q.languages_synopsys.TryGetValue(game.currentLang, out synopsys_translation))
                    {
                        q.languages_synopsys.TryGetValue(game.currentLang, out synopsys_translation);
                    }
                }
                else
                {
                    LocalizationRead.AddDictionary("qst", q.localizationDict);
                    name_translation = q.name.Translate();
                    synopsys_translation = q.synopsys.Translate(true);
                }


                // Text information displayed depending on sort option
                offset = DrawSortInfo(key, offset);

                // Frame
                frame = new UIElement(scrollArea.GetScrollTransform());
                frame.SetLocation(0f, offset, UIScaler.GetWidthUnits() - 3.2f, 6.7f);
                frame.SetBGColor(Color.white);
                frame.SetImage(scroll_paper);
                offset += 0.15f;

                // prepare/draw list of Images
                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(0.9f, offset + 0.8f, 5f, 5f); // this is the location for the shadow (to be displayed first)
                ui.SetBGColor(Color.clear);
                if (q.image.Length > 0)
                {
                    if (game.questsList.quest_list_mode != QuestsManager.QuestListMode.ONLINE)
                    {
                        DrawScenarioPicture(ContentData.FileToTexture(Path.Combine(q.path, q.image)), ui); ;
                    }
                    else if (images_list.IsImageAvailable(q.package_url + q.image))
                    {
                        DrawScenarioPicture(images_list.GetTexture(q.package_url + q.image), ui);
                    }
                    else 
                    {
                        images_list.Add(q.package_url + q.image, ui);
                    }
                }
                else
                {
                    // Draw default Valkyrie picture
                    DrawScenarioPicture(images_list.GetTexture(null), ui);
                }

                // languages flags
                if (
                    (q.languages_name!=null && q.languages_name.Count>0) ||
                    (q.localizationDict!=null && game.questsList.quest_list_mode != QuestsManager.QuestListMode.ONLINE)
                    )
                {
                    List<string> languages = null;
                    if(q.languages_name.Count > 0)
                    {
                        languages = new List<string>(q.languages_name.Keys);
                    }
                    else
                    {
                        languages = q.localizationDict.GetLanguagesList();
                    }

                    Texture2D flagTex = null;
                    const float flag_size = 0.9f;
                    float flag_y_offset = offset + 0.25f;
                    float flag_x_offset = (UIScaler.GetWidthUnits() - 10f) - languages.Count * (flag_size + 0.2f); // align right

                    foreach (string lang_name in languages)
                    {
                        ui = new UIElement(scrollArea.GetScrollTransform());
                        flagTex = Resources.Load("sprites/flags/" + lang_name) as Texture2D;
                        ui.SetLocation(flag_x_offset, flag_y_offset, flag_size, flag_size);
                        ui.SetImage(flagTex);
                        flag_x_offset += flag_size + 0.2f;
                    }
                }

                // Quest name
                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetBGColor(Color.clear);
                ui.SetLocation(5.5f, offset + 0.3f, UIScaler.GetWidthUnits() - 8, 1.5f);
                ui.SetTextPadding(0.5f);
                if (name_translation.Length >= 70)
                    name_translation = name_translation.Substring(0, 65) + "(...)";
                ui.SetText(name_translation, Color.black);
                ui.SetTextAlignment(TextAnchor.MiddleLeft);
                if (game.gameType.TypeName() == "MoM")
                    ui.SetFontSize(Mathf.RoundToInt(UIScaler.GetSmallFont() * 1.4f));
                if (game.gameType.TypeName() == "D2E")
                    ui.SetFontSize(Mathf.RoundToInt(UIScaler.GetSmallFont() * 1.28f));
                ui.SetFont(game.gameType.GetHeaderFont());

                // Required expansions
                List<string> missing_packs = q.GetMissingPacks(game.cd.GetLoadedPackIDs());
                Color expansion_text_color = Color.black;
                float expansion_x_offset = 5.8f;
                float expansion_y_offset = offset + 1.7f;
                int expansions_symbol_font_size = UIScaler.GetSmallFont();
                if (game.gameType.TypeName() == "D2E")
                {
                    // fine tune D2E placement, has fonts don't look the same
                    expansion_x_offset = 5.55f;
                    expansion_y_offset = offset + 1.45f;
                    expansions_symbol_font_size = Mathf.RoundToInt(UIScaler.GetSmallFont() * 1.1f);
                }
                string pack_symbol_available = "";
                string pack_symbol_missing = "";
                foreach (string pack in q.packs)
                {
                    string pack_symbol = game.cd.packSymbol[pack].Translate(true);
                    if (pack_symbol != "" && !(pack_symbol_available.Contains(pack_symbol) || pack_symbol_missing.Contains(pack_symbol)))
                    {
                        if (missing_packs.Contains(pack))
                            pack_symbol_missing += pack_symbol;
                        else
                            pack_symbol_available += pack_symbol;
                    }
                }
                float symbols_available_width = 0;
                if(pack_symbol_available!="")
                { 
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    symbols_available_width = ui.GetStringWidth(pack_symbol_available, expansions_symbol_font_size, game.gameType.GetSymbolFont());
                    ui.SetLocation(expansion_x_offset, expansion_y_offset, symbols_available_width+1, 1);
                    ui.SetText(pack_symbol_available, Color.black);
                    ui.SetBGColor(Color.clear);
                    ui.SetFont(game.gameType.GetSymbolFont());
                    ui.SetFontSize(expansions_symbol_font_size);
                }

                if(pack_symbol_missing!="")
                {
                    is_expansion_missing = true;
                    pack_symbol_missing = MISSING_EXPANSIONS.Translate() + pack_symbol_missing;
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    float symbols_missing_width = ui.GetStringWidth(pack_symbol_missing, expansions_symbol_font_size, game.gameType.GetSymbolFont());
                    ui.SetLocation(expansion_x_offset+ symbols_available_width, expansion_y_offset, symbols_missing_width + 1, 1);
                    ui.SetText(pack_symbol_missing, dark_red_text_color);
                    ui.SetBGColor(Color.clear);
                    ui.SetFont(game.gameType.GetSymbolFont());
                    ui.SetFontSize(expansions_symbol_font_size);
                }

                // Quest short description (synopsys)
                if (synopsys_translation != null)
                { 
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetBGColor(Color.clear);
                    ui.SetLocation(5.5f, offset + 2.2f, UIScaler.GetRight(-11f) - 5, 2f);
                    ui.SetTextPadding(0.5f);
                    if (synopsys_translation.Length >= 105)
                        synopsys_translation=synopsys_translation.Substring(0, 100) + "(...)";
                    ui.SetText(synopsys_translation, dark_grey_text_color);
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    if (game.gameType.TypeName() == "MoM")
                        ui.SetFontSize(Mathf.RoundToInt(UIScaler.GetSmallFont() * 0.87f));
                    if (game.gameType.TypeName() == "D2E")
                        ui.SetFontSize(Mathf.RoundToInt(UIScaler.GetSmallFont() * 0.80f));
                    ui.SetFontStyle(FontStyle.Italic);
                    ui.SetFont(game.gameType.GetHeaderFont());
                }

                // Action Button
                // - First burnt hole
                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetBGColor(Color.clear);
                ui.SetLocation(UIScaler.GetRight(-10.5f), offset + 0.5f, 6, 4f);
                ui.SetImage(button_hole);
                if (!is_expansion_missing)
                {
                    ui.SetButton(delegate { Selection(key); });
                }
                // - then action button
                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetBGColor(Color.clear);
                ui.SetLocation(UIScaler.GetRight(-8.1f), offset + 1.4f, 1.8f, 1.8f);
                if(is_expansion_missing)
                {
                    ui.SetImage(button_no_entry);
                }
                else if (game.questsList.quest_list_mode != QuestsManager.QuestListMode.ONLINE)
                {
                    ui.SetImage(button_play);
                    ui.SetButton(delegate { Selection(key); });
                }
                else if(q.downloaded)
                {
                    if (q.update_available)
                        ui.SetImage(button_update);
                    else
                        ui.SetImage(button_play);
                    ui.SetButton(delegate { Selection(key); });
                }
                else
                {
                    ui.SetImage(button_download);
                    ui.SetButton(delegate { Selection(key); });
                }

                // all texts below use this y value as reference
                float top_text_y = offset + 4.1f;

                // Duration
                if (q.lengthMax != 0)
                {
                    float duration_text_offset = 0f;

                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetText(new StringKey("val", "DURATION"), Color.black);
                    duration_text_offset=ui.GetStringWidth(new StringKey("val", "DURATION"));
                    ui.SetLocation(6f, top_text_y, duration_text_offset + 0.5f, 1);
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetBGColor(Color.clear);

                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(6f+duration_text_offset+0.5f, top_text_y, 5, 1);
                    ui.SetText(q.lengthMin + "  -  " + q.lengthMax, Color.black);
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetBGColor(Color.clear);
                }

                //  average duration
                if (has_stats)
                {
                    StringKey STATS_AVERAGE_DURATION = new StringKey("val", "STATS_AVERAGE_DURATION", stats_avg_duration);
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(6, top_text_y + 1, 15, 1);
                    if (stats_avg_duration > 0)
                        ui.SetText(STATS_AVERAGE_DURATION, Color.black);
                    else
                        ui.SetText(STATS_NO_AVERAGE_DURATION, Color.black);
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetBGColor(Color.clear);
                }

                // Difficulty
                if (q.difficulty != 0)
                {
                    float difficulty_text_offset = 0f;
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetText(new StringKey("val", "DIFFICULTY"), Color.black);
                    difficulty_text_offset = ui.GetStringWidth(new StringKey("val", "DIFFICULTY"));
                    ui.SetLocation(UIScaler.GetHCenter() - 5.5f, top_text_y, difficulty_text_offset+0.5f, 1);
                    ui.SetTextAlignment(TextAnchor.LowerLeft);
                    ui.SetFontSize(UIScaler.GetSmallFont());
                    ui.SetBGColor(Color.clear);

                    string difficulty_symbol = "π"; // will
                    int font_size = (int)System.Math.Round(UIScaler.GetSmallFont() * 1.1f);
                    if (game.gameType is MoMGameType)
                    {
                        difficulty_symbol = new StringKey("val", "ICON_SUCCESS_RESULT").Translate();
                    }
                    float difficulty_string_width = 0;

                    ui = new UIElement(scrollArea.GetScrollTransform());
                    difficulty_string_width = ui.GetStringWidth(difficulty_symbol + difficulty_symbol + difficulty_symbol + difficulty_symbol + difficulty_symbol, font_size);
                    ui.SetLocation(UIScaler.GetHCenter() + (difficulty_text_offset - 5.5f), top_text_y + 0.1f, difficulty_string_width, 1);
                    ui.SetText(difficulty_symbol + difficulty_symbol + difficulty_symbol + difficulty_symbol + difficulty_symbol, grey_transparent_text_color);
                    ui.SetTextAlignment(TextAnchor.LowerLeft);
                    ui.SetBGColor(Color.clear);
                    ui.SetFontSize(font_size);

                    UIElementCropped uic = new UIElementCropped(scrollArea.GetScrollTransform());
                    uic.SetLocation(UIScaler.GetHCenter() + (difficulty_text_offset - 5.5f), top_text_y + 0.1f, difficulty_string_width, 1);
                    uic.SetText(difficulty_symbol + difficulty_symbol + difficulty_symbol + difficulty_symbol + difficulty_symbol, Color.black);
                    uic.SetTextAlignment(TextAnchor.LowerLeft);
                    uic.SetBGColor(Color.clear);
                    uic.SetFontSize(font_size);
                    uic.CropHorizontal(q.difficulty);
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
                }

                //  rating
                if (has_stats)
                {
                    //  Number of user reviews
                    StringKey STATS_NB_USER_REVIEWS = new StringKey("val", "STATS_NB_USER_REVIEWS", stats_play_count);
                    float user_review_text_width = 0;
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    user_review_text_width = ui.GetStringWidth(STATS_NB_USER_REVIEWS, UIScaler.GetSmallFont()) + 1;
                    ui.SetText(STATS_NB_USER_REVIEWS, Color.black);
                    ui.SetLocation(UIScaler.GetRight(-10.5f) - (user_review_text_width), top_text_y + 0.7f, user_review_text_width, 1);
                    ui.SetTextAlignment(TextAnchor.MiddleRight);
                    ui.SetBGColor(Color.clear);
                    ui.SetFontSize(UIScaler.GetSmallFont());

                    // rating
                    string rating_symbol = "★";
                    int font_size = (int)System.Math.Round(UIScaler.GetMediumFont() * 1.05f);
                    if (game.gameType is MoMGameType)
                    {
                        rating_symbol = new StringKey("val", "ICON_TENTACLE").Translate();
                        font_size = (int)System.Math.Round(UIScaler.GetMediumFont() * 1.4f);
                    }
                    float score_text_width = 0;

                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetText(rating_symbol + rating_symbol + rating_symbol + rating_symbol + rating_symbol, grey_transparent_text_color);
                    score_text_width = ui.GetStringWidth(rating_symbol + rating_symbol + rating_symbol + rating_symbol + rating_symbol, font_size);
                    ui.SetLocation(UIScaler.GetRight(-10.5f), top_text_y + 0.4f, score_text_width, 1.8f);
                    ui.SetBGColor(Color.clear);
                    ui.SetFontSize(font_size);
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetTextHorizontalOverflow(HorizontalWrapMode.Overflow);

                    UIElementCropped uic = new UIElementCropped(scrollArea.GetScrollTransform());
                    uic.SetText(rating_symbol + rating_symbol + rating_symbol + rating_symbol + rating_symbol, Color.black);
                    uic.SetLocation(UIScaler.GetRight(-10.5f), top_text_y + 0.4f, score_text_width, 1.8f);
                    uic.SetBGColor(Color.clear);
                    uic.SetFontSize(font_size);
                    uic.SetTextAlignment(TextAnchor.MiddleLeft);
                    uic.SetTextHorizontalOverflow(HorizontalWrapMode.Overflow);
                    uic.CropHorizontal(stats_rating);

                }

                offset += 7.1f;

                scrollArea.SetScrollSize(offset);

                if (nb_filtered_out_quest > 0)
                {
                    StringKey FILTER_TEXT_NUMBER_OF_FILTERED_SCENARIO = new StringKey("val", "FILTER_TEXT_NUMBER_OF_FILTERED_SCENARIO", nb_filtered_out_quest);
                    text_number_of_filtered_scenario.SetText(FILTER_TEXT_NUMBER_OF_FILTERED_SCENARIO);
                }
                else
                {
                    text_number_of_filtered_scenario.SetText(" ");
                }

                yield return null;
            }

            // Do it one last time in case the latest scenario has been filtered or if there are no scenario in the list
            if (nb_filtered_out_quest > 0)
            {
                StringKey FILTER_TEXT_NUMBER_OF_FILTERED_SCENARIO = new StringKey("val", "FILTER_TEXT_NUMBER_OF_FILTERED_SCENARIO", nb_filtered_out_quest);
                text_number_of_filtered_scenario.SetText(FILTER_TEXT_NUMBER_OF_FILTERED_SCENARIO);
            }
            else
            {
                text_number_of_filtered_scenario.SetText(" ");
            }

            images_list.StartDownloadASync();
        }

        // Return to main menu
        public void Cancel()
        {
            CleanQuestList();

            Destroyer.MainMenu();
        }

        // Select a quest
        public void Selection(string key)
        {
            ValkyrieDebug.Log("INFO: Select quest "+ key);

            QuestData.Quest q = game.questsList.GetQuestData(key);

            Destroyer.Dialog();
            CleanQuestList();

            if (game.questsList.quest_list_mode != QuestsManager.QuestListMode.ONLINE)
            {
                // Play
                ValkyrieDebug.Log("INFO: ... and launch offline quest");
                new QuestDetailsScreen(q);
            }
            else if (q.downloaded && !q.update_available)
            {
                // Play
                ValkyrieDebug.Log("INFO: ... and launch online quest");
                new QuestDetailsScreen(QuestLoader.GetSingleQuest(key));
            }
            else
            {
                // Download / Update
                ValkyrieDebug.Log("INFO: ... and download quest");
                GameObject download = new GameObject("downloadPage");
                download.tag = Game.QUESTUI;
                QuestDownload qd = download.AddComponent<QuestDownload>();
                qd.Download(key);
                // We need to refresh local quest list after download
                game.questsList.UnloadLocalQuests();
            }
        }


        private class ImgAsyncLoader
        {
            // URL and UI element
            private Dictionary<string, UIElement> images_list = null;
            // URL and Texture
            private Dictionary<string, Texture2D> texture_list = null;

            Texture2D default_quest_picture = null;

            // Father class
            QuestSelectionScreen questSelectionScreen = null;

            public ImgAsyncLoader(QuestSelectionScreen qss)
            {
                questSelectionScreen = qss;
                images_list = new Dictionary<string, UIElement>();
                texture_list = new Dictionary<string, Texture2D>();
                default_quest_picture = Resources.Load("sprites/scenario_list/default_quest_picture") as Texture2D;
            }

            public void Add(string url, UIElement uie)
            {
                images_list.Add(url, uie);
            }

            public void Clear()
            {
                images_list.Clear();
                // do not clear Texture, we don't want to download pictures again
            }

            public void StartDownloadASync()
            {
                if (images_list.Count > 0)
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

                    // Display default picture
                    if (images_list.ContainsKey(uri.ToString())) // this can be empty if we display another screen while pictures are downloading
                        questSelectionScreen.DrawScenarioPicture(null, images_list[uri.ToString()]);
                }
                else
                {
                    // we might have started two downloads of the same picture (changing sort options before end of download)
                    if (!texture_list.ContainsKey(uri.ToString()))
                    {
                        // save texture
                        texture_list.Add(uri.ToString(), texture);

                        // Display pictures
                        if(images_list.ContainsKey(uri.ToString())) // this can be empty if we display another screen while pictures are downloading
                            questSelectionScreen.DrawScenarioPicture(GetTexture(uri.ToString()),  images_list[uri.ToString()]);
                    }
                }
            }

            public bool IsImageAvailable(string package_url)
            {
                return texture_list.ContainsKey(package_url);
            }

            public Texture2D GetTexture(string package_url)
            {
                if (package_url == null)
                    return default_quest_picture;
                else
                    return texture_list[package_url];
            }
        }
    }

}
