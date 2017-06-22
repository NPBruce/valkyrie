using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

// Window with Investigator evade information
public class HorrorCheck {
    public HorrorCheck(Quest.Monster m)
    {
        Game game = Game.Get();

        QuestMonster qm = m.monsterData as QuestMonster;
        if (qm != null && game.quest.qd.components.ContainsKey(qm.cMonster.horrorEvent))
        {
            game.quest.eManager.monsterImage = m;
            game.quest.eManager.QueueEvent(qm.cMonster.horrorEvent);
        }
        else
        {
            PickHorror(m);
        }
    }

    public void PickHorror(Quest.Monster m)
    {
        Game game = Game.Get();
        List<HorrorData> horrors = new List<HorrorData>();
        foreach (KeyValuePair<string, HorrorData> kv in game.cd.horrorChecks)
        {
            if (m.monsterData.sectionName.Equals("Monster" + kv.Value.monster))
            {
                horrors.Add(kv.Value);
            }
        }

        QuestMonster qm = m.monsterData as QuestMonster;
        if (horrors.Count == 0 && qm != null && qm.derivedType.Length > 0)
        {
            foreach (KeyValuePair<string, HorrorData> kv in game.cd.horrorChecks)
            {
                if (qm.derivedType.Equals("Monster" + kv.Value.monster))
                {
                    horrors.Add(kv.Value);
                }
            }
        }

        if (horrors.Count != 0) Draw(horrors[Random.Range(0, horrors.Count)], m);
    }

    protected void Draw(HorrorData horror, Quest.Monster m)
    {
        Game game = Game.Get();
        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            Object.Destroy(go);

        string text = horror.text.Translate().Replace("{0}", m.monsterData.name.Translate());
        UIElement ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-14), 0.5f, 28, 8);
        ui.SetText(text);
        new UIElementBorder(ui);

        game.quest.log.Add(new Quest.LogEntry(text.Replace("\n", "\\n")));

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-6f), 9, 12, 2);
        ui.SetText(CommonStringKeys.FINISHED);
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Destroyer.Dialog);
        new UIElementBorder(ui);

        MonsterDialogMoM.DrawMonster(m);
    }
}
