using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeroSelection {

	public HeroSelection(Game.Hero h)
    {
        Game game = GameObject.FindObjectOfType<Game>();

        int x = 200;
        int y = 100;

        HeroSelectButton(new Vector2(x, y), null, h.id);
        foreach (KeyValuePair<string, HeroData> hd in game.cd.heros)
        {
            x += 120;
            if (x > 900)
            {
                x = 200;
                y += 120;
            }
            HeroSelectButton(new Vector2(x, y), hd.Value, h.id);
        }
	}

    public void HeroSelectButton(Vector2 position, HeroData hd, int id)
    {
        Sprite heroSprite;
        Texture2D newTex = Resources.Load("sprites/tokens/objective-token-black") as Texture2D;
        string name = "";

        if (hd != null)
        {
            string imagePath = @"file://" + hd.image;
            WWW www = new WWW(imagePath);
            newTex = new Texture2D(256, 256, TextureFormat.DXT5, false);
            www.LoadImageIntoTexture(newTex);
            name = hd.name;
        }

        GameObject heroImg = new GameObject("heroImg" + name);
        heroImg.tag = "dialog";

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
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, position.y, 50);
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, position.x, 50);
        heroImg.AddComponent<CanvasRenderer>();


        UnityEngine.UI.Image image = heroImg.AddComponent<UnityEngine.UI.Image>();
        heroSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
        image.sprite = heroSprite;
        image.rectTransform.sizeDelta = new Vector2(80, 80);

        UnityEngine.UI.Button button = heroImg.AddComponent<UnityEngine.UI.Button>();
        button.interactable = true;
        button.onClick.AddListener(delegate { SelectHero(id, name); });
    }

    public void SelectHero(int id, string name)
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        Game game = GameObject.FindObjectOfType<Game>();
        HeroData hData = null;
        foreach (KeyValuePair<string, HeroData> hd in game.cd.heros)
        {
            if (hd.Value.name.Equals(name))
            {
                hData = hd.Value;
            }
        }
        foreach (Game.Hero h in game.heros)
        {
            if (h.id == id)
            {
                h.heroData = hData;
            }
        }

        HeroCanvas hc = GameObject.FindObjectOfType<HeroCanvas>();
        hc.UpdateImages();
    }
}
