using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.UI.Screens
{
    // Class for quest selection window
    public class QuestSelectionScreen
    {

        public Dictionary<string, QuestLoader.Quest> questList;

        public QuestSelectionScreen(Dictionary<string, QuestLoader.Quest> ql)
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
            DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 3), "Select " + game.gameType.QuestName());
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
            db.SetFont(game.gameType.GetHeaderFont());

            TextButton tb;
            // Start here
            int offset = 5;
            // Loop through all available quests
            // FIXME: this isn't paged
            foreach (KeyValuePair<string, QuestLoader.Quest> q in questList)
            {
                string key = q.Key;
                // Size is 1.2 to be clear of characters with tails
                tb = new TextButton(new Vector2(2, offset), new Vector2(UIScaler.GetWidthUnits() - 4, 1.2f), "  " + q.Value.name, delegate { Selection(key); }, Color.white, offset);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
                tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0.1f);
                offset += 2;
            }

            tb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Back", delegate { Cancel(); }, Color.red);
            tb.SetFont(game.gameType.GetHeaderFont());

            tb = new TextButton(new Vector2(UIScaler.GetRight(-9), UIScaler.GetBottom(-3)), new Vector2(8, 2), "Download", delegate { Download(); }, Color.green);
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