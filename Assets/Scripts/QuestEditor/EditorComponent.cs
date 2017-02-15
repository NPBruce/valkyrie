using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorComponent {
    public QuestData.QuestComponent component;
    public bool gettingPosition = false;
    public bool gettingPositionSnap = false;

    QuestEditorTextEdit rename;

    virtual public void Update()
    {
        Clean();

        tb = new TextButton(new Vector2(0, 29), new Vector2(3, 1), "Back", delegate { QuestEditorData.Back(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");
    }

    public void Clean()
    {
        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        // Clean up everything marked as 'editor'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("editor"))
            Object.Destroy(go);

        Game.Get().quest.ChangeAlphaAll(0.2f);
    }

    virtual public void MouseDown()
    {
        if (!gettingPosition) return;

        component.location = game.cc.GetMouseBoardPlane();
        if (gettingPositionSnap)
        {
            component.location = game.cc.GetMouseBoardRounded(game.gameType.SelectionRound());
            if (component is QuestData.Tile)
            {
                component.location = game.cc.GetMouseBoardRounded(game.gameType.TileRound());
            }
        }
        gettingPosition = false;
        Game.Get().quest.Remove(component.name);
        Game.Get().quest.Add(component.name);
        Update();
    }

    virtual public void GetPosition(bool snap=true)
    {
        gettingPosition = true;
        gettingPositionSnap = snap;
    }

    public void Rename()
    {
        string name = component.name.Substring(component.typeDynamic.Length);
        rename =  new QuestEditorTextEdit("Component Name:", name, delegate { RenameComponentFinished(); });
        rename.EditText();
    }

    public void RenameFinished()
    {
        string newName = System.Text.RegularExpressions.Regex.Replace(rename.value, "[^A-Za-z0-9_]", "");
        if (newName.Equals("")) return;
        string baseName = component.typeDynamic + newName;
        string name = baseName;
        Game game = Game.Get();
        int i = 0;
        while (game.quest.qd.components.ContainsKey(name))
        {
            name = baseName + i++;
        }

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            kv.Value.ChangeReference(component.name, name);
        }

        game.quest.qd.components.Remove(component.name);
        game.quest.Remove(component.name);
        component.name = name;
        game.quest.qd.components.Add(component.name, component);
        game.quest.Add(component.name);
        Update();
    }

}