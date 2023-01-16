using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Content;
using UnityEngine;
using ValkyrieTools;

namespace Assets.Scripts.UI.Screens
{
    // Class for content (expansions) selection page
    public class ContentSelectScreen
    {
        private const int LARGE_FONT_LIMIT = 32;

        private StringKey SELECT_EXPANSION = new StringKey("val", "SELECT_EXPANSION");
        private static readonly StringKey SELECT_EXPANSION_CUSTOM = new StringKey("val", "CONTENTPACK_CATEGORY_CUSTOM");
        private static readonly StringKey CONTENTPACK_DOWNLOAD = new StringKey("val", "CONTENTPACK_DOWNLOAD");

        public Game game;

        // List of expansions selected by ID
        public List<string> selected;

        public Dictionary<string, List<UIElement>> buttons;
        public Dictionary<string, UIElement> languageButtons;
        private static readonly StringKey DEFAULT_LANGUAGE_KEY = CommonStringKeys.BASE;

        // Create page
        public ContentSelectScreen()
        {
            // Clean everything up
            Destroyer.Destroy();
            game = Game.Get();

            if (game.cd.Count<PackTypeData>() > 1)
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
            DrawTypeListHeader();

            // Start here
            float offset = 4.5f;
            bool left = true;

            string typeIdCustom = GetCustomCategory();

            // Note this is currently unordered
            foreach (PackTypeData type in game.cd.Values<PackTypeData>())
            {
                string typeId = type.sectionName.Substring("PackType".Length);

                //skip custom category if it was added for some reason
                if (typeId.Equals(typeIdCustom, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string typeName = type.name != null ? type.name.ToString() : null;

                CreatePackTypeCategory(ref offset, ref left, typeId, typeName, type.image);
            }

            CreatePackTypeCategory(ref offset, ref left, typeIdCustom, string.Empty, string.Empty);

            DrawBackButtonTypeListPage();

            DrawDownloadButtonTypeListPage();
        }

        private void DrawTypeListHeader()
        {
            UIElement ui = new UIElement();
            ui.SetLocation(2, 1, UIScaler.GetWidthUnits() - 4, 2);
            ui.SetText(SELECT_EXPANSION);
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
        }

        private string GetCustomCategory()
        {
            return Game.Get().gameType.TypeName()  + ValkyrieConstants.customCategoryName;
        }

        private static void DrawBackButtonTypeListPage()
        {
            // Button for back to main menu
            UIElement uiBack = new UIElement();
            uiBack.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
            uiBack.SetText(CommonStringKeys.BACK);
            uiBack.SetFont(Game.Get().gameType.GetHeaderFont());
            uiBack.SetFontSize(UIScaler.GetMediumFont());
            uiBack.SetButton(Quit);
            new UIElementBorder(uiBack);
        }

        private void DrawDownloadButtonTypeListPage()
        {
            float text_width = 0f;
            UIElement uiDownloadCustomContentPack = null;
            uiDownloadCustomContentPack = new UIElement();
            uiDownloadCustomContentPack.SetText(CONTENTPACK_DOWNLOAD);
            uiDownloadCustomContentPack.SetLocation(UIScaler.GetRight(-8.5f), UIScaler.GetBottom(-3), 8, 2);
            uiDownloadCustomContentPack.SetFont(game.gameType.GetHeaderFont());
            uiDownloadCustomContentPack.SetFontSize(UIScaler.GetSmallFont());
            uiDownloadCustomContentPack.SetTextAlignment(TextAnchor.MiddleCenter);
            uiDownloadCustomContentPack.SetButton(ContentSelectDownload);
            new UIElementBorder(uiDownloadCustomContentPack);
        }

        private void CreatePackTypeCategory(ref float offset, ref bool left, string typeId, string typeName, string image)//, PackTypeData type)
        {
            UIElement ui;
            // Create a sprite with the category image
            Texture2D tex = ContentData.FileToTexture(image);

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
            if(string.IsNullOrWhiteSpace(typeName))
            {
                ui.SetText(SELECT_EXPANSION_CUSTOM, Color.black); 
            }
            else
            {
                ui.SetText(typeName, Color.black);
            }
            
            ui.SetTextAlignment(TextAnchor.MiddleLeft);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(delegate { DrawList(typeId); });

            left = !left;
            offset += 4f;
        }


        // Draw the expansions on the screen, highlighting those that are enabled
        public void DrawList(string type = "")
        {
            // Clean up
            Destroyer.Dialog();
            // Initialise selected list
            selected = new List<string>();

            // Fetch which packs are selected from the current config (items under [Packs])
            foreach (var pack in game.config.GetPacks(game.gameType.TypeName()))
            {
                selected.Add(pack);
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
            languageButtons = new Dictionary<string, UIElement>();
            // Start here
            float offset = 0.5f;
            bool left = true;
            var packLanguages = game.config.GetPackLanguages(game.gameType.TypeName());
            // Note this is currently unordered
            var allPacks = game.cd.allPacks;
            foreach (ContentPack cp in allPacks)
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

                    int text_font_size = (int)(UIScaler.GetMediumFont() * 0.9f);

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
                    ui.SetFontSize(ui.GetText().Length > LARGE_FONT_LIMIT
                        ? UIScaler.GetSmallFont()
                        : UIScaler.GetMediumFont());
                    ui.SetFontSize(text_font_size);
                    ui.SetButton(delegate { Select(id); });
                    buttons[id].Add(ui);

                    float text_width = ui.GetStringWidth() + 0.5f;
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    if (left)
                    {
                        ui.SetLocation(8 + text_width, offset + 1.5f, UIScaler.GetWidthUnits() - 19 - text_width, 3);
                    }
                    else
                    {
                        ui.SetLocation(10 + text_width, offset + 1.5f, UIScaler.GetWidthUnits() - 20 - text_width, 3);
                    }

                    ui.SetBGColor(bgColor);
                    ui.SetText("(" + game.cd.GetContentAcronym(id) + ")", Color.black);
                    ui.SetTextAlignment(TextAnchor.MiddleLeft);
                    ui.SetFont(game.gameType.GetSymbolFont());
                    ui.SetFontSize(text_font_size);
                    ui.SetButton(delegate { Select(id); });
                    buttons[id].Add(ui);


                    packLanguages.TryGetValue(id, out string packLanguage);
                    ui = new UIElement(scrollArea.GetScrollTransform());
                    ui.SetLocation(UIScaler.GetWidthUnits() - 17, offset + 2.5f, 5, 1);
                    ui.SetBGColor(Color.black);
                    ui.SetText(string.IsNullOrWhiteSpace(packLanguage) ? DEFAULT_LANGUAGE_KEY.Translate() : packLanguage, Color.white);
                    ui.SetTextAlignment(TextAnchor.MiddleCenter);
                    ui.SetFont(game.gameType.GetSymbolFont());
                    ui.SetFontSize(UIScaler.GetSmallFont());
                    ui.SetButton(delegate { SelectLanguage(id, type); });
                    new UIElementBorder(ui, Color.black, 0.1f);
                    new UIElementBorder(ui, Color.white);
                    languageButtons.Add(id, ui);

                    left = !left;
                    offset += 4f;
                }
            }

            scrollArea.SetScrollSize(offset + 2.5f);

            ui = DrawBackButtonContentListPage();
        }

        private UIElement DrawBackButtonContentListPage()
        {
            // Button for back to main menu
            UIElement ui = new UIElement();
            ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
            ui.SetText(CommonStringKeys.BACK, Color.red);
            ui.SetFont(Game.Get().gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            new UIElementBorder(ui, Color.red);
            if (game.cd.Count<PackTypeData>() > 1)
            {
                ui.SetButton(DrawTypeList);
            }
            else
            {
                ui.SetButton(Quit);
            }

            return ui;
        }

        private void SelectLanguage(string id, string type)
        {
            UIWindowSelectionList select = new UIWindowSelectionList(
                delegate(string val) { UpdateLanguage(id, val, type); }, CommonStringKeys.CHOOSE_LANGUAGE, true);

            select.AddItem(DEFAULT_LANGUAGE_KEY);
            foreach (var s in OptionsScreen.ENABLED_LANGS)
            {
                select.AddItem(s);
            }

            select.Draw();
        }

        private void UpdateLanguage(string id, string val, string type)
        {
            if (val == DEFAULT_LANGUAGE_KEY.key)
            {
                val = "";
            }

            // Cancel button
            if (val != null)
            {
                game.config.AddPack(game.gameType.TypeName(), id, val);
            }

            DrawList(type);
        }

        public static void Quit()
        {
            GameStateManager.MainMenu();
        }

        public static void ContentSelectDownload()
        {
            ValkyrieDebug.Log("INFO: Accessing content select download screen");

            new ContentSelectDownloadScreen();
        }

        public void Update()
        {
            var packs = game.config.GetPacks(game.gameType.TypeName());
            foreach (KeyValuePair<string, List<UIElement>> kv in buttons)
            {
                foreach (UIElement ui in kv.Value)
                {
                    if (packs.Contains(kv.Key))
                    {
                        ui.SetBGColor(Color.white);
                    }
                    else
                    {
                        ui.SetBGColor(new Color(0.4f, 0.4f, 0.4f));
                    }
                }
            }

            foreach (KeyValuePair<string, UIElement> kv in languageButtons)
            {
                if (!packs.Contains(kv.Key))
                {
                    kv.Value.SetText(DEFAULT_LANGUAGE_KEY);
                }
            }
        }

        // set a pack as selected by id
        public void Select(string id)
        {
            var packs = game.config.GetPacks(game.gameType.TypeName()).ToSet();
            if (packs.Contains(id))
            {
                game.config.RemovePack(game.gameType.TypeName(), id);
            }
            else
            {
                game.config.AddPack(game.gameType.TypeName(), id);
            }

            Update();
        }
    }
}