using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class PuzzleSlideWindow {
    public EventManager.Event eventData;
    public PuzzleSlide puzzle;

    public PuzzleSlideWindow(EventManager.Event e)
    {
        eventData = e;
        Game game = Game.Get();

        QuestData.Event questPuzzle = e.qEvent as QuestData.Puzzle;

        puzzle = new PuzzleSlide(questPuzzle.level);

        CreateWindow();
    }

    public void CreateWindow()
    {
        DialogBox db = new DialogBox(new Vector2(10, 0.5f), new Vector2(28f, 22f), "");
        db.AddBorder();

        if (puzzle.Solved())
        {
            new TextButton(new Vector2(11, 24.5f), new Vector2(8f, 2), "Close", delegate { ; }, Color.grey);
            new TextButton(new Vector2(UIScaler.GetWidthUnits() - 19, 24.5f), new Vector2(8f, 2), eventData.GetButtons()[0].label, delegate { Finished(); });
        }
        else
        {
            new TextButton(new Vector2(11, 24.5f), new Vector2(8f, 2), "Close", delegate { Close(); });
            new TextButton(new Vector2(UIScaler.GetWidthUnits() - 19, 24.5f), new Vector2(8f, 2), eventData.GetButtons()[0].label, delegate { ; }, Color.grey);
        }
    }

    public void Close()
    {
        Destroyer.Dialog();
        Game.Get().quest.eManager.currentEvent = null;
        Game.Get().quest.eManager.TriggerEvent();
    }

    public void Finished(int num)
    {
        Game game = Game.Get();
        Destroyer.Dialog();

        game.quest.eManager.EndEvent(num-1);
    }
}
