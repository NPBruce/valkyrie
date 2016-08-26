using UnityEngine;
using System.Collections;

public class DialogBox {

    public DialogBox(Vector2 location, Vector2 size, string text)
    {
        createDialog(location, size, text, Color.white, new Color(0, 0, 0, (float)0.7));
    }

    public DialogBox(Vector2 location, Vector2 size, string text, Color fgColour)
    {
        createDialog(location, size, text, fgColour, new Color(0, 0, 0, (float)0.7));
    }

    public DialogBox(Vector2 location, Vector2 size, string text, Color fgColour, Color bgColour)
    {
        createDialog(location, size, text, fgColour, bgColour);
    }

    void createDialog(Vector2 location, Vector2 size, string text, Color fgColour, Color bgColour)
    {
        // Create a object
        GameObject textObj = new GameObject("text" + text.Substring(0, 10));
        GameObject background = new GameObject("buttonBg" + text);
        // Mark it as dialog
        textObj.tag = "dialog";
        background.tag = "dialog";

        // Find the UI canvas
        Canvas[] canvii = GameObject.FindObjectsOfType<Canvas>();
        Canvas canvas = canvii[0];
        foreach (Canvas c in canvii)
        {
            if (c.name.Equals("UICanvas"))
            {
                canvas = c;
            }
        }
        background.transform.parent = canvas.transform;

        RectTransform transBg = background.AddComponent<RectTransform>();
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, location.y, size.y);
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, location.x, size.x);

        textObj.transform.parent = background.transform;

        RectTransform transBt = textObj.AddComponent<RectTransform>();
        transBt.SetParent(transBg);

        transBt.localPosition = Vector2.zero;
        transBt.localScale = transBg.localScale;
        transBt.sizeDelta = transBg.sizeDelta;

        textObj.AddComponent<CanvasRenderer>();
        background.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Image uiImage = background.AddComponent<UnityEngine.UI.Image>();
        uiImage.color = bgColour;

        UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();
        uiText.color = fgColour;
        uiText.text = text;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}
