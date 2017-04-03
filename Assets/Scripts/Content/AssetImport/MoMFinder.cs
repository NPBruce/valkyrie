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
        return "0.8.2";
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
        if (Application.platform == RuntimePlatform.OSXPlayer)
        {
            return "/Contents/Resources/Data";
        }
        return "/Mansions of Madness_Data";
    }
    override public string Executable()
    {
        if (Application.platform == RuntimePlatform.OSXPlayer)
        {
            return "Mansions of Madness.app";
        }
        return "Mansions of Madness.exe";
    }

    // MoM uses this key to obfuscate text
    override public int ObfuscateKey()
    {
        return 68264378;
    }
}
