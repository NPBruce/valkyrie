using UnityEngine;
using System.Text;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

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

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0, offset, 6, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "ROTATION")));

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(6, offset, 3, 1);
        ui.SetText(tokenComponent.rotation.ToString());
        ui.SetButton(delegate { Rotate(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "TYPE")));

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(4, offset, 12, 1);
        ui.SetText(tokenComponent.tokenName);
        ui.SetButton(delegate { Type(); });
        new UIElementBorder(ui);
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
