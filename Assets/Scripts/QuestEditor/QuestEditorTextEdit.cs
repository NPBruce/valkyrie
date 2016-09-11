using UnityEngine;
using System.Collections;

public class QuestEditorTextEdit {

    public string value = "";
    public string title;
    public UnityEngine.Events.UnityAction returnCall;
    public UnityEngine.Events.UnityAction cancelCall;
    public UnityEngine.UI.InputField iField;

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

        DialogBox db = new DialogBox(new Vector2(21, 0), new Vector2(20, 6), "");
        db.AddBorder();

        db = new DialogBox(new Vector2(21, 0), new Vector2(20, 1), title);

        Game game = Game.Get();
        GameObject textObj = new GameObject("textEdit");
        GameObject inputObj = new GameObject("textInput");
        textObj.tag = "dialog";
        inputObj.tag = "dialog";

        inputObj.transform.parent = game.uICanvas.transform;
        textObj.transform.parent = inputObj.transform;

        RectTransform transBg = inputObj.AddComponent<RectTransform>();
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 2 * UIScaler.GetPixelsPerUnit(), UIScaler.GetPixelsPerUnit());
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 22 * UIScaler.GetPixelsPerUnit(), 18 * UIScaler.GetPixelsPerUnit());

        RectTransform transTx = textObj.AddComponent<RectTransform>();
        transTx.SetParent(transBg);
        transTx.localScale = transBg.localScale;
        transTx.sizeDelta = transBg.sizeDelta;

        inputObj.AddComponent<CanvasRenderer>();
        textObj.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();
        iField = inputObj.AddComponent<UnityEngine.UI.InputField>();

        uiText.color = Color.white;
        uiText.font = game.gameType.GetFont();
        uiText.fontSize = UIScaler.GetSmallFont();

        iField.textComponent = uiText;
        iField.text = value;

        TextButton tb = new TextButton(new Vector2(23f, 4), new Vector2(6, 1), "OK", OKButton);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.0f, 0.03f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();

        tb = new TextButton(new Vector2(31f, 4), new Vector2(6, 1), "Cancel", cancelCall);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
    }


    public void OKButton()
    {
        value = iField.text;
        Destroyer.Dialog();
        returnCall();
    }

    public static string SymbolReplace(string input)
    {
        string output = input;
        output = output.Replace("≥", "{heart}");
        output = output.Replace("∏", "{fatigue}");
        output = output.Replace("∂", "{might}");
        output = output.Replace("π", "{will}");
        output = output.Replace("∑", "{knowledge}");
        output = output.Replace("μ", "{awareness}");
        output = output.Replace("∞", "{action}");
        output = output.Replace("±", "{shield}");
        output = output.Replace("≥", "{surge}");
        return output;
    }
}
