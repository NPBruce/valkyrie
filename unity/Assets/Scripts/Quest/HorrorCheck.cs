using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

// Window with Investigator evade information
public class HorrorCheck {
    public HorrorCheck(Quest.Monster m)
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

        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        string text = horrors[Random.Range(0, horrors.Count)].text.Translate().Replace("{0}", m.monsterData.name.Translate());
        DialogBox db = new DialogBox(new Vector2(10, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 8), 
            new StringKey(null, text,false));
        db.AddBorder();

        new TextButton(new Vector2(UIScaler.GetHCenter(-6f), 9f), new Vector2(12, 2), CommonStringKeys.FINISHED, delegate { Destroyer.Dialog(); });

        MonsterDialogMoM.DrawMonster(m);
    }
}
