using System.Collections;
using Microsoft.Win32;
using System.IO;
using System;
using System.Text;
using Read64bitRegistryFrom32bitApp;
using ValkyrieTools;

namespace FFGAppImport
{
    // Class to find FFG app installed
    abstract public class AppFinder
    {
        public abstract string AppId();
        public abstract string Destination();
        public abstract string DataDirectory();
        public abstract string Executable();
        public abstract string RequiredFFGVersion();
        public string location = "";
        public string exeLocation;
        public abstract int ObfuscateKey();

        public Platform platform;

        public AppFinder(Platform p)
        {
            platform = p;
            if (p == Platform.MacOS)
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


                string output = outputBuilder.ToString();

                ValkyrieDebug.Log("Looking for: " + "/" + Executable());
                // Quick hack rather than doing XML properly
                int foundAt = output.IndexOf("/" + Executable());
                if (foundAt > 0)
                {
                    ValkyrieDebug.Log("Name Index: " + foundAt);
                    int startPos = output.LastIndexOf("<string>", foundAt) + 8;
                    ValkyrieDebug.Log("Start Index: " + startPos);
                    location = output.Substring(startPos, output.IndexOf("</string>", startPos) - startPos).Trim();
                    ValkyrieDebug.Log("Using location: " + location);
                }
            }
            else if (platform == Platform.Linux)
            {

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

            if (location == null || location.Length == 0)
            {
                string[] args = System.Environment.GetCommandLineArgs();
                for (int i = 0; i < (args.Length - 1); i++)
                {
                    if (args[i] == "-import")
                    {
                        location = args[i + 1];
                        if (location.Length > 0)
                        {
                            if (location[location.Length - 1] == '/' || location[location.Length - 1] == '\\')
                            {
                                location = location.Substring(0, location.Length - 1);
                            }
                        }
                        ValkyrieDebug.Log("Using import flag location: " + location);
                    }
                }
            }
            exeLocation += location + "/" + Executable();
            location += DataDirectory();
            ValkyrieDebug.Log("Asset location: " + location);
        }
    }
}