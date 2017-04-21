using System;
using System.Collections;
using System.Collections.Generic;

public class PuzzleSlide
{
    public List<Block> puzzle;
    public int moves = 0;
    public Random random;

    public PuzzleSlide(int depth)
    {
        puzzle = new List<Block>();
        puzzle.Add(new Block());
        random = new Random();

        // Start with 4 blocks
        AddBlock();
        AddBlock();
        AddBlock();
        AddBlock();

        PuzzleSlideSolver solve = new PuzzleSlideSolver();
        while (moves != depth)
        {
            //Console.WriteLine("Solving:");

            //foreach (string s in FormatOutput(puzzle))
            //{
            //    Console.WriteLine(s);
            //}

            try
            {
                moves = solve.Solve(puzzle, depth);
            }
            catch (Exception)
            {
                moves = depth + 1;
            }
            //Console.WriteLine("Solver: " + steps);
            if (moves < depth && moves < 7)
            {
                if (puzzle.Count > 8)
                {
                    RemoveBlock();
                    RemoveBlock();
                    RemoveBlock();
                }
                AddBlock();
            }
            else if (moves > depth)
            {
                RemoveBlock();
            }
            else return;
        }
    }

    public List<string> FormatOutput(List<Block> puzzle)
    {
        char[][] layout = new char[8][];
        layout[0] = "########".ToCharArray();
        layout[1] = "#      #".ToCharArray();
        layout[2] = "#      #".ToCharArray();
        layout[3] = "#       ".ToCharArray();
        layout[4] = "#      #".ToCharArray();
        layout[5] = "#      #".ToCharArray();
        layout[6] = "#      #".ToCharArray();
        layout[7] = "########".ToCharArray();

        for (int i = 0; i < puzzle.Count; i++)
        {
            for (int j = 0; j < puzzle[i].GetYLen() + 1; j++)
            {
                layout[puzzle[i].GetYPos() + j + 1][puzzle[i].GetXPos() + 1] = i.ToString()[0];
            }
            for (int j = 0; j < puzzle[i].GetXLen() + 1; j++)
            {
                layout[puzzle[i].GetYPos() + 1][puzzle[i].GetXPos() + j + 1] = i.ToString()[0];
            }
        }

        List<string> output = new List<string>();
        foreach (char[] line in layout)
        {
            output.Add(new string(line));
        }
        return output;
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
        return (puzzle[0].GetXPos() == 6);
    }

    public void AddBlock()
    {
        Block b = new Block(random.Next(0, 255));
        while (b.Blocks(puzzle))
        {
            b = new Block(random.Next(0, 255));
        }
        puzzle.Add(b);
    }

    public void RemoveBlock()
    {
        puzzle.RemoveAt(random.Next(1, puzzle.Count));
    }

    public static bool Empty(List<Block> state, int x, int y)
    {
        return Empty(state.ToArray(), x, y);
    }

    public static bool Empty(Block[] state, int x, int y)
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
            if (b.BlocksXY(x,y))
            {
                return false;
            }
        }
        return true;
    }

    public static bool Empty(int[] state, int x, int y)
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
        foreach (int b in state)
        {
            if (Block.BlocksXY(b, x, y))
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
        string r = "[PuzzleSlide" + moves + id + "]" + nl;
        r += "moves=" + moves + nl;
        for (int i = 0; i < puzzle.Count; i++)
        {
            r += "block" + i + "=" + puzzle[i].ToString() + nl;
        }
        return r;
    }

    public class Block
    {
        // Red block
        public int data = 0x0920;

        public bool BlocksXY(int x, int y)
        {
            if (y < GetYPos()) return false;
            if (x < GetXPos()) return false;
            if (y > (GetYPos() + GetYLen())) return false;
            if (x > (GetXPos() + GetXLen())) return false;
            return true;
        }

        public static bool BlocksXY(int data, int x, int y)
        {
            if (y < GetYPos(data)) return false;
            if (x < GetXPos(data)) return false;
            if (y > (GetYPos(data) + GetYLen(data))) return false;
            if (x > (GetXPos(data) + GetXLen(data))) return false;
            return true;
        }

        public int GetXPos()
        {
            return (data & 0x0F);
        }

        public static int GetXPos(int data)
        {
            return (data & 0x0F);
        }

        public int GetYPos()
        {
            return ((data >> 4) & 0x0F);
        }

        public static int GetYPos(int data)
        {
            return ((data >> 4) & 0x0F);
        }

        public int GetXLen()
        {
            return ((data >> 8) & 0x07);
        }

        public static int GetXLen(int data)
        {
            return ((data >> 8) & 0x07);
        }

        public int GetYLen()
        {
            return ((data >> 12) & 0x07);
        }

        public static int GetYLen(int data)
        {
            return ((data >> 12) & 0x07);
        }

        public void ChangeXPos(int diff)
        {
            // We assume it doens't overflow;
            data += diff;
        }

        public static int ChangeXPos(int data, int diff)
        {
            return data + diff;
        }

        public void ChangeYPos(int diff)
        {
            if (diff > 0)
            {
                data = data + (diff << 4);
            }
            else
            {
                data = data - (-diff << 4);
            }
        }

        public static int ChangeYPos(int data, int diff)
        {
            if (diff > 0)
            {
                return data + (diff << 4);
            }
            else
            {
                return data  - (-diff << 4);
            }
        }

        public bool GetTarget()
        {
            return (data & 0x0800) != 0;
        }

        public static bool GetTarget(int data)
        {
            return (data & 0x0800) != 0;
        }

        public bool GetRotation()
        {
            return (data & 0x8000) != 0;
        }

        public static bool GetRotation(int data)
        {
            return (data & 0x8000) != 0;
        }

        public Vector2 GetMove(int dir, int distance = 1)
        {
            Vector2 ret = new Vector2(GetXPos(), GetYPos());
            if (dir > 0)
            {
                if (GetRotation())
                {
                    ret.y += GetYLen() + distance;
                }
                else
                {
                    ret.x += GetXLen() + distance;
                }
            }
            else
            {
                if (GetRotation())
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

        public static Vector2 GetMove(int data, int dir, int distance)
        {
            Vector2 ret = new Vector2(Block.GetXPos(data), Block.GetYPos(data));
            if (dir > 0)
            {
                if (Block.GetRotation(data))
                {
                    ret.y += Block.GetYLen(data) + distance;
                }
                else
                {
                    ret.x += Block.GetXLen(data) + distance;
                }
            }
            else
            {
                if (Block.GetRotation(data))
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
            if ((b.GetYPos() + b.GetYLen()) < GetYPos()) return false;
            if ((b.GetXPos() + b.GetXLen()) < GetXPos()) return false;
            if (b.GetYPos() > (GetYPos() + GetYLen())) return false;
            if (b.GetXPos() > (GetXPos() + GetXLen())) return false;
            return true;
        }

        public static bool Blocks(int a, int b)
        {
            if ((Block.GetYPos(b) + Block.GetYLen(b)) < Block.GetYPos(a)) return false;
            if ((Block.GetXPos(b) + Block.GetXLen(b)) < Block.GetXPos(a)) return false;
            if (Block.GetYPos(b) > (Block.GetYPos(a) + Block.GetYLen(a))) return false;
            if (Block.GetXPos(b) > (Block.GetXPos(a) + Block.GetXLen(a))) return false;
            return true;
        }

        public Block()
        {
        }

        public static int GetStartBlock()
        {
            return 0x0920;
        }

        public Block(int seed)
        {
            bool rotation = ((seed & 0x01) == 0);
            int rndData = seed >> 1;

            int length = (rndData % 4) + 1;
            if (rotation)
            {
                length = (rndData % 2) + 1;
                rndData = rndData / 2;
            }
            else
            {
                rndData = rndData / 4;
            }

            int stillpos = (rndData % 6);
            if (rotation)
            {
                rndData = rndData / 6;
            }
            else
            {
                stillpos = (rndData % 5);
                rndData = rndData / 5;
                if (stillpos == 2) stillpos++;
            }

            int movPos = (rndData % (6 - length));

            if (rotation)
            {
                data = 1 << 15;
                data |= length << 12;
                data |= movPos << 4;
                data |= stillpos;
            }
            else
            {
                data = length << 8;
                data |= stillpos << 4;
                data |= movPos;
            }
        }

        public Block(string dataStr)
        {
            data = 0;
            string[] vars = dataStr.Split(",".ToCharArray());
            
            bool rotation;
            bool.TryParse(vars[0], out rotation);
            if (rotation) data |= 1 << 15;

            int tmpInt;
            int.TryParse(vars[1], out tmpInt);
            data |= tmpInt << 8;

            int.TryParse(vars[2], out tmpInt);
            data |= tmpInt << 12;

            int.TryParse(vars[3], out tmpInt);
            data |= tmpInt;

            int.TryParse(vars[4], out tmpInt);
            data |= tmpInt << 4;

            bool target;
            bool.TryParse(vars[5], out target);
            if (target) data |= 1 << 11;
        }

        public Block(Block b)
        {
            data = b.data;
        }

        override public string ToString()
        {
            return GetRotation().ToString() + ',' + GetXLen() + ',' + GetYLen() + ',' + GetXPos() + ',' + GetYPos() + ',' + GetTarget();
        }
    }
}
