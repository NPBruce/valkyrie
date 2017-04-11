using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

// This class is for drawing hero images on the screen
public class HeroCanvas : MonoBehaviour {

    public float offset;
    public Dictionary<int, UnityEngine.UI.Image> icons;
    public Dictionary<int, UnityEngine.UI.Image> icon_frames;
    // This is assumed in a number of places
    public static float heroSize = 4;
    public static float offsetStart = 3.75f;
    public HeroSelection heroSelection;

    // Called when a quest is started, draws to screen
    public void SetupUI() {
        icons = new Dictionary<int, UnityEngine.UI.Image>();
        icon_frames = new Dictionary<int, UnityEngine.UI.Image>();
        offset = offsetStart;
        Game game = Game.Get();
        foreach (Quest.Hero h in game.quest.heroes)
            AddHero(h, game);
    }

    // Called when existing quest, cleans up
    public void Clean()
    {
        icons = null;
        icon_frames = null;
        // Clean up everything marked as 'herodisplay'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("herodisplay"))
            Object.Destroy(go);
    }

    // Add a hero
    void AddHero(Quest.Hero h, Game game)
    {
        Sprite heroSprite;
        Sprite frameSprite;

        Texture2D frameTex = Resources.Load("sprites/borders/grey_frame") as Texture2D;
        if (game.gameType is MoMGameType)
        {
            frameTex = Resources.Load("sprites/borders/momframeempty") as Texture2D;
        }

        string heroName = h.id.ToString();

        if (h.heroData != null)
        {
            frameTex = Resources.Load("sprites/borders/blue_frame") as Texture2D;
            if (game.gameType is MoMGameType)
            {
                frameTex = Resources.Load("sprites/borders/momframe") as Texture2D;
            }
            heroName = h.heroData.name.Translate();
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
        if (game.gameType is MoMGameType)
        {
            image.rectTransform.sizeDelta = new Vector2(heroSize * UIScaler.GetPixelsPerUnit() * 0.9f, heroSize * UIScaler.GetPixelsPerUnit() * 0.9f);
            heroFrame.transform.SetAsLastSibling();
        }
        image.color = Color.clear;

        UnityEngine.UI.Button button = heroImg.AddComponent<UnityEngine.UI.Button>();
        button.interactable = true;
        button.onClick.AddListener(delegate { HeroDiag(h.id); });

        // Add hero image if selected
        if (h.heroData != null)
        {
            Texture2D newTex = ContentData.FileToTexture(h.heroData.image);
            heroSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            image.sprite = heroSprite;
        }
    }

    // Update hero image state
    public void UpdateStatus()
    {
        // If we haven't set up yet just return
        if (icons == null) return;
        if (icon_frames == null) return;

        Game game = Game.Get();
        foreach(Quest.Hero h in game.quest.heroes)
        {
            // Start as white (normal)
            icons[h.id].color = Color.white;
            icon_frames[h.id].color = Color.white;

            if (h.defeated)
            {
                // Grey hero image
                icons[h.id].color = new Color((float)0.2, (float)0.2, (float)0.2, 1);
            }
            if (h.activated)
            {
                // Grey frame
                icon_frames[h.id].color = new Color((float)0.2, (float)0.2, (float)0.2, 1);
            }
            if (h.heroData == null)
            {
                // No hero, make invisible
                icons[h.id].color = Color.clear;
                icon_frames[h.id].color = Color.clear;
            }
            if (h.selected)
            {
                // green frame
                icon_frames[h.id].color = Color.green;
            }
        }
    }

    // Redraw images
    public void UpdateImages()
    {
        if (icons == null) return;
        if (icon_frames == null) return;

        Game game = Game.Get();

        foreach (Quest.Hero h in game.quest.heroes)
        {
            Texture2D frameTex = Resources.Load("sprites/borders/grey_frame") as Texture2D;
            if (game.gameType is MoMGameType)
            {
                frameTex = Resources.Load("sprites/borders/momframeempty") as Texture2D;
            }
            icons[h.id].color = Color.clear;
            icon_frames[h.id].color = Color.clear;

            if (!game.quest.heroesSelected)
            {
                icon_frames[h.id].color = Color.white;
            }

            if (h.heroData != null)
            {

                frameTex = Resources.Load("sprites/borders/blue_frame") as Texture2D;
                if (game.gameType is MoMGameType)
                {
                    frameTex = Resources.Load("sprites/borders/momframe") as Texture2D;
                }
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

    // Called when hero pressed
    void HeroDiag(int id)
    {
        Game game = Game.Get();
        Quest.Hero target = null;

        // Find the pressed hero
        foreach (Quest.Hero h in game.quest.heroes)
        {
            if (h.id == id)
            {
                target = h;
                break;
            }
        }

        // Game hasn't started, remove any selected hero
        if (!game.quest.heroesSelected)
        {
            target.heroData = null;
            UpdateImages();
            if (heroSelection != null) heroSelection.Update();
            return;
        }

        // If there are any other dialogs
        if (GameObject.FindGameObjectWithTag("dialog") != null)
        {
            // Check if we are in a hero selection dialog
            if (game.quest.eManager.currentEvent != null && game.quest.eManager.currentEvent.qEvent.maxHeroes != 0)
            {
                // Invert hero selection
                target.selected = !target.selected;
                UpdateStatus();
            }
            // Non hero selection dialog, do nothing
            return;
        }

        // We are in game and a valid hero was selected
        if (game.quest.heroesSelected && target.heroData != null)
        {
            new HeroDialog(target);
        }
    }

    // End hero selection and reorder heroselect
    // FIXME: bad name
    // FIXME: why is this even here?
    public void EndSection()
    {
        int heroCount = 0;

        Destroyer.Dialog();

        // Count number of selected heroes
        Game game = Game.Get();
        foreach (Quest.Hero h in game.quest.heroes)
        {
            if (h.heroData != null) heroCount++;
        }

        // Check for validity
        if (heroCount < 2) return;

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("heroselect"))
            Object.Destroy(go);
        heroSelection = null;

        // Reorder heros so that selected heroes are first
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

        // Set quest flag based on hero count
        game.quest.vars.SetValue("#heroes", heroCount);

        game.quest.heroesSelected = true;

        UpdateImages();
        UpdateStatus();

        // Clear off heros if not required
        if (!game.gameType.DisplayHeroes())
        {
            Clean();
        }

        // Draw morale if required
        if (game.gameType.DisplayMorale())
        {
            game.moraleDisplay = new MoraleDisplay();
            game.QuestStartEvent();
        }
        else
        {
            new InvestigatorItems();
        }

        List<string> music = new List<string>();
        foreach (AudioData ad in game.cd.audio.Values)
        {
            if (ad.ContainsTrait("quest")) music.Add(ad.file);
        }
        game.audioControl.Music(music);

    }
}
