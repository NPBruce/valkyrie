using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

public class EditorComponentActivation : EditorComponent
{
    private readonly StringKey ABILITY = new StringKey("val","ABILITY");
    private readonly StringKey MONSTER_MASTER = new StringKey("val", "MONSTER_MASTER");
    private readonly StringKey MONSTER_MINION = new StringKey("val", "MONSTER_MINION");
    private readonly StringKey FIRST = new StringKey("val", "FIRST");
    private readonly StringKey NOT_FIRST = new StringKey("val", "NOT_FIRST");
    private readonly StringKey INITIAL_MESSAGE = new StringKey("val", "INITIAL_MESSAGE");
    private readonly StringKey UNABLE_BUTTON = new StringKey("val", "UNABLE_BUTTON");
    private readonly StringKey ATTACK_MESSAGE = new StringKey("val", "ATTACK_MESSAGE");
    private readonly StringKey NO_ATTACK_MESSAGE = new StringKey("val", "NO_ATTACK_MESSAGE");


    QuestData.Activation activationComponent;

    UIElementEditablePaneled abilityUIE;
    UIElementEditable moveButtonUIE;
    UIElementEditablePaneled masterActionsUIE;
    UIElementEditablePaneled minionActionsUIE;
    UIElementEditablePaneled moveUIE;

    public EditorComponentActivation(string nameIn) : base()
    {
        Game game = Game.Get();
        activationComponent = game.quest.qd.components[nameIn] as QuestData.Activation;
        component = activationComponent;
        name = component.sectionName;
        Update();
    }
    
    override public float AddSubComponents(float offset)
    {
        if (game.gameType is MoMGameType)
        {
            return MoMActivation(offset);
        }
        return D2EActivation(offset);
    }

    public float D2EActivation(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset++, 19, 1);
        ui.SetText(new StringKey("val", "X_COLON", ABILITY));

        abilityUIE = new UIElementEditablePaneled(Game.EDITOR, scrollArea.GetScrollTransform());
        abilityUIE.SetLocation(0.5f, offset, 19, 18);
        abilityUIE.SetText(activationComponent.ability.Translate());
        offset += abilityUIE.HeightToTextPadding(1);
        abilityUIE.SetButton(delegate { UpdateAbility(); });
        new UIElementBorder(abilityUIE);
        offset += 1;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 14, 1);
        ui.SetText(new StringKey("val", "X_COLON", MONSTER_MASTER));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(14.5f, offset++, 5, 1);
        ui.SetButton(delegate { ToggleMasterFirst(); });
        new UIElementBorder(ui);
        if (activationComponent.masterFirst)
        {
            ui.SetText(FIRST);
        }
        else
        {
            ui.SetText(NOT_FIRST);
        }

        masterActionsUIE = new UIElementEditablePaneled(Game.EDITOR, scrollArea.GetScrollTransform());
        masterActionsUIE.SetLocation(0.5f, offset, 19, 18);
        masterActionsUIE.SetText(activationComponent.masterActions.Translate(true));
        offset += masterActionsUIE.HeightToTextPadding(1);
        masterActionsUIE.SetButton(delegate { UpdateMasterActions(); });
        new UIElementBorder(masterActionsUIE);
        offset += 1;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 14, 1);
        ui.SetText(new StringKey("val", "X_COLON", MONSTER_MINION));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(14.5f, offset++, 5, 1);
        ui.SetButton(delegate { ToggleMinionFirst(); });
        new UIElementBorder(ui);
        if (activationComponent.minionFirst)
        {
            ui.SetText(FIRST);
        }
        else
        {
            ui.SetText(NOT_FIRST);
        }

        minionActionsUIE = new UIElementEditablePaneled(Game.EDITOR, scrollArea.GetScrollTransform());
        minionActionsUIE.SetLocation(0.5f, offset, 19, 18);
        minionActionsUIE.SetText(activationComponent.minionActions.Translate(true));
        offset += minionActionsUIE.HeightToTextPadding(1);
        minionActionsUIE.SetButton(delegate { UpdateMinionActions(); });
        new UIElementBorder(minionActionsUIE);

        return offset + 1;
    }

    public float MoMActivation(float offset)
    {
        UIElement ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset++, 19, 1);
        ui.SetText(new StringKey("val", "X_COLON", INITIAL_MESSAGE));

        abilityUIE = new UIElementEditablePaneled(Game.EDITOR, scrollArea.GetScrollTransform());
        abilityUIE.SetLocation(0.5f, offset, 19, 18);
        abilityUIE.SetText(activationComponent.ability.Translate(true));
        offset += abilityUIE.HeightToTextPadding(1);
        abilityUIE.SetButton(delegate { UpdateAbility(); });
        new UIElementBorder(abilityUIE);
        offset += 1;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 9.5f, 1);
        ui.SetText(UNABLE_BUTTON);

        moveButtonUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
        moveButtonUIE.SetLocation(9.5f, offset, 10, 1);
        moveButtonUIE.SetText(activationComponent.moveButton.Translate(true));
        moveButtonUIE.SetSingleLine();
        moveButtonUIE.SetButton(delegate { UpdateMoveButton(); });
        new UIElementBorder(moveButtonUIE);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset++, 19, 1);
        ui.SetText(new StringKey("val", "X_COLON", ATTACK_MESSAGE));

        masterActionsUIE = new UIElementEditablePaneled(Game.EDITOR, scrollArea.GetScrollTransform());
        masterActionsUIE.SetLocation(0.5f, offset, 19, 18);
        masterActionsUIE.SetText(activationComponent.masterActions.Translate(true));
        offset += masterActionsUIE.HeightToTextPadding(1);
        masterActionsUIE.SetButton(delegate { UpdateMasterActions(); });
        new UIElementBorder(masterActionsUIE);
        offset += 1;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset++, 19, 1);
        ui.SetText(new StringKey("val", "X_COLON", NO_ATTACK_MESSAGE));

        moveUIE = new UIElementEditablePaneled(Game.EDITOR, scrollArea.GetScrollTransform());
        moveUIE.SetLocation(0.5f, offset, 19, 8);
        moveUIE.SetText(activationComponent.move.Translate(true));
        offset += moveUIE.HeightToTextPadding(1);
        moveUIE.SetButton(delegate { UpdateMove(); });
        new UIElementBorder(moveUIE);
        offset += 1;

        return offset;
    }

    public void UpdateAbility()
    {
        if (!abilityUIE.Empty() && abilityUIE.Changed())
        {
            //insert the text in the current language
            LocalizationRead.updateScenarioText(activationComponent.ability_key, abilityUIE.GetText());
            if (!abilityUIE.HeightAtTextPadding(1))
            {
                Update();
            }
        }
    }

    public void UpdateMoveButton()
    {
        if (moveButtonUIE.Empty())
        {
            LocalizationRead.dicts["qst"].Remove(activationComponent.movebutton_key);
        }
        else if (moveButtonUIE.Changed())
        {
            //insert the text in the current language
            LocalizationRead.updateScenarioText(activationComponent.movebutton_key, moveButtonUIE.GetText());
        }
    }

    public void UpdateMasterActions()
    {
        if (masterActionsUIE.Empty())
        {
            LocalizationRead.dicts["qst"].Remove(activationComponent.master_key);
        }
        else if (masterActionsUIE.Changed())
        {
            LocalizationRead.updateScenarioText(activationComponent.master_key, masterActionsUIE.GetText());
        }
        if (!masterActionsUIE.HeightAtTextPadding(1))
        {
            Update();
        }
    }

    public void UpdateMinionActions()
    {
        if (minionActionsUIE.Empty())
        {
            LocalizationRead.dicts["qst"].Remove(activationComponent.minion_key);
        }
        else if (minionActionsUIE.Changed())
        {
            LocalizationRead.updateScenarioText(activationComponent.minion_key, minionActionsUIE.GetText());
        }
        if (!minionActionsUIE.HeightAtTextPadding(1))
        {
            Update();
        }
    }

    public void UpdateMove()
    {
        if (moveUIE.Empty())
        {
            LocalizationRead.dicts["qst"].Remove(activationComponent.move_key);
        }
        else if (moveUIE.Changed())
        {
            LocalizationRead.updateScenarioText(activationComponent.move_key, moveUIE.GetText());
        }
        if (!moveUIE.HeightAtTextPadding(1))
        {
            Update();
        }
    }

    public void ToggleMasterFirst()
    {
        activationComponent.masterFirst = !activationComponent.masterFirst;
        Update();
    }

    public void ToggleMinionFirst()
    {
        activationComponent.minionFirst = !activationComponent.minionFirst;
        Update();
    }
}
