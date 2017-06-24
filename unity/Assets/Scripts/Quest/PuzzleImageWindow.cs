using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

public class PuzzleImageWindow
{

    public EventManager.Event eventData;
    QuestData.Puzzle questPuzzle;
    public PuzzleImage puzzle;
    public int previousMoves = 0;
    public Sprite[][] imageSprite;
    public float width = 1f;
    public float height = 1f;

    public PuzzleImageWindow(EventManager.Event e)
    {
        eventData = e;
        Game game = Game.Get();

        questPuzzle = e.qEvent as QuestData.Puzzle;

        if (game.quest.puzzle.ContainsKey(questPuzzle.sectionName))
        {
            puzzle = game.quest.puzzle[questPuzzle.sectionName] as PuzzleImage;
            previousMoves = puzzle.moves;
        }
        else
        {
            puzzle = new PuzzleImage(questPuzzle.puzzleLevel, questPuzzle.puzzleAltLevel);
        }

        height = 19f / questPuzzle.puzzleAltLevel;
        width = 19f / questPuzzle.puzzleLevel;

        Texture2D newTex = null;
        if (game.cd.puzzles.ContainsKey(questPuzzle.imageType))
        {
            newTex = ContentData.FileToTexture(game.cd.puzzles[questPuzzle.imageType].image);
        }
        else
        {
            newTex = ContentData.FileToTexture(System.IO.Path.GetDirectoryName(game.quest.qd.questPath) + "/" + questPuzzle.imageType);
        }
        if (newTex.width > newTex.height)
        {
            height = height * newTex.height / newTex.width;
        }
        else
        {
            width = width * newTex.width / newTex.height;
        }

        imageSprite = new Sprite[questPuzzle.puzzleLevel][];
        for (int i = 0; i < questPuzzle.puzzleLevel; i++)
        {
            imageSprite[i] = new Sprite[questPuzzle.puzzleAltLevel];
            for (int j = 0; j < questPuzzle.puzzleAltLevel; j++)
            {
                imageSprite[i][j] = Sprite.Create(newTex, new Rect(i * newTex.width / questPuzzle.puzzleLevel, (questPuzzle.puzzleAltLevel - (j + 1)) * newTex.height / questPuzzle.puzzleAltLevel, newTex.width / questPuzzle.puzzleLevel, newTex.height / questPuzzle.puzzleAltLevel), Vector2.zero, 1, 0, SpriteMeshType.FullRect);
            }
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
        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(10), 8, 3, 2);
        ui.SetText(new StringKey("val","X_COLON",CommonStringKeys.SKILL));
        ui.SetFontSize(UIScaler.GetMediumFont());

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(10), 10, 3, 2);
        ui.SetText(EventManager.OutputSymbolReplace(questPuzzle.skill));
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui);

        bool solved = puzzle.Solved();
        foreach (KeyValuePair<PuzzleImage.TilePosition, PuzzleImage.TilePosition> kv in puzzle.state)
        {
            Draw(kv.Key, kv.Value, solved);
        }

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-13f), 20f, 6, 2);
        ui.SetText(new StringKey("val","X_COLON",CommonStringKeys.MOVES));
        ui.SetFontSize(UIScaler.GetMediumFont());

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-6), 20, 3, 2);
        ui.SetText((puzzle.moves - previousMoves).ToString());
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-3f), 20f, 10, 2);
        ui.SetText(new StringKey("val","X_COLON",CommonStringKeys.TOTAL_MOVES));
        ui.SetFontSize(UIScaler.GetMediumFont());

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(7), 20, 3, 2);
        ui.SetText(puzzle.moves.ToString());
        ui.SetFontSize(UIScaler.GetMediumFont());
        new UIElementBorder(ui);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetHCenter(-13), 23.5f, 8, 2);
        if (solved)
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
        if (!solved)
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

    public void Draw(PuzzleImage.TilePosition screenPos, PuzzleImage.TilePosition imgPos, bool solved)
    {
        Game game = Game.Get();
        // Create object
        GameObject gameObject = new GameObject("PuzzleTile");
        gameObject.tag = Game.DIALOG;

        gameObject.transform.SetParent(game.uICanvas.transform);

        RectTransform trans = gameObject.AddComponent<RectTransform>();
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, UIScaler.GetPixelsPerUnit() + (UIScaler.GetPixelsPerUnit() * screenPos.y * height), height * UIScaler.GetPixelsPerUnit());
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (UIScaler.GetHCenter(-11.5f) * UIScaler.GetPixelsPerUnit()) + (UIScaler.GetPixelsPerUnit() * screenPos.x * width), width * UIScaler.GetPixelsPerUnit());
        gameObject.AddComponent<CanvasRenderer>();

        // Create the image
        UnityEngine.UI.Image image = gameObject.AddComponent<UnityEngine.UI.Image>();
        image.sprite = imageSprite[imgPos.x][imgPos.y];
        image.rectTransform.sizeDelta = new Vector2(width * UIScaler.GetPixelsPerUnit(), height * UIScaler.GetPixelsPerUnit());

        if (solved)
        {
            return;
        }
        BlockSlider slider = gameObject.AddComponent<BlockSlider>();
        slider.screenPos = screenPos;
        slider.win = this;
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

    public class BlockSlider : MonoBehaviour
    {
        public bool sliding = false;
        public Vector2 mouseStart;
        public Vector2 transStart;
        public PuzzleImage.TilePosition screenPos;
        public PuzzleImageWindow win;
        RectTransform trans;

        // Use this for initialization (called at creation)
        void Start()
        {
            trans = gameObject.GetComponent<RectTransform>();
            // Get the image attached to this game object
        }

        // Update is called once per frame
        void Update()
        {
            if (!sliding && !Input.GetMouseButtonDown(0))
            {
                return;
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (Input.mousePosition.x < trans.position.x - (trans.rect.width / 2f)) return;
                if (Input.mousePosition.y < trans.position.y - (trans.rect.height / 2f)) return;
                if (Input.mousePosition.x > trans.position.x + (trans.rect.width / 2f)) return;
                if (Input.mousePosition.y > trans.position.y + (trans.rect.height / 2f)) return;
                sliding = true;
                mouseStart = Input.mousePosition;
                transStart = trans.anchoredPosition;
            }

            if (!sliding)
            {
                return;
            }


            float yDiff = Input.mousePosition.y - mouseStart.y;
            float xDiff = Input.mousePosition.x - mouseStart.x;

            if (screenPos.x == 0 && xDiff < 0)
            {
                xDiff = 0;
            }

            if (screenPos.y == 0 && yDiff > 0)
            {
                yDiff = 0;
            }

            if (screenPos.x == (win.questPuzzle.puzzleLevel - 1) && xDiff > 0)
            {
                xDiff = 0;
            }

            if (screenPos.y == (win.questPuzzle.puzzleAltLevel - 1) && yDiff < 0)
            {
                yDiff = 0;
            }

            if (xDiff > (win.width * UIScaler.GetPixelsPerUnit()))
            {
                xDiff = (win.width * UIScaler.GetPixelsPerUnit());
            }
            if (xDiff < -(win.width * UIScaler.GetPixelsPerUnit()))
            {
                xDiff = -(win.width * UIScaler.GetPixelsPerUnit());
            }

            if (yDiff > (win.height * UIScaler.GetPixelsPerUnit()))
            {
                yDiff = (win.height * UIScaler.GetPixelsPerUnit());
            }
            if (yDiff < -(win.height * UIScaler.GetPixelsPerUnit()))
            {
                yDiff = -(win.height * UIScaler.GetPixelsPerUnit());
            }
            if (Mathf.Abs(yDiff) > Mathf.Abs(xDiff))
            {
                xDiff = 0;
            }
            else
            {
                yDiff = 0;
            }

            Vector3 pos = new Vector3(transStart.x, transStart.y, 0);
            pos.x += xDiff;
            pos.y += yDiff;
            trans.anchoredPosition = pos;
            trans.SetAsLastSibling();

            if (!Input.GetMouseButton(0))
            {
                sliding = false;
                int Xshift = Mathf.RoundToInt(xDiff / (win.width * UIScaler.GetPixelsPerUnit()));
                int Yshift = -Mathf.RoundToInt(yDiff / (win.height * UIScaler.GetPixelsPerUnit()));
                if (Yshift != 0 || Xshift != 0)
                {
                    win.puzzle.moves++;
                    PuzzleImage.TilePosition swap = null;
                    foreach (KeyValuePair<PuzzleImage.TilePosition, PuzzleImage.TilePosition> kv in win.puzzle.state)
                    {
                        if ((kv.Key.x == screenPos.x + Xshift) && (kv.Key.y == screenPos.y + Yshift))
                        {
                            swap = kv.Key;
                        }
                    }
                    swap.x = screenPos.x;
                    swap.y = screenPos.y;
                    screenPos.x += Xshift;
                    screenPos.y += Yshift;
                    win.CreateWindow();
                }
                else
                {
                    pos = new Vector3(transStart.x, transStart.y, 0);
                    trans.anchoredPosition = pos;
                }
            }
        }
    }
}
