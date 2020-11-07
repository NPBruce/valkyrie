using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Content
{
    public class EventButtonData
    {
        public EventButtonData(List<string> eventNames = null, VarOperation condition = null,
            EventButtonAction? rawConditionFailedAction = null)
        {
            EventNames = eventNames ?? new List<string>();
            RawConditionFailedAction = rawConditionFailedAction;
            Condition = condition;
        }

        public List<string> EventNames { get; }

        public VarOperation Condition { get; }

        public EventButtonAction ConditionFailedAction => RawConditionFailedAction ??
                                                     (HasCondition
                                                         ? EventButtonAction.DISABLE
                                                         : EventButtonAction.NONE);

        protected internal EventButtonAction? RawConditionFailedAction { get; }

        public bool HasCondition => Condition != null;

        public override string ToString()
        {
            return EventButtonSerializer.ToString(this);
        }
    }

    public enum EventButtonAction
    {
        NONE,
        DISABLE,
        HIDE
    }

    public class EventButtonSerializer
    {
        private static readonly char[] EVENT_NAME_SEPARATOR = {' '};
        private static readonly char[] EVENT_PARAMETER_SEPARATOR = {','};

        public static EventButtonData FromString(string eventDataString)
        {
            if (string.IsNullOrWhiteSpace(eventDataString))
            {
                return new EventButtonData();
            }

            // Extract event names
            var strings = eventDataString.Split(EVENT_PARAMETER_SEPARATOR);
            if (strings.Length <= 0)
            {
                return new EventButtonData();
            }

            var questNames = strings[0].Split(EVENT_NAME_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            // Skip conditional event parameters if not all parameters are present
            if (strings.Length < 4)
            {
                return new EventButtonData(questNames.ToList());
            }
            
            var conditionString = string.Join(",", strings[1], strings[2], strings[3]);
            VarOperation condition = new VarOperation(conditionString);

            // Optional ButtonAction parameter (defaults to DISABLE, but can be HIDE or NONE as well)
            if (strings.Length > 4 && Enum.TryParse(strings[4], true, out EventButtonAction action))
            {
                return new EventButtonData(questNames.ToList(), condition, action);
            }

            return new EventButtonData(questNames.ToList(), condition);
        }

        public static string ToString(EventButtonData eventButtonData)
        {
            if (eventButtonData?.EventNames == null)
            {
                return string.Empty;
            }

            List<string> result = new List<string> {string.Join(" ", eventButtonData.EventNames)};

            if (eventButtonData.HasCondition)
            {
                result.Add(eventButtonData.Condition.ToString());
            }

            if (eventButtonData.RawConditionFailedAction != null)
            {
                result.Add(eventButtonData.RawConditionFailedAction.ToString().ToLower());
            }

            return string.Join(",", result);
        }
    }
}