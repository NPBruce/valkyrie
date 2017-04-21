using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

// Window with starting Investigator items
public class InvestigatorItems
{
    private readonly StringKey STARTING_ITEMS = new StringKey("val", "STARTING_ITEMS");

    public InvestigatorItems()
    {
        Game game = Game.Get();

        // Items from heroes
        foreach (Quest.Hero h in game.quest.heroes)
        {
            if (h.heroData != null)
            {
                if (game.cd.items.ContainsKey(h.heroData.item))
                {
                    game.quest.items.Add(h.heroData.item);
                }
            }
        }

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            QuestData.StartingItem item = kv.Value as QuestData.StartingItem;
            if (item != null)
            {
                // Specific items
                if (item.traits.Length == 0 && (item.itemName.Length == 1))
                {
                    if (game.cd.items.ContainsKey(item.itemName[0]))
                    {
                        game.quest.items.Add(item.itemName[0]);
                    }
                }
                // Random item
                else
                {
                    List<string> candidates = new List<string>();
                    foreach (KeyValuePair<string, ItemData> id in game.cd.items)
                    {
                        bool valid = !game.quest.items.Contains(id.Value.sectionName);
                        if (id.Value.unique)
                        {
                            valid = false;
                        }
                        foreach (string trait in item.traits)
                        {
                            if (!id.Value.ContainsTrait(trait) && trait.Length > 0)
                            {
                                valid = false;
                            }
                            foreach (string s in item.itemName)
                            {
                                if (s.Equals(id.Value.sectionName))
                                {
                                    valid = false;
                                }
                            }
                        }
                        if (valid)
                        {
                            candidates.Add(id.Value.sectionName);
                        }
                    }
                    if (candidates.Count > 0)
                    {
                        game.quest.items.Add(candidates[Random.Range(0, candidates.Count)]);
                    }
                }
            }
        }

        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        DialogBox db = new DialogBox(new Vector2(10, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 2), STARTING_ITEMS);
        db.SetFont(Game.Get().gameType.GetHeaderFont());
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();

        int y = 0;
        int x = 0;
        foreach (string item in game.quest.items)
        {
            Texture2D tex = ContentData.FileToTexture(game.cd.items[item].image);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1);

            db = new DialogBox(new Vector2(UIScaler.GetHCenter(8f * x) - 19, 5f + (9f * y)), new Vector2(6, 6), StringKey.NULL);
            db.background.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
            db.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;

            db = new DialogBox(new Vector2(UIScaler.GetHCenter(8f * x) - 20, 11f + (9f * y)), new Vector2(8, 1), game.cd.items[item].name);

            x++;
            if (x > 4)
            {
                x = 0;
                y++;
            }
        }
        TextButton tb = new TextButton(new Vector2(UIScaler.GetHCenter(-6f), 27f), new Vector2(12, 2), 
            CommonStringKeys.FINISHED, delegate { game.QuestStartEvent(); });
        tb.SetFont(game.gameType.GetHeaderFont());
    }
}
