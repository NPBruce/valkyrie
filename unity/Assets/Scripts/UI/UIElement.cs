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

        protected static float textPaddingDefault = 0.25f;

        protected float textPadding = textPaddingDefault;

        // Used for calculating text size
        protected static GameObject textWidthObj;
        protected static GameObject textHeightObj;

        public UIElement(string t = "", Transform parent = null)
        {
            if (t.Length > 0) tag = t;
            CreateBG(parent);
        }

        public UIElement(Transform parent)
        {
            CreateBG(parent);
        }

        public Transform GetTransform()
        {
            return bg.transform;
        }

        public RectTransform GetRectTransform()
        {
            return bg.GetComponent<RectTransform>();
        }

        public string GetTag()
        {
            return tag;
        }

        protected void CreateBG(Transform parent)
        {
            bg = new GameObject("UIBG");
            bg.tag = tag;
            UnityEngine.UI.Image uiImage = bg.AddComponent<UnityEngine.UI.Image>();
            // default color
            uiImage.color = new Color(0, 0, 0, (float)0.9);
            if (parent == null) parent = Game.Get().uICanvas.transform;
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

        public void SetText(StringKey content, Color textColor)
        {
            SetText(content.Translate(), textColor);
        }

        public void SetText(string content)
        {
            SetText(content, Color.white);
        }

        public void SetText(string content, Color textColor)
        {
            UnityEngine.UI.Text uiText = null;
            if (text == null)
            {
                text = new GameObject("UIBG");
                text.tag = tag;
                uiText = text.AddComponent<UnityEngine.UI.Text>();
                uiText.alignment = TextAnchor.MiddleCenter;
                uiText.font = Game.Get().gameType.GetFont();
                uiText.fontSize = UIScaler.GetSmallFont();
                text.transform.SetParent(bg.transform);
                RectTransform transform = text.GetComponent<RectTransform>();
                transform.anchorMin = Vector2.zero;
                transform.anchorMax = Vector2.one;
                transform.localPosition = Vector3.zero;
                transform.localScale = Vector3.one;
                transform.offsetMin = new Vector2(textPadding * UIScaler.GetPixelsPerUnit(), 0);
                transform.offsetMax = new Vector2(-textPadding * UIScaler.GetPixelsPerUnit(), 0);
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
            uiText.text = content;
        }

        public string GetText()
        {
            if (text == null) return "";
            return text.GetComponent<UnityEngine.UI.Text>().text;
        }

        public static float GetStringWidth(StringKey content)
        {
            return GetStringWidth(content.Translate());
        }

        public static float GetStringWidth(string content)
        {
            if (textWidthObj == null)
            {
                textWidthObj = new GameObject("TextSizing");
                textWidthObj.AddComponent<UnityEngine.UI.Text>();
                RectTransform transform = textWidthObj.GetComponent<RectTransform>();
                transform.offsetMax = new Vector2(20000, 20000);
                textWidthObj.GetComponent<UnityEngine.UI.Text>().font = Game.Get().gameType.GetFont();
                textWidthObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            }
            textWidthObj.GetComponent<UnityEngine.UI.Text>().text = content;
            float width = (textWidthObj.GetComponent<UnityEngine.UI.Text>().preferredWidth / UIScaler.GetPixelsPerUnit()) +(textPaddingDefault * 2);
            return width;
        }
    }
}
