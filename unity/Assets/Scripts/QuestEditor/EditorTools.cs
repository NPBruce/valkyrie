using UnityEngine;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
using System.Security.Cryptography;
using System.IO;
using Ionic.Zip;
using ValkyrieTools;
using System.Collections.Generic;

public class EditorTools
{
    private static readonly StringKey VALIDATE_SCENARIO = new StringKey("val", "VALIDATE_SCENARIO");
    private static readonly StringKey OPTIMIZE_LOCALIZATION = new StringKey("val", "OPTIMIZE_LOCALIZATION");
    private static readonly StringKey CREATE_PACKAGE = new StringKey("val", "CREATE_PACKAGE");
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
        ui.SetLocation((UIScaler.GetWidthUnits() - 20) / 2, 7, 20, 16);
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation((UIScaler.GetWidthUnits() - 18) / 2, 8, 18, 2);
        ui.SetText(VALIDATE_SCENARIO, Color.grey);
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
        ui.SetButton(Validate_Scenario);
        new UIElementBorder(ui, Color.grey);

        ui = new UIElement();
        ui.SetLocation((UIScaler.GetWidthUnits() - 18) / 2, 11, 18, 2);
        ui.SetText(OPTIMIZE_LOCALIZATION, Color.grey);
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
        ui.SetButton(Optimize_Localization);
        new UIElementBorder(ui, Color.grey);

        ui = new UIElement();
        ui.SetLocation((UIScaler.GetWidthUnits() - 18) / 2, 14, 18, 2);
        ui.SetText(CREATE_PACKAGE);
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
        ui.SetButton(delegate { CreatePackage(); });
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation((UIScaler.GetWidthUnits() - 18) / 2, 17, 18, 2);
        ui.SetText(REORDER_COMPONENTS);
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
        ui.SetButton(delegate { new ReorderComponents(); });
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation((UIScaler.GetWidthUnits() - 10) / 2, 20, 10, 2);
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

    /// <summary>
    /// Create a .valkyrie package and associated meta data
    /// </summary>
    private static void CreatePackage()
    {
        Destroyer.Dialog();
        Game game = Game.Get();

        string packageName = Path.GetFileName(Path.GetDirectoryName(game.quest.qd.questPath));
        try
        {
            string destination = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/" + packageName;
            int postfix = 2;
            while (Directory.Exists(destination))
            {
                destination = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/" + packageName + postfix++;
            }
            Directory.CreateDirectory(destination);

            ZipFile zip = new ZipFile();
            zip.AddDirectory(Path.GetDirectoryName(game.quest.qd.questPath));
            zip.Save(destination + "/" + packageName + ".valkyrie");

            string icon = game.quest.qd.quest.image;
            if (icon.Length > 0)
            {
                string iconName = Path.GetFileName(icon);
                // Temp hack to get ToString to output local file
                game.quest.qd.quest.image = iconName;
                File.Copy(Path.Combine(Path.GetDirectoryName(game.quest.qd.questPath), icon), destination + "/" + iconName);
            }
            string manifest = game.quest.qd.quest.ToString();
            // Restore icon
            game.quest.qd.quest.image = icon;

            // Append sha version
            using (FileStream stream = File.OpenRead(destination + "/" + packageName + ".valkyrie"))
            {
                byte[] checksum = SHA256Managed.Create().ComputeHash(stream);
                manifest += "version=" + System.BitConverter.ToString(checksum) + "\n";
            }

            foreach (KeyValuePair<string, string> kv in LocalizationRead.selectDictionary("qst").ExtractAllMatches("quest.name"))
            {
                manifest += "name." + kv.Key + "=" + kv.Value + "\n";
            }


            File.WriteAllText(destination + "/" + packageName + ".ini", manifest);
        }
        catch (System.IO.IOException e)
        {
            ValkyrieDebug.Log("Warning: Unable to write to valkyrie package." + e.Message);
        }
    }
}
