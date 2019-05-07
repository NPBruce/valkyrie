namespace Fabric.Crashlytics.Internal
{
	using System.Runtime.InteropServices;
	using System.Diagnostics;
	using System.Collections.Generic;
	using Fabric.Internal.ThirdParty.MiniJSON;

	#if UNITY_IOS && !UNITY_EDITOR
	internal class IOSImpl : Impl
	{
		#region DLL Imports
		[DllImport("__Internal")]
		private static extern void CLUCrash();
		
		[DllImport("__Internal")]
		private static extern void CLUSetDebugMode(int debugMode);		
		
		[DllImport("__Internal")]
		private static extern void CLULog(string msg);
		
		[DllImport("__Internal")]
		private static extern void CLUSetKeyValue(string key, string value);
		
		[DllImport("__Internal")]
		private static extern void CLUSetUserIdentifier(string identifier);
		
		[DllImport("__Internal")]
		private static extern void CLUSetUserEmail(string email);
		
		[DllImport("__Internal")]
		private static extern void CLUSetUserName(string name);

		[DllImport("__Internal")]
		private static extern void CLURecordCustomException(string name, string reason, string stackTrace);
		#endregion
		
		public override void SetDebugMode(bool mode)
		{
			CLUSetDebugMode (mode ? 1 : 0);
		}
		
		public override void Crash()
		{
			CLUCrash ();
		}
		
		public override void Log(string message)
		{
			CLULog (message);
		}
		
		public override void SetKeyValue(string key, string value)
		{
			CLUSetKeyValue (key, value);
		}
		
		public override void SetUserIdentifier(string identifier)
		{
			CLUSetUserIdentifier (identifier);
		}
		
		public override void SetUserEmail(string email)
		{
			CLUSetUserEmail (email);
		}
		
		public override void SetUserName(string name)
		{
			CLUSetUserName (name);
		}

		public override void RecordCustomException(string name, string reason, StackTrace stackTrace)
		{
			string stackTraceString = stackTrace != null ? stackTrace.ToString () : "";
			RecordCustomException (name, reason, stackTraceString);
		}

		public override void RecordCustomException(string name, string reason, string stackTraceString)
		{
			Dictionary<string, string>[] parsedStackTrace = ParseStackTraceString (stackTraceString);
			CLURecordCustomException (name, reason, Json.Serialize (parsedStackTrace));
		}
	}
	#endif
}
