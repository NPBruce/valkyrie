using UnityEngine;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

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

        DrawMonster(monster);

        // In horror phase we do horror checks
        if (game.CurrentQuest.phase == Quest.MoMPhase.horror)
        {
            UIElement ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-8f), 2, 16, 2);
            ui.SetText(HORROR_CHECK);
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(Horror);
            new UIElementBorder(ui);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-5f), 4.5f, 10, 2);
            ui.SetText(CommonStringKeys.CANCEL);
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(OnCancel);
            new UIElementBorder(ui);
        }
        else
        { // In investigator phase we do attacks and evades
            DrawMonsterHealth(monster, delegate { CreateWindow(); });

            UIElement ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-8f), 2, 16, 2);
            ui.SetText(new StringKey("val","ACTION_X",ATTACK));
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(Attack);
            new UIElementBorder(ui);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-8f), 4.5f, 16, 2);
            ui.SetText(EVADE);
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(Evade);
            new UIElementBorder(ui);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-5f), 7, 10, 2);
            if (monster.damage == monster.GetHealth())
            {
                ui.SetText(CommonStringKeys.CANCEL, Color.gray);
                new UIElementBorder(ui, Color.gray);
            }
            else
            {
                ui.SetText(CommonStringKeys.CANCEL);
                ui.SetButton(OnCancel);
                new UIElementBorder(ui);
            }
            ui.SetFontSize(UIScaler.GetMediumFont());
        }
    }

    public static void DrawMonster(Quest.Monster monster, bool displayHealth=false)
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
        mImg.tag = Game.DIALOG;
        mImg.transform.SetParent(game.uICanvas.transform);

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
            mImgDupe.tag = Game.DIALOG;
            mImgDupe.transform.SetParent(game.uICanvas.transform);

            RectTransform dupeFrame = mImgDupe.AddComponent<RectTransform>();
            dupeFrame.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 5f * UIScaler.GetPixelsPerUnit(), UIScaler.GetPixelsPerUnit() * 4f);
            dupeFrame.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 5f * UIScaler.GetPixelsPerUnit(), 4f * UIScaler.GetPixelsPerUnit());
            mImgDupe.AddComponent<CanvasRenderer>();

            iconDupe = mImgDupe.AddComponent<UnityEngine.UI.Image>();
            iconDupe.sprite = duplicateSprite;
            iconDupe.rectTransform.sizeDelta = new Vector2(4f * UIScaler.GetPixelsPerUnit(), 4f * UIScaler.GetPixelsPerUnit());
        }

        if (displayHealth)
            DrawMonsterHealth(monster, DrawMonster);
    }

    private static void DrawMonsterHealth(Quest.Monster monster, UnityEngine.Events.UnityAction<Quest.Monster, bool> call)
    {
        UIElement ui = new UIElement();
        ui.SetLocation(0.2f, 0.2f, 2, 2);
        ui.SetText(monster.GetHealth().ToString(), Color.red);
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui, Color.red);

        ui = new UIElement();
        ui.SetLocation(1, 9, 2, 2);
        if (monster.damage == 0)
        {
            ui.SetText(CommonStringKeys.MINUS, Color.grey);
            new UIElementBorder(ui, Color.grey);
        }
        else
        {
            ui.SetText(CommonStringKeys.MINUS, Color.red);
            new UIElementBorder(ui, Color.red);
            ui.SetButton(delegate { MonsterDamageDec(monster, call); });
        }
        ui.SetFontSize(UIScaler.GetMediumFont());

        ui = new UIElement();
        ui.SetLocation(4, 9, 2, 2);
        ui.SetText(monster.damage.ToString(), Color.red);
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui, Color.red);

        ui = new UIElement();
        ui.SetLocation(7, 9, 2, 2);
        if (monster.damage == monster.GetHealth())
        {
            ui.SetText(CommonStringKeys.PLUS, Color.grey);
            new UIElementBorder(ui, Color.grey);

            UIElement defui = new UIElement();
            defui.SetLocation(2, 11.5f, 6, 2);
            defui.SetText(DEFEATED);
            defui.SetFontSize(UIScaler.GetMediumFont());
            defui.SetButton(delegate { Defeated(monster); });
            new UIElementBorder(defui, Color.red);
        }
        else
        {
            ui.SetText(CommonStringKeys.PLUS, Color.red);
            new UIElementBorder(ui, Color.red);
            ui.SetButton(delegate { MonsterDamageInc(monster, call); });
        }
        ui.SetFontSize(UIScaler.GetMediumFont());
    }

    public static void Defeated(Quest.Monster monster)
    {
        Destroyer.Dialog();
        Game game = Game.Get();
        // Remove this monster group
        game.CurrentQuest.monsters.Remove(monster);
        game.monsterCanvas.UpdateList();

        game.CurrentQuest.vars.SetValue("#monsters", game.CurrentQuest.monsters.Count);
        
        game.audioControl.PlayTrait("defeated");

        // end this event (fix #1112)
        Game.Get().CurrentQuest.eManager.currentEvent = null;

        // Trigger defeated event by monster type
        game.CurrentQuest.eManager.EventTriggerType("Defeated" + monster.monsterData.sectionName);
        // Trigger defeated event by spawn name
        game.CurrentQuest.eManager.EventTriggerType("Defeated" + monster.spawnEventName);

        // fix #982 and #1352
        if (game.CurrentQuest.phase == Quest.MoMPhase.monsters && Game.Get().CurrentQuest.eManager.currentEvent == null)
        {
            Game.Get().roundControl.MonsterActivated();
        }
    }

    public static void MonsterDamageDec(Quest.Monster monster, UnityEngine.Events.UnityAction<Quest.Monster, bool> call)
    {
        monster.damage -= 1;
        if (monster.damage < 0)
        {
            monster.damage = 0;
        }
        call(monster, true);
    }

    public static void MonsterDamageInc(Quest.Monster monster, UnityEngine.Events.UnityAction<Quest.Monster, bool> call)
    {
        monster.damage += 1;
        if (monster.damage > monster.GetHealth())
        {
            monster.damage = monster.GetHealth();
        }
        call(monster, true);
    }


    public void Attack()
    {
        Game game = Game.Get();
        // Save to undo stack
        game.CurrentQuest.Save();
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
