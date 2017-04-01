using UnityEngine;
using System.Collections;

// Class for creation of monster seleciton options
// Extends the standard class for MoM
public class MonsterDialogMoM : MonsterDialog
{
    public MonsterDialogMoM(Quest.Monster m) : base(m)
    {
    }

    public override void CreateWindow()
    {
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

        float offset = (index + 0.1f - game.monsterCanvas.offset) * (MonsterCanvas.monsterSize + 0.5f);

        // In horror phase we do horror checks
        if (game.quest.phase == Quest.MoMPhase.horror)
        {
            new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset), new Vector2(10, 2), "Horror Check", delegate { Horror(); });
            new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset + 2.5f), new Vector2(10, 2), "Cancel", delegate { OnCancel(); });
        }
        else
        { // In investigator phase we do attacks and evades
            new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset), new Vector2(10, 2), " Attack", delegate { Attack(); });
            new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset + 2.5f), new Vector2(10, 2), "Evade", delegate { Evade(); });
            new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset + 5f), new Vector2(10, 2), "Cancel", delegate { OnCancel(); });
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
