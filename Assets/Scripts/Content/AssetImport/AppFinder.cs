using UnityEngine;
using System.Collections;
using Microsoft.Win32;
using System.IO;

abstract public class AppFinder
{
    public abstract string AppId();
    public abstract string Destination();
    public abstract string DataDirectory();
    public string location;

    public AppFinder()
    {
        location = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App " + AppId(), "InstallLocation", "");
        location += DataDirectory();
        if (Directory.Exists(location))
        {
            Debug.Log("Found App installation at: " + location);
        }
    }
}
