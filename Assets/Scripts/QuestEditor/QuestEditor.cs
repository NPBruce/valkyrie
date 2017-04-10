using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Assets.Scripts.Content;

// Quest editor static helper class
public class QuestEditor {

    // start editing a quest
    public static void Begin()
    {
        Game game = Game.Get();
        game.editMode = true;

        new MenuButton();

        // re-read quest data
        Reload();
    }

    // Reload a quest from file
    public static void Reload()
    {
        Destroyer.Dialog();

        Game game = Game.Get();
        // Remove all current components
        game.quest.RemoveAll();

        // Clean up everything marked as 'editor'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("editor"))
            Object.Destroy(go);

        // Read from file
        game.quest.qd = new QuestData(game.quest.qd.questPath);

        // Is this needed?
        game.quest.RemoveAll();

        // Add all components to the quest
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            game.quest.Add(kv.Key);
        }
        // Set all components to mostly transparent
        game.quest.ChangeAlphaAll(0.2f);

        // Create a new QED
        game.qed = new QuestEditorData();
    }

    // Save the quest
    public static void Save()
    {
        Game game = Game.Get();
        // Add a comment at the start of the quest with the editor version
        StringBuilder content = new StringBuilder()
            .Append("; Saved by version: ")
            .AppendLine(game.version);

        // Save quest meta content to a string
        content.AppendLine(game.quest.qd.quest.ToString());

        // Add all quest components
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            // Skip peril, not a quest component
            if (!(kv.Value is PerilData))
            {
                content.AppendLine().Append(kv.Value);
            }
        }

        // Write to disk
        try
        {
            string ini_file = content.ToString();
            List<string> localization_file = null;
            File.WriteAllText(game.quest.qd.questPath, ini_file);
            if (LocalizationRead.scenarioDict != null)
            {
                localization_file = LocalizationRead.scenarioDict.Serialize();
                removeUnusedStringKeys(localization_file, ini_file);

                File.WriteAllText(
                    Path.GetDirectoryName(game.quest.qd.questPath) + "/Localization.txt",
                    string.Join(System.Environment.NewLine, localization_file.ToArray()));
            }
        }
        catch (System.Exception)
        {
            ValkyrieDebug.Log("Error: Failed to save quest in editor.");
            Application.Quit();
        }

        // Reload quest
        Reload();
    }

    /// <summary>
    /// Find each stringkey in dictionary inside the ini file. If doesn't appear, it is unused, can be removed
    /// </summary>
    /// <param name="localization_file"></param>
    /// <param name="ini_file"></param>
    public static void removeUnusedStringKeys(List<string> localization_file, string ini_file)
    {
        // Search each line except first one
        for (int pos = localization_file.Count - 1; pos > 1; pos--)
        {
            string key = "{qst:" + localization_file[pos].Split(',')[0] + "}";
            if (!ini_file.Contains(key))
            {
                localization_file.RemoveAt(pos);
            }
        }
    }
}
