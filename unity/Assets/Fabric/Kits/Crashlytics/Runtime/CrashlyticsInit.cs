namespace Fabric.Internal.Crashlytics
{
	using UnityEngine;
	using System;
	using System.Runtime.InteropServices;
	using System.IO;
	using Fabric.Internal.Runtime;
	using Fabric.Crashlytics;

	public class CrashlyticsInit : MonoBehaviour
	{
		// Since we do not support platforms other than Android and iOS,
		// we'll do nothing in Play/Editor mode.
#if !UNITY_EDITOR
		private static readonly string kitName = "Crashlytics";

#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern bool CLUIsInitialized();
#endif

		private static CrashlyticsInit instance;

		void Awake ()
		{
			// This singleton pattern ensures AwakeOnce() is only called once even when the scene
			// is reloaded (loading scenes destroy previous objects and wake up new ones)
			if (instance == null) {
				AwakeOnce ();

				instance = this;
				DontDestroyOnLoad(this);
			} else if (instance != this) {
				Destroy(this.gameObject);
			}
		}

		private void AwakeOnce ()
		{
			RegisterExceptionHandlers();
		}

		private static void RegisterExceptionHandlers ()
		{
			// We can only send logged exceptions if the SDK has been initialized
			if (IsSDKInitialized ()) {
				Utils.Log (kitName, "Registering exception handlers");
				
				AppDomain.CurrentDomain.UnhandledException += HandleException;	
				
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
				Application.RegisterLogCallback(HandleLog);
#else
				Application.logMessageReceived += HandleLog;
#endif
			} else {
				Utils.Log (kitName, "Did not register exception handlers: Crashlytics SDK was not initialized");
			}
		}
	
		private static bool IsSDKInitialized ()
		{
#if UNITY_IOS
			return CLUIsInitialized ();
#elif UNITY_ANDROID
			var crashlyticsClass = new AndroidJavaClass("com.crashlytics.android.Crashlytics");
			AndroidJavaObject crashlyticsInstance = null;
			try {
				crashlyticsInstance = crashlyticsClass.CallStatic<AndroidJavaObject>("getInstance");
			}
			catch {
				crashlyticsInstance = null;
			}
			return crashlyticsInstance != null;
#else
			return false;
#endif
		}

		private static void HandleException(object sender, UnhandledExceptionEventArgs eArgs)
		{
			Exception e = (Exception)eArgs.ExceptionObject;
			HandleLog (e.Message.ToString (), e.StackTrace.ToString (), LogType.Exception);
		}
		
		private static void HandleLog(string message, string stackTraceString, LogType type)
		{
			if (type == LogType.Exception) {
				Utils.Log (kitName, "Recording exception: " + message);
				Utils.Log (kitName, "Exception stack trace: " + stackTraceString);

				string[] messageParts = getMessageParts(message);
				Crashlytics.RecordCustomException (messageParts[0], messageParts[1], stackTraceString);
			}
		}
		
		private static string[] getMessageParts (string message)
		{
			// Split into two parts so we only split on the first delimiter
			char[] delim = { ':' };
			string[] messageParts = message.Split(delim, 2, StringSplitOptions.None);
			
			foreach (string part in messageParts) {
				part.Trim ();
			}
			
			if (messageParts.Length == 2) {
				return messageParts;
			} else {
				return new string[] {"Exception", message};
			}
		}
#endif
	}
}
