using UnityEngine;
using System.Text;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentToken : EditorComponent
{
    QuestData.Token tokenComponent;
    EditorSelectionList typeList;

    public EditorComponentToken(string nameIn) : base()
    {
        Game game = Game.Get();
        tokenComponent = game.quest.qd.components[nameIn] as QuestData.Token;
        component = tokenComponent;
        name = component.sectionName;
        Update();
    }
    
    override public void Update()
    {
        base.Update();
        Game game = Game.Get();
        CameraController.SetCamera(tokenComponent.location);

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), CommonStringKeys.TOKEN, delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1), 
            new StringKey(null, name.Substring("Token".Length),false), delegate { QuestEditorData.ListToken(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), CommonStringKeys.E, delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");


        DialogBox db = new DialogBox(new Vector2(0, 2), new Vector2(4, 1), CommonStringKeys.POSITION);
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 2), new Vector2(1, 1), CommonStringKeys.POSITION_SNAP , delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(5, 2), new Vector2(1, 1), CommonStringKeys.POSITION_FREE, delegate { GetPosition(false); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 4), new Vector2(8, 1),
            new StringKey("val", "ROTATION", 
            new StringKey(null, tokenComponent.rotation.ToString(), false)),
            delegate { Rotate(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 6), new Vector2(8, 1), 
            new StringKey(null, tokenComponent.tokenName,false), delegate { Type(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 8), new Vector2(8, 1), CommonStringKeys.EVENT, delegate { QuestEditorData.SelectAsEvent(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        game.quest.ChangeAlpha(tokenComponent.sectionName, 1f);
    }

    public void Rotate()
    {
        tokenComponent.rotation += 90;
        if (tokenComponent.rotation > 300)
        {
            tokenComponent.rotation = 0;
        }
        Game.Get().quest.Remove(tokenComponent.sectionName);
        Game.Get().quest.Add(tokenComponent.sectionName);
        Update();
    }

    public void Type()
    {
        typeList = new EditorSelectionList(new StringKey("val","SELECT",CommonStringKeys.TOKEN), GetTokenNames(), delegate { SelectType(); });
        typeList.SelectItem();
    }

    public static List<EditorSelectionList.SelectionListEntry> GetTokenNames()
    {
        List<EditorSelectionList.SelectionListEntry> names = new List<EditorSelectionList.SelectionListEntry>();

        foreach (KeyValuePair<string, TokenData> kv in Game.Get().cd.tokens)
        {
            StringBuilder display = new StringBuilder().Append(kv.Key);
            //StringBuilder localizedDisplay = new StringBuilder().Append(kv.Value.name.Translate());
            foreach (string s in kv.Value.sets)
            {
                display.Append(" ").Append(s);
                //localizedDisplay.Append(" ").Append(new StringKey("val", s).Translate());
            }
            names.Add( new EditorSelectionList.SelectionListEntry(display.ToString()));
        }
        return names;
    }

    public void SelectType()
    {
        tokenComponent.tokenName = typeList.selection.Split(" ".ToCharArray())[0];
        Game.Get().quest.Remove(tokenComponent.sectionName);
        Game.Get().quest.Add(tokenComponent.sectionName);
        Update();
    }
}
