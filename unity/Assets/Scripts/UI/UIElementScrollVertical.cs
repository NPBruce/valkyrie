using UnityEngine;

namespace Assets.Scripts.UI
{
    public class UIElementScrollVertical : UIElement
    {
        protected GameObject scrollBG;
        protected GameObject scrollArea;
        protected GameObject scrollBar;
        protected GameObject scrollBarHandle;

        public UIElementScrollVertical(string t = "") : base(t)
        {
        }

        protected override void CreateBG(Transform parent)
        {
            base.CreateBG(parent);

            scrollBG = new GameObject("scrollBG");
            scrollBG.tag = tag;
            scrollBG.AddComponent<UnityEngine.UI.Image>().color = Color.clear;
            scrollBG.transform.SetParent(bg.transform);
            scrollBG.AddComponent<UnityEngine.UI.RectMask2D>();
            UnityEngine.UI.ScrollRect scrollRect = scrollBG.AddComponent<UnityEngine.UI.ScrollRect>();

            scrollArea = new GameObject("scroll");
            scrollArea.tag = tag;
            scrollArea.AddComponent<RectTransform>();
            scrollArea.transform.SetParent(scrollBG.transform);

            scrollBar = new GameObject("scrollbar");
            scrollBar.tag = tag;
            scrollBar.AddComponent<RectTransform>();
            scrollBar.transform.SetParent(bg.transform);
            UnityEngine.UI.Scrollbar scrollBarCmp = scrollBar.AddComponent<UnityEngine.UI.Scrollbar>();
            scrollBarCmp.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
            scrollRect.verticalScrollbar = scrollBarCmp;

            scrollBarHandle = new GameObject("scrollbarhandle");
            scrollBarHandle.tag = tag;
            scrollBarHandle.AddComponent<UnityEngine.UI.Image>();
            scrollBarHandle.GetComponent<UnityEngine.UI.Image>().color = new Color(0.7f, 0.7f, 0.7f);
            scrollBarHandle.transform.SetParent(scrollBar.transform);

            scrollBarCmp.handleRect = scrollBarHandle.GetComponent<RectTransform>();
            scrollBarCmp.handleRect.offsetMin = Vector2.zero;
            scrollBarCmp.handleRect.offsetMax = Vector2.zero;

            scrollRect.content = scrollArea.GetComponent<RectTransform>(); ;
            scrollRect.horizontal = false;
            scrollRect.scrollSensitivity = 40f;
        }

        public override void SetLocationPixels(float x, float y, float width, float height)
        {
            base.SetLocationPixels(x, y, width, height);

            RectTransform scrollBGTrans = scrollBG.GetComponent<RectTransform>();
            scrollBGTrans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, height);
            scrollBGTrans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, width - UIScaler.GetPixelsPerUnit());

            RectTransform scrollTrans = scrollArea.GetComponent<RectTransform>();
            scrollTrans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, height);
            scrollTrans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, width - UIScaler.GetPixelsPerUnit());

            RectTransform scrollBarTrans = scrollBar.GetComponent<RectTransform>();
            scrollBarTrans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, height);
            scrollBarTrans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, UIScaler.GetPixelsPerUnit());
        }

        public Transform GetScrollTransform()
        {
            return scrollArea.transform;
        }

        public virtual void SetScrollSize(float size)
        {
            float height = size * UIScaler.GetPixelsPerUnit();
            if (height < scrollBar.GetComponent<RectTransform>().rect.height)
            {
                height = scrollBar.GetComponent<RectTransform>().rect.height;
            }
            scrollArea.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, height);
        }

        public virtual void SetScrollPosition(float pos)
        {
            scrollArea.GetComponent<RectTransform>().anchoredPosition = new Vector2(scrollArea.GetComponent<RectTransform>().anchoredPosition.x, pos + scrollArea.GetComponent<RectTransform>().rect.y);
        }

        public virtual float GetScrollPosition()
        {
            return scrollArea.GetComponent<RectTransform>().anchoredPosition.y - scrollArea.GetComponent<RectTransform>().rect.y;
        }
    }
}
