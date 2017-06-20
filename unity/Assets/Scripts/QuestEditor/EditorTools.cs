using UnityEngine;
using Assets.Scripts.Content;

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

        TextButton tb = new TextButton(
            new Vector2((UIScaler.GetWidthUnits() - 18) / 2, 10), new Vector2(18, 2f),
            VALIDATE_SCENARIO, delegate { Validate_Scenario(); }, Color.grey);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        tb = new TextButton(
            new Vector2((UIScaler.GetWidthUnits() - 18) / 2, 13), new Vector2(18, 2f),
            OPTIMIZE_LOCALIZATION, delegate { Optimize_Localization(); }, Color.grey);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        tb = new TextButton(
            new Vector2((UIScaler.GetWidthUnits() - 18) / 2, 16), new Vector2(18, 2f),
            REORDER_COMPONENTS, delegate { new ReorderComponents(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        tb = new TextButton(
            new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 19), new Vector2(10, 2f),
            CommonStringKeys.CANCEL, delegate { Destroyer.Dialog(); });
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());
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
