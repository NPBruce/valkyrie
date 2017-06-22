using Assets.Scripts.Content;
using Assets.Scripts.UI;
using UnityEngine;

// Window with Monster activation
public class ActivateDialog {
    // The monster that raises this dialog
    public Quest.Monster monster;
    public bool master;
    private readonly StringKey ACTIONS = new StringKey("val", "ACTIONS");
    private readonly StringKey MONSTER_MASTER = new StringKey("val", "MONSTER_MASTER");
    private readonly StringKey MONSTER_MINION = new StringKey("val", "MONSTER_MINION");
    private readonly StringKey ACTIVATED = new StringKey("val", "ACTIVATED");

    // Create an activation window, if master is false then it is for minions
    public ActivateDialog(Quest.Monster m, bool masterIn, bool singleStep = false)
    {
        monster = m;
        master = masterIn;
        CreateWindow(singleStep);
    }

    virtual public void CreateWindow(bool singleStep = false)
    {
        // If a dialog window is open we force it closed (this shouldn't happen)
        Destroyer.Dialog();

        // ability box - name header
        UIElement ui = new UIElement(Game.ACTIVATION);
        ui.SetLocation(15, 0.5f, UIScaler.GetWidthUnits() - 30, 2);
        ui.SetText(monster.monsterData.name);
        ui.SetButton(delegate { new InfoDialog(monster); });
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui);

        float offset = 2.5f;
        if (monster.currentActivation.effect.Length > 0)
        {
            string effect = monster.currentActivation.effect.Replace("\\n", "\n");
            // ability text
            ui = new UIElement(Game.ACTIVATION);
            ui.SetLocation(10, offset, UIScaler.GetWidthUnits() - 20, 4);
            ui.SetText(effect);
            new UIElementBorder(ui);
            offset += 4.5f;
        }

        // Activation box  header
        ui = new UIElement(Game.ACTIVATION);
        ui.SetLocation(15, offset, UIScaler.GetWidthUnits() - 30, 2);
        if (singleStep)
        {
            ui.SetText(ACTIONS);
            new UIElementBorder(ui);
        }
        else if (master)
        {
            ui.SetText(MONSTER_MASTER, Color.red);
            new UIElementBorder(ui, Color.red);
        }
        else
        {
            ui.SetText(MONSTER_MINION);
            new UIElementBorder(ui);
        }
        ui.SetFontSize(UIScaler.GetMediumFont());
        offset += 2;

        // Activation box
        string activationText = "";
        if (master)
        {
            activationText = monster.currentActivation.masterActions;
        }
        else
        {
            activationText = monster.currentActivation.minionActions;
        }

        // Create activation text box
        ui = new UIElement(Game.ACTIVATION);
        ui.SetLocation(10, offset, UIScaler.GetWidthUnits() - 20, 7);
        ui.SetText(activationText);
        if (master && !singleStep)
        {
            new UIElementBorder(ui, Color.red);
        }
        else
        {
            new UIElementBorder(ui);
        }

        offset += 7.5f;

        // Create finished button
        ui = new UIElement(Game.ACTIVATION);
        ui.SetLocation(15, offset, UIScaler.GetWidthUnits() - 30, 2);
        ui.SetButton(activated);
        if (singleStep)
        {
            ui.SetText(ACTIVATED);
            new UIElementBorder(ui);
        }
        else if (master)
        {
            ui.SetText(new StringKey("val", "X_ACTIVATED", MONSTER_MASTER), Color.red);
            new UIElementBorder(ui, Color.red);
        }
        else
        {
            ui.SetText(new StringKey("val", "X_ACTIVATED", MONSTER_MINION));
            new UIElementBorder(ui);
        }
        ui.SetFontSize(UIScaler.GetMediumFont());
    }

    virtual public void activated()
    {
        // Disable if there is a menu open
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null) return;

        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.ACTIVATION))
            Object.Destroy(go);
        Game.Get().roundControl.MonsterActivated();
    }
}
