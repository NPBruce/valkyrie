using UnityEngine;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

// Editable text box for use in quest editor
public class QuestEditorTextEdit {

    public string value = "";
    public StringKey title;
    public UnityEngine.Events.UnityAction returnCall;
    public UnityEngine.Events.UnityAction cancelCall;
    public PanCancelInputField iField;

    // Create a new text box with title, initial value and call back
    public QuestEditorTextEdit(StringKey t, string initial, UnityEngine.Events.UnityAction call)
    {
        value = initial;
        title = t;
        returnCall = call;
    }

    // Create window, remove if cancelled
    public void EditText()
    {
        EditText(delegate { Destroyer.Dialog(); });
    }

    // Create window, call event on cancel
    public void EditText(UnityEngine.Events.UnityAction call)
    {
        Destroyer.Dialog();
        cancelCall = call;

        // Border
        DialogBox db = new DialogBox(new Vector2(UIScaler.GetHCenter(-10), 1), new Vector2(20, 6), StringKey.NULL);
        db.AddBorder();

        // Heading
        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-9.5f), 1), new Vector2(19, 1), title);

        Game game = Game.Get();
        GameObject textObj = new GameObject("textEdit");
        GameObject inputObj = new GameObject("textInput");
        textObj.tag = Game.DIALOG;
        inputObj.tag = Game.DIALOG;

        inputObj.transform.SetParent(game.uICanvas.transform);
        textObj.transform.SetParent(inputObj.transform);

        RectTransform transBg = inputObj.AddComponent<RectTransform>();
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 3 * UIScaler.GetPixelsPerUnit(), UIScaler.GetPixelsPerUnit());
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, UIScaler.GetHCenter(-9) * UIScaler.GetPixelsPerUnit(), 18 * UIScaler.GetPixelsPerUnit());

        RectTransform transTx = textObj.AddComponent<RectTransform>();
        transTx.SetParent(transBg);
        transTx.localScale = transBg.localScale;
        transTx.sizeDelta = transBg.sizeDelta;

        inputObj.AddComponent<CanvasRenderer>();
        textObj.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();

        uiText.color = Color.white;
        uiText.font = game.gameType.GetFont();
        uiText.material = uiText.font.material; 
        uiText.fontSize = UIScaler.GetSmallFont();

        iField = inputObj.AddComponent<PanCancelInputField>();
        iField.textComponent = uiText;
        iField.text = value;

        TextButton tb = new TextButton(new Vector2(UIScaler.GetHCenter(-8), 5), new Vector2(6, 1), CommonStringKeys.OK, OKButton);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.0f, 0.03f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(UIScaler.GetHCenter(2), 5), new Vector2(6, 1), CommonStringKeys.CANCEL, cancelCall);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
    }


    public void OKButton()
    {
        value = iField.text;
        Destroyer.Dialog();
        returnCall();
    }
}
