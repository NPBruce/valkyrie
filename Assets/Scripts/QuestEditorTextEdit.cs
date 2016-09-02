using UnityEngine;
using System.Collections;

public class QuestEditorTextEdit {

    public string value = "";
    public string title;
    public UnityEngine.Events.UnityAction returnCall;
    public UnityEngine.Events.UnityAction cancelCall;

    public QuestEditorTextEdit(string t, string initial, UnityEngine.Events.UnityAction call)
    {
        value = initial;
        title = t;
        returnCall = call;
    }

    public void EditText()
    {
        EditText(delegate { Destroyer.Dialog(); });
    }

    public void EditText(UnityEngine.Events.UnityAction call)
    {
        Destroyer.Dialog();
        cancelCall = call;

        DialogBox db = new DialogBox(new Vector2(21, 0), new Vector2(20, 26), "");
        db.AddBorder();

        db = new DialogBox(new Vector2(21, 0), new Vector2(20, 1), title);

        Game game = Game.Get();
        GameObject textObj = new GameObject("textEdit");
        GameObject inputObj = new GameObject("textInput");
        GameObject pLaceholderObj = new GameObject("textPlaceholder");
        textObj.tag = "dialog";
        inputObj.tag = "dialog";
        pLaceholderObj.tag = "dialog";

        inputObj.transform.parent = game.uICanvas.transform;
        textObj.transform.parent = inputObj.transform;
        pLaceholderObj.transform.parent = inputObj.transform;

        RectTransform transBg = inputObj.AddComponent<RectTransform>();
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 5 * UIScaler.GetPixelsPerUnit(), 10 * UIScaler.GetPixelsPerUnit());
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 25 * UIScaler.GetPixelsPerUnit(), 10 * UIScaler.GetPixelsPerUnit());

        inputObj.AddComponent<CanvasRenderer>();
        textObj.AddComponent<CanvasRenderer>();
        pLaceholderObj.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();
        UnityEngine.UI.Text placeholderText = pLaceholderObj.AddComponent<UnityEngine.UI.Text>();
        UnityEngine.UI.InputField input = inputObj.AddComponent<UnityEngine.UI.InputField>();
        placeholderText.text = value;
        input.textComponent = uiText;
        input.placeholder = placeholderText;
        //input.color = fgColour;
        input.text = value;
        placeholderText.text = value;
        //input.onEndEdit = returnCall;
        //input.alignment = TextAnchor.MiddleCenter;
        //input.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        //input.fontSize = UIScaler.GetSmallFont();


        TextButton tb = new TextButton(new Vector2(26.5f, 20), new Vector2(9, 1), "Cancel", call);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
    }

    public void SelectComponent(string s)
    {
        title = s;
        returnCall();
    }
}
