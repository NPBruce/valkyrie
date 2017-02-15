using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Hero selection options
// This comes up when selection a hero icon to pick hero
public class HeroSelection {

    // Create page of options
	public HeroSelection(Quest.Hero h)
    {
        RenderPage(h.id, 0);
	}

    // Render hero buttons, starting at scroll offset
    public void RenderPage(int heroId, int offset)
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        Game game = Game.Get();

        // Starting array position
        float x = 8;
        float y = 5 - (5f * offset);

        // If we haven't scrolled down
        if (y > 4)
        {
            // Create an 'empty' button to clear selection
            HeroSelectButton(new Vector2(x, y), null, heroId);
        }

        // Get all available heros
        List<string> heroList = new List<string>(game.cd.heros.Keys);
        heroList.Sort();

        bool prevPage = false;
        bool nextPage = false;
        foreach (string hero in heroList)
        {
            // Shift to the right
            x += 5f;
            // If too far right move down
            if (x > UIScaler.GetRight(-13))
            {
                x = 8;
                y += 5f;
            }

            // If too far down set scroll down
            if (y >= 24)
            {
                nextPage = true;
            }
            // If too high set scroll up
            else if (y < 4)
            {
                prevPage = true;
            }
            else
            { // hero in scroll range
                bool disabled = false;
                foreach (Quest.Hero hIt in game.quest.heroes)
                {
                    if ((hIt.heroData == game.cd.heros[hero]) && (hIt.id != heroId))
                    {
                        // Hero already picked
                        disabled = true;
                    }
                }
                HeroSelectButton(new Vector2(x, y), game.cd.heros[hero], heroId, disabled);
            }
        }

        // Are we scrolling?
        if (prevPage || nextPage)
        {
            PrevButton(!prevPage, heroId, offset);
            NextButton(!nextPage, heroId, offset);
        }
    }

    // Create a button for a hero selection option
    public void HeroSelectButton(Vector2 position, HeroData hd, int id, bool disabled = false)
    {
        Sprite heroSprite;
        // Should be game type specific
        Texture2D newTex = Resources.Load("sprites/borders/grey_frame") as Texture2D;
        string name = "";

        // is this an empty hero option?
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
        if (disabled)
        {
            new TextButton(new Vector2(UIScaler.GetRight(-8), 6), new Vector2(2, 4), "/\\", delegate { noAction(); }, Color.gray);
        }
        else
        {
            new TextButton(new Vector2(UIScaler.GetRight(-8), 6), new Vector2(2, 4), "/\\", delegate { RenderPage(heroId, offset - 1); });
        }
    }

    public void NextButton(bool disabled, int heroId, int offset)
    {
        if (disabled)
        {
            new TextButton(new Vector2(UIScaler.GetRight(-8), 19), new Vector2(2, 4), "\\/", delegate { noAction(); }, Color.gray);
        }
        else
        {
            new TextButton(new Vector2(UIScaler.GetRight(-8), 19), new Vector2(2, 4), "\\/", delegate { RenderPage(heroId, offset + 1); });
        }
    }

    public void noAction()
    {
        return;
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
