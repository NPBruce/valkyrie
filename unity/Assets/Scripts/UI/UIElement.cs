using UnityEngine;
using Assets.Scripts.Content;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// UIElement is the base class for drawing UI elements.</summary>
    /// <remarks>
    /// UIElements can display solid colors, images and text.  It can also act as a button.</remarks>
    public class UIElement
    {
        // The button itself, unity object
        protected GameObject text;
        // The background, unity object
        protected GameObject bg;

        protected string tag = Game.DIALOG;

        protected static float textPaddingDefault = 0.25f;

        protected float textPadding = textPaddingDefault;

        // Used for calculating text size
        protected static GameObject textWidthObj;
        protected static GameObject textHeightObj;

        protected UnityEngine.Events.UnityAction buttonCall;

        /// <summary>
        /// Construct a UI element with options tag name and parent.</summary>
        /// <param name="t">Unity tag name for the element, cannot be changed after construction, defaults to "dialog".</param>
        /// <param name="parent">Parent transform, cannot be changed after construction, defaults to the UI Panel.</param>
        public UIElement(string t = "", Transform parent = null)
        {
            if (t.Length > 0) tag = t;
            CreateBG(parent);
        }

        /// <summary>
        /// Construct a UI element with parent.</summary>
        /// <param name="parent">Parent transform, cannot be changed after construction.</param>
        public UIElement(Transform parent)
        {
            CreateBG(parent);
        }

        /// <summary>
        /// The tranform that should be used for sub elements.</summary>
        /// <returns>
        /// The background Transform.</returns>
        public Transform GetTransform()
        {
            return bg.transform;
        }

        /// <summary>
        /// The base RectTransform for the UI element.</summary>
        /// <returns>
        /// The background RectTransform.</returns>
        public RectTransform GetRectTransform()
        {
            return bg.GetComponent<RectTransform>();
        }

        /// <summary>
        /// The tag for this UI element.</summary>
        /// <returns>
        /// The Unity tag as a string.</returns>
        public string GetTag()
        {
            return tag;
        }

        /// <summary>
        /// Internal method called by constructor to set up base GameObject.</summary>
        protected virtual void CreateBG(Transform parent)
        {
            bg = new GameObject("UIBG");
            bg.tag = tag;
            UnityEngine.UI.Image uiImage = bg.AddComponent<UnityEngine.UI.Image>();
            // default color
            uiImage.color = new Color(0, 0, 0, (float)0.9);
            if (parent == null) parent = Game.Get().uICanvas.transform;
            bg.transform.SetParent(parent);
        }

        /// <summary>
        /// Set a background color.</summary>
        /// <param name="c">Color to use.</param>
        /// <remarks>
        /// Can also be used to change image render color, is reset to white when image is set.</remarks>
        public void SetBGColor(Color c)
        {
            bg.GetComponent<UnityEngine.UI.Image>().color = c;
        }

        /// <summary>
        /// Set a background image.</summary>
        /// <param name="texture">Texture2D to use.</param>
        /// <remarks>
        /// Will also set the background color to white.</remarks>
        public void SetImage(Texture2D texture)
        {
            if (texture == null) return;
            SetBGColor(Color.white);
            bg.GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 1);
        }

        /// <summary>
        /// Set a background image.</summary>
        /// <param name="sprite">Sprite to use.</param>
        /// <remarks>
        /// Will also set the background color to white.</remarks>
        public void SetImage(Sprite sprite)
        {
            if (sprite == null) return;
            SetBGColor(Color.white);
            bg.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
        }


        /// <summary>
        /// Set UI location in UIscaler units.</summary>
        /// <param name="x">Offset from the left of the screen.</param>
        /// <param name="y">Offset from the top of the screen.</param>
        /// <param name="width">Horizontal size.</param>
        /// <param name="height">Vertical size.</param>
        /// <remarks>
        /// Cannot be changed after ***FIXME***.</remarks>
        public void SetLocation(float x, float y, float width, float height)
        {
            SetLocationPixels(UIScaler.GetPixelsPerUnit() * x, UIScaler.GetPixelsPerUnit() * y, UIScaler.GetPixelsPerUnit() * width, UIScaler.GetPixelsPerUnit() * height);
        }

        /// <summary>
        /// Set UI location in pixels.</summary>
        /// <param name="x">Offset from the left of the screen.</param>
        /// <param name="y">Offset from the top of the screen.</param>
        /// <param name="width">Horizontal size.</param>
        /// <param name="height">Vertical size.</param>
        /// <remarks>
        /// Cannot be changed after ***FIXME***.</remarks>
        public virtual void SetLocationPixels(float x, float y, float width, float height)
        {
            RectTransform transBg = bg.GetComponent<RectTransform>();
            transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y, height);
            transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, x, width);
        }

        /// <summary>
        /// Add button press event to element.</summary>
        /// <param name="call">Function to call on button press.</param>
        /// <remarks>
        /// Adds button properties to background area and text.</remarks>
        public virtual void SetButton(UnityEngine.Events.UnityAction call)
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
            buttonCall = call;
        }

        /// <summary>
        /// Set horizontal text padding.</summary>
        /// <param name="pad">Padding in UIScaler units.</param>
        /// <remarks>
        /// Must be called before SetText.</remarks>
        public void SetTextPadding(float pad)
        {
            textPadding = pad;
        }

        /// <summary>
        /// Set white element text.</summary>
        /// <param name="content">StringKey to be translated.</param>
        public void SetText(StringKey content)
        {
            SetText(content.Translate(), Color.white);
        }

        /// <summary>
        /// Set element text.</summary>
        /// <param name="content">StringKey to be translated.</param>
        /// <param name="textColor">Color to display text.</param>
        public void SetText(StringKey content, Color textColor)
        {
            SetText(content.Translate(), textColor);
        }

        /// <summary>
        /// Set white element text.</summary>
        /// <param name="content">Text to display.</param>
        public void SetText(string content)
        {
            SetText(content, Color.white);
        }

        /// <summary>
        /// Set element text.</summary>
        /// <param name="content">Text to display.</param>
        /// <param name="textColor">Color to display text.</param>
        public virtual void SetText(string content, Color textColor)
        {
            UnityEngine.UI.Text uiText = null;
            if (text == null)
            {
                text = new GameObject("UIText");
                text.tag = tag;
                uiText = text.AddComponent<UnityEngine.UI.Text>();
                uiText.alignment = TextAnchor.MiddleCenter;
                uiText.verticalOverflow = VerticalWrapMode.Overflow;
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

                if (buttonCall != null)
                {
                    UnityEngine.UI.Button uiButton = text.AddComponent<UnityEngine.UI.Button>();
                    uiButton.interactable = true;
                    uiButton.onClick.AddListener(buttonCall);
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
            uiText.text = content;
        }

        /// <summary>
        /// Set font size.</summary>
        /// <param name="size">Size in pixels.</param>
        /// <remarks>
        /// Must be called after SetText.</remarks>
        public virtual void SetFontSize(int size)
        {
            text.GetComponent<UnityEngine.UI.Text>().fontSize = size;
        }

        /// <summary>
        /// Set font.</summary>
        /// <param name="font">Font to use.</param>
        /// <remarks>
        /// Must be called after SetText.</remarks>
        public virtual void SetFont(Font font)
        {
            text.GetComponent<UnityEngine.UI.Text>().font = font;
        }

        /// <summary>
        /// Set text alignment.</summary>
        /// <param name="align">Text alignment.</param>
        /// <remarks>
        /// Must be called after SetText.</remarks>
        public virtual void SetTextAlignment(TextAnchor align)
        {
            text.GetComponent<UnityEngine.UI.Text>().alignment = align;
        }

        /// <summary>
        /// Get the UIElement display text.</summary>
        /// <returns>
        /// The display text as a string or "" if not set.</returns>
        public virtual string GetText()
        {
            if (text == null) return "";
            return text.GetComponent<UnityEngine.UI.Text>().text;
        }

        /// <summary>
        /// Is there any display text?</summary>
        /// <returns>
        /// True if text not set or empty</returns>
        public virtual bool Empty()
        {
            if (text == null) return true;
            return text.GetComponent<UnityEngine.UI.Text>().text.Length == 0;
        }

        /// <summary>
        /// Get the length of a text string at small size with standard font.</summary>
        /// <param name="content">Text to translate.</param>
        /// <returns>
        /// The size of the text in UIScaler units.</returns>
        public static float GetStringWidth(StringKey content)
        {
            return GetStringWidth(content.Translate());
        }

        /// <summary>
        /// Get the length of a text string at small size with standard font.</summary>
        /// <param name="content">Text to measure.</param>
        /// <returns>
        /// The size of the text in UIScaler units.</returns>
        public static float GetStringWidth(string content)
        {
            return GetStringWidth(content, UIScaler.GetSmallFont());
        }

        /// <summary>
        /// Get the length of a text string at small size with standard font.</summary>
        /// <param name="content">Text to measure.</param>
        /// <param name="fontSize">Size of font.</param>
        /// <returns>
        /// The size of the text in UIScaler units.</returns>
        public static float GetStringWidth(string content, int fontSize)
        {
            if (textWidthObj == null)
            {
                textWidthObj = new GameObject("TextSizing");
                textWidthObj.AddComponent<UnityEngine.UI.Text>();
                RectTransform transform = textWidthObj.GetComponent<RectTransform>();
                transform.offsetMax = new Vector2(20000, 20000);
                textWidthObj.GetComponent<UnityEngine.UI.Text>().font = Game.Get().gameType.GetFont();
                textWidthObj.GetComponent<UnityEngine.UI.Text>().fontSize = fontSize;
            }
            textWidthObj.GetComponent<UnityEngine.UI.Text>().text = content;
            float width = (textWidthObj.GetComponent<UnityEngine.UI.Text>().preferredWidth / UIScaler.GetPixelsPerUnit()) + (textPaddingDefault * 2);
            return width;
        }

        /// <summary>
        /// Get the height of a text box of fixed width with a text string at small size with standard font and standard padding.</summary>
        /// <param name="content">Text to translate.</param>
        /// <param name="width">Width of the text box in UIScaler units.</param>
        /// <returns>
        /// The required text box height in UIScaler units.</returns>
        public static float GetStringHeight(StringKey content, float width)
        {
            return GetStringHeight(content.Translate(), width);
        }

        /// <summary>
        /// Get the height of a text box of fixed width with a text string at small size with standard font and standard padding.</summary>
        /// <param name="content">Text to measure.</param>
        /// <param name="width">Width of the text box in UIScaler units.</param>
        /// <returns>
        /// The required text box height in UIScaler units.</returns>
        public static float GetStringHeight(string content, float width)
        {
            if (textHeightObj == null)
            {
                textHeightObj = new GameObject("TextSizing");
                textHeightObj.AddComponent<UnityEngine.UI.Text>();
                RectTransform transform = textHeightObj.GetComponent<RectTransform>();
                transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 6000 * UIScaler.GetPixelsPerUnit());
                transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, (width - (textPaddingDefault * 2)) * UIScaler.GetPixelsPerUnit());
                textHeightObj.GetComponent<UnityEngine.UI.Text>().font = Game.Get().gameType.GetFont();
                textHeightObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            }
            textHeightObj.GetComponent<UnityEngine.UI.Text>().text = content;
            return (textHeightObj.GetComponent<UnityEngine.UI.Text>().preferredHeight / UIScaler.GetPixelsPerUnit()) + (textPaddingDefault * 2);
        }

        /// <summary>
        /// Has the Unity object been destroyed?</summary>
        /// <returns>
        /// If the background Unity object has been destroyed.</returns>
        public bool ObjectDestroyed()
        {
            return bg == null;
        }

        /// <summary>
        /// Is the object at the specified location in UIScaler units</summary>
        /// <param name="x">x location to test</param>
        /// <param name="y">y location to test</param>
        /// <returns>If at that location</returns>
        public bool AtLocation(float x, float y)
        {
            return AtLocationPixels(x * UIScaler.GetPixelsPerUnit(), y * UIScaler.GetPixelsPerUnit());
        }

        /// <summary>
        /// Is the object at the specified location in pixels</summary>
        /// <param name="x">x location to test</param>
        /// <param name="y">y location to test</param>
        /// <returns>If at that location</returns>
        public bool AtLocationPixels(float x, float y)
        {
            RectTransform transBg = bg.GetComponent<RectTransform>();
            if (transBg.anchoredPosition.x - (transBg.sizeDelta.x / 2) > x) return false;
            if (-transBg.anchoredPosition.y - (transBg.sizeDelta.y / 2) > y) return false;
            if (transBg.anchoredPosition.x + (transBg.sizeDelta.x / 2) < x) return false;
            if (-transBg.anchoredPosition.y + (transBg.sizeDelta.y / 2) < y) return false;
            return true;
        }

        /// <summary>
        /// Get the amount of space for additional lines in the box, can be negative.</summary>
        /// <returns>Amount of vertical space, less padding value in UIScaler units</returns>
        protected float GetVerticalTextSpace()
        {
            float gap = text.GetComponent<RectTransform>().rect.height - text.GetComponent<UnityEngine.UI.Text>().preferredHeight;
            gap -= textPaddingDefault * 2f;
            return gap / UIScaler.GetPixelsPerUnit();
        }

        /// <summary>
        /// Is the amount of free vertical space within a specified range?</summary>
        /// <param name="min">Minimum space in UIScaler units</param>
        /// <param name="max">Maximum space in UIScaler units</param>
        protected bool VerticalTextSpaceInRange(float min, float max)
        {
            if (GetVerticalTextSpace() < min) return false;
            if (GetVerticalTextSpace() > max) return false;
            return true;
        }

        /// <summary>
        /// Is the height equal to text size plus padding?</summary>
        /// <param name="space">Additional vertical padding in UIScaler units</param>
        /// <returns>True if height matches</returns>
        public bool HeightAtTextPadding(float space = 0)
        {
            // Allow for floating point errors
            return VerticalTextSpaceInRange(space - 0.01f, space + 0.01f);
        }

        /// <summary>
        /// Set the UIElement to text height plus vertical padding</summary>
        /// <param name="space">Additional vertical padding in UIScaler units</param>
        /// <returns>New UIElement height in UIScaler units</returns>
        public virtual float HeightToTextPadding(float space = 0)
        {
            float newHeight = (text.GetComponent<UnityEngine.UI.Text>().preferredHeight / UIScaler.GetPixelsPerUnit()) + (textPaddingDefault * 2f) + space;
            if (newHeight < 1 + (textPaddingDefault * 2f))
            {
                newHeight = 1 + (textPaddingDefault * 2f);
            }
            float oldHeight = GetRectTransform().sizeDelta.y;
            float newHeightPixels = newHeight * UIScaler.GetPixelsPerUnit();
            GetRectTransform().anchoredPosition = new Vector2(GetRectTransform().anchoredPosition.x, GetRectTransform().anchoredPosition.y - ((newHeightPixels - oldHeight) / 2f));
            GetRectTransform().sizeDelta = new Vector2(GetRectTransform().sizeDelta.x, newHeightPixels);
            return newHeight;
        }
    }
}
