namespace Fabric.Internal.Editor.Controller
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using Fabric.Internal.Editor.Model;
	using Fabric.Internal.Editor.Update;

	using KitsObject = Update.Dependency.DependencyGraphObject.DependenciesObject.KitsObject;

	internal class KitUtils
	{
		public static bool IsKitInstalled(string name)
		{
			return Settings.Instance.InstalledKits.Exists (kit => kit.Name.Equals (name, StringComparison.OrdinalIgnoreCase) && kit.Installed);
		}

		public static Func<List<ImportedKit>> ListImportedKits(API.V1 api)
		{
			return delegate() {
				string root = FileUtils.Root + FileUtils.NormalizePathForPlatform ("/Fabric/Kits/");

				List<ImportedKit> kits = new List<ImportedKit> ();

				if (!Directory.Exists (root)) {
					return kits;
				}

				foreach (string kit in Directory.GetDirectories (root)) {
					string unqualified = kit.Substring (kit.LastIndexOf (Path.DirectorySeparatorChar) + 1);
					string qualified = String.Format ("Fabric.Internal.{0}.Editor.Controller.Controller", unqualified);

					Type type = Type.GetType (qualified);

					// Those kits without a controller will not be displayed in the KitSelector
					// as they do not have a GUI component to them.
					if (type == null) {
						continue;
					}
					object instance = Activator.CreateInstance (type, api);

					kits.Add (new ImportedKit (unqualified, instance));
				}

				return kits;
			};
		}

		public static KitStatus StatusFor(ImportedKit importedKit)
		{
			Settings.InstalledKit installedKit = Settings.Instance.InstalledKits.Find (kit => { return kit.Name == importedKit.Name; });

			if (installedKit == null) {
				return KitStatus.Imported;
			}

			Settings.KitInstallationStatus installationStatus = installedKit.InstallationStatus;

			switch (installationStatus) {
			case Settings.KitInstallationStatus.Configured:
				return KitStatus.Configured;
			case Settings.KitInstallationStatus.Installed:
				return KitStatus.Installed;
			case Settings.KitInstallationStatus.Imported:
			default:
				return KitStatus.Imported;
			}
		}

		public static bool IsUpToDate(KitsObject availableKit, ImportedKit importedKit)
		{
			System.Version latestKitVersion = new System.Version (availableKit.Version);
			System.Version currentKitVersion = new System.Version (); // Default is 0.0

			if (importedKit != null) {
				currentKitVersion = importedKit.Instance.Version ();
			}

			return latestKitVersion <= currentKitVersion;
		}

		public static string AnalyticsStateString()
		{
			List<Settings.InstalledKit> kits = Settings.Instance.InstalledKits;

			// If a single kit is in the "Configured" state, the overall stat is configured.
			if (kits.Exists (k => k.InstallationStatus == Settings.KitInstallationStatus.Configured)) {
				return "Configured";
			}

			// Otherwise, the kits are all installed, or available, or imported, but none are configured.
			return "Normal";
		}
	}
}
