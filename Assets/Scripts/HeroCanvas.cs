using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeroCanvas : MonoBehaviour {

    public int offset = 0;
    public Dictionary<string, UnityEngine.UI.Image> icons;

    // Use this for initialization
    void Start() {
        icons = new Dictionary<string, UnityEngine.UI.Image>();

        Game game = FindObjectOfType<Game>();
        foreach (Game.Hero h in game.heros)
            AddHero(h, game);
    }

    void AddHero(Game.Hero h, Game game)
    {
        string imagePath = @"file://" + h.heroData.image;

        Sprite heroSprite;

        WWW www = new WWW(imagePath);
        Texture2D newTex = new Texture2D(256, 256, TextureFormat.DXT5, false);
        www.LoadImageIntoTexture(newTex);

        GameObject heroImg = new GameObject("heroImg" + h.heroData.name);

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
        icons.Add(h.heroData.name, image);
        heroSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
        image.sprite = heroSprite;
        image.rectTransform.sizeDelta = new Vector2(80, 80);

        UnityEngine.UI.Button button = heroImg.AddComponent<UnityEngine.UI.Button>();
        button.interactable = true;
        //button.onClick.AddListener(delegate { HeroDiag(h.heroData.name); });
    }

   /* void HeroDiag(string name)
    {
        GameObject activated = new GameObject("activated");
        activated.tag = "dialog";

        Canvas[] canvii = GameObject.FindObjectsOfType<Canvas>();
        Canvas canvas = canvii[0];
        foreach (Canvas c in canvii)
        {
            if (c.name.Equals("UICanvas"))
            {
                canvas = c;
            }
        }


        activated.transform.parent = canvas.transform;

        RectTransform trans = activated.AddComponent<RectTransform>();
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 90, 20);
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 400, 50);

        CanvasRenderer cr = activated.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Button button = activated.AddComponent<UnityEngine.UI.Button>();
        button.interactable = true;
        button.onClick.AddListener(delegate { onActive(); });

        UnityEngine.UI.Text text = activated.AddComponent<UnityEngine.UI.Text>();
        text.color = Color.white;
        text.text = "Cancel";
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

    }*/

    void UpdateStatus()
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
            else if (h.activated)
            {
                image.color = new Color(50, 50, 50, 255);
            }
            else if (h.defeated && h.activated)
            {
                image.color = new Color(50, 0, 0, 255);
            }

        }
    }
}
