using Assets.Scripts.Content;
using UnityEngine;
using FFGAppImport;
using System.Threading;

namespace Assets.Scripts.UI.Screens
{

    public class LoadingScreen
    {
        /// <summary>
        /// Construct a screen with pulsing logo and optional text label
        /// </summary>
        /// <param name="display">Text to display</param>
        public LoadingScreen(string display = "")
        {
            Destroyer.Dialog();
            Draw(display);
        }

        /// <summary>
        /// Construct a screen with pulsing logo, download progress and optional text label
        /// </summary>
        /// <param name="download">WWW object tracking download</param>
        /// <param name="display">Text to display</param>
        public LoadingScreen(WWW download, string display = "")
        {
            Destroyer.Dialog();
            Draw(display);

            // Create border
            UIElement ui = new UIElement();
            ui.SetLocation(3, 27, UIScaler.GetWidthUnits() - 6, 2);
            ui.SetBGColor(Color.clear);
            new UIElementBorder(ui);

            // Create an object
            GameObject bar = new GameObject("progress");
            // Mark it as dialog
            bar.tag = Game.DIALOG;
            bar.transform.SetParent(Game.Get().uICanvas.transform);

            RectTransform transBg = bar.AddComponent<RectTransform>();
            transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 3 * UIScaler.GetPixelsPerUnit(), (UIScaler.GetWidthUnits() - 6) * UIScaler.GetPixelsPerUnit());
            transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 27 * UIScaler.GetPixelsPerUnit(), 2 * UIScaler.GetPixelsPerUnit());

            // Create the image
            UnityEngine.UI.Image uiImage = bar.AddComponent<UnityEngine.UI.Image>();
            uiImage.color = Color.white;
            bar.AddComponent<ProgressBar>().SetDownload(download);
        }

        /// <summary>
        /// Draw the contents to the screen
        /// </summary>
        /// <param name="display">Text to display</param>
        private void Draw(string display)
        {
            // Create an object
            GameObject logo = new GameObject("logo");
            // Mark it as dialog
            logo.tag = Game.DIALOG;
            logo.transform.SetParent(Game.Get().uICanvas.transform);

            RectTransform transBg = logo.AddComponent<RectTransform>();
            transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, UIScaler.GetHCenter(-3) * UIScaler.GetPixelsPerUnit(), 6 * UIScaler.GetPixelsPerUnit());
            transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 8 * UIScaler.GetPixelsPerUnit(), 6 * UIScaler.GetPixelsPerUnit());

            // Create the image
            Texture2D tex = Resources.Load("sprites/logo") as Texture2D;
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1);
            UnityEngine.UI.Image uiImage = logo.AddComponent<UnityEngine.UI.Image>();
            uiImage.sprite = sprite;
            logo.AddComponent<SpritePulser>();

            // Display message
            UIElement ui = new UIElement();
            ui.SetLocation(2, 20, UIScaler.GetWidthUnits() - 4, 2);
            ui.SetText(display);
            ui.SetFontSize(UIScaler.GetMediumFont());
        }
    }
}