using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Window with Investigator attack information
public class InvestigatorAttack {
    // The monster that raises this dialog
    public Quest.Monster monster;
    public List<AttackData> attacks;
    public HashSet<string> attackType;

    public InvestigatorAttack(Quest.Monster m)
    {
        monster = m;
        Game game = Game.Get();
        attacks = new List<AttackData>();
        attackType = new HashSet<string>();
        foreach (KeyValuePair<string, AttackData> kv in game.cd.investigatorAttacks)
        {
            if (m.monsterData.ContainsTrait(kv.Value.target))
            {
                attacks.Add(kv.Value);
                attackType.Add(kv.Value.attackType);
            }
        }
        AttackOptions();
    }

    public void AttackOptions()
    {
        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        DialogBox db = new DialogBox(new Vector2(UIScaler.GetVCenter(-15f), 0.5f), new Vector2(UIScaler.GetWidthUnits() - 30, 2), "Select Attack Type");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        float offset = 2.5f;
        foreach (string type in attackType)
        {
            string tmpType = type;
            // Make first character upper case
            string nameType = System.Char.ToUpper(type[0]) + type.Substring(1);
            new TextButton(new Vector2(UIScaler.GetVCenter(-8f), offset), new Vector2(UIScaler.GetWidthUnits() - 16, 2), nameType, delegate { Attack(tmpType); });
            offset += 2.5f;
        }

        new TextButton(new Vector2(UIScaler.GetVCenter(-6f), offset), new Vector2(UIScaler.GetWidthUnits() - 12, 2), "Cancel", delegate { Destroyer.Dialog(); });
    }

    public void Attack(string type)
    {
        List<AttackData> validAttacks = new List<AttackData>();
        foreach (AttackData ad in attacks)
        {
            if (ad.attackType.Equals(type))
            {
                validAttacks.Add(ad);
            }
        }
        AttackData attack = validAttacks[Random.Range(0, validAttacks.Count)];

        string text = attack.text.Replace("{0}", monster.monsterData.name);
        DialogBox db = new DialogBox(new Vector2(10, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 8), text);
        db.AddBorder();

        new TextButton(new Vector2(UIScaler.GetVCenter(-6f), 9f), new Vector2(UIScaler.GetWidthUnits() - 12, 2), "Finished", delegate { Destroyer.Dialog(); });
    }
}
