using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterCanvas : MonoBehaviour
{

    public int offset = 0;
    public Dictionary<string, UnityEngine.UI.Image> icons;

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
        foreach (Game.Monster m in game.monsters)
            AddMonster(m, game);

        UpdateStatus();
    }

    void AddMonster(Game.Monster m, Game game)
    {
        string imagePath = @"file://" + m.monsterData.image;

        Sprite mSprite;

        WWW www = new WWW(imagePath);
        Texture2D newTex = new Texture2D(256, 256, TextureFormat.DXT5, false);
        www.LoadImageIntoTexture(newTex);

        GameObject mImg = new GameObject("monsterImg" + m.monsterData.name);
        mImg.tag = "monsters";

        mImg.transform.parent = game.uICanvas.transform;

        RectTransform trans = mImg.AddComponent<RectTransform>();
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 30 + offset, 50);
        offset += 100;
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 30, 50);
        mImg.AddComponent<CanvasRenderer>();


        UnityEngine.UI.Image image = mImg.AddComponent<UnityEngine.UI.Image>();
        icons.Add(m.monsterData.name, image);
        mSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
        image.sprite = mSprite;
        image.rectTransform.sizeDelta = new Vector2(80, 80);

        UnityEngine.UI.Button button = mImg.AddComponent<UnityEngine.UI.Button>();
        button.interactable = true;
        button.onClick.AddListener(delegate { MonsterDiag(m.monsterData.name); });
    }

    public void UpdateStatus()
    {
        Game game = Game.Get();
        foreach (Game.Monster m in game.monsters)
        {
            UnityEngine.UI.Image image = icons[m.monsterData.name];
            if (m.activated)
            {
                image.color = new Color((float)0.2, (float)0.2, (float)0.2, 1);
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
        foreach (Game.Monster m in game.monsters)
        {
            if (name.Equals(m.monsterData.name))
                new MonsterDialog(m);
        }
    }
}
