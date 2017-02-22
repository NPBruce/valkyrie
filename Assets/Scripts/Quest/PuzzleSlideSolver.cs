using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PuzzleSlideGeneration;

public class PuzzleSlideSolver
{
    public Queue<List<Block>> queue;
    public Dictionary<List<Block>, List<Block>> pred;

    public PuzzleSlideSolver()
    {
        queue = new Queue<List<Block>>();
        pred = new Dictionary<List<Block>, List<Block>>();
    }

    public int Solve(List<Block> current, int moves)
    {
        Propose(current, null);
        boolean solved = false;
        for (int i = 0; i < moves; i++)
        {
            List<Block> current = queue.Dequeue();
            if (AtGoal(current))
            {
                return i;
            }
            explore(current);
        }
        return i;
    }

    public void Explore(List<Block> current)
    {
        foreach (Block b in current)
        {
            slide(current, block, -1);
            slide(current, block, 1);
        }
    }

    public void Slide(List<Block> current, Block b, int dir)
    {
        CountSpaces(List<Block> state, Block b, int dir)
        List<Block> newState = new List<Block>(current);
        while(CheckMove(newState, b, dir))
        {
            newState.Remove(b);

            Block newBlock = Block(b);
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
            newState = new List<Block>(newState);
        }
    }

    public void Propose(List<Block> next, List<Block> prev)
    {
        if (!pred.ContainsKey(next))
        {
            pred.Add(next, prev);
            queue.Enqueue(next);
        }
    }

    public bool CheckMove(List<Block> state, Block b, int dir)
    {
        Vector2 target = b.GetMove(dir);
        return Empty(state, target.x, target.y)
    }

    public bool AtGoal(List<Block> state)
    {
        Foreach (Block b in state)
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

    public Block AtLocation(List<Block> state, int x, int y)
    {
        if (x < 0 || y < 0 || x > 5 || y > 5)
        {
            return null;
        }
        Foreach (Block b in state)
        {
            if (b.Blocks(x,y))
            {
                return b;
            }
        }
        return null;
    }
}
