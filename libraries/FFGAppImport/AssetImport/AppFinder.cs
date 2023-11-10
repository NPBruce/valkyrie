using Ionic.Zip;
using Microsoft.Win32;
using Read64bitRegistryFrom32bitApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public abstract string ObbPath();
        public abstract string DataPath();
        public abstract string AuxDataPath();
        public string location = "";
        public string obbRoot = "";
        public string obbVersion = "";
        public string exeLocation;
        public string apkPath;
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

                ValkyrieDebug.Log("Looking for: /" + Executable());
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
            else if (platform == Platform.Android)
            {
                obbRoot = Android.GetStorage() + "/Valkyrie/Obb";
                ValkyrieDebug.Log("Obb extraction path: " + obbRoot);
                location = obbRoot + "/assets/bin/Data";
                DeleteObb();
            }
            else // Windows
            {
                // Attempt to get steam install location (current 32/64 level)
                location = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App " + AppId(), "InstallLocation", "");
                if (location == null || location.Equals(""))
                {
                    // If we are on a 64 bit system, need to read the 64bit registry from a 32 bit app (Valkyrie)
                    try
                    {
                        location = RegistryWOW6432.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App " + AppId(), "InstallLocation");
                    }
                    catch { }
                }
            }

            if (location == null || location.Length == 0)
            {
                string[] args = Environment.GetCommandLineArgs();
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

        internal void DeleteObb()
        {
            if (platform != Platform.Android) return;
            if (!Directory.Exists(obbRoot)) return;
            Directory.Delete(obbRoot, true);
        }

        public void ExtractApk()
        {
            if (platform != Platform.Android) return;

            ValkyrieDebug.Log("Extracting the file " + apkPath + " to " + obbRoot);
            DeleteObb();
            Directory.CreateDirectory(obbRoot);
            using (var zip = ZipFile.Read(apkPath))
            {
                zip.ExtractAll(obbRoot);
            }

        }

        public bool ExtractObb()
        {
            if (platform != Platform.Android) return true;
            //string obbFile = "C:/Users/Bruce/Desktop/Mansions of Madness_v1.3.5_apkpure.com/Android/obb/com.fantasyflightgames.mom/main.598.com.fantasyflightgames.mom.obb";
            //string obbFile = "C:/Users/Bruce/Desktop/Road to Legend_v1.3.1_apkpure.com/Android/obb/com.fantasyflightgames.rtl/main.319.com.fantasyflightgames.rtl.obb";
            string obbPath = ObbPath();
            if (obbPath == "") return false;
            ValkyrieDebug.Log("Extracting the file " + obbPath + " to " + obbRoot);
            DeleteObb();
            try
            {
                Directory.CreateDirectory(obbRoot);
                using (var zip = ZipFile.Read(obbPath))
                {
                    zip.ExtractAll(obbRoot);
                }

                ConvertObbStreamingAssets();
                ConvertObbAssets();
                return true;
            }
            catch (System.Exception e)
            {
                ValkyrieDebug.Log(e.ToString());
                return false;
            }
        }

        internal string ExtractObbVersion()
        {
            if (platform != Platform.Android) return "";

            if (!string.IsNullOrEmpty(obbVersion)) return obbVersion; // lookup version only once
            string obbPath = ObbPath();
            if (obbPath == "") return "";
            ValkyrieDebug.Log("Extracting the version from " + obbPath + ".");
            using (var zip = ZipFile.Read(obbPath))
            {
                using (var e = zip.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        // We want the directory named like the version in the path 'assets/AssetBundles/1.4.1'
                        string searchPath = "assets/AssetBundles/";
                        string filename = e.Current.FileName;
                        if (filename.StartsWith(searchPath) && filename.Count((c) => c == '/') > 2)
                        {
                            string version = filename.Remove(0, searchPath.Length);
                            version = version.Remove(version.IndexOf("/"));
                            ValkyrieDebug.Log("Version from Obb: " + version);
                            obbVersion = version;
                            return obbVersion;
                        }
                    }
                }
            }
            ValkyrieDebug.Log("Extracting the version from " + obbPath + " could not resolve the version dir in 'assets/AssetBundles/<version>'.");
            return "";
        }

        private void ConvertObbAssets()
        {
            var dirContent = string.Join("\n", Directory.GetFiles(location));
            ValkyrieDebug.Log("Assets parts found: " + dirContent);

            var regexObj = new Regex(@"^(?<prefix>.*)(?<split>\.split)(?<num>\d+)$", RegexOptions.Multiline);
            var splitfiles = new Dictionary<string, List<KeyValuePair<int, string>>>();
            // we will get all files as results, that are split files
            Match matchResults = regexObj.Match(dirContent);
            while (matchResults.Success)
            {
                string key = matchResults.Groups["prefix"].Value;
                int num = int.Parse(matchResults.Groups["num"].Value);
                string file = matchResults.Value;
                if (!splitfiles.Keys.Contains(key))
                    splitfiles.Add(key, new List<KeyValuePair<int, string>>());
                splitfiles[key].Add(new KeyValuePair<int, string>(num, file)); // insert sorted by num to the list
                matchResults = matchResults.NextMatch();
            }
            foreach (var e in splitfiles)
            {
                e.Value.Sort((x, y) => x.Key.CompareTo(y.Key));
            }
            foreach (var e in splitfiles)
            {
                using (var fs = new FileStream(e.Key, FileMode.Create))
                {
                    string logmsg = "Creating out file '" + e.Key + "' using:";
                    e.Value.ForEach(file => logmsg += "\nSplit asset '" + file.Value + "'");
                    ValkyrieDebug.Log(logmsg);

                    // combinde split files to one file
                    foreach (var file in e.Value)
                    {
                        byte[] part = File.ReadAllBytes(file.Value);
                        fs.Write(part, 0, part.Length);
                        // delete all split files, to make space for the extracted files
                        File.SetAttributes(file.Value, FileAttributes.Normal); // remove write protection
                        File.Delete(file.Value); // delete split files to free disk space
                    }
                }
            }
        }

        private void ConvertObbStreamingAssets()
        {
            if (obbRoot == null)
                throw new InvalidOperationException("obbRoot is null");

            char dsc = Path.DirectorySeparatorChar;
            string dirAssetBundles = Path.Combine(obbRoot, "assets" + dsc + "AssetBundles");
            List<string> dirAssetBundlesDirs = Directory.GetDirectories(dirAssetBundles).ToList();
            if (dirAssetBundlesDirs.Count < 1)
            {
                ValkyrieDebug.Log("Could not find directory '" + dirAssetBundles + "/<version>' during Obb import");
                return;
            }
            string version = dirAssetBundlesDirs[0];
            string dirVersion = Path.Combine(dirAssetBundles, version);

            string dirPlatform = Path.Combine(dirVersion, "Android");
            string dirPlatformWin = Path.Combine(dirVersion, "Windows");
            if (!Directory.Exists(dirPlatform))
            {
                ValkyrieDebug.Log("Could not find platform directory '" + dirPlatform + "' during Obb import");
                return;
            }

            ValkyrieDebug.Log("Moving dir '" + dirPlatform + "' to '" + dirPlatformWin + "'");
            Directory.Move(dirPlatform, dirPlatformWin);

            string mainAsset = Path.Combine(dirPlatformWin, "Android");
            string mainAssetWin = Path.Combine(dirPlatformWin, "Windows");
            if (!File.Exists(mainAsset))
            {
                ValkyrieDebug.Log("Could not find main asset '" + mainAsset + "' during Obb import");
                return;
            }
            string mainAssetManifest = mainAsset + ".manifest";
            string mainAssetManifestWin = mainAssetWin + ".manifest";

            ValkyrieDebug.Log("Moving main asset file '" + mainAsset + "' to '" + mainAssetWin + "'");
            File.Move(mainAsset, mainAssetWin);

            ValkyrieDebug.Log("Moving main asset manifest file '" + mainAssetManifest + "' to '" + mainAssetManifestWin + "'");
            File.Move(mainAssetManifest, mainAssetManifestWin);

            string dirStreamingAssets = Path.Combine(obbRoot, "assets" + dsc + "bin" + dsc + "Data" + dsc + "StreamingAssets");
            ValkyrieDebug.Log("Creating dir '" + dirStreamingAssets + "'");
            Directory.CreateDirectory(dirStreamingAssets);

            string dirAssetBundlesWin = Path.Combine(dirStreamingAssets, "AssetBundles");
            ValkyrieDebug.Log("Moving StreamingAssets dir '" + dirAssetBundles + "' to '" + dirAssetBundlesWin + "'");
            Directory.Move(dirAssetBundles, dirAssetBundlesWin);
        }

        public string GetDataPath(string packageName)
        {
            
            return Android.GetStorage() + "/Android/data/" + packageName;
        }

        public string GetAuxDataPath(string packageName)
        {

            return Android.GetStorage() + "/Valkyrie/" + packageName;
        }

        protected string GetObbPath(string prefix, string suffix, string altprefix = null)
        {
            if (prefix == null) throw new ArgumentNullException("prefix");
            if (suffix == null) throw new ArgumentNullException("suffix");
            try
            {
                string location = Path.Combine(Android.GetStorage(), prefix);

                if (!Directory.Exists(location))
                {
                    return "";
                }
                var file = Directory.GetFiles(location).ToList().Find(x => x.EndsWith(suffix));
                return file ?? "";
            }
            catch (Exception ex)
            {
                ValkyrieDebug.Log("GetObbPath caused " + ex.GetType().Name + ": " + ex.Message + " " + ex.StackTrace);
                if(altprefix == null)
                {
                    return "";
                }
                else
                {
                    return GetObbPath(altprefix, suffix);
                }
            }

        }
    }
}