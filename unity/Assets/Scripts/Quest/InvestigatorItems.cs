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
            QuestData.QItem item = kv.Value as QuestData.QItem;
            if (item != null && item.starting)
            {
                game.quest.items.Add(game.quest.itemSelect[kv.Key]);
            }
        }

        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
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
