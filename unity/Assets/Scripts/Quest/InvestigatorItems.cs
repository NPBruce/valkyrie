using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

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

        SortedList<string, SortedList<string, string>> itemSort = new SortedList<string, SortedList<string, string>>();

        foreach (string item in game.quest.items)
        {
            // Ignore "ItemX", find next capital letter
            int charIndex = 5;
            while (charIndex < item.Length - 1)
            {
                if (System.Char.IsUpper(item[charIndex++])) break;
            }
            string typeString = item.Substring(0, charIndex);
            string translationString = game.cd.items[item].name.Translate();

            if (!itemSort.ContainsKey(typeString))
            {
                itemSort.Add(typeString, new SortedList<string, string>());
            }

            // Duplicate names
            while (itemSort[typeString].ContainsKey(translationString))
            {
                translationString += "D";
            }

            itemSort[typeString].Add(translationString, item);
        }

        int y = 0;
        int x = 0;
        foreach (string category in itemSort.Keys)
        {
            foreach (string item in itemSort[category].Values)
            {
                Texture2D tex = ContentData.FileToTexture(game.cd.items[item].image);
                Sprite sprite = null;
                if (tex != null)
                {
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect);
                }

                ui = new UIElement();
                ui.SetLocation(UIScaler.GetHCenter(8f * x) - 19, 5f + (9f * y), 6, 6);
                if (sprite != null)
                {
                    ui.SetImage(sprite);
                }

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
        }
        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-6f), 27f, 12, 2);
        ui.SetText(CommonStringKeys.FINISHED);
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(game.QuestStartEvent);
        new UIElementBorder(ui);
    }
}
