using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

// Window with Investigator evade information
public class InvestigatorEvade {
    Quest.Monster m;
    string text;

    public InvestigatorEvade(Quest.Monster monster)
    {
        m = monster;
        Game game = Game.Get();

        QuestMonster qm = m.monsterData as QuestMonster;
        if (qm != null && game.quest.qd.components.ContainsKey(qm.cMonster.evadeEvent))
        {
            game.quest.eManager.monsterImage = m;
            game.quest.eManager.QueueEvent(qm.cMonster.evadeEvent);
        }
        else
        {
            PickEvade(m);
        }
    }

    public void PickEvade(Quest.Monster m)
    {
        Game game = Game.Get();
        List<EvadeData> evades = new List<EvadeData>();
        foreach (KeyValuePair<string, EvadeData> kv in game.cd.investigatorEvades)
        {
            if (m.monsterData.sectionName.Equals("Monster" + kv.Value.monster))
            {
                evades.Add(kv.Value);
            }
        }

        QuestMonster qm = m.monsterData as QuestMonster;
        if (evades.Count == 0 && qm != null && qm.derivedType.Length > 0)
        {
            foreach (KeyValuePair<string, EvadeData> kv in game.cd.investigatorEvades)
            {
                if (qm.derivedType.Equals("Monster" + kv.Value.monster))
                {
                    evades.Add(kv.Value);
                }
            }
        }

        if (evades.Count > 0)
        {
            text = evades[Random.Range(0, evades.Count)].text.Translate().Replace("{0}", m.monsterData.name.Translate());

            game.quest.log.Add(new Quest.LogEntry(text.Replace("\n", "\\n")));

            Draw();
        }
    }

    public void Draw()
    {
        Destroyer.Dialog();
        UIElement ui = new UIElement();
        ui.SetLocation(10, 0.5f, UIScaler.GetWidthUnits() - 20, 8);
        ui.SetText(text);
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-6f), 9, 12, 2);
        if (m.damage == m.GetHealth())
        {
            ui.SetText(CommonStringKeys.FINISHED, Color.grey);
            new UIElementBorder(ui, Color.grey);
        }
        else
        {
            ui.SetText(CommonStringKeys.FINISHED);
            ui.SetButton(Destroyer.Dialog);
            new UIElementBorder(ui);
        }
        ui.SetFontSize(UIScaler.GetMediumFont());

        MonsterDialogMoM.DrawMonster(m);
        MonsterDialogMoM.DrawMonsterHealth(m, delegate { Draw(); });
    }
}
