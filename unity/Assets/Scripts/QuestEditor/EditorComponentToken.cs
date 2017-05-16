using UnityEngine;
using System.Text;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentToken : EditorComponentEvent
{
    QuestData.Token tokenComponent;
    EditorSelectionList typeList;

    public EditorComponentToken(string nameIn) : base(nameIn)
    {
    }

    override public void Highlight()
    {
        CameraController.SetCamera(component.location);
    }

    override public void AddLocationType(float offset)
    {
    }
    
    override public float AddSubEventComponents(float offset)
    {
        tokenComponent = component as QuestData.Token;

        DialogBox db = new DialogBox(new Vector2(0, offset), new Vector2(6, 1), new StringKey("val", "X_COLON", new StringKey("val", "ROTATION")));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);
        TextButton tb = new TextButton(new Vector2(6, offset), new Vector2(3, 1),
            new StringKey(null, tokenComponent.rotation.ToString() + "˚", false), delegate { Rotate(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);
        offset += 2;

        db = new DialogBox(new Vector2(0, offset), new Vector2(4, 1), new StringKey("val", "X_COLON", new StringKey("val", "TYPE")));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);
        tb = new TextButton(new Vector2(4, offset), new Vector2(12, 1), 
            new StringKey(null, tokenComponent.tokenName,false), delegate { Type(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);
        offset += 2;

        game.quest.ChangeAlpha(tokenComponent.sectionName, 1f);

        return offset;
    }

    override public float AddEventTrigger(float offset)
    {
        return offset;
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
