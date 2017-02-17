using UnityEngine;
using System.Collections;

// Window with Investigator evade information
public class HorrorCheck {
    public HorrorCheck(Quest.Monster m)
    {
        monster = m;
        Game game = Game.Get();
        horrors = new List<HorrorData>();
        foreach (HorrorData hd in game.cd.horrorChecks)
        {
            if (m.monsterData.sectionName.equals(hd.monster))
            {
                horrors.Add(ed);
            }
        }
        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        AttackData attack = horrors[Random.Range(0, horrors.Count)];
        string text = horrors[Random.Range(0, horrors.Count)].text.Replace("{0}", m.monsterData.name);
        DialogBox db = new DialogBox(new Vector2(10, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 8), text);
        db.AddBorder();

        new TextButton(new Vector2(GetVCenter(-6f), 9f), new Vector2(UIScaler.GetWidthUnits() - 12, 2), "Finished", delegate { Destroyer.Dialog(); });
    }
}
