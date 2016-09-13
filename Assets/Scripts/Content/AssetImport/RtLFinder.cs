using UnityEngine;
using System.Collections;

public class RtLFinder : AppFinder {

    override public string RequiredFFGVersion()
    {
        return "1.1.7";
    }

    override public string RequiredValkyrieVersion()
    {
        return "0.2.3";
    }

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
    override public string Executable()
    {
        return "Road to Legend.exe";
    }
}
