using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeroCanvas : MonoBehaviour {

    public float offset;
    public Dictionary<int, UnityEngine.UI.Image> icons;
    public Dictionary<int, UnityEngine.UI.Image> icon_frames;
    public static float heroSize = 4;
    public static float offsetStart = 3.75f;

    public void SetupUI() {
        icons = new Dictionary<int, UnityEngine.UI.Image>();
        icon_frames = new Dictionary<int, UnityEngine.UI.Image>();
        offset = offsetStart;
        Game game = Game.Get();
        foreach (Quest.Hero h in game.quest.heroes)
            AddHero(h, game);
    }

    public void Clean()
    {
        icons = null;
        icon_frames = null;
        // Clean up everything marked as 'herodisplay'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("herodisplay"))
            Object.Destroy(go);
    }

    void AddHero(Quest.Hero h, Game game)
    {
        Sprite heroSprite;
        Sprite frameSprite;

        Texture2D frameTex = Resources.Load("sprites/borders/grey_frame") as Texture2D;

        string heroName = h.id.ToString();

        if (h.heroData != null)
        {
            frameTex = Resources.Load("sprites/borders/blue_frame") as Texture2D;
            heroName = h.heroData.name;
        }

        GameObject heroFrame = new GameObject("heroFrame" + heroName);
        heroFrame.tag = "herodisplay";
        heroFrame.transform.parent = game.uICanvas.transform;
        RectTransform transFrame = heroFrame.AddComponent<RectTransform>();

        transFrame.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (0.25f + offset) * UIScaler.GetPixelsPerUnit(), heroSize * UIScaler.GetPixelsPerUnit());
        transFrame.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0.25f * UIScaler.GetPixelsPerUnit(), heroSize * UIScaler.GetPixelsPerUnit());
        heroFrame.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Image imageFrame = heroFrame.AddComponent<UnityEngine.UI.Image>();
        icon_frames.Add(h.id, imageFrame);
        frameSprite = Sprite.Create(frameTex, new Rect(0, 0, frameTex.width, frameTex.height), Vector2.zero, 1);
        imageFrame.sprite = frameSprite;
        imageFrame.rectTransform.sizeDelta = new Vector2(heroSize * UIScaler.GetPixelsPerUnit(), heroSize * UIScaler.GetPixelsPerUnit());

        UnityEngine.UI.Button buttonFrame = heroFrame.AddComponent<UnityEngine.UI.Button>();
        buttonFrame.interactable = true;
        buttonFrame.onClick.AddListener(delegate { HeroDiag(h.id); });

        GameObject heroImg = new GameObject("heroImg" + heroName);
        heroImg.tag = "herodisplay";
        heroImg.transform.parent = game.uICanvas.transform;
        RectTransform trans = heroImg.AddComponent<RectTransform>();

        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (0.25f + offset) * UIScaler.GetPixelsPerUnit(), heroSize * UIScaler.GetPixelsPerUnit());
        offset += heroSize + 0.5f;
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0.25f * UIScaler.GetPixelsPerUnit(), heroSize * UIScaler.GetPixelsPerUnit());
        heroImg.AddComponent<CanvasRenderer>();
        UnityEngine.UI.Image image = heroImg.AddComponent<UnityEngine.UI.Image>();

        icons.Add(h.id, image);
        image.rectTransform.sizeDelta = new Vector2(heroSize * UIScaler.GetPixelsPerUnit() * 0.8f, heroSize * UIScaler.GetPixelsPerUnit() * 0.8f);
        image.color = Color.clear;

        UnityEngine.UI.Button button = heroImg.AddComponent<UnityEngine.UI.Button>();
        button.interactable = true;
        button.onClick.AddListener(delegate { HeroDiag(h.id); });

        if (h.heroData != null)
        {
            Texture2D newTex = ContentData.FileToTexture(h.heroData.image);
            heroSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            image.sprite = heroSprite;
        }
    }

    public void UpdateStatus()
    {
        // If we haven't set up yet just return
        if (icons == null) return;
        if (icon_frames == null) return;

        Game game = Game.Get();
        foreach(Quest.Hero h in game.quest.heroes)
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
        if (icons == null) return;
        if (icon_frames == null) return;

        Game game = Game.Get();

        foreach (Quest.Hero h in game.quest.heroes)
        {
            Texture2D frameTex = Resources.Load("sprites/borders/grey_frame") as Texture2D;
            icons[h.id].color = Color.clear;
            icon_frames[h.id].color = Color.clear;

            if (!game.quest.heroesSelected)
            {
                icon_frames[h.id].color = Color.white;
            }

            if (h.heroData != null)
            {

                frameTex = Resources.Load("sprites/borders/blue_frame") as Texture2D;
                Texture2D heroTex = ContentData.FileToTexture(h.heroData.image);

                Sprite heroSprite = Sprite.Create(heroTex, new Rect(0, 0, heroTex.width, heroTex.height), Vector2.zero, 1);

                icons[h.id].sprite = heroSprite;
                icons[h.id].color = Color.white;
                icon_frames[h.id].color = Color.white;
            }

            Sprite frameSprite = Sprite.Create(frameTex, new Rect(0, 0, frameTex.width, frameTex.height), Vector2.zero, 1);
            icon_frames[h.id].sprite = frameSprite;
        }
    }

    void HeroDiag(int id)
    {
        Game game = Game.Get();
        Quest.Hero target = null;

        foreach (Quest.Hero h in game.quest.heroes)
        {
            if (h.id == id)
            {
                target = h;
            }
        }

        // If there are any other dialogs
        if (GameObject.FindGameObjectWithTag("dialog") != null)
        {
            if (game.quest.eManager.currentEvent != null && game.quest.eManager.currentEvent.qEvent.maxHeroes != 0)
            {
                target.selected = !target.selected;
                UpdateStatus();
            }
            return;
        }

        if (game.quest.heroesSelected && target.heroData != null)
        {
            new HeroDialog(target);
        }
        if (!game.quest.heroesSelected)
        {
            icon_frames[id].color = new Color((float)0.3, (float)0.3, (float)0.3);
            if (icons[id].color.a > 0)
            {
                icons[id].color = new Color((float)0.3, (float)0.3, (float)0.3);
            }
            new HeroSelection(target);
        }
    }

    public void EndSection()
    {
        int heroCount = 0;

        if (GameObject.FindGameObjectWithTag("dialog") != null)
            return;

        Game game = Game.Get();
        foreach (Quest.Hero h in game.quest.heroes)
        {
            if (h.heroData != null) heroCount++;
        }

        if (heroCount < 2) return;

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("heroselect"))
            Object.Destroy(go);

        for (int i = 0; i < game.quest.heroes.Count - 1; i++)
        {
            int j = i;

            while (game.quest.heroes[i].heroData == null && j < game.quest.heroes.Count)
            {
                game.quest.heroes[i].heroData = game.quest.heroes[j].heroData;
                game.quest.heroes[j].heroData = null;
                j++;
            }
        }

        game.quest.flags.Add("#" + heroCount + "hero");

        game.quest.heroesSelected = true;

        UpdateImages();
        UpdateStatus();

        if (game.gameType.DisplayMorale())
        {
            game.moraleDisplay = new MoraleDisplay();
        }


        if (!game.gameType.DisplayHeroes())
        {
            Clean();
        }

        // Create the menu button
        new MenuButton();
        new NextStageButton();

        game.quest.eManager.EventTriggerType("EventStart");
    }
}
