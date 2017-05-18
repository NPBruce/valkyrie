using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using System.IO;

namespace Assets.Scripts.UI.Screens
{
    public class QuestDetailsScreen
    {
        public QuestDetailsScreen(QuestData.Quest q)
        {
            Game game = Game.Get();
            LocalizationRead.scenarioDict = q.localizationDict;
            // If a dialog window is open we force it closed (this shouldn't happen)
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
                Object.Destroy(go);

            // Heading
            DialogBox db = new DialogBox(
                new Vector2(2, 0.5f), 
                new Vector2(UIScaler.GetWidthUnits() - 4, 3), 
                q.name);
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
            db.SetFont(game.gameType.GetHeaderFont());

            // Draw Image
            if (q.image.Length > 0)
            {
                Texture2D tex = ContentData.FileToTexture(Path.Combine(q.path, q.image));
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1);

                db = new DialogBox(new Vector2(UIScaler.GetHCenter(-20), 4),
                    new Vector2(8, 8),
                    StringKey.NULL,
                    Color.white,
                    Color.white);
                db.background.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
                db.AddBorder();
            }

            // Draw Description
            db = new DialogBox(Vector2.zero, new Vector2(30, 30), q.description);
            float height = (db.textObj.GetComponent<UnityEngine.UI.Text>().preferredHeight / UIScaler.GetPixelsPerUnit()) + 1;
            db.Destroy();
            if (height > 25) height = 25;

            db = new DialogBox(new Vector2(UIScaler.GetHCenter(-7), 15 - (height / 2)), new Vector2(30, height), q.description);
            db.AddBorder();

            // Draw authors
            db = new DialogBox(Vector2.zero, new Vector2(14, 30), q.authors);
            height = (db.textObj.GetComponent<UnityEngine.UI.Text>().preferredHeight / UIScaler.GetPixelsPerUnit()) + 1;
            db.Destroy();
            if (height > 25) height = 25;

            db = new DialogBox(new Vector2(UIScaler.GetHCenter(-23), 18.5f - (height / 2)), new Vector2(14, height), q.authors);
            db.AddBorder();

            // Difficulty
            if (q.difficulty != 0)
            {
                db = new DialogBox(new Vector2(UIScaler.GetHCenter(-13), 27), new Vector2(11, 1), new StringKey("val","DIFFICULTY"));
                string symbol = new StringKey("val","ICON_SUCCESS_RESULT").Translate();
                db = new DialogBox(new Vector2(UIScaler.GetHCenter(-13), 28), new Vector2(11, 2), new StringKey(null, symbol + symbol + symbol + symbol + symbol, false));
                db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
                db = new DialogBox(new Vector2(UIScaler.GetHCenter(-10.95f) + (q.difficulty * 6.9f), 28), new Vector2((1 - q.difficulty) * 6.9f, 2), StringKey.NULL, Color.clear, new Color(0, 0, 0, 0.7f));
            }

            // Duration
            if (q.lengthMax != 0)
            {
                db = new DialogBox(new Vector2(UIScaler.GetHCenter(2), 27), new Vector2(11, 1), new StringKey("val","DURATION"));
                db = new DialogBox(new Vector2(UIScaler.GetHCenter(2), 28f), new Vector2(4, 2), q.lengthMin);
                db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
                db = new DialogBox(new Vector2(UIScaler.GetHCenter(6.5f), 28f), new Vector2(2, 2), new StringKey(null, "-", false));
                db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
                db = new DialogBox(new Vector2(UIScaler.GetHCenter(9), 28f), new Vector2(4, 2), q.lengthMax);
                db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            }

            TextButton tb = new TextButton(
                new Vector2(0.5f, UIScaler.GetBottom(-2.5f)), new Vector2(8, 2), 
                CommonStringKeys.BACK, delegate { Cancel(); }, Color.red);
            tb.SetFont(game.gameType.GetHeaderFont());

            tb = new TextButton(
                new Vector2(UIScaler.GetRight(-8.5f), UIScaler.GetBottom(-2.5f)), new Vector2(8, 2), 
                new StringKey("val", "START"), delegate { Start(q); }, Color.green);
            tb.SetFont(game.gameType.GetHeaderFont());
        }

        // Return to quest selection
        public void Cancel()
        {
            Destroyer.Dialog();
            // Get a list of available quests
            Dictionary<string, QuestData.Quest> ql = QuestLoader.GetQuests();

            // Pull up the quest selection page
            new QuestSelectionScreen(ql);
        }

        // Select a quest
        public void Start(QuestData.Quest q)
        {
            Destroyer.Dialog();
            Game.Get().StartQuest(q);
        }
    }
}
