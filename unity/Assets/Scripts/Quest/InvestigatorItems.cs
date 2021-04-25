using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

// Window with starting Investigator items
public class InvestigatorItems
{
    private const int LARGE_FONT_LIMIT = 16;

    private readonly StringKey STARTING_ITEMS = new StringKey("val", "STARTING_ITEMS");

    public InvestigatorItems()
    {
        Game game = Game.Get();

        // Items from heroes
        foreach (Quest.Hero h in game.CurrentQuest.heroes)
        {
            if (h.heroData != null)
            {
                if (game.cd.ContainsKey<ItemData>(h.heroData.item))
                {
                    game.CurrentQuest.items.Add(h.heroData.item);
                }
            }
        }

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.CurrentQuest.qd.components)
        {
            QuestData.QItem item = kv.Value as QuestData.QItem;
            if (item != null && item.starting && game.CurrentQuest.itemSelect.ContainsKey(kv.Key)
                && item.tests != null && game.CurrentQuest.vars.Test(item.tests))
            {
                game.CurrentQuest.items.Add(game.CurrentQuest.itemSelect[kv.Key]);
                if (item.inspect.Length > 0)
                {
                    if (game.CurrentQuest.itemInspect.ContainsKey(game.CurrentQuest.itemSelect[kv.Key]))
                    {
                        game.CurrentQuest.itemInspect.Remove(game.CurrentQuest.itemSelect[kv.Key]);
                    }
                    game.CurrentQuest.itemInspect.Add(game.CurrentQuest.itemSelect[kv.Key], item.inspect);
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

        foreach (string item in game.CurrentQuest.items)
        {
            // Ignore "ItemX", find next capital letter
            int charIndex = 5;
            while (charIndex < item.Length - 1)
            {
                if (System.Char.IsUpper(item[charIndex++])) break;
            }
            string typeString = item.Substring(0, charIndex);
            string translationString = game.cd.Get<ItemData>(item).name.Translate();

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
                var itemData = game.cd.Get<ItemData>(item);
                Texture2D tex = ContentData.FileToTexture(itemData.image);
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
                ui.SetText(itemData.name);
                ui.SetFontSize(ui.GetText().Length > LARGE_FONT_LIMIT ? UIScaler.GetSmallerFont() : UIScaler.GetSmallFont());
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
