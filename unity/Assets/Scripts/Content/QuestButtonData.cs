using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Assets.Scripts.Content
{
    public class QuestButtonData: ITestable
    {
        public static readonly string DEFAULT_COLOR = "white";

        public QuestButtonData(StringKey label, List<string> eventNames = null, VarTests condition = null,
            QuestButtonAction? rawConditionFailedAction = null)
        {
            Label = label;
            EventNames = eventNames ?? new List<string>();
            RawConditionFailedAction = rawConditionFailedAction;
            Condition = condition;
        }

        public List<string> EventNames { get; }

        public VarTests Condition { get; internal set; }
        
        public StringKey Label { get; set; }

        public string Color { get; set; } = DEFAULT_COLOR;

        public QuestButtonAction ConditionFailedAction => RawConditionFailedAction ??
                                                     (HasCondition
                                                         ? QuestButtonAction.DISABLE
                                                         : QuestButtonAction.NONE);

        protected internal QuestButtonAction? RawConditionFailedAction { get; internal set; }

        public bool HasCondition => Condition != null;

        // ITestable implementation
        public VarTests Tests => Condition;
        public List<VarOperation> Operations { get; } = new List<VarOperation>();
        
        public override string ToString()
        {
            return QuestButtonDataSerializer.ToEventString(0, this);
        }
    }

    public enum QuestButtonAction
    {
        NONE,
        DISABLE,
        HIDE
    }

    public class QuestButtonDataSerializer
    {
        private static readonly char[] EVENT_NAME_SEPARATOR = {' '};
        private static readonly char[] EVENT_PARAMETER_SEPARATOR = {','};


        public static QuestButtonData FromData(Dictionary<string, string> data, int position, string sectionName)
        {
            var buttonLabel = ReadButtonLabel(data, position, sectionName);
            QuestButtonData questButtonData = null;
            if (data.TryGetValue("event" + position, out var nextEventString))
            {
                questButtonData = FromSingleString(buttonLabel, nextEventString);
            }
            else
            {
                questButtonData = new QuestButtonData(buttonLabel, new List<string>());
            }

            if (data.TryGetValue($"event{position}Condition" , out var conditionString) && !string.IsNullOrWhiteSpace(conditionString))
            {
                var tests = new VarTests();
                string[] array = conditionString.Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in array)
                {
                    tests.Add(s);
                }

                questButtonData.Condition = tests;

                if (data.TryGetValue($"event{position}ConditionAction", out var conditionActionString) 
                    && !string.IsNullOrWhiteSpace(conditionActionString) 
                    && Enum.TryParse(conditionActionString, true, out QuestButtonAction action))
                {
                    questButtonData.RawConditionFailedAction = action;
                }
            }
            
            if (data.ContainsKey("buttoncolor" + position))
            {
                questButtonData.Color = data["buttoncolor" + position];
            }
            
            return questButtonData;
        }
        
        public static QuestButtonData FromSingleString(StringKey labelKey, string eventDataString)
        {
            if (string.IsNullOrWhiteSpace(eventDataString))
            {
                return new QuestButtonData(labelKey);
            }

            // Extract event names
            var strings = eventDataString.Split(EVENT_PARAMETER_SEPARATOR);
            if (strings.Length <= 0)
            {
                return new QuestButtonData(labelKey);
            }

            var questNames = strings[0].Split(EVENT_NAME_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            // Backward compatibility with unreleased 2.4.11a
            
            // Skip conditional event parameters if not all parameters are present
            if (strings.Length < 4)
            {
                return new QuestButtonData(labelKey, questNames.ToList());
            }
            
            var conditionString = string.Join(",", strings[1], strings[2], strings[3]);
            VarOperation condition = new VarOperation(conditionString);
            var varTests = new VarTests();
            varTests.Add(condition);

            // Optional ButtonAction parameter (defaults to DISABLE, but can be HIDE or NONE as well)
            if (strings.Length > 4 && Enum.TryParse(strings[4], true, out QuestButtonAction action))
            {
                return new QuestButtonData(labelKey, questNames.ToList(), varTests, action);
            }

            return new QuestButtonData(labelKey, questNames.ToList(), varTests);
        }

        private static StringKey ReadButtonLabel(Dictionary<string, string> data, int buttonNum, string sectionName)
        {
            var buttonDataKey = "button" + (buttonNum);
            if (data.TryGetValue(buttonDataKey, out string buttonDataValue))
            {
                return new StringKey(buttonDataValue);
            }

            return new StringKey("qst", $"{sectionName}.button{buttonNum}");
        }


        public static string ToEventString(int position, QuestButtonData questButtonData)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"event{position}={SerializeEventNames(questButtonData.EventNames)}").AppendLine();
            if (questButtonData.HasCondition)
            {
                sb.Append($"event{position}Condition={questButtonData.Condition}").AppendLine();
                if (questButtonData.RawConditionFailedAction != null)
                {
                    sb.Append($"event{position}ConditionAction={questButtonData.RawConditionFailedAction.ToString().ToLower()}").AppendLine();
                }
            }
            if (!questButtonData.Color.Equals(QuestButtonData.DEFAULT_COLOR))
            {
                sb.Append($"buttoncolor{position}=\"{questButtonData.Color}\"").AppendLine();
            }

            return sb.ToString();
        }

        private static string SerializeEventNames(List<string> eventNames)
        {
            if (eventNames == null)
            {
                return "";
            }

            return String.Join(" ", eventNames);
        }
    }
}