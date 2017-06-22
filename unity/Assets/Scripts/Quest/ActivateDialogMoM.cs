using UnityEngine;
using System.Collections;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

// Window with Monster activation
public class ActivateDialogMoM : ActivateDialog
{
    private readonly StringKey MONSTER_ATTACKS = new StringKey("val", "MONSTER_ATTACKS");

    // Create an activation window, if master is false then it is for minions
    public ActivateDialogMoM(Quest.Monster m) : base(m, true)
    {
    }

    override public void CreateWindow(bool singleStep = false)
    {
        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            Object.Destroy(go);

        // ability box - name header
        UIElement ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-9f), 0.5f, 18, 2);
        ui.SetText(monster.monsterData.name);
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui);

        float offset = 2.5f;
        if (monster.currentActivation.effect.Length > 0)
        {
            // ability text
            string textKey = monster.currentActivation.effect.Replace("\\n", "\n");
            // Add this to the log
            Game.Get().quest.log.Add(new Quest.LogEntry(textKey.Replace("\n", "\\n")));
            ui = new UIElement();
            ui.SetLocation(10, offset, UIScaler.GetWidthUnits() - 20, 4);
            ui.SetText(textKey);
            new UIElementBorder(ui);
            offset += 4.5f;
        }

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-9f), offset, 18, 2);
        ui.SetText(MONSTER_ATTACKS);
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(CreateAttackWindow);
        new UIElementBorder(ui);

        offset += 2.5f;

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-9f), offset, 18, 2);
        ui.SetText(monster.currentActivation.ad.moveButton);
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(CreateMoveWindow);
        new UIElementBorder(ui);

        MonsterDialogMoM.DrawMonster(monster);
    }

    public void CreateAttackWindow()
    {
        Destroyer.Dialog();

        // ability box - name header
        UIElement ui = new UIElement();
        ui.SetLocation(15, 0.5f, UIScaler.GetWidthUnits() - 30, 2);
        ui.SetText(monster.monsterData.name);
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui);

        float offset = 2.5f;
        ui = new UIElement();
        ui.SetLocation(10, offset, UIScaler.GetWidthUnits() - 20, 4);
        ui.SetText(monster.currentActivation.masterActions.Replace("\\n", "\n"));
        new UIElementBorder(ui);

        // Add this to the log
        Game.Get().quest.log.Add(new Quest.LogEntry(monster.currentActivation.masterActions.Replace("\n", "\\n")));

        offset += 4.5f;

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-6f), offset, 12, 2);
        ui.SetText(CommonStringKeys.FINISHED);
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(activated);
        new UIElementBorder(ui);

        MonsterDialogMoM.DrawMonster(monster);
    }

    public void CreateMoveWindow()
    {
        if (monster.currentActivation.ad.move.fullKey.Length == 0)
        {
            activated();
            return;
        }

        Destroyer.Dialog();
        UIElement ui = new UIElement();
        ui.SetLocation(15, 0.5f, UIScaler.GetWidthUnits() - 30, 2);
        ui.SetText(monster.monsterData.name);
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui);

        float offset = 2.5f;
        ui = new UIElement();
        ui.SetLocation(10, offset, UIScaler.GetWidthUnits() - 20, 4);
        ui.SetText(monster.currentActivation.move.Replace("\\n", "\n"));
        new UIElementBorder(ui);

        // Add this to the log
        Game.Get().quest.log.Add(new Quest.LogEntry(monster.currentActivation.move.Replace("\n", "\\n")));

        offset += 4.5f;

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-6f), offset, 12, 2);
        ui.SetText(CommonStringKeys.FINISHED);
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(activated);
        new UIElementBorder(ui);

        MonsterDialogMoM.DrawMonster(monster);
    }

    override public void activated()
    {
        Destroyer.Dialog();
        Game.Get().roundControl.MonsterActivated();
    }
}
