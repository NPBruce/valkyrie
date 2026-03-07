using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Cross-platform native file dialog for standalone builds.
    /// Uses P/Invoke on Windows, osascript on macOS, and zenity/kdialog on Linux.
    /// Android uses NativeFilePicker separately (see FileSelector.cs).
    /// </summary>
    public static class NativeFileDialog
    {
        /// <summary>
        /// Opens a native file picker dialog and returns the selected file path.
        /// Returns null if the user cancels.
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Initial directory (can be empty)</param>
        /// <param name="filter">File extension filter (e.g., "zip", can be empty for all files)</param>
        /// <returns>Selected file path, or null if cancelled</returns>
        public static string OpenFilePanel(string title, string directory, string filter)
        {
#if UNITY_STANDALONE_WIN
            return OpenFilePanelWindows(title, directory, filter);
#elif UNITY_STANDALONE_OSX
            return OpenFilePanelMac(title, directory, filter);
#elif UNITY_STANDALONE_LINUX
            return OpenFilePanelLinux(title, directory, filter);
#else
            UnityEngine.Debug.LogWarning("NativeFileDialog: Unsupported platform");
            return null;
#endif
        }

#if UNITY_STANDALONE_WIN
        // Windows: P/Invoke into comdlg32.dll (GetOpenFileName)
        // No System.Windows.Forms dependency needed

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct OpenFileName
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public string lpstrFilter;
            public string lpstrCustomFilter;
            public int nMaxCustFilter;
            public int nFilterIndex;
            public string lpstrFile;
            public int nMaxFile;
            public string lpstrFileTitle;
            public int nMaxFileTitle;
            public string lpstrInitialDir;
            public string lpstrTitle;
            public int Flags;
            public short nFileOffset;
            public short nFileExtension;
            public string lpstrDefExt;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            public string lpTemplateName;
            public IntPtr pvReserved;
            public int dwReserved;
            public int flagsEx;
        }

        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetOpenFileName(ref OpenFileName ofn);

        private const int OFN_FILEMUSTEXIST = 0x00001000;
        private const int OFN_PATHMUSTEXIST = 0x00000800;
        private const int OFN_NOCHANGEDIR = 0x00000008;

        private static string OpenFilePanelWindows(string title, string directory, string filter)
        {
            var ofn = new OpenFileName();
            ofn.lStructSize = Marshal.SizeOf(ofn);
            ofn.hwndOwner = IntPtr.Zero;

            // Build filter string: "Description\0*.ext\0All Files\0*.*\0\0"
            if (!string.IsNullOrEmpty(filter))
            {
                ofn.lpstrFilter = filter.ToUpper() + " Files\0*." + filter + "\0All Files\0*.*\0\0";
            }
            else
            {
                ofn.lpstrFilter = "All Files\0*.*\0\0";
            }

            ofn.lpstrFile = new string(new char[256]);
            ofn.nMaxFile = ofn.lpstrFile.Length;
            ofn.lpstrFileTitle = new string(new char[64]);
            ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
            ofn.lpstrInitialDir = string.IsNullOrEmpty(directory) ? null : directory;
            ofn.lpstrTitle = title;
            ofn.Flags = OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR;

            if (GetOpenFileName(ref ofn))
            {
                return ofn.lpstrFile;
            }

            return null;
        }
#endif

#if UNITY_STANDALONE_OSX
        // macOS: Use osascript (AppleScript) to show native Finder dialog
        private static string OpenFilePanelMac(string title, string directory, string filter)
        {
            try
            {
                // Build AppleScript command for file dialog
                string script;
                if (!string.IsNullOrEmpty(filter))
                {
                    script = string.Format(
                        "set theFile to choose file with prompt \"{0}\" of type {{\"{1}\"}} ",
                        EscapeAppleScript(title),
                        filter);
                }
                else
                {
                    script = string.Format(
                        "set theFile to choose file with prompt \"{0}\" ",
                        EscapeAppleScript(title));
                }

                if (!string.IsNullOrEmpty(directory))
                {
                    script += string.Format(
                        "default location POSIX file \"{0}\" ",
                        EscapeAppleScript(directory));
                }

                script += "\nreturn POSIX path of theFile";

                return RunProcess("/usr/bin/osascript", "-e", script);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning("NativeFileDialog macOS error: " + ex.Message);
                return null;
            }
        }

        private static string EscapeAppleScript(string input)
        {
            return input.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
#endif

#if UNITY_STANDALONE_LINUX
        // Linux: Use zenity (GTK) or kdialog (KDE) for native file dialog
        private static string OpenFilePanelLinux(string title, string directory, string filter)
        {
            try
            {
                // Try zenity first (most common on Linux desktops)
                string args = string.Format("--file-selection --title=\"{0}\"", title);

                if (!string.IsNullOrEmpty(filter))
                {
                    args += string.Format(" --file-filter=\"{0} files | *.{0}\" --file-filter=\"All files | *\"",
                        filter);
                }

                string result = RunProcess("zenity", args);
                if (result != null) return result;

                // Fallback to kdialog
                args = string.Format("--getopenfilename \"{0}\"",
                    string.IsNullOrEmpty(directory) ? "~" : directory);

                if (!string.IsNullOrEmpty(filter))
                {
                    args += string.Format(" \"*.{0} | {0} files\"", filter);
                }

                return RunProcess("kdialog", args);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning("NativeFileDialog Linux error: " + ex.Message);
                return null;
            }
        }
#endif

#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
        private static string RunProcess(string fileName, string arguments)
        {
            return RunProcess(fileName, arguments, null);
        }

        private static string RunProcess(string fileName, string arg1, string arg2)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                if (arg2 != null)
                {
                    // Two separate arguments (e.g. osascript -e "script")
                    psi.Arguments = arg1 + " \"" + arg2.Replace("\"", "\\\"") + "\"";
                }
                else
                {
                    psi.Arguments = arg1;
                }

                using (var process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();

                    if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                    {
                        return output;
                    }
                }
            }
            catch (Exception)
            {
                // Process not found or failed to start
            }

            return null;
        }
#endif
    }
}
