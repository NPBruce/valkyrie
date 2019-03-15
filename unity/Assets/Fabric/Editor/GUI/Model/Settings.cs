namespace Fabric.Internal.Editor.Model
{
	using UnityEngine;
	using UnityEditor;
	using System;
	using System.IO;
	using System.Collections.Generic;
	using Fabric.Internal.Editor.Net.OAuth;
	
	public class Settings : ScriptableObject
	{
		private static readonly string SettingsAssetName = "FabricSettings";
		private static readonly string SettingsPath = "Editor Default Resources";
		private static readonly string SettingsAssetExtension = "asset";

		// This gets around comparing 'instance' with 'null' on the main thread specifically for Unity 4.
		// The underlying comparison implementation has more constraints related to threads than later
		// versions of Unity.
		private static bool IsInstantiated()
		{
#if UNITY_5_3_OR_NEWER
			return instance != null;
#else
			return !object.ReferenceEquals (instance, null);
#endif
		}

		#region Instance
		private static Settings instance;
		public static Settings Instance
		{
			get {
				if (!IsInstantiated ()) {
					string assetNameWithExtension = string.Join (".", new string[] {
						SettingsAssetName,
						SettingsAssetExtension
					});

					if ((instance = EditorGUIUtility.Load (assetNameWithExtension) as Settings) == null) {
						instance = CreateInstance<Settings> ();

						if (!Directory.Exists (Path.Combine (Application.dataPath, SettingsPath))) {
							AssetDatabase.CreateFolder ("Assets", SettingsPath);
						}

						AssetDatabase.CreateAsset (instance, Path.Combine (Path.Combine ("Assets", SettingsPath), assetNameWithExtension));
					}
				}

				return instance;
			}
		}
		#endregion

		#region Email
		[SerializeField]
		private string email;

		[HideInInspector]
		public string Email
		{
			get { return Instance.email; }
			set {
				Instance.email = value;
				EditorUtility.SetDirty (Instance);
			}
		}
		#endregion

		#region OAuth Token
		[SerializeField]
		private string rawToken;

		[HideInInspector]
		private Client.Token token;

		[HideInInspector]
		public Client.Token Token
		{
			get {
				if (Instance.token == null)
					Instance.token = Client.parse (rawToken);
				return Instance.token;
			}
			set {
				Instance.token = value;
				Instance.rawToken = value == null ? null : value.ToString ();
				EditorUtility.SetDirty (Instance);
			}
		}
		#endregion

		#region Organization
		[SerializeField]
		private Organization organization;

		[HideInInspector]
		public Organization Organization
		{
			get { return Instance.organization; }
			set {
				Instance.organization = value;
				EditorUtility.SetDirty (Instance);
			}
		}
		#endregion

		#region App Icon
		[SerializeField]
		private string iconUrl;

		[HideInInspector]
		public string IconUrl
		{
			get { return Instance.iconUrl; }
			set {
				Instance.iconUrl = value;
				EditorUtility.SetDirty (Instance);
			}
		}
		#endregion

		#region Dashboard Url
		[SerializeField]
		private string dashboardUrl;

		[HideInInspector]
		public string DashboardUrl
		{
			get { return Instance.dashboardUrl; }
			set {
				Instance.dashboardUrl = value;
				EditorUtility.SetDirty (Instance);
			}
		}
		#endregion

		#region Initialization
		public enum InitializationType { Automatic, Manual }

		[SerializeField]
		private InitializationType initialization;

		[HideInInspector]
		public InitializationType Initialization
		{
			get { return Instance.initialization; }
			set {
				Instance.initialization = value;
				EditorUtility.SetDirty (Instance);
			}
		}
		#endregion

		#region Sequence
		[SerializeField]
		private int flowSequence = 0;

		[HideInInspector]
		public int FlowSequence
		{
			get { return Instance.flowSequence; }
			set {
				Instance.flowSequence = value;
				EditorUtility.SetDirty(Instance);
			}
		}
		#endregion

		#region Conflict
		[SerializeField]
		private string conflict;

		[HideInInspector]
		public string Conflict
		{
			get { return Instance.conflict; }
			set {
				Instance.conflict = value;
				EditorUtility.SetDirty (Instance);
			}
		}

		#endregion

		#region Kit
		[SerializeField]
		private string kit;
		
		[HideInInspector]
		public string Kit
		{
			get { return Instance.kit; }
			set {
				Instance.kit = value;
				EditorUtility.SetDirty (Instance);
			}
		}
		#endregion
		
		#region Installed
		[Serializable]
		public enum KitInstallationStatus {
			Imported,    // Downloaded into the project, but not yet configured.
			Configured,  // Configured locally, but not activated via device launch.
			Installed    // Fully configured and activated.
		}

		[Serializable]
		public class InstalledKit
		{
			[SerializeField]
			public string Name;
			[SerializeField]
			public KitInstallationStatus InstallationStatus;
			[SerializeField]
			public bool Installed;
			[SerializeField]
			public bool Enabled;

			[Serializable]
			public class MetaTuple
			{
				[SerializeField]
				public string Key;

				[SerializeField]
				public string Value;

				[SerializeField]
				public string Platform;
			}

			[SerializeField]
			public List<MetaTuple> Meta = new List<MetaTuple> ();
		}

		[SerializeField]
		private List<InstalledKit> installedKits = new List<InstalledKit> ();
		
		[HideInInspector]
		public List<InstalledKit> InstalledKits
		{
			get { return Instance.installedKits; }
			set {
				Instance.installedKits = value;
				EditorUtility.SetDirty (Instance);
			}
		}
		#endregion
	}
}