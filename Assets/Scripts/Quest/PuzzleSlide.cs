using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PuzzleSlide
{
    public List<Block> puzzle;
    public int moves = 0;

    public PuzzleSlide(int depth)
    {
        puzzle = new List<Block>();
        puzzle.Add(new Block());

        // Start with 4 blocks
        AddBlock();
        AddBlock();
        AddBlock();
        AddBlock();

        PuzzleSlideSolver solve = new PuzzleSlideSolver();
        int steps = 0;
        while (steps != depth)
        {
            steps = solve.Solve(puzzle, depth);
            if (steps < depth)
            {
                AddBlock();
            }
            if (steps > depth)
            {
                RemoveBlock();
            }
        }
    }

    public PuzzleSlide(Dictionary<string, string> data)
    {
        puzzle = new List<Block>();
        foreach (KeyValuePair<string, string> kv in data)
        {
            if (kv.Key.Equals("moves"))
            {
                int.TryParse(kv.Value, out moves);
            }
            else
            {
                puzzle.Add(new Block(kv.Value));
            }
        }
    }

    public bool Solved()
    {
        return (puzzle[0].xpos == 6);
    }

    public void AddBlock()
    {
        Block b = new Block(Random.Range(0, 255));
        while (b.Blocks(puzzle))
        {
            b = new Block(Random.Range(0, 255));
        }
        puzzle.Add(b);
    }

    public void RemoveBlock()
    {
        puzzle.RemoveAt(Random.Range(1, puzzle.Count));
    }

    public static bool Empty(List<Block> state, int x, int y)
    {
        if (x < 0 || y < 0 || y > 5)
        {
            return false;
        }
        if (x > 5 && y != 2)
        {
            return false;
        }
        if (x > 7)
        {
            return false;
        }
        foreach (Block b in state)
        {
            if (b.Blocks(x,y))
            {
                return false;
            }
        }
        return true;
    }

    public string ToString(string id)
    {
        string nl = System.Environment.NewLine;
        // General quest state block
        string r = "[PuzzleSlide" + id + "]" + nl;
        r += "moves=" + moves + nl;
        for (int i = 0; i < puzzle.Count; i++)
        {
            r += "block" + i + "=" + puzzle[i].ToString() + nl;
        }
        return r + nl;
    }

    public class Block
    {
        public bool rotation = false;
        // Length is 0 based (1x1 is len 0)
        public int xlen = 1;
        public int ylen = 0;
        public int xpos = 0;
        public int ypos = 2;
        public bool target = true;

        public bool Blocks(int x, int y)
        {
            if (y < ypos) return false;
            if (x < xpos) return false;
            if (y > (ypos + ylen)) return false;
            if (x > (xpos + xlen)) return false;
            return true;
        }

        public Vector2 GetMove(int dir, int distance = 1)
        {
            Vector2 ret = new Vector2(xpos, ypos);
            if (dir > 0)
            {
                if (rotation)
                {
                    ret.y += ylen + distance;
                }
                else
                {
                    ret.x += xlen + distance;
                }
            }
            else
            {
                if (rotation)
                {
                    ret.y -= distance;
                }
                else
                {
                    ret.x -= distance;
                }
            }

            return ret;
        }

        public bool Blocks(List<Block> p)
        {
            foreach (Block b in p)
            {
                if (Blocks(b))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Blocks(Block b)
        {
            if ((b.ypos + b.ylen) < ypos) return false;
            if ((b.xpos + b.xlen) < xpos) return false;
            if (b.ypos > (ypos + ylen)) return false;
            if (b.xpos > (xpos + xlen)) return false;
            return true;
        }

        public Block()
        {
        }

        public Block(int seed)
        {
            target = false;

            rotation = ((seed & 0x01) == 0);
            int rndData = seed >> 1;

            int length = (rndData % 4) + 1;
            rndData = rndData / 4;

            int stillpos = (rndData % 6);
            rndData = rndData / 6;

            int movPos = (rndData % (6 - length));

            if (rotation)
            {
                xpos = stillpos;
                ypos = movPos;
                xlen = 0;
                ylen = length;
            }
            else
            {
                ypos = stillpos;
                xpos = movPos;
                xlen = length;
                ylen = 0;
            }
        }

        public Block(string data)
        {
            string[] vars = data.Split(",".ToCharArray());
            bool.TryParse(vars[0], out rotation);
            int.TryParse(vars[1], out xlen);
            int.TryParse(vars[2], out ylen);
            int.TryParse(vars[3], out xpos);
            int.TryParse(vars[4], out ypos);
            bool.TryParse(vars[5], out target);
        }

        public Block(Block b)
        {
            rotation = b.rotation;
            xlen = b.xlen;
            ylen = b.ylen;
            xpos = b.xpos;
            ypos = b.ypos;
            target = b.target;
        }

        override public string ToString()
        {
            return rotation.ToString() + ',' + xlen + ',' + ylen + ',' + xpos + ',' + ypos + ',' +  target;
        }
    }
}
