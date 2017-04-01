using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Window with Investigator attack information
public class InvestigatorAttack
{
    // The monster that raises this dialog
    public Quest.Monster monster;
    public List<AttackData> attacks;
    public HashSet<string> attackType;
    public string attackText = "";

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

        DialogBox db = new DialogBox(new Vector2(UIScaler.GetHCenter(-10f), 0.5f), new Vector2(20, 2), "Select Attack Type");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        float offset = 3f;
        foreach (string type in attackType)
        {
            string tmpType = type;
            // Make first character upper case
            string nameType = System.Char.ToUpper(type[0]) + type.Substring(1);
            new TextButton(new Vector2(UIScaler.GetHCenter(-6f), offset), new Vector2(12, 2), nameType, delegate { Attack(tmpType); });
            offset += 2.5f;
        }

        new TextButton(new Vector2(UIScaler.GetHCenter(-4f), offset), new Vector2(8, 2), "Cancel", delegate { Destroyer.Dialog(); }, Color.red);
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
        attackText = attack.text.Translate().Replace("{0}", monster.monsterData.name.Translate()).Replace("\\n", "\n");
        Attack();
    }

    public void Attack()
    {
        Destroyer.Dialog();
        DialogBox db = new DialogBox(new Vector2(10, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 8), attackText);
        db.AddBorder();


        int health = Mathf.RoundToInt(monster.monsterData.health) + Game.Get().quest.GetHeroCount();
        if (monster.damage == health)
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-6f), 9f), new Vector2(12, 2), "Defeated", delegate { Defeated(); });
        }
        else
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-6f), 9f), new Vector2(12, 2), "Finished", delegate { Destroyer.Dialog(); });
        }

        MonsterDialogMoM.DrawMonster(monster);

        MonsterHealth();
    }

    public void Defeated()
    {
        Destroyer.Dialog();
        Game game = Game.Get();
        // Remove this monster group
        game.quest.monsters.Remove(monster);
        game.monsterCanvas.UpdateList();

        game.quest.vars.SetValue("#monsters", game.quest.monsters.Count);

        // Trigger defeated event
        game.quest.eManager.EventTriggerType("Defeated" + monster.monsterData.sectionName);
        // If unique trigger defeated unique event
        if (monster.unique)
        {
            game.quest.eManager.EventTriggerType("DefeatedUnique" + monster.monsterData.sectionName);
        }
    }

    public void MonsterHealth()
    {

        int health = Mathf.RoundToInt(monster.monsterData.health) + Game.Get().quest.GetHeroCount();
        if (monster.damage == health)
        {
            new TextButton(new Vector2(1f, 10f), new Vector2(2, 2), "-", delegate { MonsterHealthDec(); }, Color.grey);
        }
        else
        {
            new TextButton(new Vector2(1f, 10f), new Vector2(2, 2), "-", delegate { MonsterHealthDec(); }, Color.red);
        }

        DialogBox db = new DialogBox(new Vector2(4f, 10f), new Vector2(2, 2), (health - monster.damage).ToString(), Color.red);
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        if (monster.damage == 0)
        {
            new TextButton(new Vector2(7f, 10f), new Vector2(2, 2), "+", delegate { MonsterHealthInc(); }, Color.grey);
        }
        else
        {
            new TextButton(new Vector2(7f, 10f), new Vector2(2, 2), "+", delegate { MonsterHealthInc(); }, Color.red);
        }
    }

    public void MonsterHealthInc()
    {
        monster.damage -= 1;
        if (monster.damage < 0)
        {
            monster.damage = 0;
        }
        Attack();
    }

    public void MonsterHealthDec()
    {
        monster.damage += 1;
        int health = Mathf.RoundToInt(monster.monsterData.health) + Game.Get().quest.GetHeroCount();
        if (monster.damage > health)
        {
            monster.damage = health;
        }
        Attack();
    }
}