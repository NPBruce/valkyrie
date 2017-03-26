using UnityEngine;
using System.Collections;

// Next stage button is used by MoM to move between investigators and monsters
public class LogWindow
{
    // Construct and display
    public LogWindow()
    {
        Game game = Game.Get();
        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        // white background because font rendering is broken
        string log = "";
        foreach (Quest.LogEntry e in game.quest.log)
        {
            log += e.GetEntry();
        }
        log.Trim('\n');
        DialogBox db = new DialogBox(new Vector2(UIScaler.GetHCenter(-14f), 0.5f), new Vector2(28, 24.5f), log, Color.black, new Color(1, 1, 1, 0.9f));
        db.AddBorder();
        // This material works for the mask, but only renders in black
        db.textObj.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
        UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();
        scrollRect.content = db.textObj.GetComponent<RectTransform>();
        scrollRect.horizontal = false;
        RectTransform textRect = db.textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(textRect.rect.width, db.textObj.GetComponent<UnityEngine.UI.Text>().preferredHeight);
        scrollRect.verticalNormalizedPosition = 0f;

        UnityEngine.UI.Mask mask = db.background.AddComponent<UnityEngine.UI.Mask>();

        new TextButton(new Vector2(UIScaler.GetHCenter(-3f), 25f), new Vector2(6, 2), "Close", delegate { Destroyer.Dialog(); });
    }
}
