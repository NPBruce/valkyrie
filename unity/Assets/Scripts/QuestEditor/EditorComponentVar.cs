using UnityEngine;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

public class EditorComponentVar : EditorComponent
{
    QuestData.VarDefinition varComponent;

    UIElementEditable initialiseUIE;
    UIElementEditable minimumUIE;
    UIElementEditable maximumUIE;

    public EditorComponentVar(string nameIn) : base()
    {
        Game game = Game.Get();
        varComponent = game.quest.qd.components[nameIn] as QuestData.VarDefinition;
        component = varComponent;
        name = component.sectionName;
        Update();
    }

    override protected void RefreshReference()
    {
        base.RefreshReference();
        varComponent = component as QuestData.VarDefinition;
    }

    override public float AddSubComponents(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 8, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "VAR_TYPE")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(8, offset, 8, 1);
        ui.SetText(new StringKey("val","VAR_TYPE_" + varComponent.variableType));
        ui.SetButton(CycleVarType);
        new UIElementBorder(ui);
        offset += 2;

        AddCampaignControl(offset);
        AddInitialiseControl(offset);
        AddLimitsControl(offset);

        return offset;
    }

    protected float AddCampaignControl(float offset)
    {
        if (varComponent.variableType.Equals("trigger")) return offset;

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 8, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "VAR_CAMPAIGN")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(8, offset, 4, 1);
        if (varComponent.campaign)
        {
            ui.SetText(new StringKey("val","TRUE"));
        }
        else
        {
            ui.SetText(new StringKey("val","FALSE"));
        }
        ui.SetButton(CampaignToggle);
        new UIElementBorder(ui);

        return offset + 2;
    }

    protected float AddInitialiseControl(float offset)
    {
        if (varComponent.variableType.Equals("trigger")) return offset;
        if (varComponent.campaign) return offset;
        
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 8, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "VAR_INITIALISE")));

        if (varComponent.variableType.Equals("bool"))
        {
            string initString = new StringKey("val","FALSE").Translate();
            if (varComponent.initialise != 0)
            {
                initString = new StringKey("val","TRUE").Translate();
            }
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(8, offset, 4, 1);
            ui.SetText(initString);
            ui.SetButton(SetInitialise);
            new UIElementBorder(ui);
        }
        else
        {
            initialiseUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
            initialiseUIE.SetLocation(8, offset, 4, 1);
            initialiseUIE.SetText(varComponent.initialise.ToString());
            initialiseUIE.SetButton(SetInitialise);
            initialiseUIE.SetSingleLine();
            new UIElementBorder(initialiseUIE);
        }

        return offset + 2;
    }

    protected float AddLimitsControl(float offset)
    {
        if (varComponent.variableType.Equals("trigger")) return offset;
        if (varComponent.variableType.Equals("bool")) return offset;

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 8, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "VAR_MINIMUM")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(8, offset, 4, 1);
        string minEnableString = new StringKey("val","FALSE").Translate();
        if (varComponent.minimumUsed)
        {
            minEnableString = new StringKey("val","TRUE").Translate();
        }
        ui.SetText(minEnableString);
        ui.SetButton(SetMinimumEnable);
        new UIElementBorder(ui);

        if (varComponent.minimumUsed)
        {
            minimumUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
            minimumUIE.SetLocation(12.5f, offset, 4, 1);
            minimumUIE.SetText(varComponent.minimum.ToString());
            minimumUIE.SetButton(SetMinimum);
            minimumUIE.SetSingleLine();
            new UIElementBorder(minimumUIE);
        }

        offset += 2;

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 8, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "VAR_MAXIMUM")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(8, offset, 4, 1);
        string maxEnableString = new StringKey("val","FALSE").Translate();
        if (varComponent.maximumUsed)
        {
            maxEnableString = new StringKey("val","TRUE").Translate();
        }
        ui.SetText(maxEnableString);
        ui.SetButton(SetMaximumEnable);
        new UIElementBorder(ui);

        if (varComponent.maximumUsed)
        {
            maximumUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
            maximumUIE.SetLocation(12.5f, offset, 4, 1);
            maximumUIE.SetText(varComponent.maximum.ToString());
            maximumUIE.SetButton(SetMaximum);
            maximumUIE.SetSingleLine();
            new UIElementBorder(maximumUIE);
        }

        return offset + 2;
    }

    public void CycleVarType()
    {
        if (varComponent.variableType.Equals("float"))
        {
            varComponent.SetVariableType("int");
        }
        else if (varComponent.variableType.Equals("int"))
        {
            varComponent.SetVariableType("bool");
        }
        else if (varComponent.variableType.Equals("bool"))
        {
            varComponent.SetVariableType("trigger");
        }
        else
        {
            varComponent.SetVariableType("float");
        }
        Update();
    }

    public void CampaignToggle()
    {
        varComponent.campaign = !varComponent.campaign;
        Update();
    }

    public void SetInitialise()
    {
        if (varComponent.variableType.Equals("triigger")) return offset;

        if (varComponent.variableType.Equals("bool"))
        {
            if (varComponent.initialise == 0)
            {
                varComponent.initialise = 1;
            }
            else
            {
                varComponent.initialise = 0;
            }
        }
        else
        {
            float.TryParse(initialiseUIE.GetText(), out eventComponent.initialise);
        }
    }

    public void SetMinimumEnable()
    {
        varComponent.minimumUsed = !varComponent.minimumUsed;
        Update();
    }

    public void SetMaximumEnable()
    {
        varComponent.maximumUsed = !varComponent.maximumUsed;
        Update();
    }

    public void SetMinimum()
    {
        float.TryParse(minimumUIE.GetText(), out eventComponent.minimum);
        Update();
    }

    public void SetMaximum()
    {
        float.TryParse(maximumUIE.GetText(), out eventComponent.maximum);
        Update();
    }
}
