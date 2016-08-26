using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeroCanvas : MonoBehaviour {

    public int offset = 0;
    public Dictionary<string, UnityEngine.UI.Image> icons;

    public void SetupUI() {
        icons = new Dictionary<string, UnityEngine.UI.Image>();

        Game game = FindObjectOfType<Game>();
        foreach (Game.Hero h in game.heros)
            AddHero(h, game);
    }

    void AddHero(Game.Hero h, Game game)
    {

        Sprite heroSprite;

        string heroName = "null";
        Texture2D newTex = Resources.Load("sprites/tokens/search-token") as Texture2D;

        if (h != null)
        {
            string imagePath = @"file://" + h.heroData.image;
            WWW www = new WWW(imagePath);
            newTex = new Texture2D(256, 256, TextureFormat.DXT5, false);
            www.LoadImageIntoTexture(newTex);
            heroName = h.heroData.name;
        }

        GameObject heroImg = new GameObject("heroImg" + heroName);

        Canvas[] canvii = GameObject.FindObjectsOfType<Canvas>();
        Canvas canvas = canvii[0];
        foreach (Canvas c in canvii)
        {
            if (c.name.Equals("UICanvas"))
            {
                canvas = c;
            }
        }
        heroImg.transform.parent = canvas.transform;

        RectTransform trans = heroImg.AddComponent<RectTransform>();
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 30 + offset, 50);
        offset += 100;
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 30, 50);
        heroImg.AddComponent<CanvasRenderer>();


        UnityEngine.UI.Image image = heroImg.AddComponent<UnityEngine.UI.Image>();
        icons.Add(heroName, image);
        heroSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
        image.sprite = heroSprite;
        image.rectTransform.sizeDelta = new Vector2(80, 80);

        UnityEngine.UI.Button button = heroImg.AddComponent<UnityEngine.UI.Button>();
        button.interactable = true;
        button.onClick.AddListener(delegate { HeroDiag(h.id); });
    }

    public void UpdateStatus()
    {
        Game game = GameObject.FindObjectOfType<Game>();
        foreach(Game.Hero h in game.heros)
        {
            UnityEngine.UI.Image image = icons[h.heroData.name];
            image.color = Color.white;
            if (h.defeated)
            {
                image.color = Color.red;
            }
            if (h.activated)
            {
                image.color = new Color((float)0.2, (float)0.2, (float)0.2, 1);
            }
            if (h.defeated && h.activated)
            {
                image.color = new Color((float) 0.2, 0, 0, 1);
            }

        }
    }

    void HeroDiag(int id)
    {
        // If there are any other dialogs open just finish
        if (GameObject.FindGameObjectWithTag("dialog") != null)
            return;

        Game game = GameObject.FindObjectOfType<Game>();
        foreach (Game.Hero h in game.heros)
        {
            if (h.id == id)
                new HeroDialog(h);
        }
    }
}
