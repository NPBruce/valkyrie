using UnityEngine;
using System.Collections;

// Create a dialog box which has editable text
// These are pretty rough at the moment.  Only used for editor
public class DialogBoxEditable
{
    public GameObject textObj;
    public GameObject background;
    public GameObject inputObj;
    public UnityEngine.UI.InputField uiInput;

    public void ApplyTag(string tag)
    {
        textObj.tag = tag;
        background.tag = tag;
        inputObj.tag = tag;
    }

    public DialogBoxEditable(Vector2 location, Vector2 size, string text, UnityEngine.Events.UnityAction<string> call)
    {
        createDialog(location, size, text, call, Color.white, new Color(0, 0, 0, (float)0.9));
    }

    public DialogBoxEditable(Vector2 location, Vector2 size, string text, UnityEngine.Events.UnityAction<string> call, Color fgColour)
    {
        createDialog(location, size, text, call, fgColour, new Color(0, 0, 0, (float)0.9));
    }

    public DialogBoxEditable(Vector2 location, Vector2 size, string text, UnityEngine.Events.UnityAction<string> call, Color fgColour, Color bgColour)
    {
        createDialog(location, size, text, call, fgColour, bgColour);
    }

    void createDialog(Vector2 location, Vector2 size, string text, UnityEngine.Events.UnityAction<string> call, Color fgColour, Color bgColour)
    {
        // Object name includes first 10 chars of text
        string objName = text;
        if (objName.Length > 10)
        {
            objName = objName.Substring(0, 10);
        }
        // Create an object

        textObj = new GameObject("text" + objName);
        background = new GameObject("textBg" + objName);
        inputObj = new GameObject("textIn" + objName);
        // Mark it as dialog
        textObj.tag = "dialog";
        background.tag = "dialog";
        inputObj.tag = "dialog";

        Game game = Game.Get();

        background.transform.parent = game.uICanvas.transform;
        inputObj.transform.parent = background.transform;
        textObj.transform.parent = inputObj.transform;

        RectTransform transBg = background.AddComponent<RectTransform>();
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, location.y * UIScaler.GetPixelsPerUnit(), size.y * UIScaler.GetPixelsPerUnit());
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, location.x * UIScaler.GetPixelsPerUnit(), size.x * UIScaler.GetPixelsPerUnit());

        RectTransform transIn = inputObj.AddComponent<RectTransform>();
        transIn.SetParent(transBg);
        transIn.localScale = transBg.localScale;
        transIn.sizeDelta = transBg.sizeDelta;

        RectTransform transTx = textObj.AddComponent<RectTransform>();
        transTx.SetParent(transIn);
        transTx.localScale = transIn.localScale;
        transTx.sizeDelta = transIn.sizeDelta;

        textObj.AddComponent<CanvasRenderer>();
        background.AddComponent<CanvasRenderer>();
        inputObj.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Image uiImage = background.AddComponent<UnityEngine.UI.Image>();
        uiImage.color = bgColour;

        UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();
        uiText.color = fgColour;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.font = game.gameType.GetFont();
        uiText.material = uiText.font.material;
        uiText.fontSize = UIScaler.GetSmallFont();

        uiInput = inputObj.AddComponent<UnityEngine.UI.InputField>();

        uiInput.textComponent = uiText;
        uiInput.text = text;
        uiInput.onEndEdit.AddListener(call);
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
