using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

namespace Assets.Scripts.UI.Screens
{
    public class QuestDetailsScreen
    {
        public QuestDetailsScreen(QuestData.Quest q)
        {
            Game game = Game.Get();
            LocalizationRead.scenarioDict = q.Value.localizationDict;
            // If a dialog window is open we force it closed (this shouldn't happen)
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
                Object.Destroy(go);

            // Heading
            DialogBox db = new DialogBox(
                new Vector2(2, 0.5f), 
                new Vector2(UIScaler.GetWidthUnits() - 4, 3), 
                q.Value.name);
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
            db.SetFont(game.gameType.GetHeaderFont());

            // Draw Description
            db = new DialogBox(Vector2.zero, new Vector2(33, 30), q.Value.description);
            float height = (db.textObj.GetComponent<UnityEngine.UI.Text>().preferredHeight / UIScaler.GetPixelsPerUnit()) + 1;
            db.Destroy();
            if (height > 25) height = 25;

            db = new DialogBox(new Vector2(UIScaler.GetHCentre(-23), 15 - (height / 2)), new Vector2(33, height), q.Value.description);
            db = new DialogBox(new Vector2(1, 5f), new Vector2(UIScaler.GetWidthUnits()-2f, 21f), StringKey.NULL);
            db.AddBorder();

            // Draw authors
            db = new DialogBox(Vector2.zero, new Vector2(12, 30), q.Value.authors);
            float height = (db.textObj.GetComponent<UnityEngine.UI.Text>().preferredHeight / UIScaler.GetPixelsPerUnit()) + 1;
            db.Destroy();
            if (height > 25) height = 25;

            db = new DialogBox(new Vector2(UIScaler.GetHCentre(11), 15 - (height / 2)), new Vector2(12, height), q.Value.authors);
            db = new DialogBox(new Vector2(1, 5f), new Vector2(UIScaler.GetWidthUnits()-2f, 21f), StringKey.NULL);
            db.AddBorder();

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
            Destoyer.Dialog();
            Game.Get().StartQuest(q);
        }
    }
}
