using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VarManager
{
    public Dictionary<string, float> vars;

    public VarManager()
    {
        vars = new Dictionary<string, float>();
    }

    public VarManager(Dictionary<string, string> data)
    {
        vars = new Dictionary<string, float>();

        foreach (KeyValuePair<string, string> kv in data)
        {
            float value = 0;
            float.TryParse(kv.Value, out value);
            vars.Add(kv.Key, value);
        }
    }

    public VarDefinition GetDefinition(string variableName)
    {
        if (game.quest.qd.components.ContainsKey(variableName))
        {
            return game.quest.qd.components[variableName];
        }
        return new VarDefinition("");
    }

    public Dictionary<string, float> GetPrefixVars(string prefix)
    {
        Dictionary<string, float> dict = new Dictionary<string, float>();
        foreach (KeyValuePair<string, float> kv in vars)
        {
            if (kv.Key.IndexOf(prefix) == 0)
            {
                dict.Add(kv.Key, kv.Value);
            }
        }
        return dict;
    }

    public void TrimQuest()
    {
        Dictionary<string, float> newVars = new Dictionary<string, float>();
        foreach (KeyValuePair<string, float> kv in vars)
        {
            if (kv.Key.Substring(0, 2).Equals("$%")) newVars.Add(kv.Key, kv.Value);
            if (GetDefinition(kv.Key).campaign)
            {
                newVars.Add(kv.Key, kv.Value);
            }
        }
        vars = newVars;
    }

    public void SetValue(string var, float value)
    {
        if (!vars.ContainsKey(var))
        {
            if (Game.Get().quest != null && Game.Get().quest.log != null)
            {
                Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Adding quest var: " + var, true));
            }
            vars.Add(var, 0);
        }

        if (GetDefinition(var).minimumUsed && GetDefinition(var).minimum < value)
        {
            vars[var] = GetDefinition(var).minimum;
        }
        if (GetDefinition(var).maximumUsed && GetDefinition(var).maximum > value)
        {
            vars[var] = GetDefinition(var).maximum;
        }
        else
        {
            vars[var] = value;
        }

        if (GetDefinition(var).internalVariableType.Equals("int"))
        {
            vars[var] = Mathf.RoundToInt(vars[var]);
        }
    }

    public float GetValue(string var)
    {
        if (!vars.ContainsKey(var))
        {
            float initialiseValue = 0;
            if (GetDefinition(var).IsBoolean())
            {
                bool boolInitialise = false;
                bool.TryParse(GetDefinition(var).initialise, out boolInitialise);
                if (boolInitialise)
                {
                    initialiseValue = 1;
                }
            }
            else
            {
                float.TryParse(GetDefinition(var).initialise, out initialiseValue)
            }
            if (Game.Get().quest != null && Game.Get().quest.log != null)
            {
                Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Adding quest var: " + var + " As: " + initialiseValue, true));
            }
            vars.Add(var, initialiseValue);
        }
        return vars[var];
    }

    public float GetOpValue(QuestData.Event.VarOperation op)
    {
        // Ensure var exists
        if (!vars.ContainsKey(op.var))
        {
            GetValue(string op.var);
        }
        float r = 0;
        if (op.value.Length == 0)
        {
            return r;
        }
        if (char.IsNumber(op.value[0]) || op.value[0] == '-' || op.value[0] == '.')
        {
            float.TryParse(op.value, out r);
            return r;
        }
        if (op.value.IndexOf("#rand") == 0)
        {
            int randLimit;
            int.TryParse(op.value.Substring(5), out randLimit);
            r = Random.Range(1, randLimit + 1);
            return r;
        }

        if (GetDefinition(op.var).IsBoolean())
        {
            bool valueBoolParse;
            if (bool.TryParse(op.value, out valueBoolParse))
            {
                if (valueBoolParse)
                {
                    return 1;
                }
                return 0;
            }
        }
        // value is var
        return GetValue(string op.var);
    }

    public void Perform(QuestData.Event.VarOperation op)
    {
        float value = GetOpValue(op);

        if (op.var[0] == '#')
        {
            return;
        }

        if (op.operation.Equals("+") || op.operation.Equals("OR"))
        {
            SetValue(op.var, vars[op.var] + value);
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Adding: " + value + " to quest var: " + op.var + " result: " + vars[op.var], true));
        }

        if (op.operation.Equals("-"))
        {
            SetValue(op.var, vars[op.var] - value);
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Subtracting: " + value + " from quest var: " + op.var + " result: " + vars[op.var], true));
        }

        if (op.operation.Equals("*"))
        {
            SetValue(op.var, vars[op.var] * value);
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Multiplying: " + value + " with quest var: " + op.var + " result: " + vars[op.var], true));
        }

        if (op.operation.Equals("/"))
        {
            SetValue(op.var, vars[op.var] / value);
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Dividing quest var: " + op.var + " by: " + value + " result: " + vars[op.var], true));
        }

        if (op.operation.Equals("%"))
        {
            SetValue(op.var, vars[op.var] % value);
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Modulus quest var: " + op.var + " by: " + value + " result: " + vars[op.var], true));
        }

        if (op.operation.Equals("="))
        {
            SetValue(op.var, value);
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Setting: " + op.var + " to: " + value, true));
        }

        if (op.operation.Equals("&"))
        {
            bool varAtZero = (vars[op.var] == 0);
            bool valueAtZero = (value == 0);
            if (!varAtZero && !valueAtZero)
            {
                SetValue(op.var, 1);
            }
            else
            {
                SetValue(op.var, 0);
            }
            SetValue(op.var, value);
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Setting: " + op.var + " to: " + value, true));
        }

        if (op.operation.Equals("!"))
        {
            if (value == 0)
            {
                SetValue(op.var, 1);
            }
            else
            {
                SetValue(op.var, 0);
            }
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Setting: " + op.var + " to: " + value, true));
        }

        if (op.operation.Equals("^"))
        {
            bool varAtZero = (vars[op.var] == 0);
            bool valueAtZero = (value == 0);
            if (varAtZero ^ valueAtZero)
            {
                SetValue(op.var, 1);
            }
            else
            {
                SetValue(op.var, 0);
            }
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Setting: " + op.var + " to: " + value, true));
        }
    }

    public bool Test(QuestData.Event.VarOperation op)
    {
        float value = GetOpValue(op);
        if (op.operation.Equals("=="))
        {
            return (vars[op.var] == value);
        }

        if (op.operation.Equals("!="))
        {
            return (vars[op.var] != value);
        }

        if (op.operation.Equals(">="))
        {
            return (vars[op.var] >= value);
        }

        if (op.operation.Equals("<="))
        {
            return (vars[op.var] <= value);
        }

        if (op.operation.Equals(">"))
        {
            return (vars[op.var] > value);
        }

        if (op.operation.Equals("<"))
        {
            return (vars[op.var] < value);
        }

        if (op.operation.Equals("OR"))
        {
            bool varAtZero = (vars[op.var] == 0);
            bool valueAtZero = (value == 0);
            return !varAtZero || !valueAtZero
        }

        if (op.operation.Equals("&"))
        {
            bool varAtZero = (vars[op.var] == 0);
            bool valueAtZero = (value == 0);
            return !varAtZero && !valueAtZero
        }

        // unknown tests fail
        return false;
    }

    public bool Test(List<QuestData.Event.VarOperation> ops)
    {
        foreach (QuestData.Event.VarOperation op in ops)
        {
            if (!Test(op))
            {
                return false;
            }
        }
        return true;
    }

    public void Perform(List<QuestData.Event.VarOperation> ops)
    {
        foreach (QuestData.Event.VarOperation op in ops)
        {
            Perform(op);
        }
    }

    override public string ToString()
    {
        string nl = System.Environment.NewLine;
        string r = "[Vars]" + nl;

        foreach (KeyValuePair<string, float> kv in vars)
        {
            if (kv.Value != 0)
            {
                r += kv.Key + "=" + kv.Value.ToString() + nl;
            }
        }
        return r + nl;
    }
}