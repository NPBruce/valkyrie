using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

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
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
                Object.Destroy(go);

            // Clean up downloader if present
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("questui"))
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
            scrollBarObj.transform.parent = db.background.transform;
            RectTransform scrollBarRect = scrollBarObj.AddComponent<RectTransform>();
            scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 21 * UIScaler.GetPixelsPerUnit());
            scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (UIScaler.GetWidthUnits() - 3f) * UIScaler.GetPixelsPerUnit(), 1 * UIScaler.GetPixelsPerUnit());
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

            GameObject scrollArea = new GameObject("scroll");
            RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
            scrollArea.transform.parent = db.background.transform;
            scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 1);
            scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, (UIScaler.GetWidthUnits()-3f) * UIScaler.GetPixelsPerUnit());

            scrollRect.content = scrollInnerRect;
            scrollRect.horizontal = false;

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

                    // Size is 1.2 to be clear of characters with tails
                    tb = new TextButton(
                        new Vector2(2, offset), 
                        new Vector2(UIScaler.GetWidthUnits() - 5, 1.2f),
                        new StringKey("val", "INDENT", translation),
                        delegate { Selection(key); }, Color.black, (int)offset);
                    tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                    tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                    tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
                    tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
                    tb.background.transform.parent = scrollArea.transform;
                    offset += 2;
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
                    db.background.transform.parent = scrollArea.transform;
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
                        db.background.transform.parent = scrollArea.transform;
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
            download.tag = "questui";
            download.AddComponent<QuestDownload>();
        }

        // Select a quest
        public void Selection(string key)
        {
            Game game = Game.Get();

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
                Object.Destroy(go);

            game.StartQuest(questList[key]);
        }
    }
}