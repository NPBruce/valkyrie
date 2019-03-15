namespace Fabric.Internal.Editor.Prebuild
{
	using UnityEngine;
	using UnityEditor;
	using UnityEditor.Callbacks;
	using System;
	using System.IO;
	using Fabric.Internal.Editor.Model;

	public class FabricAndroidPrebuild : FabricSetup
	{
		private static readonly string BuildIdXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><resources><string name=\"com.crashlytics.android.build_id\">{0}</string></resources>";
		private static readonly string ResFilePath = Update.FileUtils.NormalizePathForPlatform (Path.Combine (
			Application.dataPath,
			"Plugins/Android/fabric/res/values/strings.xml"
		));

		private static bool Generated = false;

		[PostProcessScene(0)]
		public static void UpdateBuildId()
		{
			if (!IsAndroidBuild() || Generated || Application.isPlaying) {
				return;
			}

			string buildId = GenerateBuildId ();
			WriteBuildIdFile (buildId);
			AndroidBuildPropertiesManager.UpdateBuildProperties (buildId);
			Generated = true;
		}

		[PostProcessScene(0)]
		public static void SetInitializationType()
		{
			if (!IsAndroidBuild () || Application.isPlaying) {
				return;
			}

			SetManifestMetaData ("io.fabric.InitializationType", Settings.Instance.Initialization.ToString ());
		}

		[PostProcessScene(0)]
		private static void SetInitializationKitsList()
		{
			if (!IsAndroidBuild () || Application.isPlaying) {
				return;
			}

			if (Settings.Instance.Initialization == Settings.InitializationType.Automatic || Settings.Instance.InstalledKits.Count == 0) {
				return;
			}

			SetManifestMetaData ("io.fabric.InitializationKitsList", CommonBuild.FabricCommonBuild.InitializationKitsList ());
		}

		[PostProcessBuild(0)]
		public static void BuildComplete(BuildTarget target, string pathToBuiltProject)
		{
			if (Application.isPlaying) {
				return;
			}
			Generated = false;
		}

		private static bool IsAndroidBuild()
		{
			return EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
		}

		private static string GenerateBuildId()
		{
			return Guid.NewGuid ().ToString ();
		}

		private static void WriteBuildIdFile(string buildId)
		{
			try {
				using (StreamWriter writer = new StreamWriter (File.Create (ResFilePath))) {
					writer.WriteLine (String.Format (BuildIdXml, buildId));
				}
				
				Utils.Log ("Generated new Fabric build ID: {0}", buildId);
			} catch (Exception e) {
				Utils.Error ("Could not write build ID resource file. {0}", e.Message);
			}
		}

		private static void SetManifestMetaData(string key, string value)
		{
			string unityManifestPath = FindUnityAndroidManifest ();

			if (unityManifestPath == null) {
				Utils.Warn ("Could not find Unity's AndroidManifest.xml file, cannot initialize Fabric for Android.");
				return;
			}

			BootstrapTopLevelManifest (unityManifestPath);
			InjectMetadataIntoFabricManifest (key, value);
		}
	}
}
