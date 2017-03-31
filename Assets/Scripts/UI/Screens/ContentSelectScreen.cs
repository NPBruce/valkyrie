using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.UI.Screens
{
    // Class for content (expansions) selection page
    public class ContentSelectScreen
    {
        public Game game;
        // List of expansions selected by ID
        public List<string> selected;

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

            // Draw to the screen
            Update();
        }

        // Draw the expansions on the screen, highlighting those that are enabled
        public void Update()
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
            DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 2), "Select Expansion Content");
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            db.SetFont(game.gameType.GetHeaderFont());

            db = new DialogBox(new Vector2(1, 4f), new Vector2(UIScaler.GetWidthUnits()-2f, 22f), "");
            db.AddBorder();
            db.background.AddComponent<UnityEngine.UI.Mask>();
            UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

            GameObject scrollArea = new GameObject("scroll");
            RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
            scrollArea.transform.parent = db.background.transform;
            scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 1);
            scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, (UIScaler.GetWidthUnits()-3f) * UIScaler.GetPixelsPerUnit());

            scrollRect.content = scrollInnerRect;
            scrollRect.horizontal = false;

            TextButton tb;
            // Start here
            float offset = 4;
            // Note this is currently unordered
            foreach (ContentData.ContentPack cp in game.cd.allPacks)
            {
                // If the id is "" this is base content and can be ignored
                if (cp.id.Length > 0)
                {
                    string id = cp.id;

                    tb = new TextButton(new Vector2(2, offset), new Vector2(UIScaler.GetWidthUnits()-5f, 6.5f), "", delegate { Select(id); });
                    tb.background.transform.parent = scrollArea.transform;

                    // Create a sprite with the pack's image
                    Texture2D tex = ContentData.FileToTexture(cp.image);
                    Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1);

                    tb = new TextButton(new Vector2(2.5, offset + 0.5f), new Vector2(6, 6), "", delegate { Select(id); });
                    tb.background.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
                    tb.background.transform.parent = scrollArea.transform;

                    // Draw normally if selected, dark if not
                    if (selected.Contains(id))
                    {
                        tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
                    }
                    else
                    {
                        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.3f, 0.3f, 0.3f);
                    }

                    tb = new TextButton(new Vector2(9, offset + 1.75f), new Vector2(UIScaler.GetWidthUnits()-12.5f, 4), game.cd.GetContentName(id), delegate { Select(id); }, Color.black);
                    tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                    tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
                    tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
                    tb.background.transform.parent = scrollArea.transform;

                    offset += 7.5;
                }
            }
            scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (offset - 4) * UIScaler.GetPixelsPerUnit());

            // Button for back to main menu
            tb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Back", delegate { Destroyer.MainMenu(); }, Color.red);
            tb.SetFont(game.gameType.GetHeaderFont());
        }

        // set a pack as selected by id
        public void Select(string id)
        {
            game.config.data.Add(game.gameType.TypeName() + "Packs", id, "");
            game.config.Save();
            Update();
        }

        // set a pack as unselected by id
        public void Unselect(string id)
        {
            game.config.data.Remove(game.gameType.TypeName() + "Packs", id);
            game.config.Save();
            Update();
        }
    }
}
