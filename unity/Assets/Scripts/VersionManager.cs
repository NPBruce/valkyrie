using System;


// This class provides functions to manage the versions of the app
class VersionManager
{
    static public string online_version = "0.0.0";
    static Action version_downloaded_action = null;

    /// <summary>
    /// Download the latest version number on Github.</summary>
    /// <param name="action">Callback when version number has been downloaded (not called in case of error).</param>
    public static void GetLatestVersionAsync(Action action)
    {
        string url = "";
#if BRYNHILDR
        // check here for BRYNHILDR
        url = "https://raw.githubusercontent.com/BenRQ/valkyrie/master/unity/Assets/Resources/version.txt";
#else
        // check here for official release
        url = "https://raw.githubusercontent.com/NPBruce/valkyrie/master/unity/Assets/Resources/version.txt";
#endif

        version_downloaded_action = action;

        HTTPManager.Get(url, GetLatestVersion_callback);
    }


    /// <summary>
    /// Provide URL to latest release.</summary>
    public static string GetlatestReleaseURL()
    {
        string url = "";
#if BRYNHILDR
            // For Brynhildr release :
        url = "https://github.com/BenRQ/valkyrie/releases/latest";
#else
        // For official release :
        url = "https://github.com/NPBruce/valkyrie/releases/latest";
#endif

        return url;
    }

    private static void GetLatestVersion_callback(string data, bool error)
    {
        // do nothing in case of error
        if (error)
        {
            return;
        }

        online_version = data;

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

        // Different number of components
        if (oldV.Length != newV.Length)
        {
            return true;
        }
        // Check each component
        for (int i = 0; i < oldV.Length; i++)
        {
            // Strip for only numbers
            string oldS = System.Text.RegularExpressions.Regex.Replace(oldV[i], "[^0-9]", "");
            string newS = System.Text.RegularExpressions.Regex.Replace(newV[i], "[^0-9]", "");
            try
            {
                if (int.Parse(oldS) < int.Parse(newS))
                {
                    return true;
                }
                if (int.Parse(oldS) > int.Parse(newS))
                {
                    return false;
                }
            }
            catch (System.Exception)
            {
                return true;
            }
        }
        return false;
    }

}
