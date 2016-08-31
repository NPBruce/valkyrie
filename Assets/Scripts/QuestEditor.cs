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

        // Clean up everything marked as 'board'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("board"))
            Object.Destroy(go);

        // Clean up everything marked as 'editor'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("editor"))
            Object.Destroy(go);

        Game game = Game.Get();

        game.qd = new QuestData(game.qd.questPath);

        foreach (KeyValuePair<string, QuestData.QuestComponent> qc in game.qd.components)
        {
            qc.Value.Draw();
            qc.Value.SetVisible(.2f);
        }

        game.qed = new QuestEditorData();
    }

    public static void Save()
    {
        Game game = Game.Get();
        string content = "; Saved by version: " + Game.version + System.Environment.NewLine;
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
            Debug.Log("FDFDFD");
        }

        Reload();
    }
}
