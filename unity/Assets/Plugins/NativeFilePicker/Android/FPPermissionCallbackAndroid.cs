#if UNITY_EDITOR || UNITY_ANDROID
using UnityEngine;

namespace NativeFilePickerNamespace
{
	public class FPPermissionCallbackAndroid : AndroidJavaProxy
	{
		private readonly NativeFilePicker.PermissionCallback callback;
		private readonly FPCallbackHelper callbackHelper;

		public FPPermissionCallbackAndroid( NativeFilePicker.PermissionCallback callback ) : base( "com.yasirkula.unity.NativeFilePickerPermissionReceiver" )
		{
			this.callback = callback;
			callbackHelper = FPCallbackHelper.Create( true );
		}

		[UnityEngine.Scripting.Preserve]
		public void OnPermissionResult( int result )
		{
			callbackHelper.CallOnMainThread( () => callback( (NativeFilePicker.Permission) result ) );
		}
	}
}
#endif