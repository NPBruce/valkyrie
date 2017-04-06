using UnityEngine;
using System.Collections;
using Assets.Scripts.Content;

// Create a box with text
public class DialogBox {

    // Unity objects for the text and background
    public GameObject textObj;
    public GameObject background;
    public RectangleBorder border;

    // Set the unity object tag
    public void ApplyTag(string tag)
    {
        textObj.tag = tag;
        background.tag = tag;
    }

    // Create text on the screen
    // location: position in scale units
    // size: size in scale units
    // text: text to display
    public DialogBox(Vector2 location, Vector2 size, StringKey text)
    {
        createDialog(location, size, text, Color.white, new Color(0, 0, 0, (float)0.9));
    }

    // Create text on the screen, as above with
    // fgColour: color for the text, border if used
    public DialogBox(Vector2 location, Vector2 size, StringKey text, Color fgColour)
    {
        createDialog(location, size, text, fgColour, new Color(0, 0, 0, (float)0.9));
    }

    // Create text on the screen, as above with
    // bgColour: color for the background
    public DialogBox(Vector2 location, Vector2 size, StringKey text, Color fgColour, Color bgColour)
    {
        createDialog(location, size, text, fgColour, bgColour);
    }

    // Internal function to create the text from constructors
    void createDialog(Vector2 location, Vector2 size, StringKey textKey, Color fgColour, Color bgColour)
    {
        // Object name includes first 10 chars of text
        string objName = textKey.key;
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
        transBt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0.1f * UIScaler.GetPixelsPerUnit(), transBt.sizeDelta.x - (0.1f * UIScaler.GetPixelsPerUnit()));

        textObj.AddComponent<CanvasRenderer>();
        background.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Image uiImage = background.AddComponent<UnityEngine.UI.Image>();
        uiImage.color = bgColour;

        UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();
        uiText.color = fgColour;
        uiText.text = textKey.Translate();
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.font = game.gameType.GetFont();
        uiText.material = uiText.font.material;
        uiText.fontSize = UIScaler.GetSmallFont();
    }

    // Function to draw a border around the text
    // FIXME border does not have horizontal padding from wrapped text
    public void AddBorder()
    {
        AddBorder(textObj.GetComponent<UnityEngine.UI.Text>().color);
    }

    // As above, but with
    // c: colour for the border
    public void AddBorder(Color c)
    {
        UnityEngine.Rect rect = background.GetComponent<RectTransform>().rect;
        new RectangleBorder(background.transform, c, new Vector2(rect.width / UIScaler.GetPixelsPerUnit(), rect.height / UIScaler.GetPixelsPerUnit()));
    }

    public void SetFont(Font f)
    {
        textObj.GetComponent<UnityEngine.UI.Text>().font = f;
        textObj.GetComponent<UnityEngine.UI.Text>().material = textObj.GetComponent<UnityEngine.UI.Text>().font.material;
    }

    public void Destroy()
    {
        Object.Destroy(textObj);
        Object.Destroy(background);
        if (border == null)
        {
            return;
        }
        Object.Destroy(border.bLine[0]);
        Object.Destroy(border.bLine[1]);
        Object.Destroy(border.bLine[2]);
        Object.Destroy(border.bLine[3]);
    }
}
