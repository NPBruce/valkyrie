using UnityEngine;

namespace Assets.Scripts.UI
{
    public class UIElementScrollHorizontal : UIElementScrollVertical
    {
        public UIElementScrollHorizontal(string t = "") : base(t)
        {
        }

        protected override void CreateBG(Transform parent)
        {
            base.CreateBG(parent);
            scrollBar.GetComponent<UnityEngine.UI.Scrollbar>().direction = UnityEngine.UI.Scrollbar.Direction.LeftToRight;
            scrollBG.GetComponent<UnityEngine.UI.ScrollRect>().verticalScrollbar = null;
            scrollBG.GetComponent<UnityEngine.UI.ScrollRect>().horizontalScrollbar = scrollBar.GetComponent<UnityEngine.UI.Scrollbar>();
            scrollBG.GetComponent<UnityEngine.UI.ScrollRect>().vertical = false;
            scrollBG.GetComponent<UnityEngine.UI.ScrollRect>().horizontal = true;
        }

        public override void SetLocationPixels(float x, float y, float width, float height)
        {
            base.SetLocationPixels(x, y, width, height);

            RectTransform scrollBGTrans = scrollBG.GetComponent<RectTransform>();
            scrollBGTrans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, height - UIScaler.GetPixelsPerUnit());
            scrollBGTrans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, width);

            RectTransform scrollTrans = scrollArea.GetComponent<RectTransform>();
            scrollTrans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, height - UIScaler.GetPixelsPerUnit());
            scrollTrans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, width);

            RectTransform scrollBarTrans = scrollBar.GetComponent<RectTransform>();
            scrollBarTrans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, UIScaler.GetPixelsPerUnit());
            scrollBarTrans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, width);
        }

        public override void SetScrollSize(float size)
        {
            float width = size * UIScaler.GetPixelsPerUnit();
            if (width < scrollBar.GetComponent<RectTransform>().rect.width)
            {
                width = scrollBar.GetComponent<RectTransform>().rect.width;
            }
            scrollArea.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, width);
        }

        public override void SetScrollPosition(float pos)
        {
            scrollArea.GetComponent<RectTransform>().anchoredPosition = new Vector2(pos + scrollArea.GetComponent<RectTransform>().rect.x, scrollArea.GetComponent<RectTransform>().anchoredPosition.y);
        }

        public override float GetScrollPosition()
        {
            return scrollArea.GetComponent<RectTransform>().anchoredPosition.x - scrollArea.GetComponent<RectTransform>().rect.x;
        }
    }
}
