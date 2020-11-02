using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Content
{
    public class NextEventData
    {
        public NextEventData(List<string> eventNames = null, VarOperation condition = null,
            ButtonAction? rawAction = null)
        {
            EventNames = eventNames ?? new List<string>();
            RawAction = rawAction;
            Condition = condition;
        }

        public List<string> EventNames { get; }

        public VarOperation Condition { get; }

        public ButtonAction ConditionFailedAction => RawAction ??
                                                     (HasCondition
                                                         ? ButtonAction.DISABLE
                                                         : ButtonAction.NONE);

        protected internal ButtonAction? RawAction { get; }

        public bool HasCondition => Condition != null;

        public override string ToString()
        {
            return NextEventDataSerializer.ToString(this);
        }
    }

    public enum ButtonAction
    {
        NONE,
        DISABLE,
        HIDE
    }

    public class NextEventDataSerializer
    {
        private static readonly char[] EVENT_NAME_SEPARATOR = {' '};
        private static readonly char[] EVENT_PARAMETER_SEPARATOR = {','};

        public static NextEventData FromString(string eventDataString)
        {
            if (string.IsNullOrWhiteSpace(eventDataString))
            {
                return new NextEventData();
            }

            // Extract event names
            var strings = eventDataString.Split(EVENT_PARAMETER_SEPARATOR);
            if (strings.Length <= 0)
            {
                return new NextEventData();
            }

            var questNames = strings[0].Split(EVENT_NAME_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            // Skip conditional event parameters if not all parameters are present
            if (strings.Length < 4)
            {
                return new NextEventData(questNames.ToList());
            }
            
            var conditionString = string.Join(",", strings[1], strings[2], strings[3]);
            VarOperation condition = new VarOperation(conditionString);

            // Optional ButtonAction parameter (defaults to DISABLE, but can be HIDE or NONE as well)
            if (strings.Length > 4 && Enum.TryParse(strings[4], true, out ButtonAction action))
            {
                return new NextEventData(questNames.ToList(), condition, action);
            }

            return new NextEventData(questNames.ToList(), condition);
        }

        public static string ToString(NextEventData eventData)
        {
            if (eventData?.EventNames == null)
            {
                return string.Empty;
            }

            List<string> result = new List<string> {string.Join(" ", eventData.EventNames)};

            if (eventData.HasCondition)
            {
                result.Add(eventData.Condition.ToString());
            }

            if (eventData.RawAction != null)
            {
                result.Add(eventData.RawAction.ToString().ToLower());
            }

            return string.Join(",", result);
        }
    }
}