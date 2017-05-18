using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

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
    PaneledDialogBoxEditable abilityDBE;
    DialogBoxEditable moveButtonDBE;
    PaneledDialogBoxEditable masterActionsDBE;
    PaneledDialogBoxEditable minionActionsDBE;
    PaneledDialogBoxEditable moveDBE;

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
        DialogBox db = new DialogBox(new Vector2(0.5f, offset), new Vector2(19, 1), new StringKey("val","X_COLON",ABILITY));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);
        offset += 1;

        abilityDBE = new PaneledDialogBoxEditable(
            new Vector2(0.5f, offset), new Vector2(19, 8), 
            activationComponent.ability.Translate(), 
            delegate { UpdateAbility(); });
        abilityDBE.ApplyTag(Game.EDITOR);
        abilityDBE.background.transform.parent = scrollArea.transform;
        abilityDBE.AddBorder();
        offset += 9;

        db = new DialogBox(new Vector2(0.5f, offset), new Vector2(14, 1), new StringKey("val", "X_COLON", MONSTER_MASTER));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);
        TextButton tb = null;
        if (activationComponent.masterFirst)
        {
            tb = new TextButton(new Vector2(14.5f, offset), new Vector2(5, 1), FIRST, delegate { ToggleMasterFirst(); });
        }
        else
        {
            tb = new TextButton(new Vector2(14.5f, offset), new Vector2(5, 1), NOT_FIRST, delegate { ToggleMasterFirst(); });
        }
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);
        offset += 1;

        masterActionsDBE = new PaneledDialogBoxEditable(
            new Vector2(0.5f, offset), new Vector2(19, 8), 
            activationComponent.masterActions.Translate(true),
            delegate { UpdateMasterActions(); });
        masterActionsDBE.background.transform.parent = scrollArea.transform;
        masterActionsDBE.ApplyTag(Game.EDITOR);
        masterActionsDBE.AddBorder();
        offset += 9;

        db = new DialogBox(new Vector2(0.5f, offset), new Vector2(14, 1), new StringKey("val", "X_COLON", MONSTER_MINION));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);
        if (activationComponent.minionFirst)
        {
            tb = new TextButton(new Vector2(14.5f, offset), new Vector2(5, 1), FIRST, delegate { ToggleMinionFirst(); });
        }
        else
        {
            tb = new TextButton(new Vector2(14.5f, offset), new Vector2(5, 1), NOT_FIRST, delegate { ToggleMinionFirst(); });
        }
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);
        offset += 1;

        minionActionsDBE = new PaneledDialogBoxEditable(
            new Vector2(0.5f, offset), new Vector2(19, 8), 
            activationComponent.minionActions.Translate(true),
            delegate { UpdateMinionActions(); });
        minionActionsDBE.background.transform.parent = scrollArea.transform;
        minionActionsDBE.ApplyTag(Game.EDITOR);
        minionActionsDBE.AddBorder();

        return offset + 9;
    }

    public float MoMActivation(float offset)
    {
        DialogBox db = new DialogBox(new Vector2(0, offset), new Vector2(20, 1), INITIAL_MESSAGE);
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);
        offset += 1;

        abilityDBE = new PaneledDialogBoxEditable(
            new Vector2(0.5f, offset), new Vector2(19, 8), 
            activationComponent.ability.Translate(true),
            delegate { UpdateAbility(); });
        abilityDBE.background.transform.parent = scrollArea.transform;
        abilityDBE.ApplyTag(Game.EDITOR);
        abilityDBE.AddBorder();
        offset += 9;

        db = new DialogBox(new Vector2(0, offset), new Vector2(9.5f, 1), UNABLE_BUTTON);
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        moveButtonDBE = new DialogBoxEditable(
            new Vector2(9.5f, offset), new Vector2(10, 1), 
            activationComponent.moveButton.Translate(true),
            false, delegate { UpdateMoveButton(); });
        moveButtonDBE.background.transform.parent = scrollArea.transform;
        moveButtonDBE.ApplyTag(Game.EDITOR);
        moveButtonDBE.AddBorder();
        offset += 2;

        db = new DialogBox(new Vector2(0.5f, offset), new Vector2(19, 1), ATTACK_MESSAGE);
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);
        offset += 1;

        masterActionsDBE = new PaneledDialogBoxEditable(
            new Vector2(0.5f, offset), new Vector2(19, 8), 
            activationComponent.masterActions.Translate(true),
            delegate { UpdateMasterActions(); });
        masterActionsDBE.background.transform.parent = scrollArea.transform;
        masterActionsDBE.ApplyTag(Game.EDITOR);
        masterActionsDBE.AddBorder();
        offset += 9;

        db = new DialogBox(new Vector2(0, offset++), new Vector2(20, 1), NO_ATTACK_MESSAGE);
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        moveDBE = new PaneledDialogBoxEditable(
            new Vector2(0.5f, offset), new Vector2(19, 8), 
            activationComponent.move.Translate(true),
            delegate { UpdateMove(); });
        moveDBE.background.transform.parent = scrollArea.transform;
        moveDBE.ApplyTag(Game.EDITOR);
        moveDBE.AddBorder();
        offset += 9;

        return offset;
    }

    public void UpdateAbility()
    {
        if (abilityDBE.CheckTextChangedAndNotEmpty())
        {
            //insert the text in the current language
            LocalizationRead.updateScenarioText(activationComponent.ability_key, abilityDBE.Text);
        }
    }

    public void UpdateMoveButton()
    {
        if (moveButtonDBE.CheckTextChangedAndNotEmpty())
        {
            //insert the text in the current language
            LocalizationRead.updateScenarioText(activationComponent.movebutton_key, moveButtonDBE.Text);
        }
        else if (moveButtonDBE.CheckTextEmptied())
        {
            LocalizationRead.scenarioDict.Remove(activationComponent.movebutton_key);
        }
    }

    public void UpdateMasterActions()
    {
        if (masterActionsDBE.CheckTextChangedAndNotEmpty())
        {
            LocalizationRead.updateScenarioText(activationComponent.master_key, masterActionsDBE.Text);
        }
        else if(masterActionsDBE.CheckTextEmptied())
        {
            LocalizationRead.scenarioDict.Remove(activationComponent.master_key);
        }
    }

    public void UpdateMinionActions()
    {
        if (minionActionsDBE.CheckTextChangedAndNotEmpty())
        {
            LocalizationRead.updateScenarioText(activationComponent.minion_key, minionActionsDBE.Text);
        }
        else if (minionActionsDBE.CheckTextEmptied())
        {
            LocalizationRead.scenarioDict.Remove(activationComponent.minion_key);
        }
    }

    public void UpdateMove()
    {
        if (moveDBE.CheckTextChangedAndNotEmpty())
        {
            LocalizationRead.updateScenarioText(activationComponent.move_key, moveDBE.Text);
        }
        else if (moveDBE.CheckTextEmptied())
        {
            LocalizationRead.scenarioDict.Remove(activationComponent.move_key);
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
