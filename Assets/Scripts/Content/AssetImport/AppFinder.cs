using UnityEngine;
using System.Collections;
using Microsoft.Win32;
using System.IO;
using System;
using Read64bitRegistryFrom32bitApp;

// Class to find FFG app installed
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
    public abstract int ObfuscateKey();

    public AppFinder()
    {
        // Attempt to get steam install location (current 32/64 level)
        location = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App " + AppId(), "InstallLocation", "");
        if (location.Equals(""))
        {
            // If we are on a 64 bit system, need to read the 64bit registry from a 32 bit app (Valkyrie)
            try
            {
                location = RegistryWOW6432.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App " + AppId(), "InstallLocation");
            }
            catch (Exception) { }
        }

        exeLocation += location + "/" + Executable();
        location += DataDirectory();
    }

    // Read version of executable from app
    // Note: This is usually not updated by FFG and is not used for validity checks
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