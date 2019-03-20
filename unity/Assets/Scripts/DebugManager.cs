using System.Collections;
using UnityEngine;
using Fabric.Crashlytics;

public class DebugManager : MonoBehaviour
{
    static public void Enable()
    {
        Application.logMessageReceivedThreaded += HandleLog;
    }

    static public void Disable()
    {
        Application.logMessageReceivedThreaded -= HandleLog;
    }

    static void HandleLog(string logString, string stackTrace, LogType type)
    {
        Crashlytics.Log(logString);
    }

    static public void Crash()
    {
        Crashlytics.Crash();
    }
}
