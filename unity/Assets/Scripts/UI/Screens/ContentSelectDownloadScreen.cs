using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Content;
using UnityEngine;
using UnityEngine.Events;
using ValkyrieTools;

namespace Assets.Scripts.UI.Screens
{
    public class ContentSelectDownloadScreen
    {
        private const int LARGE_FONT_LIMIT = 32;

        private static readonly StringKey CONTENTPACK_DOWNLOAD = new StringKey("val", "CONTENTPACK_DOWNLOAD_HEADER");

        public Game game;

        // Create page
        public ContentSelectDownloadScreen()
        {
            // Clean everything up
            Destroyer.Destroy();
            game = Game.Get();

            DrawButtons();
        }

        private static void DrawButtons()
        {
            // Heading
            UIElement ui = new UIElement();
            ui.SetLocation(2, 1, UIScaler.GetWidthUnits() - 4, 3);
            ui.SetText(CONTENTPACK_DOWNLOAD);
            ui.SetFontSize(UIScaler.GetLargeFont());

            // Button for back to content select screen
            ui = new UIElement();
            ui.SetLocation(1, 0.5f, 8, 1.5f);
            ui.SetText(CommonStringKeys.BACK, Color.red);
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(delegate { Quit(); });
            new UIElementBorder(ui, Color.red);
        }

        public static void Quit()
        {
            ValkyrieDebug.Log("INFO: Accessing content selection screen");

            new ContentSelectScreen();
        }

    }
}