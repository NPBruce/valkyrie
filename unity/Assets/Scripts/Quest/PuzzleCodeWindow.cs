using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

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
    public List<ButtonInfo> buttons;


    public PuzzleCodeWindow(EventManager.Event e)
    {
        eventData = e;
        Game game = Game.Get();

        guess = new List<int>();
        questPuzzle = e.qEvent as QuestData.Puzzle;
        buttons = GetButtons();

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
                    new Vector2(hPos, 1.5f), new Vector2(2f, 2), buttons[i].label, delegate { GuessAdd(tmp); }, Color.black);
                tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, (float)0.9);
                tb.background.GetComponent<UnityEngine.UI.Image>().sprite = buttons[i].image;
                hPos += 2.5f;
            }
            hPos = UIScaler.GetHCenter(-13f);
            for (int i = 1; i <= questPuzzle.puzzleLevel; i++)
            {
                if (guess.Count >= i)
                {
                    int tmp = i - 1;
                    tb = new TextButton(
                        new Vector2(hPos, 4f), new Vector2(2f, 2f), buttons[guess[tmp]].label, 
                        delegate { GuessRemove(tmp); }, Color.black);
                    tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, (float)0.9);
                    tb.background.GetComponent<UnityEngine.UI.Image>().sprite = buttons[guess[tmp]].image;
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
            new StringKey(null, EventManager.OutputSymbolReplace(questPuzzle.skill), false));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        // Guesses window
        UIElementScrollVertical scrollArea = new UIElementScrollVertical();
        scrollArea.SetLocation(UIScaler.GetHCenter(-13.5f), 6.5f, 27, 13f);
        new UIElementBorder(scrollArea);
        scrollArea.SetScrollSize(1 + (puzzle.guess.Count * 2.5f));

        float vPos = 0.5f;
        foreach (PuzzleCode.CodeGuess g in puzzle.guess)
        {
            hPos = 0.5f;
            foreach (int i in g.guess)
            {
                UIElement ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(hPos, vPos, 2, 2);
                ui.SetText(buttons[i].label, Color.black);
                ui.SetBGColor(new Color(1, 1, 1, 0.9f));
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetImage(buttons[i].image);
                new UIElementBorder(ui);
                hPos += 2.5f;
            }

            hPos = 13.25f;
            for (int i = 0; i < g.CorrectSpot(); i++)
            {
                UIElement ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(hPos, vPos, 2, 2);
                ui.SetText(ICON_SUCCESS_RESULT, Color.black);
                ui.SetBGColor(new Color(1, 1, 1, 0.9f));
                ui.SetFontSize(UIScaler.GetMediumFont());
                new UIElementBorder(ui);
                hPos += 2.5f;
            }
            for (int i = 0; i < g.CorrectType(); i++)
            {
                UIElement ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(hPos, vPos, 2, 2);
                ui.SetText(ICON_INVESTIGATION_RESULT, Color.black);
                ui.SetBGColor(new Color(1, 1, 1, 0.9f));
                ui.SetFontSize(UIScaler.GetMediumFont());
                new UIElementBorder(ui);
                hPos += 2.5f;
            }
            vPos += 2.5f;
        }
        scrollArea.SetScrollPosition(1 + (puzzle.guess.Count * 2.5f) * UIScaler.GetPixelsPerUnit());

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-11f), 20f), new Vector2(6f, 2f),
            new StringKey("val", "X_COLON", CommonStringKeys.MOVES));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-5f), 20f), new Vector2(3f, 2f), puzzle.guess.Count - previousMoves);
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(-2f), 20f), new Vector2(10f, 2f),
            new StringKey("val", "X_COLON", CommonStringKeys.TOTAL_MOVES));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();

        db = new DialogBox(new Vector2(UIScaler.GetHCenter(8f), 20f), new Vector2(3f, 2f), puzzle.guess.Count);
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();

        if (puzzle.Solved())
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-13f), 23.5f), new Vector2(8f, 2), CommonStringKeys.CLOSE, delegate {; }, Color.grey);
            new TextButton(
                new Vector2(UIScaler.GetHCenter(5f), 23.5f), new Vector2(8f, 2), 
                eventData.GetButtons()[0].GetLabel(), delegate { Finished(); });
        }
        else
        {
            new TextButton(new Vector2(UIScaler.GetHCenter(-13f), 23.5f), new Vector2(8f, 2), CommonStringKeys.CLOSE, delegate { Close(); });
            new TextButton(
                new Vector2(UIScaler.GetHCenter(5f), 23.5f), new Vector2(8f, 2), 
                eventData.GetButtons()[0].GetLabel(), delegate {; }, Color.grey);
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
        TextButton tb = new TextButton(
            new Vector2(hPos, 4f), new Vector2(2f, 2f), 
            buttons[symbolType].label, 
            delegate { GuessRemove(tmp); }, Color.black);
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, (float)0.9);
        tb.background.GetComponent<UnityEngine.UI.Image>().sprite = buttons[symbolType].image;
    }

    public void GuessRemove(int symbolPos)
    {
        guess.RemoveAt(symbolPos);
        CreateWindow();
    }

    public List<ButtonInfo> GetButtons()
    {
        List<ButtonInfo> buttons = new List<ButtonInfo>();
        for (int i = 0; i <= questPuzzle.puzzleAltLevel; i++)
        {
            if (questPuzzle.imageType.Equals("symbol"))
            {
                Texture2D dupeTex = Resources.Load("sprites/monster_duplicate_" + i) as Texture2D;
                if (dupeTex != null)
                {
                    buttons.Add(new ButtonInfo(Sprite.Create(dupeTex, new Rect(0, 0, dupeTex.width, dupeTex.height), Vector2.zero, 1)));
                }
                else
                {
                    buttons.Add(new ButtonInfo(new StringKey(null, i.ToString(), false)));
                }
            }
            else
            {
                buttons.Add(new ButtonInfo(new StringKey(null, i.ToString(), false)));
            }
        }
        return buttons;
    }

    public class ButtonInfo
    {
        public Sprite image = null;
        public StringKey label = StringKey.NULL;

        public ButtonInfo(Sprite s)
        {
            image = s;
        }
        public ButtonInfo(StringKey l)
        {
            label = l;
        }
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
