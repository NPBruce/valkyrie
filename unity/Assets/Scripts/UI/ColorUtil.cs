using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Util class to convert colour names to RGB strings
// Returns input if not found
public class ColorUtil  {
	public static string FromName(string name) {
        if (LookUp().ContainsKey(name.ToLower()))
        {
            return LookUp()[name.ToLower()];
        }
        // No match found
        return name;
    }

    // Staticly defined dictionary of names to RGB strings
    // Data should match web standards
    public static Dictionary<string, string> LookUp()
    {
        Dictionary<string, string> lookUp = new Dictionary<string, string>();

        lookUp.Add("black", "#000000");
        lookUp.Add("white", "#FFFFFF");
        lookUp.Add("red", "#FF0000");
        lookUp.Add("lime", "#00FF00");
        lookUp.Add("blue", "#0000FF");
        lookUp.Add("yellow", "#FFFF00");
        lookUp.Add("aqua", "#00FFFF");
        lookUp.Add("cyan", "#00FFFF");
        lookUp.Add("magenta", "#FF00FF");
        lookUp.Add("fuchsia", "#FF00FF");
        lookUp.Add("silver", "#C0C0C0");
        lookUp.Add("gray", "#808080");
        lookUp.Add("maroon", "#800000");
        lookUp.Add("olive", "#808000");
        lookUp.Add("green", "#008000");
        lookUp.Add("purple", "#800080");
        lookUp.Add("teal", "#008080");
        lookUp.Add("navy", "#000080");

        return lookUp;
    }
}
