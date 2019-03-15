namespace Fabric.Runtime.Internal
{
	using System.Runtime.InteropServices;
	using System.Collections.Generic;

	#if UNITY_IOS && !UNITY_EDITOR
	internal class IOSImpl : Impl
	{
		#region DLL Imports
		[DllImport("__Internal")]
		private static extern string fabric_symbol_for_linker();
		#endregion

		public override string Initialize()
		{
			// This should really be "fabric_initialize" but for backwards compatibility reasons we're
			// reusing "fabric_symbol_for_linker".
			return fabric_symbol_for_linker ();
		}
	}
	#endif
}
