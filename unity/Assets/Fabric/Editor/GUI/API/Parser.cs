namespace Fabric.Internal.Editor.API
{
	using UnityEditor;
	using System.Collections.Generic; 
	using System.Collections;
	using System.IO;
	using Fabric.Internal.Editor.Model;
	using Fabric.Internal.ThirdParty.MiniJSON;

	public class Parser
	{
		public static List<Organization> ParseOrganizations(Stream stream)
		{
			List<Organization> organizations = new List<Organization> ();
			
			using (StreamReader reader = new StreamReader (stream)) {
				string json = reader.ReadToEnd ();
				List<object> response = Json.Deserialize (json) as List<object>;

				foreach (var obj in response) {
					Dictionary<string, object> orgData = (Dictionary<string, object>) obj;
					string id = orgData["id"] as string;
					string name = orgData["name"] as string;
					string apiKey = orgData["api_key"] as string;
					string buildSecret = orgData["build_secret"] as string;

					organizations.Add (new Organization (name, id, apiKey, buildSecret));
				}
			}
			
			return organizations;
		}

		public static List<App> ParseApps(Stream stream, BuildTarget platform)
		{
			List<App> apps = new List<App> ();

			using (StreamReader reader = new StreamReader (stream)) {
				string json = reader.ReadToEnd ();
				List<object> response = Json.Deserialize (json) as List<object>;

				foreach (var obj in response) {
					Dictionary<string, object> appData = (Dictionary<string, object>) obj;
					string name = appData["name"] as string;
					string bundleId = appData["bundle_identifier"] as string;
					string iconUrl = appData["icon_url"] as string;
					string dashboardUrl = appData["dashboard_url"] as string;
					Dictionary<string, object> productList = appData["map_of_available_products"] as Dictionary<string, object>;

					if (productList != null) {
						apps.Add (new App (name, bundleId, iconUrl, platform, dashboardUrl, ParseProducts (productList)));
					}
				}
			}

			return apps;
		}

		private static List<App.Kit> ParseProducts(Dictionary<string, object> productList)
		{
			List<App.Kit> kits = new List<App.Kit> ();

			foreach (KeyValuePair<string, object> product in productList) {
				string name = product.Key;
				bool present = (bool) product.Value;

				if (present) {
					kits.Add (new App.Kit { Name = name });
				}
			}

			return kits;
		}
	}
}
