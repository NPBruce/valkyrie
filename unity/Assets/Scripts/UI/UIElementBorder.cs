using UnityEngine;
using Assets.Scripts.Content;

namespace Assets.Scripts.UI
{
    class UIElementBorder
    {
        public UIElementBorder(UIElement element)
        {
            CreateBorder(element, Color.white);
        }

        public UIElementBorder(UIElement element, Color color)
        {
            CreateBorder(element, color);
        }

        private void CreateBorder(UIElement element, Color color)
        {
            GameObject[] bLine = new GameObject[4];

            // create 4 lines
            for (int i = 0; i < 4; i++)
            {
                bLine[i] = new GameObject("Border" + i);
                bLine[i].tag = element.GetTag();
                bLine[i].AddComponent<UnityEngine.UI.Image>().color = color;
                bLine[i].transform.SetParent(element.GetTransform());
            }

            // Set the thickness of the lines
            float thick = 0.05f * UIScaler.GetPixelsPerUnit();

            bLine[0].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, thick);
            bLine[0].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -thick, element.GetRectTransform().rect.width);

            bLine[1].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -thick, thick);
            bLine[1].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -thick, element.GetRectTransform().rect.width);

            bLine[2].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -thick, element.GetRectTransform().rect.height);
            bLine[2].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -thick, thick);

            bLine[3].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -thick, element.GetRectTransform().rect.height);
            bLine[3].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, thick);
        }
    }
}
