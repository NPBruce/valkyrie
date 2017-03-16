using UnityEngine;
using System.Collections;

// Window with Monster activation
public class ActivateDialogMoM : ActivateDialog
{
    // Create an activation window, if master is false then it is for minions
    public ActivateDialogMoM(Quest.Monster m) : base(m, true)
    {
    }

    override public void CreateWindow()
    {
        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        // ability box - name header
        DialogBox db = new DialogBox(new Vector2(UIScaler.GetHCenter(-9f), 0.5f), new Vector2(18, 2), monster.monsterData.name);
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        float offset = 2.5f;
        if (monster.currentActivation.effect.Length > 0)
        {
            // ability text
            db = new DialogBox(new Vector2(10, offset), new Vector2(UIScaler.GetWidthUnits() - 20, 4), monster.currentActivation.effect.Replace("\\n", "\n"));
            db.AddBorder();
            offset += 4.5f;
        }

        new TextButton(new Vector2(UIScaler.GetHCenter(-9f), offset), new Vector2(18, 2), "The monster attacks.", delegate { CreateAttackWindow(); });

        offset += 2.5f;

        new TextButton(new Vector2(UIScaler.GetHCenter(-9f), offset), new Vector2(18, 2), monster.currentActivation.ad.moveButton, delegate { CreateMoveWindow(); });

        DrawMonsterIcon();
    }

    public void CreateAttackWindow()
    {
        Destroyer.Dialog();

        // ability box - name header
        DialogBox db = new DialogBox(new Vector2(15, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 30, 2), monster.monsterData.name);
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        float offset = 2.5f;
        db = new DialogBox(new Vector2(10, offset), new Vector2(UIScaler.GetWidthUnits() - 20, 4), monster.currentActivation.ad.masterActions.Replace("\\n", "\n"));
        db.AddBorder();

        offset += 4.5f;

        new TextButton(new Vector2(UIScaler.GetHCenter(-6f), offset), new Vector2(12, 2), "Finished", delegate { activated(); });

        DrawMonsterIcon();
    }

    public void CreateMoveWindow()
    {
        if (monster.currentActivation.ad.move.Length == 0)
        {
            activated();
            return;
        }

        Destroyer.Dialog();
        DialogBox db = new DialogBox(new Vector2(15, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 30, 2), monster.monsterData.name);
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        float offset = 2.5f;
        db = new DialogBox(new Vector2(10, offset), new Vector2(UIScaler.GetWidthUnits() - 20, 4), monster.currentActivation.move.Replace("\\n", "\n"));
        db.AddBorder();

        offset += 4.5f;

        new TextButton(new Vector2(UIScaler.GetHCenter(-6f), offset), new Vector2(12, 2), "Finished", delegate { activated(); });

        DrawMonsterIcon();
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
