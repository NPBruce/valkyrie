using UnityEngine;

namespace Assets.Scripts.UI
{
    public class UIElementCropped : UIElement
    {
        GameObject mask = null;
        float x_mask = 0f;
        float y_mask = 0f;
        float width_mask = 0f;
        float height_mask = 0f;

        /// <summary>
        /// Construct a UI element with options tag name and parent.</summary>
        /// <param name="t">Unity tag name for the element, cannot be changed after construction, defaults to "dialog".</param>
        /// <param name="parent">Parent transform, cannot be changed after construction, defaults to the UI Panel.</param>
        public UIElementCropped(string t = "", Transform parent = null)
        {
            if (t.Length > 0) tag = t;
            CreateMask(parent);
        }

        /// <summary>
        /// Construct a UI element with parent.</summary>
        /// <param name="parent">Parent transform, cannot be changed after construction.</param>
        public UIElementCropped(Transform parent) : base(parent)
        {
            CreateMask(parent);
        }

        /// <summary>
        /// Internal method called by constructor to set up base GameObject.</summary>
        protected virtual void CreateMask(Transform parent)
        {
            mask = new GameObject("UIMASK");
            mask.tag = tag;
            mask.AddComponent<UnityEngine.UI.RectMask2D>();
            if (parent == null) parent = Game.Get().uICanvas.transform;
            mask.transform.SetParent(parent);
            // destroy base bg and create a new one
            Object.Destroy(bg);
            base.CreateBG(mask.transform);
        }

        /// <summary>
        /// Set UI location in pixels.</summary>
        /// <param name="x">Offset from the left of the screen.</param>
        /// <param name="y">Offset from the top of the screen.</param>
        /// <param name="width">Horizontal size.</param>
        /// <param name="height">Vertical size.</param>
        /// <remarks>
        /// Cannot be changed after ***FIXME***.</remarks>
        override public void SetLocationPixels(float x, float y, float width, float height)
        {
            RectTransform transBg = mask.GetComponent<RectTransform>();
            transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y, height);
            transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, x, width);

            //save x and y if we crop before screen refresh
            x_mask = x;
            y_mask = y;
            width_mask = width;
            height_mask = height;

            base.SetLocationPixels(0, 0, width, height);
        }

        /// <summary>
        /// Set horizontal crop ratio</summary>
        /// <param name="percent">0 hidden, 1 full</param>
        public void CropHorizontal(float percent)
        {
            RectTransform transBg = mask.GetComponent<RectTransform>();
            transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, x_mask, width_mask * percent);
        }

        /// <summary>
        /// Set horizontal crop ratio</summary>
        /// <param name="percent">0 hidden, 1 full</param>
        public void CropVertical(float percent)
        {
            RectTransform transBg = mask.GetComponent<RectTransform>();
            transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y_mask, height_mask * percent);
        }
    }
}
