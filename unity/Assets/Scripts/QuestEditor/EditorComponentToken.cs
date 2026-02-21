using UnityEngine;
using System.Text;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
using System.Globalization;

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
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "ROTATION")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset, 3, 1);
        ui.SetText(tokenComponent.rotation.ToString());
        ui.SetButton(delegate { Rotate(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "SIZE")));
        
        StringKey sizeKey = new StringKey("val", "ACTUAL");
        if (!tokenComponent.tokenSize.Equals(""))
        {
            if (float.TryParse(tokenComponent.tokenSize, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            {
                sizeKey = new StringKey(null, tokenComponent.tokenSize, false);
            }
            else
            {
                sizeKey = new StringKey("val", tokenComponent.tokenSize.ToUpper());
            }
        }

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset, 5, 1);
        ui.SetText(sizeKey);
        ui.SetButton(delegate { ClickSize(); });
        new UIElementBorder(ui);
        offset += 2;

        // Type Label (disable if custom image set?)
        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.TYPE));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset, 12, 1);
        if (tokenComponent.customImage.Length > 0)
        {
             ui.SetText(CommonStringKeys.NONE);
             ui.SetButton(delegate { }); // No action
             ui.SetBGColor(Color.grey);
        }
        else
        {
             ui.SetText(tokenComponent.tokenName);
             ui.SetButton(delegate { Type(); });
        }
        new UIElementBorder(ui);
        offset += 2;

        // Custom Image
        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "CUSTOM_IMAGE")));

        if (tokenComponent.customImage.Length > 0)
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(4, offset, 11, 1);
            ui.SetTextFileName(tokenComponent.customImage);
            ui.SetButton(delegate { SetCustomImage(); });
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(16.5f, offset, 3, 1);
            ui.SetText(CommonStringKeys.RESET);
            ui.SetButton(delegate { ClearCustomImage(); });
            new UIElementBorder(ui);
        }
        else
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(16.5f, offset, 3, 1);
            ui.SetText(CommonStringKeys.SET);
            ui.SetButton(delegate { SetCustomImage(); });
            new UIElementBorder(ui);
        }
        offset += 2;


        // Click Behavior Label
        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 4, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "CLICK_BEHAVIOR")));

        // Click Behavior Button
        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(4, offset, 14, 1);
        if (tokenComponent.enableClick)
        {
            ui.SetText(new StringKey("val", "CLICK_BLINK"));
        }
        else
        {
            ui.SetText(new StringKey("val", "CLICK_STATIC"));
        }
        ui.SetButton(delegate { ToggleClickEffect(); });
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
        foreach (MonsterData kv in game.cd.Values<MonsterData>())
        {
            select.AddItem(kv);
        }
        select.ExcludeExpansions();
        select.Draw();

        // Auto selection of source
        GenericData currentTokenData = null;
        if (game.cd.ContainsKey<TokenData>(tokenComponent.tokenName))
        {
            currentTokenData = game.cd.Get<TokenData>(tokenComponent.tokenName);
        }
        else if (game.cd.ContainsKey<MonsterData>(tokenComponent.tokenName))
        {
            currentTokenData = game.cd.Get<MonsterData>(tokenComponent.tokenName);
        }
        if (currentTokenData != null)
        {
            string setID = "";
            if (currentTokenData.sets.Count > 0)
            {
                setID = currentTokenData.sets[0];
            }
            if (setID.Equals(""))
            {
                setID = "base";
            }
            select.SelectTrait(CommonStringKeys.SOURCE.Translate(), new StringKey("val", setID).Translate());
        }
    }

    public void SelectType(string token)
    {
        tokenComponent.tokenName = token.Split(" ".ToCharArray())[0];
        Game.Get().CurrentQuest.Remove(tokenComponent.sectionName);
        Game.Get().CurrentQuest.Add(tokenComponent.sectionName);
        Update();
    }

    public void ClickSize()
    {
        UIWindowSelectionList select = new UIWindowSelectionList(SelectSize, new StringKey("val", "SELECT", new StringKey("val", "SIZE")));
        
        select.AddItem(new StringKey("val", "SMALL").Translate(), "small");
        select.AddItem(new StringKey("val", "MEDIUM").Translate(), "medium");
        select.AddItem(new StringKey("val", "HUGE").Translate(), "huge");
        select.AddItem(new StringKey("val", "MASSIVE").Translate(), "massive");
        select.AddItem(new StringKey("val", "ACTUAL").Translate(), "Actual");

        select.Draw();
    }

    public void SelectSize(string size)
    {
        tokenComponent.tokenSize = size;
        Game.Get().CurrentQuest.Remove(tokenComponent.sectionName);
        Game.Get().CurrentQuest.Add(tokenComponent.sectionName);
        Update();
    }

    public void ToggleClickEffect()
    {
        tokenComponent.enableClick = !tokenComponent.enableClick;
        Update();
    }

    public void SetCustomImage()
    {
        base.SetCustomImage(SelectCustomImage);
    }

    public void SelectCustomImage(string image)
    {
        if (image.Equals("{NONE}"))
        {
            tokenComponent.customImage = "";
        }
        else
        {
            tokenComponent.customImage = image;
            tokenComponent.tokenName = "TokenSearch";
            tokenComponent.tokenSize = "Actual";
        }
        Game.Get().CurrentQuest.Remove(tokenComponent.sectionName);
        Game.Get().CurrentQuest.Add(tokenComponent.sectionName);
        Update();
    }

    public void ClearCustomImage()
    {
        tokenComponent.customImage = "";
        tokenComponent.tokenName = "TokenSearch";
        Game.Get().CurrentQuest.Remove(tokenComponent.sectionName);
        Game.Get().CurrentQuest.Add(tokenComponent.sectionName);
        Update();
    }
}
