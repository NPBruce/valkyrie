using UnityEngine;
using System.Collections;
using Microsoft.Win32;
using System.IO;
using System;
using System.Text;
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
        if (Application.platform == RuntimePlatform.OSXPlayer)
        {
            ValkyrieDebug.Log("Attempting to locate AppId " + AppId() + " on MacOS.");
            System.Diagnostics.ProcessStartInfo processStartInfo;
            System.Diagnostics.Process process;

            StringBuilder outputBuilder = new StringBuilder();

            processStartInfo = new System.Diagnostics.ProcessStartInfo();
            processStartInfo.CreateNoWindow = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.Arguments = "SPApplicationsDataType -xml";
            processStartInfo.FileName = "system_profiler";

            process = new System.Diagnostics.Process();
            ValkyrieDebug.Log("Starting system_profiler.");
            process.StartInfo = processStartInfo;
            // enable raising events because Process does not raise events by default
            process.EnableRaisingEvents = true;
            // attach the event handler for OutputDataReceived before starting the process
            process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler
            (
                delegate (object sender, System.Diagnostics.DataReceivedEventArgs e)
                {
                    // append the new data to the data already read-in
                    outputBuilder.Append(e.Data);
                }
            );
            // start the process
            // then begin asynchronously reading the output
            // then wait for the process to exit
            // then cancel asynchronously reading the output
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            process.CancelOutputRead();

            
            string[] output = outputBuilder.ToString().Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            ValkyrieDebug.Log("Number of lines returned: " + output.Count);
            ValkyrieDebug.Log("Looking for: " + "/" + Executable());
            // Quick hack rather than doing XML properly
            foreach (string s in output)
            {

                if (s.IndexOf("/" + Executable()) > 0)
                {
                    ValkyrieDebug.Log("Found Line: " + s);
                    location = s.Trim();
                    // Removing <string> and </string>
                    location = location.Substring(8, location.Length - 17)
                    ValkyrieDebug.Log("Using location: " + location);
                }
            }
            if (locaton.Length == 0)
            {
                string xmlFile = Application.dataPath + "/apps.xml"
                ValkyrieDebug.Log("Could not find, looking for prefetched XML: " + xmlFile);
                if (File.Exists(xmlFile))
                {
                    string[] lines = System.IO.File.ReadAllLines(xmlFile);
                    ValkyrieDebug.Log("Number of lines returned: " + output.Count);
                    ValkyrieDebug.Log("Looking for: " + "/" + Executable());
                    // Quick hack rather than doing XML properly
                    foreach (string s in lines)
                    {

                        if (s.IndexOf("/" + Executable()) > 0)
                        {
                            ValkyrieDebug.Log("Found Line: " + s);
                            location = s.Trim();
                            // Removing <string> and </string>
                            location = location.Substring(8, location.Length - 17)
                            ValkyrieDebug.Log("Using location: " + location);
                        }
                    }
                }
                else
                {
                    ValkyrieDebug.Log("Could not find prefetched XML.");
                }
            }
            if (locaton.Length == 0)
            {
                location = "~/Library/Application Support/Steam/steamapps/common/Mansions of Madness/Mansions of Madness.app";
                ValkyrieDebug.Log("Could not find, using magic locatoin: " + location);
            }
        }
        else
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
        }

        exeLocation += location + "/" + Executable();
        location += DataDirectory();
        ValkyrieDebug.Log("Asset location: " + location);
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