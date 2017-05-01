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

            for (int i = 0; i < heroCount; i++)
            {
                db = new DialogBox(new Vector2(xOffset + 0.25f, 7f), new Vector2(8.5f, 20f), StringKey.NULL);
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
                scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 20 * UIScaler.GetPixelsPerUnit());
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

                float yOffset = 8f;
                TextButton tb = new TextButton(new Vector2(xOffset + 0.5f, yOffset), new Vector2(7f, 4f), CommonStringKeys.ACTIVATION, delegate { ; }, Color.clear);
                tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
                tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                tb.background.transform.parent = scrollArea.transform;
                tb.ApplyTag("heroselect");

                scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (yOffset - 2.5f) * UIScaler.GetPixelsPerUnit());

                Texture2D heroTex = ContentData.FileToTexture(game.quest.heroes[i].heroData.image);
                Sprite heroSprite = Sprite.Create(heroTex, new Rect(0, 0, heroTex.width, heroTex.height), Vector2.zero, 1);
                db = new DialogBox(new Vector2(xOffset + 2.5f, 3.5f), new Vector2(4f, 4f), StringKey.NULL, Color.clear, Color.white);
                db.background.GetComponent<UnityEngine.UI.Image>().sprite = heroSprite;
                db.ApplyTag("heroselect");

                xOffset += 9f;
            }
            // Add a finished button to start the quest
            TextButton endSelection = new TextButton(
                new Vector2(UIScaler.GetRight(-8.5f),
                UIScaler.GetBottom(-2.5f)),
                new Vector2(8, 2),
                CommonStringKeys.FINISHED,
                delegate { Finished(); },
                Color.green);


            TextButton cancelSelection = new TextButton(new Vector2(0.5f, UIScaler.GetBottom(-2.5f)), new Vector2(8, 2), CommonStringKeys.BACK, delegate { Destroyer.QuestSelect(); }, Color.red);
            cancelSelection.SetFont(game.gameType.GetHeaderFont());
            // Untag as dialog so this isn't cleared away during hero selection
            cancelSelection.ApplyTag("heroselect");
        }

        public void Finished()
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("heroselect"))
                Object.Destroy(go);

            Game game = Game.Get();
            game.moraleDisplay = new MoraleDisplay();
            game.QuestStartEvent();
        }
    }
}
