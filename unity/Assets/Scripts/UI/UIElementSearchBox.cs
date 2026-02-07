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

            if (searchIcon == null && input != null)
            {
                // Create search icon on the right side
                // We attach to 'input' which is the main container for the editable
                searchIcon = new UIElement(tag, input.transform);
                Texture2D searchTex = Resources.Load("sprites/search") as Texture2D;
                searchIcon.SetImage(searchTex);
                
                // Position it on the right
                RectTransform iconRect = searchIcon.GetRectTransform();
                // Anchor to right
                iconRect.anchorMin = new Vector2(1, 0);
                iconRect.anchorMax = new Vector2(1, 1);
                iconRect.pivot = new Vector2(1, 0.5f);
                
                // Width of icon approx 1.2 units (similar to previous implementation)
                // Height 1.2 units centered.
                // But SetLocationPixels logic in UIElement might conflict if we don't manage it carefully.
                // Better to simple inset:
                float iconSize = 1.2f * UIScaler.GetPixelsPerUnit();
                float padding = 0.15f * UIScaler.GetPixelsPerUnit();
                
                iconRect.sizeDelta = new Vector2(iconSize, iconSize);
                iconRect.anchoredPosition = new Vector2(-padding - (iconSize / 2f) + (iconSize / 2f), 0); 
                // Wait, if pivot x is 1. Position x=0 means right edge.
                // layout: [ ... text ... |  pad | icon | pad | ]
                // If we want icon to be 1.5 units wide reserved space?
                
                // Let's use specific insets.
                // Right edge is 0.
                // We want icon to be 1.5 units wide "button" area conceptually?
                // The previous code had 1.5 width button. Icon 1.2 inside it.
                
                float buttonWidthUnits = 1.5f;
                float buttonWidth = buttonWidthUnits * UIScaler.GetPixelsPerUnit();
                
                iconRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, buttonWidth);
                iconRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, iconRect.parent.GetComponent<RectTransform>().rect.height); // Full height?
                
                // Re-setting image to be centered/aspect fit?
                // UIElement.SetImage sets AspectRatioFitter.
                // Let's just manually position the icon rect to be 1.2x1.2 centered in the right 1.5 area.
                
                // Actually, UIElement creates a BG object.
                // Let's just use the rect and force it.
                iconRect.anchorMin = new Vector2(1f, 0.5f);
                iconRect.anchorMax = new Vector2(1f, 0.5f);
                iconRect.pivot = new Vector2(1f, 0.5f);
                iconRect.sizeDelta = new Vector2(1.2f * UIScaler.GetPixelsPerUnit(), 1.2f * UIScaler.GetPixelsPerUnit());
                iconRect.anchoredPosition = new Vector2(-0.15f * UIScaler.GetPixelsPerUnit(), 0);

                // Now adjust the text padding so it doesn't overlap the icon
                if (text != null)
                {
                    RectTransform textRect = text.GetComponent<RectTransform>();
                    float currentOffsetMaxY = textRect.offsetMax.y;
                    // Increase right padding (negative value)
                    // We need to reserve 1.5 units
                    textRect.offsetMax = new Vector2(-1.5f * UIScaler.GetPixelsPerUnit(), currentOffsetMaxY);
                }
            }
        }

        public override void SetButton(UnityEngine.Events.UnityAction call)
        {
            base.SetButton(call);
            if (searchIcon != null)
            {
                searchIcon.SetButton(call);
            }
        }
    }
}
