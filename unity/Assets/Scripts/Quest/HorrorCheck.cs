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
        if (qm != null && game.CurrentQuest.qd.components.ContainsKey(qm.cMonster.horrorEvent))
        {
            game.CurrentQuest.eManager.monsterImage = m;
            game.CurrentQuest.eManager.QueueEvent(qm.cMonster.horrorEvent);
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
        foreach (HorrorData hd in game.cd.Values<HorrorData>())
        {
            if (m.monsterData.sectionName.Equals(MonsterData.type + hd.monster))
            {
                horrors.Add(hd);
            }
        }

        QuestMonster qm = m.monsterData as QuestMonster;
        if (horrors.Count == 0 && qm != null && !string.IsNullOrWhiteSpace(qm.derivedType))
        {
            foreach (HorrorData horrorData in game.cd.Values<HorrorData>())
            {
                if (qm.derivedType.Equals("Monster" + horrorData.monster))
                {
                    horrors.Add(horrorData);
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

        game.CurrentQuest.log.Add(new Quest.LogEntry(text.Replace("\n", "\\n")));

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-6f), 9, 12, 2);
        ui.SetText(CommonStringKeys.FINISHED);
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Destroyer.Dialog);
        new UIElementBorder(ui);

        MonsterDialogMoM.DrawMonster(m, true);
    }
}
