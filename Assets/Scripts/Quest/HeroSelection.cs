using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

// Hero selection options
// This comes up when selection a hero icon to pick hero
public class HeroSelection {

    public Dictionary<string, List<TextButton>> buttons;

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
        List<string> heroList = new List<string>(game.cd.heros.Keys);
        heroList.Sort();

        DialogBox db = new DialogBox(new Vector2(4.5f, 4f), new Vector2(UIScaler.GetWidthUnits() - 5.5f, 22f), StringKey.NULL);
        db.AddBorder();
        db.background.AddComponent<UnityEngine.UI.Mask>();
        db.ApplyTag("heroselect");
        UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

        GameObject scrollArea = new GameObject("scroll");
        RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
        scrollArea.transform.parent = db.background.transform;
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 1);
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, (UIScaler.GetWidthUnits() - 3f) * UIScaler.GetPixelsPerUnit());

        GameObject scrollBarObj = new GameObject("scrollbar");
        scrollBarObj.transform.parent = db.background.transform;
        RectTransform scrollBarRect = scrollBarObj.AddComponent<RectTransform>();
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 22 * UIScaler.GetPixelsPerUnit());
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (UIScaler.GetWidthUnits() - 6.5f) * UIScaler.GetPixelsPerUnit(), 1 * UIScaler.GetPixelsPerUnit());
        UnityEngine.UI.Scrollbar scrollBar = scrollBarObj.AddComponent<UnityEngine.UI.Scrollbar>();
        scrollBar.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
        scrollRect.verticalScrollbar = scrollBar;

        GameObject scrollBarHandle = new GameObject("scrollbarhandle");
        scrollBarHandle.transform.parent = scrollBarObj.transform;
        //RectTransform scrollBarHandleRect = scrollBarHandle.AddComponent<RectTransform>();
        scrollBarHandle.AddComponent<UnityEngine.UI.Image>();
        scrollBarHandle.GetComponent<UnityEngine.UI.Image>().color = new Color(0.7f, 0.7f, 0.7f);
        scrollBar.handleRect = scrollBarHandle.GetComponent<RectTransform>();
        scrollBar.handleRect.offsetMin = Vector2.zero;
        scrollBar.handleRect.offsetMax = Vector2.zero;

        scrollRect.content = scrollInnerRect;
        scrollRect.horizontal = false;

        float offset = 4.5f;
        TextButton tb = null;
        bool left = true;
        buttons = new Dictionary<string, List<TextButton>>();

        foreach (string hero in heroList)
        {
            buttons.Add(hero, new List<TextButton>());

            // Should be game type specific
            Texture2D newTex = ContentData.FileToTexture(game.cd.heros[hero].image);
            Sprite heroSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);

            if (left)
            {
                tb = new TextButton(new Vector2(7.25f, offset + 1.5f), new Vector2(UIScaler.GetWidthUnits() - 19, 1.5f), StringKey.NULL, delegate { Select(hero); }, Color.clear);
            }
            else
            {
                tb = new TextButton(new Vector2(11.75f, offset + 1.5f), new Vector2(UIScaler.GetWidthUnits() - 18, 1.5f), StringKey.NULL, delegate { Select(hero); }, Color.clear);
            }
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            tb.background.transform.parent = scrollArea.transform;
            tb.ApplyTag("heroselect");
            buttons[hero].Add(tb);

            if (left)
            {
                tb = new TextButton(new Vector2(5f, offset), new Vector2(4.25f, 4.25f), StringKey.NULL, delegate { Select(hero); }, Color.clear);
            }
            else
            {
                tb = new TextButton(new Vector2(UIScaler.GetWidthUnits() - 7.25f, offset), new Vector2(4.25f, 4.25f), StringKey.NULL, delegate { Select(hero); }, Color.clear);
            }
            tb.background.GetComponent<UnityEngine.UI.Image>().sprite = heroSprite;
            tb.background.transform.parent = scrollArea.transform;
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            tb.ApplyTag("heroselect");
            buttons[hero].Add(tb);

            if (left)
            {
                tb = new TextButton(new Vector2(10.25f, offset + 1.5f), new Vector2(UIScaler.GetWidthUnits() - 19, 1.5f), 
                    game.cd.heros[hero].name, delegate { Select(hero); }, Color.black);
            }
            else
            {
                tb = new TextButton(new Vector2(12.75f, offset + 1.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 1.5f), 
                    game.cd.heros[hero].name, delegate { Select(hero); }, Color.black);
            }
            tb.setColor(Color.clear);
            tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
            tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
            //tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            tb.background.transform.parent = scrollArea.transform;
            tb.ApplyTag("heroselect");
            buttons[hero].Add(tb);

            left = !left;
            offset += 2.25f;
        }
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (offset - 2.5f) * UIScaler.GetPixelsPerUnit());
    }

    public void Select(string name)
    {
        Game game = Game.Get();
        HeroData hData = null;
        foreach (KeyValuePair<string, HeroData> hd in game.cd.heros)
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
                foreach (TextButton tb in buttons[name])
                {
                    tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.4f, 0.4f, 0.4f);
                }
                h.heroData = hData;
                game.heroCanvas.UpdateImages();
                return;
            }
        }
    }

    public void Update()
    {
        foreach (KeyValuePair<string, List<TextButton>> kv in buttons)
        {
            Color c = Color.white;
            foreach (Quest.Hero h in Game.Get().quest.heroes)
            {
                if (h.heroData != null && h.heroData.sectionName.Equals(kv.Key))
                {
                    c = new Color(0.4f, 0.4f, 0.4f);
                }
            }

            foreach (TextButton tb in kv.Value)
            {
                tb.background.GetComponent<UnityEngine.UI.Image>().color = c;
            }
        }
    }
}
