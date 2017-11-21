using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PuzzleImage : Puzzle
{
    public Dictionary<TilePosition, TilePosition> state;
    public int moves = 0;

    public PuzzleImage(int x, int y)
    {
        state = new Dictionary<TilePosition, TilePosition>();

        List<TilePosition> list = new List<TilePosition>();
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                list.Insert(Random.Range(0, list.Count), new TilePosition(i, j));
            }
        }

        int count = 0;
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                state.Add(new TilePosition(i, j), list[count++]);
            }
        }
    }

    public PuzzleImage(Dictionary<string, string> data)
    {
        state = new Dictionary<TilePosition, TilePosition>();
        foreach (KeyValuePair<string, string> kv in data)
        {
            if (kv.Key.Equals("moves"))
            {
                int.TryParse(kv.Value, out moves);
            }
            if (kv.Key.Equals("state"))
            {
                foreach (string s in kv.Value.Split(":".ToCharArray()))
                {
                    string[] split = s.Split(",".ToCharArray());
                    state.Add(new TilePosition(split[0]), new TilePosition(split[1]));
                }
            }
        }
    }

    public bool Solved()
    {
        foreach (KeyValuePair<TilePosition, TilePosition> kv in state)
        {
            if (kv.Key.x != kv.Value.x)
            {
                return false;
            }
            if (kv.Key.y != kv.Value.y)
            {
                return false;
            }
        }
        return true;
    }

    override public string ToString(string id)
    {
        string nl = System.Environment.NewLine;
        string r = "[PuzzleImage" + id + "]" + nl;
        r += "moves=" + moves.ToString() + nl;

        r += "state=";
        foreach (KeyValuePair<TilePosition, TilePosition> p in state)
        {
            r += p.Key.ToString() + "," + p.Value.ToString() + ":";
        }
        r = r.Substring(0, r.Length - 1) + nl;
        
        return r + nl;
    }

    public class TilePosition
    {
        public int x = 0;
        public int y = 0;

        public TilePosition(int xIn, int yIn)
        {
            x = xIn;
            y = yIn;
        }

        public TilePosition(string s)
        {
            string[] split = s.Split(" ".ToCharArray());
            int.TryParse(split[0], out x);
            int.TryParse(split[1], out y);
        }

        override public string ToString()
        {
            return x + " " + y;
        }
    }
}
