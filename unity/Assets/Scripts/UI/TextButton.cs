using UnityEngine;
using System.Collections;
using Assets.Scripts.Content;

// Used to create text buttons for the UI
public class TextButton {

    // The button itself, unity object
    public GameObject button;
    // The background, unity object
    public GameObject background;
    // Border for the button
    public RectangleBorder border;

    public UnityEngine.UI.Text uiText;
    private UnityEngine.UI.Button uiButton;

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
    public TextButton(Vector2 location, Vector2 size, StringKey textKey, UnityEngine.Events.UnityAction call)
    {
        createButton(location, size, textKey, call, Color.white, 0);
    }

    // Draw a button, as above with:
    // colour: colour for the text and border
    // id: unique identifier for Unity (default 0)
    public TextButton(Vector2 location, Vector2 size, StringKey textKey, UnityEngine.Events.UnityAction call, Color colour, int id = 0)
    {
        createButton(location, size, textKey, call, colour, id);
    }

    // Internal function to create button from constructors
    void createButton(Vector2 location, Vector2 size, StringKey textKey, UnityEngine.Events.UnityAction call, Color colour, int id)
    {
        // Create objects
        button = new GameObject("button" + textKey + id);
        background = new GameObject("buttonBg" + textKey + id);
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

        uiButton = background.AddComponent<UnityEngine.UI.Button>();
        uiButton.interactable = true;
        uiButton.onClick.AddListener(call);

        uiText = button.AddComponent<UnityEngine.UI.Text>();
        uiText.color = colour;
        uiText.text = textKey.Translate();
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.font = game.gameType.GetFont();
        uiText.material = uiText.font.material;
        // Default to medium font size
        uiText.fontSize = UIScaler.GetMediumFont();
    }

    public void SetFont(Font f)
    {
        uiText.font = f;
        uiText.material = uiText.font.material;
    }

    /// <summary>
    /// Change the color of entire element
    /// </summary>
    /// <param name="c"></param>
    public void setColor(Color c)
    {
        uiText.color = c;
        border.color = c;
    }

    /// <summary>
    /// Sets the active status of the button to enable/disable it
    /// </summary>
    /// <param name="newActiveStatus">new status</param>
    public void setActive(bool newActiveStatus)
    {
        uiButton.interactable = newActiveStatus;
        
    }

    public void Destroy()
    {
        Object.Destroy(button);
        Object.Destroy(background);
        Object.Destroy(border.bLine[0]);
        Object.Destroy(border.bLine[1]);
        Object.Destroy(border.bLine[2]);
        Object.Destroy(border.bLine[3]);
    }
}

