using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PuzzleSlideSolver
{
    public Queue<List<PuzzleSlide.Block>> queue;
    public Dictionary<List<PuzzleSlide.Block>, List<PuzzleSlide.Block>> pred;

    public PuzzleSlideSolver()
    {
        queue = new Queue<List<PuzzleSlide.Block>>();
        pred = new Dictionary<List<PuzzleSlide.Block>, List<PuzzleSlide.Block>>();
    }

    public int Solve(List<PuzzleSlide.Block> current, int moves)
    {
        Propose(current, null);
        int i;
        for (i = 0; i < moves; i++)
        {
            List<PuzzleSlide.Block> fromQueue = queue.Dequeue();
            if (AtGoal(fromQueue))
            {
                return i;
            }
            Explore(fromQueue);
        }
        return i;
    }

    public void Explore(List<PuzzleSlide.Block> current)
    {
        foreach (PuzzleSlide.Block b in current)
        {
            Slide(current, b, -1);
            Slide(current, b, 1);
        }
    }

    public void Slide(List<PuzzleSlide.Block> current, PuzzleSlide.Block b, int dir)
    {
        List<PuzzleSlide.Block> newState = new List<PuzzleSlide.Block>(current);
        while(CheckMove(newState, b, dir))
        {
            newState.Remove(b);

            PuzzleSlide.Block newBlock = new PuzzleSlide.Block(b);
            if (b.rotation)
            {
                newBlock.ypos += dir;
            }
            else
            {
                newBlock.xpos += dir;
            }
            newState.Add(b);
            Propose(newState, current);
            newState = new List<PuzzleSlide.Block>(newState);
        }
    }

    public void Propose(List<PuzzleSlide.Block> next, List<PuzzleSlide.Block> prev)
    {
        if (!pred.ContainsKey(next))
        {
            pred.Add(next, prev);
            queue.Enqueue(next);
        }
    }

    public bool CheckMove(List<PuzzleSlide.Block> state, PuzzleSlide.Block b, int dir)
    {
        Vector2 target = b.GetMove(dir);
        return PuzzleSlide.Empty(state, Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y));
    }

    public bool AtGoal(List<PuzzleSlide.Block> state)
    {
        foreach (PuzzleSlide.Block b in state)
        {
            if (b.target)
            {
                if (b.Blocks(5,2))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        return false;
    }

    public PuzzleSlide.Block AtLocation(List<PuzzleSlide.Block> state, int x, int y)
    {
        if (x < 0 || y < 0 || x > 5 || y > 5)
        {
            return null;
        }
        foreach (PuzzleSlide.Block b in state)
        {
            if (b.Blocks(x,y))
            {
                return b;
            }
        }
        return null;
    }
}
