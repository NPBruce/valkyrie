using UnityEngine;
using System.Collections;

// Used to create text buttons for the UI
public class TextButton {

    // The button itself, unity object
    public GameObject button;
    // The background, unity object
    public GameObject background;
    // Border for the button
    public RectangleBorder border;

    // Function to alter the tag of the button (unity classification)
    public void ApplyTag(string tag)
    {
        button.tag = tag;
        background.tag = tag;
        border.SetTag(tag);
    }

    // Draw a button (white)
    // location: position in scale units
    // size: size in scale units
    // text: text on the button
    // call: function to call on press
    public TextButton(Vector2 location, Vector2 size, string text, UnityEngine.Events.UnityAction call)
    {
        createButton(location, size, text, call, Color.white, 0);
    }

    // Draw a button, as above with:
    // colour: colour for the text and border
    // id: unique identifier for Unity (default 0)
    public TextButton(Vector2 location, Vector2 size, string text, UnityEngine.Events.UnityAction call, Color colour, int id = 0)
    {
        createButton(location, size, text, call, colour, id);
    }

    // Internal function to create button from constructors
    void createButton(Vector2 location, Vector2 size, string text, UnityEngine.Events.UnityAction call, Color colour, int id)
    {
        // Create objects
        button = new GameObject("button" + text + id);
        background = new GameObject("buttonBg" + text + id);
        border = new RectangleBorder(background.transform, colour, size);

        // Mark it as dialog (this can be changed with applytag)
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
        // Background is partially transparent black
        uiImage.color = new Color(0, 0, 0, (float)0.9);

        UnityEngine.UI.Button uiButton = background.AddComponent<UnityEngine.UI.Button>();
        uiButton.interactable = true;
        uiButton.onClick.AddListener(call);

        UnityEngine.UI.Text uiText = button.AddComponent<UnityEngine.UI.Text>();
        uiText.color = colour;
        uiText.text = text;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.font = game.gameType.GetFont();
        // Default to medium font size
        uiText.fontSize = UIScaler.GetMediumFont();
    }
}

