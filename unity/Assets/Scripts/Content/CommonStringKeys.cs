using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Content
{
    public class CommonStringKeys
    {
        private static string VAL = "val";

        /// <summary>
        /// StringKey with pattern {0}:
        /// </summary>
        public static readonly StringKey X_COLON = new StringKey(VAL, "X_COLON");
        public static readonly StringKey BACK = new StringKey(VAL, "BACK");
        public static readonly StringKey CLOSE = new StringKey(VAL, "CLOSE");
        public static readonly StringKey EXIT = new StringKey(VAL,"EXIT");
        public static readonly StringKey E = new StringKey(VAL, "E");
        public static readonly StringKey POSITION = new StringKey(VAL, "POSITION");
        public static readonly StringKey POSITION_SNAP = new StringKey(VAL, "POSITION_SNAP");
        public static readonly StringKey POSITION_FREE = new StringKey(VAL, "POSITION_FREE");
        public static readonly StringKey PLUS = new StringKey(null,"+", false);
        public static readonly StringKey MINUS = new StringKey(null,"-", false);
        public static readonly StringKey CONDITION = new StringKey(null,"?", false);
        public static readonly StringKey HASH = new StringKey(null,"#", false);
        public static readonly StringKey TAB = new StringKey(null, "➤", false);
        public static readonly StringKey UP = new StringKey(null, "▲", false);
        public static readonly StringKey DOWN = new StringKey(null, "▼", false);
        public static readonly StringKey EVENT = new StringKey(VAL, "EVENT");
        public static readonly StringKey OK = new StringKey(VAL, "OK");
        public static readonly StringKey CANCEL = new StringKey(VAL, "CANCEL");
        public static readonly StringKey NONE = new StringKey(VAL, "NONE");
        public static readonly StringKey TRAITS = new StringKey(VAL, "TRAITS");
        public static readonly StringKey PLACEMENT = new StringKey(VAL, "PLACEMENT");
        public static readonly StringKey CONTINUE = new StringKey("val", "CONTINUE");

        public static readonly StringKey FINISHED = new StringKey(VAL, "FINISHED");

        public static readonly StringKey SKILL = new StringKey(VAL, "SKILL");
        public static readonly StringKey MOVES = new StringKey(VAL, "MOVES");
        public static readonly StringKey TOTAL_MOVES = new StringKey(VAL, "TOTAL_MOVES");

        public static readonly StringKey TRUE = new StringKey(VAL, "TRUE");
        public static readonly StringKey FALSE = new StringKey(VAL, "FALSE");
        public static readonly StringKey TESTS = new StringKey("val", "TESTS");
        public static readonly StringKey VAR = new StringKey("val", "VAR");
        public static readonly StringKey OP = new StringKey("val", "OP");
        public static readonly StringKey VALUE = new StringKey("val", "VALUE");
        public static readonly StringKey ASSIGN = new StringKey("val", "ASSIGN");
        public static readonly StringKey VAR_NAME = new StringKey("val", "VAR_NAME");
        public static readonly StringKey AND = new StringKey("val", "AND");
        public static readonly StringKey OR = new StringKey("val", "OR");

        public static readonly StringKey FILE = new StringKey(VAL, "FILE");
        public static readonly StringKey BASE = new StringKey(VAL, "BASE");
        public static readonly StringKey TYPE = new StringKey(VAL, "TYPE");
        public static readonly StringKey SOURCE = new StringKey(VAL, "SOURCE");
        public static readonly StringKey SELECT_ITEM = new StringKey(VAL, "SELECT_ITEM");
        public static readonly StringKey SELECT_VAR = new StringKey(VAL, "SELECT", VAR);
        public static readonly StringKey SELECT_OP = new StringKey(VAL, "SELECT", OP);
        public static readonly StringKey SELECT_FILE = new StringKey(VAL, "SELECT", FILE);
        public static readonly StringKey CHOOSE_LANGUAGE = new StringKey(VAL, "CHOOSE_LANG");

        public static readonly StringKey QUEST = new StringKey(VAL, "QUEST");
        public static readonly StringKey TILE = new StringKey(VAL, "TILE");
        public static readonly StringKey DELETE = new StringKey(VAL, "DELETE");
        public static readonly StringKey RENAME = new StringKey(VAL, "RENAME");
        public static readonly StringKey UPDATE = new StringKey(VAL, "UPDATE");
        public static readonly StringKey COPY = new StringKey(VAL, "COPY");
        public static readonly StringKey CONFIRM = new StringKey(VAL, "CONFIRM");
        public static readonly StringKey NEW = new StringKey(VAL, "NEW");
        public static readonly StringKey DOOR = new StringKey(VAL, "DOOR");
        public static readonly StringKey TOKEN = new StringKey(VAL, "TOKEN");
        public static readonly StringKey UI = new StringKey(VAL, "UI");
        public static readonly StringKey MONSTER = new StringKey(VAL, "MONSTER");
        public static readonly StringKey SPAWN = new StringKey(VAL, "SPAWN");
        public static readonly StringKey MPLACE = new StringKey(VAL, "MPLACE");
        public static readonly StringKey PUZZLE = new StringKey(VAL, "PUZZLE");
        public static readonly StringKey ITEM = new StringKey(VAL, "ITEM");
        public static readonly StringKey QITEM = new StringKey(VAL, "QITEM");
        public static readonly StringKey STARTING_ITEM = new StringKey(VAL, "STARTING_ITEM");
        public static readonly StringKey UNIQUE_MONSTER = new StringKey(VAL, "UNIQUE_MONSTER");
        public static readonly StringKey CUSTOMMONSTER = new StringKey(VAL, "CUSTOMMONSTER");
        public static readonly StringKey ACTIVATION = new StringKey(VAL, "ACTIVATION");
        public static readonly StringKey NUMBER = new StringKey(VAL, "NUMBER");
        public static readonly StringKey TRIGGER = new StringKey(VAL, "TRIGGER");

        public static readonly StringKey LOG = new StringKey(VAL, "LOG");
        public static readonly StringKey SET = new StringKey(VAL, "SET");
        public static readonly StringKey RESET = new StringKey(VAL, "RESET");
        public static readonly StringKey LOADINGSCENARIOS = new StringKey(VAL, "LOADINGSCENARIOS");
        public static readonly StringKey LOADINGCONTENTPACKS = new StringKey(VAL, "LOADINGCONTENTPACKS");
        public static readonly StringKey REQUIRED = new StringKey(VAL, "REQUIRED");

        public static readonly StringKey SIZE = new StringKey(VAL, "SIZE");
        public static readonly StringKey DEFAULT = new StringKey(VAL, "DEFAULT");
        public static readonly StringKey SMALL = new StringKey(VAL, "SMALL");
        public static readonly StringKey MEDIUM = new StringKey(VAL, "MEDIUM");
        public static readonly StringKey HUGE = new StringKey(VAL, "HUGE");
        public static readonly StringKey MASSIVE = new StringKey(VAL, "MASSIVE");

    }
}
