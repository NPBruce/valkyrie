using System;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
using UnityEngine;

public class EditorComponentVarTestsUtil
{
    public static float AddEventVarConditionComponents(Transform parentTransform, float xOffset, float yOffset,
        ITestable testable, Action updateAction)
    {
        UIElement ui = new UIElement(Game.EDITOR, parentTransform);
        ui.SetLocation(xOffset, yOffset, 18, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.TESTS));

        ui = new UIElement(Game.EDITOR, parentTransform);
        ui.SetLocation(xOffset + 18f, yOffset, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddTestOp(testable, updateAction); });
        new UIElementBorder(ui, Color.green);

        if (testable.Tests.VarTestsComponents.Count > 0)
        {
            ui = new UIElement(Game.EDITOR, parentTransform);
            ui.SetLocation(xOffset, yOffset, 1, 1);
            ui.SetText(CommonStringKeys.PLUS, Color.green);
            ui.SetButton(delegate { SelectAddParenthesis(testable, updateAction); });
            new UIElementBorder(ui, Color.green);

            ui = new UIElement(Game.EDITOR, parentTransform);
            ui.SetLocation(xOffset + 1.0f, yOffset, 2, 1);
            ui.SetText("(...)");
        }

        yOffset++;

        int component_index = 0;
        foreach (VarTestsComponent tc in testable.Tests.VarTestsComponents)
        {
            if (tc is VarOperation)
            {
                int tmp_index = component_index;

                // only display arrows if item can be moved
                if (component_index != (testable.Tests.VarTestsComponents.Count - 1)
                    && testable.Tests.VarTestsComponents.Count > 1
                    && testable.Tests.VarTestsComponents.FindIndex(component_index + 1,
                        x => x.GetClassVarTestsComponentType() == VarTestsLogicalOperator.GetVarTestsComponentType()) != -1
                )
                {
                    ui = new UIElement(Game.EDITOR, parentTransform);
                    ui.SetLocation(xOffset, yOffset, 1, 1);
                    ui.SetText(CommonStringKeys.DOWN, Color.yellow);
                    ui.SetTextAlignment(TextAnchor.LowerCenter);
                    ui.SetButton(delegate
                    {
                        testable.Tests.moveComponent(tmp_index, false);
                        updateAction.Invoke();
                    });
                    new UIElementBorder(ui, Color.yellow);
                }

                if (component_index != 0
                    && testable.Tests.VarTestsComponents.FindLastIndex(component_index - 1,
                        x => x.GetClassVarTestsComponentType() == VarTestsLogicalOperator.GetVarTestsComponentType()) != -1
                )
                {
                    ui = new UIElement(Game.EDITOR, parentTransform);
                    ui.SetLocation(xOffset + 1.0f, yOffset, 1, 1);
                    ui.SetText(CommonStringKeys.UP, Color.yellow);
                    ui.SetTextAlignment(TextAnchor.LowerCenter);
                    ui.SetButton(delegate
                    {
                        testable.Tests.moveComponent(tmp_index, true);
                        updateAction.Invoke();
                    });
                    new UIElementBorder(ui, Color.yellow);
                }

                VarOperation tmp = (VarOperation) tc;
                ui = new UIElement(Game.EDITOR, parentTransform);
                ui.SetLocation(xOffset + 2f, yOffset, 8.5f, 1);
                ui.SetText(tmp.var);
                new UIElementBorder(ui);

                ui = new UIElement(Game.EDITOR, parentTransform);
                ui.SetLocation(xOffset + 10.5f, yOffset, 2, 1);
                ui.SetText(tmp.operation);
                ui.SetButton(delegate { SetTestOpp(tmp, updateAction); });
                new UIElementBorder(ui);

                ui = new UIElement(Game.EDITOR, parentTransform);
                ui.SetLocation(xOffset + 12.5f, yOffset, 5.5f, 1);
                ui.SetText(tmp.value);
                ui.SetButton(delegate { SetValue(tmp, updateAction); });
                new UIElementBorder(ui);

                ui = new UIElement(Game.EDITOR, parentTransform);
                ui.SetLocation(xOffset + 18f, yOffset++, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate { RemoveOp(testable.Tests, tmp_index, updateAction); });
                new UIElementBorder(ui, Color.red);
            }

            if (tc is VarTestsLogicalOperator)
            {
                VarTestsLogicalOperator tmp = (VarTestsLogicalOperator) tc;

                ui = new UIElement(Game.EDITOR, parentTransform);
                ui.SetLocation(xOffset + 9.5f, yOffset, 4, 1);
                if (tmp.op.Equals("AND"))
                    ui.SetText(CommonStringKeys.AND);
                else if (tmp.op.Equals("OR"))
                    ui.SetText(CommonStringKeys.OR);
                ui.SetButton(delegate
                {
                    tmp.NextLogicalOperator();
                    updateAction.Invoke();
                });
                new UIElementBorder(ui);
                yOffset++;
            }

            if (tc is VarTestsParenthesis)
            {
                int tmp_index = component_index;
                VarTestsParenthesis tp = (VarTestsParenthesis) tc;

                if (component_index != (testable.Tests.VarTestsComponents.Count - 1)
                    && testable.Tests.VarTestsComponents.FindIndex(component_index + 1,
                        x => x.GetClassVarTestsComponentType() == VarOperation.GetVarTestsComponentType()) != -1
                )
                {
                    if (tp.parenthesis == "(")
                    {
                        int valid_index = testable.Tests.FindNextValidPosition(component_index, false);
                        if (valid_index != -1
                            && testable.Tests.FindClosingParenthesis(valid_index) != -1)
                        {
                            ui = new UIElement(Game.EDITOR, parentTransform);
                            ui.SetLocation(xOffset, yOffset, 1, 1);
                            ui.SetText(CommonStringKeys.DOWN, Color.yellow);
                            ui.SetTextAlignment(TextAnchor.LowerCenter);
                            ui.SetButton(delegate
                            {
                                testable.Tests.moveComponent(tmp_index, false);
                                updateAction.Invoke();
                            });
                            new UIElementBorder(ui, Color.yellow);
                        }
                    }
                    else if (tp.parenthesis == ")")
                    {
                        ui = new UIElement(Game.EDITOR, parentTransform);
                        ui.SetLocation(xOffset, yOffset, 1, 1);
                        ui.SetText(CommonStringKeys.DOWN, Color.yellow);
                        ui.SetTextAlignment(TextAnchor.LowerCenter);
                        ui.SetButton(delegate
                        {
                            testable.Tests.moveComponent(tmp_index, false);
                            updateAction.Invoke();
                        });
                        new UIElementBorder(ui, Color.yellow);
                    }
                }

                if (component_index != 0
                    && testable.Tests.VarTestsComponents.FindLastIndex(component_index - 1,
                        x => x.GetClassVarTestsComponentType() == VarOperation.GetVarTestsComponentType()) != -1
                )
                {
                    if (tp.parenthesis == "(")
                    {
                        ui = new UIElement(Game.EDITOR, parentTransform);
                        ui.SetLocation(xOffset + 1f, yOffset, 1, 1);
                        ui.SetText(CommonStringKeys.UP, Color.yellow);
                        ui.SetTextAlignment(TextAnchor.LowerCenter);
                        ui.SetButton(delegate
                        {
                            testable.Tests.moveComponent(tmp_index, true);
                            updateAction.Invoke();
                        });
                        new UIElementBorder(ui, Color.yellow);
                    }
                    else if (tp.parenthesis == ")")
                    {
                        int valid_index = testable.Tests.FindNextValidPosition(component_index, true);
                        if (valid_index != -1
                            && testable.Tests.FindOpeningParenthesis(valid_index) != -1)
                        {
                            ui = new UIElement(Game.EDITOR, parentTransform);
                            ui.SetLocation(xOffset + 1f, yOffset, 1, 1);
                            ui.SetText(CommonStringKeys.UP, Color.yellow);
                            ui.SetTextAlignment(TextAnchor.LowerCenter);
                            ui.SetButton(delegate
                            {
                                testable.Tests.moveComponent(tmp_index, true);
                                updateAction.Invoke();
                            });
                            new UIElementBorder(ui, Color.yellow);
                        }
                    }
                }

                ui = new UIElement(Game.EDITOR, parentTransform);
                ui.SetLocation(xOffset + 2f, yOffset, 2, 1);
                ui.SetText(tp.parenthesis);
                new UIElementBorder(ui);

                ui = new UIElement(Game.EDITOR, parentTransform);
                ui.SetLocation(xOffset + 4f, yOffset, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate
                {
                    testable.Tests.Remove(tmp_index);
                    updateAction.Invoke();
                });
                new UIElementBorder(ui, Color.red);

                yOffset++;
            }

            component_index++;
        }

        return yOffset + 1;
    }

    private static void AddTestOp(ITestable testable, Action updateAction)
    {
        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(
            s => SelectAddOp(s, testable, updateAction) , 
            new StringKey("val", "SELECT", CommonStringKeys.VAR), true);

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { "Quest" });
        @select.AddItem("{" + CommonStringKeys.NEW.Translate() + "}", "{NEW}", traits, true);

        AddQuestVars(@select);

        traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { "#" });

        @select.AddItem("#monsters", traits);
        @select.AddItem("#heroes", traits);
        @select.AddItem("#round", traits);
        @select.AddItem("#eliminated", traits);
        foreach (ContentPack pack in Game.Get().cd.allPacks)
        {
            if (pack.id.Length > 0)
            {
                @select.AddItem("#" + pack.id, traits);
            }
        }
        foreach (HeroData hero in Game.Get().cd.Values<HeroData>())
        {
            if (hero.sectionName.Length > 0)
            {
                @select.AddItem("#" + hero.sectionName, traits);
            }
        }
        @select.Draw();
    }

    public static void AddQuestVars(UIWindowSelectionListTraits list)
    {
        HashSet<string> vars = new HashSet<string>();
        HashSet<string> dollarVars = new HashSet<string>();

        Game game = Game.Get();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.CurrentQuest.qd.components)
        {
            if (kv.Value is QuestData.Event)
            {
                QuestData.Event e = kv.Value as QuestData.Event;
                foreach (string s in ExtractVarsFromEvent(e))
                {
                    if (s[0] != '$')
                    {
                        vars.Add(s);
                    }
                }
            }
        }

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { "Quest" });
        foreach (string s in vars)
        {
            list.AddItem(s, traits);
        }


        foreach (PerilData e in game.cd.Values<PerilData>())
        {
            foreach (string s in ExtractVarsFromEvent(e))
            {
                if (s[0] == '$')
                {
                    dollarVars.Add(s);
                }
            }
        }

        traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { "$" });
        foreach (string s in dollarVars)
        {
            list.AddItem(s, traits);
        }
    }

    public static HashSet<string> ExtractVarsFromEvent(QuestData.Event e)
    {
        HashSet<string> vars = new HashSet<string>();
        foreach (VarOperation op in e.operations)
        {
            vars.Add(op.var);
            if (op.value.Length > 0 && op.value[0] != '#' && !char.IsNumber(op.value[0]) && op.value[0] != '-' && op.value[0] != '.')
            {
                vars.Add(op.value);
            }
        }

        if (e.tests == null) return vars;

        foreach (VarTestsComponent tc in e.tests.VarTestsComponents)
        {
            if (tc is VarOperation)
            {
                VarOperation op = (VarOperation)tc;
                if (op.var.Length > 0 && op.var[0] != '#')
                {
                    vars.Add(op.var);
                }
                if (op.value.Length > 0 && op.value[0] != '#' && !char.IsNumber(op.value[0]) && op.value[0] != '-' && op.value[0] != '.')
                {
                    vars.Add(op.value);
                }
            }
        }

        if (!string.IsNullOrEmpty(e.quotaVar) && e.quotaVar[0] != '#')
        {
            vars.Add(e.quotaVar);
        }
        return vars;
    }

    private static void SelectAddParenthesis(ITestable testable, Action updateAction)
    {
        testable.Tests.Add(new VarTestsParenthesis(")"));
        testable.Tests.Add(new VarTestsParenthesis("("));
        updateAction.Invoke();
    }

    public static void SelectAddOp(string var, ITestable testable, Action updateAction, bool test = true)
    {
        if (var == null)
        {
            updateAction.Invoke();
            return;
        }

        VarTests tests = testable.Tests;
        List<VarOperation> operations = testable.Operations;
        VarOperation op = new VarOperation();
        op.var = var;
        op.operation = "=";
        if (test)
        {
            op.operation = ">";
        }
        op.value = "0";

        if (op.var.Equals("{NEW}"))
        {
            // Var name doesn localize
            var textEdit = new QuestEditorTextEdit(CommonStringKeys.VAR_NAME, "", s => NewVar(s, op, test, testable, updateAction));
            textEdit.EditText();
        }
        else
        {
            if (test)
            {
                if (tests.VarTestsComponents.Count == 0)
                {
                    tests.Add(op);
                }
                else
                {
                    tests.Add(new VarTestsLogicalOperator());
                    tests.Add(op);
                }
            }
            else
            {
                operations.Add(op);
            }

            updateAction.Invoke();
        }
    }

    private static void NewVar(string value, VarOperation op, bool test, ITestable testable, Action updateAction)
    {
        op.var = System.Text.RegularExpressions.Regex.Replace(value, "[^A-Za-z0-9_]", "");
        if (op.var.Length > 0)
        {
            if (value[0] == '%')
            {
                op.var = '%' + op.var;
            }
            if (value[0] == '@')
            {
                op.var = '@' + op.var;
            }
            if (char.IsNumber(op.var[0]) || op.var[0] == '-' || op.var[0] == '.')
            {
                op.var = "var" + op.var;
            }
            if (test)
            {
                if (testable.Tests.VarTestsComponents.Count == 0)
                {
                    testable.Tests.Add(op);
                }
                else
                {
                    testable.Tests.Add(new VarTestsLogicalOperator());
                    testable.Tests.Add(op);
                }
            }
            else
            {
                testable.Operations.Add(op);
            }
        }
        updateAction.Invoke();
    }

    private static void SetTestOpp(VarOperation op, Action updateAction)
    {
        UIWindowSelectionList select = new UIWindowSelectionList(delegate (string s) { SelectSetOp(op, s, updateAction); }, CommonStringKeys.SELECT_OP, true);

        @select.AddItem("==");
        @select.AddItem("!=");
        @select.AddItem(">=");
        @select.AddItem("<=");
        @select.AddItem(">");
        @select.AddItem("<");

        @select.Draw();
    }

    public static void SelectSetOp(VarOperation op, string operation, Action updateAction)
    {
        if (operation == null)
        {
            updateAction.Invoke();
            return;
        }
        op.operation = operation;
        updateAction.Invoke();
    }

    public static void SetValue(VarOperation op, Action updateAction)
    {
        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(s => SelectSetValue(op, s, updateAction),
            new StringKey("val", "SELECT", CommonStringKeys.VALUE), true);

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { "Quest" });
        @select.AddItem("{" + CommonStringKeys.NUMBER.Translate() + "}", "{NUMBER}", traits, true);

        AddQuestVars(@select);

        traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(CommonStringKeys.TYPE.Translate(), new string[] { "#" });

        @select.AddItem("#monsters", traits);
        @select.AddItem("#heroes", traits);
        @select.AddItem("#round", traits);
        @select.AddItem("#eliminated", traits);
        foreach (ContentPack pack in Game.Get().cd.allPacks)
        {
            if (pack.id.Length > 0)
            {
                @select.AddItem("#" + pack.id, traits);
            }
        }
        @select.Draw();
    }

    private static void SelectSetValue(VarOperation op, string value, Action updateAction)
    {
        if (value == null)
        {
            updateAction.Invoke();
            return;
        }

        if (value.Equals("{NUMBER}"))
        {
            // Vars doesnt localize
            new QuestEditorTextEdit(
                new StringKey("val", "X_COLON", CommonStringKeys.NUMBER),
                "", s => SetNumValue(op, s, updateAction)).EditText();
        }
        else
        {
            op.value = value;
            updateAction.Invoke();
        }
    }

    private static void SetNumValue(VarOperation op, string stringValue, Action updateAction)
    {
        if (stringValue.StartsWith("#rand"))
        {
            // rand integer value

            string randLimit = stringValue.Substring(5);
            int value;
            int.TryParse(randLimit, out value);

            // The minimal random number is 1
            if (value == 0)
            {
                value = 1;
            }

            op.value = "#rand" + value;

        }
        else
        {
            // float value
            float value;
            float.TryParse(stringValue, out value);
            op.value = value.ToString();
        }
        updateAction.Invoke();
    }

    private static void RemoveOp(VarTests tests, int index, Action updateAction)
    {
        if (index < tests.VarTestsComponents.Count)
            tests.Remove(index);
        updateAction.Invoke();
    }
}
