using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Window with Investigator evade information
public class InvestigatorEvade {
    public InvestigatorEvade(Quest.Monster m)
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
        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);


        string text = evades[Random.Range(0, evades.Count)].text.Replace("{0}", m.monsterData.name);
        DialogBox db = new DialogBox(new Vector2(10, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 8), text);
        db.AddBorder();

        new TextButton(new Vector2(UIScaler.GetHCenter(-6f), 9f), new Vector2(12, 2), "Finished", delegate { Destroyer.Dialog(); });
    }
}
