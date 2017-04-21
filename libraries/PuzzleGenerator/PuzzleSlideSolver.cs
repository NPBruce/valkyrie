using System;
using System.Collections;
using System.Collections.Generic;

public class PuzzleSlideSolver
{
    public Queue<int[]> queue;
    public Queue<int[]> nextQueue;
    public HashSet<int[]> pred;

    public PuzzleSlideSolver()
    {
    }

    public int Solve(List<PuzzleSlide.Block> current, int moves)
    {
        List<int> start = new List<int>();
        foreach (PuzzleSlide.Block b in current)
        {
            start.Add(b.data);
        }

        pred = new HashSet<int[]>();
        nextQueue = new Queue<int[]>();
        Propose(start.ToArray(), null);
        int i;
        for (i = 0; i <= moves; i++)
        {
            queue = nextQueue;
            nextQueue = new Queue<int[]>();
            foreach (int[] fromQueue in queue)
            {
                if (AtGoal(fromQueue))
                {
                    return i;
                }
            }
            foreach (int[] fromQueue in queue)
            {
                Explore(fromQueue);
            }
        }
        foreach (int[] fromQueue in nextQueue)
        {
            if (AtGoal(fromQueue))
            {
                return i;
            }
        }
        return i+1;
    }

    public void Explore(int[] current)
    {
        for (int i = 0; i < current.Length; i++)
        {
            Slide(current, i, -1);
            Slide(current, i, 1);
        }
    }

    public void Slide(int[] current, int b, int dir)
    {
        int[] newState = (int[])current.Clone();
        int newBlock = current[b];
        int oldBlock = current[b];
        while (CheckMove(newState, oldBlock, dir))
        {
            if (PuzzleSlide.Block.GetRotation(oldBlock))
            {
                newBlock = PuzzleSlide.Block.ChangeYPos(newBlock, dir);
            }
            else
            {
                newBlock = PuzzleSlide.Block.ChangeXPos(newBlock, dir);
            }
            newState[b] = newBlock;
            Propose(newState, current);
            newState = (int[])newState.Clone();
            oldBlock = newBlock;
        }
    }

    public void Propose(int[] next, int[] prev)
    {
        if (!pred.Contains(next))
        {
            pred.Add(next);
            nextQueue.Enqueue(next);
        }
    }

    public bool CheckMove(int[] state, int b, int dir)
    {
        Vector2 target = PuzzleSlide.Block.GetMove(b, dir, 1);
        return PuzzleSlide.Empty(state, (int)Math.Round(target.x), (int)Math.Round(target.y));
    }

    public bool AtGoal(int[] state)
    {
        foreach (int b in state)
        {
            if (PuzzleSlide.Block.GetTarget(b))
            {
                if (PuzzleSlide.Block.BlocksXY(b, 6, 2))
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
            if (b.BlocksXY(x,y))
            {
                return b;
            }
        }
        return null;
    }
}
