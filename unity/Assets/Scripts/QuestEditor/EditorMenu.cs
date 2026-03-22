using Assets.Scripts.Content;
using Assets.Scripts.UI;
using UnityEngine;

// Menu popup when in editor
public class EditorMenu
{
    private static readonly StringKey SAVE = new StringKey("val", "SAVE");
    private static readonly StringKey RELOAD = new StringKey("val", "RELOAD");
    private static readonly StringKey MAIN_MENU = new StringKey("val", "MAIN_MENU");
    private static readonly StringKey OPEN_IN_FILE_MANAGER = new StringKey("val", "OPEN_IN_FILE_MANAGER");

    public static void Create()
    {
        Game game = Game.Get();
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        // Menu border
        UIElement ui = new UIElement();
        ui.SetLocation((UIScaler.GetWidthUnits() - 12) / 2, 9, 12, 16);
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation((UIScaler.GetWidthUnits() - 10) / 2, 10, 10, 2);
        ui.SetText(SAVE);
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
        ui.SetButton(QuestEditor.Save);
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation((UIScaler.GetWidthUnits() - 10) / 2, 13, 10, 2);
        ui.SetText(RELOAD);
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
        ui.SetButton(delegate { QuestEditor.Reload(game.CurrentQuest.originalPath); });
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation((UIScaler.GetWidthUnits() - 10) / 2, 16, 10, 2);
        ui.SetText(OPEN_IN_FILE_MANAGER);
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(OPEN_IN_FILE_MANAGER.Translate().Length > 15 ? UIScaler.GetSmallFont() : UIScaler.GetMediumFont());
        ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
        ui.SetButton(delegate { Application.OpenURL(game.CurrentQuest.originalPath); });
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation((UIScaler.GetWidthUnits() - 10) / 2, 19, 10, 2);
        ui.SetText(MAIN_MENU);
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
        ui.SetButton(GameStateManager.MainMenu);
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation((UIScaler.GetWidthUnits() - 10) / 2, 22, 10, 2);
        ui.SetText(CommonStringKeys.CANCEL);
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
        ui.SetButton(Destroyer.Dialog);
        new UIElementBorder(ui);
    }
}