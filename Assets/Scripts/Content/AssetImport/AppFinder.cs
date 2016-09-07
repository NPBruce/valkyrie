using UnityEngine;
using System.Collections;
using Microsoft.Win32;
using System.IO;
using System;
using Read64bitRegistryFrom32bitApp;

abstract public class AppFinder
{
    public abstract string AppId();
    public abstract string Destination();
    public abstract string DataDirectory();
    public abstract string Executable();
    public abstract string RequiredFFGVersion();
    public abstract string RequiredValkyrieVersion();
    public string location;
    public string exeLocation;

    public AppFinder()
    {
        location = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App " + AppId(), "InstallLocation", "");
        if (location.Equals(""))
        {
            try
            {
                location = RegistryWOW6432.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App " + AppId(), "InstallLocation");
            }
            catch (Exception) { }
        }

        exeLocation += location + "/" + Executable();
        location += DataDirectory();
    }

    public string AppVersion()
    {
        string ffgVersion = "";
        try
        {
            System.Diagnostics.FileVersionInfo info = System.Diagnostics.FileVersionInfo.GetVersionInfo(exeLocation);
            ffgVersion = info.ProductVersion;
        }
        catch (System.Exception) { }
        return ffgVersion;
    }
}