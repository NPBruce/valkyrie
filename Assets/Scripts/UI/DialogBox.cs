using UnityEngine;
using System.Collections;

public class DialogBox {

    public GameObject textObj;
    public GameObject background;

    public void ApplyTag(string tag)
    {
        textObj.tag = tag;
        background.tag = tag;
    }

    public DialogBox(Vector2 location, Vector2 size, string text)
    {
        createDialog(location, size, text, Color.white, new Color(0, 0, 0, (float)0.9));
    }

    public DialogBox(Vector2 location, Vector2 size, string text, Color fgColour)
    {
        createDialog(location, size, text, fgColour, new Color(0, 0, 0, (float)0.9));
    }

    public DialogBox(Vector2 location, Vector2 size, string text, Color fgColour, Color bgColour)
    {
        createDialog(location, size, text, fgColour, bgColour);
    }

    void createDialog(Vector2 location, Vector2 size, string text, Color fgColour, Color bgColour)
    {
        // Object name includes first 10 chars of text
        string objName = text;
        if (objName.Length > 10)
        {
            objName = objName.Substring(0, 10);
        }
        // Create an object

        textObj = new GameObject("text" + objName);
        background = new GameObject("buttonBg" + objName);
        // Mark it as dialog
        textObj.tag = "dialog";
        background.tag = "dialog";

        Game game = Game.Get();
        background.transform.parent = game.uICanvas.transform;

        RectTransform transBg = background.AddComponent<RectTransform>();
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, location.y * UIScaler.GetPixelsPerUnit(), size.y * UIScaler.GetPixelsPerUnit());
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, location.x * UIScaler.GetPixelsPerUnit(), size.x * UIScaler.GetPixelsPerUnit());

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
        //uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        //uiText.font = (Font)Resources.Load("fonts/windl");
        uiText.font = (Font)Resources.Load("fonts/gara_scenario_desc");
        uiText.fontSize = UIScaler.GetSmallFont();
    }

    public void AddBorder()
    {
        AddBorder(textObj.GetComponent<UnityEngine.UI.Text>().color);
    }

    public void AddBorder(Color c)
    {
        UnityEngine.Rect rect = background.GetComponent<RectTransform>().rect;
        new RectangleBorder(background.transform, c, new Vector2(rect.width / UIScaler.GetPixelsPerUnit(), rect.height / UIScaler.GetPixelsPerUnit()));
    }
}
