using UnityEngine;
using Assets.Scripts.Content;

namespace Assets.Scripts.UI
{
    public class UIElementEditable : UIElement
    {
        protected GameObject input;

        protected string lastText = "";

        public UIElementEditable(string t = "", Transform parent = null) : base(t, parent)
        {
        }

        public UIElementEditable(Transform parent) : base(parent)
        {
        }

        public override void SetText(string content, Color textColor)
        {
            UnityEngine.UI.Text uiText = null;
            if (text == null)
            {
                input = new GameObject("UIInput");
                input.tag = tag;
                PanCancelInputField uiInput = input.AddComponent<PanCancelInputField>();
                uiInput.lineType = UnityEngine.UI.InputField.LineType.MultiLineNewline;
                input.transform.SetParent(bg.transform);
                RectTransform transform = input.AddComponent<RectTransform>();
                transform.anchorMin = Vector2.zero;
                transform.anchorMax = Vector2.one;
                transform.localPosition = Vector3.zero;
                transform.localScale = Vector3.one;
                transform.offsetMin = Vector2.zero;
                transform.offsetMax = Vector2.zero;

                text = new GameObject("UIText");
                text.tag = tag;
                uiText = text.AddComponent<UnityEngine.UI.Text>();
                uiText.alignment = TextAnchor.MiddleCenter;
                uiText.font = Game.Get().gameType.GetFont();
                uiText.fontSize = UIScaler.GetSmallFont();
                uiText.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.transform.SetParent(input.transform);
                transform = text.GetComponent<RectTransform>();
                transform.anchorMin = Vector2.zero;
                transform.anchorMax = Vector2.one;
                transform.localPosition = Vector3.zero;
                transform.localScale = Vector3.one;
                transform.offsetMin = new Vector2(textPadding * UIScaler.GetPixelsPerUnit(), 0);
                transform.offsetMax = new Vector2(-textPadding * UIScaler.GetPixelsPerUnit(), 0);

                uiInput.textComponent = uiText;

                if (buttonCall != null)
                {
                    uiInput.onEndEdit.AddListener(delegate { buttonCall(); });
                }
            }
            uiText = text.GetComponent<UnityEngine.UI.Text>();
            uiText.color = textColor;
            if (textColor.Equals(Color.black))
            {
                uiText.material = (Material)Resources.Load("Fonts/FontMaterial");
            }
            else
            {
                uiText.material = uiText.font.material;
            }
            input.GetComponent<PanCancelInputField>().text = content;
            lastText = content;
        }

        public void SetSingleLine()
        {
            input.GetComponent<PanCancelInputField>().lineType = UnityEngine.UI.InputField.LineType.SingleLine;
        }

        public override string GetText()
        {
            if (input == null) return "";
            return input.GetComponent<PanCancelInputField>().text;
        }

        public bool Changed()
        {
            bool changed = !GetText().Equals(lastText);
            lastText = GetText();
            return changed;
        }

        public override void SetButton(UnityEngine.Events.UnityAction call)
        {
            buttonCall = call;
            if (input != null)
            {
                input.GetComponent<PanCancelInputField>().onEndEdit.AddListener(delegate { buttonCall(); });
            }
        }
    }
}
