using UnityEngine;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
using System.Collections.Generic;

public class PuzzleTowerWindow
{
    public EventManager.Event eventData;
    QuestData.Puzzle questPuzzle;
    public PuzzleTower puzzle;
    public int lastMoves = 0;

    public PuzzleTowerWindow(EventManager.Event e)
    {
        eventData = e;
        Game game = Game.Get();

        questPuzzle = e.qEvent as QuestData.Puzzle;

        if (game.quest.puzzle.ContainsKey(questPuzzle.sectionName))
        {
            puzzle = game.quest.puzzle[questPuzzle.sectionName] as PuzzleTower;
            lastMoves = puzzle.moves;
        }
        else
        {
            puzzle = new PuzzleTower(questPuzzle.puzzleLevel);
        }

        CreateWindow();
    }

    public void CreateWindow()
    {
        Destroyer.Dialog();
        UIElement ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-14f), 0.5f, 28, 22);
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-13.5f), 1, 9, 16.5f);
        new UIElementBorder(ui);
        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-4.5f), 1, 9, 16.5f);
        new UIElementBorder(ui);
        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(4.5f), 1, 9, 16.5f);
        new UIElementBorder(ui);

        for(int i = 0; i < puzzle.puzzle.Count; i++)
        {
            CreateTower(-9 + (i * 9), 17f, puzzle.puzzle[i]);
        }

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(6.5f), 3, 7, 2);
        ui.SetText(new StringKey("val","X_COLON",CommonStringKeys.SKILL));
        ui.SetFontSize(UIScaler.GetMediumFont());

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(8.5f), 5, 3, 2);
        ui.SetText(EventManager.OutputSymbolReplace(questPuzzle.skill));
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(6.5f), 12.5f, 7, 2);
        ui.SetText(new StringKey("val","X_COLON",CommonStringKeys.MOVES));
        ui.SetFontSize(UIScaler.GetMediumFont());

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-8.5f), 14.5f, 3, 2);
        ui.SetText((puzzle.moves - lastMoves).ToString());
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(6.5f), 17, 7, 2);
        ui.SetText(new StringKey("val","X_COLON",CommonStringKeys.TOTAL_MOVES));
        ui.SetFontSize(UIScaler.GetMediumFont());

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(8.5f), 19, 3, 2);
        ui.SetText(puzzle.moves.ToString());
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

    /// <summary>
    /// Draw a tower
    /// </summary>
    /// <param name="hCentre">Horizontal centre of the tower in UIScaler units</param>
    /// <param name="vBottom">Vertiacal bottom of the tower in UIScaler units</param>
    /// <param name="blocks">Block sizes to draw</param>
    public void CreateTower(float hCentre, float vBottom, List<int> blocks)
    {
        float offset = vBottom;
        foreach (int size in blocks)
        {
            CreateBlock(hCentre, vBottom, size);
            vBottom += 2;
        }
    }

    /// <summary>
    /// Draw a tower
    /// </summary>
    /// <param name="hCentre">Horizontal centre of the block in UIScaler units</param>
    /// <param name="vBottom">Vertiacal bottom of the block in UIScaler units</param>
    /// <param name="blocks">Block size to draw</param>
    public void CreateBlock(float hCentre, float vBottom, int size)
    {
        UIElement ui = new UIElement();
        ui.SetLocation(hCentre - ((size + 1.5f) / 2), vBottom - 1.5f, size + 1.5f, 1.5f);
        ui.SetBGColor(new Color(0.6f, 0.6f, 0f, 1f));
        new UIElementBorder(ui, Color.yellow);
    }
}
