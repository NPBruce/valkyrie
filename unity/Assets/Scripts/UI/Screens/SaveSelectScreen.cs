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
            if (save)
            {
                offset += 2;
                new DialogBox(new Vector2(UIScaler.GetHCenter(-21), offset), new Vector2(42, 24), StringKey.NULL);
                offset += 1;
            }
            // Options screen text
            DialogBox dbTittle = new DialogBox(
                new Vector2(UIScaler.GetHCenter(-10), offset),
                new Vector2(20, 3),
                SELECT_SAVE
                );
            dbTittle.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
            dbTittle.SetFont(game.gameType.GetHeaderFont());

            offset += 4;
            TextButton tb;
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

                    tb = new TextButton(
                        new Vector2(UIScaler.GetHCenter(-20), offset),
                        new Vector2(40, 4f),
                        new StringKey(null, "", false),
                        delegate { Select(tmp); });

                    if (saves[i].image != null)
                    {
                        Sprite imgSprite = Sprite.Create(saves[i].image, new Rect(0, 0, saves[i].image.width, saves[i].image.height), Vector2.zero, 1);
                        tb = new TextButton(
                            new Vector2(UIScaler.GetHCenter(-20), offset),
                            new Vector2(4f * (float)saves[i].image.width / (float)saves[i].image.height, 4f),
                            new StringKey(null, "", false),
                            delegate { Select(tmp); });

                        tb.background.GetComponent<UnityEngine.UI.Image>().sprite = imgSprite;
                        tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
                    }

                    tb = new TextButton(
                        new Vector2(UIScaler.GetHCenter(-12), offset + 0.5f),
                        new Vector2(10, 2f),
                        new StringKey(null, name, false),
                        delegate { Select(tmp); });
                    tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
                    tb.border.Destroy();

                    tb = new TextButton(
                        new Vector2(UIScaler.GetHCenter(-1), offset + 0.5f),
                        new Vector2(20, 2f),
                        new StringKey(null, saves[i].saveTime.ToString(), false),
                        delegate { Select(tmp); });
                    tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
                    tb.border.Destroy();

                    tb = new TextButton(
                        new Vector2(UIScaler.GetHCenter(-12), offset + 2.6f),
                        new Vector2(31, 1f),
                        new StringKey(null, saves[i].quest, false),
                        delegate { Select(tmp); });
                    tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                    tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
                    tb.border.Destroy();
                }
                else
                {
                    tb = new TextButton(
                        new Vector2(UIScaler.GetHCenter(-20), offset),
                        new Vector2(40, 4f),
                        new StringKey(null, "", false),
                        delegate { Select(tmp); }, Color.gray);
                }
                offset += 5;
            }

            if (save)
            {
                // Button for cancel
                tb = new TextButton(new Vector2(UIScaler.GetHCenter(-4), 24), new Vector2(8, 2),
                    CommonStringKeys.CANCEL, delegate { Destroyer.Dialog(); });
            }
            else
            {
                // Button for back to main menu
                tb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2),
                    CommonStringKeys.BACK, delegate { Destroyer.MainMenu(); }, Color.red);
            }
            tb.SetFont(game.gameType.GetHeaderFont());
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
