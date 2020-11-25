using System;
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

        public UIElementBorder(UIElement element, Color color, float thickness = 0.05f)
        {
            CreateBorder(element.GetTransform(), element.GetRectTransform(), element.GetTag(), color, thickness);
        }

        public UIElementBorder(Transform transform, RectTransform rectTrans, string tag, Color color)
        {
            CreateBorder(transform, rectTrans, tag, color);
        }

        private void CreateBorder(Transform transform, RectTransform rectTrans, string tag, Color color, float thickness = 0.05f)
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
            float thick = thickness * UIScaler.GetPixelsPerUnit();

            // Bottom line
            bLine[0].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, thick);
            bLine[0].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -thick, rectTrans.rect.width);
            // Top line
            bLine[1].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -thick, thick);
            bLine[1].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -thick, rectTrans.rect.width);
            
            // Left line
            bLine[2].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -thick, rectTrans.rect.height + thick);
            bLine[2].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -thick, thick);
            // Right line
            bLine[3].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -thick, rectTrans.rect.height + thick);
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
