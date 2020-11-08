using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Content
{
    public class QuestButtonData
    {
        public static readonly string DEFAULT_COLOR = "white";

        public QuestButtonData(StringKey label, List<string> eventNames = null, VarOperation condition = null,
            QuestButtonAction? rawConditionFailedAction = null)
        {
            Label = label;
            EventNames = eventNames ?? new List<string>();
            RawConditionFailedAction = rawConditionFailedAction;
            Condition = condition;
        }

        public List<string> EventNames { get; }

        public VarOperation Condition { get; }
        
        public StringKey Label { get; set; }

        public string Color { get; set; } = DEFAULT_COLOR;

        public QuestButtonAction ConditionFailedAction => RawConditionFailedAction ??
                                                     (HasCondition
                                                         ? QuestButtonAction.DISABLE
                                                         : QuestButtonAction.NONE);

        protected internal QuestButtonAction? RawConditionFailedAction { get; }

        public bool HasCondition => Condition != null;

        public override string ToString()
        {
            return QuestButtonDataSerializer.ToEventString(this);
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

        public static QuestButtonData FromString(StringKey labelKey, string eventDataString)
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

            // Skip conditional event parameters if not all parameters are present
            if (strings.Length < 4)
            {
                return new QuestButtonData(labelKey, questNames.ToList());
            }
            
            var conditionString = string.Join(",", strings[1], strings[2], strings[3]);
            VarOperation condition = new VarOperation(conditionString);

            // Optional ButtonAction parameter (defaults to DISABLE, but can be HIDE or NONE as well)
            if (strings.Length > 4 && Enum.TryParse(strings[4], true, out QuestButtonAction action))
            {
                return new QuestButtonData(labelKey, questNames.ToList(), condition, action);
            }

            return new QuestButtonData(labelKey, questNames.ToList(), condition);
        }

        public static string ToEventString(QuestButtonData questButtonData)
        {
            if (questButtonData?.EventNames == null)
            {
                return string.Empty;
            }

            List<string> result = new List<string> {string.Join(" ", questButtonData.EventNames)};

            if (questButtonData.HasCondition)
            {
                result.Add(questButtonData.Condition.ToString());
            }

            if (questButtonData.RawConditionFailedAction != null)
            {
                result.Add(questButtonData.RawConditionFailedAction.ToString().ToLower());
            }

            return string.Join(",", result);
        }
    }
}