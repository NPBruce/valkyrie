namespace Fabric.Internal.Editor.Model
{
	using UnityEditor;
	using UnityEngine;
	using System.Collections.Generic;
	
	public class App
	{
		public string Name;
		public string BundleIdentifier;
		public string IconUrl;
		public BuildTarget Platform;
		public string DashboardUrl;
		public List<Kit> SdkKits;

		public class Kit
		{
			public string Name;
		}

		public App(string name, string bundleIdentifier, string iconUrl, BuildTarget platform, string dashboardUrl, List<Kit> sdkKits)
		{
			Name = name;
			BundleIdentifier = bundleIdentifier;
			IconUrl = iconUrl;
			Platform = platform;
			DashboardUrl = dashboardUrl;
			SdkKits = sdkKits;
		}
	}
}