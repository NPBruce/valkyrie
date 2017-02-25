using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Window with starting Investigator items
public class InvestigatorItems
{
    public InvestigatorItems()
    {
        Game game = Game.Get();
        List<ItemData> items = new List<ItemData>();

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            QuestData.Item item = kv.Value as QuestData.Item;
            if (item != null)
            {
                if (game.cd.items.ContainsKey(item.itemName))
                {
                    items.Add(game.cd.items[item.itemName]);
                }
                else
                {
                    List<ItemData> candidates = new List<ItemData>();
                    foreach (KeyValuePair<string, ItemData> id in game.cd.items)
                    {
                        bool valid = true;
                        foreach (string trait in item.traits)
                        {
                            if (!id.Value.ContainsTrait(trait))
                            {
                                valid = false;
                            }
                        }
                        if (valid)
                        {
                            candidates.Add(id.Value);
                        }
                    }
                    if (candidates.Count > 0)
                    {
                        items.Add(candidates[Random.Range(0, candidates.Count)]);
                    }
                }
            }
        }

        foreach (Quest.Hero h in game.quest.heroes)
        {
            if (h.heroData != null)
            {
                if (game.cd.items.ContainsKey(h.heroData.item))
                {
                    items.Add(game.cd.items[h.heroData.item]);
                }
            }
        }
        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        DialogBox db = new DialogBox(new Vector2(10, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 2), "Starting Items");
        db.SetFont(Game.Get().gameType.GetHeaderFont());
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();

        int y = 0;
        int x = 0;
        foreach (ItemData item in items)
        {
            Texture2D tex = ContentData.FileToTexture(item.image);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1);

            db = new DialogBox(new Vector2(UIScaler.GetHCenter(8f * x) - 19, 5f + (8f * y)), new Vector2(6, 6), "");
            db.background.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
            db.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            x++;
            if (x > 4)
            {
                x = 0;
                y++;
            }
        }
        TextButton tb = new TextButton(new Vector2(UIScaler.GetHCenter(-6f), 27f), new Vector2(12, 2), "Finished", delegate { game.QuestStartEvent(); });
        tb.SetFont(game.gameType.GetHeaderFont());
    }
}
