using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterCanvas : MonoBehaviour
{

    public float offset = 0;
    public Dictionary<string, UnityEngine.UI.Image> icons;
    public static float monsterSize = 4;

    // Call to update list of monsters
    public void UpdateList()
    {
        // Clean up everything marked as 'monsters'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("monsters"))
            Object.Destroy(go);

        // New list
        offset = 0;
        icons = new Dictionary<string, UnityEngine.UI.Image>();

        Game game = Game.Get();
        foreach (Round.Monster m in game.round.monsters)
            AddMonster(m, game);

        UpdateStatus();
    }

    void AddMonster(Round.Monster m, Game game)
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
        foreach (Round.Monster m in game.round.monsters)
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
        foreach (Round.Monster m in game.round.monsters)
        {
            if (name.Equals(m.monsterData.name))
                new MonsterDialog(m);
        }
    }
}
