using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

// Window with Investigator attack information
public class InvestigatorAttack
{
    private readonly StringKey ATTACK_PROMPT = new StringKey("val", "ATTACK_PROMPT");

    // The monster that raises this dialog
    public Quest.Monster monster;
    public string attackText = "";

    public InvestigatorAttack(Quest.Monster m)
    {
        monster = m;
        Game game = Game.Get();
        AttackOptions();
    }

    public void AttackOptions()
    {
        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            Object.Destroy(go);

        UIElement ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-10), 0.5f, 20, 2);
        ui.SetText(ATTACK_PROMPT);
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui);

        float offset = 3f;
        foreach (string type in monster.monsterData.GetAttackTypes())
        {
            string tmpType = type;
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-6f), offset, 12, 2);
            ui.SetText(new StringKey("val", tmpType));
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(delegate { Attack(tmpType); });
            new UIElementBorder(ui);
            offset += 2.5f;
        }

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-4f), offset, 8, 2);
        if (monster.damage == monster.GetHealth())
        {
            ui.SetText(CommonStringKeys.CANCEL, Color.gray);
            new UIElementBorder(ui, Color.gray);
        }
        else
        {
            ui.SetText(CommonStringKeys.CANCEL);
            ui.SetButton(Destroyer.Dialog);
            new UIElementBorder(ui);
        }
        ui.SetFontSize(UIScaler.GetMediumFont());

        MonsterDialogMoM.DrawMonster(monster);
        MonsterDialogMoM.DrawMonsterHealth(monster, delegate { AttackOptions(); });
    }

    public void Attack(string type)
    {
        StringKey text = monster.monsterData.GetRandomAttack(type);
        attackText = text.Translate().Replace("{0}", monster.monsterData.name.Translate());
        Game.Get().quest.log.Add(new Quest.LogEntry(attackText.Replace("\n", "\\n")));
        Attack();
    }

    public void Attack()
    {
        Destroyer.Dialog();

        UIElement ui = new UIElement();
        ui.SetLocation(10, 0.5f, UIScaler.GetWidthUnits() - 20, 8);
        ui.SetText(attackText);
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-6), 9, 12, 2);
        if (monster.damage == monster.GetHealth())
        {
            ui.SetText(CommonStringKeys.FINISHED, Color.gray);
            new UIElementBorder(ui, Color.gray);
        }
        else
        {
            ui.SetText(CommonStringKeys.FINISHED);
            ui.SetButton(Destroyer.Dialog);
            new UIElementBorder(ui);
        }
        ui.SetFontSize(UIScaler.GetMediumFont());

        MonsterDialogMoM.DrawMonster(monster);
        MonsterDialogMoM.DrawMonsterHealth(monster, delegate { Attack(); });
    }
}