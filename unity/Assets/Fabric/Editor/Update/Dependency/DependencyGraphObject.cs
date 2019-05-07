namespace Fabric.Internal.Editor.Update.Dependency
{
	using System.Collections.Generic;
	using Fabric.Internal.ThirdParty.MiniJSON;

	internal class DependencyGraphObject
	{
		#region PluginObject
		internal class PluginObject
		{
			public readonly string Version;
			public readonly string Manifest;
			public readonly string PackageUrl;
			public readonly string PackageName;
			public readonly string ReleaseNotesUrl;

			private PluginObject(string version, string manifest, string packageUrl, string packageName, string releaseNotesUrl)
			{
				Version = version;
				Manifest = manifest;
				PackageUrl = packageUrl;
				PackageName = packageName;
				ReleaseNotesUrl = releaseNotesUrl;
			}

			public static PluginObject From(Dictionary<string, object> untyped)
			{
				return new PluginObject (
					untyped ["version"] as string,
					untyped ["manifest"] as string,
					untyped ["packageUrl"] as string,
					untyped ["packageName"] as string,
					untyped ["releaseNotesUrl"] as string
				);
			}
		}
		#endregion

		#region DependenciesObject
		internal class DependenciesObject
		{
			#region KitsObject
			internal class KitsObject
			{
				public readonly string Name;
				public readonly string Version;
				public readonly string PackageUrl;
				public readonly string PackageName;
				public readonly string Manifest;
				public readonly string ReleaseNotesUrl;
				public readonly List<string> DependsOn;
				public readonly string MinimumPluginVersion;

				private KitsObject(
					string name,
					string version,
					string packageUrl,
					string packageName,
					string manifest,
					string releaseNotesUrl,
					List<object> dependsOn,
					string minimumPluginVersion
				)
				{
					Name = name;
					Version = version;
					PackageUrl = packageUrl;
					PackageName = packageName;
					Manifest = manifest;
					ReleaseNotesUrl = releaseNotesUrl;
					DependsOn = dependsOn.ConvertAll (obj => obj as string);
					MinimumPluginVersion = minimumPluginVersion;
				}

				public static List<KitsObject> From(List<object> untyped)
				{
					List<KitsObject> deserialized = new List<KitsObject> ();

					foreach (object o in untyped) {
						Dictionary<string, object> typed = o as Dictionary<string, object>;

						deserialized.Add (new KitsObject (
							typed ["name"] as string,
							typed ["version"] as string,
							typed ["packageUrl"] as string,
							typed ["packageName"] as string,
							typed ["manifest"] as string,
							typed ["releaseNotesUrl"] as string,
							typed ["dependsOn"] as List<object>,
							typed ["minimumPluginVersion"] as string
						));
					}

					return deserialized;
				}
			}
			#endregion

			#region TransitiveDependenciesObject
			internal class TransitiveDependenciesObject
			{
				public readonly string Name;
				public readonly string Version;
				public readonly List<string> DependsOn;

				private TransitiveDependenciesObject(string name, string version, List<object> dependsOn)
				{
					Name = name;
					Version = version;
					DependsOn = dependsOn.ConvertAll (obj => obj as string);
				}

				public static List<TransitiveDependenciesObject> From(List<object> untyped)
				{
					List<TransitiveDependenciesObject> deserialized = new List<TransitiveDependenciesObject> ();

					foreach (object o in untyped) {
						Dictionary<string, object> typed = o as Dictionary<string, object>;

						deserialized.Add (new TransitiveDependenciesObject (
							typed ["name"] as string,
							typed ["version"] as string,
							typed ["dependsOn"] as List<object>
						));
					}

					return deserialized;
				}
			}
			#endregion

			#region IncompatibilityObject
			internal class IncompatibilityObject
			{
				public readonly string Name;
				public readonly List<string> Versions;

				private IncompatibilityObject(string name, List<object> versions)
				{
					Name = name;
					Versions = versions.ConvertAll (obj => (string)obj);
				}

				public static List<IncompatibilityObject> From(List<object> untyped)
				{
					List<IncompatibilityObject> deserialized = new List<IncompatibilityObject> ();

					foreach (object o in untyped) {
						Dictionary<string, object> typed = o as Dictionary<string, object>;

						deserialized.Add (new IncompatibilityObject (
							typed ["name"] as string,
							typed ["versions"] as List<object>
						));
					}

					return deserialized;
				}
			}
			#endregion

			public readonly List<KitsObject> Kits;
			public readonly List<TransitiveDependenciesObject> TransitiveDependnecies;
			public readonly List<IncompatibilityObject> Incompatibilities;
			public readonly List<string> Onboardable;

			private DependenciesObject(
				List<KitsObject> kits,
				List<TransitiveDependenciesObject> transitiveDependencies,
				List<IncompatibilityObject> incompatibilities,
				List<object> onboardable
			)
			{
				Kits = kits;
				TransitiveDependnecies = transitiveDependencies;
				Incompatibilities = incompatibilities;
				Onboardable = onboardable.ConvertAll (obj => (string)obj);
			}

			public static DependenciesObject From(Dictionary<string, object> untyped)
			{
				return new DependenciesObject (
					KitsObject.From (untyped ["kits"] as List<object>),
					TransitiveDependenciesObject.From (untyped ["transitiveDependencies"] as List<object>),
					IncompatibilityObject.From (untyped ["incompatibility"] as List<object>),
					untyped ["onboardable"] as List<object>
				);
			}
		}
		#endregion

		public readonly string Version;
		public readonly PluginObject Plugin;
		public readonly DependenciesObject Dependencies;

		private DependencyGraphObject(string version, PluginObject pluginObject, DependenciesObject dependenciesObject)
		{
			Version = version;
			Plugin = pluginObject;
			Dependencies = dependenciesObject;
		}

		private static DependencyGraphObject V1(Dictionary<string, object> deserialized)
		{
			// This is a naive implementation. Once a V2 is necessary, we can revisit this.
			return new DependencyGraphObject (
				deserialized ["version"] as string,
				PluginObject.From (deserialized ["plugin"] as Dictionary<string, object>),
				DependenciesObject.From (deserialized ["dependencies"] as Dictionary<string, object>)
			);
		}

		private static DependencyGraphObject FromVersion(Dictionary<string, object> deserialized, System.Version schemaVersion)
		{
			switch (schemaVersion.Major) {
			case 0:
				return V1 (deserialized);
			default:
				throw new System.Exception ("Unsupported dependency graph version, please upgrade the plugin!");
			}
		}

		public static DependencyGraphObject FromJson(string raw)
		{
			try {
				Dictionary<string, object> deserialized = Json.Deserialize (raw) as Dictionary<string, object>;
				return FromVersion (deserialized, new System.Version (deserialized ["version"] as string));
			} catch (System.Exception e) {
				Utils.Error ("Couldn't deserialize dependency graph; {0}", e.ToString ());
				return null;
			}
		}
	}
}
