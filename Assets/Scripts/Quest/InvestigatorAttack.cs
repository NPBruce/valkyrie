using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

// Window with Investigator attack information
public class InvestigatorAttack
{
    private readonly StringKey ATTACK_PROMPT = new StringKey("val", "ATTACK_PROMPT");

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

        DialogBox db = new DialogBox(new Vector2(UIScaler.GetHCenter(-10f), 0.5f), new Vector2(20, 2), ATTACK_PROMPT);
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        float offset = 3f;
        foreach (string type in attackType)
        {
            string tmpType = type;
            // Make first character upper case
            string nameType = System.Char.ToUpper(type[0]) + type.Substring(1);
            new TextButton(new Vector2(UIScaler.GetHCenter(-6f), offset), new Vector2(12, 2), 
                new StringKey(nameType,false), delegate { Attack(tmpType); });
            offset += 2.5f;
        }

        int health = Mathf.RoundToInt(monster.monsterData.health) + Game.Get().quest.GetHeroCount();
        if (monster.damage == health)
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-4f), offset), new Vector2(8, 2), CommonStringKeys.CANCEL, delegate { ; }, Color.gray);
        }
        else
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-4f), offset), new Vector2(8, 2), CommonStringKeys.CANCEL, delegate { Destroyer.Dialog(); });
        }

        MonsterDialogMoM.DrawMonster(monster);
        MonsterDialogMoM.DrawMonsterHealth(monster, delegate { AttackOptions(); });
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
        attackText = attack.text.Translate().Replace("{0}", monster.monsterData.name.Translate());
        Attack();
    }

    public void Attack()
    {
        Destroyer.Dialog();
        DialogBox db = new DialogBox(new Vector2(10, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 8), 
            new StringKey(attackText,false));
        db.AddBorder();


        int health = Mathf.RoundToInt(monster.monsterData.health) + Game.Get().quest.GetHeroCount();
        if (monster.damage == health)
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-6f), 9f), new Vector2(12, 2), CommonStringKeys.FINISHED, delegate { ; }, Color.grey);
        }
        else
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-6f), 9f), new Vector2(12, 2), CommonStringKeys.FINISHED, delegate { Destroyer.Dialog(); });
        }

        MonsterDialogMoM.DrawMonster(monster);
        MonsterDialogMoM.DrawMonsterHealth(monster, delegate { Attack(); });
    }
}