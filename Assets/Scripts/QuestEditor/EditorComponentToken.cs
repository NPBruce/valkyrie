using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorComponentToken : EditorComponent
{
    QuestData.Token tokenComponent;
    EditorSelectionList typeList;

    public EditorComponentToken(string name) : base()
    {
        Game game = Game.Get();
        tokenComponent = game.quest.qd.components[name] as QuestData.Token;
        component = tokenComponent;
        name = component.name;
        Update();
    }
    
    override public void Update()
    {
        base.Update();
        Game game = Game.Get();
        CameraController.SetCamera(tokenComponent.location);

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), "Token", delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1), name.Substring("Token".Length), delegate { QuestEditorData.ListToken(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");


        DialogBox db = new DialogBox(new Vector2(0, 2), new Vector2(4, 1), "Position");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 2), new Vector2(1, 1), "><", delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(5, 2), new Vector2(1, 1), "~", delegate { GetPosition(false); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 4), new Vector2(8, 1), tokenComponent.tokenName, delegate { Type(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 6), new Vector2(8, 1), "Event", delegate { QuestEditorData.SelectAsEvent(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        game.quest.ChangeAlpha(tokenComponent.name, 1f);
    }

    public void Type()
    {
        typeList = new EditorSelectionList("Select Token", GetTokenNames(), delegate { SelectType(); });
        typeList.SelectItem();
    }

    public static List<string> GetTokenNames()
    {
        List<string> names = new List<string>();

        foreach (KeyValuePair<string, TokenData> kv in Game.Get().cd.tokens)
        {
            string display = kv.Key;
            foreach (string s in kv.Value.sets)
            {
                display += " " + s;
            }
            names.Add(display);
        }
        return names;
    }

    public void SelectType()
    {
        tokenComponent.tokenName = typeList.selection.Split(" ".ToCharArray())[0];
        Game.Get().quest.Remove(tokenComponent.name);
        Game.Get().quest.Add(tokenComponent.name);
        Update();
    }
}
