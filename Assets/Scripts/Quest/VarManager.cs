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

    public float GetValue(QuestData.Event.VarOperation op)
    {
        if (!vars.Contains(op.var))
        {
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Adding quest var: " + op.var, true));
            vars.Add(op.var, 0);
        }
        float r = 0;
        if (op.value.Length == 0)
        {
            return r;
        }
        if (Char.IsNumber(op.value[0])
        {
            float.TryParse(op.value, r);
            return r;
        }
        if (op.value.IndexOf("#Rand") == 0)
        {
            int randLimit;
            int.TryParse(op.value.Substring(5), out randLimit)
            r = Random.Range(1, randLimit + 1);
            return r;
        }
        // value is var
        if (!vars.Contains(op.value))
        {
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Adding quest var: " + op.var, true));
            vars.Add(op.value, 0);
        }
        return vars[op.value];
    }

    public void Perform(QuestData.Event.VarOperation op)
    {
        float value = GetValue(op)

        if (op.operation.Equals("+"))
        {
            vars[op.var] += value;
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Adding: " + value + " to quest var: " + op.var + " result: " + vars[op.var], true));
        }

        if (op.operation.Equals("-"))
        {
            vars[op.var] -= value;
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Subtracting: " + value + " from quest var: " + op.var + " result: " + vars[op.var], true));
        }
    }

    public bool Test(QuestData.Event.VarOperation op)
    {
        float value = GetValue(op)
        if (op.operation.Equals("=="))
        {
            return (vars[op.var] == value)
        }

        if (op.operation.Equals("!="))
        {
            return (vars[op.var] != value)
        }

        if (op.operation.Equals(">="))
        {
            return (vars[op.var] >= value)
        }

        if (op.operation.Equals("<="))
        {
            return (vars[op.var] <= value)
        }

        if (op.operation.Equals(">"))
        {
            return (vars[op.var] > value)
        }

        if (op.operation.Equals("<"))
        {
            return (vars[op.var] < value)
        }

        // Don't test assign operations
        return true;
    }
    
    public bool Test(List<QuestData.Event.VarOperation> ops)
    {
        foreach (QuestData.Event.VarOperation op in ops)
        {
            if !Test(op)
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
            Perform(op)
        }
    }
}