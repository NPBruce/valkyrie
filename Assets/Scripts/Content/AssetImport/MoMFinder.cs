using UnityEngine;
using System.Collections;

public class MoMFinder : AppFinder {
    override public string AppId()
    {
        return "478980";
    }
    override public string Destination()
    {
        return "MoM";
    }
    override public string DataDirectory()
    {
        return "/Mansions of Madness_Data";
    }
    override public string Executable()
    {
        return "Mansions of Madness.exe";
    }
}
