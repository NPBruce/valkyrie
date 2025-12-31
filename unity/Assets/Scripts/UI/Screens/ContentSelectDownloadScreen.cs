using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Content;
using UnityEngine;
using UnityEngine.Events;
using ValkyrieTools;

namespace Assets.Scripts.UI.Screens
{
    public class ContentSelectDownloadScreen : MonoBehaviour, IContentImageDrawer
    {
        private static readonly StringKey CONTENTPACK_DOWNLOAD = new StringKey("val", "CONTENTPACK_DOWNLOAD_HEADER");

        public Game game;

        // Persistent UI Element
        private UIElement text_connection_status = null;
        private UIElementScrollVertical scrollArea = null;
        private readonly StringKey OFFLINE = new StringKey("val", "OFFLINE");
        private readonly StringKey ONLINE = new StringKey("val", "ONLINE");
        private readonly StringKey DOWNLOAD_ONGOING = new StringKey("val", "DOWNLOAD_ONGOING");
        private readonly StringKey OFFLINE_DUE_TO_ERROR = new StringKey("val", "OFFLINE_DUE_TO_ERROR");

        // Class to handle async images to display
        ImgAsyncLoader images_list = null;

        // textures
        Texture2D picture_shadow = null;
        Texture2D picture_pin = null;
        private Texture2D button_download = null;
        private Texture2D button_update = null;
        private Texture2D button_no_entry = null;
        // Display coroutine
        Coroutine co_display = null;

        // Create page
        public ContentSelectDownloadScreen()
        {
            CleanUpDialogs();

            // Initialize list of images for asynchronous loading
            images_list = new ImgAsyncLoader(this);

            //preload textures
            picture_shadow = Resources.Load("sprites/scenario_list/picture_shadow") as Texture2D;
            picture_pin = Resources.Load("sprites/scenario_list/picture_pin") as Texture2D;
            button_download = Resources.Load("sprites/scenario_list/button_download") as Texture2D;
            button_update = Resources.Load("sprites/scenario_list/button_update") as Texture2D;
            button_no_entry = Resources.Load("sprites/scenario_list/button_no_entry") as Texture2D;

            // Clean everything up
            Destroyer.Destroy();
            game = Game.Get();

            DrawHeadingAndButtons();
            DrawContentPackList();
        }

        private static void CleanUpDialogs()
        {
            // If a dialog window is open we force it closed (this shouldn't happen)
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            {
                Destroy(go);
            }

            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.CONTENTPACKLIST))
            {
                Destroy(go);
            }
        }

        private void DrawContentPackList()
        {
            UIElement ui = null;

            // Start here
            float offset = 0;
            int nb_filtered_out_quest = 0;

            if (scrollArea == null)
            {
                // scroll area
                scrollArea = new UIElementScrollVertical(Game.CONTENTPACKLIST);
                scrollArea.SetLocation(1, 5, UIScaler.GetWidthUnits() - 2f, UIScaler.GetHeightUnits() - 6f);
                new UIElementBorder(scrollArea, Color.grey);
            }

            var localContentPackList = game.cd.Values<PackTypeData>();
            foreach (var contentPack in game.remoteContentPackManager.remote_RemoteContentPack_data)
            {

                ui = RenderContentPackNameAndDescription(offset, contentPack);
                ui = RenderImage(offset, contentPack);
                RenderActionButton(offset, contentPack, localContentPackList);
                RenderDeleteButton(offset, contentPack, localContentPackList);

                offset += 7.1f;

                scrollArea.SetScrollSize(offset);
            }

            images_list.StartDownloadASync(scrollArea);
        }

        private void RenderActionButton(float offset, KeyValuePair<string, RemoteContentPack> contentPack, IEnumerable<PackTypeData> localContentPackList)
        {
            if (!contentPack.Value.update_available && contentPack.Value.downloaded)
            {
                return;
            }

            var ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetBGColor(Color.clear);
            ui.SetLocation(UIScaler.GetRight(-8.1f), offset + 1.4f, 1.8f, 1.8f);

            if (contentPack.Value.downloaded)
            {
                ui.SetImage(button_update);
                ui.SetButton(delegate { Download(contentPack.Value.identifier); });
            }
            else
            {
                ui.SetImage(button_download);
                ui.SetButton(delegate { Download(contentPack.Value.identifier); });
            }
        }

        private void RenderDeleteButton(float offset, KeyValuePair<string, RemoteContentPack> contentPack, IEnumerable<PackTypeData> localContentPackList)
        {
            if (contentPack.Value.downloaded)
            {

                var ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetBGColor(Color.clear);
                ui.SetLocation(UIScaler.GetRight(-6.0f), offset + 1.4f, 1.8f, 1.8f);
                ui.SetImage(button_no_entry);
                ui.SetButton(delegate { Delete(contentPack.Value.identifier); });
            }
        }

        public void Delete(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                ValkyrieDebug.Log("Could not delete content pack because of invalid key.");
                return;
            }

            try
            {
                string filePath = ContentData.CustomContentPackPath() + Path.DirectorySeparatorChar + key + ValkyrieConstants.ContentPackDownloadContainerExtension;
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    game.remoteContentPackManager.SetContentPackAvailability(key, false);
                    ContentLoader.RemoveContentPack(game, key);
                }
                else
                {
                    ValkyrieDebug.Log("Could not find file: " + key);
                }
            }
            catch (Exception)
            {
                ValkyrieDebug.Log("Failed to delete content pack: " + key);
            }

            new ContentSelectDownloadScreen();
        }

        public void Download(string key)
        {
            Destroyer.Dialog();

            // Download / Update
            ValkyrieDebug.Log("INFO: ... and download quest");
            GameObject download = new GameObject("downloadPage");
            download.tag = Game.CONTENTPACKUI;
            QuestAndContentPackDownload qd = download.AddComponent<QuestAndContentPackDownload>();
            qd.Download(key, true);
        }

        private UIElement RenderContentPackNameAndDescription(float offset, KeyValuePair<string, RemoteContentPack> remoteContentPack)
        {
            // Content pack name
            UIElement ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetBGColor(Color.clear);
            ui.SetLocation(5.5f, offset + 0.9f, UIScaler.GetWidthUnits() - 8, 1.5f);
            ui.SetTextPadding(0.5f);

            string name = remoteContentPack.Value.languages_name.FirstOrDefault().Value;

            ui.SetText(name, Color.red);
            ui.SetTextAlignment(TextAnchor.MiddleLeft);
            if (game.gameType.TypeName() == "MoM")
                ui.SetFontSize(Mathf.RoundToInt(UIScaler.GetSmallFont() * 1.4f));
            if (game.gameType.TypeName() == "D2E")
                ui.SetFontSize(Mathf.RoundToInt(UIScaler.GetSmallFont() * 1.28f));
            ui.SetFont(game.gameType.GetHeaderFont());

            string description = remoteContentPack.Value.languages_description.FirstOrDefault().Value;

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
        private UIElement RenderImage(float offset, KeyValuePair<string, RemoteContentPack> contentPack)
        {
            // prepare/draw list of Images
            UIElement ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetLocation(0.9f, offset + 0.8f, 5f, 5f); // this is the location for the shadow (to be displayed first)
            ui.SetBGColor(Color.clear);
            if (contentPack.Value.image.Length > 0)
            {
                if (images_list.IsImageAvailable(contentPack.Value.package_url + contentPack.Value.image))
                {
                    DrawPicture(images_list.GetTexture(contentPack.Value.package_url + contentPack.Value.image), ui);
                }
                else
                {
                    images_list.Add(contentPack.Value.package_url + contentPack.Value.image, ui);
                }
            }
            else
            {
                // Draw default Valkyrie picture
                DrawPicture(images_list.GetTexture(null), ui);
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

        private void DrawBackButton()
        {
            UIElement DrawBackButton = new UIElement();
            DrawBackButton.SetText(CommonStringKeys.BACK, Color.red);
            DrawBackButton.SetLocation(1, 0.5f, 6, 1.2f);
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
                game.remoteContentPackManager.Register_cb_download(RemoteQuestsListDownload_cb);
            }
            else if (game.remoteContentPackManager.content_pack_list_Mode == RemoteContentPackManager.RemoteContentPackListMode.ONLINE)
            {
                // Download done, we are online
                text_connection_status.SetText(ONLINE, Color.green);
                
            }
            else
            {
                // Download done, user has switched offline modline
                text_connection_status.SetText(OFFLINE, Color.red);
            }

            text_width = text_connection_status.GetStringWidth(text_connection_status.GetText(), UIScaler.GetSmallFont(), game.gameType.GetHeaderFont());
            text_connection_status.SetLocation(UIScaler.GetWidthUnits() - text_width - 1, 0.5f, text_width, 1.2f);
            text_connection_status.SetFont(game.gameType.GetHeaderFont());
            text_connection_status.SetFontSize(UIScaler.GetSmallFont());
            text_connection_status.SetTextAlignment(TextAnchor.MiddleCenter);
            if (border)
                new UIElementBorder(text_connection_status, text_connection_status.GetTextColor());

            //TODO check how Coroutine  logic works
            //if (co_display != null)
            //    StopCoroutine(co_display);
            //co_display = StartCoroutine(DrawContentPackList());

            //TODO when Coroutine logic works remove this line
            DrawContentPackList();
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

        public void DrawPicture(Texture2D texture, UIElement ui_picture_shadow)
        {
            float width_heigth = ui_picture_shadow.GetRectTransform().rect.width / UIScaler.GetPixelsPerUnit();
            UnityAction buttonCall = ui_picture_shadow.GetAction();

            // draw picture shadow
            ui_picture_shadow.SetImage(picture_shadow);

            // draw image
            UIElement picture = new UIElement(ui_picture_shadow.GetTransform());
            picture.SetLocation(0.30f, 0.30f, width_heigth - 0.6f, width_heigth - 0.6f);
            picture.SetBGColor(Color.clear);
            picture.SetImage(texture);
            picture.SetButton(buttonCall);

            // draw pin
            const float pin_width = 1.4f;
            const float pin_height = 1.6f;
            UIElement pin = new UIElement(picture.GetTransform());
            pin.SetLocation((width_heigth / 2f) - (pin_width / 1.5f), (-pin_height / 2f), pin_width, pin_height);
            pin.SetBGColor(Color.clear);
            pin.SetImage(picture_pin);
            pin.SetButton(buttonCall);
        }
    }
}