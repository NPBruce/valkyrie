﻿using Assets.Scripts.Content;
using Assets.Scripts.UI;
using UnityEngine;

// Menu popup when in editor
public class EditorMenu
{
    private static readonly StringKey SAVE = new StringKey("val", "SAVE");
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
        UIElement ui = new UIElement();
        ui.SetLocation((UIScaler.GetWidthUnits() - 12) / 2, 9, 12, 13);
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
        ui.SetButton(delegate { QuestEditor.Reload(game.quest.originalPath); });
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation((UIScaler.GetWidthUnits() - 10) / 2, 16, 10, 2);
        ui.SetText(MAIN_MENU);
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
        ui.SetButton(Quit);
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation((UIScaler.GetWidthUnits() - 10) / 2, 19, 10, 2);
        ui.SetText(CommonStringKeys.CANCEL);
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
        ui.SetButton(Destroyer.Dialog);
        new UIElementBorder(ui);
    }

    public static void Quit()
    {
        Game game = Game.Get();
        // All content data has been loaded by editor, cleanup everything
        game.cd = new ContentData(game.gameType.DataDirectory());
        // Load the base content - pack will be loaded later if required
        game.cd.LoadContentID("");

        GameStateManager.MainMenu();
    }
}