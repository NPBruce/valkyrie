using System.Collections;
using UnityEngine;
using Firebase.Crashlytics;

public class DebugManager : MonoBehaviour
{
    static public void Enable()
    {
        Firebase.FirebaseApp app = Firebase.FirebaseApp.Create();

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });

        Application.logMessageReceivedThreaded += HandleLog;
    }

    static public void Disable()
    {
        Application.logMessageReceivedThreaded -= HandleLog;
    }

    static void HandleLog(string logString, string stackTrace, LogType type)
    {

        // only capture log from main thread, otherwise crashes
        if (Application.platform == RuntimePlatform.Android && Game.Get().mainThread.Equals(System.Threading.Thread.CurrentThread))
        {
          Crashlytics.Log(logString);
        }
    }

    static public void Crash()
    {
       GameObject go_dbg = new GameObject("DebugManager");
       go_dbg.AddComponent<CrashlyticsTester>();
    }
}


public class CrashlyticsTester : MonoBehaviour
{

    int updatesBeforeException;

    // Use this for initialization
    void Start()
    {
        updatesBeforeException = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // Call the exception-throwing method here so that it's run
        // every frame update
        throwExceptionEvery60Updates();
    }

    // A method that tests your Crashlytics implementation by throwing an
    // exception every 60 frame updates. You should see non-fatal errors in the
    // Firebase console a few minutes after running your app with this method.
    void throwExceptionEvery60Updates()
    {
        if (updatesBeforeException > 0)
        {
            updatesBeforeException--;
        }
        else
        {
            // Set the counter to 60 updates
            updatesBeforeException = 60;

            // Throw an exception to test your Crashlytics implementation
            throw new System.Exception("test exception please ignore");
        }
    }
}
