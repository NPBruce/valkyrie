using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
using ValkyrieTools;

namespace Assets.Scripts.UI.Screens
{
    // Class for content (expansions) selection page
    public class ContentSelectScreen
    {
        private StringKey SELECT_EXPANSION = new StringKey("val","SELECT_EXPANSION");

        public Game game;
        // List of expansions selected by ID
        public List<string> selected;

        public Dictionary<string, List<UIElement>> buttons;

        // Create page
        public ContentSelectScreen()
        {
            // Clean everything up
            Destroyer.Destroy();
            game = Game.Get();

            // Find any content packs at the location
            game.cd = new ContentData(game.gameType.DataDirectory());
            // Check if we found anything (must have found at least base)
            if (game.cd.allPacks.Count == 0)
            {
                ValkyrieDebug.Log("Error: Failed to find any content packs, please check that you have them present in: " + game.gameType.DataDirectory() + System.Environment.NewLine);
                Application.Quit();
            }

            // load base to get types
            game.cd.LoadContentID("");
            if (game.cd.packTypes.Count > 1)
            {
                DrawTypeList();
            }
            else
            {
                DrawList("");
            }
        }

        public void DrawTypeList()
        {
            // Clean up
            Destroyer.Dialog();

            // Draw a header
            UIElement ui = new UIElement();
            ui.SetLocation(2, 1, UIScaler.GetWidthUnits() - 4, 2);
            ui.SetText(SELECT_EXPANSION);
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());

            // Start here
            float offset = 4.5f;
            bool left = true;
            // Note this is currently unordered
            foreach (PackTypeData type in game.cd.packTypes.Values)
            {
                // Create a sprite with the category image
                Texture2D tex = ContentData.FileToTexture(type.image);

                string typeId = type.sectionName.Substring("PackType".Length);

                ui = new UIElement();
                if (left)
                {
                    ui.SetLocation(2, offset, 6, 6);
                }
                else
                {
                    ui.SetLocation(UIScaler.GetWidthUnits() - 9, offset, 6, 6);
                }
                ui.SetImage(tex);
                ui.SetButton(delegate { DrawList(typeId); });
                new UIElementBorder(ui);

                ui = new UIElement();
                if (left)
                {
                    ui.SetLocation(8, offset + 1.5f, UIScaler.GetWidthUnits() - 19, 3);
                }
                else
                {
                    ui.SetLocation(10, offset + 1.5f, UIScaler.GetWidthUnits() - 20, 3);
                }
                ui.SetBGColor(Color.white);
                ui.SetButton(delegate { DrawList(typeId); });

                ui = new UIElement();
                if (left)
                {
                    ui.SetLocation(9, offset + 1.5f, UIScaler.GetWidthUnits() - 19, 3);
                }
                else
                {
                    ui.SetLocation(11, offset + 1.5f, UIScaler.GetWidthUnits() - 20, 3);
                }
                ui.SetBGColor(Color.white);
                ui.SetText(type.name, Color.black);
                ui.SetTextAlignment(TextAnchor.MiddleLeft);
                ui.SetFont(game.gameType.GetHeaderFont());
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetButton(delegate { DrawList(typeId); });

                left = !left;
                offset += 4f;
            }

            // Button for back to main menu
            ui = new UIElement();
            ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
            ui.SetText(CommonStringKeys.BACK);
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(Destroyer.MainMenu);
            new UIElementBorder(ui);
        }


        // Draw the expansions on the screen, highlighting those that are enabled
        public void DrawList(string type = "")
        {
            // Clean up
            Destroyer.Dialog();
            // Initialise selected list
            selected = new List<string>();

            // Fetch which packs are selected from the current config (items under [Packs])
            Dictionary<string, string> setPacks = game.config.data.Get(game.gameType.TypeName() + "Packs");
            if (setPacks != null)
            {
                foreach (KeyValuePair<string, string> kv in setPacks)
                {
                    // As packs are just a list, only the key is used, value is empty
                    selected.Add(kv.Key);
                }
            }

            // Draw a header
            UIElement ui = new UIElement();
            ui.SetLocation(2, 1, UIScaler.GetWidthUnits() - 4, 2);
            ui.SetText(SELECT_EXPANSION);
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());

            UIElementScrollVertical scrollArea = new UIElementScrollVertical();
            scrollArea.SetLocation(1, 4, UIScaler.GetWidthUnits() - 2, 22);
            new UIElementBorder(scrollArea);

            buttons = new Dictionary<string, List<UIElement>>();
            // Start here
            float offset = 0.5f;
            bool left = true;
            // Note this is currently unordered
            foreach (ContentData.ContentPack cp in game.cd.allPacks)
            {
                // If the id is "" this is base content and can be ignored
                if (cp.id.Length > 0 && cp.type.Equals(type))
                {
                    string id = cp.id;
                    buttons.Add(id, new List<UIElement>());
                    Color bgColor = Color.white;
                    if (!selected.Contains(id))
                    {
                        bgColor = new Color(0.3f, 0.3f, 0.3f);
                    }

                    // Create a sprite with the pack's image
                    Texture2D tex = ContentData.FileToTexture(cp.image);
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    if (left)
                    {
                        ui.SetLocation(1, offset, 6, 6);
                    }
                    else
                    {
                        ui.SetLocation(UIScaler.GetWidthUnits() - 10, offset, 6, 6);
                    }
                    ui.SetImage(tex);
                    ui.SetButton(delegate { Select(id); });
                    ui.SetBGColor(bgColor);
                    new UIElementBorder(ui);
                    buttons[id].Add(ui);


                    ui = new UIElement(scrollArea.GetScrollTransform());
                    if (left)
                    {
                        ui.SetLocation(7, offset + 1.5f, UIScaler.GetWidthUnits() - 19, 3);
                    }
                    else
                    {
                        ui.SetLocation(9, offset + 1.5f, UIScaler.GetWidthUnits() - 20, 3);
                    }
                    ui.SetBGColor(bgColor);
                    ui.SetButton(delegate { Select(id); });
                    buttons[id].Add(ui);

                    ui = new UIElement(scrollArea.GetScrollTransform());
                    if (left)
                    {
                        ui.SetLocation(8, offset + 1.5f, UIScaler.GetWidthUnits() - 19, 3);
                    }
                    else
                    {
                        ui.SetLocation(10, offset + 1.5f, UIScaler.GetWidthUnits() - 20, 3);
                    }
                    ui.SetBGColor(bgColor);
                    ui.SetText(game.cd.GetContentName(id), Color.black);
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetFont(game.gameType.GetHeaderFont());
                    ui.SetFontSize(UIScaler.GetMediumFont());
                    ui.SetButton(delegate { Select(id); });
                    buttons[id].Add(ui);

                    left = !left;
                    offset += 4f;
                }
            }
            scrollArea.SetScrollSize(offset + 2.5f);

            // Button for back to main menu
            ui = new UIElement();
            ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
            ui.SetText(CommonStringKeys.BACK, Color.red);
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            new UIElementBorder(ui, Color.red);
            if (game.cd.packTypes.Count > 1)
            {
                ui.SetButton(DrawTypeList);
            }
            else
            {
                ui.SetButton(Destroyer.MainMenu);
            }
        }
        
        public void Update()
        {
            foreach (KeyValuePair<string, List<UIElement>> kv in buttons)
            {
                foreach (UIElement ui in kv.Value)
                {
                    if (game.config.data.Get(game.gameType.TypeName() + "Packs") != null
                        && game.config.data.Get(game.gameType.TypeName() + "Packs").ContainsKey(kv.Key))
                    {
                        ui.SetBGColor(Color.white);
                    }
                    else
                    {
                        ui.SetBGColor(new Color(0.4f, 0.4f, 0.4f));
                    }
                }
            }
        }

        // set a pack as selected by id
        public void Select(string id)
        {
            if (game.config.data.Get(game.gameType.TypeName() + "Packs") != null
                && game.config.data.Get(game.gameType.TypeName() + "Packs").ContainsKey(id))
            {
                game.config.data.Remove(game.gameType.TypeName() + "Packs", id);
            }
            else
            {
                game.config.data.Add(game.gameType.TypeName() + "Packs", id, "");
            }
            game.config.Save();
            Update();
        }
    }
}
