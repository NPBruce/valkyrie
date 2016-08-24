using UnityEngine;
using System.Collections;

public class TextButton {

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
        // Create an object
        GameObject button = new GameObject("button" + text);
        // Mark it as dialog
        button.tag = "dialog";

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
        button.transform.parent = canvas.transform;

        RectTransform trans = button.AddComponent<RectTransform>();
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, location.y, size.y);
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, location.x, size.x);

        button.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Button uiButton = button.AddComponent<UnityEngine.UI.Button>();
        uiButton.interactable = true;
        //uiButton.onClick.AddListener(delegate { onPress(); });
        uiButton.onClick.AddListener(call);

        UnityEngine.UI.Text uiText = button.AddComponent<UnityEngine.UI.Text>();
        uiText.color = colour;
        uiText.text = text;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    public void onPress()
    {
        Debug.Log("DFDa");
    }
}
