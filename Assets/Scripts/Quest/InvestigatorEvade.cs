using UnityEngine;
using System.Collections;

// Window with Investigator evade information
public class InvestigatorEvade {
    public InvestigatorAttack(Quest.Monster m)
    {
        monster = m;
        Game game = Game.Get();
        evades = new List<EvadeData>();
        foreach (EvadeData ed in game.cd.investigatorEvades)
        {
            if (m.monsterData.sectionName.equals(ed.monster))
            {
                evades.Add(ed);
            }
        }
        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);


        string text = evades[Random.Range(0, evades.Count)].text.Replace("{0}", m.monsterData.name);
        DialogBox db = new DialogBox(new Vector2(10, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 8), text);
        db.AddBorder();

        new TextButton(new Vector2(GetVCenter(-6f), 9f), new Vector2(UIScaler.GetWidthUnits() - 12, 2), "Finished", delegate { Destroyer.Dialog(); });
    }
}
