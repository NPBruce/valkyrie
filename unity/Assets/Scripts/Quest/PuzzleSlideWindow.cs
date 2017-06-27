using UnityEngine;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

public class PuzzleSlideWindow
{

    public EventManager.Event eventData;
    QuestData.Puzzle questPuzzle;
    public PuzzleSlide puzzle;
    public int lastMoves = 0;

    public PuzzleSlideWindow(EventManager.Event e)
    {
        eventData = e;
        Game game = Game.Get();

        questPuzzle = e.qEvent as QuestData.Puzzle;

        if (game.quest.puzzle.ContainsKey(questPuzzle.sectionName))
        {
            puzzle = game.quest.puzzle[questPuzzle.sectionName] as PuzzleSlide;
            lastMoves = puzzle.moves;
        }
        else
        {
            puzzle = new PuzzleSlide(questPuzzle.puzzleLevel);
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
        GameObject background = new GameObject("puzzleContent");
        background.tag = Game.DIALOG;
        RectTransform transBg = background.AddComponent<RectTransform>();
        background.transform.SetParent(Game.Get().uICanvas.transform);
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, UIScaler.GetPixelsPerUnit() * 2.5f, 18f * UIScaler.GetPixelsPerUnit());
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, UIScaler.GetHCenter(-12f) * UIScaler.GetPixelsPerUnit(), 24f * UIScaler.GetPixelsPerUnit());

        DrawSlideFrame(background.transform);

        foreach (PuzzleSlide.Block b in puzzle.puzzle)
        {
            CreateBlock(b, transBg, b.target);
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
        ui.SetLocation(UIScaler.GetHCenter(8.5f), 14.5f, 3, 2);
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

    public void DrawSlideFrame(Transform trans, float scale = 3f)
    {
        GameObject[] bLine = new GameObject[8];
        // create 4 lines
        for (int i = 0; i < 8; i++)
        {
            bLine[i] = new GameObject("PuzzleFrame" + i);
            bLine[i].tag = Game.DIALOG;
            bLine[i].AddComponent<RectTransform>();
            bLine[i].AddComponent<CanvasRenderer>();
            bLine[i].transform.SetParent(trans);
            UnityEngine.UI.Image blImage = bLine[i].AddComponent<UnityEngine.UI.Image>();
            blImage.color = Color.white;
        }

        // Set the thickness of the lines
        float thick = 0.05f * UIScaler.GetPixelsPerUnit();

        bLine[0].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, -thick, thick);
        bLine[0].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -thick, (scale * 6f * UIScaler.GetPixelsPerUnit()) + (2 * thick));

        bLine[1].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -thick, thick);
        bLine[1].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -thick, (scale * 6f * UIScaler.GetPixelsPerUnit()) + (2 * thick));

        bLine[2].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -thick, thick);
        bLine[2].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -thick, (scale * 6f * UIScaler.GetPixelsPerUnit()) + (2 * thick));

        bLine[3].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, (scale * 2f * UIScaler.GetPixelsPerUnit()) - thick, thick);
        bLine[3].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -thick, (scale * 2f * UIScaler.GetPixelsPerUnit()) + thick);

        bLine[4].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, (scale * 2f * UIScaler.GetPixelsPerUnit()) - thick, thick);
        bLine[4].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, -thick, (scale * 3f * UIScaler.GetPixelsPerUnit()) + thick);

        bLine[5].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, -thick, thick);
        bLine[5].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (scale * 2f * UIScaler.GetPixelsPerUnit()) - thick, (scale * UIScaler.GetPixelsPerUnit()) + (2 * thick));

        bLine[6].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (scale * 2f * UIScaler.GetPixelsPerUnit()) - thick, thick);
        bLine[6].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, -thick, (scale * 2f * UIScaler.GetPixelsPerUnit()) + thick);

        bLine[7].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, (scale * 3f * UIScaler.GetPixelsPerUnit()) - thick, thick);
        bLine[7].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, -thick, (scale * 2f * UIScaler.GetPixelsPerUnit()) + thick);
    }

    public void CreateBlock(PuzzleSlide.Block block, RectTransform pos, bool target = false)
    {
        Color borderColour = Color.yellow;
        Color bgColour = new Color(0.6f, 0.6f, 0f, 1f);

        // Create object
        GameObject blockGO = new GameObject("puzzleBlock");
        if (block.target)
        {
            borderColour = Color.red;
            bgColour = new Color(0.8f, 0.0f, 0f, 1f);
        }
        blockGO.tag = Game.DIALOG;

        //Game game = Game.Get();
        blockGO.transform.SetParent(pos);

        RectTransform transBg = blockGO.AddComponent<RectTransform>();
        transBg.pivot = Vector2.up;
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (block.ypos * 3f * UIScaler.GetPixelsPerUnit()) + 0.1f, ((block.ylen + 1) * 3f * UIScaler.GetPixelsPerUnit()) - 0.2f);
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (block.xpos * 3f * UIScaler.GetPixelsPerUnit()) + 0.1f, ((block.xlen + 1) * 3f * UIScaler.GetPixelsPerUnit()) - 0.2f);
        blockGO.AddComponent<CanvasRenderer>();

        new UIElementBorder(blockGO.transform, transBg, Game.DIALOG, borderColour);

        UnityEngine.UI.Image uiImage = blockGO.AddComponent<UnityEngine.UI.Image>();
        uiImage.color = bgColour;

        BlockSlider slider = blockGO.AddComponent<BlockSlider>();
        slider.block = block;
        slider.win = this;
    }
}

public class BlockSlider : MonoBehaviour
{
    public bool sliding = false;
    public Vector2 mouseStart;
    public Vector2 transStart;
    public PuzzleSlide.Block block;
    public PuzzleSlideWindow win;
    RectTransform trans;

    // Use this for initialization (called at creation)
    void Start ()
    {
        trans = gameObject.GetComponent<RectTransform>();
        // Get the image attached to this game object
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!sliding && !Input.GetMouseButtonDown(0))
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (Input.mousePosition.x < trans.position.x) return;
            if (Input.mousePosition.y < trans.position.y - trans.rect.height) return;
            if (Input.mousePosition.x > trans.position.x + trans.rect.width) return;
            if (Input.mousePosition.y > trans.position.y) return;
            sliding = true;
            mouseStart = Input.mousePosition;
            transStart = trans.anchoredPosition;
        }

        if (!sliding)
        {
            return;
        }

        if (block.rotation)
        {
            float yTarget = -transStart.y + mouseStart.y - Input.mousePosition.y;
            float yTargetSq = yTarget / (3f * UIScaler.GetPixelsPerUnit());
            int yLimit = GetNegativeLimit();
            if (yTargetSq < yLimit)
            {
                yTargetSq = yLimit;
            }
            yLimit = GetPositiveLimit();
            if (yTargetSq > yLimit)
            {
                yTargetSq = yLimit;
            }
            yTarget = (yTargetSq * 3f * UIScaler.GetPixelsPerUnit());
            float nearestFit = (yTargetSq * 3f * UIScaler.GetPixelsPerUnit());
            if (Mathf.Abs(yTarget - nearestFit) < (UIScaler.GetPixelsPerUnit() * 1f))
            {
                yTarget = nearestFit;
            }
            Vector3 pos = trans.anchoredPosition;
            pos.y = -yTarget;
            trans.anchoredPosition = pos;
        }
        else
        {
            float xTarget = transStart.x + Input.mousePosition.x - mouseStart.x;
            float xTargetSq = xTarget / (3f * UIScaler.GetPixelsPerUnit());
            int xLimit = GetNegativeLimit();
            if (xTargetSq < xLimit)
            {
                xTargetSq = xLimit;
            }
            xLimit = GetPositiveLimit();
            if (xTargetSq > xLimit)
            {
                xTargetSq = xLimit;
            }
            xTarget = xTargetSq * 3f * UIScaler.GetPixelsPerUnit();
            float nearestFit = Mathf.Round(xTargetSq) * 3f * UIScaler.GetPixelsPerUnit();
            if (Mathf.Abs(xTarget - nearestFit) < (UIScaler.GetPixelsPerUnit() * 1f))
            {
                xTarget = nearestFit;
            }
            Vector3 pos = trans.anchoredPosition;
            pos.x = xTarget;
            trans.anchoredPosition = pos;
        }

        if (!Input.GetMouseButton(0))
        {
            sliding = false;
            int newXPos = Mathf.RoundToInt(trans.anchoredPosition.x / (3f * UIScaler.GetPixelsPerUnit()));
            int newYPos = Mathf.RoundToInt(-trans.anchoredPosition.y / (3f * UIScaler.GetPixelsPerUnit()));
            if (newXPos != block.xpos || newYPos != block.ypos)
            {
                win.puzzle.moves++;
                block.xpos = newXPos;
                block.ypos = newYPos;
            }
            // Update
            win.CreateWindow();
        }
    }

    public int GetNegativeLimit()
    {
        int posx = block.xpos;
        int posy = block.ypos;

        do
        {
            if (block.rotation)
            {
                posy--;
            }
            else
            {
                posx--;
            }
        } while (PuzzleSlide.Empty(win.puzzle.puzzle, posx, posy));

        if (block.rotation)
        {
            return posy + 1;
        }
        if (block.target && posx == 4)
        {
            return 6;
        }
        return posx + 1;
    }

    public int GetPositiveLimit()
    {
        int posx = block.xpos;
        int posy = block.ypos;

        if (block.rotation)
        {
            posy += block.ylen + 1;
        }
        else
        {
            posx += block.xlen + 1;
        }
        
        while (PuzzleSlide.Empty(win.puzzle.puzzle, posx, posy))
        {
            if (block.rotation)
            {
                posy++;
            }
            else
            {
                posx++;
            }
        }

        if (block.rotation)
        {
            return posy - (1 + block.ylen);
        }
        return posx - (1 + block.xlen);
    }
}
