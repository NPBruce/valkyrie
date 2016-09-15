using UnityEngine;
using System.Collections.Generic;
using System.IO;

class ContentSelect
{
    public Game game;
    public List<string> selected;

    public ContentSelect()
    {
        Destroyer.Destroy();
        game = Game.Get();

        // Find any content packs at the location
        game.cd = new ContentData(game.gameType.DataDirectory());
        // Check if we found anything
        if (game.cd.allPacks.Count == 0)
        {
            Debug.Log("Error: Failed to find any content packs, please check that you have them present in: " + game.gameType.DataDirectory() + System.Environment.NewLine);
            Application.Quit();
        }

        Update();
    }

    public void Update()
    {
        Destroyer.Dialog();
        selected = new List<string>();
        Dictionary<string, string> setPacks = game.config.data.Get(game.gameType.TypeName() + "Packs");
        if (setPacks != null)
        {
            foreach (KeyValuePair<string, string> kv in setPacks)
            {
                selected.Add(kv.Key);
            }
        }

        // Name.  We should replace this with a banner
        DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 2), "Select Expansion Content");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();

        float x = 1;
        float y = 4;

        foreach (ContentData.ContentPack cp in game.cd.allPacks)
        {
            if (cp.id.Length > 0)
            {
                string id = cp.id;

                Texture2D tex = ContentData.FileToTexture(cp.image);
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1);

                if (selected.Contains(id))
                {
                    TextButton tb = new TextButton(new Vector2(x, y), new Vector2(6, 6), "", delegate { Unselect(id); });
                    tb.background.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
                    tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
                }
                else
                {
                    TextButton tb = new TextButton(new Vector2(x, y), new Vector2(6, 6), "", delegate { Select(id); }, new Color(0.3f, 0.3f, 0.3f));
                    tb.background.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
                    tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.3f, 0.3f, 0.3f);
                }
                x += 7;
                if (x > UIScaler.GetRight(-7))
                {
                    x = 1;
                    y += 7;
                }
            }
        }

        new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Back", delegate { Destroyer.MainMenu(); }, Color.red);
    }

    public void Select(string id)
    {
        game.config.data.Add(game.gameType.TypeName() + "Packs", id, "");
        game.config.Save();
        Update();
    }

    public void Unselect(string id)
    {
        game.config.data.Remove(game.gameType.TypeName() + "Packs", id);
        game.config.Save();
        Update();
    }
}
