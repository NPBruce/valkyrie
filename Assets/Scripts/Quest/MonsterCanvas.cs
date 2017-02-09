using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterCanvas : MonoBehaviour
{

    public float offset = 0;
    public Dictionary<string, MonsterIcon> icons;
    public Dictionary<string, UnityEngine.UI.Image> iconFrames;
    public static float monsterSize = 4;

    // Call to update list of monsters
    public void UpdateList()
    {
        // Clean up everything marked as 'monsters'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("monsters"))
            Object.Destroy(go);

        // New list
        offset = 0;
        icons = new Dictionary<string, MonsterIcon>();

        Game game = Game.Get();
        foreach (Quest.Monster m in game.quest.monsters)
            AddMonster(m, game);

        UpdateStatus();
    }

    void AddMonster(Quest.Monster m, Game game)
    {
        Sprite mSprite;

        Texture2D newTex = ContentData.FileToTexture(m.monsterData.image);

        GameObject mImg = new GameObject("monsterImg" + m.monsterData.name);
        mImg.tag = "monsters";

        mImg.transform.parent = game.uICanvas.transform;

        RectTransform trans = mImg.AddComponent<RectTransform>();
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (0.25f + offset) * UIScaler.GetPixelsPerUnit(), monsterSize * UIScaler.GetPixelsPerUnit());
        offset += monsterSize + 0.5f;
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0.25f * UIScaler.GetPixelsPerUnit(), monsterSize * UIScaler.GetPixelsPerUnit());
        mImg.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Image image = mImg.AddComponent<UnityEngine.UI.Image>();
        icons.Add(m.monsterData.name, image);
        mSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
        image.sprite = mSprite;
        image.rectTransform.sizeDelta = new Vector2(monsterSize * UIScaler.GetPixelsPerUnit(), monsterSize * UIScaler.GetPixelsPerUnit());

        UnityEngine.UI.Button button = mImg.AddComponent<UnityEngine.UI.Button>();
        button.interactable = true;
        button.onClick.AddListener(delegate { MonsterDiag(m.monsterData.name); });
    }

    public void UpdateStatus()
    {
        Game game = Game.Get();
        foreach (Quest.Monster m in game.quest.monsters)
        {
            UnityEngine.UI.Image image = icons[m.monsterData.name];
            if (m.activated && m.unique)
            {
                image.color = new Color(0f, 0.3f, 0f, 1);
            }
            else if (m.activated)
            {
                image.color = new Color((float)0.2, (float)0.2, (float)0.2, 1);
            }
            else if (m.unique)
            {
                image.color = new Color(0.6f, 1f, 0.6f, 1);
            }
            else
            {
                image.color = Color.white;
            }
        }
    }

    void MonsterDiag(string name)
    {
        // If there are any other dialogs open just finish
        if (GameObject.FindGameObjectWithTag("dialog") != null)
            return;

        Game game = Game.Get();
        foreach (Quest.Monster m in game.quest.monsters)
        {
            if (name.Equals(m.monsterData.name))
            {
                if (game.gameType.DisplayHeroes())
                {
                    new MonsterDialog(m);
                }
                else
                {
                    new MonsterDialogMoM(m);
                }
            }
        }
    }

    public class MonsterIcon
    {
        Quest.Monster m;

        Game game;

        UnityEngine.UI.Image icon;
        UnityEngine.UI.Image frame;

        Sprite iconSprite;
        Sprite frameSprite;

        int index;

        public MonsterIcon(Quest.Monster monster, int i = 0)
        {
            m = monster;
            index = i;

            game = Game.Get();

            Texture2D newTex = ContentData.FileToTexture(m.monsterData.image);
            Texture2D frameTex = Resources.Load("sprites/borders/Frame_Monster_1x1") as Texture2D;
            iconSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            frameSprite = Sprite.Create(frameTex, new Rect(0, 0, frameTex.width, frameTex.height), Vector2.zero, 1);
        }

        public void Draw(int offset)
        {
            if (index < offset)
            {
                return;
            }
            if (index > offset + 6)
            {
                return;
            }

            GameObject mImg = new GameObject("monsterImg" + m.monsterData.name);
            mImg.tag = "monsters";

            mImg.transform.parent = game.uICanvas.transform;

            RectTransform trans = mImg.AddComponent<RectTransform>();
            trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (0.25f + (offset * 4.5f)) * UIScaler.GetPixelsPerUnit(), monsterSize * UIScaler.GetPixelsPerUnit());
            trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0.25f * UIScaler.GetPixelsPerUnit(), monsterSize * UIScaler.GetPixelsPerUnit());
            mImg.AddComponent<CanvasRenderer>();

            icon = mImg.AddComponent<UnityEngine.UI.Image>();
            icon.sprite = iconSprite;
            icon.rectTransform.sizeDelta = new Vector2(monsterSize * UIScaler.GetPixelsPerUnit(), monsterSize * UIScaler.GetPixelsPerUnit());

            UnityEngine.UI.Button button = mImg.AddComponent<UnityEngine.UI.Button>();
            button.interactable = true;
            button.onClick.AddListener(delegate { MonsterDiag(m.monsterData.name); });
        }

        void MonsterDiag(string name)
        {
            // If there are any other dialogs open just finish
            if (GameObject.FindGameObjectWithTag("dialog") != null)
                return;

            Game game = Game.Get();
            foreach (Quest.Monster m in game.quest.monsters)
            {
                if (name.Equals(m.monsterData.name))
                {
                    if (game.gameType.DisplayHeroes())
                    {
                        new MonsterDialog(m);
                    }
                    else
                    {
                        new MonsterDialogMoM(m);
                    }
                }
            }
        }
    }
}
