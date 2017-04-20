using UnityEngine;
using Assets.Scripts.Content;
using System.Collections.Generic;
using ValkyrieTools;
using System.IO;
using System.Text;

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

        content.AppendLine("[QuestData]");
        content.AppendLine("tiles.ini");
        content.AppendLine("events.ini");
        content.AppendLine("tokens.ini");
        content.AppendLine("spawns.ini");
        content.AppendLine("monsters.ini");
        content.AppendLine("other.ini");

        StringBuilder tiles = new StringBuilder()
            .Append("; Saved by version: ")
            .AppendLine(game.version);

        StringBuilder events = new StringBuilder()
            .Append("; Saved by version: ")
            .AppendLine(game.version);

        StringBuilder tokens = new StringBuilder()
            .Append("; Saved by version: ")
            .AppendLine(game.version);

        StringBuilder spawns = new StringBuilder()
            .Append("; Saved by version: ")
            .AppendLine(game.version);

        StringBuilder monsters = new StringBuilder()
            .Append("; Saved by version: ")
            .AppendLine(game.version);

        StringBuilder other = new StringBuilder()
            .Append("; Saved by version: ")
            .AppendLine(game.version);

        // Add all quest components
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Tile)
            {
                tiles.AppendLine().Append(kv.Value);
            }
            else if (kv.Value is QuestData.Event && !kv.Value.GetType().IsSubclassOf(typeof(QuestData.Event)))
            {
                events.AppendLine().Append(kv.Value);
            }
            else if (kv.Value is QuestData.Token)
            {
                tokens.AppendLine().Append(kv.Value);
            }
            else if (kv.Value is QuestData.Spawn)
            {
                spawns.AppendLine().Append(kv.Value);
            }
            else if (kv.Value is QuestData.CustomMonster || kv.Value is QuestData.Activation)
            {
                monsters.AppendLine().Append(kv.Value);
            }
            // Skip peril, not a quest component
            else if (!(kv.Value is PerilData))
            {
                other.AppendLine().Append(kv.Value);
            }
        }

        // Write to disk
        try
        {
            File.WriteAllText(game.quest.qd.questPath, content.ToString());
            File.WriteAllText(Path.GetDirectoryName(game.quest.qd.questPath) + "/tiles.ini", tiles.ToString());
            File.WriteAllText(Path.GetDirectoryName(game.quest.qd.questPath) + "/events.ini", events.ToString());
            File.WriteAllText(Path.GetDirectoryName(game.quest.qd.questPath) + "/tokens.ini", tokens.ToString());
            File.WriteAllText(Path.GetDirectoryName(game.quest.qd.questPath) + "/spawns.ini", spawns.ToString());
            File.WriteAllText(Path.GetDirectoryName(game.quest.qd.questPath) + "/monsters.ini", monsters.ToString());
            File.WriteAllText(Path.GetDirectoryName(game.quest.qd.questPath) + "/other.ini", other.ToString());


            string ini_content = content.ToString();
            ini_content += tiles.ToString();
            ini_content += events.ToString();
            ini_content += tokens.ToString();
            ini_content += spawns.ToString();
            ini_content += monsters.ToString();
            ini_content += other.ToString();


            if (LocalizationRead.scenarioDict != null)
            {
                List<string> localization_file = LocalizationRead.scenarioDict.Serialize();

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
        for (int pos = localization_file.Count - 1; pos > 0; pos--)
        {
            string key = "{qst:" + localization_file[pos].Split(',')[0] + "}";
            if (!ini_file.Contains(key))
            {
                localization_file.RemoveAt(pos);
            }
        }
    }
}
