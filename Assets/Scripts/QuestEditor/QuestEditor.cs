using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestEditor {

    public static void Begin()
    {
        Game game = Game.Get();
        game.editMode = true;

        Reload();

        new MenuButton();

        game.qed = new QuestEditorData();
    }

    public static void Reload()
    {
        Destroyer.Dialog();

        Game game = Game.Get();
        game.quest.RemoveAll();

        // Clean up everything marked as 'editor'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("editor"))
            Object.Destroy(go);

        game.qd = new QuestData(game.qd.questPath);

        game.quest.RemoveAll();

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.qd.components)
        {
            game.quest.Add(kv.Key);
        }
        game.quest.ChangeAlphaAll(0.2f);

        game.qed = new QuestEditorData();
    }

    public static void Save()
    {
        Game game = Game.Get();
        string content = "; Saved by version: " + game.version + System.Environment.NewLine;
        content += game.qd.quest.ToString() + System.Environment.NewLine;

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.qd.components)
        {
            content += System.Environment.NewLine;
            content += kv.Value.ToString();
        }

        try
        {
            System.IO.File.WriteAllText(game.qd.questPath, content);
        }
        catch (System.Exception)
        {
            Debug.Log("Error: Failed to save quest in editor.");
            Application.Quit();
        }

        Reload();
    }
}
