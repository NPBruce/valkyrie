using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class PuzzleSlideWindow {
    public EventManager.Event eventData;
    QuestData.Puzzle questPuzzle;
    public PuzzleSlide puzzle;

    public PuzzleSlideWindow(EventManager.Event e)
    {
        eventData = e;
        Game game = Game.Get();

        questPuzzle = e.qEvent as QuestData.Puzzle;

        if (game.quest.puzzle.ContainsKey(questPuzzle.name))
        {
            puzzle = game.quest.puzzle[questPuzzle.name];
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
        DialogBox db = new DialogBox(new Vector2( UIScaler.GetHCenter(-14f), 0.5f), new Vector2(28f, 22f), "");
        db.AddBorder();


        // Puzzle goes here
        GameObject background = new GameObject("puzzleContent");
        RectTransform transBg = background.AddComponent<RectTransform>();
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, UIScaler.GetPixelsPerUnit(), 18f * UIScaler.GetPixelsPerUnit());
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, UIScaler.GetHCenter(-12f) * UIScaler.GetPixelsPerUnit(), 24f * UIScaler.GetPixelsPerUnit());

        DrawSlideFrame(transBg);

        foreach (PuzzleSlide.Block b in puzzle.puzzle)
        {
            CreateBlock(b, transBg, b.target);
        }

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

    public void DrawSlideFrame(RectTransform pos, float scale = 3f)
    {
        GameObject[] bLine = new GameObject[8];
        // create 4 lines
        for (int i = 0; i < 8; i++)
        {
            bLine[i] = new GameObject("PuzzleFrame" + i);
            bLine[i].tag = "dialog";
            bLine[i].AddComponent<RectTransform>();
            bLine[i].AddComponent<CanvasRenderer>();
            bLine[i].transform.SetParent(pos);
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
        public GameObject blockGO;
        public RectangleBorder border;
        public Color borderColour = Color.yellow;
        public Color bgColour = new Color(0.6f, 0.6f, 0f, 1f);

       // Create object
       GameObject blockGO = new GameObject("puzzleBlock");
        if (target)
        {
            borderColour = Color.red;
            bgColour = new Color(0.8f, 0.0f, 0f, 1f);
        }
        border = new RectangleBorder(blockGO.transform, borderColour, new Vector2(block.xlen, block.ylen));

        blockGO.tag = "dialog";

        Game game = Game.Get();
        blockGO.transform.parent = pos;

        RectTransform transBg = blockGO.AddComponent<RectTransform>();
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (location.y * 3f * UIScaler.GetPixelsPerUnit()) + 0.1f, (size.y * 3f * UIScaler.GetPixelsPerUnit()) - 0.2f);
        transBg.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (location.x * 3f * UIScaler.GetPixelsPerUnit()) + 0.1f, (size.x * 3f * UIScaler.GetPixelsPerUnit()) - 0.2f);
        blockGO.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Image uiImage = blockGO.AddComponent<UnityEngine.UI.Image>();
        uiImage.color = bgColour;

        BlockSlider slider = blockGO.AddComponent<BlockSlider>();
        slider.block = block;
        slider.win = this;

        UnityEngine.UI.Button uiButton = blockGO.AddComponent<UnityEngine.UI.Button>();
        uiButton.interactable = true;
        uiButton.onClick.AddListener(delegate { slider.Click(); });
    }
}

public BlockSlider : MonoBehaviour {

    RectTransform trans;
    bool sliding = false;
    Vector2 mouseStart;
    PuzzleSlide.Block block;
    PuzzleSlideWindow win;

	// Use this for initialization (called at creation)
	void Start ()
    {
        // Get the image attached to this game object
        trans = gameObject.transform
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!sliding)
        {
            return;
        }
        // FIXME collisions
        if (block.rotation)
        {
            float yTarget = trans.position.y + Input.mousePosition.y - mouseStart.y;
            float yLimit = (GetNegativeLimit() * 3f * UIScaler.GetPixelsPerUnit()) + 0.1f;
            if (yTarget < yLimit)
            {
                yTarget = yLimit;
            }
            float yLimit = (GetPositiveLimit() * 3f * UIScaler.GetPixelsPerUnit()) + 0.1f;
            if (yTarget > yLimit)
            {
                yTarget = yLimit;
            }
            float nearestFit = Round((yTarget - 0.1f) / (3f * UIScaler.GetPixelsPerUnit())) + 0.1f;
            if (Math.Abs(yTarget - nearestFit) < 0.5f)
            {
                yTarget = nearestFit;
            }
            trans.Traslate(Vector3.down * yTarget);
        }
        else
        {
            float xTarget = trans.position.x + Input.mousePosition.x - mouseStart.x;
            float xLimit = (GetNegativeLimit() * 3f * UIScaler.GetPixelsPerUnit()) + 0.1f;
            if (xTarget < xLimit)
            {
                xTarget = xLimit;
            }
            float xLimit = (GetPositiveLimit() * 3f * UIScaler.GetPixelsPerUnit()) + 0.1f;
            if (xTarget > xLimit)
            {
                xTarget = xLimit;
            }
            float nearestFit = Round((xTarget - 0.1f) / (3f * UIScaler.GetPixelsPerUnit())) + 0.1f;
            if (Math.Abs(xTarget - nearestFit) < 0.5f)
            {
                xTarget = nearestFit;
            }
            trans.Traslate(Vector3.right * xTarget);
        }

        if (!Input.GetMouseButton(0))
        {
            sliding = false;
            block.posx = RoundToInt((trans.position.x - 0.1f) / (3f * UIScaler.GetPixelsPerUnit()));
            block.posy = RoundToInt((trans.position.y - 0.1f) / (3f * UIScaler.GetPixelsPerUnit()));
            // Update
            win.CreateWindow();
        }
    }

    public int GetNegativeLimit()
    {
        int posx = block.xpos;
        int posy = block.xpos;

        do
        {
            if (rotation)
            {
                posy--;
            }
            else
            {
                posx--;
            }
        } while (PuzzleSlide.Empty(win.puzzle.puzzle, posx, posy);

        if (rotation)
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
        int posy = block.xpos;

        if (rotation)
        {
            posy += length + 1;
        }
        else
        {
            posx += length + 1;
        }
        
        while (PuzzleSlide.Empty(win.puzzle.puzzle, posx, posy)
        {
            if (rotation)
            {
                posy++;
            }
            else
            {
                posx++;
            }
        }

        if (rotation)
        {
            return posy - 1;
        }
        return posx - 1;
    }

    public int GetNegativeLimit()
    {
        int = 0;

    }

    public void Click()
    {
        sliding = true;
        mouseStart = Input.mousePosition;
    }
}