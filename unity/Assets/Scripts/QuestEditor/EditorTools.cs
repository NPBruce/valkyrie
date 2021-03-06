﻿using UnityEngine;
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

        // save content before creating the package
        QuestEditor.Save();

        string packageName = Path.GetFileName(Path.GetDirectoryName(game.CurrentQuest.qd.questPath));
        try
        {
            string desktopDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            string destination = Path.Combine(desktopDir, packageName);
            int postfix = 2;
            while (Directory.Exists(destination))
            {
                destination = Path.Combine(desktopDir, packageName + postfix++);
            }
            Directory.CreateDirectory(destination);

            string packageFile = Path.Combine(destination, packageName + ".valkyrie");

            using (var zip = new ZipFile())
            {
                zip.AddDirectory(Path.GetDirectoryName(game.CurrentQuest.qd.questPath));
                zip.Save(packageFile);
            }

            // Append sha version
            using (FileStream stream = File.OpenRead(packageFile))
            {
                byte[] checksum = SHA256Managed.Create().ComputeHash(stream);
                game.CurrentQuest.qd.quest.version = System.BitConverter.ToString(checksum);
            }

            string icon = game.CurrentQuest.qd.quest.image.Replace('\\', '/');
            if (icon.Length > 0)
            {
                string iconName = Path.GetFileName(icon);
                // Temp hack to get ToString to output local file
                game.CurrentQuest.qd.quest.image = iconName;
                string src = Path.Combine(Path.GetDirectoryName(game.CurrentQuest.qd.questPath), icon);
                string dest = Path.Combine(destination, iconName);
                File.Copy(src, dest);
            }
            string manifest = game.CurrentQuest.qd.quest.ToString();
            // Restore icon
            game.CurrentQuest.qd.quest.image = icon;

            foreach (KeyValuePair<string, string> kv in LocalizationRead.selectDictionary("qst").ExtractAllMatches("quest.name"))
            {
                manifest += "name." + kv.Key + "=" + kv.Value + System.Environment.NewLine;
            }

            foreach (KeyValuePair<string, string> kv in LocalizationRead.selectDictionary("qst").ExtractAllMatches("quest.synopsys"))
            {
                manifest += "synopsys." + kv.Key + "=" + kv.Value.Replace("\n", "").Replace("\r", "") + System.Environment.NewLine;
            }

            foreach (KeyValuePair<string, string> kv in LocalizationRead.selectDictionary("qst").ExtractAllMatches("quest.description"))
            {
                manifest += "description." + kv.Key + "=" + kv.Value.Replace("\n", "\\n").Replace("\r", "") + System.Environment.NewLine;
            }

            foreach (KeyValuePair<string, string> kv in LocalizationRead.selectDictionary("qst").ExtractAllMatches("quest.authors"))
            {
                manifest += "authors." + kv.Key + "=" + kv.Value.Replace("\n", "\\n").Replace("\\r", "") + System.Environment.NewLine;
            }

            foreach (KeyValuePair<string, string> kv in LocalizationRead.selectDictionary("qst").ExtractAllMatches("quest.authors_short"))
            {
                manifest += "authors_short." + kv.Key + "=" + kv.Value.Replace("\n", "").Replace("\r", "") + System.Environment.NewLine;
            }

            File.WriteAllText(Path.Combine(destination, packageName + ".ini"), manifest);
        }
        catch (System.IO.IOException e)
        {
            ValkyrieDebug.Log("Warning: Unable to write to valkyrie package." + e.Message);
        }
    }
}
