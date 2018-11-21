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
            if (kv.Key[0].Equals('%')) newVars.Add(kv.Key, kv.Value);
            if (kv.Key.Substring(0, 2).Equals("$%")) newVars.Add(kv.Key, kv.Value);
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
        vars[var] = value;
    }

    public float GetValue(string var)
    {
        if (!vars.ContainsKey(var))
        {
            return 0;
        }
        return vars[var];
    }

    public float GetOpValue(QuestData.Event.VarOperation op)
    {
        if (!vars.ContainsKey(op.var))
        {
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Adding quest var: " + op.var, true));
            vars.Add(op.var, 0);
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
        // value is var
        if (!vars.ContainsKey(op.value))
        {
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Adding quest var: " + op.var, true));
            vars.Add(op.value, 0);
        }
        return vars[op.value];
    }

    public void Perform(QuestData.Event.VarOperation op)
    {
        float value = GetOpValue(op);

        if (op.var[0] == '#')
        {
            return;
        }

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

        if (op.operation.Equals("*"))
        {
            vars[op.var] *= value;
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Multiplying: " + value + " with quest var: " + op.var + " result: " + vars[op.var], true));
        }

        if (op.operation.Equals("/"))
        {
            vars[op.var] /= value;
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Dividing quest var: " + op.var + " by: " + value + " result: " + vars[op.var], true));
        }

        if (op.operation.Equals("%"))
        {
            vars[op.var] %= value;
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Modulus quest var: " + op.var + " by: " + value + " result: " + vars[op.var], true));
        }

        if (op.operation.Equals("="))
        {
            vars[op.var] = value;
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Setting: " + op.var + " to: " + value, true));
        }
    }


    public bool Test(QuestData.Event.EventTests tests)
    {
        if (tests == null || tests.testComponents == null || tests.testComponents.Count == 0)
            return true;

        bool result = true;
        string current_operator = "AND";
        int index = 0;
        int ignore_inside_parenthesis=0;

        foreach (QuestData.Event.TestComponent tc in tests.testComponents)
        {
            // ignore tests while we are running inside a parenthesis
            if (ignore_inside_parenthesis > 0)
            {
                if (tc is QuestData.Event.TestParenthesis)
                {
                    QuestData.Event.TestParenthesis tp = (QuestData.Event.TestParenthesis)tc;
                    if (tp.parenthesis == "(")
                        ignore_inside_parenthesis++;
                    else if (tp.parenthesis == ")")
                        ignore_inside_parenthesis--;
                }

                index++;
                continue;
            }

            if (tc is QuestData.Event.VarOperation)
            {
                if (current_operator == "AND")
                    result = (result && Test((QuestData.Event.VarOperation)tc));
                else if (current_operator == "OR")
                    result = (result || Test((QuestData.Event.VarOperation)tc));
            }
            else if (tc is QuestData.Event.TestLogicalOperator)
            {
                current_operator = ((QuestData.Event.TestLogicalOperator)tc).op;
            }
            else if (tc is QuestData.Event.TestParenthesis)
            {
                QuestData.Event.TestParenthesis tp = (QuestData.Event.TestParenthesis)tc;
                if (tp.parenthesis == "(")
                {
                    List<QuestData.Event.TestComponent> remaining_tests = tests.testComponents.GetRange(index+1, tests.testComponents.Count - (index+1));
                    if (current_operator == "AND")
                        result = (result && Test(new QuestData.Event.EventTests(remaining_tests)));
                    else if (current_operator == "OR")
                        result = (result || Test(new QuestData.Event.EventTests(remaining_tests)));

                    ignore_inside_parenthesis = 1;
                }
                else if (tp.parenthesis == ")")
                {
                    return result;
                }
            }

            index++;
        }

        if(ignore_inside_parenthesis>0)
        {
            // we should not get here
            ValkyrieTools.ValkyrieDebug.Log("Invalid Test :" + tests.ToString() + "\n returns " + result);
        }

        return result;
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

        // unknown tests fail
        return false;
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