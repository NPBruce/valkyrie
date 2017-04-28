using UnityEngine;
using System.Text;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentUI : EditorComponent
{
    QuestData.UI uiComponent;
    EditorSelectionList typeList;

    public EditorComponentUI(string nameIn) : base()
    {
        Game game = Game.Get();
        uiComponent = game.quest.qd.components[nameIn] as QuestData.UI;
        component = uiComponent;
        name = component.sectionName;
        Update();
    }

    override public void Update()
    {
        base.Update();
        Game game = Game.Get();

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(2, 1), CommonStringKeys.UI, delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(2, 0), new Vector2(17, 1),
            new StringKey(null, name.Substring("UI".Length), false), delegate { QuestEditorData.ListUI(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), CommonStringKeys.E, delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 2), new Vector2(20, 1),
            new StringKey(null, uiComponent.imageName, false), delegate { ; });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 4), new Vector2(8, 1), CommonStringKeys.EVENT, delegate { QuestEditorData.SelectAsEvent(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");
    }
}
