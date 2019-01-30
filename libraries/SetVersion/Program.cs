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
            string versionCodeFile = Path.Combine(rootDir, @"unity/Assets/Resources/versioncode.txt");
            if (!CheckFile(versionCodeFile)) return;
            string projectSettingsFile = Path.Combine(rootDir, @"unity/ProjectSettings/ProjectSettings.asset");
            if (!CheckFile(projectSettingsFile)) return;

            // Check version values
            string version = File.ReadAllText(versionFile).Trim();
            if (string.IsNullOrEmpty(version))
            {
                Console.WriteLine("version is invalid!");
                return;
            }
            string versionCode = File.ReadAllText(versionCodeFile).Trim();
            if (string.IsNullOrEmpty(versionCode))
            {
                Console.WriteLine("versioncode is invalid!");
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
                text = Regex.Replace(text, @"(AndroidBundleVersionCode:).*[^\n]$", "$1 " + versionCode, RegexOptions.Multiline);
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
    }
}
