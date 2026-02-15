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
        ui.SetLocation(0, offset, 5, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "CUSTOM_IMAGE")));

        if (tokenComponent.customImage.Length > 0)
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(5, offset, 11, 1);
            ui.SetText(tokenComponent.customImage);
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
        ui.SetLocation(0, offset, 5, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "CLICK_BEHAVIOR")));

        // Click Behavior Button
        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(5, offset, 14, 1);
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

    public void ToggleClickEffect()
    {
        tokenComponent.enableClick = !tokenComponent.enableClick;
        Update();
    }

    public void SetCustomImage()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        UIWindowSelectionListImage select = new UIWindowSelectionListImage(SelectCustomImage, new StringKey("val", "SELECT", new StringKey("val", "CUSTOM_IMAGE")));
        select.AddItem("{NONE}", "", true);

        string relativePath = new System.IO.FileInfo(System.IO.Path.GetDirectoryName(Game.Get().CurrentQuest.qd.questPath)).FullName;
        
        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.SOURCE.Translate(), new string[] { CommonStringKeys.FILE.Translate() });

        foreach (string s in System.IO.Directory.GetFiles(relativePath, "*.png", System.IO.SearchOption.AllDirectories))
        {
            select.AddItem(s.Substring(relativePath.Length + 1), traits);
        }
        foreach (string s in System.IO.Directory.GetFiles(relativePath, "*.jpg", System.IO.SearchOption.AllDirectories))
        {
            select.AddItem(s.Substring(relativePath.Length + 1), traits);
        }
        foreach (ImageData imageData in Game.Get().cd.Values<ImageData>())
        {
            select.AddItem(imageData);
        }
        select.ExcludeExpansions();
        select.Draw();
    }

    public void SelectCustomImage(string image)
    {
        tokenComponent.customImage = image;
        tokenComponent.tokenName = ""; // Clear Type
        Game.Get().CurrentQuest.Remove(tokenComponent.sectionName);
        Game.Get().CurrentQuest.Add(tokenComponent.sectionName);
        Update();
    }

    public void ClearCustomImage()
    {
        tokenComponent.customImage = "";
        Update();
    }
}
