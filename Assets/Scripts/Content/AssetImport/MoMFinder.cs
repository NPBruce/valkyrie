using UnityEngine;
using System.Collections;

public class MoMFinder : AppFinder {
    override public string RequiredFFGVersion()
    {
        return "1.3.0";
    }
    override public string RequiredValkyrieVersion()
    {
        return "0.5.4";
    }
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

    override public int ObfuscateKey()
    {
        return 68264378;
    }
}
