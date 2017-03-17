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

        Stack<TilePosition> stack = new Stack<TilePositon>();
        for (int i = 0, i < x; i++)
        {
            for (int j = 0, j < y; j++)
            {
                stack.Insert(Random.Range(0, stack.Count), new TilePosition(x, y));
            }
        }

        for (int i = 0, i < x; i++)
        {
            for (int j = 0, j < y; j++)
            {
                state.Add(new TilePosition(x, y), stack.Pop());
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
            return true;
        }
    }

    override public string ToString(string id)
    {
        string nl = System.Environment.NewLine;
        string r = "[PuzzleCode" + id + "]" + nl;
        r += "moves=" + moves.ToString() + nl;

        r += "state="
        foreach (KeyValuePair<TilePosition, TilePosition> p in state)
        {
            r += p.Key.ToString() + "," + p.Value.ToString() + ":";
        }
        r = r.substring(0, r.length - 1) + nl;
        
        return r + nl;
    }

    Public Class TilePosition
    {
        public int x = 0;
        public int y = 0;

        public TilePosition(string s)
        {
            string[] split = s.Split(" ".ToCharArray())
            int.TryParse(split[0], out x);
            int.TryParse(split[1], out y);
        }

        public string ToString()
        {
            return x + " " + y;
        }
    }
}
