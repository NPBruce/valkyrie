using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ValkyrieTools;

public class PuzzleTower : Puzzle
{
    public List<List<int>> puzzle;
    public int moves = 0;

    /// <summary>
    /// Create a Tower puzzle with minumum moves required
    /// </summary>
    /// <param name="depth">Minimum moves to solve puzzle</param>
    public PuzzleTower(int depth)
    {
        List<List<List<int>>> options = BuildPuzzles(depth);
        List<List<int>> p = options[Random.Range(0, options.Count)];

        // Randomise tower order
        puzzle = new List<List<int>>();
        int pos = Random.Range(0, 3);
        puzzle.Add(p[pos]);
        p.RemoveAt(pos);
        pos = Random.Range(0, 2);
        puzzle.Add(p[pos]);
        p.RemoveAt(pos);
        puzzle.Add(p[0]);
    }

    /// <summary>
    /// Create a Tower puzzle from saved data
    /// </summary>
    /// <param name="data">puzzle save data</param>
    public PuzzleTower(Dictionary<string, string> data)
    {
        Loadpuzzle(data);
    }

    /// <summary>
    /// Build list of possible puzzles with minimum moves to solve
    /// </summary>
    /// <param name="depth">Minimum moves to solve puzzle</param>
    /// <returns>List of uint formatted puzzle definitions</returns>
    protected List<List<List<int>>> BuildPuzzles(int depth)
    {
        // End puzzle state
        List<List<int>> end = new List<List<int>>();
        end.Add(new List<int>());
        end.Add(new List<int>());
        end.Add(new List<int>());

        for(int i = 0; i < 8; i++)
        {
            end[0].Add(7 - i);
        }

        List<List<List<List<int>>>> allStates = new List<List<List<List<int>>>>();
        allStates.Add(new List<List<List<int>>>());
        allStates[0].Add(end);

        for(int currentLevel = 0; currentLevel < depth; currentLevel++)
        {
            allStates.Add(new List<List<List<int>>>());
            foreach (List<List<int>> state in allStates[currentLevel])
            {
                AddStates(state, allStates);
            }
        }

        return allStates[depth];
    }

    /// <summary>
    /// Convert puzzle to uint format
    /// </summary>
    /// <param name="in">puzzle</param>
    protected void AddStates(List<List<int>> state, List<List<List<List<int>>>> allStates)
    {
        for (int i = 0; i < state.Count; i++)
        {
            if (!ReverseMoveOK(i, state)) continue;

            for (int j = 0; j < state.Count; j++)
            {
                if (j == i) continue;

                List<List<int>> newState = CopyState(state);
                newState[j].Add(state[i][state[i].Count - 1]);
                newState[i].RemoveAt(state[i].Count - 1);
                bool uniqueState = true;
                foreach (List<List<List<int>>> level in allStates)
                {
                    foreach (List<List<int>> oldState in level)
                    {
                        if (Equal(newState, oldState))
                        {
                            uniqueState = false;
                        }
                    }
                }
                if (uniqueState)
                {
                    allStates[allStates.Count - 1].Add(newState);
                }
            }
        }
    }

    /// <summary>
    /// Copy puzzle state
    /// </summary>
    /// <param name="state">puzzle</param>
    /// <returns>copy of puzzle state</returns>
    public List<List<int>> CopyState(List<List<int>> state)
    {
        List<List<int>> copy = new List<List<int>>();
        for (int i = 0; i < state.Count; i++)
        {
            copy.Add(new List<int>());
            for (int j = 0; j < state[i].Count; j++)
            {
                copy[i].Add(state[i][j]);
            }
        }
        return copy;
    }

    /// <summary>
    /// Compare puzzle states
    /// </summary>
    /// <param name="state">puzzle state</param>
    /// <param name="stateTwo">puzzle state</param>
    /// <returns>If the states are equal</returns>
    protected bool Equal(List<List<int>> state, List<List<int>> stateTwo)
    {
        if (state.Count != state.Count) return false;
        for (int i = 0; i < state.Count; i++)
        {
            if (state[i].Count != stateTwo[i].Count) return false;
            for (int j = 0; j < state[i].Count; j++)
            {
                if (state[i][j] != stateTwo[i][j]) return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Populate Tower puzzle from saved data
    /// </summary>
    /// <param name="data">puzzle save data</param>
    protected void Loadpuzzle(Dictionary<string, string> data)
    {
        puzzle = new List<List<int>>();
        puzzle.Add(new List<int>());
        puzzle.Add(new List<int>());
        puzzle.Add(new List<int>());

        foreach (KeyValuePair<string, string> kv in data)
        {
            if (kv.Key.Equals("moves"))
            {
                int.TryParse(kv.Value, out moves);
            }
            else
            {
                int tower = 0;
                int.TryParse(kv.Key, out tower);

                foreach(string s in kv.Value.Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries))
                {
                    int size = 0;
                    int.TryParse(s, out size);
                    puzzle[tower].Add(size);
                }
            }
        }
    }

    /// <summary>
    /// Check if the puzzle is solved
    /// </summary>
    /// <returns>If the puzzle has been solved</returns>
    public bool Solved()
    {
        foreach (List<int> list in puzzle)
        {
            if (list.Count > 0 && list.Count < 8) return false;
            int lastSize = 10;
            foreach (int size in list)
            {
                if (size > lastSize) return false;
                lastSize = size;
            }
        }

        return true;
    }

    /// <summary>
    /// Check a puzzle move is legal
    /// </summary>
    /// <param name="fromTower">Tower to move block from</param>
    /// <param name="toTower">Tower to move block to</param>
    /// <returns>If the move is legal</returns>
    public bool MoveOK(int fromTower, int toTower)
    {
        return MoveOK(fromTower, toTower, puzzle);
    }

    /// <summary>
    /// Check a puzzle move is legal
    /// </summary>
    /// <param name="fromTower">Tower to move block from</param>
    /// <param name="toTower">Tower to move block to</param>
    /// <param name="p">Puzzle state to use</param>
    /// <returns>If the move is legal</returns>
    public bool MoveOK(int fromTower, int toTower, List<List<int>> p)
    {
        if (p.Count <= fromTower) return false;
        if (fromTower < 0) return false;
        if (p[fromTower].Count == 0) return false;
        if (p.Count <= toTower) return false;
        if (toTower < 0) return false;
        if (p[toTower].Count == 0) return true;

        int fromSize = p[fromTower][p[fromTower].Count - 1];
        int toSize = p[toTower][p[toTower].Count - 1];
        return fromSize < toSize;
    }

    /// <summary>
    /// Check if a reverse puzzle move is legal
    /// </summary>
    /// <param name="fromTower">Tower to move block from</param>
    /// <param name="p">Puzzle state to use</param>
    /// <returns>If the move is legal</returns>
    public bool ReverseMoveOK(int fromTower, List<List<int>> p)
    {
        if (p.Count <= fromTower) return false;
        if (fromTower < 0) return false;
        if (p[fromTower].Count == 0) return false;
        if (p[fromTower].Count == 1) return true;

        int moveSize = p[fromTower][p[fromTower].Count - 1];
        int baseSize = p[fromTower][p[fromTower].Count - 2];
        return moveSize < baseSize;
    }

    /// <summary>
    /// Perform a puzzle move, if legal
    /// </summary>
    /// <param name="fromTower">Tower to move block from</param>
    /// <param name="toTower">Tower to move block to</param>
    public void Move(int fromTower, int toTower)
    {
        Move(fromTower, toTower, puzzle);
    }

    /// <summary>
    /// Perform a puzzle move, if legal
    /// </summary>
    /// <param name="fromTower">Tower to move block from</param>
    /// <param name="toTower">Tower to move block to</param>
    /// <param name="p">Puzzle state to use</param>
    public void Move(int fromTower, int toTower, List<List<int>> p)
    {
        if (!MoveOK(fromTower, toTower, p)) return;

        p[toTower].Add(p[fromTower][p[fromTower].Count - 1]);
        p[fromTower].RemoveAt(p[fromTower].Count - 1);
    }

    /// <summary>
    /// write puzzle to string for saving
    /// </summary>
    /// <param name="id">name of the event component</param>
    /// <returns>string output of the puzzle</returns>
    override public string ToString(string id)
    {
        string nl = System.Environment.NewLine;
        // General quest state block
        string r = "[PuzzleTower" + id + "]" + nl;
        r += "moves=" + moves + nl;
        for (int i = 0; i < puzzle.Count; i++)
        {
            r += i + "=";
            foreach (int size in puzzle[i])
            {
                r += size + " ";
            }
            r = r.Substring(0, r.Length - 1) + nl;
        }
        return r + nl;
    }
}
