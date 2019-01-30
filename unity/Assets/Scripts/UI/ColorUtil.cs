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

    public static Color ColorFromName(string name)
    {
        string colorRGB = FromName(name);
        // Check format is valid
        if ((colorRGB.Length != 7 && colorRGB.Length != 9) || (colorRGB[0] != '#'))
        {
            Game.Get().quest.log.Add(new Quest.LogEntry("Warning: Color must be in #RRGGBB format or a known name: " + name, true));
        }

        // State with white (used for alpha)
        Color colour = Color.white;
        // Hexadecimal to float convert (0x00-0xFF -> 0.0-1.0)
        colour[0] = (float)System.Convert.ToInt32(colorRGB.Substring(1, 2), 16) / 255f;
        colour[1] = (float)System.Convert.ToInt32(colorRGB.Substring(3, 2), 16) / 255f;
        colour[2] = (float)System.Convert.ToInt32(colorRGB.Substring(5, 2), 16) / 255f;
        if(colorRGB.Length == 9)
            colour[3] = (float)System.Convert.ToInt32(colorRGB.Substring(7, 2), 16) / 255f;

        return colour;
    }

    // Staticly defined dictionary of names to RGB strings
    // Data should match web standards
    public static Dictionary<string, string> LookUp()
    {
        Dictionary<string, string> lookUp = new Dictionary<string, string>();

        lookUp.Add("black",       "#000000");
        lookUp.Add("white",       "#FFFFFF");
        lookUp.Add("red",         "#FF0000");
        lookUp.Add("lime",        "#00FF00");
        lookUp.Add("blue",        "#0000FF");
        lookUp.Add("yellow",      "#FFFF00");
        lookUp.Add("aqua",        "#00FFFF");
        lookUp.Add("cyan",        "#00FFFF");
        lookUp.Add("magenta",     "#FF00FF");
        lookUp.Add("fuchsia",     "#FF00FF");
        lookUp.Add("silver",      "#C0C0C0");
        lookUp.Add("gray",        "#808080");
        lookUp.Add("maroon",      "#800000");
        lookUp.Add("olive",       "#808000");
        lookUp.Add("green",       "#008000");
        lookUp.Add("purple",      "#800080");
        lookUp.Add("teal",        "#008080");
        lookUp.Add("navy",        "#000080");
        lookUp.Add("transparent", "#00000000");

        return lookUp;
    }
}
