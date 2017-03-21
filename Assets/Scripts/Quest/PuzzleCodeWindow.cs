using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class PuzzleCodeWindow
{
    public EventManager.Event eventData;
    QuestData.Puzzle questPuzzle;
    public PuzzleCode puzzle;
    public List<int> guess;
    public int previousMoves = 0;

    public PuzzleCodeWindow(EventManager.Event e)
    {
        eventData = e;
        Game game = Game.Get();

        guess = new List<int>();
        questPuzzle = e.qEvent as QuestData.Puzzle;

        if (game.quest.puzzle.ContainsKey(questPuzzle.name))
        {
            puzzle = game.quest.puzzle[questPuzzle.name] as PuzzleCode;
            previousMoves = puzzle.guess.Count;
        }
        else
        {
            puzzle = new PuzzleCode(questPuzzle.puzzleLevel, questPuzzle.puzzleAltLevel);
        }

        CreateWindow();
    }

    public void CreateWindow()
    {
        Destroyer.Dialog();
        DialogBox db = new DialogBox(new Vector2(UIScaler.GetHCenter(-14f), 0.5f), new Vector2(28f, 22f), "");
        db.AddBorder();

        // Puzzle goes here

        float hPos = UIScaler.GetHCenter(-13f);
        if (!puzzle.Solved())
        {
            for (int i = 1; i <= questPuzzle.puzzleAltLevel; i++)
            {
                int tmp = i;
                new TextButton(new Vector2(hPos, 1.5f), new Vector2(2f, 2), i.ToString(), delegate { GuessAdd(tmp); });
                hPos += 2.5f;
            }
            hPos = UIScaler.GetHCenter(-13f);
            for (int i = 1; i <= questPuzzle.puzzleLevel; i++)
            {
                if (guess.Count >= i)
                {
                    int tmp = i - 1;
                    new TextButton(new Vector2(hPos, 4f), new Vector2(2f, 2f), guess[tmp].ToString(), delegate { GuessRemove(tmp); });
                }
                else
                {
                    db = new DialogBox(new Vector2(hPos, 4f), new Vector2(2f, 2), "");
                    db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
                    db.AddBorder();
                }
                hPos += 2.5f;
            }
        }

        new TextButton(new Vector2(UIScaler.GetHCenter(), 2.75f), new Vector2(5f, 2f), "Guess", delegate { Guess(); });

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(6.5f), 1.5f), new Vector2(6f, 2f), "Skill:");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(8f), 4f), new Vector2(3f, 2f), EventManager.SymbolReplace(questPuzzle.skill));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        // Guesses window
        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-13.5f), 6.5f), new Vector2(27, 13f), "");
        db.AddBorder();
        db.background.AddComponent<UnityEngine.UI.Mask>();
        UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

        GameObject scrollArea = new GameObject("scroll");
        RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
        scrollArea.transform.parent = db.background.transform;
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (1 + (puzzle.guess.Count * 2.5f)) * UIScaler.GetPixelsPerUnit());
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 27 * UIScaler.GetPixelsPerUnit());

        scrollRect.content = scrollInnerRect;
        scrollRect.horizontal = false;

        float vPos = 7f;
        foreach (PuzzleCode.CodeGuess g in puzzle.guess)
        {
            hPos = UIScaler.GetHCenter(-13f);
            foreach (int i in g.guess)
            {
                db = new DialogBox(new Vector2(hPos, vPos), new Vector2(2f, 2f), i.ToString());
                db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
                db.background.transform.parent = scrollArea.transform;
                db.AddBorder();
                hPos += 2.5f;
            }

            hPos = UIScaler.GetHCenter();
            for (int i = 0; i < g.CorrectSpot(); i++)
            {
                db = new DialogBox(new Vector2(hPos, vPos), new Vector2(2f, 2f), "");
                db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
                db.background.transform.parent = scrollArea.transform;
                db.AddBorder();
                hPos += 2.5f;
            }
            for (int i = 0; i < g.CorrectType(); i++)
            {
                db = new DialogBox(new Vector2(hPos, vPos), new Vector2(2f, 2f), "");
                db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
                db.background.transform.parent = scrollArea.transform;
                db.AddBorder();
                hPos += 2.5f;
            }
            vPos += 2.5f;
        }
        scrollRect.verticalNormalizedPosition = 0f;

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-11f), 20f), new Vector2(6f, 2f), "Moves:");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-5f), 20f), new Vector2(3f, 2f), (puzzle.guess.Count - previousMoves).ToString());
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-2f), 20f), new Vector2(10f, 2f), "Total Moves:");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(8f), 20f), new Vector2(3f, 2f), puzzle.guess.Count.ToString());
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        if (puzzle.Solved())
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-13f), 23.5f), new Vector2(8f, 2), "Close", delegate {; }, Color.grey);
            new TextButton(new Vector2(UIScaler.GetHCenter(5f), 23.5f), new Vector2(8f, 2), eventData.GetButtons()[0].label, delegate { Finished(); });
        }
        else
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-13f), 23.5f), new Vector2(8f, 2), "Close", delegate { Close(); });
            new TextButton(new Vector2(UIScaler.GetHCenter(5f), 23.5f), new Vector2(8f, 2), eventData.GetButtons()[0].label, delegate {; }, Color.grey);
        }
    }

    public void GuessAdd(int symbolType)
    {
        if (guess.Count >= questPuzzle.puzzleLevel)
        {
            return;
        }
        float hPos = UIScaler.GetHCenter(-13f) + (guess.Count * 2.5f);
        guess.Add(symbolType);

        int tmp = guess.Count - 1;
        new TextButton(new Vector2(hPos, 4f), new Vector2(2f, 2f), symbolType.ToString(), delegate { GuessRemove(tmp); });
    }

    public void GuessRemove(int symbolPos)
    {
        guess.RemoveAt(symbolPos);
        CreateWindow();
    }

    public void Guess()
    {
        if (guess.Count < questPuzzle.puzzleLevel)
        {
            return;
        }
        puzzle.AddGuess(guess);
        guess = new List<int>();
        CreateWindow();
    }

    public void Close()
    {
        Destroyer.Dialog();
        Game game = Game.Get();
        if (game.quest.puzzle.ContainsKey(questPuzzle.name))
        {
            game.quest.puzzle.Remove(questPuzzle.name);
        }
        game.quest.puzzle.Add(questPuzzle.name, puzzle);

        game.quest.eManager.currentEvent = null;
        game.quest.eManager.currentEvent = null;
        game.quest.eManager.TriggerEvent();
    }

    public void Finished()
    {
        Destroyer.Dialog();
        Game game = Game.Get();
        if (game.quest.puzzle.ContainsKey(questPuzzle.name))
        {
            game.quest.puzzle.Remove(questPuzzle.name);
        }

        game.quest.eManager.EndEvent();
    }
}
