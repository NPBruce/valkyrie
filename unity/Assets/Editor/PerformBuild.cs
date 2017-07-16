using UnityEditor;
using System.IO;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

class PerformBuild
{
    private static string BUILD_LOCATION = "+buildlocation";

    static string GetBuildLocation(BuildTarget buildTarget)
    {
        string[] args = System.Environment.GetCommandLineArgs();
        int indexOfBuildLocation = System.Array.IndexOf(args, BUILD_LOCATION);
        if (indexOfBuildLocation >= 0)
        {
            indexOfBuildLocation++;
            Debug.Log(string.Format("Build Location for {0} set to {1}", buildTarget.ToString(), args[indexOfBuildLocation]));
            return args[indexOfBuildLocation];
        }
        else
        {
            Debug.Log(string.Format("Build Location for {0} not set. Defaulting to {1}", buildTarget.ToString(),
                                    EditorUserBuildSettings.GetBuildLocation(buildTarget)));
            return EditorUserBuildSettings.GetBuildLocation(buildTarget);
        }
    }

    static string[] GetBuildScenes()
    {
        List<string> names = new List<string>();

        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;

            if (e.enabled)
                names.Add(e.path);
        }
        return names.ToArray();
    }

    [UnityEditor.MenuItem("Perform Build/Android Command Line Build")]
    static void CommandLineBuildAndroid()
    {
        Debug.Log("Command line build android version\n------------------\n------------------");

        string[] scenes = GetBuildScenes();
        string path = GetBuildLocation(BuildTarget.Android);
        if (scenes == null || scenes.Length == 0 || path == null)
            return;

        Debug.Log(string.Format("Path: \"{0}\"", path));
        for (int i = 0; i < scenes.Length; ++i)
        {
            Debug.Log(string.Format("Scene[{0}]: \"{1}\"", i, scenes[i]));
        }

        Debug.Log(string.Format("Creating Directory \"{0}\" if it does not exist", path));
        (new FileInfo(path)).Directory.Create();

        Debug.Log(string.Format("Switching Build Target to {0}", "Android"));
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);

        Debug.Log("Starting Android Build!");
        BuildPipeline.BuildPlayer(scenes, path, BuildTarget.Android, BuildOptions.None);
    }
}