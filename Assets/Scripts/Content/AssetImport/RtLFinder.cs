using UnityEngine;
using System.Collections;

public class RtLFinder : AppFinder {

    override public string AppId()
    {
        return "477200";
    }
    override public string Destination()
    {
        return "D2E";
    }

    override public string DataDirectory()
    {
        return "/Road to Legend_Data";
    }
}
