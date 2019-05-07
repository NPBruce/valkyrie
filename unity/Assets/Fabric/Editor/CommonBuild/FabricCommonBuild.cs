namespace Fabric.Internal.Editor.CommonBuild
{
	using Fabric.Internal.Editor.Model;
	using UnityEditor;
	using UnityEngine;
	using UnityEditor.Callbacks;
	using System;

	internal class FabricCommonBuild : MonoBehaviour
	{
		private static string MakeKitNameAndInitMethod(string kit)
		{
			// Going forward, all new kits should have their Init method in 'Fabric.<kit>.<kit>.Init()' format.
			// Crashlytics does this a bit different at the moment, thus, we need to special-case it.
			if (kit.Equals ("Crashlytics", System.StringComparison.OrdinalIgnoreCase)) {
				return "Fabric.Internal.Crashlytics.CrashlyticsInit.RegisterExceptionHandlers";
			}

			return "Fabric." + kit + "." + kit + ".Init";
		}

		internal static string InitializationKitsList()
		{
			System.Text.StringBuilder kits = new System.Text.StringBuilder ();

			foreach (Settings.InstalledKit kit in Settings.Instance.InstalledKits) {
				kits.Append (CommonBuild.FabricCommonBuild.MakeKitNameAndInitMethod (kit.Name));
				kits.Append (',');
			}

			// Remove the trailing comma
			return kits.Remove (kits.Length - 1, 1).ToString ();
		}

		[PostProcessScene(0)]
		internal static void ModifyKitPrefabs()
		{
			ModifyKitPrefabs (Settings.Instance.Initialization);
		}

		internal static void ModifyKitPrefabs(Settings.InitializationType initializationType)
		{
			// Use imported kits instead of installed kits. If the user has dragged the prefab
			// into a scene but has not finished onboarding completely, we need to remove the
			// attached script.
			foreach (ImportedKit kit in Controller.KitUtils.ListImportedKits (null)()) {
				ModifyKitPrefab (kit.Name, initializationType);
			}
		}

		internal static void ModifyKitPrefab(string kit, Settings.InitializationType initializationType)
		{
			string gameObjectName = kit + "Init";
			string gameObjectScriptName = kit + "Init";
			GameObject gameObject = null;

			foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType (typeof (GameObject))) {
				if (obj.name.StartsWith (gameObjectName)) {
					gameObject = obj;
					break;
				}
			}
			
			if (gameObject == null) {
				Fabric.Internal.Editor.Utils.Warn ("Couldn't find {0}'s GameObject", kit);
				return;
			}

			Component script = gameObject.GetComponent (gameObjectScriptName);

			switch (initializationType) {
			case Settings.InitializationType.Manual:
				if (script != null) {
					Fabric.Internal.Editor.Utils.Log ("Removing component {0} from {1} prefab.", gameObjectScriptName, gameObject);
					Component.DestroyImmediate (script);
				}
				break;
			case Settings.InitializationType.Automatic:
				if (script == null) {
					Fabric.Internal.Editor.Utils.Log ("Adding component {0} to {1} prefab.", gameObjectScriptName, gameObject);
					gameObject.AddComponent (Type.GetType (string.Format ("Fabric.Internal.{0}", gameObjectScriptName)));
				}
				break;
			}
		}
	}
}
