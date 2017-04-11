using UnityEngine;

// Create a dialog box which has editable text
// These are pretty rough at the moment.  Only used for editor
public class DialogBoxEditable
{
    private GameObject textObj;
    private GameObject background;
    private GameObject inputObj;
    /// <summary>
    /// Small text to show translation info
    /// </summary>
    //private GameObject translationTextObj;

    private UnityEngine.UI.InputField uiInput;

    private string translationKey;

    public void ApplyTag(string tag)
    {
        textObj.tag = tag;
        background.tag = tag;
        inputObj.tag = tag;
        //translationTextObj.tag = tag;
    }

    /// <summary>
    /// Dialog Box editable with white text and black background
    /// </summary>
    /// <param name="location">Vector of location of the dialog</param>
    /// <param name="size">Vector of size of the dialog</param>
    /// <param name="text">default text to put inside the dialog</param>
    /// <param name="call">event to call when interacting with the dialog</param>
    public DialogBoxEditable(Vector2 location, Vector2 size, string text, string localizationKey, UnityEngine.Events.UnityAction<string> call)
    {
        createDialog(location, size, text, localizationKey, call, Color.white, new Color(0, 0, 0, (float)0.9));
    }

    /// <summary>
    /// Dialog Box editable with black background
    /// </summary>
    /// <param name="location">Vector of location of the dialog</param>
    /// <param name="size">Vector of size of the dialog</param>
    /// <param name="text">default text to put inside the dialog</param>
    /// <param name="call">event to call when interacting with the dialog</param>
    /// <param name="fgColour">color or the font inside dialog</param>
    public DialogBoxEditable(Vector2 location, Vector2 size, string text, string localizationKey, UnityEngine.Events.UnityAction<string> call, Color fgColour)
    {
        createDialog(location, size, text, localizationKey, call, fgColour, new Color(0, 0, 0, (float)0.9));
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
    public DialogBoxEditable(Vector2 location, Vector2 size, string text, string localizationKey, UnityEngine.Events.UnityAction<string> call, Color fgColour, Color bgColour)
    {
        createDialog(location, size, text, localizationKey, call, fgColour, bgColour);
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
    void createDialog(Vector2 location, Vector2 size, string text, string localizationKey, UnityEngine.Events.UnityAction<string> call, Color fgColour, Color bgColour)
    {
        // Object name includes first 10 chars of text
        string objName = text;
        if (objName.Length > 10)
        {
            objName = objName.Substring(0, 10);
        }
        // Create an object

        translationKey = localizationKey;

        textObj = new GameObject("text" + objName);
        background = new GameObject("textBg" + objName);
        inputObj = new GameObject("textIn" + objName);
        //translationTextObj = new GameObject("translationText" + objName);

        // Mark it as dialog
        textObj.tag = "dialog";
        background.tag = "dialog";
        inputObj.tag = "dialog";
        //translationTextObj.tag = "dialog";

        Game game = Game.Get();

        background.transform.parent = game.uICanvas.transform;
        inputObj.transform.parent = background.transform;
        textObj.transform.parent = inputObj.transform;
        //translationTextObj.transform.parent = background.transform;

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

        //RectTransform transTran = translationTextObj.AddComponent<RectTransform>();
        //transTran.SetParent(transBg);
        //transTran.localScale = transBg.localScale;
        //transTran.sizeDelta = transBg.sizeDelta;

        textObj.AddComponent<CanvasRenderer>();
        background.AddComponent<CanvasRenderer>();
        inputObj.AddComponent<CanvasRenderer>();
        //translationTextObj.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Image uiImage = background.AddComponent<UnityEngine.UI.Image>();
        uiImage.color = bgColour;

        UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();
        uiText.color = fgColour;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.font = game.gameType.GetFont();
        uiText.material = uiText.font.material;
        uiText.fontSize = UIScaler.GetSmallFont();

        //UnityEngine.UI.Text uiTranslationText = translationTextObj.AddComponent<UnityEngine.UI.Text>();
        // Very dark text
        //uiTranslationText.color = new Color(0.2f,0.2f,0.2f);
        //uiTranslationText.alignment = TextAnchor.LowerRight;
        //uiTranslationText.font = game.gameType.GetFont();
        //uiTranslationText.material = uiText.font.material;
        //// Smallest
        //uiTranslationText.fontSize = 8;
        //uiTranslationText.text = translationKey;

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
