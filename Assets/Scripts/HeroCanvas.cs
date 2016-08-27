using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeroCanvas : MonoBehaviour {

    public int offset = 0;
    public Dictionary<int, UnityEngine.UI.Image> icons;

    public void SetupUI() {
        icons = new Dictionary<int, UnityEngine.UI.Image>();

        Game game = FindObjectOfType<Game>();
        foreach (Game.Hero h in game.heros)
            AddHero(h, game);
    }

    void AddHero(Game.Hero h, Game game)
    {

        Sprite heroSprite;

        string heroName = h.id.ToString();
        Texture2D newTex = Resources.Load("sprites/tokens/objective-token-black") as Texture2D;

        if (h.heroData != null)
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
        icons.Add(h.id, image);
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
            UnityEngine.UI.Image image = icons[h.id];
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
            if (h.heroData == null)
            {
                image.color = Color.clear;
            }
            if (h.selected)
            {
                image.color = Color.green;
            }
        }
    }

    public void UpdateImages()
    {
        Game game = GameObject.FindObjectOfType<Game>();
        foreach (Game.Hero h in game.heros)
        {
            UnityEngine.UI.Image image = icons[h.id];

            Texture2D newTex = Resources.Load("sprites/tokens/objective-token-black") as Texture2D;

            if (h.heroData != null)
            {
                string imagePath = @"file://" + h.heroData.image;
                WWW www = new WWW(imagePath);
                newTex = new Texture2D(256, 256, TextureFormat.DXT5, false);
                www.LoadImageIntoTexture(newTex);
            }

            Sprite heroSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            image.sprite = heroSprite;
            image.color = Color.white;
        }
    }

    void HeroDiag(int id)
    {
        Game game = GameObject.FindObjectOfType<Game>();
        Game.Hero target = null;

        foreach (Game.Hero h in game.heros)
        {
            if (h.id == id)
            {
                target = h;
            }
        }

        // If there are any other dialogs
        if (GameObject.FindGameObjectWithTag("dialog") != null)
        {
            if (game.eventList.Count > 0 && game.eventList.Peek().maxHeroes != 0)
            {
                target.selected = !target.selected;
                UpdateStatus();
            }
            return;
        }

        if (game.heroesSelected && target.heroData != null)
        {
            new HeroDialog(target);
        }
        if (!game.heroesSelected)
        {
            icons[id].color = new Color((float)0.3, (float)0.3, (float)0.3);
            new HeroSelection(target);
        }
    }

    public void EndSection()
    {
        int heroCount = 0;

        if (GameObject.FindGameObjectWithTag("dialog") != null)
            return;

        Game game = GameObject.FindObjectOfType<Game>();
        foreach (Game.Hero h in game.heros)
        {
            if (h.heroData != null) heroCount++;
        }

        if (heroCount < 2) return;

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("heroselect"))
            Object.Destroy(go);

        /*for(int i = 0; i < game.heros.Count; i++)
        {
            game.heros[i].id = 0;
        }*/

        UpdateImages();
        UpdateStatus();

        game.heroesSelected = true;
        EventHelper.QueueEvent("EventStart");
    }
}
