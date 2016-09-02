using UnityEngine;
using System.Collections;
using Microsoft.Win32;

abstract public class AppFinder
{
    public abstract string AppId();
    public abstract string Destination();

    public AppFinder()
    {
        string location = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App " + AppId(), "InstallLocation", "");
    }
}
