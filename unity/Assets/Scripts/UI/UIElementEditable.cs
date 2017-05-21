using UnityEngine;
using Assets.Scripts.Content;

namespace Assets.Scripts.UI
{
    class UIElementEditable : UIElement
    {
        protected GameObject input;

        public override void SetText(string content, Color textColor)
        {
            UnityEngine.UI.Text uiText = null;
            if (text == null)
            {
                text = new GameObject("UIText");
                text.tag = tag;
                uiText = text.AddComponent<UnityEngine.UI.Text>();
                uiText.alignment = TextAnchor.MiddleCenter;
                uiText.font = Game.Get().gameType.GetFont();
                uiText.fontSize = UIScaler.GetSmallFont();
                uiText.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.transform.SetParent(bg.transform);
                RectTransform transform = text.GetComponent<RectTransform>();
                transform.anchorMin = Vector2.zero;
                transform.anchorMax = Vector2.one;
                transform.localPosition = Vector3.zero;
                transform.localScale = Vector3.one;
                transform.offsetMin = new Vector2(textPadding * UIScaler.GetPixelsPerUnit(), 0);
                transform.offsetMax = new Vector2(-textPadding * UIScaler.GetPixelsPerUnit(), 0);

                input = new GameObject("UIInput");
                PanCancelInputField uiInput = input.AddComponent<PanCancelInputField>();
                uiInput.textComponent = uiText;
                uiInput.lineType = UnityEngine.UI.InputField.LineType.MultiLineNewline;

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
        }

        public void SetSingleLine()
        {
            input.GetComponent<PanCancelInputField>().lineType = UnityEngine.UI.InputField.LineType.SingleLine;
        }

        public override string GetText()
        {
            if (text == null) return "";
            return text.GetComponent<UnityEngine.UI.Text>().text;
        }

        public override void SetButton(UnityEngine.Events.UnityAction call)
        {
            buttonCall = call;
            if (text != null)
            {
                text.GetComponent<PanCancelInputField>().onEndEdit.AddListener(delegate { buttonCall(); });
            }
        }
    }
}
