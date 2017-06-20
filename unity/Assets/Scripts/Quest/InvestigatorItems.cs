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
            if (item != null && item.starting && game.quest.itemSelect.ContainsKey(kv.Key))
            {
                game.quest.items.Add(game.quest.itemSelect[kv.Key]);
                if (item.inspect.Length > 0)
                {
                    if (game.quest.itemInspect.ContainsKey(game.quest.itemSelect[kv.Key]))
                    {
                        game.quest.itemInspect.Remove(game.quest.itemSelect[kv.Key]);
                    }
                    game.quest.itemInspect.Add(game.quest.itemSelect[kv.Key], item.inspect);
                }
            }
        }

        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            Object.Destroy(go);

        UIElement ui = new UIElement();
        ui.SetLocation(10, 0.5f, UIScaler.GetWidthUnits() - 20, 2);
        ui.SetText(STARTING_ITEMS);
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetFont(Game.Get().gameType.GetHeaderFont());

        int y = 0;
        int x = 0;
        foreach (string item in game.quest.items)
        {
            Texture2D tex = ContentData.FileToTexture(game.cd.items[item].image);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(8f * x) - 19, 5f + (9f * y), 6, 6);
            ui.SetImage(sprite);

            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(8f * x) - 20, 11f + (9f * y), 8, 1);
            ui.SetText(game.cd.items[item].name);

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
