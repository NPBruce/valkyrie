using UnityEngine;

/// <summary>
/// Debugger for Valkyrie customizable and disableable
/// in order to create unit tests
/// </summary>
public class ValkyrieDebug
{
    /// <summary>
    /// Property to disable logger
    /// </summary>
    public static bool enabled { get; set; }

    /// <summary>
    /// Default initial value
    /// </summary>
    static ValkyrieDebug()
    {
        enabled = true;
    }

    /// <summary>
    /// logs message to debug logger
    /// </summary>
    /// <param name="message">message to log</param>
    public static void Log(object message)
    {
        if (enabled)
        {
            Debug.Log(message);
        }
    }

    /// <summary>
    /// logs message to debug logger in selected context
    /// </summary>
    /// <param name="message">message to log</param>
    /// <param name="context">context of message</param>
    public static void Log(object message, Object context)
    {
        if (enabled)
        {
            Debug.Log(message, context);
        }
    }
}

