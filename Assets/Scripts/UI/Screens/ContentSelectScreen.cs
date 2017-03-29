using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

namespace Assets.Scripts.UI.Screens
{
    // Class for content (expansions) selection page
    public class ContentSelectScreen
    {
        private StringKey SELECT_EXPANSION = new StringKey("val","SELECT_EXPANSION");
        private StringKey BACK = new StringKey("val","BACK");
        private StringKey EMPTY = new StringKey("val","EMPTY");

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
            DialogBox db = new DialogBox(
                new Vector2(2, 1), 
                new Vector2(UIScaler.GetWidthUnits() - 4, 2), 
                SELECT_EXPANSION
                );
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            db.SetFont(game.gameType.GetHeaderFont());

            // Location for first pack
            float x = 1;
            float y = 4;

            TextButton tb;
            // Draw all packs to the screen
            // Note this is currently unordered
            foreach (ContentData.ContentPack cp in game.cd.allPacks)
            {
                // If the id is "" this is base content and can be ignored
                if (cp.id.Length > 0)
                {
                    string id = cp.id;

                    // Create a sprite with the pack's image
                    Texture2D tex = ContentData.FileToTexture(cp.image);
                    Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1);

                    // Draw normally if selected, dark if not
                    if (selected.Contains(id))
                    {
                        tb = new TextButton(new Vector2(x, y), new Vector2(6, 6), EMPTY, delegate { Unselect(id); });
                        tb.background.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
                        tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
                    }
                    else
                    {
                        tb = new TextButton(new Vector2(x, y), new Vector2(6, 6), EMPTY, delegate { Select(id); }, new Color(0.3f, 0.3f, 0.3f));
                        tb.background.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
                        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.3f, 0.3f, 0.3f);
                    }
                    // Move to next on the right
                    x += 7;

                    // Check to move down to next row
                    if (x > UIScaler.GetRight(-7))
                    {
                        x = 1;
                        y += 7;
                    }
                }
            }

            // Button for back to main menu
            tb = new TextButton(
                new Vector2(1, UIScaler.GetBottom(-3)), 
                new Vector2(8, 2), 
                BACK, 
                delegate { Destroyer.MainMenu(); }, 
                Color.red);

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
