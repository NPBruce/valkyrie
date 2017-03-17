using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PuzzleCode : Puzzle
{
    public List<int> = state;
    public List<List<int>> = guess;

    public PuzzleCode(int count, int style)
    {
        state = new List<int>();
        guess = new List<List<int>>();


    }

    public PuzzleCode(Dictionary<string, string> data)
    {
        state = new List<int>();
        guess = new List<List<int>>();
        foreach (KeyValuePair<string, string> kv in data)
        {
            if (kv.Key.Equals("state"))
            {
                foreach (string s in kv.Value.Split(" ".ToCharArray()))
                {
                    int temp = 0;
                    int.TryParse(s, out temp);
                    state.Add(temp);
                }
            }
            if (kv.Key.Equals("guess"))
            {
                foreach (string s in kv.Value.Split(",".ToCharArray()))
                {
                    List<int> guessList = new List<int>();
                    foreach (int i in s.Split(" ".ToCharArray()))
                    {
                        int temp = 0;
                        int.TryParse(i, out temp);
                        guessList.Add(temp);
                    }
                    guess.Add(guessList);
                }
            }
        }
    }

    public string ToString(string id)
    {
        string nl = System.Environment.NewLine;
        // General quest state block
        string r = "[PuzzleCode" + id + "]" + nl;
        r += "state="
        foreach (int i in state)
        {
            r += i + " ";
        }
        r = r.substring(0, r.length - 1) + nl;

        r += "guess="
        foreach (List<int> l in guess)
        {
            foreach (int i in l)
            {
                r += i + " ";
            }
            r[r.length - 1] = ",";
        }
        r = r.substring(0, r.length - 1) + nl;
        
        return r + nl;
    }

}
