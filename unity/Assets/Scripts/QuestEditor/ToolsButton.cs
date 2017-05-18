using UnityEngine;
using System.Collections;
using Assets.Scripts.Content;

// Special class for the Menu button present while in a quest
public class ToolsButton
{
    private StringKey TOOLS = new StringKey("val", "TOOLS");

    public ToolsButton()
    {
        Game game = Game.Get();
        if (!game.editMode) return;

        // Back button
        TextButton tb = new TextButton(new Vector2(UIScaler.GetRight(-4), 0), new Vector2(4, 1), CommonStringKeys.BACK, delegate { QuestEditorData.Back(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.QUESTUI);

        tb = new TextButton(new Vector2(UIScaler.GetRight(-10), 0), new Vector2(6, 1), new StringKey("val", "COMPONENTS"), delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.QUESTUI);

        tb = new TextButton(new Vector2(UIScaler.GetRight(-14), 0), new Vector2(4, 1), TOOLS, delegate { EditorTools.Create(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag(Game.QUESTUI);

    }
}
