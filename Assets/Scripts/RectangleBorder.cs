using UnityEngine;
using System.Collections;

public class RectangleBorder{

    public GameObject[] bLine;

    public void SetTag(string tag)
    {
        foreach (GameObject go in bLine)
        {
            go.tag = tag;
        }
    }

    public RectangleBorder(Transform t, Color c, Vector2 size)
    {
        Create(t, c, size, "");
    }

    public RectangleBorder(Transform t, Color c, Vector2 size, string tag)
    {
        Create(t, c, size, tag);
    }

    public void Create(Transform t, Color c, Vector2 size, string tag)
    {

        // Create objects
        bLine = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            bLine[i] = new GameObject("Border" + i);
            if (!tag.Equals(""))
            {
                bLine[i].tag = "dialog";
            }
            bLine[i].AddComponent<RectTransform>();
            bLine[i].AddComponent<CanvasRenderer>();
            bLine[i].transform.SetParent(t);
            UnityEngine.UI.Image blImage = bLine[i].AddComponent<UnityEngine.UI.Image>();
            blImage.color = c;
        }

        float thick = 0.05f * UIScaler.GetPixelsPerUnit();

        bLine[0].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, thick);
        bLine[0].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -thick, size.x * UIScaler.GetPixelsPerUnit());

        bLine[1].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -thick, thick);
        bLine[1].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -thick, size.x * UIScaler.GetPixelsPerUnit());

        bLine[2].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -thick, size.y* UIScaler.GetPixelsPerUnit());
        bLine[2].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -thick, thick);

        bLine[3].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -thick, size.y* UIScaler.GetPixelsPerUnit());
        bLine[3].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, thick);
	}
}
