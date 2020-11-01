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
        protected ButtonAction? RawAction { get; }

        public bool HasCondition => Condition != null;
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

            var strings = eventDataString.Split(EVENT_PARAMETER_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length <= 0)
            {
                return new NextEventData();
            }

            var questNames = strings[0].Split(EVENT_NAME_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            return new NextEventData(questNames.ToList());
        }

        public static string ToString(NextEventData eventData)
        {
            if (eventData?.EventNames == null)
            {
                return string.Empty;
            }

            return string.Join(" ", eventData.EventNames);
        }
    }

    public enum ButtonAction
    {
        NONE,
        DISABLE,
        HIDE
    }
}