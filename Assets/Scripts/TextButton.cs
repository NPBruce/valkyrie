using UnityEngine;
using System.Collections;

public class TextButton {

    public GameObject button;
    public GameObject background;

    public TextButton(Vector2 location, Vector2 size, string text, UnityEngine.Events.UnityAction call)
    {
        createButton(location, size, text, call, Color.white);
    }

    public TextButton(Vector2 location, Vector2 size, string text, UnityEngine.Events.UnityAction call, Color colour)
    {
        createButton(location, size, text, call, colour);
    }

    void createButton(Vector2 location, Vector2 size, string text, UnityEngine.Events.UnityAction call, Color colour)
    {
        // Create objects
        button = new GameObject("button" + text);
        background = new GameObject("buttonBg" + text);
        // Mark it as dialog
        button.tag = "dialog";
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

        button.transform.parent = background.transform;

        RectTransform transBt = button.AddComponent<RectTransform>();
        transBt.SetParent(transBg);

        transBt.localPosition = Vector2.zero;
        transBt.localScale = transBg.localScale;
        transBt.sizeDelta = transBg.sizeDelta;

        button.AddComponent<CanvasRenderer>();
        background.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Image uiImage = background.AddComponent<UnityEngine.UI.Image>();
        uiImage.color = new Color(0, 0, 0, (float)0.7);

        UnityEngine.UI.Button uiButton = background.AddComponent<UnityEngine.UI.Button>();
        uiButton.interactable = true;
        uiButton.onClick.AddListener(call);

        UnityEngine.UI.Text uiText = button.AddComponent<UnityEngine.UI.Text>();
        uiText.color = colour;
        uiText.text = text;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}
