using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Assets.Scripts.Content;
using UnityEngine;
using UnityEngine.Events;
using ValkyrieTools;


namespace Assets.Scripts.UI.Screens
{
    public class ContentSelectDownloadScreen : MonoBehaviour
    {
        private const int LARGE_FONT_LIMIT = 32;

        private static readonly StringKey CONTENTPACK_DOWNLOAD = new StringKey("val", "CONTENTPACK_DOWNLOAD_HEADER");

        public Game game;

        // Persistent UI Element
        private UIElement text_connection_status = null;
        private UIElementScrollVertical scrollArea = null;
        private RemoteContentPackManager manager;
        private readonly StringKey GO_OFFLINE = new StringKey("val", "GO_OFFLINE");
        private readonly StringKey GO_ONLINE = new StringKey("val", "GO_ONLINE");
        private readonly StringKey DOWNLOAD_ONGOING = new StringKey("val", "DOWNLOAD_ONGOING");
        private readonly StringKey OFFLINE_DUE_TO_ERROR = new StringKey("val", "OFFLINE_DUE_TO_ERROR");

        private Texture2D button_download = null;
        private Texture2D button_update = null;
        private Texture2D button_play = null;
        private Texture2D button_no_entry = null;
        // Display coroutine
        Coroutine co_display = null;

        // Create page
        public ContentSelectDownloadScreen()
        {
            button_download = Resources.Load("sprites/scenario_list/button_download") as Texture2D;
            button_update = Resources.Load("sprites/scenario_list/button_update") as Texture2D;
            button_play = Resources.Load("sprites/scenario_list/button_play") as Texture2D;
            button_no_entry = Resources.Load("sprites/scenario_list/button_no_entry") as Texture2D;

            // Clean everything up
            Destroyer.Destroy();
            game = Game.Get();

            DrawHeadingAndButtons();
            DrawContentPackList();
        }

        private void DrawContentPackList()
        {
            UIElement ui = null;

            // Start here
            float offset = 0;
            int nb_filtered_out_quest = 0;
            bool is_expansion_missing = false;

            if (scrollArea == null)
            {
                // scroll area
                scrollArea = new UIElementScrollVertical(Game.QUESTLIST);
                scrollArea.SetLocation(1, 5, UIScaler.GetWidthUnits() - 2f, UIScaler.GetHeightUnits() - 6f);
                new UIElementBorder(scrollArea, Color.grey);
            }

            var localContentPackList = game.cd.Values<PackTypeData>();
            foreach (var contentPack in game.remoteContentPackManager.remote_RemoteContentPack_data)
            {

                ui = RenderContentPackNameAndDescription(offset, contentPack);
                ui = RenderActionButton(offset, is_expansion_missing, contentPack, localContentPackList);

            }

            //yield return null;
        }

        private UIElement RenderActionButton(float offset, bool is_expansion_missing, KeyValuePair<string, RemoteContentPack> contentPack, IEnumerable<PackTypeData> localContentPackList)
        {
            var ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetBGColor(Color.clear);
            ui.SetLocation(UIScaler.GetRight(-8.1f), offset + 1.4f, 1.8f, 1.8f);
            if (is_expansion_missing)
            {
                ui.SetImage(button_no_entry);
            }

            //TODO
            if (contentPack.Value.downloaded)
            {
                if (contentPack.Value.update_available)
                    ui.SetImage(button_update);
                else
                    ui.SetImage(button_play);
                ui.SetButton(delegate { Download(contentPack.Value.identifier); });
            }
            else
            {
                ui.SetImage(button_download);
                ui.SetButton(delegate { Download(contentPack.Value.identifier); });
            }

            return ui;
        }

        public void Download(string key)
        {
            ValkyrieDebug.Log("INFO: Select contentpack " + key);

            Destroyer.Dialog();
            CleanContentPackList();


            // Download / Update
            ValkyrieDebug.Log("INFO: ... and download quest");
            GameObject download = new GameObject("downloadPage");
            download.tag = Game.QUESTUI;
            QuestAndContentPackDownload qd = download.AddComponent<QuestAndContentPackDownload>();
            qd.Download(key, true);
            // We need to refresh local content pack list after download
            //TODO
            //game.UnloadLocalQuests();

        }

        private void CleanContentPackList()
        {
            //TODO
            //throw new NotImplementedException();
        }

        private UIElement RenderContentPackNameAndDescription(float offset, KeyValuePair<string, RemoteContentPack> contentPack)
        {

            // Content pack name
            UIElement ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetBGColor(Color.clear);
            ui.SetLocation(5.5f, offset + 0.3f, UIScaler.GetWidthUnits() - 8, 1.5f);
            ui.SetTextPadding(0.5f);

            string name = contentPack.Value.languages_name.FirstOrDefault().Value;

            ui.SetText(name, Color.red);
            ui.SetTextAlignment(TextAnchor.MiddleLeft);
            if (game.gameType.TypeName() == "MoM")
                ui.SetFontSize(Mathf.RoundToInt(UIScaler.GetSmallFont() * 1.4f));
            if (game.gameType.TypeName() == "D2E")
                ui.SetFontSize(Mathf.RoundToInt(UIScaler.GetSmallFont() * 1.28f));
            ui.SetFont(game.gameType.GetHeaderFont());

            string description = contentPack.Value.languages_description.FirstOrDefault().Value;

            if (description != null)
            {
                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetBGColor(Color.clear);
                ui.SetLocation(5.5f, offset + 2.2f, UIScaler.GetRight(-11f) - 5, 2f);
                ui.SetTextPadding(0.5f);
                if (description.Length >= 105)
                    description = description.Substring(0, 100) + "(...)";
                ui.SetText(description, Color.red);
                ui.SetTextAlignment(TextAnchor.MiddleLeft);
                if (game.gameType.TypeName() == "MoM")
                    ui.SetFontSize(Mathf.RoundToInt(UIScaler.GetSmallFont() * 0.87f));
                if (game.gameType.TypeName() == "D2E")
                    ui.SetFontSize(Mathf.RoundToInt(UIScaler.GetSmallFont() * 0.80f));
                ui.SetFontStyle(FontStyle.Italic);
                ui.SetFont(game.gameType.GetHeaderFont());
            }

            return ui;
        }

        private void DrawHeadingAndButtons()
        {
            // Heading
            DrawHeading();

            // Button for back to content select screen
            DrawBackButton();

            DrawOnlineModeButton();
        }

        private static void DrawBackButton()
        {
            UIElement DrawBackButton = new UIElement();
            DrawBackButton.SetLocation(1, 0.5f, 8, 1.5f);
            DrawBackButton.SetText(CommonStringKeys.BACK, Color.red);
            DrawBackButton.SetFont(Game.Get().gameType.GetHeaderFont());
            DrawBackButton.SetFontSize(UIScaler.GetMediumFont());
            DrawBackButton.SetButton(delegate { Quit(); });
            new UIElementBorder(DrawBackButton, Color.red);
        }

        private static void DrawHeading()
        {
            UIElement ui = new UIElement();
            ui.SetLocation(2, 1, UIScaler.GetWidthUnits() - 4, 3);
            ui.SetText(CONTENTPACK_DOWNLOAD);
            ui.SetFontSize(UIScaler.GetLargeFont());
        }

        private void DrawOnlineModeButton()
        {
            float text_width = 0f;
            bool border = false;

            if (text_connection_status != null)
                text_connection_status.Destroy();

            text_connection_status = new UIElement();


            //TODO Continue here
            // Display connection status message
            if (game.remoteContentPackManager.content_pack_list_Mode == RemoteContentPackManager.RemoteContentPackListMode.ERROR_DOWNLOAD)
            {
                // error download (no connection, timeout, of file not available)
                text_connection_status.SetText(OFFLINE_DUE_TO_ERROR, Color.red);
            }
            else if (game.remoteContentPackManager.content_pack_list_Mode == RemoteContentPackManager.RemoteContentPackListMode.DOWNLOADING)
            {
                // Download ongoing
                text_connection_status.SetText(DOWNLOAD_ONGOING, Color.cyan);
                manager.Register_cb_download(RemoteQuestsListDownload_cb);
            }
            else if (game.remoteContentPackManager.content_pack_list_Mode == RemoteContentPackManager.RemoteContentPackListMode.ONLINE)
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
            if (border)
                new UIElementBorder(text_connection_status, text_connection_status.GetTextColor());

            //TODO check routine logic
            //if (co_display != null)
            //    StopCoroutine(co_display);
            //co_display = StartCoroutine(DrawContentPackList());

            DrawContentPackList();
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
            //TODO
            //ReloadQuestList();
        }

        private void RemoteQuestsListDownload_cb(bool is_available)
        {
            DrawOnlineModeButton();
        }

        public static void Quit()
        {
            ValkyrieDebug.Log("INFO: Accessing content selection screen");

            new ContentSelectScreen();
        }
    }
}