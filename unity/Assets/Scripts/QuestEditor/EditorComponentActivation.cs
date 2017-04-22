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
    DialogBoxEditable abilityDBE;
    DialogBoxEditable moveButtonDBE;
    DialogBoxEditable masterActionsDBE;
    DialogBoxEditable minionActionsDBE;
    DialogBoxEditable moveDBE;

    public EditorComponentActivation(string nameIn) : base()
    {
        Game game = Game.Get();
        activationComponent = game.quest.qd.components[nameIn] as QuestData.Activation;
        component = activationComponent;
        name = component.sectionName;
        Update();
    }
    
    override public void Update()
    {
        base.Update();
        Game game = Game.Get();

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), 
            CommonStringKeys.ACTIVATION, delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1), 
            new StringKey(null, name.Substring("Activation".Length),false), 
            delegate { QuestEditorData.ListActivation(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(
            new Vector2(19, 0), new Vector2(1, 1), CommonStringKeys.E, delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        if (game.gameType is MoMGameType)
        {
            MoMActivation();
        }
        else
        {
            Activation();
        }
    }

    public void Activation()
    {
        DialogBox db = new DialogBox(new Vector2(0, 1), new Vector2(20, 1), new StringKey("val","X_COLON",ABILITY));
        db.ApplyTag("editor");

        abilityDBE = new DialogBoxEditable(
            new Vector2(0, 2), new Vector2(20, 8), 
            activationComponent.ability.Translate(),
            delegate { UpdateAbility(); });
        abilityDBE.ApplyTag("editor");
        abilityDBE.AddBorder();

        db = new DialogBox(new Vector2(0, 10), new Vector2(15, 1), new StringKey("val", "X_COLON", MONSTER_MASTER));
        db.ApplyTag("editor");
        TextButton tb = null;
        if (activationComponent.masterFirst)
        {
            tb = new TextButton(new Vector2(15, 10), new Vector2(5, 1), FIRST, delegate { ToggleMasterFirst(); });
        }
        else
        {
            tb = new TextButton(new Vector2(15, 10), new Vector2(5, 1), NOT_FIRST, delegate { ToggleMasterFirst(); });
        }
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        masterActionsDBE = new DialogBoxEditable(
            new Vector2(0, 11), new Vector2(20, 8), 
            activationComponent.masterActions.Translate(true),
            delegate { UpdateMasterActions(); });
        masterActionsDBE.ApplyTag("editor");
        masterActionsDBE.AddBorder();

        db = new DialogBox(new Vector2(0, 19), new Vector2(15, 1), new StringKey("val", "X_COLON", MONSTER_MINION));
        db.ApplyTag("editor");
        if (activationComponent.minionFirst)
        {
            tb = new TextButton(new Vector2(15, 19), new Vector2(5, 1), FIRST, delegate { ToggleMinionFirst(); });
        }
        else
        {
            tb = new TextButton(new Vector2(15, 19), new Vector2(5, 1), NOT_FIRST, delegate { ToggleMinionFirst(); });
        }
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        minionActionsDBE = new DialogBoxEditable(
            new Vector2(0, 20), new Vector2(20, 8), 
            activationComponent.minionActions.Translate(true),
            delegate { UpdateMinionActions(); });
        minionActionsDBE.ApplyTag("editor");
        minionActionsDBE.AddBorder();
    }

    public void MoMActivation()
    {
        DialogBox db = new DialogBox(new Vector2(0, 1), new Vector2(20, 1), INITIAL_MESSAGE);
        db.ApplyTag("editor");

        abilityDBE = new DialogBoxEditable(
            new Vector2(0, 2), new Vector2(20, 8), 
            activationComponent.ability.Translate(true),
            delegate { UpdateAbility(); });
        abilityDBE.ApplyTag("editor");
        abilityDBE.AddBorder();

        db = new DialogBox(new Vector2(0, 10), new Vector2(10, 1), UNABLE_BUTTON);

        moveButtonDBE = new DialogBoxEditable(
            new Vector2(10, 10), new Vector2(10, 1), 
            activationComponent.moveButton.Translate(true),
            delegate { UpdateMoveButton(); });
        moveButtonDBE.ApplyTag("editor");
        moveButtonDBE.AddBorder();

        db = new DialogBox(new Vector2(0, 11), new Vector2(20, 1), ATTACK_MESSAGE);
        db.ApplyTag("editor");

        masterActionsDBE = new DialogBoxEditable(
            new Vector2(0, 12), new Vector2(20, 8), 
            activationComponent.masterActions.Translate(true),
            delegate { UpdateMasterActions(); });
        masterActionsDBE.ApplyTag("editor");
        masterActionsDBE.AddBorder();

        db = new DialogBox(new Vector2(0, 20), new Vector2(20, 1), NO_ATTACK_MESSAGE);
        db.ApplyTag("editor");

        moveDBE = new DialogBoxEditable(
            new Vector2(0, 21), new Vector2(20, 8), 
            activationComponent.move.Translate(true),
            delegate { UpdateMove(); });
        moveDBE.ApplyTag("editor");
        moveDBE.AddBorder();
    }

    public void UpdateAbility()
    {
        if (!abilityDBE.Text.Equals(""))
        {
            //insert the text in the current language
            LocalizationRead.updateScenarioText(activationComponent.ability_key, abilityDBE.Text);
        }
    }

    public void UpdateMoveButton()
    {
        if (!moveButtonDBE.Text.Equals(""))
        {
            //insert the text in the current language
            LocalizationRead.updateScenarioText(activationComponent.movebutton_key, moveButtonDBE.Text);
        }
        else
        {
            LocalizationRead.scenarioDict.Remove(activationComponent.movebutton_key);
        }
    }

    public void UpdateMasterActions()
    {
        if (!masterActionsDBE.Text.Equals(""))
        {
            LocalizationRead.updateScenarioText(activationComponent.master_key, masterActionsDBE.Text);
        }
        else
        {
            LocalizationRead.scenarioDict.Remove(activationComponent.master_key);
        }
    }

    public void UpdateMinionActions()
    {
        if (!minionActionsDBE.Text.Equals(""))
        {
            LocalizationRead.updateScenarioText(activationComponent.minion_key, minionActionsDBE.Text);
        }
        else
        {
            LocalizationRead.scenarioDict.Remove(activationComponent.minion_key);
        }
    }

    public void UpdateMove()
    {
        if (!moveDBE.Text.Equals(""))
        {
            LocalizationRead.updateScenarioText(activationComponent.move_key, moveDBE.Text);
        }
        else
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
