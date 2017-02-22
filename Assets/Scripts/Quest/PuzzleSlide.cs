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
        AddBlock());
        AddBlock());
        AddBlock());
        AddBlock());

        PuzzleSlideSolver solve = new PuzzleSlideSolver();
        int steps = 0;
        while (steps != depth)
        {
            steps = solve.Solve(puzzle, depth)
            if (steps < depth)
            {
                AddBlock());
            }
            if (steps > depth)
            {
                RemoveBlock());
            }
        }
    }

    public PuzzleSlide(Dictionary<string, string> data)
    {
        puzzle = new List<Block>();
        foreach (KeyValuePair<string, string> in data)
        {
            if (kv.key.Equals("moves"))
            {
                int.TryParse(kv.value, out moves)
            }
            else
            {
                puzzle.Add(new Block(kv.Value));
            }
        }
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
        if (x < 0 || y < 0 || x > 5 || y > 5)
        {
            return false;
        }
        Foreach (Block b in state)
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
        for (int i = 0; i < blocks.Count)
        {
            r += "block" + i + "=" + blocks[i].ToString() + nl;
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

        public Vector2 GetMove(bool dir)
        {
            Vector2 ret = new Vector2(xpos, ypos);
            if (dir > 0)
            {
                if (rotation)
                {
                    ret.y += ylen + 1;
                }
                else
                {
                    ret.x += xlen + 1;
                }
            }
            else
            {
                if (rotation)
                {
                    ret.y -= 1;
                }
                else
                {
                    ret.x -= 1;
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
            if ((b.y + b.ylen) < ypos) return false;
            if ((b.x + b.xlen) < xpos) return false;
            if (b.y > (ypos + ylen)) return false;
            if (b.x > (xpos + xlen)) return false;
            return true;
        }

        public Block(int seed)
        {
            target = false;

            rotation = seed & 0x01;
            int rndData = seed >> 1;

            int length = (rndData % 5);
            rndData = rndData / 5;

            int stillpos = (rndData % 6);
            rndData = rndData / 6;

            itn movPos = (rndData % (6 - length));

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
            string[] vars = data.Split[",".ToCharArray()];
            bool.TryParse(vars[0], out rotation);
            int.TryParse(vars[1], out xlen);
            int.TryParse(vars[2], out ylen);
            int.TryParse(vars[3], out xpos);
            int.TryParse(vars[4], out ypos);
            int.TryParse(vars[5], out target);
            rotation + ',' + xlen + ',' + ylen + ',' + xpos + ',' + ypos + ',' +  target;
        }

        public string ToString
        {
            return rotation + ',' + xlen + ',' + ylen + ',' + xpos + ',' + ypos + ',' +  target;
        }
    }
}
