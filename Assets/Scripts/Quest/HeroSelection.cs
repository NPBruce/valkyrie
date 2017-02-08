using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeroSelection {

	public HeroSelection(Quest.Hero h)
    {
        RenderPage(h.id, 0);
	}

    public void RenderPage(int heroId, int offset)
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        Game game = Game.Get();

        float x = 8;
        float y = 5 - (5f * offset);

        if (y > 4)
        {
            HeroSelectButton(new Vector2(x, y), null, heroId);
        }

        List<string> heroList = new List<string>(game.cd.heros.Keys);

        heroList.Sort();

        bool prevPage = false;
        bool nextPage = false;
        foreach (string hero in heroList)
        {
            x += 5f;
            if (x > UIScaler.GetRight(-13))
            {
                x = 8;
                y += 5f;
            }

            if (y >= 24)
            {
                nextPage = true;
            }
            else if (y < 4)
            {
                prevPage = true;
            }
            else
            {
                bool disabled = false;
                foreach (Quest.Hero hIt in game.quest.heroes)
                {
                    if ((hIt.heroData == game.cd.heros[hero]) && (hIt.id != heroId))
                    {
                        disabled = true;
                    }
                }
                HeroSelectButton(new Vector2(x, y), game.cd.heros[hero], heroId, disabled);
            }
        }

        PrevButton(!prevPage, heroId, offset);
        NextButton(!nextPage, heroId, offset);
    }

    public void HeroSelectButton(Vector2 position, HeroData hd, int id, bool disabled = false)
    {
        Sprite heroSprite;
        Texture2D newTex = Resources.Load("sprites/tokens/objective-token-black") as Texture2D;
        string name = "";

        if (hd != null)
        {
            newTex = ContentData.FileToTexture(hd.image);
            name = hd.name;
        }

        GameObject heroImg = new GameObject("heroImg" + name);
        heroImg.tag = "dialog";

        Game game = Game.Get();
        heroImg.transform.parent = game.uICanvas.transform;

        RectTransform trans = heroImg.AddComponent<RectTransform>();
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, position.y * UIScaler.GetPixelsPerUnit(), 4.25f * UIScaler.GetPixelsPerUnit());
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, position.x * UIScaler.GetPixelsPerUnit(), 4.25f * UIScaler.GetPixelsPerUnit());
        heroImg.AddComponent<CanvasRenderer>();


        UnityEngine.UI.Image image = heroImg.AddComponent<UnityEngine.UI.Image>();
        heroSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
        image.sprite = heroSprite;
        image.rectTransform.sizeDelta = new Vector2(4.25f * UIScaler.GetPixelsPerUnit(), 4.25f * UIScaler.GetPixelsPerUnit());

        UnityEngine.UI.Button button = heroImg.AddComponent<UnityEngine.UI.Button>();
        button.interactable = !disabled;
        button.onClick.AddListener(delegate { SelectHero(id, name); });
    }

    public void PrevButton(bool disabled, int heroId, int offset)
    {
        Sprite heroSprite;
        Texture2D newTex = Resources.Load("sprites/tokens/objective-token-black") as Texture2D;

        GameObject prevImg = new GameObject("prevImg");
        prevImg.tag = "dialog";

        Game game = Game.Get();
        prevImg.transform.parent = game.uICanvas.transform;

        RectTransform trans = prevImg.AddComponent<RectTransform>();
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 6 * UIScaler.GetPixelsPerUnit(), 4f * UIScaler.GetPixelsPerUnit());
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, UIScaler.GetRight(-8) * UIScaler.GetPixelsPerUnit(), 2f * UIScaler.GetPixelsPerUnit());
        prevImg.AddComponent<CanvasRenderer>();


        UnityEngine.UI.Image image = prevImg.AddComponent<UnityEngine.UI.Image>();
        heroSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
        image.sprite = heroSprite;
        image.rectTransform.sizeDelta = new Vector2(2f * UIScaler.GetPixelsPerUnit(), 5f * UIScaler.GetPixelsPerUnit());

        UnityEngine.UI.Button button = prevImg.AddComponent<UnityEngine.UI.Button>();
        button.interactable = !disabled;
        button.onClick.AddListener(delegate { RenderPage(heroId, offset-1); });
    }

    public void NextButton(bool disabled, int heroId, int offset)
    {
        Sprite heroSprite;
        Texture2D newTex = Resources.Load("sprites/tokens/objective-token-black") as Texture2D;

        GameObject nextImg = new GameObject("nextImg");
        nextImg.tag = "dialog";

        Game game = Game.Get();
        nextImg.transform.parent = game.uICanvas.transform;

        RectTransform trans = nextImg.AddComponent<RectTransform>();
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 20 * UIScaler.GetPixelsPerUnit(), 4f * UIScaler.GetPixelsPerUnit());
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, UIScaler.GetRight(-8) * UIScaler.GetPixelsPerUnit(), 2f * UIScaler.GetPixelsPerUnit());
        nextImg.AddComponent<CanvasRenderer>();


        UnityEngine.UI.Image image = nextImg.AddComponent<UnityEngine.UI.Image>();
        heroSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
        image.sprite = heroSprite;
        image.rectTransform.sizeDelta = new Vector2(2f * UIScaler.GetPixelsPerUnit(), 5f * UIScaler.GetPixelsPerUnit());

        UnityEngine.UI.Button button = nextImg.AddComponent<UnityEngine.UI.Button>();
        button.interactable = !disabled;
        button.onClick.AddListener(delegate { RenderPage(heroId, offset+1); });
    }

    public void SelectHero(int id, string name)
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        Game game = Game.Get();
        HeroData hData = null;
        foreach (KeyValuePair<string, HeroData> hd in game.cd.heros)
        {
            if (hd.Value.name.Equals(name))
            {
                hData = hd.Value;
            }
        }
        foreach (Quest.Hero h in game.quest.heroes)
        {
            if (h.id == id)
            {
                h.heroData = hData;
            }
        }

        game.heroCanvas.UpdateImages();
    }
}
