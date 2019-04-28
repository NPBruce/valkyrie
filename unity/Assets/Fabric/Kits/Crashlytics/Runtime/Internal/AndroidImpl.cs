namespace Fabric.Crashlytics.Internal
{
	using UnityEngine;
	using System;
	using System.Diagnostics;
	using System.Runtime.InteropServices;
	using System.Collections;
	using System.Collections.Generic;
	using Fabric.Internal.Runtime;

	#if UNITY_ANDROID && !UNITY_EDITOR
	internal class AndroidImpl : Impl
	{
		private readonly List<IntPtr> references = new List<IntPtr> ();

		private AndroidJavaObject native = null;
		private AndroidJavaObject Native
		{
			get {
				if (native == null)
					native = new AndroidJavaObject ("com.crashlytics.android.Crashlytics");
				return native;
			}
		}
		private AndroidJavaClass crashWrapper = null;
		private AndroidJavaClass CrashWrapper
		{
			get {
				if (crashWrapper == null)
					crashWrapper = new AndroidJavaClass ("io.fabric.unity.crashlytics.android.CrashlyticsAndroidWrapper");
				return crashWrapper;
			}
		}

		private AndroidJavaObject instance = null;
		private AndroidJavaObject Instance
		{
			get {
				if (instance == null)
					instance = Native.CallStatic<AndroidJavaObject> ("getInstance");
				if (instance == null)
					throw new JavaInteropException ("Couldn't get an instance of the Crashlytics class!");
				return instance;
			}
		}

		public class JavaInteropException : System.Exception
		{
			public JavaInteropException(string message) : base (message)
			{
			}
		}

		public AndroidImpl()
		{
		}
		
		public override void Crash ()
		{
			// Unity's C#-Java bridge intercepts Java exceptions, so here we provide a convenience
			// crasher that throws an exception on another thread (and hence bypasses Unity's exception handlers)
			CrashWrapper.CallStatic ("crash");
		}
		
		public override void Log(string message)
		{
			Instance.CallStatic ("log", message);
		}
		
		public override void SetKeyValue(string key, string value)
		{
			Instance.CallStatic ("setString", new object[] { key, value });
		}
		
		public override void SetUserIdentifier(string identifier)
		{
			Instance.CallStatic ("setUserIdentifier", identifier);
		}
		
		public override void SetUserEmail(string email)
		{
			Instance.CallStatic ("setUserEmail", email);
		}
		
		public override void SetUserName(string name)
		{
			Instance.CallStatic ("setUserName", name);
		}

		public override void RecordCustomException(string name, string reason, StackTrace stackTrace)
		{
			RecordCustomException (name, reason, stackTrace.ToString ());
		}

		public override void RecordCustomException(string name, string reason, string stackTraceString)
		{
			references.Clear ();

			// new Exception(message)
			var exceptionClass = AndroidJNI.FindClass("java/lang/Exception");
			var exceptionConstructor = AndroidJNI.GetMethodID (exceptionClass, "<init>", "(Ljava/lang/String;)V");
			var exceptionArgs = new jvalue[1];

			exceptionArgs[0].l = AndroidJNI.NewStringUTF(name + " : " + reason);

			var exceptionObj = AndroidJNI.NewObject (exceptionClass, exceptionConstructor, exceptionArgs);

			references.Add (exceptionArgs [0].l);
			references.Add (exceptionObj);

			// stackTrace = [StackTraceElement, ...]
			var stackTraceElClass = AndroidJNI.FindClass ("java/lang/StackTraceElement");
			var stackTraceElConstructor = AndroidJNI.GetMethodID (stackTraceElClass, "<init>", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;I)V");
			var parsedStackTrace = ParseStackTraceString (stackTraceString);
			var stackTraceArray = AndroidJNI.NewObjectArray(parsedStackTrace.Length, stackTraceElClass, IntPtr.Zero);

			references.Add (stackTraceArray);

			for (var i = 0; i < parsedStackTrace.Length; i++) {
				var frame = parsedStackTrace[i];
				// new StackTraceElement()
				var stackTraceArgs = new jvalue[4];

				stackTraceArgs[0].l = AndroidJNI.NewStringUTF(frame["class"]);
				stackTraceArgs[1].l = AndroidJNI.NewStringUTF(frame["method"]);
				stackTraceArgs[2].l = AndroidJNI.NewStringUTF(frame["file"]);

				references.Add (stackTraceArgs [0].l);
				references.Add (stackTraceArgs [1].l);
				references.Add (stackTraceArgs [2].l);

				stackTraceArgs[3].i = Int32.Parse(frame["line"]);

				var stackTraceEl = AndroidJNI.NewObject (stackTraceElClass, stackTraceElConstructor, stackTraceArgs);

				references.Add (stackTraceEl);

				AndroidJNI.SetObjectArrayElement(stackTraceArray, i, stackTraceEl);
			}

			// exception.setStackTrace(stackTraceArray)
			var setStackTraceMethod = AndroidJNI.GetMethodID (exceptionClass, "setStackTrace", "([Ljava/lang/StackTraceElement;)V");
			var setStackTraceArgs = new jvalue[1];
			setStackTraceArgs[0].l = stackTraceArray;
			AndroidJNI.CallVoidMethod (exceptionObj, setStackTraceMethod, setStackTraceArgs);

			// Crashlytics.logException(exception)
			var crashClass = AndroidJNI.FindClass ("com/crashlytics/android/Crashlytics");
			var logExceptionMethod = AndroidJNI.GetStaticMethodID (crashClass, "logException", "(Ljava/lang/Throwable;)V");
			var logExceptionArgs = new jvalue[1];
			logExceptionArgs[0].l = exceptionObj;
			AndroidJNI.CallStaticVoidMethod (crashClass, logExceptionMethod, logExceptionArgs);

			foreach (IntPtr reference in references) {
				AndroidJNI.DeleteLocalRef (reference);
			}
		}

	}
	#endif
}
