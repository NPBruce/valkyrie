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
        DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 3), "Select Expansion Content");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();

        foreach (ContentData.ContentPack cp in game.cd.allPacks)
        {
            if (cp.id.Length > 0)
            {
                string id = cp.id;

                if (selected.Contains(id))
                {
                    new TextButton(new Vector2(5, 5), new Vector2(8, 2), cp.name, delegate { Unselect(id); });
                }
                else
                {
                    new TextButton(new Vector2(5, 5), new Vector2(8, 2), cp.name, delegate { Select(id); }, Color.gray);
                }
            }
        }

        new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Back", delegate { Destroyer.MainMenu(); }, Color.red);
    }

    public void Select(string id)
    {
        Debug.Log(id);
        game.config.data.Add(game.gameType.TypeName() + "Packs", id, "");
        game.config.Save();
        Update();
    }

    public void Unselect(string id)
    {
        Debug.Log(id);
        game.config.data.Remove(game.gameType.TypeName() + "Packs", id);
        game.config.Save();
        Update();
    }
}
