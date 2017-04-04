using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

namespace Assets.Scripts.UI.Screens
{
    // Class for content (expansions) selection page
    public class ContentSelectScreen
    {
        private StringKey SELECT_EXPANSION = new StringKey("val","SELECT_EXPANSION");

        public Game game;
        // List of expansions selected by ID
        public List<string> selected;

        public Dictionary<string, List<TextButton>> buttons;

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
            DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 2), SELECT_EXPANSION);
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            db.SetFont(game.gameType.GetHeaderFont());

            // Start here
            float offset = 4.5f;
            TextButton tb = null;
            bool left = true;
            // Note this is currently unordered
            foreach (PackTypeData type in game.cd.packTypes.Values)
            {
                // Create a sprite with the category image
                Texture2D tex = ContentData.FileToTexture(type.image);
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1);

                string typeId = type.sectionName.Substring("PackType".Length);

                if (left)
                {
                    tb = new TextButton(new Vector2(2f, offset), new Vector2(6, 6), StringKey.NULL, delegate { DrawList(typeId); });
                }
                else
                {
                    tb = new TextButton(new Vector2(UIScaler.GetWidthUnits() - 9, offset), new Vector2(6, 6), StringKey.NULL, delegate { DrawList(typeId); });
                }
                tb.background.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
                tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;

                if (left)
                {
                    tb = new TextButton(new Vector2(8, offset + 1.5f), new Vector2(UIScaler.GetWidthUnits() - 19, 3), StringKey.NULL, delegate { DrawList(typeId); }, Color.clear);
                }
                else
                {
                    tb = new TextButton(new Vector2(10, offset + 1.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 3), StringKey.NULL, delegate { DrawList(typeId); }, Color.clear);
                }
                tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;

                if (left)
                {
                    tb = new TextButton(new Vector2(9, offset + 1.5f), new Vector2(UIScaler.GetWidthUnits() - 19, 3), type.name, delegate { DrawList(typeId); }, Color.black);
                }
                else
                {
                    tb = new TextButton(new Vector2(11, offset + 1.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 3), type.name, delegate { DrawList(typeId); }, Color.black);
                }
                tb.setColor(Color.clear);
                tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
                tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
                //tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
                tb.SetFont(game.gameType.GetHeaderFont());
                tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;

                left = !left;
                offset += 4f;
            }

            // Button for back to main menu
            tb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), CommonStringKeys.BACK, delegate { Destroyer.MainMenu(); });
            tb.SetFont(game.gameType.GetHeaderFont());
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
            DialogBox db = new DialogBox(
                new Vector2(2, 1), 
                new Vector2(UIScaler.GetWidthUnits() - 4, 2), 
                SELECT_EXPANSION
                );
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            db.SetFont(game.gameType.GetHeaderFont());

            db = new DialogBox(new Vector2(1, 4f), new Vector2(UIScaler.GetWidthUnits()-2f, 22f), StringKey.NULL);
            db.AddBorder();
            db.background.AddComponent<UnityEngine.UI.Mask>();
            UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

            GameObject scrollArea = new GameObject("scroll");
            RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
            scrollArea.transform.parent = db.background.transform;
            scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 1);
            scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, (UIScaler.GetWidthUnits()-3f) * UIScaler.GetPixelsPerUnit());

            GameObject scrollBarObj = new GameObject("scrollbar");
            scrollBarObj.transform.parent = db.background.transform;
            RectTransform scrollBarRect = scrollBarObj.AddComponent<RectTransform>();
            scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 22 * UIScaler.GetPixelsPerUnit());
            scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (UIScaler.GetWidthUnits() - 3f) * UIScaler.GetPixelsPerUnit(), 1 * UIScaler.GetPixelsPerUnit());
            UnityEngine.UI.Scrollbar scrollBar = scrollBarObj.AddComponent<UnityEngine.UI.Scrollbar>();
            scrollBar.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
            scrollRect.verticalScrollbar = scrollBar;

            GameObject scrollBarHandle = new GameObject("scrollbarhandle");
            scrollBarHandle.transform.parent = scrollBarObj.transform;
            //RectTransform scrollBarHandleRect = scrollBarHandle.AddComponent<RectTransform>();
            scrollBarHandle.AddComponent<UnityEngine.UI.Image>();
            scrollBarHandle.GetComponent<UnityEngine.UI.Image>().color = new Color(0.7f, 0.7f, 0.7f);
            scrollBar.handleRect = scrollBarHandle.GetComponent<RectTransform>();
            scrollBar.handleRect.offsetMin = Vector2.zero;
            scrollBar.handleRect.offsetMax = Vector2.zero;

            scrollRect.content = scrollInnerRect;
            scrollRect.horizontal = false;

            buttons = new Dictionary<string, List<TextButton>>();
            // Start here
            float offset = 4.5f;
            TextButton tb = null;
            bool left = true;
            // Note this is currently unordered
            foreach (ContentData.ContentPack cp in game.cd.allPacks)
            {
                // If the id is "" this is base content and can be ignored
                if (cp.id.Length > 0 && cp.type.Equals(type))
                {
                    string id = cp.id;
                    buttons.Add(id, new List<TextButton>());
                    Color bgColor = Color.white;
                    if (!selected.Contains(id))
                    {
                        bgColor = new Color(0.3f, 0.3f, 0.3f);
                    }

                    // Create a sprite with the pack's image
                    Texture2D tex = ContentData.FileToTexture(cp.image);
                    Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1);

                    if (left)
                    {
                        tb = new TextButton(new Vector2(2f, offset), new Vector2(6, 6), StringKey.NULL, delegate { Select(id); });
                    }
                    else
                    {
                        tb = new TextButton(new Vector2(UIScaler.GetWidthUnits() - 9, offset), new Vector2(6, 6), StringKey.NULL, delegate { Select(id); });
                    }
                    tb.background.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
                    tb.background.transform.parent = scrollArea.transform;
                    tb.background.GetComponent<UnityEngine.UI.Image>().color = bgColor;
                    buttons[id].Add(tb);

                    if (left)
                    {
                        tb = new TextButton(new Vector2(8, offset + 1.5f), new Vector2(UIScaler.GetWidthUnits() - 19, 3), new StringKey("val", "INDENT", new StringKey(game.cd.GetContentName(id),false)), delegate { Select(id); }, Color.clear);
                    }
                    else
                    {
                        tb = new TextButton(new Vector2(10, offset + 1.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 3), new StringKey("val", "INDENT", new StringKey(game.cd.GetContentName(id),false)), delegate { Select(id); }, Color.clear);
                    }
                    tb.background.GetComponent<UnityEngine.UI.Image>().color = bgColor;
                    tb.background.transform.parent = scrollArea.transform;
                    buttons[id].Add(tb);

                    if (left)
                    {
                        tb = new TextButton(new Vector2(9, offset + 1.5f), new Vector2(UIScaler.GetWidthUnits() - 19, 3), new StringKey(game.cd.GetContentName(id), false), delegate { Select(id); }, Color.black);
                    }
                    else
                    {
                        tb = new TextButton(new Vector2(11, offset + 1.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 3), new StringKey(game.cd.GetContentName(id), false), delegate { Select(id); }, Color.black);
                    }
                    tb.setColor(Color.clear);
                    tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
                    tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                    tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
                    //tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
                    tb.SetFont(game.gameType.GetHeaderFont());
                    tb.background.GetComponent<UnityEngine.UI.Image>().color = bgColor;
                    tb.background.transform.parent = scrollArea.transform;
                    buttons[id].Add(tb);

                    left = !left;
                    offset += 4f;
                }
            }
            scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (offset - 2.5f) * UIScaler.GetPixelsPerUnit());

            // Button for back to main menu
            if (game.cd.packTypes.Count > 1)
            {
                tb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), CommonStringKeys.BACK, delegate { DrawTypeList(); }, Color.red);
            }
            else
            {
                tb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), CommonStringKeys.BACK, delegate { Destroyer.MainMenu(); }, Color.red);
            }
            tb.SetFont(game.gameType.GetHeaderFont());
        }
        
        public void Update()
        {
            foreach (KeyValuePair<string, List<TextButton>> kv in buttons)
            {
                foreach (TextButton tb in kv.Value)
                {
                    if (game.config.data.Get(game.gameType.TypeName() + "Packs") != null
                        && game.config.data.Get(game.gameType.TypeName() + "Packs").ContainsKey(kv.Key))
                    {
                        tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
                    }
                    else
                    {
                        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.4f, 0.4f, 0.4f);
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
