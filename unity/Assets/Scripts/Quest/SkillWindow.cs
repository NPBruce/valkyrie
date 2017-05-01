using Assets.Scripts.Content;
using UnityEngine;
using System.Collections.Generic;

// Next stage button is used by MoM to move between investigators and monsters
public class SkillWindow
{
    public Dictionary<string, DialogBoxEditable> valueDBE;

    public bool developerToggle = false;

    // Construct and display
    public SkillWindow()
    {
        Update();
    }

    public void Update(bool toggle = false)
    {
        Destroyer.Dialog();
    }
}
