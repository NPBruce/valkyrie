using Assets.Scripts.Content;
using UnityEngine;
using FFGAppImport;
using System.Threading;
using System.Collections.Generic;

namespace Assets.Scripts.UI.Screens
{

    public class LoadingScreen
    {
        private UIElement messageUI;
        private UIElementScrollVertical scrollArea;
        private List<UIElement> logLineUIs = new List<UIElement>();
        private List<string> logLineStrings = new List<string>();
        private List<float> logLineHeights = new List<float>();

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
        /// <param name="download">UnityWebRequest object tracking download</param>
        /// <param name="display">Text to display</param>
        public LoadingScreen(UnityEngine.Networking.UnityWebRequest download, string display = "")
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
            // Background
            UIElement bg = new UIElement();
            bg.SetLocation(0, 0, UIScaler.GetWidthUnits(), UIScaler.GetHeightUnits());
            bg.SetBGColor(Color.black);

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
            messageUI = new UIElement();
            messageUI.SetLocation(2, 20, UIScaler.GetWidthUnits() - 4, 2);
            messageUI.SetText(display);
            messageUI.SetFontSize(UIScaler.GetMediumFont());
        }

        /// <summary>
        /// Update the displayed text on the loading screen
        /// </summary>
        /// <param name="display">Text to display</param>
        public void UpdateDisplay(string display)
        {
            if (messageUI == null || display == null) return;

            if (scrollArea == null)
            {
                // Move the main message to the top
                messageUI.SetLocation(2, 1, UIScaler.GetWidthUnits() - 4, 2);

                scrollArea = new UIElementScrollVertical();
                scrollArea.SetLocation(2, 4, UIScaler.GetWidthUnits() - 4, 22);
                // Quest Log workaround for broken Unity font rendering:
                // Black text on a white background. When text bleeds outside the box,
                // it hits the black screen background and becomes completely invisible!
                scrollArea.SetBGColor(Color.white);
                new UIElementBorder(scrollArea);
            }

            // Fix horizontal overflow by allowing Unity to wrap at directory separators
            display = display.Replace("\\", "\u200B\\").Replace("/", "\u200B/");
            // Repair broken rich text closing tags caused by the slash replacement
            display = display.Replace("<\u200B/", "</");

            float textWidth = UIScaler.GetWidthUnits() - 5.5f;

            if (logLineStrings.Count >= 50)
            {
                // Recycle the oldest UIElement
                logLineStrings.RemoveAt(0);
                logLineHeights.RemoveAt(0);

                UIElement oldUi = logLineUIs[0];
                logLineUIs.RemoveAt(0);
                logLineUIs.Add(oldUi);
            }
            else
            {
                // Create a new one
                UIElement ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetText("", Color.black); // Initialize text GameObject to prevent NullReferenceException
                ui.SetTextAlignment(UnityEngine.TextAnchor.UpperLeft);
                ui.SetFontSize(UIScaler.GetSmallFont());
                ui.SetBGColor(Color.clear);
                logLineUIs.Add(ui);
            }

            UIElement newUi = logLineUIs[logLineUIs.Count - 1];
            float h = newUi.GetStringHeight(display + "\n...", textWidth);

            logLineStrings.Add(display);
            logLineHeights.Add(h);

            float offset = 0f;
            for (int i = 0; i < logLineStrings.Count; i++)
            {
                logLineUIs[i].SetLocation(0, offset, textWidth, logLineHeights[i]);
                logLineUIs[i].SetText(logLineStrings[i], Color.black);
                offset += logLineHeights[i];
            }

            if (offset < 22) offset = 22;
            scrollArea.SetScrollSize(offset);

            UnityEngine.UI.ScrollRect sr = scrollArea.GetScrollTransform().parent.GetComponent<UnityEngine.UI.ScrollRect>();
            if (sr != null)
            {
                sr.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
                sr.verticalNormalizedPosition = 0f;
            }

            UpdateIndicator(UnityEngine.Time.time);
        }

        /// <summary>
        /// Updates the blinking loading indicator at the bottom of the log
        /// </summary>
        /// <param name="time">Current time</param>
        public void UpdateIndicator(float time)
        {
            if (scrollArea == null || logLineStrings.Count == 0) return;

            int dots = (int)(time * 2f) % 4;
            string indicator = new string('.', dots);

            int lastIdx = logLineUIs.Count - 1;
            logLineUIs[lastIdx].SetText(logLineStrings[lastIdx] + "\n" + indicator, Color.black);
        }
    }
}