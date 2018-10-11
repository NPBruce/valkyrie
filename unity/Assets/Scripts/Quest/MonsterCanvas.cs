﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

// This class controls the list of monsters
public class MonsterCanvas : MonoBehaviour
{
    private readonly StringKey UP_ARROW = new StringKey(null, "˄", false);
    private readonly StringKey DOWN_ARROW = new StringKey(null, "˅", false);

    // offset stores the scoll position, reset at creation
    public int offset = 0;
    // This is assumed in a few places, can't just be changed
    public static float monsterSize = 4;
    // We keep a collection of the icons here
    public List<MonsterIcon> icons;

    void Awake()
    {
        icons = new List<MonsterIcon>();
    }

    // Call to update list of monsters
    public void UpdateList()
    {
        // Clean up everything marked as 'monsters'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.MONSTERS))
            Object.Destroy(go);

        // New list
        icons = new List<MonsterIcon>();

        Game game = Game.Get();
        int index = 0;
        // Create icons (not drawn)
        foreach (Quest.Monster m in game.quest.monsters)
        {
            icons.Add(new MonsterIcon(m, index++));
        }

        if (game.quest.monsters.Count - offset < 5)
        {
            offset = game.quest.monsters.Count - 5;
            if (offset < 0)
            {
                offset = 0;
            }
        }

        // Draw scoll buttons if required
        DrawUp();
        DrawDown();

        // Draw icons in scroll range
        foreach (MonsterIcon mi in icons)
        {
            mi.Draw(offset);
        }
    }

    // Draw up button if > 5 monsters, disabled if at top
    public void DrawUp()
    {
        Game game = Game.Get();
        // Check if scroll required
        if (game.quest.monsters.Count < 6 && offset == 0)
        {
            return;
        }
        // If at top
        UIElement ui = new UIElement(Game.MONSTERS);
        ui.SetLocation(UIScaler.GetRight(-4.25f), 3, 4, 1);
        if (offset == 0)
        {
            ui.SetText(UP_ARROW, Color.gray);
            new UIElementBorder(ui, Color.gray);
        }
        else
        {
            // Scroll up active
            ui.SetText(UP_ARROW);
            new UIElementBorder(ui);
            ui.SetButton(delegate { Move(-1); });
        }
        ui.SetFontSize(UIScaler.GetMediumFont());
    }

    // Draw down button if > 5 monsters, disabled if at bottom
    public void DrawDown()
    {
        Game game = Game.Get();
        // Check if scroll required
        if (game.quest.monsters.Count < 6)
        {
            return;
        }
        // If at buttom
        UIElement ui = new UIElement(Game.MONSTERS);
        ui.SetLocation(UIScaler.GetRight(-4.25f), 26.1f, 4, 1);
        if (game.quest.monsters.Count - offset <  6)
        {
            ui.SetText(DOWN_ARROW, Color.gray);
            new UIElementBorder(ui, Color.gray);
        }
        else
        {
            // Scroll up active
            ui.SetText(DOWN_ARROW);
            new UIElementBorder(ui);
            ui.SetButton(delegate { Move(); });
        }
        ui.SetFontSize(UIScaler.GetMediumFont());
    }

    // Called by scroll up/down
    public static void Move(int index = 1)
    {
        Game game = Game.Get();
        // Update offset and redraw
        game.monsterCanvas.offset += index;
        game.monsterCanvas.UpdateList();
    }

    public static void noAction()
    {
    }

    // FIXME: should update existing, does this draw over the top?
    public void UpdateStatus()
    {
        foreach (MonsterIcon mi in icons)
        {
            mi.Update();
        }
    }

    // class for tracking monster sprites
    public class MonsterIcon
    {
        Quest.Monster m;

        Game game;

        Sprite iconSprite;
        Sprite frameSprite;
        Sprite duplicateSprite;
        UnityEngine.UI.Image icon;
        UnityEngine.UI.Image iconFrame;
        UnityEngine.UI.Image iconDupe;

        // Location of the monster in the list
        int index;

        public MonsterIcon(Quest.Monster monster, int i = 0)
        {
            m = monster;
            index = i;

            game = Game.Get();

            // Get monster image and grame
            Texture2D newTex = ContentData.FileToTexture(m.monsterData.image);
            // FIXME: should be game type specific
            Texture2D frameTex = Resources.Load("sprites/borders/Frame_Monster_1x1") as Texture2D;
            Texture2D dupeTex = Resources.Load("sprites/monster_duplicate_" + m.duplicate) as Texture2D;
            iconSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            frameSprite = Sprite.Create(frameTex, new Rect(0, 0, frameTex.width, frameTex.height), Vector2.zero, 1);
            if (dupeTex != null)
            {
               duplicateSprite = Sprite.Create(dupeTex, new Rect(0, 0, dupeTex.width, dupeTex.height), Vector2.zero, 1);
            }
        }

        // Draw monster if in scroll range (offset is scroll position)
        public void Draw(int offset)
        {
            float top_position_y = 4.3f; //3.75
            float offset_y = 4.4f; // 4.5

            // Check if in scroll range
            if (index < offset)
            {
                return;
            }
            if (index > offset + 4)
            {
                return;
            }

            GameObject mImg = new GameObject("monsterImg" + m.monsterData.name);
            mImg.tag = Game.MONSTERS;
            mImg.transform.SetParent(game.uICanvas.transform);

            RectTransform trans = mImg.AddComponent<RectTransform>();
            trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (top_position_y + ((index - offset) * offset_y)) * UIScaler.GetPixelsPerUnit(), monsterSize * UIScaler.GetPixelsPerUnit());
            trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0.25f * UIScaler.GetPixelsPerUnit(), monsterSize * UIScaler.GetPixelsPerUnit());
            mImg.AddComponent<CanvasRenderer>();

            icon = mImg.AddComponent<UnityEngine.UI.Image>();
            icon.sprite = iconSprite;
            icon.rectTransform.sizeDelta = new Vector2(monsterSize * UIScaler.GetPixelsPerUnit(), monsterSize * UIScaler.GetPixelsPerUnit());

            UnityEngine.UI.Button button = mImg.AddComponent<UnityEngine.UI.Button>();
            button.interactable = true;
            button.onClick.AddListener(delegate { MonsterDiag(); });

            iconFrame = null;
            if (game.gameType is D2EGameType)
            {
                icon.rectTransform.sizeDelta = new Vector2(monsterSize * UIScaler.GetPixelsPerUnit() * 0.83f, monsterSize * UIScaler.GetPixelsPerUnit() * 0.83f);
                GameObject mImgFrame = new GameObject("monsterFrame" + m.monsterData.name);
                mImgFrame.tag = Game.MONSTERS;
                mImgFrame.transform.SetParent(game.uICanvas.transform);

                RectTransform transFrame = mImgFrame.AddComponent<RectTransform>();
                transFrame.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (top_position_y + ((index - offset) * offset_y)) * UIScaler.GetPixelsPerUnit(), monsterSize * UIScaler.GetPixelsPerUnit());
                transFrame.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0.25f * UIScaler.GetPixelsPerUnit(), monsterSize * UIScaler.GetPixelsPerUnit());
                mImgFrame.AddComponent<CanvasRenderer>();

                iconFrame = mImgFrame.AddComponent<UnityEngine.UI.Image>();
                iconFrame.sprite = frameSprite;
                iconFrame.rectTransform.sizeDelta = new Vector2(monsterSize * UIScaler.GetPixelsPerUnit(), monsterSize * UIScaler.GetPixelsPerUnit());

                UnityEngine.UI.Button buttonFrame = mImgFrame.AddComponent<UnityEngine.UI.Button>();
                buttonFrame.interactable = true;
                buttonFrame.onClick.AddListener(delegate { MonsterDiag(); });

                if (m.GetHealth() != 0)
                {
                    UIElement ui = new UIElement(Game.MONSTERS);
                    ui.SetLocation(UIScaler.GetRight(-2.25f), 5.75f + ((index - offset) * offset_y), 2, 2);
                    ui.SetText(m.GetHealth().ToString(), Color.red);
                    ui.SetFontSize(UIScaler.GetMediumFont());
                    ui.SetButton(MonsterDiag);
                    new UIElementBorder(ui, Color.red);
                }
            }
            else
            {
                // MoM
                if(game.quest.phase == Quest.MoMPhase.investigator)
                {
                    DrawAwareness(top_position_y + ((index - offset) * offset_y));
                }
                else
                {
                    DrawHorror(top_position_y + ((index - offset) * offset_y));
                }
            }

            iconDupe = null;
            if (duplicateSprite != null)
            {
                GameObject mImgDupe = new GameObject("monsterDupe" + m.monsterData.name);
                mImgDupe.tag = Game.MONSTERS;
                mImgDupe.transform.SetParent(game.uICanvas.transform);

                RectTransform dupeFrame = mImgDupe.AddComponent<RectTransform>();
                dupeFrame.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, ((monsterSize / 2f) + top_position_y + ((index - offset) * offset_y)) * UIScaler.GetPixelsPerUnit(), monsterSize * UIScaler.GetPixelsPerUnit() / 2f);
                dupeFrame.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 2.25f * UIScaler.GetPixelsPerUnit(), monsterSize * UIScaler.GetPixelsPerUnit() / 2f);
                mImgDupe.AddComponent<CanvasRenderer>();

                iconDupe = mImgDupe.AddComponent<UnityEngine.UI.Image>();
                iconDupe.sprite = duplicateSprite;
                iconDupe.rectTransform.sizeDelta = new Vector2(monsterSize * UIScaler.GetPixelsPerUnit() / 2f, monsterSize * UIScaler.GetPixelsPerUnit() / 2f);

                UnityEngine.UI.Button buttonDupe = mImgDupe.AddComponent<UnityEngine.UI.Button>();
                buttonDupe.interactable = true;
                buttonDupe.onClick.AddListener(delegate { MonsterDiag(); });
            }

            Update();
        }

        public void DrawHorror(float offset)
        {
            if (m.monsterData.horror == 0) return;
            UIElement ui = new UIElement(Game.MONSTERS);
            ui.SetLocation(UIScaler.GetRight(-2.25f), offset + 2, 2, 2);
            ui.SetText(m.monsterData.horror.ToString(), Color.blue);
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(MonsterDiag);
            new UIElementBorder(ui, Color.blue);
        }

        public void DrawAwareness(float offset)
        {
            if (m.monsterData.awareness == 0) return;
            UIElement ui = new UIElement(Game.MONSTERS);
            ui.SetLocation(UIScaler.GetRight(-2.25f), offset, 2, 2);
            ui.SetText(m.monsterData.awareness.ToString(), Color.green);
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(MonsterDiag);
            new UIElementBorder(ui, Color.green);
        }

        public void Update()
        {
            // MoM doesn't do colours
            // Bad test
            if (iconFrame == null) return;

            // Set colour based on monster state
            if (m.activated && m.unique)
            {
                // Green frame, dim monster
                icon.color = new Color(0.5f, 0.5f, 0.5f, 1);
                if (iconFrame != null)
                {
                    iconFrame.color = new Color(0f, 0.3f, 0f, 1);
                }
                if (iconDupe != null)
                {
                    iconDupe.color = new Color(0.5f, 0.5f, 0.5f, 1);
                }
            }
            else if (m.activated)
            {
                // dim
                icon.color = new Color(0.5f, 0.5f, 0.5f, 1);
                if (iconFrame != null)
                {
                    iconFrame.color = new Color((float)0.2, (float)0.2, (float)0.2, 1);
                }
                if (iconDupe != null)
                {
                    iconDupe.color = new Color(0.5f, 0.5f, 0.5f, 1);
                }
            }
            else if (m.unique)
            {
                // green frame
                icon.color = Color.white;
                if (iconFrame != null)
                {
                    iconFrame.color = new Color(0.6f, 1f, 0.6f, 1);
                }
                if (iconDupe != null)
                {
                    iconDupe.color = new Color(0.5f, 0.5f, 0.5f, 1);
                }
            }
            else
            {
                icon.color = Color.white;
                if (iconFrame != null)
                {
                    iconFrame.color = Color.white;
                }
                if (iconDupe != null)
                {
                    iconDupe.color = Color.white;
                }
            }
        }

        // Function when monster icon pressed
        public void MonsterDiag()
        {
            // If there are any other dialogs open just finish
            if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
                return;

            Game game = Game.Get();
            // This is a bad test
            if (game.gameType.DisplayHeroes())
            {
                new MonsterDialog(m);
            }
            else
            {
                new MonsterDialogMoM(m);
            }
        }
    }
}
