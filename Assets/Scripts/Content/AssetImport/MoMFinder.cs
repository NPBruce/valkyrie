using UnityEngine;
using System.Collections;

// Details for FFG MoM app
public class MoMFinder : AppFinder {
    // If the installed app isn't this or higher don't import
    override public string RequiredFFGVersion()
    {
        return "1.3.0";
    }
    // If an import wasn't performed with this Valkyrie version or higher reimport
    override public string RequiredValkyrieVersion()
    {
        return "0.7.2";
    }
    // Steam app ID
    override public string AppId()
    {
        return "478980";
    }
    // Where to store imported data
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

    // MoM uses this key to obfuscate text
    override public int ObfuscateKey()
    {
        return 68264378;
    }
}
