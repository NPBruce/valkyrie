using System;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;
#if UNITY_ANDROID || UNITY_IOS
using NativeFilePickerNamespace;
#endif

public static class NativeFilePicker
{
	public delegate void PermissionCallback( Permission permission );
	public delegate void FilePickedCallback( string path );
	public delegate void MultipleFilesPickedCallback( string[] paths );
	public delegate void FilesExportedCallback( bool success );

	public enum Permission { Denied = 0, Granted = 1, ShouldAsk = 2 };

	#region Platform Specific Elements
#if !UNITY_EDITOR && UNITY_ANDROID
	private static AndroidJavaClass m_ajc = null;
	private static AndroidJavaClass AJC
	{
		get
		{
			if( m_ajc == null )
				m_ajc = new AndroidJavaClass( "com.yasirkula.unity.NativeFilePicker" );

			return m_ajc;
		}
	}

	private static AndroidJavaObject m_context = null;
	private static AndroidJavaObject Context
	{
		get
		{
			if( m_context == null )
			{
				using( AndroidJavaObject unityClass = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" ) )
				{
					m_context = unityClass.GetStatic<AndroidJavaObject>( "currentActivity" );
				}
			}

			return m_context;
		}
	}
#elif !UNITY_EDITOR && UNITY_IOS
	[System.Runtime.InteropServices.DllImport( "__Internal" )]
	private static extern int _NativeFilePicker_CanPickMultipleFiles();

	[System.Runtime.InteropServices.DllImport( "__Internal" )]
	private static extern string _NativeFilePicker_ConvertExtensionToUTI( string extension );

	[System.Runtime.InteropServices.DllImport( "__Internal" )]
	private static extern void _NativeFilePicker_PickFile( string[] UTIs, int UTIsCount );

	[System.Runtime.InteropServices.DllImport( "__Internal" )]
	private static extern void _NativeFilePicker_PickMultipleFiles( string[] UTIs, int UTIsCount );

	[System.Runtime.InteropServices.DllImport( "__Internal" )]
	private static extern void _NativeFilePicker_ExportFiles( string[] files, int filesCount );
#endif
	#endregion

#if !UNITY_EDITOR && UNITY_ANDROID
	private static string m_selectedFilePath = null;
	private static string SelectedFilePath
	{
		get
		{
			if( m_selectedFilePath == null )
			{
				m_selectedFilePath = Path.Combine( Application.temporaryCachePath, "pickedFile" );
				Directory.CreateDirectory( Application.temporaryCachePath );
			}

			return m_selectedFilePath;
		}
	}
#endif

	#region Runtime Permissions
	public static bool CheckPermission( bool readPermissionOnly = false )
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		return AJC.CallStatic<int>( "CheckPermission", Context, readPermissionOnly ) == 1;
#else
		return true;
#endif
	}

	public static void RequestPermissionAsync( PermissionCallback callback, bool readPermissionOnly = false )
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		FPPermissionCallbackAndroid nativeCallback = new( callback );
		AJC.CallStatic( "RequestPermission", Context, nativeCallback, readPermissionOnly );
#else
		callback( Permission.Granted );
#endif
	}

	public static Task<Permission> RequestPermissionAsync( bool readPermissionOnly = false )
	{
		TaskCompletionSource<Permission> tcs = new TaskCompletionSource<Permission>();
		RequestPermissionAsync( ( permission ) => tcs.SetResult( permission ), readPermissionOnly );
		return tcs.Task;
	}

	public static void OpenSettings()
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		AJC.CallStatic( "OpenSettings", Context );
#endif
	}
	#endregion

	#region Helper Functions
	public static bool CanPickMultipleFiles()
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		return AJC.CallStatic<bool>( "CanPickMultipleFiles" );
#elif !UNITY_EDITOR && UNITY_IOS
		return _NativeFilePicker_CanPickMultipleFiles() == 1;
#else
		return false;
#endif
	}

	public static bool CanExportFiles()
	{
#if UNITY_EDITOR
		return true;
#elif UNITY_ANDROID
		return AJC.CallStatic<bool>( "CanExportFiles" );
#elif UNITY_IOS
		return true;
#else
		return false;
#endif
	}

	public static bool CanExportMultipleFiles()
	{
#if UNITY_EDITOR
		return true;
#elif UNITY_ANDROID
		return AJC.CallStatic<bool>( "CanExportMultipleFiles" );
#elif UNITY_IOS
		return _NativeFilePicker_CanPickMultipleFiles() == 1;
#else
		return false;
#endif
	}

	public static bool IsFilePickerBusy()
	{
#if !UNITY_EDITOR && UNITY_IOS
		return FPResultCallbackiOS.IsBusy;
#else
		return false;
#endif
	}

	public static string ConvertExtensionToFileType( string extension )
	{
		if( string.IsNullOrEmpty( extension ) )
			return null;

		if( extension.IndexOf( '*' ) >= 0 )
		{
			// So many users try to do this that it's now necessary to throw an exception for this particular scenario
			throw new ArgumentException( "See: https://github.com/yasirkula/UnityNativeFilePicker#faq" );
		}

#if !UNITY_EDITOR && UNITY_ANDROID
		return AJC.CallStatic<string>( "GetMimeTypeFromExtension", extension.ToLowerInvariant() );
#elif !UNITY_EDITOR && UNITY_IOS
		return _NativeFilePicker_ConvertExtensionToUTI( extension.ToLowerInvariant() );
#else
		return extension;
#endif
	}
	#endregion

	#region Import Functions
	public static void PickFile( FilePickedCallback callback, params string[] allowedFileTypes )
	{
		// If no file type is specified, allow all file types
		if( allowedFileTypes == null || allowedFileTypes.Length == 0 )
		{
#if UNITY_ANDROID
			allowedFileTypes = new string[] { "*/*" };
#else
			allowedFileTypes = new string[] { "public.item", "public.content" };
#endif
		}

		RequestPermissionAsync( ( permission ) =>
		{
			if( permission != Permission.Granted || IsFilePickerBusy() )
			{
				callback?.Invoke( null );
				return;
			}

#if UNITY_EDITOR
			// Accept Android and iOS UTIs when possible, for user's convenience
			string[] editorFilters = new string[allowedFileTypes.Length * 2];
			for( int i = 0; i < allowedFileTypes.Length; i++ )
			{
				if( allowedFileTypes[i].IndexOf( '*' ) >= 0 )
				{
					if( allowedFileTypes[i] == "image/*" )
					{
						editorFilters[i * 2] = "Image files";
						editorFilters[i * 2 + 1] = "png,jpg,jpeg";
					}
					else if( allowedFileTypes[i] == "video/*" )
					{
						editorFilters[i * 2] = "Video files";
						editorFilters[i * 2 + 1] = "mp4,mov,webm,avi";
					}
					else if( allowedFileTypes[i] == "audio/*" )
					{
						editorFilters[i * 2] = "Audio files";
						editorFilters[i * 2 + 1] = "mp3,wav,aac,flac";
					}
					else
					{
						editorFilters[i * 2] = "All files";
						editorFilters[i * 2 + 1] = "*";
					}
				}
				else
				{
					editorFilters[i * 2] = allowedFileTypes[i];

					if( allowedFileTypes[i].IndexOf( '/' ) >= 0 ) // Android UTIs like 'image/png'
						editorFilters[i * 2 + 1] = allowedFileTypes[i].Substring( allowedFileTypes[i].IndexOf( '/' ) + 1 );
					else if( allowedFileTypes[i].StartsWith( "public." ) ) // iOS UTIs like 'public.png'
						editorFilters[i * 2 + 1] = allowedFileTypes[i].Substring( 7 );
					else if( allowedFileTypes[i].IndexOf( '.' ) == 0 ) // Extensions starting with period like '.png'
						editorFilters[i * 2 + 1] = allowedFileTypes[i].Substring( 1 );
					else
						editorFilters[i * 2 + 1] = allowedFileTypes[i];
				}
			}

			string pickedFile = UnityEditor.EditorUtility.OpenFilePanelWithFilters( "Select file", "", editorFilters );

			if( callback != null )
				callback( pickedFile != "" ? pickedFile : null );
#elif UNITY_ANDROID
			AJC.CallStatic( "PickFiles", Context, new FPResultCallbackAndroid( callback, null, null ), false, SelectedFilePath, allowedFileTypes, "" );
#elif UNITY_IOS
			FPResultCallbackiOS.Initialize( callback, null, null );
			_NativeFilePicker_PickFile( allowedFileTypes, allowedFileTypes.Length );
#else
			if( callback != null )
				callback( null );
#endif
		}, true );
	}

	public static void PickMultipleFiles( MultipleFilesPickedCallback callback, params string[] allowedFileTypes )
	{
		// If no file type is specified, allow all file types
		if( allowedFileTypes == null || allowedFileTypes.Length == 0 )
		{
#if UNITY_ANDROID
			allowedFileTypes = new string[] { "*/*" };
#else
			allowedFileTypes = new string[] { "public.item", "public.content" };
#endif
		}

		RequestPermissionAsync( ( permission ) =>
		{
			if( permission != Permission.Granted || IsFilePickerBusy() )
			{
				callback?.Invoke( null );
				return;
			}

			if( CanPickMultipleFiles() )
			{
#if !UNITY_EDITOR && UNITY_ANDROID
				AJC.CallStatic( "PickFiles", Context, new FPResultCallbackAndroid( null, callback, null ), true, SelectedFilePath, allowedFileTypes, "" );
#elif !UNITY_EDITOR && UNITY_IOS
				FPResultCallbackiOS.Initialize( null, callback, null );
				_NativeFilePicker_PickMultipleFiles( allowedFileTypes, allowedFileTypes.Length );
#endif
			}
			else if( callback != null )
				callback( null );
		}, true );
	}
	#endregion

	#region Export Functions
	public static void ExportFile( string filePath, FilesExportedCallback callback = null )
	{
		if( string.IsNullOrEmpty( filePath ) )
			throw new ArgumentException( "Parameter 'filePath' is null or empty!" );

		RequestPermissionAsync( ( permission ) =>
		{
			if( permission != Permission.Granted || IsFilePickerBusy() )
			{
				callback?.Invoke( false );
				return;
			}

			if( CanExportFiles() )
			{
#if UNITY_EDITOR
				string extension = Path.GetExtension( filePath );
				if( extension == null )
					extension = "";
				else if( extension.IndexOf( '.' ) == 0 )
					extension = extension.Substring( 1 );

				string destination = UnityEditor.EditorUtility.SaveFilePanel( "Select destination", Path.GetDirectoryName( filePath ), Path.GetFileName( filePath ), extension );
				if( string.IsNullOrEmpty( destination ) )
				{
					if( callback != null )
						callback( false );
				}
				else
				{
					try
					{
						File.Copy( filePath, destination, true );

						if( callback != null )
							callback( true );
					}
					catch( Exception e )
					{
						Debug.LogException( e );

						if( callback != null )
							callback( false );
					}
				}
#elif UNITY_ANDROID
				AJC.CallStatic( "ExportFiles", Context, new FPResultCallbackAndroid( null, null, callback ), new string[1] { filePath }, 1 );
#elif UNITY_IOS
				FPResultCallbackiOS.Initialize( null, null, callback );
				_NativeFilePicker_ExportFiles( new string[1] { filePath }, 1 );
#endif
			}
			else if( callback != null )
				callback( false );
		}, false );
	}

	public static void ExportMultipleFiles( string[] filePaths, FilesExportedCallback callback = null )
	{
		if( filePaths == null || filePaths.Length == 0 )
			throw new ArgumentException( "Parameter 'filePaths' is null or empty!" );

		RequestPermissionAsync( ( permission ) =>
		{
			if( permission != Permission.Granted || IsFilePickerBusy() )
			{
				callback?.Invoke( false );
				return;
			}

			if( CanExportMultipleFiles() )
			{
#if UNITY_EDITOR
				string destination = UnityEditor.EditorUtility.OpenFolderPanel( "Select destination", Path.GetDirectoryName( filePaths[0] ), "" );
				if( string.IsNullOrEmpty( destination ) )
				{
					if( callback != null )
						callback( false );
				}
				else
				{
					try
					{
						for( int i = 0; i < filePaths.Length; i++ )
							File.Copy( filePaths[i], Path.Combine( destination, Path.GetFileName( filePaths[i] ) ), true );

						if( callback != null )
							callback( true );
					}
					catch( Exception e )
					{
						Debug.LogException( e );

						if( callback != null )
							callback( false );
					}
				}
#elif UNITY_ANDROID
				AJC.CallStatic( "ExportFiles", Context, new FPResultCallbackAndroid( null, null, callback ), filePaths, filePaths.Length );
#elif UNITY_IOS
				FPResultCallbackiOS.Initialize( null, null, callback );
				_NativeFilePicker_ExportFiles( filePaths, filePaths.Length );
#endif
			}
			else if( callback != null )
				callback( false );
		}, false );
	}
	#endregion
}