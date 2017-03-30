using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Content
{
    public class CommonStringKeys
    {
        private static string VAL = "val";

        public static readonly StringKey BACK = new StringKey(VAL, "BACK");
        public static readonly StringKey CLOSE = new StringKey(VAL, "CLOSE");
        public static readonly StringKey EXIT = new StringKey(VAL,"EXIT");
        public static readonly StringKey E = new StringKey(VAL, "E");
        public static readonly StringKey POSITION = new StringKey(VAL, "POSITION");
        public static readonly StringKey POSITION_SNAP = new StringKey("><",false);
        public static readonly StringKey POSITION_FREE = new StringKey("~", false);
        public static readonly StringKey PLUS = new StringKey("+", false);
        public static readonly StringKey MINUS = new StringKey("+", false);
        public static readonly StringKey EVENT = new StringKey(VAL, "EVENT");
        public static readonly StringKey CANCEL = new StringKey(VAL, "CANCEL");
        public static readonly StringKey TRAITS_DOTS = new StringKey(VAL, "TRAITS_DOTS");
        public static readonly StringKey PLACEMENT = new StringKey(VAL, "PLACEMENT");
        public static readonly StringKey FINISHED = new StringKey("val", "FINISHED");

        public static readonly StringKey SKILL_DOTS = new StringKey(VAL, "SKILL_DOTS");
        public static readonly StringKey MOVES_DOTS = new StringKey(VAL, "MOVES_DOTS");
        public static readonly StringKey TOTAL_MOVES_DOTS = new StringKey(VAL, "TOTAL_MOVES_DOTS");

        public static readonly StringKey TYPE = new StringKey(VAL, "TYPE");
        public static readonly StringKey SELECT_ITEM = new StringKey(VAL, "SELECT_ITEM");

        public static readonly StringKey QUEST = new StringKey(VAL, "QUEST");
        public static readonly StringKey TILE = new StringKey(VAL, "TILE");
        public static readonly StringKey DELETE = new StringKey(VAL, "DELETE");
        public static readonly StringKey DOOR = new StringKey(VAL, "DOOR");
        public static readonly StringKey TOKEN = new StringKey(VAL, "TOKEN");
        public static readonly StringKey MONSTER = new StringKey(VAL, "MONSTER");
        public static readonly StringKey MPLACE = new StringKey(VAL, "MPLACE");
        public static readonly StringKey PUZZLE = new StringKey(VAL, "PUZZLE");
        public static readonly StringKey ITEM = new StringKey(VAL, "ITEM");
        public static readonly StringKey UNIQUE_MONSTER = new StringKey(VAL, "UNIQUE_MONSTER");
        public static readonly StringKey ACTIVATION = new StringKey(VAL, "ACTIVATION");
        public static readonly StringKey NUMBER = new StringKey(VAL, "NUMBER");
        public static readonly StringKey TRIGGER = new StringKey(VAL, "TRIGGER");
        public static readonly StringKey TAB = new StringKey("->",false);
        public static readonly StringKey LOG = new StringKey(VAL, "LOG");
        public static readonly StringKey SET = new StringKey(VAL, "SET");
        public static readonly StringKey RESET = new StringKey(VAL, "RESET");
    }
}
