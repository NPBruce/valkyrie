using UnityEngine;
using System.IO;
using System.Text;

public class FileLogger : MonoBehaviour
{
    private static string logPath;
    private static FileLogger instance;
    private const int MAX_LOG_SIZE_BYTES = 5 * 1024 * 1024; // 5MB Limit
    private bool stopWriting = false;

    public static string GetLogPath()
    {
        if (string.IsNullOrEmpty(logPath))
        {
            logPath = Path.Combine(Application.persistentDataPath, "session.log");
        }
        return logPath;
    }

    void Awake()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            Destroy(this);
            return;
        }

        if (instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        // Initialize log file
        GetLogPath();
        try
        {
            // Overwrite existing file to start fresh for this session
            File.WriteAllText(logPath, "--- Log Start ---\n");
        }
        catch (System.Exception e)
        {
            Debug.LogError("FileLogger failed to initialize: " + e.Message);
            stopWriting = true;
        }

        Application.logMessageReceivedThreaded += HandleLog;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            Application.logMessageReceivedThreaded -= HandleLog;
        }
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (stopWriting) return;

        try
        {
            // Simple size check before writing (optimization: check periodically or catch IOException)
            // For robustness, we check length of file info occasionally or just let it grow until next restart if performance is key.
            // But requirement was explicit size safety.
            
            // To avoid excessive FileInfo creation, maybe check every N logs or just catch exception if disk full.
            // Requirement was: "Checks file size before writing". 
            // We'll do a check. Optimally this should be buffered, but for stability/simplicity in threaded context:
            
            var fileInfo = new FileInfo(logPath);
            if (fileInfo.Exists && fileInfo.Length > MAX_LOG_SIZE_BYTES)
            {
                stopWriting = true;
                string overflowMsg = "\n[FileLogger] Log size limit reached. Stopping log capture.";
                File.AppendAllText(logPath, overflowMsg);
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.Append(System.DateTime.Now.ToString("HH:mm:ss.fff"));
            sb.Append("] ");
            sb.Append(type.ToString().ToUpper());
            sb.Append(": ");
            sb.Append(logString);
            sb.Append("\n");
            
            if (type == LogType.Exception || type == LogType.Error)
            {
                 sb.Append(stackTrace);
                 sb.Append("\n");
            }

            // AppendAllText is thread-safe effectively as it opens/closes file, 
            // but heavy for high frequency logs. 
            // Given "logMessageReceivedThreaded", we should be careful. 
            // lock to ensure sequential writes if multiple threads log simultaneously.
            lock (this)
            {
                File.AppendAllText(logPath, sb.ToString());
            }
        }
        catch (System.Exception)
        {
            // If writing fails, stop trying to avoid spamming errors
            stopWriting = true;
        }
    }
}
