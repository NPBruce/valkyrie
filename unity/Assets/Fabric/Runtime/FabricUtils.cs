namespace Fabric.Internal.Runtime
{
	using UnityEngine;
	using System.Collections;
	
	public static class Utils {

		public static void Log (string kit, string message)
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			var logClass = new AndroidJavaClass("android.util.Log");
			logClass.CallStatic<int> ("d", kit, message);
#else
			Debug.Log ("[" + kit + "] " + message);
#endif
		}
	}

}
