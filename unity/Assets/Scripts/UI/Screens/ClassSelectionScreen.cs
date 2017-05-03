using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

namespace Assets.Scripts.UI.Screens
{
    class ClassSelectionScreen
    {
        public ClassSelectionScreen()
        {
            Draw();
        }

        public void Draw()
        {
            // Clean up
            Destroyer.Dialog();

            Game game = Game.Get();

            // Add a title to the page
            DialogBox db = new DialogBox(
                new Vector2(8, 1),
                new Vector2(UIScaler.GetWidthUnits() - 16, 3),
                new StringKey("val", "SELECT_CLASS")
                );
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
            db.SetFont(game.gameType.GetHeaderFont());
            db.ApplyTag("heroselect");

            // Get all heros
            int heroCount = 0;
            // Count number of selected heroes
            foreach (Quest.Hero h in game.quest.heroes)
            {
                if (h.heroData != null) heroCount++;
            }

            float xOffset = UIScaler.GetHCenter(-18);
            if (heroCount < 4) xOffset += 4.5f;
            if (heroCount < 3) xOffset += 4.5f;

            TextButton tb = null;

            for (int i = 0; i < heroCount; i++)
            {
                DrawHero(xOffset, i);
                xOffset += 9f;
            }
            // Add a finished button to start the quest
            tb = new TextButton(
                new Vector2(UIScaler.GetRight(-8.5f),
                UIScaler.GetBottom(-2.5f)),
                new Vector2(8, 2),
                CommonStringKeys.FINISHED,
                delegate { Finished(); },
                Color.green);
            tb.SetFont(game.gameType.GetHeaderFont());
            tb.ApplyTag("heroselect");

            TextButton cancelSelection = new TextButton(new Vector2(0.5f, UIScaler.GetBottom(-2.5f)), new Vector2(8, 2), CommonStringKeys.BACK, delegate { Destroyer.QuestSelect(); }, Color.red);
            cancelSelection.SetFont(game.gameType.GetHeaderFont());
            // Untag as dialog so this isn't cleared away during hero selection
            cancelSelection.ApplyTag("heroselect");
        }

        public void DrawHero(float xOffset, int hero)
        {
            Game game = Game.Get();

            string archetype = game.quest.heroes[hero].heroData.archetype;
            string hybridClass = game.quest.heroes[hero].hybridClass;
            float yStart = 7f;

            DialogBox db = null;
            TextButton tb = null;
            if (hybridClass.Length > 0)
            {
                archetype = game.cd.classes[hybridClass].hybridArchetype;
                db = new DialogBox(new Vector2(xOffset + 0.25f, yStart), new Vector2(8.5f, 5f), StringKey.NULL);
                db.AddBorder();
                db.ApplyTag("heroselect");

                tb = new TextButton(new Vector2(xOffset + 0.75f, yStart + 0.5f), new Vector2(7f, 4f), game.cd.classes[hybridClass].name, delegate { Select(hero, hybridClass); });
                tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.7f, 0);
                tb.ApplyTag("heroselect");

                yStart += 5;
            }

            db = new DialogBox(new Vector2(xOffset + 0.25f, yStart), new Vector2(8.5f, 27f - yStart), StringKey.NULL);
            db.AddBorder();
            db.background.AddComponent<UnityEngine.UI.Mask>();
            db.ApplyTag("heroselect");
            UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

            GameObject scrollArea = new GameObject("scroll");
            RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
            scrollArea.transform.parent = db.background.transform;
            scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 1);
            scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, (8f) * UIScaler.GetPixelsPerUnit());

            GameObject scrollBarObj = new GameObject("scrollbar");
            scrollBarObj.transform.parent = db.background.transform;
            RectTransform scrollBarRect = scrollBarObj.AddComponent<RectTransform>();
            scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (27f - yStart) * UIScaler.GetPixelsPerUnit());
            scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (7.75f) * UIScaler.GetPixelsPerUnit(), 1 * UIScaler.GetPixelsPerUnit());
            UnityEngine.UI.Scrollbar scrollBar = scrollBarObj.AddComponent<UnityEngine.UI.Scrollbar>();
            scrollBar.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
            scrollRect.verticalScrollbar = scrollBar;

            GameObject scrollBarHandle = new GameObject("scrollbarhandle");
            scrollBarHandle.transform.parent = scrollBarObj.transform;
            scrollBarHandle.AddComponent<UnityEngine.UI.Image>();
            scrollBarHandle.GetComponent<UnityEngine.UI.Image>().color = new Color(0.7f, 0.7f, 0.7f);
            scrollBar.handleRect = scrollBarHandle.GetComponent<RectTransform>();
            scrollBar.handleRect.offsetMin = Vector2.zero;
            scrollBar.handleRect.offsetMax = Vector2.zero;

            scrollRect.content = scrollInnerRect;
            scrollRect.horizontal = false;
            scrollRect.scrollSensitivity = 27f;

            float yOffset = yStart + 1f;

            foreach (ClassData cd in game.cd.classes.Values)
            {
                if (!cd.archetype.Equals(archetype)) continue;
                if (cd.hybridArchetype.Length > 0 && hybridClass.Length > 0) continue;

                string className = cd.sectionName;
                bool available = true;
                bool pick = false;

                for (int i = 0; i < game.quest.heroes.Count; i++)
                {
                    if (game.quest.heroes[i].className.Equals(className))
                    {
                        available = false;
                        if (hero == i)
                        {
                            pick = true;
                        }
                    }
                }

                if (available)
                {
                    tb = new TextButton(new Vector2(xOffset + 0.5f, yOffset), new Vector2(7f, 4f), cd.name, delegate { Select(hero, className); }, Color.clear);
                    tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
                }
                else
                {
                    tb = new TextButton(new Vector2(xOffset + 0.5f, yOffset), new Vector2(7f, 4f), cd.name, delegate {; }, Color.clear);
                    tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.2f, 0.2f, 0.2f);
                    if (pick)
                    {
                        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.7f, 0);
                    }
                }
                tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
                tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                tb.background.transform.parent = scrollArea.transform;
                tb.ApplyTag("heroselect");

                yOffset += 5f;
            }

            scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (yOffset - 2.5f) * UIScaler.GetPixelsPerUnit());

            Texture2D heroTex = ContentData.FileToTexture(game.quest.heroes[hero].heroData.image);
            Sprite heroSprite = Sprite.Create(heroTex, new Rect(0, 0, heroTex.width, heroTex.height), Vector2.zero, 1);
            db = new DialogBox(new Vector2(xOffset + 2.5f, 3.5f), new Vector2(4f, 4f), StringKey.NULL, Color.clear, Color.white);
            db.background.GetComponent<UnityEngine.UI.Image>().sprite = heroSprite;
            db.ApplyTag("heroselect");
        }

        public void Select(int hero, string className)
        {
            Game game = Game.Get();
            if (game.cd.classes[className].hybridArchetype.Length > 0)
            {
                if (game.quest.heroes[hero].hybridClass.Length > 0)
                {
                    game.quest.heroes[hero].hybridClass = "";
                }
                else
                {
                    game.quest.heroes[hero].hybridClass = className;
                }
            }
            else
            {
                game.quest.heroes[hero].className = className;
            }
            Draw();
        }

        public void Finished()
        {
            Game game = Game.Get();

            HashSet<string> items = new HashSet<string>();
            foreach (Quest.Hero h in game.quest.heroes)
            {
                if (h.heroData == null) continue;
                if (h.className.Length == 0) return;

                foreach (string s in game.cd.classes[h.className].items)
                {
                    items.Add(s);
                }

                if (h.hybridClass.Length == 0) continue;
                foreach (string s in game.cd.classes[h.hybridClass].items)
                {
                    items.Add(s);
                }
            }
            game.quest.items.UnionWith(items);

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("heroselect"))
                Object.Destroy(go);
            game.moraleDisplay = new MoraleDisplay();
            game.QuestStartEvent();
        }
    }
}
