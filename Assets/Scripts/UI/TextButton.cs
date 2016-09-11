using UnityEngine;
using System.Collections;

public class TextButton {

    public GameObject button;
    public GameObject background;
    public RectangleBorder border;

    public void ApplyTag(string tag)
    {
        button.tag = tag;
        background.tag = tag;
        border.SetTag(tag);
    }

    public TextButton(Vector2 location, Vector2 size, string text, UnityEngine.Events.UnityAction call)
    {
        createButton(location, size, text, call, Color.white, 0);
    }

    public TextButton(Vector2 location, Vector2 size, string text, UnityEngine.Events.UnityAction call, Color colour, int id = 0)
    {
        createButton(location, size, text, call, colour, id);
    }

    void createButton(Vector2 location, Vector2 size, string text, UnityEngine.Events.UnityAction call, Color colour, int id)
    {
        // Create objects
        button = new GameObject("button" + text + id);
        background = new GameObject("buttonBg" + text + id);
        border = new RectangleBorder(background.transform, colour, size);

        // Mark it as dialog
        button.tag = "dialog";
        background.tag = "dialog";

        Game game = Game.Get();
        background.transform.parent = game.uICanvas.transform;

        RectTransform transBg = background.AddComponent<RectTransform>();
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, location.y * UIScaler.GetPixelsPerUnit(), size.y * UIScaler.GetPixelsPerUnit());
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, location.x * UIScaler.GetPixelsPerUnit(), size.x * UIScaler.GetPixelsPerUnit());

        button.transform.parent = background.transform;

        RectTransform transBt = button.AddComponent<RectTransform>();
        transBt.SetParent(transBg);

        transBt.localPosition = Vector2.zero;
        transBt.localScale = transBg.localScale;
        transBt.sizeDelta = transBg.sizeDelta;

        button.AddComponent<CanvasRenderer>();
        background.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Image uiImage = background.AddComponent<UnityEngine.UI.Image>();
        uiImage.color = new Color(0, 0, 0, (float)0.9);

        UnityEngine.UI.Button uiButton = background.AddComponent<UnityEngine.UI.Button>();
        uiButton.interactable = true;
        uiButton.onClick.AddListener(call);

        UnityEngine.UI.Text uiText = button.AddComponent<UnityEngine.UI.Text>();
        uiText.color = colour;
        uiText.text = text;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.font = game.gameType.GetFont();
        uiText.fontSize = UIScaler.GetMediumFont();
    }
}

