using Assets.Scripts.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI
{
    //sorted_by_rating
    //sorted_by_name
    //sorted_by_difficulty
    //sorted_by_duration
    //sorted_by_date
    internal class SortOption
    {
        public string name;
        public StringKey button_text;

        public SortOption(string p_name, StringKey p_button_text)
        {
            name = p_name;
            button_text = p_button_text;
        }
    }
}
