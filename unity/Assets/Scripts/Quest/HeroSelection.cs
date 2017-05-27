using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

// Hero selection options
// This comes up when selection a hero icon to pick hero
public class HeroSelection {

    public Dictionary<string, List<UIElement>> buttons;

    // Create page of options
    public HeroSelection()
    {
        Draw();
	}

    public void Draw()
    {
        // Clean up
        Destroyer.Dialog();

        Game game = Game.Get();
        // Get all available heros
        List<string> heroList = new List<string>(game.cd.heroes.Keys);
        heroList.Sort();

        UIElementScrollVertical scrollArea = new UIElementScrollVertical(Game.HEROSELECT);
        scrollArea.SetLocation(4.5f, 4, UIScaler.GetWidthUnits() - 5.5f, 22f);

        new UIElementBorder(scrollArea);

        float offset = 0;
        bool left = true;
        buttons = new Dictionary<string, List<UIElement>>();
        UIElement ui = null;
        foreach (string hero in heroList)
        {
            buttons.Add(hero, new List<UIElement>());

            // Should be game type specific
            Texture2D newTex = ContentData.FileToTexture(game.cd.heroes[hero].image);

            ui = new UIElement(Game.HEROSELECT, scrollArea.GetScrollTransform());
            if (left)
            {
                ui.SetLocation(3.25f, offset + 1.5f, UIScaler.GetWidthUnits() - 19, 1.5f);
            }
            else
            {
                ui.SetLocation(7.75f, offset + 1.5f, UIScaler.GetWidthUnits() - 18, 1.5f);
            }
            ui.SetBGColor(Color.white);
            ui.SetButton(delegate { Select(hero); });
            buttons[hero].Add(ui);

            ui = new UIElement(Game.HEROSELECT, scrollArea.GetScrollTransform());
            if (left)
            {
                ui.SetLocation(1, offset, 4.25f, 4.25f);
            }
            else
            {
                ui.SetLocation(UIScaler.GetWidthUnits() - 11.25f, offset, 4.25f, 4.25f);
            }
            ui.SetImage(newTex);
            ui.SetButton(delegate { Select(hero); });
            buttons[hero].Add(ui);

            ui = new UIElement(Game.HEROSELECT, scrollArea.GetScrollTransform());
            if (left)
            {
                ui.SetLocation(6.25f, offset + 1.5f, UIScaler.GetWidthUnits() - 19, 1.5f);
            }
            else
            {
                ui.SetLocation(8.75f, offset + 1.5f, UIScaler.GetWidthUnits() - 20, 1.5f);
            }
            ui.SetBGColor(Color.white);
            ui.SetText(game.cd.heroes[hero].name, Color.black);
            ui.SetTextAlignment(TextAnchor.MiddleLeft);
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(delegate { Select(hero); });
            buttons[hero].Add(ui);

            left = !left;
            offset += 2.25f;
        }
        scrollArea.SetScrollSize(offset + 2);
    }

    public void Select(string name)
    {
        Game game = Game.Get();
        HeroData hData = null;
        foreach (KeyValuePair<string, HeroData> hd in game.cd.heroes)
        {
            if (hd.Value.sectionName.Equals(name))
            {
                hData = hd.Value;
                break;
            }
        }
        foreach (Quest.Hero h in game.quest.heroes)
        {
            if (hData == h.heroData)
            {
                return;
            }
        }
        foreach (Quest.Hero h in game.quest.heroes)
        {
            if (h.heroData == null)
            {
                foreach (UIElement ui in buttons[name])
                {
                    ui.SetBGColor(new Color(0.4f, 0.4f, 0.4f));
                }
                h.heroData = hData;
                game.heroCanvas.UpdateImages();
                return;
            }
        }
    }

    public void Update()
    {
        foreach (KeyValuePair<string, List<UIElement>> kv in buttons)
        {
            Color c = Color.white;
            foreach (Quest.Hero h in Game.Get().quest.heroes)
            {
                if (h.heroData != null && h.heroData.sectionName.Equals(kv.Key))
                {
                    c = new Color(0.4f, 0.4f, 0.4f);
                }
            }

            foreach (UIElement ui in kv.Value)
            {
                ui.SetBGColor(c);
            }
        }
    }
}
