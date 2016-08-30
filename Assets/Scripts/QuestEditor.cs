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

        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        // Clean up everything marked as 'board'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("board"))
            Object.Destroy(go);

        // Clean up everything marked as 'questui'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("questui"))
            Object.Destroy(go);

        Game game = Game.Get();

        game.qd = new QuestData(game.qd.questPath);

        foreach (KeyValuePair<string, QuestData.QuestComponent> qc in game.qd.components)
        {
            qc.Value.SetVisible(true);
            qc.Value.SetVisible(.2f);
        }

        game.qed = new QuestEditorData();
    }

    public static void Save()
    {
        //Fixme
    }
}
