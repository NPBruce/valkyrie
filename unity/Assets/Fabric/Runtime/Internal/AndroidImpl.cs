namespace Fabric.Runtime.Internal
{
	using UnityEngine;
	using System.Collections.Generic;

	#if UNITY_ANDROID && !UNITY_EDITOR
	internal class AndroidImpl : Impl
	{
		private static readonly AndroidJavaClass FabricInitializer = new AndroidJavaClass (
			"io.fabric.unity.android.FabricInitializer"
		);

		public override string Initialize ()
		{
			return FabricInitializer.CallStatic<string> ("JNI_InitializeFabric");
		}
	}
	#endif
}
