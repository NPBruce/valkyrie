namespace Fabric.Internal.Editor.Update.Dependency
{
	using System.Collections.Generic;
	using System;
	using KitsList = System.Collections.Generic.List<DependencyGraphObject.DependenciesObject.KitsObject>;
	using KitsObject = DependencyGraphObject.DependenciesObject.KitsObject;
	using IncompatibilityObject = DependencyGraphObject.DependenciesObject.IncompatibilityObject;
	using UnityEngine;

	internal class DependencyGraphResolver
	{
		private readonly DependencyGraphObject dependencyGraph;

		#region VersionedDependency
		[Serializable]
		internal class VersionedDependency
		{
			[SerializeField]
			public string Name;

			[SerializeField]
			public string Version;

			#region Equals and GetHashCode
			public override bool Equals(object obj)
			{
				if (obj == null || GetType () != obj.GetType ()) {
					return false;
				}

				VersionedDependency dep = obj as VersionedDependency;

				return
					this.Name.Equals (dep.Name, StringComparison.OrdinalIgnoreCase) &&
					this.Version.Equals (dep.Version, StringComparison.OrdinalIgnoreCase);
			}

			public override int GetHashCode()
			{
				return (Name + Version).GetHashCode ();
			}
			#endregion
		}
		#endregion

		public DependencyGraphResolver(DependencyGraphObject dependencyGraph)
		{
			this.dependencyGraph = dependencyGraph;
		}

		public HashSet<string> Resolve(string name, HashSet<VersionedDependency> transitiveDependencyChain, Dictionary<string, HashSet<VersionedDependency>> installed)
		{
			List<string> flagged = new List<string> ();

			if (dependencyGraph != null) {
				// This algorithm is not as efficient as it should be. We check each kit's dependency chain for the
				// presence of each dependency in the about-to-be-installed kit. In theory, can be improved. On paper,
				// this doesn't matter given the number of dependencies and kits that need to be checked.
				foreach (VersionedDependency requiredDependency in transitiveDependencyChain) {
					flagged.AddRange (Resolve (requiredDependency, installed));
				}
			}

			HashSet<string> toUpdate = new HashSet<String> (flagged);
			toUpdate.Remove (name);
			return toUpdate;
		}

		#region Resolve Implementation

		private List<string> Resolve(VersionedDependency requiredDependency, Dictionary<string, HashSet<VersionedDependency>> installed)
		{
			List<string> flagged = new List<string> ();

			foreach (var kitChain in installed) {
				flagged.AddRange (Resolve (requiredDependency, kitChain));
			}

			return flagged;
		}

		private List<string> Resolve(VersionedDependency requiredDependency, KeyValuePair<string, HashSet<VersionedDependency>> installed)
		{
			List<string> flagged = new List<string> ();

			foreach (VersionedDependency installedDependency in installed.Value) {
				if (IsConflicting (installedDependency, requiredDependency)) {
					flagged.Add (installed.Key);
				}
			}

			return flagged;
		}

		private bool IsConflicting(VersionedDependency installed, VersionedDependency required)
		{
			return installed.Name.Equals (required.Name, StringComparison.OrdinalIgnoreCase) && IncompatibilityExistsBetween (
				installed.Name,
				new System.Version (installed.Version),
				new System.Version (required.Version)
			);
		}

		private bool IncompatibilityExistsBetween(string name, System.Version installed, System.Version required)
		{
			IncompatibilityObject incompatibilities = dependencyGraph.Dependencies.Incompatibilities.Find (
				obj => obj.Name.Equals (name, StringComparison.OrdinalIgnoreCase)
			);

			if (incompatibilities == null || incompatibilities.Versions.Count == 0) {
				return false;
			}

			// Check for the presence of a version in the incompatibilities list that is between
			// the installed version and the required version.
			return incompatibilities.Versions.Exists (raw => {
				System.Version version = new System.Version (raw);
				return installed < version && version <= required;
			});
		}

		#endregion
	}
}
