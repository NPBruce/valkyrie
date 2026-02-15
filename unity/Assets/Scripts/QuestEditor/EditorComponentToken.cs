using UnityEngine;
using System.Text;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

public class EditorComponentToken : EditorComponentEvent
{
    QuestData.Token tokenComponent;

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

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 6, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "ROTATION")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(6, offset, 3, 1);
        ui.SetText(tokenComponent.rotation.ToString());
        ui.SetButton(delegate { Rotate(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 6, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "SIZE")));

        StringKey sizeKey = new StringKey("val", "DEFAULT");
        if (!tokenComponent.tokenSize.Equals(""))
        {
            sizeKey = new StringKey("val", tokenComponent.tokenSize.ToUpper());
        }

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(6, offset, 5, 1);
        ui.SetText(sizeKey);
        ui.SetButton(delegate { CycleSize(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.TYPE));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset, 12, 1);
        ui.SetText(tokenComponent.tokenName);
        ui.SetButton(delegate { Type(); });
        new UIElementBorder(ui);
        offset += 2;

        game.CurrentQuest.ChangeAlpha(tokenComponent.sectionName, 1f);

        return offset;
    }

    override public float AddEventTrigger(float offset)
    {
        return offset;
    }

    public override float AddEventVarConditionComponents(float yOffset)
    {
        return yOffset;
    }

    public void Rotate()
    {
        tokenComponent.rotation += 90;
        if (tokenComponent.rotation > 300)
        {
            tokenComponent.rotation = 0;
        }
        Game.Get().CurrentQuest.Remove(tokenComponent.sectionName);
        Game.Get().CurrentQuest.Add(tokenComponent.sectionName);
        Update();
    }

    public void Type()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();
        UIWindowSelectionListTraits select = new UIWindowSelectionListImage(SelectType, new StringKey("val", "SELECT", CommonStringKeys.TOKEN));

        select.AddItem(CommonStringKeys.NONE.Translate(), "{NONE}", true);

        foreach (TokenData kv in game.cd.Values<TokenData>())
        {
            select.AddItem(kv);
        }
        select.ExcludeExpansions();
        select.Draw();
    }

    public void SelectType(string token)
    {
        tokenComponent.tokenName = token.Split(" ".ToCharArray())[0];
        Game.Get().CurrentQuest.Remove(tokenComponent.sectionName);
        Game.Get().CurrentQuest.Add(tokenComponent.sectionName);
        Update();
    }

    public void CycleSize()
    {
        if (tokenComponent.tokenSize.Equals(""))
        {
            tokenComponent.tokenSize = "small";
        }
        else if (tokenComponent.tokenSize.Equals("small"))
        {
            tokenComponent.tokenSize = "medium";
        }
        else if (tokenComponent.tokenSize.Equals("medium"))
        {
            tokenComponent.tokenSize = "huge";
        }
        else if (tokenComponent.tokenSize.Equals("huge"))
        {
            tokenComponent.tokenSize = "massive";
        }
        else
        {
            tokenComponent.tokenSize = "";
        }
        Game.Get().CurrentQuest.Remove(tokenComponent.sectionName);
        Game.Get().CurrentQuest.Add(tokenComponent.sectionName);
        Update();
    }
}
