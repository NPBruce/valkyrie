using UnityEngine;
using System.Collections;

public class MainMenu {
    public MainMenu()
    {
        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 3), "Valkyrie");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();

        TextButton tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 10), new Vector2(12, 2f), "Start Quest", delegate { Start(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 13), new Vector2(12, 2f), "Select Content", delegate { Content(); }, Color.red);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 16), new Vector2(12, 2f), "Quest Editor", delegate { Editor(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 19), new Vector2(12, 2f), "Exit", delegate { Exit(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);
    }

    public void Start()
    {
        Game game = Game.Get();

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        game.SelectQuest();
    }

    public void Content()
    {
    }


    public void Editor()
    {
        new QuestEditSelection();
    }

    public void Exit()
    {
        Application.Quit();
    }
}
