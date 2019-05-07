namespace Fabric.Internal.Crashlytics.Editor.Postbuild
{
	using UnityEngine;
	using UnityEditor;
	using UnityEditor.Callbacks;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using Fabric.Internal.Editor;
	using Fabric.Internal.Editor.Model;
	using Fabric.Internal.Editor.Postbuild;
	using Fabric.Internal.Editor.ThirdParty.xcodeapi;
#if !UNITY_5_5_OR_NEWER
	using System.Linq;
#endif
	
	public class CrashlyticsPostBuildiOS : PostBuildiOS
	{
		private const string Name = Controller.Controller.Name;

		private static string[] platformFrameworks = new string[] {
			"Security.framework"
		};

		private static string[] frameworks = new string[] {
			"Fabric.framework",
			"Crashlytics.framework"
		};

		private static string[] platformLibs = new string[] {
			"libz.dylib",
			"libc++.dylib"
		};

		private static string[] libs = new string[] {
			"Fabric-Init/libFabriciOSInit.a",
			"Crashlytics-Wrapper/libCrashlyticsiOSWrapper.a"
		};

		[PostProcessBuild(100)]
		public static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
		{
			if (!IsKitOnboarded ("Crashlytics")) {
				Fabric.Internal.Editor.Utils.Warn ("Crashlytics not yet onboarded, skipping post-build steps.");
				return;
			}

			// BuiltTarget.iOS is not defined in Unity 4, so we just use strings here
			if (buildTarget.ToString () == "iOS" || buildTarget.ToString () == "iPhone") {
				CheckiOSVersion ();

				PrepareProject (buildPath);
				PreparePlist (buildPath);
			}
		}

		private static bool Enabled ()
		{
			var installed = Settings.Instance.InstalledKits.Find (k => k.Name.Equals (Name));
			return installed != null && installed.Enabled;
		}

		private static void CheckiOSVersion ()
		{
#if UNITY_5_5_OR_NEWER
			var isOldiOSVersion = new System.Version (PlayerSettings.iOS.targetOSVersionString) < new System.Version ("6.0");
#else
			iOSTargetOSVersion[] oldiOSVersions = {
				iOSTargetOSVersion.iOS_4_0,
				iOSTargetOSVersion.iOS_4_1,
				iOSTargetOSVersion.iOS_4_2,
				iOSTargetOSVersion.iOS_4_3,
				iOSTargetOSVersion.iOS_5_0,
				iOSTargetOSVersion.iOS_5_1
			};
			var isOldiOSVersion = oldiOSVersions.Contains (PlayerSettings.iOS.targetOSVersion);
#endif
			if (Enabled () && isOldiOSVersion) {
				Fabric.Internal.Editor.Utils.Error (
					"{0} requires iOS 6+. Please change the Target iOS Version in Player Settings to iOS 6 or higher.",
					Name
				);
			}
		}

		private static void PrepareProject (string buildPath)
		{
			string projPath = Path.Combine (buildPath, "Unity-iPhone.xcodeproj/project.pbxproj");
			PBXProject project = new PBXProject();
			project.ReadFromString (File.ReadAllText(projPath));		
			string target = project.TargetGuidByName ("Unity-iPhone");
			
			AddPlatformFrameworksToProject (platformFrameworks, project, target);		
			AddFrameworksToProject (frameworks, buildPath, project, target);		
			AddPlatformLibsToProject (platformLibs, project, target);
			AddLibsToProject (libs, project, target, buildPath);

			Fabric.Internal.Editor.Utils.Log ("Setting Debug Information Format to DWARF with dSYM File in Xcode project.");
			project.SetBuildProperty (target, "DEBUG_INFORMATION_FORMAT", "dwarf-with-dsym");
			
			File.WriteAllText (projPath, project.WriteToString());
			
			if (!Enabled ()) {
				Fabric.Internal.Editor.Utils.Log ("{0} disabled. Crashlytics will not be initialized on app launch.", Name);
			}
		}

		private static void PreparePlist (string buildPath)
		{
			Dictionary<string, PlistElementDict> kitsDict = new Dictionary<string, PlistElementDict>();
			
			if (Enabled ()) {
				kitsDict.Add(Name, new PlistElementDict ());
			}
			
			AddFabricKitsToPlist(buildPath, kitsDict);
			AddCrashlyticsVersionToPlist (buildPath);
		}	

		private static void AddCrashlyticsVersionToPlist (string buildPath)
		{
			Utils.Log ("Adding Crashlytics version to Info.plist");

			string plistPath = Path.Combine (buildPath, "Info.plist");			
			PlistDocument plist = new PlistDocument();

			plist.ReadFromFile(plistPath);
			plist.root.SetString("CrashlyticsUnityVersion", Info.Version.ToString());
			plist.WriteToFile(plistPath);
		}
	}

}
