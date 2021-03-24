using UnityEngine;

namespace Assets.Scripts.UI
{
    public sealed class UITtsSpeakButton : UIElement
    {
        public UITtsSpeakButton(UIElement textElement) : base(textElement.GetTag(), textElement.GetTransform())
        {
            var transform = GetRectTransform();
            var size = UIScaler.GetPixelsPerUnit() * 1.3f;
            
            transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 1f, size);
            transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 1f, size);
            SetText("\u25B6");
            SetFontSize(UIScaler.GetSmallFont());
            SetButton(() =>
            {
                GoogleTTSClient.SpeakText(textElement.GetText());
            });
        }
    }
}