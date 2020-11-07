﻿using System;
using System.Collections.Generic;
using Assets.Scripts.Content;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ValkyrieTools;

namespace Assets.Scripts.UI.Screens
{
    // Class for options menu
    public class OptionsScreen
    {
        private static readonly string IMG_LOW_EDITOR_TRANSPARENCY    = "ImageLowEditorTransparency";
        private static readonly string IMG_MEDIUM_EDITOR_TRANSPARENCY = "ImageMediumEditorTransparency";
        private static readonly string IMG_HIGH_EDITOR_TRANSPARENCY   = "ImageHighEditorTransparency";

        private readonly StringKey OPTIONS = new StringKey("val", "OPTIONS");
        private readonly StringKey CHOOSE_LANG = new StringKey("val", "CHOOSE_LANG");
        private readonly StringKey EFFECTS = new StringKey("val", "EFFECTS");
        private readonly StringKey MUSIC = new StringKey("val", "MUSIC");
        private readonly StringKey SET_EDITOR_ALPHA = new StringKey("val", "SET_EDITOR_ALPHA");

        Game game = Game.Get();

        public Slider musicSlide;
        public Slider musicSlideRev;
        public Slider effectSlide;
        public Slider effectSlideRev;

        // Create a menu which will take up the whole screen and have options.  All items are dialog for destruction.
        public OptionsScreen()
        {
            // This will destroy all, because we shouldn't have anything left at the main menu
            Destroyer.Destroy();

            game = Game.Get();

            // Create elements for the screen
            CreateElements();
        }

        /// <summary>
        /// Method to create UI elements in the screen
        /// </summary>
        /// <param name="game">current game</param>
        private void CreateElements()
        {
            // Options screen text
            UIElement ui = new UIElement();
            ui.SetLocation(2, 1, UIScaler.GetWidthUnits() - 4, 3);
            ui.SetText(OPTIONS);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetLargeFont());

            CreateLanguageElements();

            CreateAudioElements();

            CreateEditorTransparencyElements();

            // Button for back to main menu
            ui = new UIElement();
            ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
            ui.SetText(CommonStringKeys.BACK, Color.red);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(GameStateManager.MainMenu);
            new UIElementBorder(ui, Color.red);
        }

        private void CreateEditorTransparencyElements()
        {
            Game game = Game.Get();

            // Select language text
            UIElement ui = new UIElement(Game.DIALOG);
            ui.SetLocation(UIScaler.GetHCenter() - 8, 5, 16, 2);
            ui.SetText(SET_EDITOR_ALPHA);
            ui.SetTextAlignment(TextAnchor.MiddleCenter);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());

            Texture2D SampleTex = ContentData.FileToTexture(game.cd.images[IMG_LOW_EDITOR_TRANSPARENCY].image);
            Sprite SampleSprite = Sprite.Create(SampleTex, new Rect(0, 0, SampleTex.width, SampleTex.height), Vector2.zero, 1);
            ui = new UIElement(Game.DIALOG);
            ui.SetLocation(UIScaler.GetHCenter()-3, 8, 6, 6);
            ui.SetButton(delegate { UpdateEditorTransparency(0.2f); });
            ui.SetImage(SampleSprite);
            if(game.editorTransparency == 0.2f)
                new UIElementBorder(ui, Color.white);

            SampleTex = ContentData.FileToTexture(game.cd.images[IMG_MEDIUM_EDITOR_TRANSPARENCY].image);
            SampleSprite = Sprite.Create(SampleTex, new Rect(0, 0, SampleTex.width, SampleTex.height), Vector2.zero, 1);
            ui = new UIElement(Game.DIALOG);
            ui.SetLocation(UIScaler.GetHCenter() - 3, 15, 6, 6);
            ui.SetButton(delegate { UpdateEditorTransparency(0.3f); });
            ui.SetImage(SampleSprite);
            if (game.editorTransparency == 0.3f)
                new UIElementBorder(ui, Color.white);
            
            SampleTex = ContentData.FileToTexture(game.cd.images[IMG_HIGH_EDITOR_TRANSPARENCY].image);
            SampleSprite = Sprite.Create(SampleTex, new Rect(0, 0, SampleTex.width, SampleTex.height), Vector2.zero, 1);
            ui = new UIElement(Game.DIALOG);
            ui.SetLocation(UIScaler.GetHCenter() - 3, 22, 6, 6);
            ui.SetButton(delegate { UpdateEditorTransparency(0.4f); });
            ui.SetImage(SampleSprite);
            if (game.editorTransparency == 0.4f)
                new UIElementBorder(ui, Color.white);

        }

        private void CreateAudioElements()
        {
            UIElement ui = new UIElement();
            ui.SetLocation((0.75f * UIScaler.GetWidthUnits()) - 4, 8, 10, 2);
            ui.SetText(MUSIC);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());

            float mVolume;
            string vSet = game.config.data.Get("UserConfig", "music");
            float.TryParse(vSet, out mVolume);
            if (vSet.Length == 0) mVolume = 1;

            ui = new UIElement();
            ui.SetLocation((0.75f * UIScaler.GetWidthUnits()) - 6, 11, 14, 2);
            ui.SetBGColor(Color.clear);
            new UIElementBorder(ui);

            GameObject musicSlideObj = new GameObject("musicSlide");
            musicSlideObj.tag = Game.DIALOG;
            musicSlideObj.transform.SetParent(game.uICanvas.transform);
            musicSlide = musicSlideObj.AddComponent<Slider>();
            RectTransform musicSlideRect = musicSlideObj.GetComponent<RectTransform>();
            musicSlideRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 11 * UIScaler.GetPixelsPerUnit(), 2 * UIScaler.GetPixelsPerUnit());
            musicSlideRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, ((0.75f * UIScaler.GetWidthUnits()) - 6) * UIScaler.GetPixelsPerUnit(), 14 * UIScaler.GetPixelsPerUnit());
            musicSlide.onValueChanged.AddListener(delegate { UpdateMusic(); });

            GameObject musicFill = new GameObject("musicfill");
            musicFill.tag = Game.DIALOG;
            musicFill.transform.SetParent(musicSlideObj.transform);
            musicFill.AddComponent<Image>();
            musicFill.GetComponent<Image>().color = Color.white;
            musicSlide.fillRect = musicFill.GetComponent<RectTransform>();
            musicSlide.fillRect.offsetMin = Vector2.zero;
            musicSlide.fillRect.offsetMax = Vector2.zero;

            // Double slide is a hack because I can't get a click in the space to work otherwise
            GameObject musicSlideObjRev = new GameObject("musicSlideRev");
            musicSlideObjRev.tag = Game.DIALOG;
            musicSlideObjRev.transform.SetParent(game.uICanvas.transform);
            musicSlideRev = musicSlideObjRev.AddComponent<Slider>();
            RectTransform musicSlideRectRev = musicSlideObjRev.GetComponent<RectTransform>();
            musicSlideRectRev.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 11 * UIScaler.GetPixelsPerUnit(), 2 * UIScaler.GetPixelsPerUnit());
            musicSlideRectRev.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, ((0.75f * UIScaler.GetWidthUnits()) - 6) * UIScaler.GetPixelsPerUnit(), 14 * UIScaler.GetPixelsPerUnit());
            musicSlideRev.onValueChanged.AddListener(delegate { UpdateMusicRev(); });
            musicSlideRev.direction = Slider.Direction.RightToLeft;

            GameObject musicFillRev = new GameObject("musicfillrev");
            musicFillRev.tag = Game.DIALOG;
            musicFillRev.transform.SetParent(musicSlideObjRev.transform);
            musicFillRev.AddComponent<Image>();
            musicFillRev.GetComponent<Image>().color = Color.clear;
            musicSlideRev.fillRect = musicFillRev.GetComponent<RectTransform>();
            musicSlideRev.fillRect.offsetMin = Vector2.zero;
            musicSlideRev.fillRect.offsetMax = Vector2.zero;


            musicSlide.value = mVolume;
            musicSlideRev.value = 1 - mVolume;

            ui = new UIElement();
            ui.SetLocation((0.75f * UIScaler.GetWidthUnits()) - 4, 14, 10, 2);
            ui.SetText(EFFECTS);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());

            float eVolume;
            vSet = game.config.data.Get("UserConfig", "effects");
            float.TryParse(vSet, out eVolume);
            if (vSet.Length == 0) eVolume = 1;

            ui = new UIElement();
            ui.SetLocation((0.75f * UIScaler.GetWidthUnits()) - 6, 17, 14, 2);
            ui.SetBGColor(Color.clear);
            new UIElementBorder(ui);

            GameObject effectSlideObj = new GameObject("effectSlide");
            effectSlideObj.tag = Game.DIALOG;
            effectSlideObj.transform.SetParent(game.uICanvas.transform);
            effectSlide = effectSlideObj.AddComponent<Slider>();
            RectTransform effectSlideRect = effectSlideObj.GetComponent<RectTransform>();
            effectSlideRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 17 * UIScaler.GetPixelsPerUnit(), 2 * UIScaler.GetPixelsPerUnit());
            effectSlideRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, ((0.75f * UIScaler.GetWidthUnits()) - 6) * UIScaler.GetPixelsPerUnit(), 14 * UIScaler.GetPixelsPerUnit());
            effectSlide.onValueChanged.AddListener(delegate { UpdateEffects(); });
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerUp;
            entry.callback.AddListener(delegate { PlayTestSound(); });
            effectSlideObj.AddComponent<EventTrigger>().triggers.Add(entry);

            GameObject effectFill = new GameObject("effectFill");
            effectFill.tag = Game.DIALOG;
            effectFill.transform.SetParent(effectSlideObj.transform);
            effectFill.AddComponent<Image>();
            effectFill.GetComponent<Image>().color = Color.white;
            effectSlide.fillRect = effectFill.GetComponent<RectTransform>();
            effectSlide.fillRect.offsetMin = Vector2.zero;
            effectSlide.fillRect.offsetMax = Vector2.zero;

            // Double slide is a hack because I can't get a click in the space to work otherwise
            GameObject effectSlideObjRev = new GameObject("effectSlideRev");
            effectSlideObjRev.tag = Game.DIALOG;
            effectSlideObjRev.transform.SetParent(game.uICanvas.transform);
            effectSlideRev = effectSlideObjRev.AddComponent<Slider>();
            RectTransform effectSlideRectRev = effectSlideObjRev.GetComponent<RectTransform>();
            effectSlideRectRev.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 17 * UIScaler.GetPixelsPerUnit(), 2 * UIScaler.GetPixelsPerUnit());
            effectSlideRectRev.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, ((0.75f * UIScaler.GetWidthUnits()) - 6) * UIScaler.GetPixelsPerUnit(), 14 * UIScaler.GetPixelsPerUnit());
            effectSlideRev.onValueChanged.AddListener(delegate { UpdateEffectsRev(); });
            effectSlideRev.direction = Slider.Direction.RightToLeft;
            effectSlideObjRev.AddComponent<EventTrigger>().triggers.Add(entry);

            GameObject effectFillRev = new GameObject("effectFillRev");
            effectFillRev.tag = Game.DIALOG;
            effectFillRev.transform.SetParent(effectSlideObjRev.transform);
            effectFillRev.AddComponent<Image>();
            effectFillRev.GetComponent<Image>().color = Color.clear;
            effectSlideRev.fillRect = effectFillRev.GetComponent<RectTransform>();
            effectSlideRev.fillRect.offsetMin = Vector2.zero;
            effectSlideRev.fillRect.offsetMax = Vector2.zero;

            effectSlide.value = eVolume;
            effectSlideRev.value = 1 - eVolume;
        }


        /// <summary>
        /// Method to create language UI elements in the screen
        /// </summary>
        /// <param name="game">current game</param>
        private void CreateLanguageElements()
        {
            // Select langauge text
            UIElement ui = new UIElement();
            ui.SetLocation((0.25f * UIScaler.GetWidthUnits()) - 11, 2, 18, 2);
            ui.SetText(CHOOSE_LANG);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());

            // The list of languages is determined by FFG languages for MoM
            // In D2E there is an additional language
            // It can change in future

            string[] langs = "English,Spanish,French,German,Italian,Portuguese,Polish,Russian,Chinese,Korean,Czech".Split(','); // Japanese removed to fit into screen
            // For now, the languages below are available.
            HashSet<string> enabled_langs = new HashSet<string>("English,Spanish,French,Italian,German,Portuguese,Polish,Russian,Chinese,Korean".Split(','));

            //The first button in the list of buttons should start in this vertical coordinate
            float verticalStart = UIScaler.GetVCenter(-2f) - langs.Length;

            for (int i = 0; i < langs.Length; i++)
            {
                int position = i + 1;
                // Need current index in order to delegate not point to loop for variable
                string currentLanguage = langs[i];

                ui = new UIElement();
                ui.SetLocation((0.25f * UIScaler.GetWidthUnits()) - 6, verticalStart + (2f * position), 8, 1.8f);
                if (!enabled_langs.Contains(currentLanguage))
                {
                    ui.SetText(currentLanguage, Color.red);
                    new UIElementBorder(ui, Color.red);
                }
                else
                {
                    ui.SetButton(delegate { SelectLang(currentLanguage); });
                    if (currentLanguage == game.currentLang)
                    {
                        ui.SetText(currentLanguage);
                        new UIElementBorder(ui);
                    }
                    else
                    {
                        ui.SetText(currentLanguage, Color.grey);
                        new UIElementBorder(ui, Color.grey);
                    }
                }
                ui.SetFontSize(UIScaler.GetMediumFont());
            }
        }

        private void UpdateEditorTransparency(float alpha)
        {
            game.config.data.Add("UserConfig", "editorTransparency", alpha.ToString());
            game.config.Save();
            game.editorTransparency = alpha;

            new OptionsScreen();
        }


        private void UpdateMusic()
        {
            musicSlideRev.value = 1 - musicSlide.value;
            game.config.data.Add("UserConfig", "music", musicSlide.value.ToString());
            game.config.Save();
            game.audioControl.audioSource.volume = musicSlide.value;
            game.audioControl.musicVolume = musicSlide.value;
        }

        private void UpdateMusicRev()
        {
            musicSlide.value = 1 - musicSlideRev.value;
            game.config.data.Add("UserConfig", "music", musicSlide.value.ToString());
            game.config.Save();
            game.audioControl.audioSource.volume = musicSlide.value;
            game.audioControl.musicVolume = musicSlide.value;
        }

        private void UpdateEffects()
        {
            effectSlideRev.value = 1 - effectSlide.value;
            game.config.data.Add("UserConfig", "effects", effectSlide.value.ToString());
            game.config.Save();
            game.audioControl.effectVolume = effectSlide.value;
        }

        private void UpdateEffectsRev()
        {
            effectSlide.value = 1 - effectSlideRev.value;
            game.config.data.Add("UserConfig", "effects", effectSlide.value.ToString());
            game.config.Save();
            game.audioControl.effectVolume = effectSlide.value;
        }

        private void PlayTestSound()
        {
            game.audioControl.PlayTest();
        }

        /// <summary>
        /// Select current language to specified
        /// </summary>
        /// <param name="langName"></param>
        private void SelectLang(string lang)
        {
            // Set newn lang in UI...
            string newLang = lang;

            // ... and in configuration
            game.config.data.Add("UserConfig", "currentLang", newLang);
            game.config.Save();
            game.currentLang = newLang;
            LocalizationRead.changeCurrentLangTo(newLang);
            ValkyrieDebug.Log("new current language stablished:" + newLang + Environment.NewLine);

            new OptionsScreen();

            // clear list of local quests to make sure we take the latest changes
            Game.Get().questsList.UnloadLocalQuests();
        }
    }
}
