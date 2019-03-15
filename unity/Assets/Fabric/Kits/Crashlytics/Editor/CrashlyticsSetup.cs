namespace Fabric.Internal.Crashlytics.Editor
{
	using UnityEditor;
	using System.IO;
	using UnityEngine;
	using System.Xml;
	using Fabric.Internal.Editor;
	using Fabric.Internal.Editor.Model;
	
	public class CrashlyticsSetup : FabricSetup
	{
		private const string Name = Controller.Controller.Name;

		public static void EnableCrashlytics (bool checkSetupComplete)
		{
			var settings = Settings.Instance;
			var listed = settings.InstalledKits.Find (k => k.Name.Equals (Name));
			var installed = listed != null && listed.Installed;

			if (checkSetupComplete && !installed) {
				Fabric.Internal.Editor.Utils.Error ("Please first prepare Crashlytics in the Fabric menu to obtain your login credentials.");
				return;
			}

			SetKitScriptExecutionOrder (typeof(Fabric.Internal.Crashlytics.CrashlyticsInit));

			EnableCrashlyticsiOS ();
			EnableCrashlyticsAndroid ();

			if (listed != null) {
				listed.Enabled = true;
			}
		}

		public static void DisableCrashlytics ()
		{
			var listed = Settings.Instance.InstalledKits.Find (k => k.Name.Equals (Name));

			if (listed != null) {
				listed.Enabled = false;
				DisableCrashlyticsiOS ();
				DisableCrashlyticsAndroid ();
			}
		}

		private static void EnableCrashlyticsiOS ()
		{
			// In the case of iOS, this is currently taken care of at post-build time
		}

		private static void DisableCrashlyticsiOS ()
		{
			// In the case of iOS, this is currently taken care of at post-build time
		}

		private static void EnableCrashlyticsAndroid ()
		{
			string unityManifestPath = FindUnityAndroidManifest ();

			if (unityManifestPath == null) {
				Utils.Warn ("Could not find Unity's AndroidManifest.xml file, cannot initialize Crashlytics for Android.");
				return;
			}

			BootstrapTopLevelManifest (unityManifestPath);
			ToggleApplicationInTopLevelManifest (enableFabric: true);

			InjectMetadataIntoFabricManifest ("io.fabric.ApiKey", Settings.Instance.Organization.ApiKey);
			InjectMetadataIntoFabricManifest ("io.fabric.unity.crashlytics.version", Info.Version.ToString ());

			InjectMetadataIntoFabricManifest ("io.fabric.crashlytics.qualified", "com.crashlytics.android.Crashlytics");
			InjectMetadataIntoFabricManifest ("io.fabric.crashlytics.unqualified", "Crashlytics");
			InjectMetadataIntoFabricManifest ("io.fabric.kits", "crashlytics", true);
		}

		private static void DisableCrashlyticsAndroid ()
		{
			RemoveMetadataFromFabricManifest ("io.fabric.kits", "crashlytics");
		}
	}

}
