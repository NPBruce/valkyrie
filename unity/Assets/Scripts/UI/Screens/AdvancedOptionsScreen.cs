using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ValkyrieTools;
using Assets.Scripts.Content;

namespace Assets.Scripts.UI.Screens
{
    // Class for advanced options menu
    public class AdvancedOptionsScreen
    {
        private static readonly string IMG_LOW_EDITOR_TRANSPARENCY = "ImageLowEditorTransparency";
        private static readonly string IMG_MEDIUM_EDITOR_TRANSPARENCY = "ImageMediumEditorTransparency";
        private static readonly string IMG_HIGH_EDITOR_TRANSPARENCY = "ImageHighEditorTransparency";

        private readonly StringKey ADVANCED_OPTIONS = new StringKey("val", "ADVANCED_OPTIONS");
        private readonly StringKey SET_EDITOR_ALPHA = new StringKey("val", "SET_EDITOR_ALPHA");
        private readonly StringKey PLAY_AUDIO_IN_BACKGROUND = new StringKey("val", "PLAY_AUDIO_IN_BACKGROUND");
        private readonly StringKey VIEW_LOCK = new StringKey("val", "VIEW_LOCK");
        private readonly StringKey OptionON = new StringKey("val", "ON");
        private readonly StringKey OptionOff = new StringKey("val", "OFF");
        private readonly StringKey EXPORT_LOG = new StringKey("val", "EXPORT_LOG");

        Game game = Game.Get();

        // Create a menu which will take up the whole screen and have options.  All items are dialog for destruction.
        public AdvancedOptionsScreen()
        {
            // This will destroy all, because we shouldn't have anything left at the main menu
            Destroyer.Destroy();

            game = Game.Get();

            // Create elements for the screen
            CreateElements();
        }

        private void CreateElements()
        {
            // Options screen text
            UIElement ui = new UIElement();
            ui.SetLocation(2, 1, UIScaler.GetWidthUnits() - 4, 3);
            ui.SetText(ADVANCED_OPTIONS);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetLargeFont());

            CreateEditorTransparencyElements();
            CreateAudioElements();
            CreateViewLockElements();
            CreateLogElement();

            // Button for back to main menu
            ui = new UIElement();
            ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
            ui.SetText(CommonStringKeys.BACK, Color.red);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(delegate { Destroyer.Dialog(); new OptionsScreen(); });
            new UIElementBorder(ui, Color.red);
        }

        private void CreateEditorTransparencyElements()
        {
            // Select language text
            UIElement ui = new UIElement(Game.DIALOG);
            ui.SetLocation(UIScaler.GetHCenter() - 10, 5, 20, 2);
            ui.SetText(SET_EDITOR_ALPHA);
            ui.SetTextAlignment(TextAnchor.MiddleCenter);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());

            Texture2D SampleTex = ContentData.FileToTexture(game.cd.Get<ImageData>(IMG_LOW_EDITOR_TRANSPARENCY).image);
            Sprite SampleSprite = Sprite.Create(SampleTex, new Rect(0, 0, SampleTex.width, SampleTex.height), Vector2.zero, 1);
            ui = new UIElement(Game.DIALOG);
            ui.SetLocation(UIScaler.GetHCenter() - 10, 8, 5, 5);
            ui.SetButton(delegate { UpdateEditorTransparency(0.2f); });
            ui.SetImage(SampleSprite);
            if (game.editorTransparency == 0.2f)
                new UIElementBorder(ui, Color.white);

            SampleTex = ContentData.FileToTexture(game.cd.Get<ImageData>(IMG_MEDIUM_EDITOR_TRANSPARENCY).image);
            SampleSprite = Sprite.Create(SampleTex, new Rect(0, 0, SampleTex.width, SampleTex.height), Vector2.zero, 1);
            ui = new UIElement(Game.DIALOG);
            ui.SetLocation(UIScaler.GetHCenter() - 2.5f, 8, 5, 5);
            ui.SetButton(delegate { UpdateEditorTransparency(0.3f); });
            ui.SetImage(SampleSprite);
            if (game.editorTransparency == 0.3f)
                new UIElementBorder(ui, Color.white);

            SampleTex = ContentData.FileToTexture(game.cd.Get<ImageData>(IMG_HIGH_EDITOR_TRANSPARENCY).image);
            SampleSprite = Sprite.Create(SampleTex, new Rect(0, 0, SampleTex.width, SampleTex.height), Vector2.zero, 1);
            ui = new UIElement(Game.DIALOG);
            ui.SetLocation(UIScaler.GetHCenter() + 5, 8, 5, 5);
            ui.SetButton(delegate { UpdateEditorTransparency(0.4f); });
            ui.SetImage(SampleSprite);
            if (game.editorTransparency == 0.4f)
                new UIElementBorder(ui, Color.white);
        }

        private void CreateAudioElements()
        {
            // Background Audio Toggle
            // Only render on Windows, Mac or Linux (player or editor)
            var p = Application.platform;
            bool isSupportedPlatform =
                p == RuntimePlatform.WindowsPlayer || p == RuntimePlatform.OSXPlayer || p == RuntimePlatform.LinuxPlayer
                || p == RuntimePlatform.WindowsEditor || p == RuntimePlatform.OSXEditor || p == RuntimePlatform.LinuxEditor;

            if (isSupportedPlatform)
            {
                // Check config
                string configBgAudio = game.config.data.Get("UserConfig", "playAudioInBackground");
                bool isBgAudio = configBgAudio == "1";

                UIElement ui = new UIElement();
                ui.SetLocation(UIScaler.GetHCenter() - 10, 14, 20, 2);
                ui.SetText(PLAY_AUDIO_IN_BACKGROUND);
                ui.SetFont(game.gameType.GetHeaderFont());
                ui.SetFontSize(UIScaler.GetMediumFont());

                ui = new UIElement();
                ui.SetLocation(UIScaler.GetHCenter() - 3, 16.5f, 6, 2);
                ui.SetText(isBgAudio ? OptionON : OptionOff);
                ui.SetButton(delegate
                {
                    bool newState = !isBgAudio;
                    Application.runInBackground = newState;
                    game.config.data.Add("UserConfig", "playAudioInBackground", newState ? "1" : "0");
                    game.config.Save();
                    new AdvancedOptionsScreen();
                });
                if (isBgAudio)
                    new UIElementBorder(ui, Color.white);
                else
                    new UIElementBorder(ui, Color.grey);
            }
        }

        private void CreateViewLockElements()
        {
            string configViewLock = game.config.data.Get("UserConfig", "viewLock");
            bool isViewLock = configViewLock == "1";

            UIElement ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter() - 10, 19.5f, 20, 2);
            ui.SetText(VIEW_LOCK);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter() - 3, 22f, 6, 2);
            ui.SetText(isViewLock ? OptionON : OptionOff);
            ui.SetButton(delegate
            {
                bool newState = !isViewLock;
                game.config.data.Add("UserConfig", "viewLock", newState ? "1" : "0");
                game.config.Save();
                new AdvancedOptionsScreen();
            });
            if (isViewLock)
                new UIElementBorder(ui, Color.white);
            else
                new UIElementBorder(ui, Color.grey);
        }

        private void UpdateEditorTransparency(float alpha)
        {
            game.config.data.Add("UserConfig", "editorTransparency", alpha.ToString());
            game.config.Save();
            game.editorTransparency = alpha;

            new AdvancedOptionsScreen();
        }

        private void CreateLogElement()
        {
            UIElement ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter() - 4, 25.5f, 8, 2);
            ui.SetText(EXPORT_LOG);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetSmallFont());
            ui.SetButton(delegate
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    NativeFilePicker.ExportFile(FileLogger.GetLogPath());
                }
                else
                {
                    string path = System.IO.Path.GetDirectoryName(Application.consoleLogPath);
                    Application.OpenURL(path);
                }
            });
            new UIElementBorder(ui, Color.white);
        }
    }
}
