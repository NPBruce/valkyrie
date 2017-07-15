using UnityEngine;
using Assets.Scripts.Content;

namespace Assets.Scripts.UI
{
    public class UIElementBorder
    {
        protected GameObject[] bLine;

        public UIElementBorder(UIElement element)
        {
            CreateBorder(element.GetTransform(), element.GetRectTransform(), element.GetTag(), Color.white);
        }

        public UIElementBorder(UIElement element, Color color)
        {
            CreateBorder(element.GetTransform(), element.GetRectTransform(), element.GetTag(), color);
        }

        public UIElementBorder(Transform transform, RectTransform rectTrans, string tag, Color color)
        {
            CreateBorder(transform, rectTrans, tag, color);
        }

        private void CreateBorder(Transform transform, RectTransform rectTrans, string tag, Color color)
        {
            bLine = new GameObject[4];

            // create 4 lines
            for (int i = 0; i < 4; i++)
            {
                bLine[i] = new GameObject("Border" + i);
                bLine[i].tag = tag;
                bLine[i].AddComponent<UnityEngine.UI.Image>().color = color;
                bLine[i].transform.SetParent(transform);
            }

            // Set the thickness of the lines
            float thick = 0.05f * UIScaler.GetPixelsPerUnit();

            bLine[0].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, thick);
            bLine[0].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -thick, rectTrans.rect.width);

            bLine[1].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -thick, thick);
            bLine[1].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -thick, rectTrans.rect.width);

            bLine[2].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -thick, rectTrans.rect.height);
            bLine[2].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -thick, thick);

            bLine[3].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -thick, rectTrans.rect.height);
            bLine[3].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, thick);
        }

        /// <summary>
        /// Set border color.</summary>
        /// <param name="color">Color to use.</param>
        public void SetColor(Color color)
        {
            foreach (GameObject line in bLine)
            {
                if (line != null)
                {
                    line.GetComponent<UnityEngine.UI.Image>().color = color;
                }
            }
        }
    }
}
