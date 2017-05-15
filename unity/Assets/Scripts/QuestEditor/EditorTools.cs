using UnityEngine;
using Assets.Scripts.Content;

public class EditorTools
{
    private static readonly StringKey VALIDATE_SCENARIO = new StringKey("val", "VALIDATE_SCENARIO");
    private static readonly StringKey OPTIMIZE_LOCALIZATION = new StringKey("val", "OPTIMIZE_LOCALIZATION");
    private static readonly StringKey EXPORT_LOCALIZATION = new StringKey("val", "EXPORT_LOCALIZATION");
    private static readonly StringKey IMPORT_LOCALIZATION = new StringKey("val", "IMPORT_LOCALIZATION");

    public static void Create()
    {

        Game game = Game.Get();
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            Destroyer.Dialog();
        }

        // Menu border
        DialogBox db = new DialogBox(new Vector2(2,2),new Vector2(UIScaler.GetWidthUnits() - 4, UIScaler.GetHeightUnits() - 4), StringKey.NULL);
        db.AddBorder();
        db.SetFont(game.gameType.GetHeaderFont());

        TextButton tb = new TextButton(
            new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 8), new Vector2(10, 2f),
            VALIDATE_SCENARIO, delegate { Validate_Scenario(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        tb = new TextButton(
            new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 11), new Vector2(10, 2f),
            OPTIMIZE_LOCALIZATION, delegate { Optimize_Localization(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        tb = new TextButton(
            new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 14), new Vector2(10, 2f),
            EXPORT_LOCALIZATION, delegate { Export_Localization(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        tb = new TextButton(
            new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 17), new Vector2(10, 2f),
            IMPORT_LOCALIZATION, delegate { Import_Localization(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        tb = new TextButton(
            new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 20), new Vector2(10, 2f),
            CommonStringKeys.CANCEL, delegate { Destroyer.Dialog(); }, Color.white);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
        tb.SetFont(game.gameType.GetHeaderFont());

        // Menu border
        DialogBox logBox = new DialogBox(new Vector2(2, 2), new Vector2(UIScaler.GetWidthUnits() - 4, UIScaler.GetHeightUnits() - 4), StringKey.NULL);
        logBox.AddBorder();
        logBox.SetFont(game.gameType.GetHeaderFont());
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

    /// <summary>
    /// Create a single language localization file to be
    /// easily translatable. This fila will ignore {qst:*} texts
    /// </summary>
    private static void Export_Localization()
    {

    }

    /// <summary>
    /// Merge a single language localization file into actual localization file.
    /// If already exists, it will be overwritten. (WARN before)
    /// </summary>
    private static void Import_Localization()
    {

    }
}

