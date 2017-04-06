using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Assets.Scripts.Content;

public class PuzzleCodeWindow
{
    private readonly StringKey PUZZLE_GUESS = new StringKey("val", "PUZZLE_GUESS");
    private readonly StringKey ICON_SUCCESS_RESULT = new StringKey("val","ICON_SUCCESS_RESULT");
    private readonly StringKey ICON_INVESTIGATION_RESULT = new StringKey("val", "ICON_INVESTIGATION_RESULT");

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

        if (game.quest.puzzle.ContainsKey(questPuzzle.sectionName))
        {
            puzzle = game.quest.puzzle[questPuzzle.sectionName] as PuzzleCode;
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
        DialogBox db = new DialogBox(
            new Vector2(UIScaler.GetHCenter(-14f), 0.5f), new Vector2(28f, 22f), StringKey.NULL);
        db.AddBorder();

        // Puzzle goes here
        TextButton tb = null;
        float hPos = UIScaler.GetHCenter(-13f);
        if (!puzzle.Solved())
        {
            for (int i = 1; i <= questPuzzle.puzzleAltLevel; i++)
            {
                int tmp = i;
                tb = new TextButton(
                    new Vector2(hPos, 1.5f), new Vector2(2f, 2), 
                    new StringKey(i.ToString(),false), 
                    delegate { GuessAdd(tmp); }, Color.black);
                tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, (float)0.9);
                hPos += 2.5f;
            }
            hPos = UIScaler.GetHCenter(-13f);
            for (int i = 1; i <= questPuzzle.puzzleLevel; i++)
            {
                if (guess.Count >= i)
                {
                    int tmp = i - 1;
                    tb = new TextButton(
                        new Vector2(hPos, 4f), new Vector2(2f, 2f), 
                        new StringKey(guess[tmp].ToString(),false), 
                        delegate { GuessRemove(tmp); }, Color.black);
                    tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, (float)0.9);
                }
                else
                {
                    db = new DialogBox(new Vector2(hPos, 4f), new Vector2(2f, 2), StringKey.NULL, Color.white);
                    db.AddBorder();
                }
                hPos += 2.5f;
            }
        }

        new TextButton(new Vector2(UIScaler.GetHCenter(), 2.75f), new Vector2(5f, 2f), PUZZLE_GUESS, delegate { Guess(); });

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(6.5f), 1.5f), new Vector2(6f, 2f), 
            new StringKey("val","X_COLON",CommonStringKeys.SKILL));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(8f), 4f), new Vector2(3f, 2f),
            new StringKey(EventManager.SymbolReplace(questPuzzle.skill), false));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        // Guesses window
        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-13.5f), 6.5f), new Vector2(27, 13f), StringKey.NULL);
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
                db = new DialogBox(
                    new Vector2(hPos, vPos), new Vector2(2f, 2f), 
                    new StringKey(i.ToString(),false), 
                    Color.black, new Color(1, 1, 1, 0.9f));
                db.textObj.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
                db.background.transform.parent = scrollArea.transform;
                db.AddBorder();
                hPos += 2.5f;
            }

            hPos = UIScaler.GetHCenter();
            for (int i = 0; i < g.CorrectSpot(); i++)
            {
                db = new DialogBox(new Vector2(hPos, vPos), new Vector2(2f, 2f), ICON_SUCCESS_RESULT, Color.black, new Color(1, 1, 1, 0.9f));
                db.textObj.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
                db.background.transform.parent = scrollArea.transform;
                db.AddBorder();
                hPos += 2.5f;
            }
            for (int i = 0; i < g.CorrectType(); i++)
            {
                db = new DialogBox(new Vector2(hPos, vPos), new Vector2(2f, 2f), ICON_INVESTIGATION_RESULT, Color.black, new Color(1, 1, 1, 0.9f));
                db.textObj.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
                db.background.transform.parent = scrollArea.transform;
                db.AddBorder();
                hPos += 2.5f;
            }
            vPos += 2.5f;
        }
        scrollRect.verticalNormalizedPosition = 0f;

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-11f), 20f), new Vector2(6f, 2f),
            new StringKey("val", "X_COLON", CommonStringKeys.MOVES));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-5f), 20f), new Vector2(3f, 2f), new StringKey((puzzle.guess.Count - previousMoves).ToString(),false));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-2f), 20f), new Vector2(10f, 2f),
            new StringKey("val", "X_COLON", CommonStringKeys.TOTAL_MOVES));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(8f), 20f), new Vector2(3f, 2f), new StringKey(puzzle.guess.Count.ToString(),false));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        if (puzzle.Solved())
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-13f), 23.5f), new Vector2(8f, 2), CommonStringKeys.CLOSE, delegate {; }, Color.grey);
            new TextButton(new Vector2(UIScaler.GetHCenter(5f), 23.5f), new Vector2(8f, 2), new StringKey(eventData.GetButtons()[0].label,false), delegate { Finished(); });
        }
        else
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-13f), 23.5f), new Vector2(8f, 2), CommonStringKeys.CLOSE, delegate { Close(); });
            new TextButton(new Vector2(UIScaler.GetHCenter(5f), 23.5f), new Vector2(8f, 2), new StringKey(eventData.GetButtons()[0].label,false), delegate {; }, Color.grey);
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
        new TextButton(
            new Vector2(hPos, 4f), new Vector2(2f, 2f), 
            new StringKey(symbolType.ToString(),false), 
            delegate { GuessRemove(tmp); });
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
        if (game.quest.puzzle.ContainsKey(questPuzzle.sectionName))
        {
            game.quest.puzzle.Remove(questPuzzle.sectionName);
        }
        game.quest.puzzle.Add(questPuzzle.sectionName, puzzle);

        game.quest.eManager.currentEvent = null;
        game.quest.eManager.currentEvent = null;
        game.quest.eManager.TriggerEvent();
    }

    public void Finished()
    {
        Destroyer.Dialog();
        Game game = Game.Get();
        if (game.quest.puzzle.ContainsKey(questPuzzle.sectionName))
        {
            game.quest.puzzle.Remove(questPuzzle.sectionName);
        }

        game.quest.eManager.EndEvent();
    }
}
