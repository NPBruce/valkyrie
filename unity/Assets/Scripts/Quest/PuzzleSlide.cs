using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ValkyrieTools;

public class PuzzleSlide : Puzzle
{
    public List<Block> puzzle;
    public int moves = 0;

    public PuzzleSlide(int depth)
    {
        if (depth < 1)
        {
            depth = 1;
        }
        List<Dictionary<string, string>> options = new List<Dictionary<string, string>>();
        TextAsset textAsset = (TextAsset)Resources.Load("slidepuzzles");
        string puzzleText = textAsset.text;
        IniData puzzles = IniRead.ReadFromString(puzzleText);

        while (options.Count == 0)
        {
            foreach (Dictionary<string, string> p in puzzles.data.Values)
            {
                int moves = 1;
                int.TryParse(p["moves"], out moves);
                if (moves == depth)
                {
                    options.Add(p);
                }
            }
            depth--;
            if (depth == 0)
            {
                ValkyrieDebug.Log("Error: Unable to find puzzle.");
                Application.Quit();
            }
        }
        Loadpuzzle(options[Random.Range(0, options.Count)]);
        moves = 0;
    }

    public PuzzleSlide(Dictionary<string, string> data)
    {
        Loadpuzzle(data);
    }

    public void Loadpuzzle(Dictionary<string, string> data)
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
            if (b.Blocks(x, y))
            {
                return false;
            }
        }
        return true;
    }

    override public string ToString(string id)
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
            return rotation.ToString() + ',' + xlen + ',' + ylen + ',' + xpos + ',' + ypos + ',' + target;
        }
    }

    public static Dictionary<string, string> HardCodedPuzzle()
    {
        Dictionary<string, string> content = new Dictionary<string, string>();
        content.Add("moves", "0");
        content.Add("block0", "False,1,0,0,2,True");
        content.Add("block1", "False,2,0,0,1,False");
        content.Add("block2", "True,0,1,5,0,False");
        content.Add("block3", "True,0,1,4,1,False");
        content.Add("block4", "False,3,0,0,5,False");
        content.Add("block5", "False,2,0,0,4,False");
        content.Add("block6", "False,1,0,2,0,False");
        content.Add("block7", "False,4,0,0,3,False");
        content.Add("block8", "True,0,1,5,2,False");
        content.Add("block9", "False,1,0,4,4,False");
        return content;
    }
}
