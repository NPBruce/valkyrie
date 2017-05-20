using UnityEngine;
using Assets.Scripts.Content;

namespace Assets.Scripts.UI
{
    /*
     * - tag
     * - parent
     * 
     * background
     *  - color
     *  - position
     *  - size
     *  - image
     *  
     * text
     *  - content
     *  - font
     *  - size
     *  - color
     *  - alignment
     *  - padding
     * 
     * button
     *  - call
     * 
     * border
     *  - color
     * 
     */

    class UIElement
    {
        // The button itself, unity object
        protected GameObject text;
        // The background, unity object
        protected GameObject bg;
        // Border for the button
        protected RectangleBorder border;

        protected string tag = Game.DIALOG;

        protected float textPadding = 0.1f;

        public UIElement(string t = "", Transform parent = null)
        {
            if (t.Length > 0) tag = t;
            CreateBG(parent);
        }

        public UIElement(Transform parent = null)
        {
            CreateBG(parent);
        }

        protected void CreateBG(Transform parent)
        {
            bg = new GameObject("UIBG");
            bg.tag = tag;
            UnityEngine.UI.Image uiImage = bg.AddComponent<UnityEngine.UI.Image>();
            // default color
            uiImage.color = new Color(0, 0, 0, (float)0.9);
            bg.transform.SetParent(parent);
        }

        public void SetBGColor(Color c)
        {
            bg.GetComponent<UnityEngine.UI.Image>().color = c;
        }

        public void SetLocation(float x, float y, float width, float height)
        {
            SetLocationPixels(UIScaler.GetPixelsPerUnit() * x, UIScaler.GetPixelsPerUnit() * y, UIScaler.GetPixelsPerUnit() * width, UIScaler.GetPixelsPerUnit() * height);
        }

        public void SetLocationPixels(float x, float y, float width, float height)
        {
            RectTransform transBg = bg.GetComponent<RectTransform>();
            transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y, height);
            transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, x, width);
        }

        public void SetButton(UnityEngine.Events.UnityAction call)
        {
            UnityEngine.UI.Button uiButton = bg.AddComponent<UnityEngine.UI.Button>();
            uiButton.interactable = true;
            uiButton.onClick.AddListener(call);
            if (text != null)
            {
                uiButton = text.AddComponent<UnityEngine.UI.Button>();
                uiButton.interactable = true;
                uiButton.onClick.AddListener(call);
            }
        }

        public void SetTextPadding(float pad)
        {
            textPadding = pad;
        }

        public void SetText(StringKey content)
        {
            SetText(content.Translate(), Color.white);
        }

        public void SetText(string content)
        {
            SetText(content, Color.white);
        }

        public void SetText(string content, Color textColor)
        {
            text = new GameObject("UIBG");
            text.tag = tag;
            UnityEngine.UI.Text uiText = text.AddComponent<UnityEngine.UI.Text>();
            uiText.color = textColor;
            uiText.alignment = TextAnchor.MiddleCenter;
            uiText.font = Game.Get().gameType.GetFont();
            uiText.fontSize = UIScaler.GetSmallFont();
            if (textColor.Equals(Color.black))
            {
                uiText.material = (Material)Resources.Load("Fonts/FontMaterial");
            }
            else
            {
                uiText.material = uiText.font.material;
            }
            uiText.text = content;
            text.transform.SetParent(bg.transform);
            RectTransform transform = text.GetComponent<RectTransform>();
            transform.sizeDelta = Vector3.zero;
            transform.anchorMin = Vector2.zero;
            transform.anchorMax = Vector2.one;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
        }
    }
}
