using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SetVersion
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                WriteConsoleHelp();
                return;
            }

            // Read and check files
            var rootDir = args[0];
            if (!CheckDir(rootDir)) return;
            string versionFile = Path.Combine(rootDir, @"unity/Assets/Resources/version.txt");
            if (!CheckFile(versionFile)) return;
            string projectSettingsFile = Path.Combine(rootDir, @"unity/ProjectSettings/ProjectSettings.asset");
            if (!CheckFile(projectSettingsFile)) return;

            // Check version values
            string version = File.ReadAllText(versionFile).Trim();
            if (string.IsNullOrEmpty(version))
            {
                Console.WriteLine("version is invalid!");
                return;
            }

            // Change the settings to contain the new versions
            string location = "ReadAllText";
            try
            {
                string text = File.ReadAllText(projectSettingsFile, new UTF8Encoding(false));
                location = "bundleVersion";
                text = Regex.Replace(text, @"(bundleVersion:).*[^\n]$", "$1 " + version, RegexOptions.Multiline);
                location = "AndroidBundleVersionCode";
                text = Regex.Replace(text, @"(AndroidBundleVersionCode:).*[^\n]$", "$1 " + VersionCodeGenerate(version), RegexOptions.Multiline);
                location = "WriteAllText";
                File.WriteAllText(projectSettingsFile, text, new UTF8Encoding(false));
            }
            catch (Exception ex)
            {
                string msg = string.Format(
                    "Could not replace version strings at {0}. {1}: {2} {3}",
                    location,
                    ex.GetType().Name,
                    ex.Message,
                    ex.StackTrace);
                Console.WriteLine(msg);
            }
        }

        private static void WriteConsoleHelp()
        {
            Console.WriteLine("SetVersion <rootpath>\n\nrootpath: The path to the repository root, for example C:\\projects\\valkyrie");
        }

        private static bool CheckFile(string file)
        {
            if (!File.Exists(file))
            {
                Console.WriteLine(string.Format("{0} file could not be found!", file));
                return false;
            }
            return true;
        }

        private static bool CheckDir(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Console.WriteLine(string.Format("{0} directory could not be found!", directory));
                return false;
            }
            return true;
        }

        private static string VersionCodeGenerate(string version)
        {
            if (version.Length == 0)
            {
                Console.WriteLine("No version found to convert.");
                return "0";
            }
            if (version.Length == 1)
            {
                if (Char.IsDigit(version[0]))
                {
                    return version;
                }
                Console.WriteLine("Version does not include a number.");
                return "0";
            }

            // We don't handle more than 1 trailing alpha
            for (int i = 0; i < (version.Length - 1); i++)
            {
                if (!Char.IsDigit(version[i]) && version[i] != '.')
                {
                    Console.WriteLine("Version has letters (other than a single final little).");
                    return "0";
                }
            }

            int majorDot = version.IndexOf('.');
            string majorString = version;
            string minorString = "0";
            string patchString = "0";
            if (majorDot != -1)
            {
                majorString = version.Substring(0, majorDot);

                int minorDot = version.IndexOf('.', majorDot + 1);
                minorString = version.Substring(majorDot + 1);
                {
                    if (minorDot != -1)
                    {
                        minorString = version.Substring(majorDot + 1, minorDot - (majorDot + 1));
                        patchString = version.Substring(minorDot + 1);
                        {
                            if (!Char.IsDigit(version[version.Length - 1]))
                            {
                                patchString = patchString.Substring(0, patchString.Length - 1);
                            }
                        }
                    }
                }
            }

            int majorNumber = 0;
            int minorNumber = 0;
            int patchNumber = 0;
            int VersionComponentChar = 0;

            if (!int.TryParse(majorString, out majorNumber))
            {
                Console.WriteLine("Error reading major version: " + majorString + ".");
                return "0";
            }
            if (!int.TryParse(minorString, out minorNumber))
            {
                Console.WriteLine("Error reading minor version: " + minorString + ".");
                return "0";
            }
            if (!int.TryParse(patchString, out patchNumber))
            {
                Console.WriteLine("Error reading patch version: " + patchString + ".");
                return "0";
            }

            if (!Char.IsDigit(version[version.Length - 1]))
            {
                VersionComponentChar = version[version.Length - 1] + 1 - 'a';
                if (VersionComponentChar < 1)
                {
                    Console.WriteLine("Error reading training letter.");
                    return "0";
                }
                if (VersionComponentChar > 9)
                {
                    Console.WriteLine("Trailing letter to high.");
                    return "0";
                }
            }

            int versionCode = VersionComponentChar;
            versionCode += patchNumber * 10;
            versionCode += minorNumber * 10000;
            versionCode += majorNumber * 10000000;

            if (versionCode > 2100000000)
            {
                Console.WriteLine("Version exceeds android limit.");
                return "0";
            }
            return versionCode.ToString();
        }
    }
}
