using UnityEngine;
using Assets.Scripts.Content;

// Menu popup when in editor
public class EditorMenu {

    private static readonly StringKey SAVE = new StringKey("val", "SAVE");
    private static readonly StringKey TOOLS = new StringKey("val", "TOOLS");
    private static readonly StringKey RELOAD = new StringKey("val", "RELOAD");
    private static readonly StringKey MAIN_MENU = new StringKey("val", "MAIN_MENU");

    public static void Create()
    {
        Game game = Game.Get();
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        // Menu border
        DialogBox db = new DialogBox(new Vector2((UIScaler.GetWidthUnits() - 12) / 2, 7), new Vector2(12, 16), StringKey.NULL);
        db.AddBorder();
        db.SetFont(game.gameType.GetHeaderFont());

        TextButton tb = new TextButton(
            new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 8), new Vector2(10, 2f), 
            SAVE, delegate { QuestEditor.Save(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        tb = new TextButton(
            new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 11), new Vector2(10, 2f),
            TOOLS, delegate { EditorTools.Create(); }, Color.red);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.setActive(false);
        tb.SetFont(game.gameType.GetHeaderFont());

        tb = new TextButton(
            new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 14), new Vector2(10, 2f), 
            RELOAD, delegate { QuestEditor.Reload(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        tb = new TextButton(
            new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 17), new Vector2(10, 2f), 
            MAIN_MENU, delegate { Destroyer.MainMenu(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        tb = new TextButton(
            new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 20), new Vector2(10, 2f),
            CommonStringKeys.CANCEL, delegate { Destroyer.Dialog(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

    }
}
