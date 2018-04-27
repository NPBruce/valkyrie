using UnityEngine;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

public class EditorComponentVar : EditorComponent
{
    QuestData.VarDefinition varComponent;

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
        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5, offset, 8, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "VAR_TYPE")));

        StringKey rotateKey = new StringKey("val","RIGHT");
        if (mPlaceComponent.rotate)
        {
            rotateKey = new StringKey("val", "DOWN");
        }

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(8, offset, 8, 1);
        ui.SetText(new StringKey("val","VAR_TYPE" + varComponent.variableType));
        ui.SetButton(CycleVarType);
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 8, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "VAR_CAMPAIGN"));

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

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 8, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "VAR_INITIALISE"));

        string initString = varComponent.initialise;
        if (varComponent.variableType.equals("bool"))
        {
            if (varComponent.initialise == 0)
            {
                initString = ui.SetText(new StringKey("val","FALSE"));
            }
            else
            {
                initString = ui.SetText(new StringKey("val","TRUE"));
            }
        }
        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(8, offset, 4, 1);
        ui.SetText(initString);
        ui.SetButton(SetInitialise);
        new UIElementBorder(ui);

        offset += 2;

        return offset;
    }

    public void CycleVarType()
    {
        if (varComponent.variableType.equals("float"))
        {
            varComponent.SetVariableType("int");
        }
        else if (varComponent.variableType.equals("int"))
        {
            varComponent.SetVariableType("bool");
        }
        else if (varComponent.variableType.equals("bool"))
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
}
