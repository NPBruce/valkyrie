using UnityEngine;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

public class EditorTools
{
    private static readonly StringKey VALIDATE_SCENARIO = new StringKey("val", "VALIDATE_SCENARIO");
    private static readonly StringKey OPTIMIZE_LOCALIZATION = new StringKey("val", "OPTIMIZE_LOCALIZATION");
    private static readonly StringKey REORDER_COMPONENTS = new StringKey("val", "REORDER_COMPONENTS");

    public static void Create()
    {

        Game game = Game.Get();
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        // Menu border
        UIElement ui = new UIElement();
        ui.SetLocation((UIScaler.GetWidthUnits() - 20) / 2, 9, 20, 13);
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation((UIScaler.GetWidthUnits() - 18) / 2, 10, 18, 2);
        ui.SetText(VALIDATE_SCENARIO, Color.grey);
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
        ui.SetButton(Validate_Scenario);
        new UIElementBorder(ui, Color.grey);

        ui = new UIElement();
        ui.SetLocation((UIScaler.GetWidthUnits() - 18) / 2, 13, 18, 2);
        ui.SetText(OPTIMIZE_LOCALIZATION, Color.grey);
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
        ui.SetButton(Optimize_Localization);
        new UIElementBorder(ui, Color.grey);

        ui = new UIElement();
        ui.SetLocation((UIScaler.GetWidthUnits() - 18) / 2, 16, 18, 2);
        ui.SetText(REORDER_COMPONENTS);
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
        ui.SetButton(delegate { new ReorderComponents(); });
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

    /// <summary>
    /// Check the scenario to find possible bugs (Issue #56)
    /// </summary>
    private static void Validate_Scenario()
    {

    }

    /// <summary>
    /// Create keys to repeated texts and reference it
    /// </summary>
    private static void Optimize_Localization()
    {

    }
}
