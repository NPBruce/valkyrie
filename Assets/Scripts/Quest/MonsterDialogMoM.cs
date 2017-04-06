using UnityEngine;
using System.Collections;
using Assets.Scripts.Content;

// Class for creation of monster seleciton options
// Extends the standard class for MoM
public class MonsterDialogMoM : MonsterDialog
{
    private static readonly StringKey DEFEATED = new StringKey("val", "DEFEATED");
    private readonly StringKey EVADE = new StringKey("val", "EVADE");
    private readonly StringKey ATTACK = new StringKey("val", "ATTACK");
    private readonly StringKey HORROR_CHECK = new StringKey("val", "HORROR_CHECK");

    public MonsterDialogMoM(Quest.Monster m) : base(m)
    {
    }

    public override void CreateWindow()
    {
        Destroyer.Dialog();
        Game game = Game.Get();
        // Get the offset position of the monster
        int index = 0;
        for (int i = 0; i < game.quest.monsters.Count; i++)
        {
            if (game.quest.monsters[i] == monster)
            {
                index = i;
            }
        }

        DrawMonster(monster);

        // In horror phase we do horror checks
        if (game.quest.phase == Quest.MoMPhase.horror)
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-8f), 2), new Vector2(16, 2), HORROR_CHECK, delegate { Horror(); });
            new TextButton(new Vector2(UIScaler.GetHCenter(-5f), 4.5f), new Vector2(10, 2), CommonStringKeys.CANCEL, delegate { OnCancel(); });
        }
        else
        { // In investigator phase we do attacks and evades
            DrawMonsterHealth(monster, delegate { CreateWindow(); });
            new TextButton(new Vector2(UIScaler.GetHCenter(-8f), 2), new Vector2(16, 2),
                new StringKey("val","ACTION_X",ATTACK), delegate { Attack(); });
            new TextButton(new Vector2(UIScaler.GetHCenter(-8f), 4.5f), new Vector2(16, 2), EVADE, delegate { Evade(); });
            int health = Mathf.RoundToInt(monster.monsterData.health) + Game.Get().quest.GetHeroCount();
            if (monster.damage == health)
            {
                new TextButton(new Vector2(UIScaler.GetHCenter(-5f), 7f), new Vector2(10, 2), CommonStringKeys.CANCEL, delegate { ; }, Color.gray);
            }
            else
            {
                new TextButton(new Vector2(UIScaler.GetHCenter(-5f), 7f), new Vector2(10, 2), CommonStringKeys.CANCEL, delegate { OnCancel(); });
            }
        }
    }

    public static void DrawMonster(Quest.Monster monster)
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

    public static void DrawMonsterHealth(Quest.Monster monster, UnityEngine.Events.UnityAction call)
    {
        int health = Mathf.RoundToInt(monster.monsterData.health) + Game.Get().quest.GetHeroCount();

        DialogBox db = new DialogBox(new Vector2(0.2f, 0.2f), new Vector2(2, 2),
            new StringKey((health).ToString(),false), Color.red);
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        if (monster.damage == 0)
        {
            new TextButton(new Vector2(1f, 9), new Vector2(2, 2), CommonStringKeys.MINUS, delegate { MonsterDamageDec(monster, call); }, Color.grey);
        }
        else
        {
            new TextButton(new Vector2(1f, 9), new Vector2(2, 2), CommonStringKeys.MINUS, delegate { MonsterDamageDec(monster, call); }, Color.red);
        }

        db = new DialogBox(new Vector2(4f, 9), new Vector2(2, 2),
            new StringKey((monster.damage).ToString(),false), Color.red);
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        if (monster.damage == health)
        {
            new TextButton(new Vector2(7f, 9), new Vector2(2, 2), CommonStringKeys.PLUS, delegate { MonsterDamageInc(monster, call); }, Color.grey);
            new TextButton(new Vector2(2, 11.5f), new Vector2(6, 2), DEFEATED, delegate { Defeated(monster); }, Color.red);
        }
        else
        {
            new TextButton(new Vector2(7f, 9), new Vector2(2, 2), CommonStringKeys.PLUS, delegate { MonsterDamageInc(monster, call); }, Color.red);
        }
    }

    public static void Defeated(Quest.Monster monster)
    {
        Destroyer.Dialog();
        Game game = Game.Get();
        // Remove this monster group
        game.quest.monsters.Remove(monster);
        game.monsterCanvas.UpdateList();

        game.quest.vars.SetValue("#monsters", game.quest.monsters.Count);
        
        game.audioControl.PlayTrait("defeated");

        // Trigger defeated event
        game.quest.eManager.EventTriggerType("Defeated" + monster.monsterData.sectionName);
        // If unique trigger defeated unique event
        if (monster.unique)
        {
            game.quest.eManager.EventTriggerType("DefeatedUnique" + monster.monsterData.sectionName);
        }
    }

    public static void MonsterDamageDec(Quest.Monster monster, UnityEngine.Events.UnityAction call)
    {
        monster.damage -= 1;
        if (monster.damage < 0)
        {
            monster.damage = 0;
        }
        call();
    }

    public static void MonsterDamageInc(Quest.Monster monster, UnityEngine.Events.UnityAction call)
    {
        monster.damage += 1;
        int health = Mathf.RoundToInt(monster.monsterData.health) + Game.Get().quest.GetHeroCount();
        if (monster.damage > health)
        {
            monster.damage = health;
        }
        call();
    }


    public void Attack()
    {
        Game game = Game.Get();
        // Save to undo stack
        game.quest.Save();
        new InvestigatorAttack(monster);
    }

    public void Evade()
    {
        new InvestigatorEvade(monster);
    }

    public void Horror()
    {
        new HorrorCheck(monster);
    }
}
