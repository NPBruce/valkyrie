using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ValkyrieTools;

public class PuzzleCode : Puzzle
{
    public Answer answer;
    public List<CodeGuess> guess;

    public PuzzleCode(int items, int options, string solution)
    {
        guess = new List<CodeGuess>();

        // Initialize puzzle answer depending on whether we have a pre-determined solution or not.
        if (solution.Length > 0)
        {
            ValkyrieDebug.Log("Setting solution to " + solution);
            answer = new Answer(solution);
        }
        else
        {
            answer = new Answer(items, options);
        }
    }

    public PuzzleCode(Dictionary<string, string> data)
    {
        guess = new List<CodeGuess>();
        List<string> guessString = new List<string>();
        foreach (KeyValuePair<string, string> kv in data)
        {
            if (kv.Key.Equals("answer"))
            {
                answer = new Answer(kv.Value);
            }
            if (kv.Key.Equals("guess"))
            {
                foreach (string s in kv.Value.Split(",".ToCharArray()))
                {
                    guessString.Add(s);
                }
            }
        }
        foreach (string s in guessString)
        {
            guess.Add(new CodeGuess(answer, s));
        }
    }

    public void AddGuess(List<int> g)
    {
        guess.Add(new CodeGuess(answer, g));
    }

    public bool Solved()
    {
        if (guess.Count == 0)
        {
            return false;
        }
        if (guess[guess.Count - 1].Correct())
        {
            return true;
        }
        return false;
    }

    override public string ToString(string id)
    {
        string nl = System.Environment.NewLine;
        string r = "[PuzzleCode" + id + "]" + nl;
        r += "answer=" + answer.ToString() + nl;

        r += "guess=";
        foreach (CodeGuess g in guess)
        {
            r += g.ToString() + ",";
        }
        r = r.Substring(0, r.Length - 1) + nl;
        
        return r + nl;
    }

    public class Answer
    {
        public List<int> state;

        public Answer(int items, int options)
        {
            state = new List<int>();
            for (int i = 0; i < items; i++)
            {
                state.Add(Random.Range(0, options) + 1);
            }
        }

        /* Setup a code answer with 's'. Used for saved data.
         * Example: s = "1 3 5 4" */
        public Answer(string s)
        {
            state = new List<int>();
            foreach (string part in s.Split(" ".ToCharArray()))
            {
                int temp = 0;
                int.TryParse(part, out temp);
                state.Add(temp);
            }
        }

        override public string ToString()
        {
            string r = "";
            foreach (int i in state)
            {
                r += i + " ";
            }
            r = r.Substring(0, r.Length - 1);
            return r;
        }
    }

    public class CodeGuess
    {
        Answer answer;
        public List<int> guess;

        public CodeGuess(Answer a, List<int> g)
        {
            answer = a;
            guess = g;
        }

        public CodeGuess(Answer a, string g)
        {
            answer = a;
            guess = new List<int>();
            foreach (string s in g.Split(" ".ToCharArray()))
            {
                int temp = 0;
                int.TryParse(s, out temp);
                guess.Add(temp);
            }
        }

        public bool Correct()
        {
            for (int i = 0; i < answer.state.Count; i++)
            {
                if (answer.state[i] != guess[i])
                {
                    return false;
                }
            }
            return true;
        }

        public int CorrectSpot()
        {
            int r = 0;
            for (int i = 0; i < answer.state.Count; i++)
            {
                if (answer.state[i] == guess[i])
                {
                    r++;
                }
            }
            return r;
        }

        public int CorrectType()
        {
            int r = 0;
            bool[] used = new bool[answer.state.Count];

            for (int i = 0; i < guess.Count; i++)
            {
                if (guess[i] == answer.state[i]) continue;
                
                for (int j = 0; j < answer.state.Count; j++)
                {
                    if (i == j) continue;
                    if (used[j]) continue;
                    if (answer.state[j] == guess[j]) continue;
                    
                    if (answer.state[j] == guess[i])
                    {
                        r++;
                        used[j] = true;
                        break;
                    }
                }
            }
            return r;
        }

        override public string ToString()
        {
            string r = "";
            foreach (int i in guess)
            {
                r += i + " ";
            }
            r = r.Substring(0, r.Length - 1);
            return r;
        }
    }
}
