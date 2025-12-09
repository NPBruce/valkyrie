using System.IO;
using UnityEditor;
using UnityEngine;
#if UNITY_IOS
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
#endif

namespace NativeFilePickerNamespace
{
	[System.Serializable]
	public class Settings
	{
		private const string SAVE_PATH = "ProjectSettings/NativeFilePicker.json";

		public bool AutoSetupFrameworks = true;
		public bool AutoSetupiCloud = false;

		private static Settings m_instance = null;
		public static Settings Instance
		{
			get
			{
				if( m_instance == null )
				{
					try
					{
						if( File.Exists( SAVE_PATH ) )
							m_instance = JsonUtility.FromJson<Settings>( File.ReadAllText( SAVE_PATH ) );
						else
							m_instance = new Settings();
					}
					catch( System.Exception e )
					{
						Debug.LogException( e );
						m_instance = new Settings();
					}
				}

				return m_instance;
			}
		}

		public void Save()
		{
			File.WriteAllText( SAVE_PATH, JsonUtility.ToJson( this, true ) );
		}

		[SettingsProvider]
		public static SettingsProvider CreatePreferencesGUI()
		{
			return new SettingsProvider( "Project/yasirkula/Native File Picker", SettingsScope.Project )
			{
				guiHandler = ( searchContext ) => PreferencesGUI(),
				keywords = new System.Collections.Generic.HashSet<string>() { "Native", "File", "Picker", "Android", "iOS" }
			};
		}

		public static void PreferencesGUI()
		{
			EditorGUI.BeginChangeCheck();

			Instance.AutoSetupFrameworks = EditorGUILayout.Toggle( new GUIContent( "Auto Setup Frameworks", "Automatically adds MobileCoreServices and CloudKit frameworks to the generated Xcode project" ), Instance.AutoSetupFrameworks );
			Instance.AutoSetupiCloud = EditorGUILayout.Toggle( new GUIContent( "Auto Setup iCloud", "Automatically enables iCloud capability of the generated Xcode project" ), Instance.AutoSetupiCloud );

			if( EditorGUI.EndChangeCheck() )
				Instance.Save();
		}
	}

	public class NativeFilePickerPostProcessBuild
	{
#if UNITY_IOS
#pragma warning disable 0162
		[PostProcessBuild]
		public static void OnPostprocessBuild( BuildTarget target, string buildPath )
		{
			// Add declared custom types to Info.plist
			if( target == BuildTarget.iOS )
			{
				NativeFilePickerCustomTypes.TypeHolder[] customTypes = NativeFilePickerCustomTypes.GetCustomTypes();
				if( customTypes != null )
				{
					string plistPath = Path.Combine( buildPath, "Info.plist" );

					PlistDocument plist = new PlistDocument();
					plist.ReadFromString( File.ReadAllText( plistPath ) );

					PlistElementDict rootDict = plist.root;

					for( int i = 0; i < customTypes.Length; i++ )
					{
						NativeFilePickerCustomTypes.TypeHolder customType = customTypes[i];
						PlistElementArray customTypesArray = GetCustomTypesArray( rootDict, customType.isExported );

						// Don't allow duplicate entries
						RemoveCustomTypeIfExists( customTypesArray, customType.identifier );

						PlistElementDict customTypeDict = customTypesArray.AddDict();
						customTypeDict.SetString( "UTTypeIdentifier", customType.identifier );
						customTypeDict.SetString( "UTTypeDescription", customType.description );

						PlistElementArray conformsTo = customTypeDict.CreateArray( "UTTypeConformsTo" );
						for( int j = 0; j < customType.conformsTo.Length; j++ )
							conformsTo.AddString( customType.conformsTo[j] );

						PlistElementDict tagSpecification = customTypeDict.CreateDict( "UTTypeTagSpecification" );
						PlistElementArray tagExtensions = tagSpecification.CreateArray( "public.filename-extension" );
						for( int j = 0; j < customType.extensions.Length; j++ )
							tagExtensions.AddString( customType.extensions[j] );
					}

					File.WriteAllText( plistPath, plist.WriteToString() );
				}
			}

			// Rest of the function shouldn't execute unless build post-processing is enabled
			if( !Settings.Instance.AutoSetupFrameworks && !Settings.Instance.AutoSetupiCloud )
				return;

			if( target == BuildTarget.iOS )
			{
				string pbxProjectPath = PBXProject.GetPBXProjectPath( buildPath );

				PBXProject pbxProject = new PBXProject();
				pbxProject.ReadFromFile( pbxProjectPath );

				string targetGUID = pbxProject.GetUnityFrameworkTargetGuid();
				if( Settings.Instance.AutoSetupFrameworks )
				{
					pbxProject.AddFrameworkToProject( targetGUID, "MobileCoreServices.framework", false );
					pbxProject.AddFrameworkToProject( targetGUID, "CloudKit.framework", false );
				}

				File.WriteAllText( pbxProjectPath, pbxProject.WriteToString() );

				if( Settings.Instance.AutoSetupiCloud )
				{
					ProjectCapabilityManager manager = new ProjectCapabilityManager( pbxProjectPath, "iCloud.entitlements", "Unity-iPhone" );
					manager.AddiCloud( false, true, false, true, null );
					manager.WriteToFile();
				}
			}
		}

		// Adding PRODUCT_BUNDLE_IDENTIFIER if not exists (if another plugin also fills this value, we must not touch it)
		[PostProcessBuild( 99 )]
		public static void OnPostprocessBuild2( BuildTarget target, string buildPath )
		{
			if( !Settings.Instance.AutoSetupFrameworks && !Settings.Instance.AutoSetupiCloud )
				return;

			if( target == BuildTarget.iOS )
			{
				string pbxProjectPath = PBXProject.GetPBXProjectPath( buildPath );

				PBXProject pbxProject = new PBXProject();
				pbxProject.ReadFromFile( pbxProjectPath );

				string targetGUID = pbxProject.GetUnityFrameworkTargetGuid();
				if( string.IsNullOrEmpty( pbxProject.GetBuildPropertyForAnyConfig( targetGUID, "PRODUCT_BUNDLE_IDENTIFIER" ) ) )
					pbxProject.AddBuildProperty( targetGUID, "PRODUCT_BUNDLE_IDENTIFIER", PlayerSettings.applicationIdentifier );

				File.WriteAllText( pbxProjectPath, pbxProject.WriteToString() );
			}
		}

		private static PlistElementArray GetCustomTypesArray( PlistElementDict rootDict, bool isExported )
		{
			string key = isExported ? "UTExportedTypeDeclarations" : "UTImportedTypeDeclarations";
			PlistElementArray result = rootDict[key] as PlistElementArray;
			if( result == null )
				result = rootDict.CreateArray( key );

			return result;
		}

		private static void RemoveCustomTypeIfExists( PlistElementArray customTypesArray, string UTI )
		{
			List<PlistElement> values = customTypesArray.values;
			if( values == null )
				return;

			for( int i = values.Count - 1; i >= 0; i-- )
			{
				PlistElementDict exportedType = values[i] as PlistElementDict;
				if( exportedType != null )
				{
					PlistElementString exportedTypeID = exportedType["UTTypeIdentifier"] as PlistElementString;
					if( exportedTypeID != null && exportedTypeID.value == UTI )
						values.RemoveAt( i );
				}
			}
		}
#pragma warning restore 0162
#endif
	}
}