namespace Fabric.Crashlytics
{
	using UnityEngine;
	using Fabric.Internal.Runtime;
	using System.Diagnostics;

	public class Crashlytics
	{
		private static readonly Internal.Impl impl;

		static Crashlytics()
		{
			impl = Internal.Impl.Make ();
		}

		public static void SetDebugMode(bool debugMode)
		{
			impl.SetDebugMode (debugMode);
		}

		/// <summary>
		/// Force a crash.
		/// </summary>

		public static void Crash()
		{
			impl.Crash ();
		}

		/// <summary>
		/// Throws an exception.
		/// </summary>
		public static void ThrowNonFatal()
		{
			impl.ThrowNonFatal ();
		}

		public static void Log(string message)
		{
			impl.Log (message);
		}

		public static void SetKeyValue(string key, string value)
		{
			impl.SetKeyValue (key, value);
		}

		public static void SetUserIdentifier(string identifier)
		{
			impl.SetUserIdentifier (identifier);
		}

		public static void SetUserEmail(string email)
		{
			impl.SetUserEmail (email);
		}

		public static void SetUserName(string name)
		{
			impl.SetUserName (name);
		}

		public static void RecordCustomException(string name, string reason, StackTrace stackTrace)
		{
			impl.RecordCustomException (name, reason, stackTrace);
		}

		public static void RecordCustomException(string name, string reason, string stackTraceString)
		{
			impl.RecordCustomException (name, reason, stackTraceString);
		}
	}
}
