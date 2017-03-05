using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// Class used to draw rectangular borders around things
public class RectangleBorder{

    // Borders are made up of 4 lines
    public GameObject[] bLine;

    // Store resulting image to change color dinamically
    private Image[] image;

    /// <summary>
    /// Sets current color of all lines in rectangle
    /// </summary>
    public Color color
    {
        get
        {
            return image[0].color;
        }
        set
        {
            for (int side = 0; side < 4; side++)
            {
                image[side].color = value;
            }            
        }
    }
    

    // Used to set the unity tag for the border
    public void SetTag(string tag)
    {
        foreach (GameObject go in bLine)
        {
            go.tag = tag;
        }
    }

    // Create a border
    // t: unity transform to use as reference
    // c: colour
    // size: size in scale units
    public RectangleBorder(Transform t, Color c, Vector2 size)
    {
        Create(t, c, size, "");
    }

    // Create a border, as above with:
    // tag: Tag to apply
    public RectangleBorder(Transform t, Color c, Vector2 size, string tag)
    {
        Create(t, c, size, tag);
    }

    // Internal function to draw from constructors
    public void Create(Transform t, Color c, Vector2 size, string tag)
    {
        bLine = new GameObject[4];
        image = new Image[4];
        // create 4 lines
        for (int i = 0; i < 4; i++)
        {
            bLine[i] = new GameObject("Border" + i);
            // FIXME this looks wrong
            if (!tag.Equals(""))
            {
                bLine[i].tag = "dialog";
            }
            bLine[i].AddComponent<RectTransform>();
            bLine[i].AddComponent<CanvasRenderer>();
            bLine[i].transform.SetParent(t);
            image[i] = bLine[i].AddComponent<UnityEngine.UI.Image>();
            image[i].color = c;
        }

        // Set the thickness of the lines
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
