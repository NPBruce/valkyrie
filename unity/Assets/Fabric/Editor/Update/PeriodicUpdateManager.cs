namespace Fabric.Internal.Editor.Update
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;
	using Fabric.Internal.Editor.Model;
	using KitsList = System.Collections.Generic.List<Dependency.DependencyGraphObject.DependenciesObject.KitsObject>;
	using KitsObject = Dependency.DependencyGraphObject.DependenciesObject.KitsObject;
	using PluginObject = Dependency.DependencyGraphObject.PluginObject;
	using KitUtils = Fabric.Internal.Editor.Controller.KitUtils;
	using VersionedDependency = Dependency.DependencyGraphResolver.VersionedDependency;

	[InitializeOnLoad]
	internal class PeriodicUpdateManager {
		private static readonly int periodMillis = 6 * 60 * 60 * 1000; // 6h
		private static readonly int delayMillis = 2 * 1000; // 2s
		private static readonly PeriodicUpdateChecker updateChecker;
		private static bool firstInvokation = true;

		#region Update Checks

		private static void CheckForPluginUpdate()
		{
			if (IsPluginUpdateAvailable ()) {
				Utils.Log (
					"Plugin version {0} is available! Please open the Fabric plugin to install the latest version.",
					LatestAvailablePluginVersion ().ToString ()
				);
			}
		}

		private static void CheckForKitUpdates()
		{
			try {
				KitsList available = LatestAvailableKitsVersions ();
				List<ImportedKit> installed = KitUtils.ListImportedKits (null)().FindAll (
					k => KitUtils.IsKitInstalled (k.Name)
				);

				foreach (ImportedKit kit in installed) {
					KitsObject upgradeable = available.Find (
						k => k.Name.Equals (kit.Name, System.StringComparison.OrdinalIgnoreCase) && new System.Version (k.Version) > kit.Instance.Version ()
					);

					if (upgradeable == null) {
						continue;
					}

					Utils.Log (
						"{0} kit version {1} is available! Please open the Fabric plugin to install the latest version.",
						upgradeable.Name,
						upgradeable.Version
					);
				}
			} catch (System.Exception) {
				Utils.Log ("Couldn't determine whether kit updates are available!");
			}
		}

		#endregion

		static PeriodicUpdateManager ()
		{
			updateChecker = new PeriodicUpdateChecker (periodMillis, delayMillis);
			updateChecker.RegisterUpdateCheckCallback (delegate () {
				CheckForPluginUpdate ();

				if (firstInvokation) {
					firstInvokation = false;
					return;
				}

				CheckForKitUpdates ();
			});

			EditorApplication.update += StartChecking;
		}

		private static void StartChecking ()
		{
			if (!Net.Validator.Initialize (FileUtils.NormalizePathForPlatform (Application.dataPath))) {
				Utils.Warn ("Failed to initialize validator");
			}

			// Loads the settings from disk if not yet loaded.
			if (Settings.Instance != null) {
				updateChecker.Start ();
			} else {
				Utils.Warn ("Failed to load settings. Update checks are disabled.");
			}

			EditorApplication.update -= StartChecking;
		}

		public static bool Suspend()
		{
			Dependency.DependencyGraphManager dg = updateChecker.GetDependencyGraphManager ();
			return dg != null ? dg.Suspend () : false;
		}

		public static void Continue()
		{
			Dependency.DependencyGraphManager dg = updateChecker.GetDependencyGraphManager ();
			if (dg != null) {
				dg.Continue ();
			}
		}

		public static HashSet<string> Resolve(string name, Dictionary<string, HashSet<VersionedDependency>> installed)
		{
			Dependency.DependencyGraphManager dg = updateChecker.GetDependencyGraphManager ();
			return dg != null ? dg.Resolve (name, installed) : new HashSet<string> ();
		}

		public static bool IsPluginUpdateAvailable()
		{
			Dependency.DependencyGraphManager dg = updateChecker.GetDependencyGraphManager ();
			return dg != null && dg.LatestAvailablePluginVersion () > Fabric.Internal.Editor.Info.Version;
		}

		public static bool IsKitUpdateAvailable(string name, System.Version installedVersion)
		{
			Dependency.DependencyGraphManager dg = updateChecker.GetDependencyGraphManager ();
			if (dg == null) {
				return false;
			}

			KitsObject found = dg.LatestAvailableKitVersions ().Find (
				k => k.Name.Equals (name, System.StringComparison.OrdinalIgnoreCase)
			);
			return found != null && new System.Version (found.Version) > installedVersion;
		}

		public static bool IsPluginUpdateRequired(string name)
		{
			Dependency.DependencyGraphManager dg = updateChecker.GetDependencyGraphManager ();
			if (dg == null) {
				return false;
			}

			KitsObject found = dg.LatestAvailableKitVersions ().Find (
				k => k.Name.Equals (name, System.StringComparison.OrdinalIgnoreCase)
			);

			return found != null && new System.Version (found.MinimumPluginVersion) > Fabric.Internal.Editor.Info.Version;
		}

		public static System.Version LatestAvailablePluginVersion()
		{
			Dependency.DependencyGraphManager dg = updateChecker.GetDependencyGraphManager ();
			return dg != null ? dg.LatestAvailablePluginVersion () : Fabric.Internal.Editor.Info.Version;
		}

		public static KitsList LatestAvailableKitsVersions()
		{
			Dependency.DependencyGraphManager dg = updateChecker.GetDependencyGraphManager ();
			return dg != null ? dg.LatestAvailableKitVersions () : new KitsList ();
		}

		public static KitsList LatestAvailableOnboardableKitVersions()
		{
			Dependency.DependencyGraphManager dg = updateChecker.GetDependencyGraphManager ();
			return dg != null ? dg.LatestAvailableOnboardableKitVersions () : new KitsList ();
		}

		public static PluginObject PluginDescriptor()
		{
			Dependency.DependencyGraphManager dg = updateChecker.GetDependencyGraphManager ();
			return dg != null ? dg.PluginDescriptor () : null;
		}

		public static HashSet<VersionedDependency> TransitiveDependencyChainFor(string kit)
		{
			Dependency.DependencyGraphManager dg = updateChecker.GetDependencyGraphManager ();
			return dg != null ? Dependency.DependencyGraphManager.TransitiveDependencyChainFor (dg.GraphObject (), kit) : new HashSet<VersionedDependency> ();
		}
	}
}
