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
        UIElement ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-14f), 0.5f, 28, 22);
        new UIElementBorder(ui);

        // Puzzle goes here
        float hPos = UIScaler.GetHCenter(-13f);
        if (!puzzle.Solved())
        {
            for (int i = 1; i <= questPuzzle.puzzleAltLevel; i++)
            {
                int tmp = i;
                ui = new UIElement();
                ui.SetLocation(hPos, 1.5f, 2, 2);
                ui.SetText(buttons[i].label, Color.black);
                ui.SetFontSize(UIScaler.GetMediumFont());
                ui.SetButton(delegate { GuessAdd(tmp); });
                ui.SetImage(buttons[i].image);
                ui.SetBGColor(new Color(1, 1, 1, 0.9f));
                hPos += 2.5f;
            }
            hPos = UIScaler.GetHCenter(-13f);
            for (int i = 1; i <= questPuzzle.puzzleLevel; i++)
            {
                if (guess.Count >= i)
                {
                    int tmp = i - 1;
                    ui = new UIElement();
                    ui.SetLocation(hPos, 4, 2, 2);
                    ui.SetText(buttons[guess[tmp]].label, Color.black);
                    ui.SetFontSize(UIScaler.GetMediumFont());
                    ui.SetButton(delegate { GuessRemove(tmp); });
                    ui.SetImage(buttons[guess[tmp]].image);
                    ui.SetBGColor(new Color(1, 1, 1, 0.9f));
                }
                else
                {
                    ui = new UIElement();
                    ui.SetLocation(hPos, 4, 2, 2);
                    new UIElementBorder(ui);
                }
                hPos += 2.5f;
            }
        }

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(), 2.75f, 5, 2);
        ui.SetText(PUZZLE_GUESS);
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Guess);
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(6.5f), 1.5f, 6, 2);
        ui.SetText(new StringKey("val","X_COLON",CommonStringKeys.SKILL));
        ui.SetFontSize(UIScaler.GetMediumFont());

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(8), 4, 3, 2);
        ui.SetText(EventManager.OutputSymbolReplace(questPuzzle.skill));
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui);

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
                ui = new UIElement(scrollArea.GetScrollTransform());
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
                ui = new UIElement(scrollArea.GetScrollTransform());
                ui.SetLocation(hPos, vPos, 2, 2);
                ui.SetText(ICON_SUCCESS_RESULT, Color.black);
                ui.SetBGColor(new Color(1, 1, 1, 0.9f));
                ui.SetFontSize(UIScaler.GetMediumFont());
                new UIElementBorder(ui);
                hPos += 2.5f;
            }
            for (int i = 0; i < g.CorrectType(); i++)
            {
                ui = new UIElement(scrollArea.GetScrollTransform());
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

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-11f), 20f, 6, 2);
        ui.SetText(new StringKey("val","X_COLON",CommonStringKeys.MOVES));
        ui.SetFontSize(UIScaler.GetMediumFont());

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-5), 20, 3, 2);
        ui.SetText((puzzle.guess.Count - previousMoves).ToString());
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-2f), 20f, 10, 2);
        ui.SetText(new StringKey("val","X_COLON",CommonStringKeys.TOTAL_MOVES));
        ui.SetFontSize(UIScaler.GetMediumFont());

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(8), 20, 3, 2);
        ui.SetText(puzzle.guess.Count.ToString());
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-13), 23.5f, 8, 2);
        if (puzzle.Solved())
        {
            ui.SetText(CommonStringKeys.CLOSE, Color.grey);
            new UIElementBorder(ui, Color.grey);
        }
        else
        {
            ui.SetText(CommonStringKeys.CLOSE);
            new UIElementBorder(ui);
            ui.SetButton(Close);
        }
        ui.SetFontSize(UIScaler.GetMediumFont());

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(5), 23.5f, 8, 2);
        if (!puzzle.Solved())
        {
            ui.SetText(eventData.GetButtons()[0].GetLabel(), Color.grey);
            new UIElementBorder(ui, Color.grey);
        }
        else
        {
            ui.SetText(eventData.GetButtons()[0].GetLabel());
            new UIElementBorder(ui);
            ui.SetButton(Finished);
        }
        ui.SetFontSize(UIScaler.GetMediumFont());
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
        UIElement ui = new UIElement();
        ui.SetLocation(hPos, 4, 2, 2);
        ui.SetText(buttons[symbolType].label, Color.black);
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(delegate { GuessRemove(tmp); });
        ui.SetImage(buttons[symbolType].image);
        ui.SetBGColor(new Color(1, 1, 1, 0.9f));
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
                    buttons.Add(new ButtonInfo(Sprite.Create(dupeTex, new Rect(0, 0, dupeTex.width, dupeTex.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect)));
                }
                else
                {
                    buttons.Add(new ButtonInfo(new StringKey(null, i.ToString(), false)));
                }
            }
            else if (questPuzzle.imageType.Equals("element"))
            {
                Texture2D dupeTex = Resources.Load("sprites/element" + i) as Texture2D;
                if (dupeTex != null)
                {
                    buttons.Add(new ButtonInfo(Sprite.Create(dupeTex, new Rect(0, 0, dupeTex.width, dupeTex.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect)));
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
