using UnityEngine;
using Assets.Scripts.Content;

namespace Assets.Scripts.UI
{
    public class UIElementEditable : UIElement
    {
        protected GameObject input;

        protected string lastText = "";
        protected GameObject placeholderObject;
        protected bool placeholderHiddenOnFocus = false;

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

                uiInput.onValueChanged.AddListener(delegate { OnValueChanged(); });
                
                // Add listeners to hide/show placeholder
                uiInput.onSelectEvent.AddListener(OnSelect);
                uiInput.onDeselectEvent.AddListener(OnDeselect);
            }

            this.SetColor(textColor);
            input.GetComponent<PanCancelInputField>().text = content;
            lastText = content;
        }

        // Set the color of this text
        public void SetColor(Color textColor)
        {
            UnityEngine.UI.Text uiText = text.GetComponent<UnityEngine.UI.Text>();
            uiText.color = textColor;
            if (textColor.Equals(Color.black))
            {
                uiText.material = (Material)Resources.Load("Fonts/FontMaterial");
            }
            else
            {
                uiText.material = uiText.font.material;
            }
        }

        public void SetSingleLine()
        {
            input.GetComponent<PanCancelInputField>().lineType = UnityEngine.UI.InputField.LineType.SingleLine;
        }

        public void SetMaxCharacters(int maxCharacter)
        {
            input.GetComponent<PanCancelInputField>().characterLimit = maxCharacter;
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

        public void SetPlaceholder(StringKey placeholder)
        {
            SetPlaceholder(placeholder.Translate());
        }

        public void SetPlaceholder(string placeholder)
        {
            if (input == null) return;
            PanCancelInputField uiInput = input.GetComponent<PanCancelInputField>();

            if (placeholderObject == null)
            {
                placeholderObject = new GameObject("UIPlaceholder");
                placeholderObject.tag = tag;
                UnityEngine.UI.Text uiText = placeholderObject.AddComponent<UnityEngine.UI.Text>();
                uiText.alignment = TextAnchor.MiddleCenter;
                uiText.font = Game.Get().gameType.GetFont();
                uiText.fontSize = UIScaler.GetSmallFont();
                uiText.fontStyle = FontStyle.Italic;
                uiText.horizontalOverflow = HorizontalWrapMode.Wrap;
                uiText.color = new Color(0.5f, 0.5f, 0.5f, 0.8f); // Grey color for placeholder

                placeholderObject.transform.SetParent(input.transform);
                RectTransform transform = placeholderObject.GetComponent<RectTransform>();
                transform.anchorMin = Vector2.zero;
                transform.anchorMax = Vector2.one;
                transform.localPosition = Vector3.zero;
                transform.localScale = Vector3.one;
                transform.offsetMin = new Vector2(textPadding * UIScaler.GetPixelsPerUnit(), 0);
                transform.offsetMax = new Vector2(-textPadding * UIScaler.GetPixelsPerUnit(), 0);

                uiInput.placeholder = uiText;
            }

            placeholderObject.GetComponent<UnityEngine.UI.Text>().text = placeholder;
            placeholderHiddenOnFocus = true;
        }

        private void OnValueChanged()
        {
            // Standard Unity placeholder logic handles hiding on typing, 
            // but we might want to enforce visibility rules if needed.
        }

        private void OnSelect()
        {
            if (placeholderHiddenOnFocus && placeholderObject != null)
            {
                placeholderObject.SetActive(false);
            }
        }

        private void OnDeselect()
        {
            if (placeholderHiddenOnFocus && placeholderObject != null && GetText().Length == 0)
            {
                placeholderObject.SetActive(true);
            }
        }
    }
}
