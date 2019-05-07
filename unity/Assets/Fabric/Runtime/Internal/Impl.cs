namespace Fabric.Runtime.Internal
{
	using global::Fabric.Internal.Runtime;
	using System.Collections.Generic;

	internal class Impl
	{
		protected const string Name = "Fabric";

		public static Impl Make()
		{
			#if UNITY_IOS && !UNITY_EDITOR
			return new IOSImpl ();
			#elif UNITY_ANDROID && !UNITY_EDITOR
			return new AndroidImpl ();
			#else
			return new Impl ();
			#endif
		}

		public virtual string Initialize()
		{
			Utils.Log (Name, "Method Initialize () is unimplemented on this platform");
			return "";
		}
	}
}
