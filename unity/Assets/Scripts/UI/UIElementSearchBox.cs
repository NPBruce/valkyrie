using UnityEngine;
using Assets.Scripts.Content;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class UIElementSearchBox : UIElementEditable
    {
        private UIElement searchIcon;

        public UIElementSearchBox(string t = "", Transform parent = null) : base(t, parent)
        {
        }

        public override void SetText(string content, Color textColor)
        {
            base.SetText(content, textColor);

            if (input != null)
            {
                InitSearchIcon();
                
                UnityEngine.UI.InputField inputField = input.GetComponent<UnityEngine.UI.InputField>();
                if (inputField != null)
                {
                    inputField.onValueChanged.RemoveAllListeners();
                    inputField.onValueChanged.AddListener(delegate { UpdateIconState(); });
                }
            }
            
            UpdateIconState();
        }

        private void InitSearchIcon()
        {
            if (searchIcon == null && input != null)
            {
                // Create search icon on the right side
                searchIcon = new UIElement(tag, input.transform);
                
                // Position it on the right
                RectTransform iconRect = searchIcon.GetRectTransform();
                // Anchor to right
                iconRect.anchorMin = new Vector2(1, 0);
                iconRect.anchorMax = new Vector2(1, 1);
                iconRect.pivot = new Vector2(1, 0.5f);
                
                float buttonWidth = input.GetComponent<RectTransform>().rect.height;
                // Fallback if height is not set yet
                if (buttonWidth == 0) buttonWidth = 1.5f * UIScaler.GetPixelsPerUnit();

                iconRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, buttonWidth);
                iconRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, iconRect.parent.GetComponent<RectTransform>().rect.height);
                
                iconRect.anchorMin = new Vector2(1f, 0.5f);
                iconRect.anchorMax = new Vector2(1f, 0.5f);
                iconRect.pivot = new Vector2(1f, 0.5f);
                
                float iconSize = buttonWidth * 0.8f;
                iconRect.sizeDelta = new Vector2(iconSize, iconSize);
                iconRect.anchoredPosition = new Vector2(-0.1f * buttonWidth, 0);

                // Now adjust the text padding so it doesn't overlap the icon
                if (text != null)
                {
                    RectTransform textRect = text.GetComponent<RectTransform>();
                    float currentOffsetMaxY = textRect.offsetMax.y;
                    textRect.offsetMax = new Vector2(-buttonWidth, currentOffsetMaxY);
                }
            }
        }



        private void UpdateIconState()
        {
            if (searchIcon == null) return;

            string currentText = "";
            UnityEngine.UI.InputField inputField = null;
            if (input != null) inputField = input.GetComponent<UnityEngine.UI.InputField>();
            
            if (inputField != null) currentText = inputField.text;
            else if (text != null) currentText = text.GetComponent<UnityEngine.UI.Text>().text;

            if (string.IsNullOrEmpty(currentText))
            {
                // Show Search Icon
                Texture2D searchTex = Resources.Load("sprites/search") as Texture2D;
                searchIcon.SetImage(searchTex);
                // Search trigger removed as per user request
                // searchIcon.SetButton(buttonCall); 
            }
            else
            {
                // Show Close Icon
                Texture2D closeTex = Resources.Load("sprites/close") as Texture2D;
                // Fallback to search if close missing (though user said it is there)
                if(closeTex == null) closeTex = Resources.Load("sprites/search") as Texture2D;
                
                searchIcon.SetImage(closeTex);
                searchIcon.SetButton(delegate { 
                    SetText(""); 
                    if(buttonCall != null) buttonCall(); 
                });
            }
        }

        public override void SetButton(UnityEngine.Events.UnityAction call)
        {
            buttonCall = call;
            if (input != null)
            {
                input.GetComponent<PanCancelInputField>().onEndEdit.AddListener(delegate {
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        if (buttonCall != null) buttonCall();
                    }
                });
            }
            UpdateIconState();
        }
    }
}
