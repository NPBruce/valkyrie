namespace Fabric.Internal.Editor.Controller
{
	using UnityEditor;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Threading;
	using Fabric.Internal.Editor.Model;
	using Fabric.Internal.Editor.Detail;

	internal class KitInstallationChecker
	{
		public void CheckInstalledKits(string organization, string bundleId, BuildTarget buildTarget, Action<App> onSuccess, Action<Exception> onFailure, Action<Exception> onNoNetwork)
		{
			// Ignore lack of network.
			API.AsyncV1.Fetch<App> (onSuccess, onFailure, onNoNetwork, (API.V1 api) => {
				return KitsForApp (api, organization, bundleId, buildTarget);
			});
		}

		private static App KitsForApp(API.V1 api, string organization, string bundleId, BuildTarget buildTarget)
		{
			List<Organization> organizations = api.Organizations ();
			List<App> appsForOrg = api.ApplicationsFor (organization, organizations);

			App activatedApp = appsForOrg.Find (app => {
				bool matchesBundleId = app.BundleIdentifier.Equals (bundleId, StringComparison.OrdinalIgnoreCase);
				bool matchesPlatform = app.Platform == buildTarget;

				return matchesBundleId && matchesPlatform;
			});

			return activatedApp;
		}
	}
}
