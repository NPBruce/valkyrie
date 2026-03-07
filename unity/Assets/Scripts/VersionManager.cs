using System;


// This class provides functions to manage the versions of the app
public class VersionManager
{
    static public string online_version = "0.0.0";
    static Action version_downloaded_action = null;

    /// <summary>
    /// Download the latest version number on Github.</summary>
    /// <param name="action">Callback when version number has been downloaded (not called in case of error).</param>
    public static void GetLatestVersionAsync(Action action)
    {
        // check here for official release
        string url = "https://raw.githubusercontent.com/NPBruce/valkyrie/master/unity/Assets/Resources/prod_version.txt";

        version_downloaded_action = action;

        HTTPManager.Get(url, GetLatestVersion_callback);
    }


    /// <summary>
    /// Provide URL to latest release.</summary>
    public static string GetlatestReleaseURL()
    {
        string url = "https://github.com/NPBruce/valkyrie/releases/latest";
        return url;
    }

    private static void GetLatestVersion_callback(string data, bool error, System.Uri uri)
    {
        // do nothing in case of error
        if (error)
        {
            return;
        }

        online_version = data.Trim().Replace("\r\n", " ").Replace("\n", " ");

        if(version_downloaded_action != null)
            version_downloaded_action();
    }

    // Test version of the form a.b.c is newer or equal
    public static bool VersionNewerOrEqual(string oldVersion, string newVersion)
    {
        string oldS = System.Text.RegularExpressions.Regex.Replace(oldVersion, "[^0-9]", "");
        string newS = System.Text.RegularExpressions.Regex.Replace(newVersion, "[^0-9]", "");
        // If numbers are the same they are equal
        if (oldS.Equals(newS)) return true;
        return VersionNewer(oldVersion, newVersion);
    }

    // Test version of the form a.b.c is newer
    public static bool VersionNewer(string oldVersion, string newVersion)
    {
        // Split into components
        string[] oldV = oldVersion.Split('.');
        string[] newV = newVersion.Split('.');

        if (newVersion.Equals("")) return false;
        if (oldVersion.Equals("")) return true;

        int commonLen = Math.Min(oldV.Length, newV.Length);

        // Check each common component
        for (int i = 0; i < commonLen; i++)
        {
            int oldInt = 0;
            int newInt = 0;

            string oldS = System.Text.RegularExpressions.Regex.Replace(oldV[i], "[^0-9]", "");
            int.TryParse(oldS, out oldInt);
            string newS = System.Text.RegularExpressions.Regex.Replace(newV[i], "[^0-9]", "");
            int.TryParse(newS, out newInt);

            if (oldInt < newInt) return true;
            if (oldInt > newInt) return false;
        }

        // Common components are equal. 
        // If one version has more components (e.g. 3.20.1 vs 3.20)
        if (newV.Length > oldV.Length)
        {
            // e.g. 3.20 -> 3.20.1. New is newer numerically.
            return true;
        }
        if (oldV.Length > newV.Length)
        {
            // e.g. 3.20.1 -> 3.20. Old has more.
            // In our logic, 3.20.1 is Beta, 3.20 is Stable.
            // Moving from Beta to Stable is an upgrade if numeric parts match.
            return GetVersionPriority(newVersion) > GetVersionPriority(oldVersion);
        }

        // Same components, just check release type suffixes (Beta < Stable < Major)
        return GetVersionPriority(newVersion) > GetVersionPriority(oldVersion);
    }

    /// <summary>
    /// Gets a priority score for the version's release type.
    /// Beta = 1, Stable/Normal = 2, Major = 3
    /// </summary>
    private static int GetVersionPriority(string version)
    {
        string vLower = version.ToLower();
        if (vLower.Contains("major")) return 3;
        if (IsBeta(version)) return 1;
        return 2; // Default for normal releases
    }

    /// <summary>
    /// Checks if the provided version string indicates a beta version.
    /// A beta version is defined as having more than 2 components (e.g., 3.12.1)
    /// or containing the string "beta".
    /// </summary>
    /// <param name="version">The version string to check.</param>
    /// <returns>True if the version is beta, otherwise false.</returns>
    public static bool IsBeta(string version)
    {
        return version.Split('.').Length > 2 || version.ToLower().Contains("beta");
    }

}
