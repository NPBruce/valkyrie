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
        public static readonly StringKey EMPTY = new StringKey(VAL, "EMPTY");
        public static readonly StringKey CLOSE = new StringKey("val", "CLOSE");
    }
}
