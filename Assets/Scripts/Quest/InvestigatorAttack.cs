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
        attackText = attack.text.Translate().Replace("{0}", monster.monsterData.name.Translate());
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

        DrawMonsterIcon();

        MonsterHealth();
    }

    public void Defeated()
    {
        Destroyer.Dialog();
        Game game = Game.Get();
        // Remove this monster group
        game.quest.monsters.Remove(monster);
        game.monsterCanvas.UpdateList();

        // Check if all monsters gone
        if (game.quest.monsters.Count == 0)
        {
            // clear monster flag
            game.quest.flags.Remove("#monsters");
        }

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

    public void DrawMonsterIcon()
    {
        Game game = Game.Get();

        Texture2D newTex = ContentData.FileToTexture(monster.monsterData.image);
        Texture2D dupeTex = Resources.Load("sprites/monster_duplicate_" + monster.duplicate) as Texture2D;
        Sprite iconSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
        Sprite duplicateSprite = null;
        if (dupeTex != null)
        {
            duplicateSprite = Sprite.Create(dupeTex, new Rect(0, 0, dupeTex.width, dupeTex.height), Vector2.zero, 1);
        }

        GameObject mImg = new GameObject("monsterImg" + monster.monsterData.name);
        mImg.tag = "dialog";
        mImg.transform.parent = game.uICanvas.transform;

        RectTransform trans = mImg.AddComponent<RectTransform>();
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 1f * UIScaler.GetPixelsPerUnit(), 8f * UIScaler.GetPixelsPerUnit());
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 1f * UIScaler.GetPixelsPerUnit(), 8f * UIScaler.GetPixelsPerUnit());
        mImg.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Image icon = mImg.AddComponent<UnityEngine.UI.Image>();
        icon.sprite = iconSprite;
        icon.rectTransform.sizeDelta = new Vector2(8f * UIScaler.GetPixelsPerUnit(), 8f * UIScaler.GetPixelsPerUnit());

        UnityEngine.UI.Image iconDupe = null;
        if (duplicateSprite != null)
        {
            GameObject mImgDupe = new GameObject("monsterDupe" + monster.monsterData.name);
            mImgDupe.tag = "dialog";
            mImgDupe.transform.parent = game.uICanvas.transform;

            RectTransform dupeFrame = mImgDupe.AddComponent<RectTransform>();
            dupeFrame.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 5f * UIScaler.GetPixelsPerUnit(), UIScaler.GetPixelsPerUnit() * 4f);
            dupeFrame.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 5f * UIScaler.GetPixelsPerUnit(), 4f * UIScaler.GetPixelsPerUnit());
            mImgDupe.AddComponent<CanvasRenderer>();

            iconDupe = mImgDupe.AddComponent<UnityEngine.UI.Image>();
            iconDupe.sprite = duplicateSprite;
            iconDupe.rectTransform.sizeDelta = new Vector2(4f * UIScaler.GetPixelsPerUnit(), 4f * UIScaler.GetPixelsPerUnit());
        }
    }
}