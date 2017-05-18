using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using System.IO;

namespace Assets.Scripts.UI.Screens
{
    // Class for quest selection window
    public class QuestSelectionScreen
    {
        private StringKey DOWNLOAD = new StringKey("val", "DOWNLOAD");

        public Dictionary<string, QuestData.Quest> questList;

        public QuestSelectionScreen(Dictionary<string, QuestData.Quest> ql)
        {
            questList = ql;
            Game game = Game.Get();
            // If a dialog window is open we force it closed (this shouldn't happen)
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
                Object.Destroy(go);

            // Clean up downloader if present
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.QUESTUI))
                Object.Destroy(go);

            // Heading
            DialogBox db = new DialogBox(
                new Vector2(2, 1), 
                new Vector2(UIScaler.GetWidthUnits() - 4, 3), 
                new StringKey("val","SELECT",game.gameType.QuestName())
                );
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
            db.SetFont(game.gameType.GetHeaderFont());

            db = new DialogBox(new Vector2(1, 5f), new Vector2(UIScaler.GetWidthUnits()-2f, 21f), StringKey.NULL);
            db.AddBorder();
            db.background.AddComponent<UnityEngine.UI.Mask>();
            UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

            GameObject scrollBarObj = new GameObject("scrollbar");
            scrollBarObj.transform.SetParent(db.background.transform);
            RectTransform scrollBarRect = scrollBarObj.AddComponent<RectTransform>();
            scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 21 * UIScaler.GetPixelsPerUnit());
            scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (UIScaler.GetWidthUnits() - 3f) * UIScaler.GetPixelsPerUnit(), 1 * UIScaler.GetPixelsPerUnit());
            UnityEngine.UI.Scrollbar scrollBar = scrollBarObj.AddComponent<UnityEngine.UI.Scrollbar>();
            scrollBar.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
            scrollRect.verticalScrollbar = scrollBar;

            GameObject scrollBarHandle = new GameObject("scrollbarhandle");
            scrollBarHandle.transform.SetParent(scrollBarObj.transform);
            //RectTransform scrollBarHandleRect = scrollBarHandle.AddComponent<RectTransform>();
            scrollBarHandle.AddComponent<UnityEngine.UI.Image>();
            scrollBarHandle.GetComponent<UnityEngine.UI.Image>().color = new Color(0.7f, 0.7f, 0.7f);
            scrollBar.handleRect = scrollBarHandle.GetComponent<RectTransform>();
            scrollBar.handleRect.offsetMin = Vector2.zero;
            scrollBar.handleRect.offsetMax = Vector2.zero;

            GameObject scrollArea = new GameObject("scroll");
            RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
            scrollArea.transform.SetParent(db.background.transform);
            scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 1);
            scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, (UIScaler.GetWidthUnits()-3f) * UIScaler.GetPixelsPerUnit());

            scrollRect.content = scrollInnerRect;
            scrollRect.horizontal = false;
            scrollRect.scrollSensitivity = 27f;

            TextButton tb;
            // Start here
            float offset = 5;
            // Loop through all available quests
            foreach (KeyValuePair<string, QuestData.Quest> q in questList)
            {
                if (q.Value.GetMissingPacks(game.cd.GetLoadedPackIDs()).Count == 0)
                {
                    string key = q.Key;
                    LocalizationRead.scenarioDict = q.Value.localizationDict;
                    string translation = q.Value.name.Translate();

                    // Draw Image
                    db = new DialogBox(new Vector2(2, offset),
                        new Vector2(3, 3),
                        StringKey.NULL,
                        Color.white,
                        Color.white);
                    db.background.transform.SetParent(scrollArea.transform);
                    if (q.Value.image.Length > 0)
                    {
                        Texture2D tex = ContentData.FileToTexture(Path.Combine(q.Value.path, q.Value.image));
                        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1);
                        db.background.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
                    }

                    tb = new TextButton(
                        new Vector2(5, offset), 
                        new Vector2(UIScaler.GetWidthUnits() - 8, 3f),
                        new StringKey("val", "INDENT", translation),
                        delegate { Selection(key); }, Color.clear, (int)offset);
                    tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                    //tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                    tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
                    tb.button.GetComponent<UnityEngine.UI.Text>().color = Color.black;
                    tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
                    tb.background.transform.SetParent(scrollArea.transform);


                    // Duration
                    if (q.Value.lengthMax != 0)
                    {
                        db = new DialogBox(new Vector2(UIScaler.GetRight(-10), offset), new Vector2(2, 1), q.Value.lengthMin, Color.black, Color.clear);
                        db.background.transform.SetParent(scrollArea.transform);
                        db.textObj.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                        db = new DialogBox(new Vector2(UIScaler.GetRight(-8), offset), new Vector2(1, 1), new StringKey(null, "-", false), Color.black, Color.clear);
                        db.background.transform.SetParent(scrollArea.transform);
                        db.textObj.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                        db = new DialogBox(new Vector2(UIScaler.GetRight(-7), offset), new Vector2(2, 1), q.Value.lengthMax, Color.black, Color.clear);
                        db.background.transform.SetParent(scrollArea.transform);
                        db.textObj.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                    }

                    // Difficulty
                    if (q.Value.difficulty != 0)
                    {
                        string symbol = "π"; // will
                        if (game.gameType is MoMGameType)
                        {
                            symbol = new StringKey("val", "ICON_SUCCESS_RESULT").Translate();
                        }
                        db = new DialogBox(new Vector2(UIScaler.GetRight(-11), offset + 1), new Vector2(7, 2), new StringKey(null, symbol + symbol + symbol + symbol + symbol, false), Color.black, Color.clear);
                        db.background.transform.SetParent(scrollArea.transform);
                        db.textObj.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
                        db = new DialogBox(new Vector2(UIScaler.GetRight(-10.95f) + (q.Value.difficulty * 6.9f), offset + 1), new Vector2((1 - q.Value.difficulty) * 6.9f, 2), StringKey.NULL, Color.clear, new Color(1, 1, 1, 0.7f));
                        db.background.transform.SetParent(scrollArea.transform);
                    }

                    offset += 4;
                }
            }

            // Loop through all unavailable quests
            foreach (KeyValuePair<string, QuestData.Quest> q in questList)
            {
                if (q.Value.GetMissingPacks(game.cd.GetLoadedPackIDs()).Count > 0)
                {
                    string key = q.Key;
                    LocalizationRead.scenarioDict = q.Value.localizationDict;
                    string translation = q.Value.name.Translate();

                    // Size is 1.2 to be clear of characters with tails
                    db = new DialogBox(
                        new Vector2(2, offset), 
                        new Vector2(UIScaler.GetWidthUnits() - 5, 1.2f),
                        new StringKey("val", "INDENT", translation),
                        Color.black);
                    db.textObj.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                    db.textObj.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
                    db.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.4f, 0.4f, 0.4f);
                    db.background.transform.SetParent(scrollArea.transform);
                    offset += 1.2f;
                    foreach (string s in q.Value.GetMissingPacks(game.cd.GetLoadedPackIDs()))
                    {
                        db = new DialogBox(
                            new Vector2(4, offset), 
                            new Vector2(UIScaler.GetWidthUnits() - 9, 1.2f),
                            // TODO: Expansion names should be keys too
                            new StringKey("val", "REQUIRES_EXPANSION", game.cd.GetContentName(s)),                    
                            Color.black);
                        db.textObj.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                        db.textObj.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
                        db.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.4f, 0.4f, 0.4f);
                        db.background.transform.SetParent(scrollArea.transform);
                        offset += 1.2f;
                    }
                }
                offset += 0.8f;
            }

            scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (offset - 5) * UIScaler.GetPixelsPerUnit());

            tb = new TextButton(
                new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), 
                CommonStringKeys.BACK, delegate { Cancel(); }, Color.red);

            tb.SetFont(game.gameType.GetHeaderFont());

            tb = new TextButton(
                new Vector2(UIScaler.GetRight(-9), UIScaler.GetBottom(-3)), new Vector2(8, 2), 
                DOWNLOAD, delegate { Download(); }, Color.green);
            tb.SetFont(game.gameType.GetHeaderFont());
        }

        // Return to main menu
        public void Cancel()
        {
            Destroyer.MainMenu();
        }

        // Return to main menu
        public void Download()
        {
            Destroyer.Dialog();
            GameObject download = new GameObject("downloadPage");
            download.tag = Game.QUESTUI;
            download.AddComponent<QuestDownload>();
        }

        // Select a quest
        public void Selection(string key)
        {
            Destroyer.Dialog();
            new QuestDetailsScreen(questList[key]);
        }
    }
}