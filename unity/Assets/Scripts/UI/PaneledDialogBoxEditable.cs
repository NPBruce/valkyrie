using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.UI;
using Assets.Scripts.Content;

// Create a dialog box which has editable text
// These are pretty rough at the moment.  Only used for editor
public class PaneledDialogBoxEditable
{
    private GameObject textObj;
    private GameObject background;
    private GameObject inputObj;

    private PanCancelInputField uiInput;

    public void ApplyTag(string tag)
    {
        textObj.tag = tag;
        background.tag = tag;
        inputObj.tag = tag;
    }

    /// <summary>
    /// Dialog Box editable with white text and black background
    /// </summary>
    /// <param name="location">Vector of location of the dialog</param>
    /// <param name="size">Vector of size of the dialog</param>
    /// <param name="text">default text to put inside the dialog</param>
    /// <param name="call">event to call when interacting with the dialog</param>
    public PaneledDialogBoxEditable(Vector2 location, Vector2 size, string text, UnityEngine.Events.UnityAction<string> call)
    {
        createDialog(location, size, text, call, Color.white, new Color(0, 0, 0, (float)0.9));
    }

    /// <summary>
    /// Dialog Box editable with black background
    /// </summary>
    /// <param name="location">Vector of location of the dialog</param>
    /// <param name="size">Vector of size of the dialog</param>
    /// <param name="text">default text to put inside the dialog</param>
    /// <param name="call">event to call when interacting with the dialog</param>
    /// <param name="fgColour">color or the font inside dialog</param>
    public PaneledDialogBoxEditable(Vector2 location, Vector2 size, string text, UnityEngine.Events.UnityAction<string> call, Color fgColour)
    {
        createDialog(location, size, text, call, fgColour, new Color(0, 0, 0, (float)0.9));
    }

    /// <summary>
    /// Dialog Box editable
    /// </summary>
    /// <param name="location">Vector of location of the dialog</param>
    /// <param name="size">Vector of size of the dialog</param>
    /// <param name="text">default text to put inside the dialog</param>
    /// <param name="call">event to call when interacting with the dialog</param>
    /// <param name="fgColour">color or the font inside dialog</param>
    /// <param name="bgColour">backgroudn color of the dialog</param>
    public PaneledDialogBoxEditable(Vector2 location, Vector2 size, string text, UnityEngine.Events.UnityAction<string> call, Color fgColour, Color bgColour)
    {
        createDialog(location, size, text, call, fgColour, bgColour);
    }

    /// <summary>
    /// Main constructor for dialog box editable
    /// </summary>
    /// <param name="location">Vector of location of the dialog</param>
    /// <param name="size">Vector of size of the dialog</param>
    /// <param name="text">default text to put inside the dialog</param>
    /// <param name="call">event to call when interacting with the dialog</param>
    /// <param name="fgColour">color or the font inside dialog</param>
    /// <param name="bgColour">backgroudn color of the dialog</param>
    void createDialog(Vector2 location, Vector2 size, string text, UnityEngine.Events.UnityAction<string> call, Color fgColour, Color bgColour)
    {
        // Object name includes first 10 chars of text
        string objName = text;
        if (objName.Length > 10)
        {
            objName = objName.Substring(0, 10);
        }

        // Create special characters

        float offset = location.y;
        TextButton tb = null;
        DialogBox db = null;
        StringKey translated = null;
        float hOffset = location.x + 2f;
        Dictionary<string, string> CHARS = null;
        EventManager.CHARS_MAP.TryGetValue(Game.Get().gameType.TypeName(), out CHARS);
        if (CHARS != null)
        {
            foreach (string oneChar in CHARS.Keys)
            {
                translated = new StringKey(null, oneChar);
                // Traits are in val dictionary
                db = new DialogBox(Vector2.zero, new Vector2(10, 1), translated);
                // Calculate width
                float width = (db.textObj.GetComponent<UnityEngine.UI.Text>().preferredWidth / UIScaler.GetPixelsPerUnit()) + 0.5f;
                db.Destroy();
                if (hOffset + width > 40)
                {
                    hOffset = 22;
                    offset++;
                }
                string translation = null;
                CHARS.TryGetValue(oneChar,out translation);
                tb = new TextButton(
                    new Vector2(hOffset, offset), new Vector2(width, 1),
                    translated, 
                    delegate { InsertCharacter(translation); });

                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                hOffset += width;
            }
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
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top,  (location.y +1) * UIScaler.GetPixelsPerUnit(), size.y * UIScaler.GetPixelsPerUnit());
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, location.x * UIScaler.GetPixelsPerUnit(), size.x * UIScaler.GetPixelsPerUnit());

        RectTransform transIn = inputObj.AddComponent<RectTransform>();
        transIn.SetParent(transBg);
        transIn.localScale = transBg.localScale;
        transIn.localPosition = Vector3.down; 
        transIn.sizeDelta = transBg.sizeDelta;

        RectTransform transTx = textObj.AddComponent<RectTransform>();
        transTx.SetParent(transIn);
        transTx.localScale = transIn.localScale;
        //transTx.localPosition = transIn.localPosition;
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
        uiText.horizontalOverflow = HorizontalWrapMode.Wrap;


        uiInput = inputObj.AddComponent<PanCancelInputField>();

        uiInput.textComponent = uiText;
        // We must set first the multiline attribute in order
        // to not lose the newlines inside variable text
        uiInput.lineType = UnityEngine.UI.InputField.LineType.MultiLineNewline;
        uiInput.text = text;
        uiInput.onEndEdit.AddListener(call);
    }

    public void InsertCharacter(string specialChar)
    {
        uiInput.text = uiInput.text.Insert(uiInput.caretPosition, specialChar);
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

    public string Text
    {
        get { return uiInput.text; }
    }

    public void setMaterialAndBackgroundTransformParent(Material mat, Transform trans)
    {
        this.textObj.GetComponent<UnityEngine.UI.Text>().material = mat;
        this.background.transform.parent = trans;
    }
}
