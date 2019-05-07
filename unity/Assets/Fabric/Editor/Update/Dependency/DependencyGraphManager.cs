namespace Fabric.Internal.Editor.Update.Dependency
{
	using System.IO;
	using System;
	using UnityEngine;
	using System.Collections.Generic;
	using KitsList = System.Collections.Generic.List<DependencyGraphObject.DependenciesObject.KitsObject>;
	using KitsObject = DependencyGraphObject.DependenciesObject.KitsObject;
	using TransitiveDependenciesObject = DependencyGraphObject.DependenciesObject.TransitiveDependenciesObject;
	using IncompatibilityObject = DependencyGraphObject.DependenciesObject.IncompatibilityObject;
	using VersionedDependency = DependencyGraphResolver.VersionedDependency;

	internal class DependencyGraphManager
	{
		private static readonly System.TimeSpan SuspendTimeout = new System.TimeSpan (0, 10, 0); // 10 minutes
		private static readonly System.TimeSpan ManageTimeout = new System.TimeSpan (1, 0, 0); // 1 hour

		private static readonly string persistencePath = Path.Combine (
			FileUtils.Root,
			FileUtils.NormalizePathForPlatform ("Fabric/DependencyGraph.json")
		);
		private DependencyGraphObject current = null;
		private object monitorObj = new object ();

		public DependencyGraphManager()
		{
			if (!File.Exists (persistencePath)) {
				// Although a missing dependency graph is unusual, we hope that it
				// will be renewed next time we check for updates.
				Utils.Warn ("Couldn't load dependency graph ({0})", persistencePath);
				return;
			}

			current = DependencyGraphObject.FromJson (File.ReadAllText (persistencePath));
		}

		public bool Suspend()
		{
			return System.Threading.Monitor.TryEnter (monitorObj, SuspendTimeout);
		}

		public void Continue()
		{
			System.Threading.Monitor.Exit (monitorObj);
		}

		public bool Manage(string json)
		{
			if (!System.Threading.Monitor.TryEnter (monitorObj, ManageTimeout)) {
				Utils.Log ("Plugin or kit update is in progress, delaying managing new dependency graph");
				return false;
			}

			try {
				DependencyGraphObject latest = DependencyGraphObject.FromJson (json);

				if (current == null || IsNewer (current.Version, latest.Version)) {
					File.WriteAllText (persistencePath, json);
					current = latest;
					Utils.Log ("Updated dependency graph to {0}", latest.Version);
				}
			} finally {
				System.Threading.Monitor.Exit (monitorObj);
			}

			return true;
		}

		public HashSet<string> Resolve(string name, Dictionary<string, HashSet<VersionedDependency>> installed)
		{
			return new DependencyGraphResolver (current).Resolve (name, TransitiveDependencyChainFor (current, name), installed);
		}

		private static bool IsNewer(string current, string recent)
		{
			return new System.Version (recent) > new System.Version (current);
		}

		public DependencyGraphObject GraphObject()
		{
			return current;
		}

		public DependencyGraphObject.PluginObject PluginDescriptor()
		{
			return current != null ? current.Plugin : null;
		}

		public System.Version LatestAvailablePluginVersion()
		{
			return current != null ? new System.Version (current.Plugin.Version) : Fabric.Internal.Editor.Info.Version;
		}

		public KitsList LatestAvailableKitVersions()
		{
			return current != null ? current.Dependencies.Kits : new KitsList ();
		}

		public KitsList LatestAvailableOnboardableKitVersions()
		{
			return LatestAvailableKitVersions ().FindAll (delegate(KitsObject obj) {
				return current.Dependencies.Onboardable.Exists (k => k.Equals (obj.Name, System.StringComparison.OrdinalIgnoreCase));
			});
		}

		public static HashSet<VersionedDependency> TransitiveDependencyChainFor(DependencyGraphObject current, string kit)
		{
			HashSet<VersionedDependency> dependencies = new HashSet<VersionedDependency> ();
			return current != null ? Collect (current, kit, ref dependencies) : dependencies;
		}

		private static HashSet<VersionedDependency> Collect(DependencyGraphObject current, string kit, ref HashSet<VersionedDependency> collected)
		{
			KitsObject kitObject = current.Dependencies.Kits.Find (
				k => k.Name.Equals (kit, System.StringComparison.OrdinalIgnoreCase)
			);

			if (kitObject == null) {
				return collected;
			}

			return CollectTransitiveDependencies (current, kitObject.DependsOn, ref collected);
		}

		private static HashSet<VersionedDependency> CollectTransitiveDependencies(DependencyGraphObject current, List<string> transitiveDependencies, ref HashSet<VersionedDependency> collected)
		{
			foreach (string transitiveDependency in transitiveDependencies) {
				CollectTransitiveDependencies (current, transitiveDependency, ref collected);
			}

			return collected;
		}

		private static void CollectTransitiveDependencies(DependencyGraphObject current, string transitiveDependency, ref HashSet<VersionedDependency> collected)
		{
			TransitiveDependenciesObject dependency = current.Dependencies.TransitiveDependnecies.Find (
				k => k.Name.Equals (transitiveDependency, System.StringComparison.OrdinalIgnoreCase)
			);

			if (dependency == null) {
				Utils.Warn ("Failed to gather dependency information for {0}; is there a type-o in the Dependency Graph?", transitiveDependency);
				return;
			}

			collected.Add (new VersionedDependency {
				Name = dependency.Name,
				Version = dependency.Version
			});
			CollectTransitiveDependencies (current, dependency.DependsOn, ref collected);
		}
	}
}
