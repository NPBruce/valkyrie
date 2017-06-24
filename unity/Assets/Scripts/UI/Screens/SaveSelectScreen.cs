using Assets.Scripts.Content;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Screens
{
    class SaveSelectScreen
    {
        public bool save;
        List<SaveManager.SaveData> saves;
        Game game = Game.Get();
        private readonly StringKey SELECT_SAVE = new StringKey("val", "SELECT_SAVE");
        private readonly StringKey SAVE = new StringKey("val", "SAVE");
        private readonly StringKey AUTOSAVE = new StringKey("val", "AUTOSAVE");

        public SaveSelectScreen(bool performSave = false, Texture2D s = null)
        {
            save = performSave;
            if (!save)
            {
                // This will destroy all, because we shouldn't have anything left at the main menu
                Destroyer.Destroy();
            }

            saves = SaveManager.GetSaves();

            // Create elements for the screen
            CreateElements();
        }

        private void CreateElements()
        {
            float offset = 1f;
            UIElement ui = null;
            if (save)
            {
                offset += 2;
                ui = new UIElement();
                ui.SetLocation(UIScaler.GetHCenter(-21), offset, 42, 24);
                offset += 1;
            }
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-10), offset, 20, 3);
            ui.SetText(SELECT_SAVE);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetLargeFont());

            offset += 4;
            for (int i = 0; i < saves.Count; i++)
            {
                int tmp = i;
                if (saves[i].valid)
                {
                    string name = SAVE.Translate() + " " + i;
                    if (i == 0)
                    {
                        if (save) continue;
                        name = AUTOSAVE.Translate();
                    }

                    ui = new UIElement();
                    ui.SetLocation(UIScaler.GetHCenter(-20), offset, 40, 4);
                    ui.SetButton(delegate { Select(tmp); });
                    new UIElementBorder(ui);

                    if (saves[i].image != null)
                    {
                        Sprite imgSprite = Sprite.Create(saves[i].image, new Rect(0, 0, saves[i].image.width, saves[i].image.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect);
                        ui = new UIElement();
                        ui.SetLocation(UIScaler.GetHCenter(-20), offset, 4f * (float)saves[i].image.width / (float)saves[i].image.height, 4);
                        ui.SetButton(delegate { Select(tmp); });
                        ui.SetImage(imgSprite);
                        new UIElementBorder(ui);
                    }

                    ui = new UIElement();
                    ui.SetLocation(UIScaler.GetHCenter(-12), offset + 0.5f, 10, 2);
                    ui.SetText(name);
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetFontSize(UIScaler.GetMediumFont());
                    ui.SetButton(delegate { Select(tmp); });

                    ui = new UIElement();
                    ui.SetLocation(UIScaler.GetHCenter(-1), offset + 0.5f, 20, 2);
                    ui.SetText(saves[i].saveTime.ToString());
                    ui.SetTextAlignment(TextAnchor.MiddleRight);
                    ui.SetFontSize(UIScaler.GetMediumFont());
                    ui.SetButton(delegate { Select(tmp); });

                    ui = new UIElement();
                    ui.SetLocation(UIScaler.GetHCenter(-12), offset + 2.6f, 31, 1);
                    ui.SetText(saves[i].quest);
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetButton(delegate { Select(tmp); });
                }
                else
                {
                    ui = new UIElement();
                    ui.SetLocation(UIScaler.GetHCenter(-20), offset, 40, 4);
                    ui.SetButton(delegate { Select(tmp); });
                    new UIElementBorder(ui, Color.gray);
                }
                offset += 5;
            }

            if (save)
            {
                // Button for cancel
                ui = new UIElement();
                ui.SetLocation(UIScaler.GetHCenter(-4), 24, 8, 2);
                ui.SetText(CommonStringKeys.CANCEL);
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetButton(Destroyer.Dialog);
                new UIElementBorder(ui);
            }
            else
            {
                // Button for back to main menu
                ui = new UIElement();
                ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
                ui.SetText(CommonStringKeys.BACK, Color.red);
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetButton(Destroyer.MainMenu);
                new UIElementBorder(ui, Color.red);
            }
            ui.SetFont(game.gameType.GetHeaderFont());
        }

        public void Select(int num)
        {
            if (save)
            {
                SaveManager.SaveWithScreen(num);
                Destroyer.Dialog();
            }
            else
            {
                if (saves[num].valid)
                {
                    SaveManager.Load(num);
                }
            }
        }
    }
}
