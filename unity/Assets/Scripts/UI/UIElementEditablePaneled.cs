using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

namespace Assets.Scripts.UI
{
    public class UIElementEditablePaneled : UIElementEditable
    {
        public UIElementEditablePaneled(string t = "", Transform parent = null) : base(t, parent)
        {
        }

        public UIElementEditablePaneled(Transform parent) : base(parent)
        {
        }

        public override void SetLocationPixels(float x, float y, float width, float height)
        {
            UIElement ui = new UIElement(tag, bg.transform);
            ui.SetLocationPixels(0, -UIScaler.GetPixelsPerUnit(), width, UIScaler.GetPixelsPerUnit());
            new UIElementBorder(ui);

            base.SetLocationPixels(x, y + UIScaler.GetPixelsPerUnit(), width, height - UIScaler.GetPixelsPerUnit());
            Dictionary<string, string> CHARS = null;
            EventManager.CHARS_MAP.TryGetValue(Game.Get().gameType.TypeName(), out CHARS);

            if (CHARS != null)
            {
                float buttonWidth = width / CHARS.Keys.Count;
                float xPos = 0;
                foreach (string oneChar in CHARS.Keys)
                {
                    StringKey translated = new StringKey(null, oneChar);
                    string translation = null;
                    CHARS.TryGetValue(oneChar, out translation);
                    ui = new UIElement(tag, bg.transform);
                    ui.SetLocationPixels(xPos, -UIScaler.GetPixelsPerUnit(), buttonWidth, UIScaler.GetPixelsPerUnit());
                    ui.SetText(translated);
                    ui.SetButton(delegate { InsertCharacter(translation); });

                    xPos += buttonWidth;
                }
            }
        }

        public void InsertCharacter(string specialChar)
        {
            PanCancelInputField uiInput = input.GetComponent<PanCancelInputField>();
            uiInput.text = uiInput.text.Insert(uiInput.getLastCaretPosition(), specialChar);
            uiInput.Select();
        }

        /// <summary>
        /// Set the UIElement to text height plus vertical padding</summary>
        /// <returns>New UIElement height</returns>
        public override float HeightToTextPadding(float space = 0)
        {
            return base.HeightToTextPadding(space) + 1;
        }
    }
}
